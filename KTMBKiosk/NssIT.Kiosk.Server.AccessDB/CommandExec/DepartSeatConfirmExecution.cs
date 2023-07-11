using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Command.Sales;
using NssIT.Kiosk.AppDecorator.Common.AppService.Events;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
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
    public class DepartSeatConfirmExecution : IAccessCommandExec, IDisposable
    {
        private const string LogChannel = "ServerAccess";

        private AccessCommandPack _commandPack = null;
        private ServerAccess _serverAccess = null;

        private DbLog _log = null;
        private DbLog Log { get => (_log ?? (_log = DbLog.GetDbLog())); }

        private string _webApiUrl = @"TrainService/updateBooking";

        public AccessCommandPack Execute(ServerAccess serverAccess, AccessCommandPack commPack)
        {
            string methodTag = "DepartSeatConfirmExecution.Execute";
            string domainEntityTag = "Seat Booking";

            _serverAccess = serverAccess;
            _commandPack = commPack;
            bool eventSent = false;
            DepartSeatConfirmCommand command = (DepartSeatConfirmCommand)_commandPack.Command;
            UIDepartSeatConfirmResult uiSeatConf = null;
            AppDecorator.Config.Setting _setting = NssIT.Kiosk.AppDecorator.Config.Setting.GetSetting();

            methodTag = $@"{methodTag}; Train Inx.: {command.TrainIndex}";
            domainEntityTag = $@"{domainEntityTag}; Train Inx.: {command.TrainIndex}";

            try
            {   // BookingResult
                // KTM Seat Boooking XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                BookingSeatRequest apiParams = new BookingSeatRequest()
                {
                    Booking_Id = command.BookingId, 
                    Channel = _setting.PurchaseChannel, 
                    MCounters_Id = _setting.KioskId, 
                    TrainSeatModel_Id = command.TrainSeatModelId
                };

                List<BookingSeatDetailModel> seatBookingList = new List<BookingSeatDetailModel>();
                foreach (CustSeatDetail seatBooking in command.PassengerSeatDetail)
                {
                    seatBookingList.Add(new BookingSeatDetailModel() { SeatLayoutModel_Id = seatBooking.SeatLayoutModel_Id });
                }
                apiParams.BookingSeatDetailModels = seatBookingList.ToArray();

                dynamic apiRes = _serverAccess.WebAPI.Post<BookingResult>(apiParams, _webApiUrl, $@"{methodTag}").GetAwaiter().GetResult();

                bool validDataFound = false;

                if ((apiRes is BookingResult bookR) && (bookR.Code.Equals(WebAPIAgent.ApiCodeOK)) && (bookR.Data != null) && (bookR.Data.ErrorCode == 0))
                {
                    if ((string.IsNullOrWhiteSpace(bookR.Data.Booking_Id)) == false)
                    {
                        uiSeatConf = new UIDepartSeatConfirmResult(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, bookR);
                        commPack.UpSertResult(false, uiSeatConf);
                        whenCompletedSendEvent(ResultStatus.Success, _commandPack.NetProcessId, _commandPack.ProcessId, $"{methodTag}:A02", uiSeatConf);
                        validDataFound = true;
                    }
                    else
                    {
                        apiRes = new WebApiException(new Exception($@"No valid data found; {bookR.Data.ErrorMessage}; (EXIT21640)"));
                    }
                }

                if (validDataFound == false)
                {
                    string errorMsg = "";
                    string errCode = "99";

                    if (apiRes is BookingResult errResult)
                    {
                        errCode = "99";
                        errorMsg = $@"No valid data ({domainEntityTag}) found; Code: {errCode}; (EXIT21641)";

                        Log.LogText(LogChannel, "-", errResult, "A05", methodTag, AppDecorator.Log.MessageType.Error,
                            netProcessId: commPack.NetProcessId, extraMsg: $@"{errorMsg}; MsgObj: BookingResult");
                    }
                    else if (apiRes is WebApiException wex)
                    {
                        errCode = wex.Code;
                        errorMsg = $@"{wex.MessageString() ?? "Web process error"}; ({domainEntityTag});Code: {errCode}; (EXIT21642)";

                        Log.LogText(LogChannel, "-", wex, "A08", methodTag,
                            AppDecorator.Log.MessageType.Error, netProcessId: commPack.NetProcessId, extraMsg: $@"{errorMsg}; MsgObj: WebApiException");
                    }
                    else
                    {
                        errCode = "99";
                        errorMsg = $@"Unexpected error occurred; ({domainEntityTag}); Code: {errCode};(EXIT21643)";

                        Log.LogText(LogChannel, "-", errorMsg, "A09", methodTag, AppDecorator.Log.MessageType.Error, netProcessId: commPack.NetProcessId);
                    }

                    List<string> msgs = new List<string>(new string[] { errorMsg });

                    BookingResult errData = new BookingResult() { Code = errCode, Data = null, Messages = msgs, Status = false };

                    uiSeatConf = new UIDepartSeatConfirmResult(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, errData)
                    { ErrorMessage = (string.IsNullOrWhiteSpace(errorMsg) == true ? "Unknown error when reading data (EXIT21644)" : errorMsg) };

                    commPack.UpSertResult(true, uiSeatConf, uiSeatConf.ErrorMessage);

                    whenCompletedSendEvent(ResultStatus.Fail, _commandPack.NetProcessId, _commandPack.ProcessId, $@"{methodTag}:A10", uiSeatConf, new Exception(uiSeatConf.ErrorMessage));
                }
            }
            catch (Exception ex)
            {
                BookingResult errData = new BookingResult() { Code = "99", Data = null, Messages = new List<string>(new string[1] { $@"{ex.Message}; (EXIT21645)" }), Status = false };
                uiSeatConf = new UIDepartSeatConfirmResult(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, errData) { ErrorMessage = $@"{ex.Message}; (EXIT21645)" };
                commPack.UpSertResult(true, uiSeatConf, errorMessage: uiSeatConf.ErrorMessage);
                whenCompletedSendEvent(ResultStatus.ErrorFound, _commandPack.NetProcessId, _commandPack.ProcessId, $@"{methodTag}:A15", uiSeatConf, new Exception(uiSeatConf.ErrorMessage));
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