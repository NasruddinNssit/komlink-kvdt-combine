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
    public class CardSettlementExecution : IAccessCommandExec, IDisposable
    {
        private const string LogChannel = "ServerAccess";
        private AccessCommandPack _commandPack = null;
        private ServerAccess _serverAccess = null;

        private CardSettlementCommand _command = null;
        private string _webApiUrl = @"Kiosk/saveCardSettlementInfo";
        private DbLog _log = null;
        private DbLog Log { get => (_log ?? (_log = DbLog.GetDbLog())); }

        public AccessCommandPack Execute(ServerAccess serverAccess, AccessCommandPack commPack)
        {
            string methodTag = "CardSettlementExecution.Execute";
            string domainEntityTag = "Save Card Settlement Info";

            _serverAccess = serverAccess;
            _commandPack = commPack;
            bool eventSent = false;
            UISalesCardSettlementStatusAck uiSettmStt;

            try
            {
                // KTM Save Settlement Info XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                AppDecorator.Config.Setting setting = NssIT.Kiosk.AppDecorator.Config.Setting.GetSetting();

                _command = (CardSettlementCommand)commPack.Command;

                IPostRequestParam param = new CardSettlementRequest()
                {
                    MachineId = setting.KioskId, 
                    BatchCount = _command.BatchCount, 
                    BatchCurrencyAmount = _command.BatchCurrencyAmount, 
                    BatchNumber = _command.BatchNumber, 
                    ErrorMessage = _command.ErrorMessage, 
                    HostNo = _command.HostNo, 
                    StatusCode = _command.StatusCode
                };

                dynamic apiRes = _serverAccess.WebAPI.Post<CardSettlementResult>(param, _webApiUrl, $@"{methodTag}").GetAwaiter().GetResult();

                bool validDataFound = false;

                if ((apiRes is CardSettlementResult settResX) && (settResX.Code.Equals(WebAPIAgent.ApiCodeOK)))
                {
                    CardSettlementResult settRes = settResX;

                    //if ((tick?.Data.Host.Length > 0))
                    //{
                    uiSettmStt = new UISalesCardSettlementStatusAck(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, settRes);
                    commPack.UpSertResult(false, uiSettmStt);
                    whenCompletedSendEvent(ResultStatus.Success, _commandPack.NetProcessId, _commandPack.ProcessId, $"{methodTag}:A02", uiSettmStt);
                    validDataFound = true;
                    //}
                    //else
                    //{
                    //    apiRes = new WebApiException(new Exception($@"No valid data found; (EXIT21821); Error Code : {tick?.Code}"));

                    //    Log.LogText(LogChannel, "-", tickX, "A03", methodTag, AppDecorator.Log.MessageType.Error,
                    //        netProcessId: commPack.NetProcessId, extraMsg: $@"No valid data found; (EXIT21821); Error Code : {tick?.Code}; MsgObj: CardSettlementResult");
                    //}
                }

                if (validDataFound == false)
                {
                    string errorMsg = "";
                    string errCode = "99";

                    if (apiRes is CardSettlementResult errResult)
                    {
                        errCode = "99";
                        errorMsg = $@"No valid data ({domainEntityTag}) found; Code: {errCode}; (EXIT21822)";

                        Log.LogText(LogChannel, "-", errResult, "A05", methodTag, AppDecorator.Log.MessageType.Error,
                            netProcessId: commPack.NetProcessId, extraMsg: $@"{errorMsg}; MsgObj: CardSettlementResult");
                    }
                    else if (apiRes is WebApiException wex)
                    {
                        errCode = wex.Code;
                        errorMsg = $@"{wex.MessageString() ?? "Web process error"}; ({domainEntityTag});Code: {errCode}; (EXIT21823)";

                        Log.LogText(LogChannel, "-", errorMsg, "A08", methodTag,
                            AppDecorator.Log.MessageType.Error, netProcessId: commPack.NetProcessId);
                    }
                    else
                    {
                        errCode = "99";
                        errorMsg = $@"Unexpected error occurred; ({domainEntityTag}); Code: {errCode}; (EXIT21824)";

                        Log.LogText(LogChannel, "-", errorMsg, "A09", methodTag, AppDecorator.Log.MessageType.Error, netProcessId: commPack.NetProcessId);
                    }

                    List<string> msgs = new List<string>(new string[] { errorMsg });

                    CardSettlementResult errData = new CardSettlementResult() { Code = errCode, Data = null, Messages = msgs, Status = false };

                    uiSettmStt = new UISalesCardSettlementStatusAck(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, errData)
                    { ErrorMessage = (string.IsNullOrWhiteSpace(errorMsg) == true ? "Unknown error when reading data (EXIT21825)" : errorMsg) };

                    commPack.UpSertResult(true, uiSettmStt, uiSettmStt.ErrorMessage);

                    whenCompletedSendEvent(ResultStatus.Fail, _commandPack.NetProcessId, _commandPack.ProcessId, $@"{methodTag}:A10", uiSettmStt, new Exception(uiSettmStt.ErrorMessage));
                }
            }
            catch (Exception ex)
            {
                List<string> msgs = new List<string>(new string[] { $@"{ex.Message}; (EXIT21826)" });
                CardSettlementResult errData = new CardSettlementResult() { Code = "99", Data = null, Messages = msgs, Status = false };

                uiSettmStt = new UISalesCardSettlementStatusAck(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, errData) { ErrorMessage = errData.MessageString() };
                commPack.UpSertResult(true, uiSettmStt, errorMessage: uiSettmStt.ErrorMessage);
                whenCompletedSendEvent(ResultStatus.ErrorFound, _commandPack.NetProcessId, _commandPack.ProcessId, $@"{methodTag}.Execute:A11", uiSettmStt, new Exception(uiSettmStt.ErrorMessage));
            }

            return commPack;
            //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
            void whenCompletedSendEvent(ResultStatus resultState, Guid? netProcessId, string processId, string lineTag,
                IKioskMsg uiKioskMsg, Exception error = null)
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
            _command = null;
        }
    }
}
