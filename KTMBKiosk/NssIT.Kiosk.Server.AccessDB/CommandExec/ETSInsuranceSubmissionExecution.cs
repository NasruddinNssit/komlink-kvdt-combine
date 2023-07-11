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
    public class ETSInsuranceSubmissionExecution : IAccessCommandExec, IDisposable
    {
        private const string LogChannel = "ServerAccess";

        private AccessCommandPack _commandPack = null;
        private ServerAccess _serverAccess = null;

        private DbLog _log = null;
        private DbLog Log { get => (_log ?? (_log = DbLog.GetDbLog())); }

        private string _webApiUrl = @"TrainService/updateBookingInsurance";

        public AccessCommandPack Execute(ServerAccess serverAccess, AccessCommandPack commPack)
        {
            string methodTag = "ETSInsuranceSubmissionExecution.Execute";
            string domainEntityTag = "Submit ETS Insurance";

            _serverAccess = serverAccess;
            _commandPack = commPack;
            bool eventSent = false;
            UISalesETSInsuranceSubmissionResult uiSubmissionRes;

            try
            {
                AppDecorator.Config.Setting setting = NssIT.Kiosk.AppDecorator.Config.Setting.GetSetting();

                ETSInsuranceSubmissionCommand axComm = (ETSInsuranceSubmissionCommand)_commandPack.Command;

                UpdateBookingInsuranceRequest apiParam = new UpdateBookingInsuranceRequest()
                {
                    Booking_Id = axComm.TransactionNo,
                    Channel = setting.PurchaseChannel,
                    MCounters_Id = setting.KioskId,
                    MInsuranceHeaders_Id = axComm.InsuranceHeadersId
                };

                // XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                dynamic apiRes = _serverAccess.WebAPI.Post<UpdateBookingInsuranceResult>(apiParam, _webApiUrl, $@"{methodTag}").GetAwaiter().GetResult();

                bool validDataFound = false;

                if ((apiRes is UpdateBookingInsuranceResult sttr) && (sttr.Code.Equals(WebAPIAgent.ApiCodeOK)))
                {
                    if ((sttr.Status == true) && (sttr.Code?.Equals("0") == true) && (sttr.Data?.Error?.Equals(YesNo.No) == true))
                    {
                        //testMsg = $@"Success; Record Cound : {sttr.data?.Count().ToString() ?? "--"}";
                        uiSubmissionRes = new UISalesETSInsuranceSubmissionResult(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, sttr);
                        commPack.UpSertResult(false, uiSubmissionRes);
                        whenCompletedSendEvent(ResultStatus.Success, _commandPack.NetProcessId, _commandPack.ProcessId, $"{methodTag}:A02", uiSubmissionRes);
                        validDataFound = true;
                    }
                    else
                        apiRes = new WebApiException(new Exception("Fail to submit insurance; (EXIT21863)"));
                }

                if (validDataFound == false)
                {
                    string errorMsg = "";
                    string errCode = "99";

                    if (apiRes is UpdateBookingInsuranceResult errResult)
                    {
                        errCode = (errResult.Code?.Equals("0") == true) ? "99" : errResult.Code;
                        errorMsg = $@"Fail to submit insurance; ({domainEntityTag}); Code: {errCode}; (EXIT21861)";

                        Log.LogText(LogChannel, "-", errResult, "A05", methodTag, AppDecorator.Log.MessageType.Error,
                            netProcessId: commPack.NetProcessId, extraMsg: $@"{errorMsg}; MsgObj: UpdateBookingInsuranceResult");
                    }
                    else if (apiRes is WebApiException wex)
                    {
                        errCode = wex.Code;
                        errorMsg = $@"{wex.MessageString() ?? "Web process error"}; ({domainEntityTag});Code: {errCode}; (EXIT21864)";

                        Log.LogText(LogChannel, "-", errorMsg, "A08", methodTag,
                            AppDecorator.Log.MessageType.Error, netProcessId: commPack.NetProcessId);
                    }
                    else
                    {
                        errCode = "99";
                        errorMsg = $@"Unexpected error occurred; ({domainEntityTag}); Code: {errCode};(EXIT21865)";

                        Log.LogText(LogChannel, "-", errorMsg, "A09", methodTag, AppDecorator.Log.MessageType.Error, netProcessId: commPack.NetProcessId);
                    }

                    List<string> msgs = new List<string>(new string[] { errorMsg });

                    UpdateBookingInsuranceResult errData = new UpdateBookingInsuranceResult() { Code = errCode, Data = null, Messages = msgs, Status = false };

                    uiSubmissionRes = new UISalesETSInsuranceSubmissionResult(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, errData)
                    { ErrorMessage = string.IsNullOrWhiteSpace(errorMsg) == true ? "Unknown error when reading data (EXIT21866)" : errorMsg };

                    commPack.UpSertResult(true, uiSubmissionRes, uiSubmissionRes.ErrorMessage);

                    whenCompletedSendEvent(ResultStatus.Fail, _commandPack.NetProcessId, _commandPack.ProcessId, $@"{methodTag}:A10", uiSubmissionRes, new Exception(uiSubmissionRes.ErrorMessage));
                }
            }
            catch (Exception ex)
            {
                uiSubmissionRes = new UISalesETSInsuranceSubmissionResult(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, null) { ErrorMessage = $@"{ex.Message}; (EXIT21869)" };
                commPack.UpSertResult(true, uiSubmissionRes, errorMessage: uiSubmissionRes.ErrorMessage);
                whenCompletedSendEvent(ResultStatus.ErrorFound, _commandPack.NetProcessId, _commandPack.ProcessId, $@"{methodTag}:A15", uiSubmissionRes, new Exception(uiSubmissionRes.ErrorMessage));
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