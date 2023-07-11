using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Command.Sales;
using NssIT.Kiosk.AppDecorator.Common.AppService.Events;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI;
using NssIT.Kiosk.Log.DB;
using NssIT.Train.Kiosk.Common.Common;
using NssIT.Train.Kiosk.Common.Common.WebApi;
using NssIT.Train.Kiosk.Common.Constants;
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
    public class GetReturnSeatListExecution : IAccessCommandExec, IDisposable
    {
        private const string LogChannel = "ServerAccess";
        private AccessCommandPack _commandPack = null;
        private ServerAccess _serverAccess = null;

        private ReturnSeatListCommand _command = null;
        private string _webApiUrl = @"TrainService/getSeat";
        private DbLog _log = null;
        private DbLog Log { get => (_log ?? (_log = DbLog.GetDbLog())); }

        public AccessCommandPack Execute(ServerAccess serverAccess, AccessCommandPack commPack)
        {
            string methodTag = "GetReturnSeatListExecution.Execute";
            string domainEntityTag = "Return Seat List";

            _serverAccess = serverAccess;
            _commandPack = commPack;
            bool eventSent = false;
            UIReturnSeatListAck uiSeatList;

            try
            {
                // KTM Trip List XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                AppDecorator.Config.Setting setting = NssIT.Kiosk.AppDecorator.Config.Setting.GetSetting();

                _command = (ReturnSeatListCommand)commPack.Command;

                IPostRequestParam param = new SeatRequest()
                {
                    Id = _command.TripId,
                    MCounters_Id = setting.KioskId, 
                    Channel = setting.PurchaseChannel
                };

                dynamic apiRes = _serverAccess.WebAPI.Post<TrainSeatResult>(param, _webApiUrl, $@"{methodTag}").GetAwaiter().GetResult();

                bool validDataFound = false;

                if ((apiRes is TrainSeatResult seatX) && (seatX.Code.Equals(WebAPIAgent.ApiCodeOK)))
                {
                    TrainSeatResult seat = BlockOKUSeat(seatX);

                    if ((seat?.Data?.ErrorCode == 0))
                    {
                        //testMsg = $@"Success; Record Cound : {sttr.data?.Count().ToString() ?? "--"}";
                        uiSeatList = new UIReturnSeatListAck(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, seat);
                        commPack.UpSertResult(false, uiSeatList);
                        whenCompletedSendEvent(ResultStatus.Success, _commandPack.NetProcessId, _commandPack.ProcessId, $"{methodTag}:A02", uiSeatList);
                        validDataFound = true;
                    }
                    else
                        apiRes = new WebApiException(new Exception($@"No valid data found; (EXIT21601); Error Code : {seat?.Data?.ErrorCode}"));
                }

                if (validDataFound == false)
                {
                    string errorMsg = "";
                    string errCode = "99";

                    if (apiRes is TrainSeatResult errResult)
                    {
                        errCode = "99";
                        errorMsg = $@"No valid data ({domainEntityTag}) found; Code: {errCode}; (EXIT21602)";

                        Log.LogText(LogChannel, "-", errResult, "A05", methodTag, AppDecorator.Log.MessageType.Error,
                            netProcessId: commPack.NetProcessId, extraMsg: $@"{errorMsg}; MsgObj: TrainSeatResult");
                    }
                    else if (apiRes is WebApiException wex)
                    {
                        errCode = wex.Code;
                        errorMsg = $@"{wex.MessageString() ?? "Web process error"}; ({domainEntityTag});Code: {errCode}; (EXIT21603)";

                        Log.LogText(LogChannel, "-", errorMsg, "A08", methodTag,
                            AppDecorator.Log.MessageType.Error, netProcessId: commPack.NetProcessId);
                    }
                    else
                    {
                        errCode = "99";
                        errorMsg = $@"Unexpected error occurred; ({domainEntityTag}); Code: {errCode}; (EXIT21604)";

                        Log.LogText(LogChannel, "-", errorMsg, "A09", methodTag, AppDecorator.Log.MessageType.Error, netProcessId: commPack.NetProcessId);
                    }

                    List<string> msgs = new List<string>(new string[] { errorMsg });

                    TrainSeatResult errData = new TrainSeatResult() { Code = errCode, Data = null, Messages = msgs, Status = false };

                    uiSeatList = new UIReturnSeatListAck(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, errData)
                    { ErrorMessage = (string.IsNullOrWhiteSpace(errorMsg) == true ? "Unknown error when reading data (EXIT21605)" : errorMsg) };

                    commPack.UpSertResult(true, uiSeatList, uiSeatList.ErrorMessage);

                    whenCompletedSendEvent(ResultStatus.Fail, _commandPack.NetProcessId, _commandPack.ProcessId, $@"{methodTag}:A10", uiSeatList, new Exception(uiSeatList.ErrorMessage));
                }
            }
            catch (Exception ex)
            {
                List<string> msgs = new List<string>(new string[] { $@"{ex.Message}; (EXIT21606A)" });
                TrainSeatResult errData = new TrainSeatResult() { Code = "99", Data = null, Messages = msgs, Status = false };

                uiSeatList = new UIReturnSeatListAck(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, errData) { ErrorMessage = errData.MessageString() };
                commPack.UpSertResult(true, uiSeatList, errorMessage: uiSeatList.ErrorMessage);
                whenCompletedSendEvent(ResultStatus.ErrorFound, _commandPack.NetProcessId, _commandPack.ProcessId, $@"{methodTag}.Execute:A11", uiSeatList, new Exception(uiSeatList.ErrorMessage));
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

            TrainSeatResult BlockOKUSeat(TrainSeatResult trainSeatResult)
            {
                if (trainSeatResult?.Data?.CoachModels?.Length > 0)
                {
                    foreach (var coach in trainSeatResult.Data.CoachModels)
                    {
                        if (coach?.SeatLayoutModels?.Length > 0)
                        {
                            foreach (var seat in coach.SeatLayoutModels)
                            {
                                if (seat.IsOKU?.Equals(YesNo.Yes) == true)
                                {
                                    seat.Status = SeatStatus.Blocked;
                                }
                            }
                        }
                    }
                }
                return trainSeatResult;
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

