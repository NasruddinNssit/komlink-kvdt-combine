using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Command.Sales;
using NssIT.Kiosk.AppDecorator.Common.AppService.Events;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI;
using NssIT.Kiosk.Log.DB;
using NssIT.Train.Kiosk.Common.Common;
using NssIT.Train.Kiosk.Common.Common.WebApi;
using NssIT.Train.Kiosk.Common.Data;
using NssIT.Train.Kiosk.Common.Data.PostRequestParam;
using NssIT.Train.Kiosk.Common.Data.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Server.AccessDB
{
    public class BasicWebAPI: IDisposable
    {
        private const string LogChannel = "ServerAccess";

        private ServerAccess _serverAccess = null;

        private DbLog _log = null;
        private DbLog Log { get => (_log ?? (_log = DbLog.GetDbLog())); }

        private Guid _dummyNetProcessId = Guid.NewGuid();

        private string _webApiUrl = @"Config/getCounterConfig";

        public UICounterConfigurationResult Execute(ServerAccess serverAccess)
        {
            string methodTag = "BasicWebAPI.Execute";
            string domainEntityTag = "Machine Info(Data) Reading";

            _serverAccess = serverAccess;
            UICounterConfigurationResult uiConfig = null;

            try
            {
                AppDecorator.Config.Setting setting = NssIT.Kiosk.AppDecorator.Config.Setting.GetSetting();

                // KTM Counter Configuration Request XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                CounterConfigRequest apiParam = new CounterConfigRequest() { CounterId = setting.KioskId };

                dynamic apiRes = _serverAccess.WebAPI.Post<CounterConfigCompiledResult>(apiParam, _webApiUrl, $@"{methodTag}").GetAwaiter().GetResult();

                bool validDataFound = false;

                if ((apiRes is CounterConfigCompiledResult configX) && (configX.Code.Equals(WebAPIAgent.ApiCodeOK)))
                {
                    if (string.IsNullOrWhiteSpace(configX.Data.CounterInfo.StationId) == false)
                    {
                        uiConfig = new UICounterConfigurationResult(_dummyNetProcessId, "**", DateTime.Now, configX, setting.KioskId, isResultSuccess: true);
                        validDataFound = true;
                    }
                    else
                    {
                        apiRes = new WebApiException(new Exception("No valid data found; (EXIT21790)"));
                        Log.LogText(LogChannel, "-", configX, "A03",
                            methodTag,
                            AppDecorator.Log.MessageType.Error,
                            netProcessId: _dummyNetProcessId,
                            extraMsg: "(EXIT21790); MsgObj: CounterConfigCompiledResult");
                    }
                }

                if (validDataFound == false)
                {
                    string errorMsg = "";
                    string errCode = "99";

                    if (apiRes is CounterConfigCompiledResult errResult)
                    {
                        errCode = "99";
                        errorMsg = $@"No valid data ({domainEntityTag}) found; Code: {errCode}; (EXIT21791)";

                        Log.LogText(LogChannel, "-", errResult, "A05", methodTag, AppDecorator.Log.MessageType.Error,
                            netProcessId: _dummyNetProcessId, extraMsg: $@"{errorMsg}; MsgObj: CounterConfigCompiledResult");
                    }
                    else if (apiRes is WebApiException wex)
                    {
                        errCode = wex.Code;
                        errorMsg = $@"{wex.MessageString() ?? "Web process error"}; ({domainEntityTag});Code: {errCode}; (EXIT21792)";

                        Log.LogText(LogChannel, "-", errorMsg, "A08", methodTag,
                            AppDecorator.Log.MessageType.Error, netProcessId: _dummyNetProcessId);
                    }
                    else
                    {
                        errCode = "99";
                        errorMsg = $@"Unexpected error occurred; ({domainEntityTag}); Code: {errCode};(EXIT21793)";

                        Log.LogText(LogChannel, "-", errorMsg, "A09", methodTag, AppDecorator.Log.MessageType.Error, netProcessId: _dummyNetProcessId);
                    }

                    List<string> msgs = new List<string>(new string[] { errorMsg });

                    CounterConfigCompiledResult errData = new CounterConfigCompiledResult() { Code = errCode, Data = null, Messages = msgs, Status = false };

                    uiConfig = new UICounterConfigurationResult(_dummyNetProcessId, "**", DateTime.Now, errData, setting.KioskId, isResultSuccess: false)
                    { ErrorMessage = (string.IsNullOrWhiteSpace(errorMsg) == true ? "Unknown error when reading data (EXIT21794)" : errorMsg) };
                }
            }
            catch (Exception ex)
            {
                uiConfig = new UICounterConfigurationResult(_dummyNetProcessId, "**", DateTime.Now, null, "**", isResultSuccess: false) { ErrorMessage = $@"{ex.Message}; (EXIT21795)" };
            }

            return uiConfig;
            //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
        }

        public void Dispose()
        {
            _log = null;
            _serverAccess = null;
        }
    }
}
