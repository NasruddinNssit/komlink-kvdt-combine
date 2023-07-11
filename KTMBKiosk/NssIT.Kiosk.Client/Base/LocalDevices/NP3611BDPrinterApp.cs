using NssIT.Kiosk.Device.Nippon.NP3611BD.AccessSDK;
using NssIT.Kiosk.Device.Nippon.NP3611BD.AccessSDK.Command;
using NssIT.Kiosk.Device.Nippon.NP3611BD.AccessSDK.Command.Working;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.Base.LocalDevices
{
    public class NP3611BDPrinterApp : IDisposable 
    {
        private const string _logChannel = "PRINTER_NP3611BD_APP";

        private NP3611BDPrinterAccess _printerAccess = null;
        private string _initLog = "";

        private static NP3611BDPrinterApp _printerApp = null;
        private static SemaphoreSlim _appLock = new SemaphoreSlim(1);

        private string _initPrinterName = "";
        private bool _checkPrinterPaperLowState = true;

        public static NP3611BDPrinterApp GetApp(string printerName, bool checkPrinterPaperLowState)
        {
            if (_printerApp != null)
                return _printerApp;

            try
            {
                _appLock.WaitAsync().Wait();

                if (_printerApp != null)
                    return _printerApp;

                _printerApp = new NP3611BDPrinterApp(printerName, checkPrinterPaperLowState);

                if (_printerApp == null)
                    throw new Exception("Fail to create printer app");
                else
                    return _printerApp;
            }
            catch (Exception ex)
            {
                Exception nEx = new Exception("Error: Unable to create printer app.", ex);
                App.Log.LogError(_logChannel, "*", nEx, "EX01", "NP3611BDPrinterApp.GetApp");
                throw nEx;
            }
            finally
            {
                if (_appLock.CurrentCount == 0)
                    _appLock.Release();
            }
        }

        private NP3611BDPrinterApp(string initPrinterName, bool checkPrinterPaperLowState)
        {
            _initPrinterName = initPrinterName;
            _checkPrinterPaperLowState = checkPrinterPaperLowState;

            App.Log.LogText(_logChannel, "*", $@"Parameter_Printer_Name : {initPrinterName ?? ""} ; Check_Printer_Paper_Low_State : {checkPrinterPaperLowState}", "A01", "NP3611BDPrinterApp.<Constructor>", AppDecorator.Log.MessageType.Info);
        }

        private (bool? LastIsPrinterErrorState, bool? LastIsPrinterWarningState, DateTime? LastLogTime) _printerStatusLogHistory
            = (LastIsPrinterErrorState: null, LastIsPrinterWarningState: null, LastLogTime: null);

        public void GetPrinterStatus(out bool isPrinterError, out bool isPrinterWarning, out string statusDescription, out string localStatusDescription)
        {
            isPrinterError = false;
            isPrinterWarning = false;
            statusDescription = "";
            localStatusDescription = "";

            ReadPrinterStatusEcho echo = null;

            if (AppDecorator.Config.Setting.GetSetting().DisablePrinterTracking)
            {
                echo = new ReadPrinterStatusEcho(false, false, "Printer Tracking Disabled", "-Printer Tracking Disabled-");
            }
            else
            {
                ReadPrinterStatusAx<NullAccessParameter, ReadPrinterStatusEcho> command
                = new ReadPrinterStatusAx<NullAccessParameter, ReadPrinterStatusEcho>();

                ReadPrinterStatusAx<NullAccessParameter, ReadPrinterStatusEcho>
                    execResult
                    = (ReadPrinterStatusAx<NullAccessParameter, ReadPrinterStatusEcho>)
                    PrinterAccess.ExecCommand(command, waitDelaySec: 20);

                echo = execResult.SuccessEcho;
            }
            

            isPrinterError = echo.IsPrinterError;
            isPrinterWarning = echo.IsPrinterWarning;
            statusDescription = echo.StatusDescription;
            localStatusDescription = echo.LocalStatusDescription;

            int statusLogRepeatDelayMinutes = 15;
            string msg = $@"Is Printer Error : {isPrinterError} ; Is Printer Warning : {isPrinterWarning} ; StatusDescription : {statusDescription} ; LocalStateDesc : {localStatusDescription}";
            App.ShowDebugMsg(msg);

            if ((_printerStatusLogHistory.LastLogTime.HasValue == false)
                ||
                (_printerStatusLogHistory.LastIsPrinterErrorState.HasValue == false)
                ||
                (_printerStatusLogHistory.LastIsPrinterWarningState.HasValue == false)
                ||
                (_printerStatusLogHistory.LastIsPrinterErrorState.Value != isPrinterError)
                ||
                (_printerStatusLogHistory.LastIsPrinterWarningState.Value != isPrinterWarning)
                ||
                (_printerStatusLogHistory.LastLogTime.Value.AddMinutes(statusLogRepeatDelayMinutes).Ticks < DateTime.Now.Ticks)
                )
            {
                App.Log.LogText(_logChannel, "*", msg, "A05", "NP3611BDPrinterApp.GetPrinterStatus", AppDecorator.Log.MessageType.Info);

                _printerStatusLogHistory.LastLogTime = DateTime.Now;
                _printerStatusLogHistory.LastIsPrinterErrorState = isPrinterError;
                _printerStatusLogHistory.LastIsPrinterWarningState = isPrinterWarning;
            }
        }

        public void Dispose()
        {
            try
            {
                _printerAccess?.Dispose();
            }
            catch { }
        }

        private NP3611BDPrinterAccess PrinterAccess
        {
            get
            {
                if (_printerAccess != null)
                    return _printerAccess;

                _printerAccess = NP3611BDPrinterAccess.GetPrinterAccess(_initPrinterName, _checkPrinterPaperLowState);

                DateTime endTime = DateTime.Now.AddSeconds(30);
                while (endTime.Ticks > DateTime.Now.Ticks)
                {
                    if (_printerAccess.IsApiCreatedSuccessfully.HasValue == false)
                    {
                        Thread.Sleep(1000);
                    }
                    else
                        break;
                }

                if ((_printerAccess?.IsApiCreatedSuccessfully.HasValue == true) && _printerAccess.IsApiCreatedSuccessfully.Value == false)
                {
                    Exception ex2 = _printerAccess.ApiError;

                    if (ex2 is null)
                        ex2 = new Exception("Unknown printer access creation error");

                    _printerAccess = null;

                    throw ex2;
                }
                else if (_printerAccess?.IsApiCreatedSuccessfully.HasValue == false)
                {
                    Exception ex2 = new Exception("Timeout; Unable to create printer API on application initialization");

                    _printerAccess = null;

                    throw ex2;
                }
                else if (_printerAccess is null)
                {
                    Exception ex2 = new Exception("Timeout; Unable to create printer access");

                    throw ex2;
                }

                App.SysParam.SetPrinterName(_printerAccess.FinalizedPrinterName);
                return _printerAccess;
            }
        }
    }
}
