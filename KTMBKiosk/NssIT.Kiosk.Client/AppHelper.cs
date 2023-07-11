using NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI;
using NssIT.Kiosk.Client.Base.LocalDevices;
using NssIT.Kiosk.Log.DB.StatusMonitor;
using NssIT.Train.Kiosk.Common.Data.Response;
using System;

namespace NssIT.Kiosk.Client
{
    /// <summary>
    /// Application Helper
    /// </summary>
    public class AppHelper : IDisposable
    {
        private string _logChannel = "AppSys";

        public const string PrinterErrorTag = "[PRT-ERR]";

        public CounterConfigCompiledResult CounterConfiguration { get; private set; }
        public string MachineID { get; private set; }
        public bool IsETSIntercitySalesValid { get; private set; } = true;
        public bool IsKomuterSalesValid { get; private set; } = true;
        public bool IsKVDTSalesValid { get; private set; } = true;
        public bool IsKomlinkSalesValid { get; private set; } = true;

        private NP3611BDPrinterApp _printerApp = null;

        private string _svrAppVersion = null;
        public string ServerApplicationVersion
        {
            get
            {
                if (_svrAppVersion is null)
                    return "#*#";
                else
                    return _svrAppVersion;
            }
            private set
            {
                if ((value?.Length > 0) && (_svrAppVersion == null))
                {
                    if (value.Equals("XXERRXX", StringComparison.InvariantCultureIgnoreCase) == false)
                    {
                        _svrAppVersion = value;
                    }
                }
            }
        }

        //private SemaphoreSlim _sysHealthLock = new SemaphoreSlim(1);
        /// <summary>
        /// Return true when Valid Logon.
        /// </summary>
        /// <returns></returns>
        public bool SystemHealthCheck()
        {
            try
            {
                //_sysHealthLock.WaitAsync().Wait();

                return HealthCheckWorking();
            }
            finally
            {
                //if (_sysHealthLock.CurrentCount == 0)
                //_sysHealthLock.Release();
            }

            return false;
            /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            bool HealthCheckWorking()
            {
                bool isServerResponded = false;
                //bool serverWebServiceIsDetected = false;
                bool serverAppHasDisposed = true;
                bool serverAppHasShutdown = true;
                UIServerApplicationStatusAck serverStatusAck = null;

                if (App.NetClientSvc.SalesService is null)
                    throw new Exception("Client Sales Services is no ready (ZX01)");

                App.NetClientSvc.SalesService.QuerySalesServerStatus(out isServerResponded, out serverAppHasDisposed,
                        out serverAppHasShutdown, out serverStatusAck, waitDelaySec: 60);

                if ((isServerResponded == true) && (serverAppHasDisposed == false) && (serverAppHasShutdown == false)
                    && (serverStatusAck?.IsResultSuccess == true) && (serverStatusAck.MessageData is CounterConfigCompiledResult configRes))
                {
                    NssIT.Kiosk.AppDecorator.Config.Setting sysSetting = NssIT.Kiosk.AppDecorator.Config.Setting.GetSetting();

                    MachineID = serverStatusAck.MachineId;
                    CounterConfiguration = configRes;
                    ServerApplicationVersion = serverStatusAck.ApplicationVersion;
                    sysSetting.WebApiURL = serverStatusAck.WebApiURL;

                    if ((configRes.Data.StationMachineSetting.KomuterCheck?.Equals("1") == true) || (configRes.Data.StationMachineSetting.OthersCheck?.Equals("1") == true))
                    {
                        if (configRes.Data.StationMachineSetting.OthersCheck?.Equals("1") == true)
                            IsETSIntercitySalesValid = true;
                        else
                            IsETSIntercitySalesValid = false;

                        if (configRes.Data.StationMachineSetting.KomuterCheck?.Equals("1") == true)
                            IsKomuterSalesValid = true;
                        else
                            IsKomuterSalesValid = false;
                    }

                    bool isPrinterError = false;
                    bool isPrinterWarning = false;
                    string statusDescription = null;
                    string locStateDesc = null;

                    try
                    {
                        PrinterApp.GetPrinterStatus(out isPrinterError, out isPrinterWarning, out statusDescription, out locStateDesc);
                    }
                    catch (Exception ex)
                    {
                        App.Log.LogError(_logChannel, "-", ex, "EX02", "AppHelper.SystemHealthCheck");
                        throw new Exception($@"{ex.Message}; (EXIT10000019); {PrinterErrorTag}");
                    }

                    if (isPrinterError || isPrinterWarning)
                    {
                        StatusHub.GetStatusHub().zNewStatus_IsPrinterStandBy(AppDecorator.DomainLibs.KioskStatus.KioskCommonStatus.No, statusDescription);

                        if (string.IsNullOrWhiteSpace(statusDescription))
                            statusDescription = "Printer Error (X01); (EXIT10000017)";

                        App.Log.LogText(_logChannel, "-", $@"Error; (EXIT10000018); {statusDescription}", "A11A", "AppHelper.SystemHealthCheck", AppDecorator.Log.MessageType.Error);
                        throw new Exception($@"{statusDescription}; (EXIT10000018); {PrinterErrorTag}");
                    }
                    else
                    {
                        StatusHub.GetStatusHub().zNewStatus_IsPrinterStandBy(AppDecorator.DomainLibs.KioskStatus.KioskCommonStatus.Yes, "Printer Standing By");
                    }

                    if (AppDecorator.Config.Setting.GetSetting().DisablePrinterTracking == false)
                    {
                        if (string.IsNullOrWhiteSpace(App.SysParam.FinalizedPrinterName) == true)
                        {
                            App.Log.LogText(_logChannel, "-", $@"Error; (EXIT10000019); Default (or correct) printer not found", "A11B", "AppHelper.SystemHealthCheck", AppDecorator.Log.MessageType.Error);
                            throw new Exception($@"{statusDescription}; (EXIT10000019); Default (or correct) printer not found; {PrinterErrorTag}");
                        }
                    }

                    ////if (RDLCLibraryStarter.LibraryIsReady == false)
                    ////{
                    ////    string pMsg = "RDLC report engine initiation in progress; Please wait a moment";

                    ////    App.Log.LogText(_logChannel, "-", $@"Error, (EXIT10000019B); {pMsg}; ", "A11C", "AppHelper.SystemHealthCheck", AppDecorator.Log.MessageType.Error);
                    ////    throw new Exception($@"{pMsg}; (EXIT10000019B); {PrinterErrorTag}");
                    ////}

                    return true;
                }
                else
                {
                    App.Log.LogText(_logChannel, "-", serverStatusAck, "A20", "AppHelper.SystemHealthCheck", AppDecorator.Log.MessageType.Error,
                        extraMsg: $@"Error; (EXIT10000010); isServerResponded: {isServerResponded}; serverAppHasDisposed: {serverAppHasDisposed}; serverAppHasShutdown: {serverAppHasShutdown}; MsgObj: UIServerApplicationStatusAck");

                    if (isServerResponded == false)
                        throw new Exception("Local Server not responding; (EXIT10000011)");

                    else if (serverAppHasDisposed)
                        throw new Exception("Local Server disposed; (EXIT10000012)");

                    else if (serverAppHasShutdown)
                        throw new Exception("Local Server already shutdown; (EXIT10000013)");

                    else if (serverStatusAck?.IsResultSuccess == false)
                    {
                        if (string.IsNullOrWhiteSpace(serverStatusAck.ErrorMessage) == false)
                        {
                            throw new Exception(serverStatusAck.ErrorMessage);
                        }
                        else
                        {
                            throw new Exception("Unknown Local Server Error; (EXIT10000014)");
                        }
                    }

                    else if (serverStatusAck is null)
                        throw new Exception("Unknown Local Server Error; (EXIT10000015)");

                    else
                    {
                        throw new Exception("Local Server has unknown error; (EXIT10000016)");
                    }
                }
            }

        }

        public void Dispose()
        {
            try
            {
                _printerApp?.Dispose();
            }
            catch { }
        }

        public NP3611BDPrinterApp PrinterApp
        {
            get
            {
                if (_printerApp != null)
                    return _printerApp;
                string printName = App.SysParam.PrmPrinterName;

                _printerApp = NP3611BDPrinterApp.GetApp(App.SysParam.PrmPrinterName, App.SysParam.PrmCheckPrinterPaperLow.Value);

                return _printerApp;
            }
        }
    }
}