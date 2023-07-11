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

    /// <summary>
    /// ClassCode:EXIT25.017
    /// </summary>
    public class GetPNRTicketTypeExecution : IAccessCommandExec, IDisposable
    {
        private const string LogChannel = "ServerAccess";

        private AccessCommandPack _commandPack = null;
        private ServerAccess _serverAccess = null;

        private DbLog _log = null;
        private DbLog Log { get => (_log ?? (_log = DbLog.GetDbLog())); }

        private string _webApiUrl = @"TrainService/getPNRTicketType";

        /// <summary>
        /// FuncCode:EXIT25.017010
        /// </summary>
        public AccessCommandPack Execute(ServerAccess serverAccess, AccessCommandPack commPack)
        {
            string methodTag = "GetPNRTicketTypeExecution.Execute";
            string domainEntityTag = "Get Custumer PNR Ticket Type";
            AppDecorator.Config.Setting _setting = NssIT.Kiosk.AppDecorator.Config.Setting.GetSetting();

            _serverAccess = serverAccess;
            _commandPack = commPack;

            bool eventSent = false;
            UICustInfoPNRTicketTypeAck uiCustPNRTickType = null;

            try
            {
                CustInfoPNRTicketTypeRequestCommand pnrTickTypeComm = (CustInfoPNRTicketTypeRequestCommand)_commandPack.Command;

                // KTM Get Custumer Info Prerequisite Data - Ticket Type XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                PassengerPNRTicketTypeRequest apiParams = new PassengerPNRTicketTypeRequest()
                { 
                    Booking_Id = pnrTickTypeComm.BookingId, 
                    IdentityNo = pnrTickTypeComm.PassenggerIdentityNo, 
                    TripScheduleSeatLayoutDetails_Ids = pnrTickTypeComm.TripScheduleSeatLayoutDetails_Ids,
                    Channel = _setting.PurchaseChannel,
                    MCounters_Id = _setting.KioskId
                };
                //PassengerPNRTicketTypeResult
                dynamic apiRes = _serverAccess.WebAPI.Post<PassengerPNRTicketTypeResult>(apiParams, _webApiUrl, $@"{methodTag}").GetAwaiter().GetResult();

                bool validDataFound = false;

                if ((apiRes is PassengerPNRTicketTypeResult ttr) && (ttr.Code.Equals(WebAPIAgent.ApiCodeOK)))
                {
                    if ((ttr.Data?.PassengerTicketTypeModels?.Length > 0) && 
                        ((from ticTyp in ttr.Data.PassengerTicketTypeModels where ticTyp.IsEnabled select ticTyp).ToArray().Length > 0))
                    {
                        PassengerPNRTicketTypeResult resl = new PassengerPNRTicketTypeResult()
                        {
                            Code = ttr.Code,
                            Messages = ttr.Messages,
                            Status = ttr.Status,
                            Data = new PassengerPNRTicketTypeModel()
                            {
                                BookingNo = ttr.Data.BookingNo,
                                Booking_Id = ttr.Data.Booking_Id,
                                Error = ttr.Data.Error,
                                ErrorMessage = ttr.Data.ErrorMessage, 
                                PNRNo = ttr.Data.PNRNo,
                                PassengerTicketTypeModels = (from ticTyp in ttr.Data.PassengerTicketTypeModels where ticTyp.IsEnabled select ticTyp).ToArray()
                            }
                        };

                        uiCustPNRTickType = new UICustInfoPNRTicketTypeAck(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, resl);

                        commPack.UpSertResult(false, uiCustPNRTickType);
                        whenCompletedSendEvent(ResultStatus.Success, _commandPack.NetProcessId, _commandPack.ProcessId, $"{methodTag}:A02", uiCustPNRTickType);
                        validDataFound = true;
                    }
                    else
                        apiRes = new WebApiException(new Exception("No valid PNR Ticket Type found; (EXIT25.017010.X01)"));
                }

                if (validDataFound == false)
                {
                    string errorMsg = "";
                    string errCode = "99";

                    if (apiRes is PassengerPNRTicketTypeResult errResult)
                    {
                        errCode = "99";
                        errorMsg = $@"No valid data ({domainEntityTag}) found; Code: {errCode}; (EXIT25.017010.X05)";

                        Log.LogText(LogChannel, "-", errResult, "A05", methodTag, AppDecorator.Log.MessageType.Error,
                            netProcessId: commPack.NetProcessId, extraMsg: $@"{errorMsg}; MsgObj: PassengerPNRTicketTypeResult");
                    }
                    else if (apiRes is WebApiException wex)
                    {
                        errCode = wex.Code;
                        errorMsg = $@"{wex.MessageString() ?? "Web process error"}; ({domainEntityTag});Code: {errCode}; (EXIT25.017010.X08)";

                        Log.LogText(LogChannel, "-", errorMsg, "A08", methodTag,
                            AppDecorator.Log.MessageType.Error, netProcessId: commPack.NetProcessId);
                    }
                    else
                    {
                        errCode = "99";
                        errorMsg = $@"Unexpected error occurred; ({domainEntityTag}); Code: {errCode};(EXIT25.017010.X11)";

                        Log.LogText(LogChannel, "-", errorMsg, "A09", methodTag, AppDecorator.Log.MessageType.Error, netProcessId: commPack.NetProcessId);
                    }

                    List<string> msgs = new List<string>(new string[] { errorMsg });

                    PassengerPNRTicketTypeResult errData = new PassengerPNRTicketTypeResult() { Code = errCode, Data = null, Messages = msgs, Status = false };

                    uiCustPNRTickType = new UICustInfoPNRTicketTypeAck(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, errData)
                    { ErrorMessage = (string.IsNullOrWhiteSpace(errorMsg) == true ? "Unknown error when reading data (EXIT25.017010.X13)" : errorMsg) };

                    commPack.UpSertResult(true, uiCustPNRTickType, uiCustPNRTickType.ErrorMessage);

                    whenCompletedSendEvent(ResultStatus.Fail, _commandPack.NetProcessId, _commandPack.ProcessId, $@"{methodTag}:A10", uiCustPNRTickType, new Exception(uiCustPNRTickType.ErrorMessage));
                }
            }
            catch (Exception ex)
            {
                uiCustPNRTickType = new UICustInfoPNRTicketTypeAck(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, null) { ErrorMessage = $@"{ex.Message}; (EXIT25.017010.EX01)" };
                commPack.UpSertResult(true, uiCustPNRTickType, errorMessage: uiCustPNRTickType.ErrorMessage);
                whenCompletedSendEvent(ResultStatus.ErrorFound, _commandPack.NetProcessId, _commandPack.ProcessId, $@"{methodTag}:A15", uiCustPNRTickType, new Exception(uiCustPNRTickType.ErrorMessage));
            }

            return commPack;
            //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

            /// <summary>
            /// FuncCode:EXIT25.017020
            /// </summary>
            void whenCompletedSendEvent(ResultStatus resultState, Guid? netProcessId, string processId, string lineTag, IKioskMsg uiKioskMsg,
                Exception error = null)
            {
                if (eventSent)
                    return;
                _serverAccess.RaiseOnSendMessage(resultState, netProcessId, processId, lineTag, uiKioskMsg, error);
                eventSent = true;
            }
        }

        /// <summary>
        /// FuncCode:EXIT25.017990
        /// </summary>
        public void Dispose()
        {
            _commandPack = null;
            _log = null;
            _serverAccess = null;
        }
    }
}

