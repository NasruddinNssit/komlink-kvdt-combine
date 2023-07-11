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
using NssIT.Train.Kiosk.Common.Constants;

namespace NssIT.Kiosk.Server.AccessDB.CommandExec
{
    public class CustInfoUpdateExecutionV3 : IAccessCommandExec, IDisposable
    {
        private const string LogChannel = "ServerAccess";

        private AccessCommandPack _commandPack = null;
        private ServerAccess _serverAccess = null;

        private DbLog _log = null;
        private DbLog Log { get => (_log ?? (_log = DbLog.GetDbLog())); }

        private string _webApiUrl = @"TrainService/updatePassengerV3";

        public AccessCommandPack Execute(ServerAccess serverAccess, AccessCommandPack commPack)
        {
            string methodTag = "CustInfoUpdateExecutionV3.Execute";
            string domainEntityTag = "Updating Customer Info";

            _serverAccess = serverAccess;
            _commandPack = commPack;

            AppDecorator.Config.Setting _setting = NssIT.Kiosk.AppDecorator.Config.Setting.GetSetting();
            CustInfoUpdateCommand custUpdComm = null;

            bool eventSent = false;
            UIDepartCustInfoUpdateResult uiCustUpdRes;

            try
            {
                custUpdComm = (CustInfoUpdateCommand)_commandPack.Command;

                CustSeatDetail[] depSeats = custUpdComm.DepartPassengerSeatDetail;
                CustSeatDetail[] retSeats = custUpdComm.ReturnPassengerSeatDetail;

                UpdatePassengerRequest apiParam = new UpdatePassengerRequest()
                {
                    Booking_Id = custUpdComm.TransactionNo,
                    Channel = _setting.PurchaseChannel,
                    MCounters_Id = _setting.KioskId
                };

                List<PassengerDetailModel> pssgDetList = new List<PassengerDetailModel>();

                foreach (CustSeatDetail cSeatDet in depSeats)
                {
                    pssgDetList.Add(new PassengerDetailModel()
                    {
                        SeatLayoutModel_Id = cSeatDet.SeatLayoutModel_Id,
                        Gender = cSeatDet.Gender,
                        PassengerIC = cSeatDet.CustIC,
                        PassengerName = cSeatDet.CustName,
                        PhoneNo = cSeatDet.Contact,
                        TicketTypes_Id = cSeatDet.TicketType,
                        PromoCode = cSeatDet.DepartPromoCode,
                        PNR = cSeatDet.PNR
                    });
                }

                if (retSeats?.Length > 0)
                {
                    foreach (CustSeatDetail rSeatDet in retSeats)
                    {
                        pssgDetList.Add(new PassengerDetailModel()
                        {
                            SeatLayoutModel_Id = rSeatDet.SeatLayoutModel_Id,
                            Gender = rSeatDet.Gender,
                            PassengerIC = rSeatDet.CustIC,
                            PassengerName = rSeatDet.CustName,
                            PhoneNo = rSeatDet.Contact,
                            TicketTypes_Id = rSeatDet.TicketType,
                            PromoCode = rSeatDet.ReturnPromoCode,
                            PNR = rSeatDet.PNR
                        });
                    }
                }

                apiParam.PassengerDetailModels = pssgDetList.ToArray();

                // KTM Submit Passenger Info XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                dynamic apiRes = _serverAccess.WebAPI.Post<PassengerSubmissionResultV3>(apiParam, _webApiUrl, $@"{methodTag}").GetAwaiter().GetResult();

                if ((apiRes is PassengerSubmissionResultV3 pssgSubmRes))
                {
                    if (pssgSubmRes.Code.Equals(WebAPIAgent.ApiCodeOK))
                    {
                        // Promo Code Error Handling
                        if (pssgSubmRes.Data?.UpdatePassengerResult?.PassengerDetailErrorModels?.Length > 0)
                        {
                            PassengerDetailErrorModel[] errPssgInfo = (from pssg in pssgSubmRes.Data.UpdatePassengerResult.PassengerDetailErrorModels
                                                                       where pssg.PromoError.Equals(YesNo.Yes)
                                                                       select pssg).ToArray();

                            PassengerDetailErrorModel[] fatalErrPssgInfo = (from pssg in errPssgInfo
                                                                            where (string.IsNullOrWhiteSpace(pssg.PromoErrorMessage) == true)
                                                                            select pssg).ToArray();

                            if (fatalErrPssgInfo.Length > 0)
                            {
                                // Fatal Error !!
                                string errStr = $@"Unknown error when validate Promo Code '{fatalErrPssgInfo[0].PromoCode}'; Seat Layout Id : {fatalErrPssgInfo[0].SeatLayoutModel_Id}; Ticket Type Id : {fatalErrPssgInfo[0].TicketTypes_Id}";
                                List<string> msgs = new List<string>(new string[] { errStr });
                                PassengerSubmissionResultV3 errData = new PassengerSubmissionResultV3() { Code = "99", Data = null, Messages = msgs, Status = false };

                                uiCustUpdRes = new UIDepartCustInfoUpdateResult(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, errData) { ErrorMessage = errStr };

                                commPack.UpSertResult(true, uiCustUpdRes, uiCustUpdRes.ErrorMessage);

                                whenCompletedSendEvent(ResultStatus.Fail, _commandPack.NetProcessId, _commandPack.ProcessId, $@"{methodTag}:A18", uiCustUpdRes, new Exception(uiCustUpdRes.ErrorMessage));
                                //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                            }
                            else if (errPssgInfo.Length > 0)
                            {
                                uiCustUpdRes = new UIDepartCustInfoUpdateResult(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, pssgSubmRes, isRequestAmendPassengerInfo: true) { ErrorMessage = "Request Promo Code amendment" };
                                commPack.UpSertResult(false, uiCustUpdRes);
                                whenCompletedSendEvent(ResultStatus.Fail, _commandPack.NetProcessId, _commandPack.ProcessId, $"{methodTag}:A19", uiCustUpdRes);
                            }
                        }
                        //-----------------------------------------------------------------------------------------------------------------------------------
                        if ((eventSent == false)
                            && (pssgSubmRes.Data?.UpdatePassengerResult?.Error.Equals(YesNo.No) == true)
                            && (pssgSubmRes.Data?.GetPassengerResult?.Error.Equals(YesNo.No) == true)
                            )
                        {
                            PassengerResultDetailModel pssgResultDetailModel = null;
                            List<PassengerResultDetailModel> pssgDetResultList = new List<PassengerResultDetailModel>(pssgSubmRes.Data.GetPassengerResult.PassengerDetailModels);

                            foreach (CustSeatDetail cSeatDet in depSeats)
                            {
                                pssgResultDetailModel = null;
                                pssgResultDetailModel = pssgDetResultList.Find(p => ((p.SeatLayoutModel_Id == cSeatDet.SeatLayoutModel_Id) && (p.SeatLayoutModel_Id.Equals(Guid.Empty) == false)));

                                if (pssgResultDetailModel is null)
                                    throw new Exception("Unmatch seat record after passenger data submission to web; (EXIT21727)");
                            }

                            if (retSeats?.Length > 0)
                            {
                                foreach (CustSeatDetail rSeatDet in retSeats)
                                {
                                    pssgResultDetailModel = null;
                                    pssgResultDetailModel = pssgDetResultList.Find(p => ((p.SeatLayoutModel_Id == rSeatDet.SeatLayoutModel_Id) && (p.SeatLayoutModel_Id.Equals(Guid.Empty) == false)));

                                    if (pssgResultDetailModel is null)
                                        throw new Exception("Unmatch seat record after passenger data submission to web; (EXIT21728)");
                                }
                            }

                            uiCustUpdRes = new UIDepartCustInfoUpdateResult(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, pssgSubmRes);
                            commPack.UpSertResult(false, uiCustUpdRes);
                            whenCompletedSendEvent(ResultStatus.Success, _commandPack.NetProcessId, _commandPack.ProcessId, $"{methodTag}:A02", uiCustUpdRes);
                        }
                    }

                    if (eventSent == false)
                    {
                        string errMsg = "";

                        if ((string.IsNullOrWhiteSpace(pssgSubmRes.MessageString()) == false))
                        {
                            errMsg += pssgSubmRes.MessageString();
                        }
                        else if ((string.IsNullOrWhiteSpace(pssgSubmRes.Data?.UpdatePassengerResult?.ErrorMessage) == false))
                        {
                            errMsg += pssgSubmRes.Data.UpdatePassengerResult.ErrorMessage;
                        }
                        else if ((string.IsNullOrWhiteSpace(pssgSubmRes.Data?.GetPassengerResult?.ErrorMessage) == false))
                        {
                            errMsg += pssgSubmRes.Data.GetPassengerResult.ErrorMessage;
                        }
                        else if (string.IsNullOrWhiteSpace(errMsg))
                        {
                            errMsg += $@"Unable to submit passenger details. (EXIT21726)";
                        }

                        errMsg = errMsg + $@"; ErrorCode:{pssgSubmRes.Code}";
                        //-------------------------------------------------------------------------------------------------

                        Log.LogText(LogChannel, "-", pssgSubmRes, "A07", methodTag, AppDecorator.Log.MessageType.Error,
                            netProcessId: commPack.NetProcessId, extraMsg: $@"{errMsg}; MsgObj: PassengerSubmissionResultV3");

                        apiRes = new WebApiException(new Exception($@"Invalid data transaction; {errMsg}; (EXIT21720)"));
                    }
                }

                if (eventSent == false)
                {
                    string errorMsg = "";
                    string errCode = "99";

                    if (apiRes is PassengerSubmissionResultV3 errResult)
                    {
                        errCode = "99";
                        errorMsg = $@"No valid data ({domainEntityTag}) found; Code: {errCode}; (EXIT21721)";

                        Log.LogText(LogChannel, "-", errResult, "A05", methodTag, AppDecorator.Log.MessageType.Error,
                            netProcessId: commPack.NetProcessId, extraMsg: $@"{errorMsg}; MsgObj: PassengerSubmissionResultV3");
                    }
                    else if (apiRes is WebApiException wex)
                    {
                        errCode = wex.Code;
                        errorMsg = $@"{wex.MessageString() ?? "Web process error"}; ({domainEntityTag});Code: {errCode}; (EXIT21722)";

                        Log.LogText(LogChannel, "-", errorMsg, "A08", methodTag,
                            AppDecorator.Log.MessageType.Error, netProcessId: commPack.NetProcessId);
                    }
                    else
                    {
                        errCode = "99";
                        errorMsg = $@"Unknown error; ({domainEntityTag}); Code: {errCode};(EXIT21723)";

                        Log.LogText(LogChannel, "-", errorMsg, "A09", methodTag, AppDecorator.Log.MessageType.Error, netProcessId: commPack.NetProcessId);
                    }

                    List<string> msgs = new List<string>(new string[] { errorMsg });

                    PassengerSubmissionResultV3 errData = new PassengerSubmissionResultV3() { Code = errCode, Data = null, Messages = msgs, Status = false };

                    uiCustUpdRes = new UIDepartCustInfoUpdateResult(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, errData)
                    { ErrorMessage = (string.IsNullOrWhiteSpace(errorMsg) == true ? "Unknown error when reading data (EXIT21724)" : errorMsg) };

                    commPack.UpSertResult(true, uiCustUpdRes, uiCustUpdRes.ErrorMessage);

                    whenCompletedSendEvent(ResultStatus.Fail, _commandPack.NetProcessId, _commandPack.ProcessId, $@"{methodTag}:A10", uiCustUpdRes, new Exception(uiCustUpdRes.ErrorMessage));
                }
            }
            catch (Exception ex)
            {
                uiCustUpdRes = new UIDepartCustInfoUpdateResult(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, null) { ErrorMessage = $@"{ex.Message}; (EXIT21725)" };
                commPack.UpSertResult(true, uiCustUpdRes, errorMessage: uiCustUpdRes.ErrorMessage);
                whenCompletedSendEvent(ResultStatus.ErrorFound, _commandPack.NetProcessId, _commandPack.ProcessId, $@"{methodTag}:A15", uiCustUpdRes, new Exception(uiCustUpdRes.ErrorMessage));
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
