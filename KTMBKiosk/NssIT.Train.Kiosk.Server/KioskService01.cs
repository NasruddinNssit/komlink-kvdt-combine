using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Config;
using NssIT.Kiosk.AppDecorator.UI;
using NssIT.Kiosk.Common.AppService.Network;
//using NssIT.Kiosk.Device.B2B.B2BApp;
using NssIT.Kiosk.Common.AppService.Network.TCP;
using NssIT.Kiosk.Server.Service.Adaptor;
using System;
//using NssIT.Kiosk.Device.B2B.Server.Service.Adaptor;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Train.Kiosk.Server
{
    public partial class KioskService01 : ServiceBase
    {
        string logChannel = "KioskService";
        string logDBConnStr = $@"Data Source=C:\dev\source code\Kiosk\Code\NssIT.Kiosk.Server\LogDB\NssITKioskLog01_Test.db;Version=3";

        private NssIT.Kiosk.AppDecorator.Config.Setting _sysSetting = null;
        private NssIT.Kiosk.Log.DB.DbLog _log = null;
        private INetMediaInterface _netInterface = null;
        private CashPaymentServerSvcAdaptor _cashPaymentSvc = null;
        private SalesServerSvcAdaptor _salesSvr = null;
        private BTnGServerSvcAdaptor _bTnGSvr = null;
        private StatusMonitorSvcAdaptor _sttMonSvr = null;

        /// <summary>
        /// Version Refer to an application Version, Date, and release count of the day.
        /// Like "V1.R20200805.1" mean application Version is V1, the release Year is 2020, 5th (05) of August (08), and 1st (.1) release count of the day.
        /// Note : With "DEV-" for undeployable version. This version is not for any release purpose. Only for development process.
        /// </summary>
        private string SystemVersion = "R220801.1";

        private IUIPayment _cashPaymentApp = null;

        private NetInfoRepository _netInfoRepository = null;

        public KioskService01()
        {
            InitializeComponent();
        }

        public SysLocalParam SysParam { get; private set; }

        protected override void OnStart(string[] args)
        {
            /////CYA-TEST .. 
            System.Diagnostics.Debugger.Launch();
            //-----------------------------------------

            try
            {
                //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                // Resolve Cert Problem 
                //Trust all certificates
                System.Net.ServicePointManager.ServerCertificateValidationCallback =
                    ((sender, certificate, chain, sslPolicyErrors) => true);

                // trust sender
                System.Net.ServicePointManager.ServerCertificateValidationCallback
                                = ((sender, cert, chain, errors) => cert.Subject.Contains("YourServerName"));

                // validate cert by calling a function
                ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(ValidateRemoteCertificate);
                //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

                _log = NssIT.Kiosk.Log.DB.DbLog.GetDbLog();
                _log.LogText(logChannel, "KioskService01", "Start - KioskService01 XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX", "A01", "KioskService01.OnStart");

                RegistrySetup.GetRegistrySetting().ReadAllRegistryValues(out string registryErrorMsg);

                if (string.IsNullOrWhiteSpace(registryErrorMsg) == false)
                {
                    _log.LogText(logChannel, "KioskService01", $@"Registry Error -- {registryErrorMsg}", "X21", "KioskService01.OnStart");

                    throw new Exception(registryErrorMsg);
                }

                SysParam = new SysLocalParam();
                SysParam.ReadParameters();

                _sysSetting = NssIT.Kiosk.AppDecorator.Config.Setting.GetSetting();

                _sysSetting.ApplicationVersion = SystemVersion;

                //_sysSetting.WebServiceURL = SysParam.PrmWebServiceURL;
                _sysSetting.WebApiURL = RegistrySetup.GetRegistrySetting().WebApiUrl;
                _sysSetting.IsDebugMode = SysParam.PrmIsDebugMode;
                _sysSetting.LocalServicePort = SysParam.PrmLocalServerPort;
                _sysSetting.PayMethod = SysParam.PrmPayMethod;
                _sysSetting.KioskId = SysParam.PrmKioskId;
                _sysSetting.BTnGMinimumWaitingPeriod = SysParam.PrmBTnGMinimumWaitingPeriod;

                if (_sysSetting.IsDebugMode)
                {
                    _sysSetting.IPAddress = "10.1.1.111";
                    //_sysSetting.IPAddress = "10.238.4.15";
                }
                else
                    _sysSetting.IPAddress = NssIT.Kiosk.AppDecorator.Config.Setting.GetLocalIPAddress();

                _log.LogText(logChannel, "SystemParam", SysParam, "PARAMETER", "KioskService01.OnStart");
                _log.LogText(logChannel, "SystemSetting", _sysSetting, "SETTING", "KioskService01.OnStart");

                //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

                _sysSetting.HashSecretKey = Properties.Settings.Default["HashSecretKey"].ToString();
                _sysSetting.TVMKey = Properties.Settings.Default["TVMKey"].ToString();
                _sysSetting.TimeZoneId = Properties.Settings.Default["TimeZoneId"].ToString();
                _sysSetting.AesEncryptKey = Properties.Settings.Default["AesEncryptKey"].ToString();

                //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

                CheckRegistryConfig();

                //-----------------------------------------------------------------------------------------------------------------------------
                _netInfoRepository = new NetInfoRepository();
                _netInterface = new LocalTcpService(_sysSetting.LocalServicePort);
                //_cashPaymentApp = new B2BPaymentApplication();

                // Standard Server Service Adaptors ---------- ---------- ---------- ---------- ---------- ---------- ---------- 
                // Module : UIKioskSales
                _salesSvr = new SalesServerSvcAdaptor(_netInterface, _netInfoRepository);
                // Module : UIPayment
                //_cashPaymentSvc = new CashPaymentServerSvcAdaptor(_netInterface, _cashPaymentApp, _netInfoRepository);
                //---------- ---------- ---------- ---------- ---------- ---------- ---------- ---------- ---------- ---------- 
                // Custom Device Server Service Adaptors
                // Module : UIB2B
                //_b2bSvr = new B2BServerServiceAdaptor(_netInterface, _netInfoRepository);
                //---------- ---------- ---------- ---------- ---------- ---------- ---------- ---------- ---------- ---------- 
                // BTnGServerSvcAdaptor
                // Module : UIBTnG
                _bTnGSvr = new BTnGServerSvcAdaptor(_netInterface, _netInfoRepository);
                //---------- ---------- ---------- ---------- ---------- ---------- ---------- ---------- ---------- ---------- 
                // StatusMonitorSvcAdaptor
                // Module : UIKioskStatusMonitor
                _sttMonSvr = new StatusMonitorSvcAdaptor(_netInterface, _netInfoRepository);
                //---------- ---------- ---------- ---------- ---------- ---------- ---------- ---------- ---------- ---------- 

                _log.LogText(logChannel, "-", $@"Local Kiosk Service Loaded; Kiosk Loacl Server Version : {SystemVersion}", "A10", "KioskService01.OnStart");
            }
            catch (Exception ex)
            {
                _log.LogError(logChannel, "KioskService01", ex, "EX01", "KioskService01.OnStart");

                //Note : Allow log to be written into sqlite
                Task.Delay(3000).Wait();
                throw ex;
            }
            finally
            {
                _log.LogText(logChannel, "KioskService01", "End - KioskService01", "A100", "KioskService01.OnStart");
            }
        }

        private void CheckRegistryConfig()
        {
            Exception err = null;
            Thread testT = new Thread(new ThreadStart(new Action(() =>
            {
                try
                {
                    string rValue = RegistrySetup.GetRegistrySetting().DeviceId;

                    //_msg.ShowMessage($@"Current Device Id : {rValue}");
                }
                catch (Exception ex)
                {
                    err = ex;
                    _log.LogError(logChannel, "KioskService01", ex, "EX01", "KioskService01.CheckRegistryConfig");
                }
            })));
            testT.IsBackground = true;
            testT.Start();
            testT.Join();

            if (err != null)
                throw err;
        }

        // callback used to validate the certificate in an SSL conversation
        private static bool ValidateRemoteCertificate(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors policyErrors)
        {
            //bool result = cert.Subject.Contains("YourServerName");
            //return result;
            return true;
        }

        protected override void OnStop()
        {
            try
            {
                _sttMonSvr?.Dispose();
            }
            catch { }

            try
            {
                _bTnGSvr?.Dispose();
            }
            catch { }

            if (_netInterface != null)
            {
                try
                {
                    _netInterface.ShutdownService();
                }
                catch { }
                try
                {
                    _netInterface.Dispose();
                }
                catch { }

                _netInterface = null;
            }

            if (_netInfoRepository != null)
            {
                try
                {
                    _netInfoRepository.Dispose();
                }
                catch { }

                _netInfoRepository = null;
            }

            if (_cashPaymentSvc != null)
            {
                try
                {
                    _cashPaymentSvc.Dispose();
                }
                catch { }

                _cashPaymentSvc = null;
            }

            if (_cashPaymentApp != null)
            {
                try
                {
                    _cashPaymentApp.ShutDown();
                }
                catch { }
                try
                {
                    _cashPaymentApp.Dispose();
                }
                catch { }

                _cashPaymentApp = null;
            }

            try
            {
                NssIT.Kiosk.AppDecorator.Config.Setting.Shutdown();
            }
            catch { }

            Task.Delay(500).Wait();
        }
    }
}
