using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Network;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI;
using NssIT.Kiosk.AppDecorator.UI;
using NssIT.Kiosk.Client.Base;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static NssIT.Kiosk.Client.Base.NetServiceAnswerMan;

namespace NssIT.Kiosk.Client.NetClient
{
	/// <summary>
	/// ClassCode:EXIT80.04
	/// </summary>
	public class NetClientSalesService
	{
		public const int WebServerTimeout = 9999999;
		public const int InvalidAuthentication = 9999998;

		private string _logChannel = "NetClientService";

		private NssIT.Kiosk.Log.DB.DbLog _log = null;
		private INetMediaInterface _netInterface;

		private UIServerApplicationStatusAck _serverApplicationStatus = null;
		private UIWebServerLogonStatusAck _webServerLogonStatus = null;

		private ReceivedNetProcessIdTracker _recvedNetProcIdTracker = new ReceivedNetProcessIdTracker();

		public event EventHandler<DataReceivedEventArgs> OnDataReceived;

		public NetClientSalesService(INetMediaInterface netInterface)
		{
			_netInterface = netInterface;

			_log = NssIT.Kiosk.Log.DB.DbLog.GetDbLog();

			if (_netInterface != null)
				_netInterface.OnDataReceived += _netInterface_OnDataReceived;
		}

		private int GetServerPort() => App.SysParam.PrmLocalServerPort;

		public void QuerySalesServerStatus(out bool isServerResponded, out bool serverAppHasDisposed, out bool serverAppHasShutdown, 
			out UIServerApplicationStatusAck serverState, 
			int waitDelaySec = 60)
		{
			serverAppHasDisposed = false;
			serverAppHasShutdown = false;
			serverState = null;
			isServerResponded = false;

			_serverApplicationStatus = null;
			Guid lastNetProcessId;

			waitDelaySec = (waitDelaySec < 0) ? 20 : waitDelaySec;

			_log.LogText(_logChannel, "-", "Start - QuerySalesServerStatus", "A01", "NetClientSalesService.QuerySalesServerStatus");

			UIServerApplicationStatusRequest res = new UIServerApplicationStatusRequest("-", DateTime.Now);

			NetMessagePack msgPack = new NetMessagePack(res) { DestinationPort = GetServerPort() };
			lastNetProcessId = msgPack.NetProcessId;

			_log.LogText(_logChannel, "-",
				msgPack, "A05", "NetClientSalesService.QuerySalesServerStatus", extraMsg: "MsgObject: NetMessagePack");

			_netInterface.SendMsgPack(msgPack);

			DateTime startTime = DateTime.Now;
			DateTime endTime = startTime.AddSeconds(waitDelaySec);

			while (endTime.Subtract(DateTime.Now).TotalSeconds > 0)
			{
				if ((_serverApplicationStatus == null) && (_recvedNetProcIdTracker.CheckReceivedResponded(lastNetProcessId) == false))
					Task.Delay(100).Wait();
				else
					break;
			}

			bool alreadyExpired = false;

			if (_serverApplicationStatus == null)
				alreadyExpired = _netInterface.SetExpiredNetProcessId(msgPack.NetProcessId);

			if (alreadyExpired)
			{
				_log.LogText(_logChannel, "-", $@"Already expired; (EXIT9000201) ", "A20",
					"NetClientSalesService.QuerySalesServerStatus");
			}
			else if (_serverApplicationStatus == null)
			{
				_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000202); Adnormal result !!;", "A21",
					"NetClientSalesService.QuerySalesServerStatus");
			}
			else
			{
				isServerResponded = true;
				serverAppHasDisposed = _serverApplicationStatus.ServerAppHasDisposed;
				serverAppHasShutdown = _serverApplicationStatus.ServerAppHasShutdown;
			}

			serverState = _serverApplicationStatus;

			_log.LogText(_logChannel, "-", _serverApplicationStatus, "A100", "NetClientSalesService.QuerySalesServerStatus", 
				extraMsg: $@"End - IsLocalServerResponded: {isServerResponded}; ServerAppHasDisposed: {serverAppHasDisposed}; ServerAppHasShutdown: {serverAppHasShutdown}; MsgObj: UIServerApplicationStatusAck");
		}

		public void WebServerLogon(out bool isServerResponded, out bool isLogonSuccess, out bool isNetworkTimeout, out bool isValidAuthentication, out bool isLogonErrorFound, out string errorMessage, int waitDelaySec = 120)
		{
			isServerResponded = false;
			isLogonSuccess = false;
			isNetworkTimeout = true;
			isValidAuthentication = false;
			isLogonErrorFound = true;
			errorMessage = null;

			_webServerLogonStatus = null;

			Guid lastNetProcessId;

			waitDelaySec = (waitDelaySec < 0) ? 60 : waitDelaySec;

			_log.LogText(_logChannel, "-", "Start - WebServerLogon", "A01", "NetClientSalesService.WebServerLogon");

			UIWebServerLogonRequest res = new UIWebServerLogonRequest("-", DateTime.Now);

			NetMessagePack msgPack = new NetMessagePack(res) { DestinationPort = GetServerPort() };
			lastNetProcessId = msgPack.NetProcessId;

			_log.LogText(_logChannel, "-",
				msgPack, "A05", "NetClientSalesService.WebServerLogon", extraMsg: "MsgObject: NetMessagePack");

			_netInterface.SendMsgPack(msgPack);

			DateTime startTime = DateTime.Now;
			DateTime endTime = startTime.AddSeconds(waitDelaySec);

			while (endTime.Subtract(DateTime.Now).TotalSeconds > 0)
			{
				if (_webServerLogonStatus == null)
					Task.Delay(100).Wait();
				else
				{
					isServerResponded = true;
					break;
				}
			}

			bool alreadyExpired = false;

			if (isServerResponded == false)
				alreadyExpired = _netInterface.SetExpiredNetProcessId(msgPack.NetProcessId);

			if (alreadyExpired)
			{
				_log.LogText(_logChannel, "-", $@"Unable to read from Local Server", "A20",
					"NetClientSalesService.WebServerLogon");

				isLogonSuccess = false;
				isNetworkTimeout = false;
				isValidAuthentication = false;
				isLogonErrorFound = true;
				errorMessage = $@"Unable to read from Local Server; (EXIT9000001)";
			}
			else if (_webServerLogonStatus == null)
			{
				_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000002); Adnormal result !!;", "A21",
					"NetClientSalesService.WebServerLogon");

				isLogonSuccess = false;
				isNetworkTimeout = false;
				isValidAuthentication = false;
				isLogonErrorFound = true;
				errorMessage = $@"Unable to read from Local Server; (EXIT9000003)";
			}
			else
			{
				isLogonSuccess = _webServerLogonStatus.LogonSuccess;
				isNetworkTimeout = _webServerLogonStatus.NetworkTimeout;
				isValidAuthentication = _webServerLogonStatus.IsValidAuthentication;
				isLogonErrorFound = _webServerLogonStatus.LogonErrorFound;
			}

			_log.LogText(_logChannel, "-",
				$@"End - isLogonSuccess: {isLogonSuccess}; isNetworkTimeout: {isNetworkTimeout}; isValidAuthentication: {isValidAuthentication}; isLogonErrorFound: {isLogonErrorFound}; errorMessage: {errorMessage}",
				"A100",
				"NetClientSalesService.WebServerLogon");

		}

		public void RequestMaintenance(out bool isServerResponded, int waitDelaySec = 60)
		{
			isServerResponded = false;

			Guid lastNetProcessId;

			waitDelaySec = (waitDelaySec < 0) ? 20 : waitDelaySec;

			_log.LogText(_logChannel, "-", "Start - RequestMaintenance", "A01", "NetClientSalesService.RequestMaintenance");

			UISalesClientMaintenanceRequest req = new UISalesClientMaintenanceRequest("-", DateTime.Now);

			NetMessagePack msgPack = new NetMessagePack(req) { DestinationPort = GetServerPort() };
			lastNetProcessId = msgPack.NetProcessId;

			_log.LogText(_logChannel, "-",
				msgPack, "A05", "NetClientSalesService.RequestMaintenance", extraMsg: "MsgObject: NetMessagePack");

			_netInterface.SendMsgPack(msgPack);

			DateTime startTime = DateTime.Now;
			DateTime endTime = startTime.AddSeconds(waitDelaySec);

			while (endTime.Subtract(DateTime.Now).TotalSeconds > 0)
			{
				if (_recvedNetProcIdTracker.CheckReceivedResponded(lastNetProcessId) == false)
					Task.Delay(100).Wait();
				else
				{
					isServerResponded = true;
					break;
				}
			}

			bool alreadyExpired = false;

			if (isServerResponded == false)
				alreadyExpired = _netInterface.SetExpiredNetProcessId(msgPack.NetProcessId);

			if (alreadyExpired)
			{
				_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000012)", "A20",
					"NetClientSalesService.RequestMaintenance", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
			}
			else if (isServerResponded == false)
			{
				_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000013); Adnormal result !!;", "A21",
					"NetClientSalesService.RequestMaintenance", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
			}
			else
			{
				isServerResponded = true;
			}

			_log.LogText(_logChannel, "-",
				$@"End - IsLocalServerResponded: {isServerResponded};",
				"A100",
				"NetClientSalesService.SubmitTravelRequestMaintenanceDates");
		}

		public void SubmitFinishedMaintenance()
		{
			Guid lastNetProcessId;

			_log.LogText(_logChannel, "-", "Start - SubmitFinishedMaintenance", "A01", "NetClientSalesService.SubmitFinishedMaintenance");

			UISalesClientMaintenanceFinishedSubmission subm = new UISalesClientMaintenanceFinishedSubmission("-", DateTime.Now);

			NetMessagePack msgPack = new NetMessagePack(subm) { DestinationPort = GetServerPort() };
			lastNetProcessId = msgPack.NetProcessId;

			_log.LogText(_logChannel, "-",
				msgPack, "A05", "NetClientSalesService.SubmitFinishedMaintenance", extraMsg: "MsgObject: NetMessagePack");

			_netInterface.SendMsgPack(msgPack);

			_log.LogText(_logChannel, "-",
				$@"End - SubmitFinishedMaintenance",
				"A100",
				"NetClientSalesService.SubmitFinishedMaintenance");
		}

		public void RequestOutstandingCardSettlementStatus(out bool isServerResponded, int waitDelaySec = 60)
		{
			isServerResponded = false;

			Guid lastNetProcessId;

			waitDelaySec = (waitDelaySec < 0) ? 20 : waitDelaySec;

			_log.LogText(_logChannel, "-", "Start - RequestOutstandingCardSettlementStatus", "A01", "NetClientSalesService.RequestOutstandingCardSettlementStatus");

			UISalesCheckOutstandingCardSettlementRequest req = new UISalesCheckOutstandingCardSettlementRequest("-", DateTime.Now);

			NetMessagePack msgPack = new NetMessagePack(req) { DestinationPort = GetServerPort() };
			lastNetProcessId = msgPack.NetProcessId;

			_log.LogText(_logChannel, "-",
				msgPack, "A05", "NetClientSalesService.RequestOutstandingCardSettlementStatus", extraMsg: "MsgObject: NetMessagePack");

			_netInterface.SendMsgPack(msgPack);

			DateTime startTime = DateTime.Now;
			DateTime endTime = startTime.AddSeconds(waitDelaySec);

			while (endTime.Subtract(DateTime.Now).TotalSeconds > 0)
			{
				if (_recvedNetProcIdTracker.CheckReceivedResponded(lastNetProcessId) == false)
					Task.Delay(100).Wait();
				else
				{
					isServerResponded = true;
					break;
				}
			}

			bool alreadyExpired = false;

			if (isServerResponded == false)
				alreadyExpired = _netInterface.SetExpiredNetProcessId(msgPack.NetProcessId);

			if (alreadyExpired)
			{
				_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000090)", "A20",
					"NetClientSalesService.RequestOutstandingCardSettlementStatus", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
			}
			else if (isServerResponded == false)
			{
				_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000091); Adnormal result !!;", "A21",
					"NetClientSalesService.RequestOutstandingCardSettlementStatus", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
			}
			else
			{
				isServerResponded = true;
			}

			_log.LogText(_logChannel, "-",
				$@"End - IsLocalServerResponded: {isServerResponded};",
				"A100",
				"NetClientSalesService.RequestOutstandingCardSettlementStatus");
		}

		public void SubmitCardSettlement(string processId, string hostNo, string batchNumber, string batchCount, decimal batchCurrencyAmount,
			string statusCode, string machineId, string errorMessage,
			out bool isServerResponded, int waitDelaySec = 60)
		{
			isServerResponded = false;
			processId = string.IsNullOrWhiteSpace(processId) ? "*" : processId;

			Guid lastNetProcessId;

			waitDelaySec = (waitDelaySec < 0) ? 20 : waitDelaySec;

			_log.LogText(_logChannel, "-", "Start - SubmitCardSettlement", "A01", "NetClientSalesService.SubmitCardSettlement");

			UISalesCardSettlementSubmission req = new UISalesCardSettlementSubmission(processId, DateTime.Now, 
				hostNo, batchNumber, batchCount, batchCurrencyAmount, statusCode, machineId, errorMessage);

			NetMessagePack msgPack = new NetMessagePack(req) { DestinationPort = GetServerPort() };
			lastNetProcessId = msgPack.NetProcessId;

			_log.LogText(_logChannel, "-",
				msgPack, "A05", "NetClientSalesService.SubmitCardSettlement", extraMsg: "MsgObject: NetMessagePack");

			_netInterface.SendMsgPack(msgPack);

			DateTime startTime = DateTime.Now;
			DateTime endTime = startTime.AddSeconds(waitDelaySec);

			while (endTime.Subtract(DateTime.Now).TotalSeconds > 0)
			{
				if (_recvedNetProcIdTracker.CheckReceivedResponded(lastNetProcessId) == false)
					Task.Delay(100).Wait();
				else
				{
					isServerResponded = true;
					break;
				}
			}

			bool alreadyExpired = false;

			if (isServerResponded == false)
				alreadyExpired = _netInterface.SetExpiredNetProcessId(msgPack.NetProcessId);

			if (alreadyExpired)
			{
				_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000092)", "A20",
					"NetClientSalesService.SubmitCardSettlement", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
			}
			else if (isServerResponded == false)
			{
				_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000093); Adnormal result !!;", "A21",
					"NetClientSalesService.SubmitCardSettlement", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
			}
			else
			{
				isServerResponded = true;
			}

			_log.LogText(_logChannel, "-",
				$@"End - IsLocalServerResponded: {isServerResponded};",
				"A100",
				"NetClientSalesService.SubmitCardSettlement");
		}

		public void StartNewSessionCountDown(out bool isServerResponded, int waitDelaySec = 60, TransportGroup vehicleCategory = TransportGroup.EtsIntercity)
		{
			isServerResponded = false;

			Guid lastNetProcessId;

			waitDelaySec = (waitDelaySec < 0) ? 20 : waitDelaySec;

			_log.LogText(_logChannel, "-", "Start - StartNewSessionCountDown", "A01", "NetClientSalesService.StartNewSessionCountDown");

			UICountDownStartRequest res = new UICountDownStartRequest("-", DateTime.Now, 30, vehicleCategory);

			NetMessagePack msgPack = new NetMessagePack(res) { DestinationPort = GetServerPort() };
			lastNetProcessId = msgPack.NetProcessId;

			_log.LogText(_logChannel, "-",
				msgPack, "A05", "NetClientSalesService.StartNewSessionCountDown", extraMsg: "MsgObject: NetMessagePack");

			_netInterface.SendMsgPack(msgPack);

			DateTime startTime = DateTime.Now;
			DateTime endTime = startTime.AddSeconds(waitDelaySec);
			
			while (endTime.Subtract(DateTime.Now).TotalSeconds > 0)
			{
				if (_recvedNetProcIdTracker.CheckReceivedResponded(lastNetProcessId) == false)
					Task.Delay(100).Wait();
				else
				{
					isServerResponded = true;
					break;
				}
			}

			bool alreadyExpired = false;

			if (isServerResponded == false)
				alreadyExpired = _netInterface.SetExpiredNetProcessId(msgPack.NetProcessId);

			if (alreadyExpired)
			{
				_log.LogText(_logChannel, "-", $@"Unable to read from Local Server", "A20",
					"NetClientSalesService.StartNewSessionCountDown", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
			}
			else if (isServerResponded == false)
			{
				if (_recvedNetProcIdTracker.CheckReceivedResponded(lastNetProcessId) == false)
					_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000006); Adnormal result !!;", "A21",
						"NetClientSalesService.StartNewSessionCountDown", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
				else
					isServerResponded = true;
			}
			else
			{
				isServerResponded = true;
			}

			_log.LogText(_logChannel, "-",
				$@"End - IsLocalServerResponded: {isServerResponded};",
				"A100",
				"NetClientSalesService.StartNewSessionCountDown");
		}

		public void SubmitLanguage(LanguageCode language, out bool isServerResponded, int waitDelaySec = 60)
		{
			isServerResponded = false;

			Guid lastNetProcessId;

			waitDelaySec = (waitDelaySec < 0) ? 20 : waitDelaySec;

			_log.LogText(_logChannel, "-", "Start - SubmitLanguage", "A01", "NetClientSalesService.StartSales");

			UILanguageSubmission res = new UILanguageSubmission("-", DateTime.Now, language);

			NetMessagePack msgPack = new NetMessagePack(res) { DestinationPort = GetServerPort() };
			lastNetProcessId = msgPack.NetProcessId;

			_log.LogText(_logChannel, "-",
				msgPack, "A05", "NetClientSalesService.SubmitLanguage", extraMsg: "MsgObject: NetMessagePack");

			_netInterface.SendMsgPack(msgPack);

			DateTime startTime = DateTime.Now;
			DateTime endTime = startTime.AddSeconds(waitDelaySec);
			
			while (endTime.Subtract(DateTime.Now).TotalSeconds > 0)
			{
				if (_recvedNetProcIdTracker.CheckReceivedResponded(lastNetProcessId) == false)
					Task.Delay(100).Wait();
				else
				{
					isServerResponded = true;
					break;
				}
			}

			bool alreadyExpired = false;

			if (isServerResponded == false)
				alreadyExpired = _netInterface.SetExpiredNetProcessId(msgPack.NetProcessId);

			if (alreadyExpired)
			{
				_log.LogText(_logChannel, "-", $@"Unable to read from Local Server", "A20",
					"NetClientSalesService.SubmitLanguage", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
			}
			else if (isServerResponded == false)
			{
				_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000011); Adnormal result !!;", "A21",
					"NetClientSalesService.SubmitLanguage", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
			}
			else
			{
				isServerResponded = true;
			}

			_log.LogText(_logChannel, "-",
				$@"End - IsLocalServerResponded: {isServerResponded};",
				"A100",
				"NetClientSalesService.SubmitLanguage");
		}

		public void SubmitDestination(string destinationCode, string destinationName, IList<string> trainService, out bool isServerResponded, int waitDelaySec = 60)
		{
			isServerResponded = false;

			Guid lastNetProcessId;

			waitDelaySec = (waitDelaySec < 0) ? 20 : waitDelaySec;

			_log.LogText(_logChannel, "-", "Start - SubmitDestination", "A01", "NetClientSalesService.SubmitDestination");

			UIDestinationSubmission res = new UIDestinationSubmission("-", DateTime.Now, destinationCode, destinationName, trainService);

			NetMessagePack msgPack = new NetMessagePack(res) { DestinationPort = GetServerPort() };
			lastNetProcessId = msgPack.NetProcessId;

			_log.LogText(_logChannel, "-",
				msgPack, "A05", "NetClientSalesService.SubmitDestination", extraMsg: "MsgObject: NetMessagePack");

			_netInterface.SendMsgPack(msgPack);

			DateTime startTime = DateTime.Now;
			DateTime endTime = startTime.AddSeconds(waitDelaySec);

			while (endTime.Subtract(DateTime.Now).TotalSeconds > 0)
			{
				if (_recvedNetProcIdTracker.CheckReceivedResponded(lastNetProcessId) == false)
					Task.Delay(100).Wait();
				else
				{
					isServerResponded = true;
					break;
				}
			}

			bool alreadyExpired = false;

			if (isServerResponded == false)
				alreadyExpired = _netInterface.SetExpiredNetProcessId(msgPack.NetProcessId);

			if (alreadyExpired)
			{
				_log.LogText(_logChannel, "-", $@"Already expired; (EXIT9000016)", "A20",
					"NetClientSalesService.SubmitDestination", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
			}
			else if (isServerResponded == false)
			{
				_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000017); Adnormal result !!;", "A21",
					"NetClientSalesService.SubmitDestination", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
			}
			else
			{
				isServerResponded = true;
			}

			_log.LogText(_logChannel, "-",
				$@"End - IsLocalServerResponded: {isServerResponded};",
				"A100",
				"NetClientSalesService.SubmitDestination");
		}

		public void SubmitOriginStation(string destinationCode, string destinationName, IList<string> trainService, out bool isServerResponded, int waitDelaySec = 60)
		{
			isServerResponded = false;

			Guid lastNetProcessId;

			waitDelaySec = (waitDelaySec < 0) ? 20 : waitDelaySec;

			_log.LogText(_logChannel, "-", "Start - SubmitOriginStation", "A01", "NetClientSalesService.SubmitOriginStation");

			UIOriginSubmission res = new UIOriginSubmission("-", DateTime.Now, destinationCode, destinationName, trainService);

			NetMessagePack msgPack = new NetMessagePack(res) { DestinationPort = GetServerPort() };
			lastNetProcessId = msgPack.NetProcessId;

			_log.LogText(_logChannel, "-",
				msgPack, "A05", "NetClientSalesService.SubmitOriginStation", extraMsg: "MsgObject: NetMessagePack");

			_netInterface.SendMsgPack(msgPack);

			DateTime startTime = DateTime.Now;
			DateTime endTime = startTime.AddSeconds(waitDelaySec);

			while (endTime.Subtract(DateTime.Now).TotalSeconds > 0)
			{
				if (_recvedNetProcIdTracker.CheckReceivedResponded(lastNetProcessId) == false)
					Task.Delay(100).Wait();
				else
				{
					isServerResponded = true;
					break;
				}
			}

			bool alreadyExpired = false;

			if (isServerResponded == false)
				alreadyExpired = _netInterface.SetExpiredNetProcessId(msgPack.NetProcessId);

			if (alreadyExpired)
			{
				_log.LogText(_logChannel, "-", $@"Already expired; (EXIT9000004)", "A20",
					"NetClientSalesService.SubmitOriginStation", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
			}
			else if (isServerResponded == false)
			{
				_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000005); Adnormal result !!;", "A21",
					"NetClientSalesService.SubmitOriginStation", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
			}
			else
			{
				isServerResponded = true;
			}

			_log.LogText(_logChannel, "-",
				$@"End - IsLocalServerResponded: {isServerResponded};",
				"A100",
				"NetClientSalesService.SubmitOriginStation");
		}

		public void SubmitTravelDates(DateTime? departDate, DateTime? returnDate, int numberOfPassenger, out bool isServerResponded, int waitDelaySec = 60)
		{
			isServerResponded = false;

			Guid lastNetProcessId;

			waitDelaySec = (waitDelaySec < 0) ? 20 : waitDelaySec;

			_log.LogText(_logChannel, "-", "Start - SubmitTravelDates", "A01", "NetClientSalesService.SubmitTravelDates");

			UITravelDateSubmission res = new UITravelDateSubmission("-", DateTime.Now, departDate, returnDate, numberOfPassenger);

			NetMessagePack msgPack = new NetMessagePack(res) { DestinationPort = GetServerPort() };
			lastNetProcessId = msgPack.NetProcessId;

			_log.LogText(_logChannel, "-",
				msgPack, "A05", "NetClientSalesService.SubmitTravelDates", extraMsg: "MsgObject: NetMessagePack");

			_netInterface.SendMsgPack(msgPack);

			DateTime startTime = DateTime.Now;
			DateTime endTime = startTime.AddSeconds(waitDelaySec);

			while (endTime.Subtract(DateTime.Now).TotalSeconds > 0)
			{
				if (_recvedNetProcIdTracker.CheckReceivedResponded(lastNetProcessId) == false)
					Task.Delay(100).Wait();
				else
				{
					isServerResponded = true;
					break;
				}
			}

			bool alreadyExpired = false;

			if (isServerResponded == false)
				alreadyExpired = _netInterface.SetExpiredNetProcessId(msgPack.NetProcessId);

			if (alreadyExpired)
			{
				_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000021)", "A20",
					"NetClientSalesService.SubmitTravelDates", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
			}
			else if (isServerResponded == false)
			{
				_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000022); Adnormal result !!;", "A21",
					"NetClientSalesService.SubmitTravelDates", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
			}
			else
			{
				isServerResponded = true;
			}

			_log.LogText(_logChannel, "-",
				$@"End - IsLocalServerResponded: {isServerResponded};",
				"A100",
				"NetClientSalesService.SubmitTravelDates");
		}

		public void QueryDepartTripList(DateTime passengerDepartDate, string fromStationCode, string toStationCode, out bool isServerResponded, int waitDelaySec = 60)
		{
			isServerResponded = false;
			Guid? netProcessId = null;

			try
			{
				waitDelaySec = (waitDelaySec < 0) ? 20 : waitDelaySec;

				_log.LogText(_logChannel, "-", "Start - QueryDepartTripList", "A01", "NetClientSalesService.QueryDepartTripList");

				UIDepartTripListRequest res = new UIDepartTripListRequest("-", DateTime.Now, passengerDepartDate, fromStationCode, toStationCode);

				NetMessagePack msgPack = new NetMessagePack(res) { DestinationPort = GetServerPort() };
				netProcessId = msgPack.NetProcessId;

				_log.LogText(_logChannel, "-",
					msgPack, "A05", "NetClientSalesService.QueryDepartTripList", extraMsg: "MsgObject: NetMessagePack");

				_netInterface.SendMsgPack(msgPack);

				DateTime startTime = DateTime.Now;
				DateTime endTime = startTime.AddSeconds(waitDelaySec);

				while (endTime.Subtract(DateTime.Now).TotalSeconds > 0)
				{
					if (_recvedNetProcIdTracker.CheckReceivedResponded(netProcessId.Value) == false)
						Task.Delay(100).Wait();
					else
					{
						isServerResponded = true;
						break;
					}
				}

				bool alreadyExpired = false;

				if (isServerResponded == false)
					alreadyExpired = _netInterface.SetExpiredNetProcessId(msgPack.NetProcessId);

				if (alreadyExpired)
				{
					_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000026)", "A20",
						"NetClientSalesService.QueryDepartTripList", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
				}
				else if (isServerResponded == false)
				{
					_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000027); Adnormal result !!;", "A21",
						"NetClientSalesService.QueryDepartTripList", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
				}
				else
				{
					isServerResponded = true;
				}

				_log.LogText(_logChannel, "-",
					$@"End - IsLocalServerResponded: {isServerResponded};",
					"A100",
					"NetClientSalesService.QueryDepartTripList");
			}
			catch (ThreadAbortException)
			{
				try
				{
					_log.LogText(_logChannel, "-",
					$@"ThreadAbortException",
					"A101",
					"NetClientSalesService.QueryDepartTripList");
				}
				catch { }

				if (netProcessId.HasValue)
				{
					try
					{
						_netInterface.SetExpiredNetProcessId(netProcessId.Value);
					}
					catch { }
				}
			}
		}

		public void SubmitDepartTrip(
			string tripId,
			DateTime passengerDepartDateTime,
			DateTime passengerArrivalDateTime,
			string passengerDepartTimeStr,
			int passengerArrivalDayOffset,
			string passengerArrivalTimeStr,
			string vehicleService,
			string vehicleNo,
			string serviceCategory,
			string currency,
			decimal price,
			out bool isServerResponded, int waitDelaySec = 60)
		{
			isServerResponded = false;

			Guid lastNetProcessId;

			waitDelaySec = (waitDelaySec < 0) ? 20 : waitDelaySec;

			_log.LogText(_logChannel, "-", "Start - SubmitDepartTrip", "A01", "NetClientSalesService.SubmitDepartTrip");

			UIDepartTripSubmission res = new UIDepartTripSubmission("-", DateTime.Now, 
				tripId,
				passengerDepartDateTime, 
				passengerArrivalDateTime,
				passengerDepartTimeStr,
				passengerArrivalDayOffset,
				passengerArrivalTimeStr,
				vehicleService,
				vehicleNo,
				serviceCategory,
				currency,
				price);

			NetMessagePack msgPack = new NetMessagePack(res) { DestinationPort = GetServerPort() };
			lastNetProcessId = msgPack.NetProcessId;

			_log.LogText(_logChannel, "-",
				msgPack, "A05", "NetClientSalesService.SubmitDepartTrip", extraMsg: "MsgObject: NetMessagePack");

			_netInterface.SendMsgPack(msgPack);

			DateTime startTime = DateTime.Now;
			DateTime endTime = startTime.AddSeconds(waitDelaySec);

			while (endTime.Subtract(DateTime.Now).TotalSeconds > 0)
			{
				if (_recvedNetProcIdTracker.CheckReceivedResponded(lastNetProcessId) == false)
					Task.Delay(100).Wait();
				else
				{
					isServerResponded = true;
					break;
				}
			}

			bool alreadyExpired = false;

			if (isServerResponded == false)
				alreadyExpired = _netInterface.SetExpiredNetProcessId(msgPack.NetProcessId);

			if (alreadyExpired)
			{
				_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000031)", "A20",
					"NetClientSalesService.SubmitDepartTrip", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
			}
			else if (isServerResponded == false)
			{
				_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000032); Adnormal result !!;", "A21",
					"NetClientSalesService.SubmitDepartTrip", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
			}
			else
			{
				isServerResponded = true;
			}

			_log.LogText(_logChannel, "-",
				$@"End - IsLocalServerResponded: {isServerResponded};",
				"A100",
				"NetClientSalesService.SubmitDepartTrip");
		}

		public void SubmitDepartSeatList(CustSeatDetail[] custSeatDetailList, string trainSeatModelId, 
			out bool isServerResponded, int waitDelaySec = 60)
		{
			isServerResponded = false;

			Guid lastNetProcessId;

			waitDelaySec = (waitDelaySec < 0) ? 20 : waitDelaySec;

			_log.LogText(_logChannel, "-", "Start - SubmitDepartSeatList", "A01", "NetClientSalesService.SubmitDepartSeatList");

			UIDepartSeatSubmission res = new UIDepartSeatSubmission("-", DateTime.Now, 
				custSeatDetailList, trainSeatModelId);

			NetMessagePack msgPack = new NetMessagePack(res) { DestinationPort = GetServerPort() };
			lastNetProcessId = msgPack.NetProcessId;

			_log.LogText(_logChannel, "-",
				msgPack, "A05", "NetClientSalesService.SubmitDepartSeatList", extraMsg: "MsgObject: NetMessagePack");

			_netInterface.SendMsgPack(msgPack);

			DateTime startTime = DateTime.Now;
			DateTime endTime = startTime.AddSeconds(waitDelaySec);

			while (endTime.Subtract(DateTime.Now).TotalSeconds > 0)
			{
				if (_recvedNetProcIdTracker.CheckReceivedResponded(lastNetProcessId) == false)
					Task.Delay(100).Wait();
				else
				{
					isServerResponded = true;
					break;
				}
			}

			bool alreadyExpired = false;

			if (isServerResponded == false)
				alreadyExpired = _netInterface.SetExpiredNetProcessId(msgPack.NetProcessId);

			if (alreadyExpired)
			{
				_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000036)", "A20",
					"NetClientSalesService.SubmitDepartSeatList", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
			}
			else if (isServerResponded == false)
			{
				_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000037); Adnormal result !!;", "A21",
					"NetClientSalesService.SubmitDepartSeatList", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
			}
			else
			{
				isServerResponded = true;
			}

			_log.LogText(_logChannel, "-",
				$@"End - IsLocalServerResponded: {isServerResponded};",
				"A100",
				"NetClientSalesService.SubmitDepartSeatList");
		}

		public void QueryReturnTripList(DateTime passengerDepartDate, string fromStationCode, string toStationCode, out bool isServerResponded, int waitDelaySec = 60)
		{
			isServerResponded = false;
			Guid? netProcessId = null;

			try
			{
				waitDelaySec = (waitDelaySec < 0) ? 20 : waitDelaySec;

				_log.LogText(_logChannel, "-", "Start - QueryReturnTripList", "A01", "NetClientSalesService.QueryReturnTripList");

				UIReturnTripListRequest res = new UIReturnTripListRequest("-", DateTime.Now, passengerDepartDate, fromStationCode, toStationCode);

				NetMessagePack msgPack = new NetMessagePack(res) { DestinationPort = GetServerPort() };
				netProcessId = msgPack.NetProcessId;

				_log.LogText(_logChannel, "-",
					msgPack, "A05", "NetClientSalesService.QueryReturnTripList", extraMsg: "MsgObject: NetMessagePack");

				_netInterface.SendMsgPack(msgPack);

				DateTime startTime = DateTime.Now;
				DateTime endTime = startTime.AddSeconds(waitDelaySec);

				while (endTime.Subtract(DateTime.Now).TotalSeconds > 0)
				{
					if (_recvedNetProcIdTracker.CheckReceivedResponded(netProcessId.Value) == false)
						Task.Delay(100).Wait();
					else
					{
						isServerResponded = true;
						break;
					}
				}

				bool alreadyExpired = false;

				if (isServerResponded == false)
					alreadyExpired = _netInterface.SetExpiredNetProcessId(msgPack.NetProcessId);

				if (alreadyExpired)
				{
					_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000066)", "A20",
						"NetClientSalesService.QueryReturnTripList", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
				}
				else if (isServerResponded == false)
				{
					_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000067); Adnormal result !!;", "A21",
						"NetClientSalesService.QueryReturnTripList", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
				}
				else
				{
					isServerResponded = true;
				}

				_log.LogText(_logChannel, "-",
					$@"End - IsLocalServerResponded: {isServerResponded};",
					"A100",
					"NetClientSalesService.QueryReturnTripList");
			}
			catch (ThreadAbortException)
			{
				try
				{
					_log.LogText(_logChannel, "-",
					$@"ThreadAbortException",
					"A101",
					"NetClientSalesService.QueryReturnTripList");
				}
				catch { }

				if (netProcessId.HasValue)
				{
					try
					{
						_netInterface.SetExpiredNetProcessId(netProcessId.Value);
					}
					catch { }
				}
			}
		}

		public void SubmitReturnTrip(
			string tripId,
			DateTime passengerDepartDateTime,
			DateTime passengerArrivalDateTime,
			string passengerDepartTimeStr,
			int passengerArrivalDayOffset,
			string passengerArrivalTimeStr,
			string vehicleService,
			string vehicleNo,
			string serviceCategory,
			string currency,
			decimal price,
			out bool isServerResponded, int waitDelaySec = 60)
		{
			isServerResponded = false;

			Guid lastNetProcessId;

			waitDelaySec = (waitDelaySec < 0) ? 20 : waitDelaySec;

			_log.LogText(_logChannel, "-", "Start - SubmitReturnTrip", "A01", "NetClientSalesService.SubmitReturnTrip");

			UIReturnTripSubmission res = new UIReturnTripSubmission("-", DateTime.Now,
				tripId,
				passengerDepartDateTime,
				passengerArrivalDateTime,
				passengerDepartTimeStr,
				passengerArrivalDayOffset,
				passengerArrivalTimeStr,
				vehicleService,
				vehicleNo,
				serviceCategory,
				currency,
				price);

			NetMessagePack msgPack = new NetMessagePack(res) { DestinationPort = GetServerPort() };
			lastNetProcessId = msgPack.NetProcessId;

			_log.LogText(_logChannel, "-",
				msgPack, "A05", "NetClientSalesService.SubmitReturnTrip", extraMsg: "MsgObject: NetMessagePack");

			_netInterface.SendMsgPack(msgPack);

			DateTime startTime = DateTime.Now;
			DateTime endTime = startTime.AddSeconds(waitDelaySec);

			while (endTime.Subtract(DateTime.Now).TotalSeconds > 0)
			{
				if (_recvedNetProcIdTracker.CheckReceivedResponded(lastNetProcessId) == false)
					Task.Delay(100).Wait();
				else
				{
					isServerResponded = true;
					break;
				}
			}

			bool alreadyExpired = false;

			if (isServerResponded == false)
				alreadyExpired = _netInterface.SetExpiredNetProcessId(msgPack.NetProcessId);

			if (alreadyExpired)
			{
				_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000071)", "A20",
					"NetClientSalesService.SubmitReturnTrip", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
			}
			else if (isServerResponded == false)
			{
				_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000072); Adnormal result !!;", "A21",
					"NetClientSalesService.SubmitReturnTrip", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
			}
			else
			{
				isServerResponded = true;
			}

			_log.LogText(_logChannel, "-",
				$@"End - IsLocalServerResponded: {isServerResponded};",
				"A100",
				"NetClientSalesService.SubmitReturnTrip");
		}

		public void SubmitReturnSeatList(CustSeatDetail[] custSeatDetailList, string trainSeatModelId,
			out bool isServerResponded, int waitDelaySec = 60)
		{
			isServerResponded = false;

			Guid lastNetProcessId;

			waitDelaySec = (waitDelaySec < 0) ? 20 : waitDelaySec;

			_log.LogText(_logChannel, "-", "Start - SubmitReturnSeatList", "A01", "NetClientSalesService.SubmitReturnSeatList");

			UIReturnSeatSubmission res = new UIReturnSeatSubmission("-", DateTime.Now,
				custSeatDetailList, trainSeatModelId);

			NetMessagePack msgPack = new NetMessagePack(res) { DestinationPort = GetServerPort() };
			lastNetProcessId = msgPack.NetProcessId;

			_log.LogText(_logChannel, "-",
				msgPack, "A05", "NetClientSalesService.SubmitReturnSeatList", extraMsg: "MsgObject: NetMessagePack");

			_netInterface.SendMsgPack(msgPack);

			DateTime startTime = DateTime.Now;
			DateTime endTime = startTime.AddSeconds(waitDelaySec);

			while (endTime.Subtract(DateTime.Now).TotalSeconds > 0)
			{
				if (_recvedNetProcIdTracker.CheckReceivedResponded(lastNetProcessId) == false)
					Task.Delay(100).Wait();
				else
				{
					isServerResponded = true;
					break;
				}
			}

			bool alreadyExpired = false;

			if (isServerResponded == false)
				alreadyExpired = _netInterface.SetExpiredNetProcessId(msgPack.NetProcessId);

			if (alreadyExpired)
			{
				_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000074)", "A20",
					"NetClientSalesService.SubmitReturnSeatList", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
			}
			else if (isServerResponded == false)
			{
				_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000075); Adnormal result !!;", "A21",
					"NetClientSalesService.SubmitReturnSeatList", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
			}
			else
			{
				isServerResponded = true;
			}

			_log.LogText(_logChannel, "-",
				$@"End - IsLocalServerResponded: {isServerResponded};",
				"A100",
				"NetClientSalesService.SubmitReturnSeatList");
		}

		public void SubmitPickupNDrop(string departPick, string departPickDesn, string departPickTime, string departDrop, string departDropDesn,
			out bool isServerResponded, int waitDelaySec = 60)
		{
			isServerResponded = false;

			Guid lastNetProcessId;

			waitDelaySec = (waitDelaySec < 0) ? 20 : waitDelaySec;

			_log.LogText(_logChannel, "-", "Start - SubmitPickupNDrop", "A01", "NetClientSalesService.SubmitDestination");

			UIDepartPickupNDropSubmission res = new UIDepartPickupNDropSubmission("-", DateTime.Now,
				departPick, departPickDesn, departPickTime, departDrop, departDropDesn);

			NetMessagePack msgPack = new NetMessagePack(res) { DestinationPort = GetServerPort() };
			lastNetProcessId = msgPack.NetProcessId;

			_log.LogText(_logChannel, "-",
				msgPack, "A05", "NetClientSalesService.SubmitPickupNDrop", extraMsg: "MsgObject: NetMessagePack");

			_netInterface.SendMsgPack(msgPack);

			DateTime startTime = DateTime.Now;
			DateTime endTime = startTime.AddSeconds(waitDelaySec);

			while (endTime.Subtract(DateTime.Now).TotalSeconds > 0)
			{
				if (_recvedNetProcIdTracker.CheckReceivedResponded(lastNetProcessId) == false)
					Task.Delay(100).Wait();
				else
				{
					isServerResponded = true;
					break;
				}
			}

			bool alreadyExpired = false;

			if (isServerResponded == false)
				alreadyExpired = _netInterface.SetExpiredNetProcessId(msgPack.NetProcessId);

			if (alreadyExpired)
			{
				_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000041)", "A20",
					"NetClientSalesService.SubmitPickupNDrop", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
			}
			else if (isServerResponded == false)
			{
				_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000042); Adnormal result !!;", "A21",
					"NetClientSalesService.SubmitPickupNDrop", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
			}
			else
			{
				isServerResponded = true;
			}

			_log.LogText(_logChannel, "-",
				$@"End - IsLocalServerResponded: {isServerResponded};",
				"A100",
				"NetClientSalesService.SubmitPickupNDrop");
		}

		public void RequestPromoCodeVerification(
			string trainSeatModelId, Guid seatLayoutModelId, string ticketTypesId, string passengerIC, string promoCode,
			out bool isServerResponded, int waitDelaySec = 30)
		{
			isServerResponded = false;
			Guid? netProcessId = null;

			try
			{
				waitDelaySec = (waitDelaySec < 0) ? 20 : waitDelaySec;

				_log.LogText(_logChannel, "-", "Start - RequestPromoCodeVerification", "A01", "NetClientSalesService.RequestPromoCodeVerification");

				UICustPromoCodeVerifyRequest res = new UICustPromoCodeVerifyRequest("-", DateTime.Now,
					trainSeatModelId, seatLayoutModelId, ticketTypesId, passengerIC, promoCode);

				NetMessagePack msgPack = new NetMessagePack(res) { DestinationPort = GetServerPort() };
				netProcessId = msgPack.NetProcessId;

				_log.LogText(_logChannel, "-",
					msgPack, "A05", "NetClientSalesService.RequestPromoCodeVerification", extraMsg: "MsgObject: NetMessagePack");

				_netInterface.SendMsgPack(msgPack);

				DateTime startTime = DateTime.Now;
				DateTime endTime = startTime.AddSeconds(waitDelaySec);

				while (endTime.Subtract(DateTime.Now).TotalSeconds > 0)
				{
					if (_recvedNetProcIdTracker.CheckReceivedResponded(netProcessId.Value) == false)
						Task.Delay(100).Wait();
					else
					{
						isServerResponded = true;
						break;
					}
				}

				bool alreadyExpired = false;

				if (isServerResponded == false)
					alreadyExpired = _netInterface.SetExpiredNetProcessId(msgPack.NetProcessId);

				if (alreadyExpired)
				{
					_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000079)", "A20",
						"NetClientSalesService.RequestPromoCodeVerification", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
				}
				else if (isServerResponded == false)
				{
					_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000081); Adnormal result !!;", "A21",
						"NetClientSalesService.RequestPromoCodeVerification", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
				}
				else
				{
					isServerResponded = true;
				}

				_log.LogText(_logChannel, "-",
					$@"End - IsLocalServerResponded: {isServerResponded};",
					"A100",
					"NetClientSalesService.RequestPromoCodeVerification");
			}
			catch (ThreadAbortException)
			{
				try
				{
					_log.LogText(_logChannel, "-",
					$@"ThreadAbortException",
					"A101",
					"NetClientSalesService.RequestPromoCodeVerification");
				}
				catch { }

				if (netProcessId.HasValue)
				{
					try
					{
						_netInterface.SetExpiredNetProcessId(netProcessId.Value);
					}
					catch { }
				}
			}
		}

		public void RequestPNRTicketType(
			string bookingId, string passenggerId, Guid[] tripScheduleSeatLayoutDetails_Ids,
			out bool isServerResponded, int waitDelaySec = 30)
		{
			isServerResponded = false;
			Guid? netProcessId = null;

			try
			{
				waitDelaySec = (waitDelaySec < 0) ? 20 : waitDelaySec;

				_log.LogText(_logChannel, "-", "Start - RequestPNRTicketType", "A01", "NetClientSalesService.RequestPNRTicketType");

				UICustInfoPNRTicketTypeRequest res = new UICustInfoPNRTicketTypeRequest("-", DateTime.Now, bookingId, passenggerId, tripScheduleSeatLayoutDetails_Ids);

				NetMessagePack msgPack = new NetMessagePack(res) { DestinationPort = GetServerPort() };
				netProcessId = msgPack.NetProcessId;

				_log.LogText(_logChannel, "-",
					msgPack, "A05", "NetClientSalesService.RequestPNRTicketType", extraMsg: "MsgObject: NetMessagePack");

				_netInterface.SendMsgPack(msgPack);

				DateTime startTime = DateTime.Now;
				DateTime endTime = startTime.AddSeconds(waitDelaySec);

				while (endTime.Subtract(DateTime.Now).TotalSeconds > 0)
				{
					if (_recvedNetProcIdTracker.CheckReceivedResponded(netProcessId.Value) == false)
						Task.Delay(100).Wait();
					else
					{
						isServerResponded = true;
						break;
					}
				}

				bool alreadyExpired = false;

				if (isServerResponded == false)
					alreadyExpired = _netInterface.SetExpiredNetProcessId(msgPack.NetProcessId);

				if (alreadyExpired)
				{
					_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000015)", "A20",
						"NetClientSalesService.RequestPNRTicketType", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
				}
				else if (isServerResponded == false)
				{
					_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000018); Adnormal result !!;", "A21",
						"NetClientSalesService.RequestPNRTicketType", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
				}
				else
				{
					isServerResponded = true;
				}

				_log.LogText(_logChannel, "-",
					$@"End - IsLocalServerResponded: {isServerResponded};",
					"A100",
					"NetClientSalesService.RequestPNRTicketType");
			}
			catch (ThreadAbortException)
			{
				try
				{
					_log.LogText(_logChannel, "-",
					$@"ThreadAbortException",
					"A101",
					"NetClientSalesService.RequestPNRTicketType");
				}
				catch { }

				if (netProcessId.HasValue)
				{
					try
					{
						_netInterface.SetExpiredNetProcessId(netProcessId.Value);
					}
					catch { }
				}
			}
		}

		public void SubmitPassengerInfo(CustSeatDetail[] custSeatDetailList,  
			out bool isServerResponded, int waitDelaySec = 120)
		{
			isServerResponded = false;

			Guid lastNetProcessId;

			waitDelaySec = (waitDelaySec < 0) ? 20 : waitDelaySec;

			_log.LogText(_logChannel, "-", "Start - SubmitPassengerInfo", "A01", "NetClientSalesService.SubmitPassengerInfo");

			UICustInfoSubmission res = new UICustInfoSubmission("-", DateTime.Now, custSeatDetailList);

			NetMessagePack msgPack = new NetMessagePack(res) { DestinationPort = GetServerPort() };
			lastNetProcessId = msgPack.NetProcessId;

			_log.LogText(_logChannel, "-",
				msgPack, "A05", "NetClientSalesService.SubmitPassengerInfo", extraMsg: "MsgObject: NetMessagePack");

			_netInterface.SendMsgPack(msgPack);

			DateTime startTime = DateTime.Now;
			DateTime endTime = startTime.AddSeconds(waitDelaySec);

			while (endTime.Subtract(DateTime.Now).TotalSeconds > 0)
			{
				if (_recvedNetProcIdTracker.CheckReceivedResponded(lastNetProcessId) == false)
					Task.Delay(100).Wait();
				else
				{
					isServerResponded = true;
					break;
				}
			}

			bool alreadyExpired = false;

			if (isServerResponded == false)
				alreadyExpired = _netInterface.SetExpiredNetProcessId(msgPack.NetProcessId);

			if (alreadyExpired)
			{
				_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000036)", "A20",
					"NetClientSalesService.SubmitPassengerInfo", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
			}
			else if (isServerResponded == false)
			{
				_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000037); Adnormal result !!;", "A21",
					"NetClientSalesService.SubmitPassengerInfo", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
			}
			else
			{
				isServerResponded = true;
			}

			_log.LogText(_logChannel, "-",
				$@"End - IsLocalServerResponded: {isServerResponded};",
				"A100",
				"NetClientSalesService.SubmitPassengerInfo");
		}

		public void SubmitETSInsurance(string transactionNo, string insuranceHeadersId, bool isAgreeToBuyInsurance,
			out bool isServerResponded, int waitDelaySec = 120)
		{
			isServerResponded = false;

			Guid lastNetProcessId;

			waitDelaySec = (waitDelaySec < 0) ? 20 : waitDelaySec;

			_log.LogText(_logChannel, "-", "Start - SubmitETSInsurance", "A01", "NetClientSalesService.SubmitETSInsurance");

			UIETSInsuranceSubmissionRequest res = new UIETSInsuranceSubmissionRequest(transactionNo, DateTime.Now, transactionNo, insuranceHeadersId, isAgreeToBuyInsurance);

			NetMessagePack msgPack = new NetMessagePack(res) { DestinationPort = GetServerPort() };
			lastNetProcessId = msgPack.NetProcessId;

			_log.LogText(_logChannel, "-",
				msgPack, "A05", "NetClientSalesService.SubmitETSInsurance", extraMsg: "MsgObject: NetMessagePack");

			_netInterface.SendMsgPack(msgPack);

			DateTime startTime = DateTime.Now;
			DateTime endTime = startTime.AddSeconds(waitDelaySec);

			while (endTime.Subtract(DateTime.Now).TotalSeconds > 0)
			{
				if (_recvedNetProcIdTracker.CheckReceivedResponded(lastNetProcessId) == false)
					Task.Delay(100).Wait();
				else
				{
					isServerResponded = true;
					break;
				}
			}

			bool alreadyExpired = false;

			if (isServerResponded == false)
				alreadyExpired = _netInterface.SetExpiredNetProcessId(msgPack.NetProcessId);

			if (alreadyExpired)
			{
				_log.LogText(_logChannel, transactionNo, $@"Unable to read from Local Server; (EXIT9000023)", "A20",
					"NetClientSalesService.SubmitETSInsurance", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
			}
			else if (isServerResponded == false)
			{
				_log.LogText(_logChannel, transactionNo, $@"Unable to read from Local Server; (EXIT9000024); Adnormal result !!;", "A21",
					"NetClientSalesService.SubmitETSInsurance", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
			}
			else
			{
				isServerResponded = true;
			}

			_log.LogText(_logChannel, transactionNo,
				$@"End - IsLocalServerResponded: {isServerResponded};",
				"A100",
				"NetClientSalesService.SubmitETSInsurance");
		}

		public void SubmitSalesPayment(string seatBookingId, decimal totalAmount, string tradeCurrency, string bankReferenceNo, CreditCardResponse creditCardAnswer,
			out bool isServerResponded, int waitDelaySec = 120)
		{
			isServerResponded = false;

			Guid lastNetProcessId;

			waitDelaySec = (waitDelaySec < 0) ? 20 : waitDelaySec;

			_log.LogText(_logChannel, "-", "Start - SubmitSalesPayment", "A01", "NetClientSalesService.SubmitSalesPayment");

			UISalesPaymentSubmission res = new UISalesPaymentSubmission
				("-", DateTime.Now, seatBookingId, totalAmount, tradeCurrency, bankReferenceNo, creditCardAnswer);

			NetMessagePack msgPack = new NetMessagePack(res) { DestinationPort = GetServerPort() };
			lastNetProcessId = msgPack.NetProcessId;

			_log.LogText(_logChannel, "-",
				msgPack, "A05", "NetClientSalesService.SubmitSalesPayment", extraMsg: "MsgObject: NetMessagePack");

			_netInterface.SendMsgPack(msgPack);

			DateTime startTime = DateTime.Now;
			DateTime endTime = startTime.AddSeconds(waitDelaySec);

			while (endTime.Subtract(DateTime.Now).TotalSeconds > 0)
			{
				if (_recvedNetProcIdTracker.CheckReceivedResponded(lastNetProcessId) == false)
					Task.Delay(100).Wait();
				else
				{
					isServerResponded = true;
					break;
				}
			}

			bool alreadyExpired = false;

			if (isServerResponded == false)
				alreadyExpired = _netInterface.SetExpiredNetProcessId(msgPack.NetProcessId);

			if (alreadyExpired)
			{
				_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000056)", "A20",
					"NetClientSalesService.SubmitSalesPayment", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
			}
			else if (isServerResponded == false)
			{
				_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000057); Adnormal result !!;", "A21",
					"NetClientSalesService.SubmitSalesPayment", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
			}
			else
			{
				isServerResponded = true;
			}

			_log.LogText(_logChannel, "-",
				$@"End - IsLocalServerResponded: {isServerResponded};",
				"A100",
				"NetClientSalesService.SubmitSalesPayment");
		}

		public void SubmitSalesPayment(string seatBookingId, decimal totalAmount, string tradeCurrency, string bTnGSaleTransactionNo, string paymentMethod,
			out bool isServerResponded, int waitDelaySec = 120)
		{
			isServerResponded = false;

			Guid lastNetProcessId;

			waitDelaySec = (waitDelaySec < 0) ? 20 : waitDelaySec;

			_log.LogText(_logChannel, "-", "Start - SubmitSalesPayment", "A01", "NetClientSalesService.SubmitSalesPayment");

			UISalesPaymentSubmission res = new UISalesPaymentSubmission
				("-", DateTime.Now, seatBookingId, totalAmount, tradeCurrency, bTnGSaleTransactionNo, paymentMethod);

			NetMessagePack msgPack = new NetMessagePack(res) { DestinationPort = GetServerPort() };
			lastNetProcessId = msgPack.NetProcessId;

			_log.LogText(_logChannel, "-",
				msgPack, "A05", "NetClientSalesService.SubmitSalesPayment", extraMsg: "MsgObject: NetMessagePack");

			_netInterface.SendMsgPack(msgPack);

			DateTime startTime = DateTime.Now;
			DateTime endTime = startTime.AddSeconds(waitDelaySec);

			while (endTime.Subtract(DateTime.Now).TotalSeconds > 0)
			{
				if (_recvedNetProcIdTracker.CheckReceivedResponded(lastNetProcessId) == false)
					Task.Delay(100).Wait();
				else
				{
					isServerResponded = true;
					break;
				}
			}

			bool alreadyExpired = false;

			if (isServerResponded == false)
				alreadyExpired = _netInterface.SetExpiredNetProcessId(msgPack.NetProcessId);

			if (alreadyExpired)
			{
				_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000056)", "A20",
					"NetClientSalesService.SubmitSalesPayment", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
			}
			else if (isServerResponded == false)
			{
				_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000057); Adnormal result !!;", "A21",
					"NetClientSalesService.SubmitSalesPayment", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
			}
			else
			{
				isServerResponded = true;
			}

			_log.LogText(_logChannel, "-",
				$@"End - IsLocalServerResponded: {isServerResponded};",
				"A100",
				"NetClientSalesService.SubmitSalesPayment");
		}

		public void RequestSeatRelease(string transactionNo)
		{
			//isServerResponded = false;

			Guid lastNetProcessId;

			//waitDelaySec = (waitDelaySec < 0) ? 20 : waitDelaySec;

			_log.LogText(_logChannel, "-", "Start - SubmitSalesPayment", "A01", "NetClientSalesService.SubmitSalesPayment");

			UISeatReleaseRequest res = new UISeatReleaseRequest("-", DateTime.Now, transactionNo);

			NetMessagePack msgPack = new NetMessagePack(res) { DestinationPort = GetServerPort() };
			lastNetProcessId = msgPack.NetProcessId;

			_log.LogText(_logChannel, "-",
				msgPack, "A05", "NetClientSalesService.SubmitSalesPayment", extraMsg: "MsgObject: NetMessagePack");

			_netInterface.SendMsgPack(msgPack);
		}

		public NetServiceAnswerMan QueryKomuterTicketTypePackage(string fromStationCode, string fromStationName, string toStationCode, string toStationName, string noResponseErrorMessage, 
			FailLocalServerResponseCallBackDelg failLocalServerResponseCallBackDelgHandle,
			int waitDelaySec = 60)
		{
			Guid netProcessId = Guid.Empty;
			NetServiceAnswerMan retMan = null;
			string runningTag = "Query-Komuter-Ticket-Package-Type-(OPR0001)";

			try
			{
				_log.LogText(_logChannel, "-", $@"Start - {runningTag}", "A01", "NetClientSalesService.QueryKomuterTicketTypePackage");

				UIKomuterTicketTypePackageRequest res = new UIKomuterTicketTypePackageRequest("-", DateTime.Now, fromStationCode, fromStationName, toStationCode, toStationName);
				NetMessagePack msgPack = new NetMessagePack(res) { DestinationPort = GetServerPort() };

				retMan = new NetServiceAnswerMan(msgPack, runningTag, 
					noResponseErrorMessage, _logChannel, failLocalServerResponseCallBackDelgHandle,
					_netInterface, _recvedNetProcIdTracker, processId: "*", waitDelaySec: waitDelaySec, threadPriority: ThreadPriority.AboveNormal);

				return retMan;
			}
			catch (Exception ex)
			{
				throw new Exception($@"Error when {runningTag}; (EXIT.OPR0001)", ex);
			}
		}

		public void EndUserSession()
		{
			_log.LogText(_logChannel, "-", "Start - StartSales", "A01", "NetClientSalesService.ResetKomuterUserSession");

			UISalesEndSessionRequest res = new UISalesEndSessionRequest("-", DateTime.Now);
			NetMessagePack msgPack = new NetMessagePack(res) { DestinationPort = GetServerPort() };

			_log.LogText(_logChannel, "-", msgPack, "A05", "NetClientSalesService.ResetKomuterUserSession",
				extraMsg: "MsgObject: NetMessagePack");

			_netInterface.SendMsgPack(msgPack);
		}

		public void ResetKomuterUserSession()
		{
			_log.LogText(_logChannel, "-", "Start - StartSales", "A01", "NetClientSalesService.ResetKomuterUserSession");

			UIKomuterResetUserSessionRequest res = new UIKomuterResetUserSessionRequest("-", DateTime.Now);
			NetMessagePack msgPack = new NetMessagePack(res) { DestinationPort = GetServerPort() };

			_log.LogText(_logChannel, "-", msgPack, "A05", "NetClientSalesService.ResetKomuterUserSession",
				extraMsg: "MsgObject: NetMessagePack");

			_netInterface.SendMsgPack(msgPack);
		}

		public NetServiceAnswerMan RequestKomuterTicketBooking(
			string originStationId,
			string originStationName,
			string destinationStationId,
			string destinationStationName,
			string komuterPackageId,
			TicketItem[] ticketItemList,
			string noResponseErrorMessage,
			FailLocalServerResponseCallBackDelg failLocalServerResponseCallBackDelgHandle,
			int waitDelaySec = 60)
		{
			Guid netProcessId = Guid.Empty;
			NetServiceAnswerMan retMan = null;
			string runningTag = "Request-Komuter-Ticket-Booking-(OPR0001)";

			try
			{
				_log.LogText(_logChannel, "-", $@"Start - {runningTag}", "A01", "NetClientSalesService.RequestKomuterTicketBooking");

				UIKomuterTicketBookingRequest res = new UIKomuterTicketBookingRequest("-", DateTime.Now,
					originStationId, originStationName, destinationStationId, destinationStationName, komuterPackageId, ticketItemList);
				NetMessagePack msgPack = new NetMessagePack(res) { DestinationPort = GetServerPort() };

				retMan = new NetServiceAnswerMan(msgPack, runningTag,
					noResponseErrorMessage, _logChannel, failLocalServerResponseCallBackDelgHandle,
					_netInterface, _recvedNetProcIdTracker, processId: "*", waitDelaySec: waitDelaySec, threadPriority: ThreadPriority.AboveNormal);

				return retMan;
			}
			catch (Exception ex)
			{
				throw new Exception($@"Error when {runningTag}; (EXIT.OPR0001)", ex);
			}
		}

		public void RequestKomuterBookingCheckout(string processId,
			string bookingId,
			decimal totalAmount,
			string financePaymentMethod,
			out bool isServerResponded, int waitDelaySec = 60)
		{
			isServerResponded = false;
			Guid? netProcessId = null;

			try
			{
				waitDelaySec = (waitDelaySec < 0) ? 20 : waitDelaySec;

				_log.LogText(_logChannel, "-", "Start - RequestKomuterBookingCheckout", "A01", "NetClientSalesService.RequestKomuterBookingCheckout");

				UIKomuterBookingCheckoutRequest res = new UIKomuterBookingCheckoutRequest(processId, DateTime.Now,
					bookingId, totalAmount, financePaymentMethod);

				NetMessagePack msgPack = new NetMessagePack(res) { DestinationPort = GetServerPort() };
				netProcessId = msgPack.NetProcessId;

				_log.LogText(_logChannel, processId,
					msgPack, "A05", "NetClientSalesService.RequestKomuterBookingCheckout", extraMsg: "MsgObject: NetMessagePack");

				_netInterface.SendMsgPack(msgPack);

				DateTime startTime = DateTime.Now;
				DateTime endTime = startTime.AddSeconds(waitDelaySec);

				while (endTime.Subtract(DateTime.Now).TotalSeconds > 0)
				{
					if (_recvedNetProcIdTracker.CheckReceivedResponded(netProcessId.Value) == false)
						Task.Delay(100).Wait();
					else
					{
						isServerResponded = true;
						break;
					}
				}

				bool alreadyExpired = false;

				if (isServerResponded == false)
					alreadyExpired = _netInterface.SetExpiredNetProcessId(msgPack.NetProcessId);

				if (alreadyExpired)
				{
					_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000083)", "A20",
						"NetClientSalesService.RequestKomuterBookingCheckout", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
				}
				else if (isServerResponded == false)
				{
					_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000085); Adnormal result !!;", "A21",
						"NetClientSalesService.RequestKomuterBookingCheckout", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
				}
				else
				{
					isServerResponded = true;
				}

				_log.LogText(_logChannel, "-",
					$@"End - IsLocalServerResponded: {isServerResponded};",
					"A100",
					"NetClientSalesService.RequestKomuterBookingCheckout");
			}
			catch (ThreadAbortException)
			{
				try
				{
					_log.LogText(_logChannel, "-",
					$@"ThreadAbortException",
					"A101",
					"NetClientSalesService.RequestKomuterBookingCheckout");
				}
				catch { }

				if (netProcessId.HasValue)
				{
					try
					{
						_netInterface.SetExpiredNetProcessId(netProcessId.Value);
					}
					catch { }
				}
			}
		}

		public void SubmitKomuterBookingPayment(string processId,
			string bookingId,
			string currencyId,
			decimal amount,
			string financePaymentMethod, CreditCardResponse creditCardAnswer,
			out bool isServerResponded, int waitDelaySec = 60)
		{
			isServerResponded = false;
			Guid? netProcessId = null;

			try
			{
				waitDelaySec = (waitDelaySec < 0) ? 20 : waitDelaySec;

				_log.LogText(_logChannel, processId, "Start - SubmitKomuterBookingPayment", "A01", "NetClientSalesService.SubmitKomuterBookingPayment");

				UIKomuterCompletePaymentSubmission res = new UIKomuterCompletePaymentSubmission(processId, DateTime.Now,
					bookingId, currencyId, amount, financePaymentMethod, creditCardAnswer);

				NetMessagePack msgPack = new NetMessagePack(res) { DestinationPort = GetServerPort() };
				netProcessId = msgPack.NetProcessId;

				_log.LogText(_logChannel, processId,
					msgPack, "A05", "NetClientSalesService.SubmitKomuterBookingPayment", extraMsg: "MsgObject: NetMessagePack");

				_netInterface.SendMsgPack(msgPack);

				DateTime startTime = DateTime.Now;
				DateTime endTime = startTime.AddSeconds(waitDelaySec);

				while (endTime.Subtract(DateTime.Now).TotalSeconds > 0)
				{
					if (_recvedNetProcIdTracker.CheckReceivedResponded(netProcessId.Value) == false)
						Task.Delay(100).Wait();
					else
					{
						isServerResponded = true;
						break;
					}
				}

				bool alreadyExpired = false;

				if (isServerResponded == false)
					alreadyExpired = _netInterface.SetExpiredNetProcessId(msgPack.NetProcessId);

				if (alreadyExpired)
				{
					_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000087)", "A20",
						"NetClientSalesService.SubmitKomuterBookingPayment", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
				}
				else if (isServerResponded == false)
				{
					_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000089); Adnormal result !!;", "A21",
						"NetClientSalesService.SubmitKomuterBookingPayment", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
				}
				else
				{
					isServerResponded = true;
				}

				_log.LogText(_logChannel, "-",
					$@"End - IsLocalServerResponded: {isServerResponded};",
					"A100",
					"NetClientSalesService.SubmitKomuterBookingPayment");
			}
			catch (ThreadAbortException)
			{
				try
				{
					_log.LogText(_logChannel, "-",
					$@"ThreadAbortException",
					"A101",
					"NetClientSalesService.SubmitKomuterBookingPayment");
				}
				catch { }

				if (netProcessId.HasValue)
				{
					try
					{
						_netInterface.SetExpiredNetProcessId(netProcessId.Value);
					}
					catch { }
				}
			}
		}

		/// <summary>
		/// FuncCode:EXIT80.0450
		/// </summary>
		public void SubmitKomuterBookingPayment(string processId,
			string bookingId,
			string currencyId,
			decimal amount, string financePaymentMethod,
			string bTnGSaleTransNo,
			out bool isServerResponded, int waitDelaySec = 60)
		{
			isServerResponded = false;
			Guid? netProcessId = null;

			try
			{
				waitDelaySec = (waitDelaySec < 0) ? 20 : waitDelaySec;

				_log.LogText(_logChannel, processId, "Start - SubmitKomuterBookingPaymentII", "A01", "NetClientSalesService.SubmitKomuterBookingPaymentII");

				UIKomuterCompletePaymentSubmission res = new UIKomuterCompletePaymentSubmission(processId, DateTime.Now,
					bookingId, currencyId, amount, financePaymentMethod, bTnGSaleTransNo);

				NetMessagePack msgPack = new NetMessagePack(res) { DestinationPort = GetServerPort() };
				netProcessId = msgPack.NetProcessId;

				_log.LogText(_logChannel, processId,
					msgPack, "A05", "NetClientSalesService.SubmitKomuterBookingPaymentII", extraMsg: "MsgObject: NetMessagePack");

				_netInterface.SendMsgPack(msgPack);

				DateTime startTime = DateTime.Now;
				DateTime endTime = startTime.AddSeconds(waitDelaySec);

				while (endTime.Subtract(DateTime.Now).TotalSeconds > 0)
				{
					if (_recvedNetProcIdTracker.CheckReceivedResponded(netProcessId.Value) == false)
						Task.Delay(100).Wait();
					else
					{
						isServerResponded = true;
						break;
					}
				}

				bool alreadyExpired = false;

				if (isServerResponded == false)
					alreadyExpired = _netInterface.SetExpiredNetProcessId(msgPack.NetProcessId);

				if (alreadyExpired)
				{
					_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT80.0450.P01)", "A20",
						"NetClientSalesService.SubmitKomuterBookingPaymentII", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
				}
				else if (isServerResponded == false)
				{
					_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT80.0450.P02); Adnormal result !!;", "A21",
						"NetClientSalesService.SubmitKomuterBookingPaymentII", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
				}
				else
				{
					isServerResponded = true;
				}

				_log.LogText(_logChannel, "-",
					$@"End - IsLocalServerResponded: {isServerResponded};",
					"A100",
					"NetClientSalesService.SubmitKomuterBookingPaymentII");
			}
			catch (ThreadAbortException)
			{
				try
				{
					_log.LogText(_logChannel, "-",
					$@"ThreadAbortException",
					"A101",
					"NetClientSalesService.SubmitKomuterBookingPaymentII");
				}
				catch { }

				if (netProcessId.HasValue)
				{
					try
					{
						_netInterface.SetExpiredNetProcessId(netProcessId.Value);
					}
					catch { }
				}
			}
		}

		public void PauseCountDown(out bool isServerResponded, int waitDelaySec = 5)
		{
			isServerResponded = false;

			Guid lastNetProcessId;

			waitDelaySec = (waitDelaySec < 0) ? 20 : waitDelaySec;

			_log.LogText(_logChannel, "-", "Start - PauseCountDown", "A01", "NetClientSalesService.StartSales");

			UICountDownPausedRequest res = new UICountDownPausedRequest("-", DateTime.Now);

			NetMessagePack msgPack = new NetMessagePack(res) { DestinationPort = GetServerPort() };
			lastNetProcessId = msgPack.NetProcessId;

			_log.LogText(_logChannel, "-",
				msgPack, "A05", "NetClientSalesService.PauseCountDown", extraMsg: "MsgObject: NetMessagePack");

			_netInterface.SendMsgPack(msgPack);

			//DateTime startTime = DateTime.Now;
			//DateTime endTime = startTime.AddSeconds(waitDelaySec);

			//while (endTime.Subtract(DateTime.Now).TotalSeconds > 0)
			//{
			//	if (_recvedNetProcIdTracker.CheckReceivedResponded(lastNetProcessId) == false)
			//		Task.Delay(100).Wait();
			//	else
			//	{
			//		isServerResponded = true;
			//		break;
			//	}
			//}

			//bool alreadyExpired = false;

			//if (isServerResponded == false)
			//	alreadyExpired = _netInterface.SetExpiredNetProcessId(msgPack.NetProcessId);

			//if (alreadyExpired)
			//{
			//	_log.LogText(_logChannel, "-", $@"Unable to read from Local Server", "A20",
			//		"NetClientSalesService.PauseCountDown", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
			//}
			//else if (isServerResponded == false)
			//{
			//	_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000014); Adnormal result !!;", "A21",
			//		"NetClientSalesService.PauseCountDown", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
			//}
			//else
			//{
			//	isServerResponded = true;
			//}

			//_log.LogText(_logChannel, "-",
			//	$@"End - IsLocalServerResponded: {isServerResponded};",
			//	"A100",
			//	"NetClientSalesService.PauseCountDown");
		}

		public void EditSalesDetail(TickSalesMenuItemCode editDetailItem, out bool isServerResponded, int waitDelaySec = 60)
		{
			isServerResponded = false;

			RemoveCustomerInfoEntryTimeoutExtension();

			Guid lastNetProcessId;

			waitDelaySec = (waitDelaySec < 0) ? 20 : waitDelaySec;

			_log.LogText(_logChannel, "-", "Start - StartSales", "A01", "NetClientSalesService.EditSalesDetail");

			UIDetailEditRequest res = new UIDetailEditRequest("-", DateTime.Now, editDetailItem);

			NetMessagePack msgPack = new NetMessagePack(res) { DestinationPort = GetServerPort() };
			lastNetProcessId = msgPack.NetProcessId;

			_log.LogText(_logChannel, "-",
				msgPack, "A05", "NetClientSalesService.EditSalesDetail", extraMsg: "MsgObject: NetMessagePack");

			_netInterface.SendMsgPack(msgPack);

			DateTime startTime = DateTime.Now;
			DateTime endTime = startTime.AddSeconds(waitDelaySec);

			while (endTime.Subtract(DateTime.Now).TotalSeconds > 0)
			{
				if (_recvedNetProcIdTracker.CheckReceivedResponded(lastNetProcessId) == false)
					Task.Delay(100).Wait();
				else
				{
					isServerResponded = true;
					break;
				}
			}

			bool alreadyExpired = false;

			if (isServerResponded == false)
				alreadyExpired = _netInterface.SetExpiredNetProcessId(msgPack.NetProcessId);

			if (alreadyExpired)
			{
				_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000046)", "A20",
					"NetClientSalesService.EditSalesDetail", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
			}
			else if (isServerResponded == false)
			{
				_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000047); Adnormal result !!;", "A21",
					"NetClientSalesService.EditSalesDetail", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
			}
			else
			{
				isServerResponded = true;
			}

			_log.LogText(_logChannel, "-",
				$@"End - IsLocalServerResponded: {isServerResponded};",
				"A100",
				"NetClientSalesService.EditSalesDetail");
		}


		//public void NavigateToPage(PageNavigateDirection navigateDirection, out bool isServerResponded, int waitDelaySec = 60)
		public void NavigateToPage(PageNavigateDirection navigateDirection, int waitDelaySec = 60)
		{
			//isServerResponded = false;
			RemoveCustomerInfoEntryTimeoutExtension();

			Guid lastNetProcessId;

			waitDelaySec = (waitDelaySec < 0) ? 20 : waitDelaySec;

			_log.LogText(_logChannel, "-", "Start - StartSales", "A01", "NetClientSalesService.NavigateToPage");

			UIPageNavigateRequest res = new UIPageNavigateRequest("-", DateTime.Now, navigateDirection);

			NetMessagePack msgPack = new NetMessagePack(res) { DestinationPort = GetServerPort() };
			lastNetProcessId = msgPack.NetProcessId;

			_log.LogText(_logChannel, "-",
				msgPack, "A05", "NetClientSalesService.NavigateToPage", extraMsg: "MsgObject: NetMessagePack");

			_netInterface.SendMsgPack(msgPack);

			//DateTime startTime = DateTime.Now;
			//DateTime endTime = startTime.AddSeconds(waitDelaySec);

			//while (endTime.Subtract(DateTime.Now).TotalSeconds > 0)
			//{
			//	if (_recvedNetProcIdTracker.CheckReceivedResponded(lastNetProcessId) == false)
			//		Task.Delay(100).Wait();
			//	else
			//	{
			//		isServerResponded = true;
			//		break;
			//	}
			//}

			//bool alreadyExpired = false;

			//if (isServerResponded == false)
			//	alreadyExpired = _netInterface.SetExpiredNetProcessId(msgPack.NetProcessId);

			//if (alreadyExpired)
			//{
			//	_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000061)", "A20",
			//		"NetClientSalesService.NavigateToPage", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
			//}
			//else if (isServerResponded == false)
			//{
			//	_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000062); Adnormal result !!;", "A21",
			//		"NetClientSalesService.NavigateToPage", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
			//}
			//else
			//{
			//	isServerResponded = true;
			//}

			//_log.LogText(_logChannel, "-",
			//	$@"End - IsLocalServerResponded: {isServerResponded};",
			//	"A100",
			//	"NetClientSalesService.NavigateToPage");
		}

		public void ResetTimeout()
		{
			_log.LogText(_logChannel, "-", "Start - StartSales", "A01", "NetClientSalesService.ResetTimeout");

			UITimeoutChangeRequest res = new UITimeoutChangeRequest("-", DateTime.Now);
			NetMessagePack msgPack = new NetMessagePack(res) { DestinationPort = GetServerPort() };
			
			_log.LogText(_logChannel, "-", msgPack, "A05", "NetClientSalesService.ResetTimeout", 
				extraMsg: "MsgObject: NetMessagePack");

			_netInterface.SendMsgPack(msgPack);
		}

		public void RestartMachineRequest()
		{
			_log.LogText(_logChannel, "-", "Start - StartSales", "A01", "NetClientSalesService.ResetTimeout");

			UIRestartMachineRequest res = new UIRestartMachineRequest("-", DateTime.Now);
			NetMessagePack msgPack = new NetMessagePack(res) { DestinationPort = GetServerPort() };

			_log.LogText(_logChannel, "-", msgPack, "A05", "NetClientSalesService.ResetTimeout",
				extraMsg: "MsgObject: NetMessagePack");

			_netInterface.SendMsgPack(msgPack);
		}

		public void ExtendCustomerInfoEntryTimeout(int extentionTimeSec)
		{
			_log.LogText(_logChannel, "-", "Start - StartSales", "A01", "NetClientSalesService.ExtendCustomerInfoEntryTimeout");

			UITimeoutChangeRequest res = new UITimeoutChangeRequest("-", DateTime.Now, TimeoutChangeMode.MandatoryExtension, extentionTimeSec);
			NetMessagePack msgPack = new NetMessagePack(res) { DestinationPort = GetServerPort() };

			_log.LogText(_logChannel, "-", msgPack, "A05", "NetClientSalesService.ExtendCustomerInfoEntryTimeout",
				extraMsg: "MsgObject: NetMessagePack");

			_netInterface.SendMsgPack(msgPack);
		}

		public void RemoveCustomerInfoEntryTimeoutExtension()
		{
			_log.LogText(_logChannel, "-", "Start - StartSales", "A01", "NetClientSalesService.RemoveCustomerInfoEntryTimeoutExtend");

			UITimeoutChangeRequest res = new UITimeoutChangeRequest("-", DateTime.Now, TimeoutChangeMode.RemoveMandatoryTimeout, 0);
			NetMessagePack msgPack = new NetMessagePack(res) { DestinationPort = GetServerPort() };

			_log.LogText(_logChannel, "-", msgPack, "A05", "NetClientSalesService.RemoveCustomerInfoEntryTimeoutExtend",
				extraMsg: "MsgObject: NetMessagePack");

			_netInterface.SendMsgPack(msgPack);
		}

		public void StartSales(out bool isServerResponded, int waitDelaySec = 60)
		{
			isServerResponded = false;

			Guid lastNetProcessId;

			waitDelaySec = (waitDelaySec < 0) ? 20 : waitDelaySec;

			_log.LogText(_logChannel, "-", "Start - StartSales", "A01", "NetClientSalesService.StartSales");

			UIStartNewSalesRequest res = new UIStartNewSalesRequest("-", DateTime.Now);

			NetMessagePack msgPack = new NetMessagePack(res) { DestinationPort = GetServerPort() };
			lastNetProcessId = msgPack.NetProcessId;

			_log.LogText(_logChannel, "-",
				msgPack, "A05", "NetClientSalesService.StartSales", extraMsg: "MsgObject: NetMessagePack");

			_netInterface.SendMsgPack(msgPack);

			DateTime startTime = DateTime.Now;
			DateTime endTime = startTime.AddSeconds(waitDelaySec);


			while (endTime.Subtract(DateTime.Now).TotalSeconds > 0)
			{
				if (_recvedNetProcIdTracker.CheckReceivedResponded(lastNetProcessId) == false)
					Task.Delay(100).Wait();
				else
				{
					isServerResponded = true;
					break;
				}
			}

			bool alreadyExpired = false;

			if (isServerResponded == false)
				alreadyExpired = _netInterface.SetExpiredNetProcessId(msgPack.NetProcessId);

			if (alreadyExpired)
			{
				_log.LogText(_logChannel, "-", $@"Unable to read from Local Server", "A20",
					"NetClientSalesService.StartSales", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
			}
			else if (isServerResponded == false)
			{
				_log.LogText(_logChannel, "-", $@"Unable to read from Local Server; (EXIT9000006); Adnormal result !!;", "A21",
					"NetClientSalesService.StartSales", NssIT.Kiosk.AppDecorator.Log.MessageType.Error);
			}
			else
			{
				isServerResponded = true;
			}

			_log.LogText(_logChannel, "-",
				$@"End - IsLocalServerResponded: {isServerResponded};",
				"A100",
				"NetClientSalesService.StartSales");
		}

		private void _netInterface_OnDataReceived(object sender, DataReceivedEventArgs e)
		{
			if (e.ReceivedData?.Module == AppModule.UIKioskSales)
			{
				if ((e.ReceivedData?.MsgObject is IKioskMsg tMsg) && (tMsg.Instruction == CommInstruction.ReferToGenericsUIObj))
                {
					///// By Pass
					int tVal1 = 0;
                }
				else if (e.ReceivedData.MsgObject is UIServerApplicationStatusAck appStt)
				{
					_serverApplicationStatus = appStt;
				}
				else if (e.ReceivedData.MsgObject is UIWebServerLogonStatusAck logStt)
				{
					_webServerLogonStatus = logStt;
				}
				else
				{
					_recvedNetProcIdTracker.AddNetProcessId(e.ReceivedData.NetProcessId);
					RaiseOnDataReceived(sender, e);
				}
			}
			else if (e.ReceivedData?.Module == AppModule.Unknown)
			{
				string errMsg = $@"Error : {e.ReceivedData.ErrorMessage}; NetProcessId: {e.ReceivedData.NetProcessId}";
				_log.LogText(_logChannel, "-", errMsg, "A02", "NetClientSalesService._netInterface_OnDataReceived", NssIT.Kiosk.AppDecorator.Log.MessageType.Error, netProcessId: e.ReceivedData.NetProcessId);
			}
		}

		private void RaiseOnDataReceived(object sender, DataReceivedEventArgs e)
		{
			try
			{
				if (OnDataReceived != null)
				{
					OnDataReceived.Invoke(sender, e);
				}
			}
			catch (Exception ex)
			{
				_log.LogError(_logChannel, "-", new Exception("Unhandled event exception; (EXIT9000200)", ex), "EX01", "NetClientSalesService.RaiseOnDataReceived", netProcessId: e?.ReceivedData?.NetProcessId);
			}
		}

		public class ReceivedNetProcessIdTracker
		{
			private (int ClearNetProcessIdListIntervalSec, DateTime NextClearListTime, ConcurrentDictionary<Guid, DateTime> NetProcessIdList) _receivedNetProcess
				= (ClearNetProcessIdListIntervalSec: 60, NextClearListTime: DateTime.Now, NetProcessIdList: new ConcurrentDictionary<Guid, DateTime>());

			public ReceivedNetProcessIdTracker() { }

			public void AddNetProcessId(Guid netProcessId)
			{
				lock (_receivedNetProcess.NetProcessIdList)
				{
					// Clear NetProcessIdList -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- 
					if (_receivedNetProcess.NextClearListTime.Ticks < DateTime.Now.Ticks)
					{
						Guid[] clearNetProcessIdArr = (from keyPair in _receivedNetProcess.NetProcessIdList
													   where (keyPair.Value.Ticks <= _receivedNetProcess.NextClearListTime.Ticks)
													   select keyPair.Key).ToArray();

						foreach (Guid netProcId in clearNetProcessIdArr)
							_receivedNetProcess.NetProcessIdList.TryRemove(netProcId, out DateTime receivedTime);
						_receivedNetProcess.NextClearListTime = DateTime.Now.AddSeconds(_receivedNetProcess.ClearNetProcessIdListIntervalSec);
					}
					// -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- 

					if (_receivedNetProcess.NetProcessIdList.TryGetValue(netProcessId, out DateTime receivedTime2))
						_receivedNetProcess.NetProcessIdList.TryRemove(netProcessId, out DateTime receivedTime3);

					try
					{
						_receivedNetProcess.NetProcessIdList.TryAdd(netProcessId, DateTime.Now);
					}
					catch { }
				}
			}

			/// <summary>
			/// Return true if NetProcessId has responded.
			/// </summary>
			/// <param name="netProcessId"></param>
			/// <returns></returns>
			public bool CheckReceivedResponded(Guid netProcessId)
			{
				bool retFound = false;

				Thread execThread = new Thread(new ThreadStart(new Action(() =>
				{
					lock (_receivedNetProcess.NetProcessIdList)
					{
						if (_receivedNetProcess.NetProcessIdList.TryGetValue(netProcessId, out DateTime receivedTime2))
							retFound = true;
					}
				})));
				execThread.IsBackground = true;
				execThread.Start();
				execThread.Join();

				return retFound;
			}
		}
	}
}