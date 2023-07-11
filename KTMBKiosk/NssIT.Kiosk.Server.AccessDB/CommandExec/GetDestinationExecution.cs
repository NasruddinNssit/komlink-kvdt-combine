using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Command.Sales;
using NssIT.Kiosk.AppDecorator.Common.AppService.Events;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI;
using NssIT.Kiosk.Log.DB;
using NssIT.Train.Kiosk.Common.Common;
using NssIT.Train.Kiosk.Common.Common.WebApi;
using NssIT.Train.Kiosk.Common.Data;
using NssIT.Train.Kiosk.Common.Data.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Server.AccessDB.CommandExec
{
	public class GetDestinationExecution : IAccessCommandExec, IDisposable 
	{
        private const string LogChannel = "ServerAccess";

		private AccessCommandPack _commandPack = null;
        private ServerAccess _serverAccess = null;

        private DbLog _log = null;
        private DbLog Log { get => (_log ?? (_log = DbLog.GetDbLog())); }

        private string _webApiUrl = @"Station/getStation";

        public AccessCommandPack Execute(ServerAccess serverAccess, AccessCommandPack commPack)
        {
            string methodTag = "GetDestinationExecution.Execute";
            string domainEntityTag = "Destination Stations List";

            _serverAccess = serverAccess;
            _commandPack = commPack;
            bool eventSent = false;
            UIDestinationListAck uiDest;

            string originStationId = ((DestinationListRequestCommand)_commandPack.Command).OriginStationId;

            try
            {
                dynamic apiRes = _serverAccess.WebAPI.Post<StationResult>(null, _webApiUrl, $@"{methodTag}").GetAwaiter().GetResult();

                bool validDataFound = false;

                if ((apiRes is StationResult sttr) && (sttr.Status == true) &&(sttr.Code.Equals(WebAPIAgent.ApiCodeOK)))
                {
                    if ((sttr.Data?.Length > 0))
                    {
                        sttr.Data = FilterStationList(originStationId, sttr);

                        uiDest = new UIDestinationListAck(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, sttr);
                        commPack.UpSertResult(false, uiDest);
                        whenCompletedSendEvent(ResultStatus.Success, _commandPack.NetProcessId, _commandPack.ProcessId, $"{methodTag}:A02", uiDest);
                        validDataFound = true;
                    }
                    else
                        apiRes = new WebApiException(new Exception("No valid data found; (EXIT21810)"));
                }

                if (validDataFound == false)
                {
                    string errorMsg = "";
                    string errCode = "99";

                    if (apiRes is StationResult sttrX)
                    {
                        errCode = "99";
                        errorMsg = $@"No valid data ({domainEntityTag}) found; Code: {errCode}; (EXIT21801)";

                        Log.LogText(LogChannel, "-", sttrX, "A05", methodTag, AppDecorator.Log.MessageType.Error,
                            netProcessId: commPack.NetProcessId, extraMsg: $@"{errorMsg}; MsgObj: StationResult");
                    }
                    else if (apiRes is WebApiException wex)
                    {
                        errCode = wex.Code;
                        errorMsg = $@"{wex.MessageString() ?? "Web process error"}; ({domainEntityTag});Code: {errCode}; (EXIT21804)";

                        Log.LogText(LogChannel, "-", errorMsg, "A08", methodTag,
                            AppDecorator.Log.MessageType.Error, netProcessId: commPack.NetProcessId);
                    }
                    else
                    {
                        errCode = "99";
                        errorMsg = $@"Unexpected error occurred; ({domainEntityTag}); Code: {errCode};(EXIT21805)";

                        Log.LogText(LogChannel, "-", errorMsg, "A09", methodTag, AppDecorator.Log.MessageType.Error, netProcessId: commPack.NetProcessId);
                    }

                    List<string> msgs = new List<string>(new string[] { errorMsg });

                    StationResult errData = new StationResult() { Code = errCode, Data = null, Messages = msgs, Status = false };

                    uiDest = new UIDestinationListAck(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, errData)
                    { ErrorMessage = (string.IsNullOrWhiteSpace(errorMsg) == true ? "Unknown error when reading data (EXIT21806)" : errorMsg) };

                    commPack.UpSertResult(true, uiDest, uiDest.ErrorMessage);

                    whenCompletedSendEvent(ResultStatus.Fail, _commandPack.NetProcessId, _commandPack.ProcessId, $@"{methodTag}:A10", uiDest, new Exception(uiDest.ErrorMessage));
                }
            }
            catch (Exception ex)
            {
                uiDest = new UIDestinationListAck(commPack.NetProcessId, commPack.ProcessId, DateTime.Now, null) { ErrorMessage = $@"{ex.Message}; (EXIT21809)" };
                commPack.UpSertResult(true, uiDest, errorMessage: uiDest.ErrorMessage);
                whenCompletedSendEvent(ResultStatus.ErrorFound, _commandPack.NetProcessId, _commandPack.ProcessId, $@"{methodTag}:A15", uiDest, new Exception(uiDest.ErrorMessage));
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

            StationDetailsModel[] FilterStationList(string origStationId, StationResult stationEnt)
            {
                // Get Origin Station Data
                StationDetailsModel[] oriStationList = (from stt in stationEnt.Data
                    where stt.Id.Equals(origStationId)
                    select stt).ToArray();

                if (oriStationList.Length == 0)
                    throw new Exception($@"Unregconize origin station code ({origStationId})");

                StationDetailsModel oriStation = oriStationList[0];
                // ---------------------------------------------------------------
                // Get All Expected Train Services
                List<string> trainServiceList = null;
                if (oriStation.TrainService?.Count > 0)
                {
                    trainServiceList = new List<string>(oriStation.TrainService.Select(tSvc => (string)tSvc));
                }
                else
                    trainServiceList = new List<string>(new string[] {""});
                // ---------------------------------------------------------------
                // Get Stations related to trainServiceList except Origin station
                List<StationDetailsModel> filterStationList = new List<StationDetailsModel>();
                foreach (string serv in trainServiceList)
                {

                    StationDetailsModel[] temList 
                        = (from stt in stationEnt.Data
                            where ((stt.Id.Equals(origStationId) == false) && stt.TrainService.Contains(serv))
                            select stt).ToArray();

                    if (filterStationList.Count == 0)
                        filterStationList.AddRange(temList.ToArray());
                    else
                        foreach (StationDetailsModel stt in temList)
                            if (filterStationList.Find(s => (s.Id.Equals(stt.Id))) == null)
                                filterStationList.Add(stt);

                }
                // ---------------------------------------------------------------
                return filterStationList.ToArray();
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
