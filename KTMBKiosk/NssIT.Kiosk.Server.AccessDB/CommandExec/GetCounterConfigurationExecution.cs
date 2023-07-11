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

namespace NssIT.Kiosk.Server.AccessDB.CommandExec
{
    public class GetCounterConfigurationExecution : IAccessCommandExec, IDisposable
    {
        private const string LogChannel = "ServerAccess";

        private AccessCommandPack _commandPack = null;
        private ServerAccess _serverAccess = null;

        private DbLog _log = null;
        private DbLog Log { get => (_log ?? (_log = DbLog.GetDbLog())); }

        private string _webApiUrl = @"Config/getCounterConfig";

        public AccessCommandPack Execute(ServerAccess serverAccess, AccessCommandPack commPack)
        {
            string methodTag = "GetCounterConfigurationExecution.Execute";
            string domainEntityTag = "Get Counter Configuration";

            _serverAccess = serverAccess;
            _commandPack = commPack;
            bool eventSent = false;
            UICounterConfigurationResult uiConfig;

            try
            {
                CounterConfigurationRequestCommand configReqComm = (CounterConfigurationRequestCommand)_commandPack.Command;
                AppDecorator.Config.Setting setting = NssIT.Kiosk.AppDecorator.Config.Setting.GetSetting();

                // KTM Counter Configuration Request XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                CounterConfigRequest apiParam = new CounterConfigRequest() { CounterId = setting.KioskId };

                dynamic apiRes = _serverAccess.WebAPI.Post<CounterConfigCompiledResult>(apiParam, _webApiUrl, $@"{methodTag}").GetAwaiter().GetResult();

                bool validDataFound = false;

                if ((apiRes is CounterConfigCompiledResult configX) && (configX.Code.Equals(WebAPIAgent.ApiCodeOK)))
                {
                    if ((string.IsNullOrWhiteSpace(configX.Data.CounterInfo.StationId) == false) && (configX.Data.StationMachineSetting != null))
                    {
                        uiConfig = new UICounterConfigurationResult(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, configX, setting.KioskId, isResultSuccess: true);
                        commPack.UpSertResult(false, uiConfig);
                        whenCompletedSendEvent(ResultStatus.Success, _commandPack.NetProcessId, _commandPack.ProcessId, $"{methodTag}:A02", uiConfig);
                        validDataFound = true;
                    }
                    else
                    {
                        apiRes = new WebApiException(new Exception("No valid data found; (EXIT21730)"));
                        Log.LogText(LogChannel, "-", configX, "A03",
                            methodTag,
                            AppDecorator.Log.MessageType.Error,
                            netProcessId: commPack.NetProcessId,
                            extraMsg: "(EXIT21730); MsgObj: CounterConfigCompiledResult");
                    }
                }

                if (validDataFound == false)
                {
                    string errorMsg = "";
                    string errCode = "99";

                    if (apiRes is CounterConfigCompiledResult errResult)
                    {
                        errCode = "99";
                        errorMsg = $@"No valid data ({domainEntityTag}) found; Code: {errCode}; (EXIT21731)";

                        Log.LogText(LogChannel, "-", errResult, "A05", methodTag, AppDecorator.Log.MessageType.Error,
                            netProcessId: commPack.NetProcessId, extraMsg: $@"{errorMsg}; MsgObj: CounterConfigCompiledResult");
                    }
                    else if (apiRes is WebApiException wex)
                    {
                        errCode = wex.Code;
                        errorMsg = $@"{wex.MessageString() ?? "Web process error"}; ({domainEntityTag});Code: {errCode}; (EXIT21732)";

                        Log.LogText(LogChannel, "-", errorMsg, "A08", methodTag,
                            AppDecorator.Log.MessageType.Error, netProcessId: commPack.NetProcessId);
                    }
                    else
                    {
                        errCode = "99";
                        errorMsg = $@"Unexpected error occurred; ({domainEntityTag}); Code: {errCode};(EXIT21733)";

                        Log.LogText(LogChannel, "-", errorMsg, "A09", methodTag, AppDecorator.Log.MessageType.Error, netProcessId: commPack.NetProcessId);
                    }

                    List<string> msgs = new List<string>(new string[] { errorMsg });

                    CounterConfigCompiledResult errData = new CounterConfigCompiledResult() { Code = errCode, Data = null, Messages = msgs, Status = false };

                    uiConfig = new UICounterConfigurationResult(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, errData, setting.KioskId, isResultSuccess: false)
                    { ErrorMessage = (string.IsNullOrWhiteSpace(errorMsg) == true ? "Unknown error when reading data (EXIT21734)" : errorMsg) };

                    commPack.UpSertResult(true, uiConfig, uiConfig.ErrorMessage);

                    whenCompletedSendEvent(ResultStatus.Fail, _commandPack.NetProcessId, _commandPack.ProcessId, $@"{methodTag}:A10", uiConfig, new Exception(uiConfig.ErrorMessage));
                }
            }
            catch (Exception ex)
            {
                uiConfig = new UICounterConfigurationResult(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, null, "**", isResultSuccess: false) { ErrorMessage = $@"{ex.Message}; (EXIT21735)" };
                commPack.UpSertResult(true, uiConfig, errorMessage: uiConfig.ErrorMessage);
                whenCompletedSendEvent(ResultStatus.ErrorFound, _commandPack.NetProcessId, _commandPack.ProcessId, $@"{methodTag}:A15", uiConfig, new Exception(uiConfig.ErrorMessage));
            }

            return commPack;
            //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
            void whenCompletedSendEvent(ResultStatus resultState, Guid? netProcessId, string processId, string lineTag, IKioskMsg uiKioskMsg,
                Exception error = null)
            {
                if (eventSent)
                    return;
                _serverAccess.RaiseOnSendMessage(resultState, netProcessId, processId, lineTag, uiKioskMsg, error);
                eventSent = true;
            }
        }

        public void Dispose()
        {
            _commandPack = null;
            _log = null;
            _serverAccess = null;
        }
    }
}

