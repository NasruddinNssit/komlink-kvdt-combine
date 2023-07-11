using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Command.Sales;
using NssIT.Kiosk.AppDecorator.Common.AppService.Events;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
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
    public class GetKomuterBookingExecution : IAccessCommandExec, IDisposable
    {
        private const string LogChannel = "ServerAccess";
        private AccessCommandPack _commandPack = null;
        private ServerAccess _serverAccess = null;

        private KomuterTicketBookingRequestCommand _command = null;
        private string _webApiUrl = @"Komuter/updateBooking";
        private DbLog _log = null;
        private DbLog Log { get => (_log ?? (_log = DbLog.GetDbLog())); }

        public AccessCommandPack Execute(ServerAccess serverAccess, AccessCommandPack commPack)
        {
            string methodTag = "GetKomuterBookingExecution.Execute";
            string domainEntityTag = "Get Komuter Booking";

            _serverAccess = serverAccess;
            _commandPack = commPack;
            bool eventSent = false;
            UIKomuterTicketBookingAck uiTickBook;

            try
            {
                // KTM Trip List XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                AppDecorator.Config.Setting setting = NssIT.Kiosk.AppDecorator.Config.Setting.GetSetting();

                _command = (KomuterTicketBookingRequestCommand)commPack.Command;

                List<KomuterNewBookingTicketListModel> tickBookList = new List<KomuterNewBookingTicketListModel>();
                foreach(TicketItem tItm in _command.TicketItemList)
                {
                    List<KomuterNewBookingTicketDetailListModel> detList = new List<KomuterNewBookingTicketDetailListModel>();

                    if ((tItm.DetailList?.Length > 0) == true)
                    {
                        foreach(TicketItemDetail tDet in tItm.DetailList)
                        {
                            detList.Add(new KomuterNewBookingTicketDetailListModel() { TicketTypeId = tDet.TicketTypeId, MyKadId = tDet.MyKadId, Name = tDet.Name });
                        }
                    }

                    tickBookList.Add(new KomuterNewBookingTicketListModel() {TicketTypeId = tItm.TicketTypeId, Quantity = tItm.Quantity, DetailList = (detList.Count > 0 ? detList.ToArray() : null) });
                }

                KomuterTicketBookingRequest param = new KomuterTicketBookingRequest()
                {
                    Channel = setting.PurchaseChannel,
                    DestinationStationId = _command.DestinationStationId,
                    DestinationStationName = _command.DestinationStationName,
                    KomuterPackageId = _command.KomuterPackageId,
                    MCounter_Id = setting.KioskId,
                    OriginStationId = _command.OriginStationId,
                    OriginStationName = _command.OriginStationName, 
                    KomuterNewBookingTicketList = tickBookList.ToArray()
                };

                dynamic apiRes = _serverAccess.WebAPI.Post<KomuterBookingResult>(param, _webApiUrl, $@"{methodTag}").GetAwaiter().GetResult();

                bool validDataFound = false;

                if ((apiRes is KomuterBookingResult bookX) && (bookX.Code?.Equals(WebAPIAgent.ApiCodeOK) == true))
                {
                    KomuterBookingResult bookR = bookX;

                    if ((bookR.Data?.Error?.Equals(YesNo.No) == true)
                        && (string.IsNullOrWhiteSpace(bookR.Data?.BookingNo) == false))
                    {
                        uiTickBook = new UIKomuterTicketBookingAck(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, bookR);
                        commPack.UpSertResult(false, uiTickBook);
                        whenCompletedSendEvent(ResultStatus.Success, _commandPack.NetProcessId, _commandPack.ProcessId, $"{methodTag}:A02", uiTickBook);
                        validDataFound = true;
                    }
                    else
                    {
                        apiRes = new WebApiException(new Exception($@"No valid data found; (EXIT21760); Error Code : {bookR?.Code}"));

                        Log.LogText(LogChannel, "-", bookX, "A03", methodTag, AppDecorator.Log.MessageType.Error,
                            netProcessId: commPack.NetProcessId, extraMsg: $@"No valid data found; (EXIT21760); Error Code : {bookR?.Code}; MsgObj: KomuterBookingResult");
                    }
                }

                if (validDataFound == false)
                {
                    string errorMsg = "";
                    string errCode = "99";

                    if (apiRes is KomuterBookingResult errResult)
                    {
                        errCode = "99";
                        errorMsg = $@"No valid data ({domainEntityTag}) found; Code: {errCode}; (EXIT21761)";

                        Log.LogText(LogChannel, "-", errResult, "A05", methodTag, AppDecorator.Log.MessageType.Error,
                            netProcessId: commPack.NetProcessId, extraMsg: $@"{errorMsg}; MsgObj: KomuterBookingResult");
                    }
                    else if (apiRes is WebApiException wex)
                    {
                        errCode = wex.Code;
                        errorMsg = $@"{wex.MessageString() ?? "Web process error"}; ({domainEntityTag});Code: {errCode}; (EXIT21762)";

                        Log.LogText(LogChannel, "-", errorMsg, "A08", methodTag,
                            AppDecorator.Log.MessageType.Error, netProcessId: commPack.NetProcessId);
                    }
                    else
                    {
                        errCode = "99";
                        errorMsg = $@"Unexpected error occurred; ({domainEntityTag}); Code: {errCode}; (EXIT21763)";

                        Log.LogText(LogChannel, "-", errorMsg, "A09", methodTag, AppDecorator.Log.MessageType.Error, netProcessId: commPack.NetProcessId);
                    }

                    List<string> msgs = new List<string>(new string[] { errorMsg });

                    KomuterBookingResult errData = new KomuterBookingResult() { Code = errCode, Data = null, Messages = msgs, Status = false };

                    uiTickBook = new UIKomuterTicketBookingAck(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, errData)
                    { ErrorMessage = (string.IsNullOrWhiteSpace(errorMsg) == true ? "Unknown error when reading data (EXIT21764)" : errorMsg) };

                    commPack.UpSertResult(true, uiTickBook, uiTickBook.ErrorMessage);

                    whenCompletedSendEvent(ResultStatus.Fail, _commandPack.NetProcessId, _commandPack.ProcessId, $@"{methodTag}:A10", uiTickBook, new Exception(uiTickBook.ErrorMessage));
                }
            }
            catch (Exception ex)
            {
                List<string> msgs = new List<string>(new string[] { $@"{ex.Message}; (EXIT21765)" });
                KomuterBookingResult errData = new KomuterBookingResult() { Code = "99", Data = null, Messages = msgs, Status = false };

                uiTickBook = new UIKomuterTicketBookingAck(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, errData) { ErrorMessage = errData.MessageString() };
                commPack.UpSertResult(true, uiTickBook, errorMessage: uiTickBook.ErrorMessage);
                whenCompletedSendEvent(ResultStatus.ErrorFound, _commandPack.NetProcessId, _commandPack.ProcessId, $@"{methodTag}.Execute:A11", uiTickBook, new Exception(uiTickBook.ErrorMessage));
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

