using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Command.Sales;
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
using System.Threading.Tasks;

namespace NssIT.Kiosk.Server.AccessDB.CommandExec
{
    public class GetReturnTripExecution : IAccessCommandExec, IDisposable
    {
        private const string LogChannel = "ServerAccess";
        private AccessCommandPack _commandPack = null;
        private ServerAccess _serverAccess = null;

        private ReturnTripListCommand _command = null;
        private string _webApiUrl = @"Trip/getTrip";
        private DbLog _log = null;
        private DbLog Log { get => (_log ?? (_log = DbLog.GetDbLog())); }

        public AccessCommandPack Execute(ServerAccess serverAccess, AccessCommandPack commPack)
        {
            string methodTag = "GetReturnTripExecution.Execute";
            string domainEntityTag = "Return Trip List";

            _serverAccess = serverAccess;
            _commandPack = commPack;
            bool eventSent = false;
            UIReturnTripListAck uiTripList;

            try
            {
                // KTM Trip List XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                AppDecorator.Config.Setting setting = NssIT.Kiosk.AppDecorator.Config.Setting.GetSetting();

                _command = (ReturnTripListCommand)commPack.Command;

                IPostRequestParam param = new TripRequest()
                {
                    DepartureDate = _command.PassengerDepartDate,
                    Origin = _command.FromStationCode,
                    Destination = _command.ToStationCode,
                    TrainService = null,
                    Channel = setting.PurchaseChannel,
                    MCounters_Id = setting.KioskId
                };

                dynamic apiRes = _serverAccess.WebAPI.Post<TripResult>(param, _webApiUrl, $@"{methodTag}").GetAwaiter().GetResult();

                bool validDataFound = false;

                if ((apiRes is TripResult tripX) && (tripX.Code.Equals(WebAPIAgent.ApiCodeOK)))
                {
                    TripResult trip = tripX;
                    //if ((trip.Data is null))
                    //{
                    //testMsg = $@"Success; Record Cound : {sttr.data?.Count().ToString() ?? "--"}";

                    if (NssIT.Kiosk.AppDecorator.Config.Setting.GetSetting().IsDebugMode)
                    {
                        trip = DebugTestData(tripX);
                    }

                    uiTripList = new UIReturnTripListAck(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, trip, AppDecorator.Common.TravelMode.ReturnOnly);
                    commPack.UpSertResult(false, uiTripList);
                    whenCompletedSendEvent(ResultStatus.Success, _commandPack.NetProcessId, _commandPack.ProcessId, $"{methodTag}:A02", uiTripList);
                    validDataFound = true;
                    //}
                    //else
                    //    apiRes = new WebApiException(new Exception("No valid data found; (EXIT21651)"));
                }

                if (validDataFound == false)
                {
                    string errorMsg = "";
                    string errCode = "99";

                    if (apiRes is TripResult errResult)
                    {
                        errCode = "99";
                        errorMsg = $@"No valid data ({domainEntityTag}) found; Code: {errCode}; (EXIT21652)";

                        Log.LogText(LogChannel, "-", errResult, "A05", methodTag, AppDecorator.Log.MessageType.Error,
                            netProcessId: commPack.NetProcessId, extraMsg: $@"{errorMsg}; MsgObj: TripResult");
                    }
                    else if (apiRes is WebApiException wex)
                    {
                        errCode = wex.Code;
                        errorMsg = $@"{wex.MessageString() ?? "Web process error"}; ({domainEntityTag});Code: {errCode}; (EXIT21653)";

                        Log.LogText(LogChannel, "-", errorMsg, "A08", methodTag,
                            AppDecorator.Log.MessageType.Error, netProcessId: commPack.NetProcessId);
                    }
                    else
                    {
                        errCode = "99";
                        errorMsg = $@"Unexpected error occurred; ({domainEntityTag}); Code: {errCode}; (EXIT21654)";

                        Log.LogText(LogChannel, "-", errorMsg, "A09", methodTag, AppDecorator.Log.MessageType.Error, netProcessId: commPack.NetProcessId);
                    }

                    List<string> msgs = new List<string>(new string[] { errorMsg });

                    TripResult errData = new TripResult() { Code = errCode, Data = null, Messages = msgs, Status = false };

                    uiTripList = new UIReturnTripListAck(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, errData, AppDecorator.Common.TravelMode.ReturnOnly)
                    { ErrorMessage = (string.IsNullOrWhiteSpace(errorMsg) == true ? "Unknown error when reading data (EXIT21655)" : errorMsg) };

                    commPack.UpSertResult(true, uiTripList, uiTripList.ErrorMessage);

                    whenCompletedSendEvent(ResultStatus.Fail, _commandPack.NetProcessId, _commandPack.ProcessId, $@"{methodTag}:A10", uiTripList, new Exception(uiTripList.ErrorMessage));
                }
            }
            catch (Exception ex)
            {
                uiTripList = new UIReturnTripListAck(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, null, AppDecorator.Common.TravelMode.ReturnOnly) { ErrorMessage = $@"{ex.Message}; (EXIT21656)" };
                commPack.UpSertResult(true, uiTripList, errorMessage: uiTripList.ErrorMessage);
                whenCompletedSendEvent(ResultStatus.ErrorFound, _commandPack.NetProcessId, _commandPack.ProcessId, $@"{methodTag}.Execute:A11", uiTripList, new Exception(uiTripList.ErrorMessage));
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

        private TripResult DebugTestData(TripResult tripResult)
        {
            if ((tripResult.Data is null) || (tripResult.Data.Length == 0))
                return tripResult;

            //NssIT.Kiosk.AppDecorator.Config.Setting.GetSetting();
            TripResult trpRet = new TripResult() { Code = tripResult.Code, Messages = tripResult.Messages, Status = tripResult.Status };

            TripModel[] data = new TripModel[tripResult.Data.Length + 4];

            int inx = 0;
            foreach (TripModel tpX in tripResult.Data)
            {
                data[inx] = new TripModel()
                {
                    ArrivalDayOffset = tpX.ArrivalDayOffset,
                    ArrivalLocalDateTime = tpX.ArrivalLocalDateTime,
                    ArrivalTimeFormat = tpX.ArrivalTimeFormat,
                    Currency = tpX.Currency,
                    DepartDateFormat = tpX.DepartDateFormat,
                    DepartLocalDateTime = tpX.DepartLocalDateTime,
                    DepartTimeFormat = tpX.DepartTimeFormat,
                    Id = tpX.Id,
                    Price = tpX.Price,
                    PriceFormat = tpX.PriceFormat,
                    SeatAvailable = tpX.SeatAvailable,
                    ServiceCategory = tpX.ServiceCategory,
                    TrainNo = tpX.TrainNo
                };
                inx++;
            }

            TripModel tp = tripResult.Data[0];
            //For - Available Seat Test
            data[inx] = new TripModel()
            {
                ArrivalDayOffset = 2,
                ArrivalLocalDateTime = tp.ArrivalLocalDateTime,
                ArrivalTimeFormat = tp.ArrivalTimeFormat,
                Currency = tp.Currency,
                DepartDateFormat = tp.DepartDateFormat,
                DepartLocalDateTime = tp.DepartLocalDateTime,
                DepartTimeFormat = tp.DepartTimeFormat,
                Id = tp.Id,
                Price = tp.Price,
                PriceFormat = tp.PriceFormat,
                SeatAvailable = 20,
                ServiceCategory = tp.ServiceCategory,
                TrainNo = tp.TrainNo
            };

            //For - Quick Finish Seat Test
            data[++inx] = new TripModel()
            {
                ArrivalDayOffset = 2,
                ArrivalLocalDateTime = tp.ArrivalLocalDateTime,
                ArrivalTimeFormat = tp.ArrivalTimeFormat,
                Currency = tp.Currency,
                DepartDateFormat = tp.DepartDateFormat,
                DepartLocalDateTime = tp.DepartLocalDateTime,
                DepartTimeFormat = tp.DepartTimeFormat,
                Id = tp.Id,
                Price = tp.Price,
                PriceFormat = tp.PriceFormat,
                SeatAvailable = 4,
                ServiceCategory = tp.ServiceCategory,
                TrainNo = tp.TrainNo
            };

            //For - Sold Out Test
            data[++inx] = new TripModel()
            {
                ArrivalDayOffset = tp.ArrivalDayOffset,
                ArrivalLocalDateTime = tp.ArrivalLocalDateTime,
                ArrivalTimeFormat = tp.ArrivalTimeFormat,
                Currency = tp.Currency,
                DepartDateFormat = tp.DepartDateFormat,
                DepartLocalDateTime = tp.DepartLocalDateTime,
                DepartTimeFormat = tp.DepartTimeFormat,
                Id = tp.Id,
                Price = tp.Price,
                PriceFormat = tp.PriceFormat,
                SeatAvailable = 0,
                ServiceCategory = tp.ServiceCategory,
                TrainNo = tp.TrainNo
            };

            //For - Not Enough Pax Seat Test
            data[++inx] = new TripModel()
            {
                ArrivalDayOffset = tp.ArrivalDayOffset,
                ArrivalLocalDateTime = tp.ArrivalLocalDateTime,
                ArrivalTimeFormat = tp.ArrivalTimeFormat,
                Currency = tp.Currency,
                DepartDateFormat = tp.DepartDateFormat,
                DepartLocalDateTime = tp.DepartLocalDateTime,
                DepartTimeFormat = tp.DepartTimeFormat,
                Id = tp.Id,
                Price = tp.Price,
                PriceFormat = tp.PriceFormat,
                SeatAvailable = 1,
                ServiceCategory = tp.ServiceCategory,
                TrainNo = tp.TrainNo
            };

            trpRet.Data = data;
            return trpRet;
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
