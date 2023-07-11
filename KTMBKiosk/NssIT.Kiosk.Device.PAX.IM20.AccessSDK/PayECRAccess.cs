using NssIT.Kiosk.AppDecorator.DomainLibs.Common.CreditDebitCharge;
using NssIT.Kiosk.Device.PAX.IM20.OrgAPI;
using NssIT.Kiosk.Device.PAX.IM20.OrgAPI.Base;
using NssIT.Kiosk.Log.DB;
using NssIT.Kiosk.Log.DB.StatusMonitor;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Device.PAX.IM20.AccessSDK
{
	public class PayECRAccess : IDisposable
	{
		private const string LogChannel = "PAX_IM20_ACCESS";
		private DbLog _log = null;

		private const double _minimumAmount = 0.1;

		private string[] _commandList = { PayCommand.Norm_Sale, PayCommand.Norm_Void, PayCommand.Norm_Settlement, PayCommand.Norm_Query};
		//private string _receiptPath = @"C:\ECR_Receipts\";
		private int _defaultDecimalPoint = 2;
		private int _receiveBufferSize = 1024;
		private bool _lastProcessIsSuccess = false;
		private bool _workInProgress = false;
		private bool _stopPayECRProcess = false;

		public string _comPort = "COM100";
		public int _comPortTimeout = 120000;

		private Thread _threadWorker = null;
		private ConcurrentQueue<PayECSData> _payECSDataList = new ConcurrentQueue<PayECSData>();

		private PayECR _ecr;

		public const int SaleMaxWaitingSec = 300;
		public const int VoidMaxWaitingSec = 300;
		public const int SettlementMaxWaitingSec = (1 * 60 * 60 * 3);

		public event EventHandler<TrxCallBackEventArgs> OnCompleteCallback;
		public event EventHandler<InProgressEventArgs> OnInProgressCall;

		public DbLog Log
		{
			get
			{
				return _log ?? (_log = DbLog.GetDbLog());
			}
		}

		private static PayECRAccess _payECRAccess = null;
		public static PayECRAccess GetPayECRAccess(string comPort, int timeout)
        {
			if (_payECRAccess == null)
				_payECRAccess = new PayECRAccess(comPort, timeout);
			
			return _payECRAccess;
		}

		public static PayECRAccess GetExistingPayECRAccess()
		{
			return _payECRAccess;
		}

		public PayECRAccess(string comPort, int timeout) 
		{
			_comPort = comPort;
			_comPortTimeout = timeout;

			_threadWorker = new Thread(new ThreadStart(PayECRProcessThreadWorking));
			_threadWorker.SetApartmentState(ApartmentState.STA);
			_threadWorker.IsBackground = true;
			_threadWorker.Start();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="amount">
		/// </param>
		/// <param name="qrId">
		///		This qrId will be sent to paywave as a reference number.
		/// </param>
		/// <param name="qrNo">
		/// </param>
		/// <param name="docNumbers">
		///		This numbers is an extra reference numbers. These numbers will not be sent to paywave. But will be log to together with "qrId". Can use a string separator (like "^") to split document number.
		/// </param>
		public void Pay(string ProcessId, string amount, AccountType accType, string qrId = null, string qrNo = null, string docNumbers = null)
		{
			string ecrCurrencyAmt = IM20.OrgAPI.Tools.Convertion.ToECRCurrencyString(amount, _defaultDecimalPoint);
			PayECSData newData = new PayECSData()
			{
				ProcessId = ProcessId,
				Command = PayCommand.Norm_Sale,
				Host = "00",
				AccType = AccountType.CreditCard,
				Amount = ecrCurrencyAmt,
				QrId = qrId,
				QrNo = qrNo,
				DocNumbers = docNumbers,
				MaxWaitingSec = SaleMaxWaitingSec, 
				FirstWaitingTimeSec = 60
			};

			if (AddPayECSData(newData) == false)
				throw new Exception("Pay ECR is busy.");
		}

		public void VoidPayment(string ProcessId, string amount, string host, AccountType accType, string qrId = null, string qrNo = null)
		{
			string ecrCurrencyAmt = IM20.OrgAPI.Tools.Convertion.ToECRCurrencyString(amount, _defaultDecimalPoint);
			PayECSData newData = new PayECSData()
			{
				ProcessId = ProcessId,
				Command = PayCommand.Norm_Void,
				Host = host,
				AccType = accType,
				Amount = ecrCurrencyAmt,
				QrId = qrId,
				QrNo = qrNo,
				MaxWaitingSec = VoidMaxWaitingSec
			};

			if (AddPayECSData(newData) == false)
				throw new Exception("Pay ECR is busy.");
		}

		public void QueryCardResponse(string host, string docNumbers = null)
		{
			PayECSData newData = new PayECSData()
			{
				ProcessId = docNumbers,
				Command = PayCommand.Norm_Query,
				Host = host,
				AccType = AccountType.CreditCard,
				Amount = "0",
				QrId = docNumbers,
				QrNo = null,
				DocNumbers = docNumbers,
				MaxWaitingSec = 60,
				FirstWaitingTimeSec = 60
			};

			if (AddPayECSData(newData) == false)
				throw new Exception("Pay ECR is busy.");
		}

		public void SettlePayment(string ProcessId, string host)
		{
			PayECSData newData = new PayECSData() { ProcessId = ProcessId, Command = PayCommand.Norm_Settlement, Host = host, Amount = "", QrId = "", QrNo = "", MaxWaitingSec = SettlementMaxWaitingSec, FirstWaitingTimeSec = 60 * 60 * 6 };

			if (AddPayECSData(newData) == false)
				throw new Exception("Pay ECR is busy.");
		}

		public void Echo()
		{
			PayECSData newData = new PayECSData() { Command = PayCommand.Base_Echo, Host = "", Amount = "", QrId = "", QrNo = "" };

			if (AddPayECSData(newData) == false)
				throw new Exception("Pay ECR is busy.");
		}

		public void CancelRequest()
		{
			lock (_payECSDataList)
			{
				if (_ecr != null)
				{
					_ecr.Stop = true;
					Log.LogText(LogChannel, "*", $@"Card Transaction Cancel Request; Is_Stop : {_ecr.Stop}", "A10", "PayECRAccess.CancelRequest");
				}
			}
		}

		public void ForceToCancel()
		{
			lock (_payECSDataList)
			{
				if (_ecr != null)
				{
					_ecr.ForceToStop();
					Log.LogText(LogChannel, "*", $@"Card Transaction Force to Cancel; Is_Stop : {_ecr.Stop}", "A10", "PayECRAccess.ForceToCancel");
				}
			}
		}

		/// <summary>
		/// Event handle for _ecr.OnInProgress. This will pass the handle to another event handle.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void PayWaveProgressHandle(object sender, InProgressEventArgs e)
		{
			try
			{
				OnInProgressCall?.Invoke(null, e);
			}
			catch (Exception ex)
			{
				Log.LogError(LogChannel, "-", new Exception("Unhandle exception at ecr.OnInProgress", ex), "EX01", "PayECRAccess.PayWaveProgress");
			}
		}

		/// <summary>
		/// Use to send OnInProgressCall event to a event handle.
		/// </summary>
		/// <param name="processId"></param>
		/// <param name="locTag">Location Tag if error found</param>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void CallPayWaveProgressEvent(string processId, string locTag, object sender, InProgressEventArgs e)
		{
			try
			{
				OnInProgressCall?.Invoke(null, e);
			}
			catch (Exception ex)
			{
				locTag = locTag ?? "-";
				Log.LogError(LogChannel, processId, ex, locTag, "PayECRAccess.CallPayWaveProgress");
			}
		}

		private bool AddPayECSData(PayECSData data)
		{
			if (data == null)
				throw new Exception("Data parameter cannot be NULL at PCA.AddPayECSData");

			lock (_payECSDataList)
			{
				if ((_workInProgress == false) && (_payECSDataList.Count == 0) && (_stopPayECRProcess == false))
				{
					Log.LogText(LogChannel, "*", data, "DBG02", "PayECRAccess.AddPayECSData", AppDecorator.Log.MessageType.Debug, extraMsg: "MsgObj: PayECSData");

					_payECSDataList.Enqueue(data);
					Monitor.PulseAll(_payECSDataList);
					return true;
				}
				else
				{
					string tMsg = $@"_workInProgress: {_workInProgress}; _payECSDataList.Count: {_payECSDataList?.Count};_stopPayECRProcess: {_stopPayECRProcess}";
					Log.LogText(LogChannel, "*", tMsg, "DBG05", "PayECRAccess.AddPayECSData", AppDecorator.Log.MessageType.Debug);

					return false;
				}
			}
		}

		private TimeSpan _MaxWaitPeriod = new TimeSpan(0, 0, 10);
		private PayECSData GetNextPayECSData()
		{
			PayECSData retData = null;

			lock (_payECSDataList)
			{
				if (_stopPayECRProcess == false)
				{
					if (_payECSDataList.Count == 0)
					{
						Monitor.Wait(_payECSDataList, _MaxWaitPeriod);
					}
					if (_payECSDataList.TryDequeue(out retData))
					{
						Log.LogText(LogChannel, "*", retData.Duplicate(), "DBG02", "PayECRAccess.GetNextPayECSData", AppDecorator.Log.MessageType.Debug, extraMsg: $@"PayECSDataList.Count: {_payECSDataList?.Count}; MsgObj: PayECSData");

						_workInProgress = true;
						return retData;
					}
				}
			}
			return null;
		}

		private void PayECRProcessThreadWorking()
		{
			TrxCallBackEventArgs processResult = null;
			InProgressEventArgs inPEv = null;
			ResponseInfo respInfor = null;
			string currProcessId = "--";
			bool onCompleteCallbackEventFlag = false;
			PayECSData data = null;

			while (_stopPayECRProcess == false)
			{
				try
				{
					onCompleteCallbackEventFlag = false;
					data = null;
					data = GetNextPayECSData();

					if (data != null)
					{
						Log.LogText(LogChannel, "*", data.Duplicate(), "DBG02", "PayECRAccess.PayECRProcessThreadWorking", AppDecorator.Log.MessageType.Debug, extraMsg: $@"PayECSDataList.Count: {_payECSDataList?.Count}; MsgObj: PayECSData");

						if (_ecr is null)
						{
							_ecr = new PayECR(_comPort, _comPortTimeout, _receiveBufferSize);
							_ecr.OnInProgress += PayWaveProgressHandle;
						}

						processResult = new TrxCallBackEventArgs();
						respInfor = new ResponseInfo();
						if ((data.HasCommand) && (data.ProcessDone == false))
						{
							_ecr.Stop = false;
							currProcessId = data.ProcessId ?? "-----";

							Log.LogText(LogChannel, currProcessId, $@"Start Process; Thread Hash Code : {Thread.CurrentThread.GetHashCode()};", "A01", "PayECRAccess.OnPayECRProcess");

							respInfor = Send(data.ProcessId, data.Command, data.Host, data.Amount, data.AccType, data.QrId, data.QrNo, data.DocNumbers, data.MaxWaitingSec, data.FirstWaitingTimeSec);

							if (respInfor.StatusCode.Equals(ResponseStatusCode.Success))
							{
								if ((data.HasCommand) && data.Command.Equals(IM20.OrgAPI.Base.PayCommand.Norm_Sale))
									Log.LogText(LogChannel, currProcessId, "Card Transaction Success", "A10", "PayECRAccess.OnPayECRProcess",
										adminMsg: "Card Transaction Success");

								processResult.IsSuccess = true;
								processResult.Result = respInfor;
								processResult.Error = null;

								_lastProcessIsSuccess = true;
							}
							else
							{
								respInfor.ErrorMsg = ResponseStatusCode.TranslateCode(respInfor.StatusCode);

								if ((data.HasCommand) && data.Command.Equals(IM20.OrgAPI.Base.PayCommand.Norm_Sale))
									Log.LogText(LogChannel, currProcessId, $@"Card Transaction Fail; {respInfor.ErrorMsg}", "A15", "PayECRAccess.OnPayECRProcess",
										adminMsg: $@"Card Transaction Fail; {respInfor.ErrorMsg}");

								processResult.IsSuccess = false;
								processResult.Result = respInfor;
								processResult.Error = new Exception(respInfor.ErrorMsg);

								_lastProcessIsSuccess = false;
							}

							Log.LogText(LogChannel, currProcessId, "DEBUG::T002", "T002", "PayECRAccess.OnPayECRProcess", AppDecorator.Log.MessageType.Debug);
							try
							{
								onCompleteCallbackEventFlag = true;
								OnCompleteCallback?.Invoke(null, processResult);
							}
							catch (Exception ex)
							{
								Log.LogError(LogChannel, currProcessId, new Exception("Unhandle exception at OnCompleteCallback Event", ex), "EX01", "PayECRAccess.OnPayECRProcess");
							}
							Log.LogText(LogChannel, currProcessId, "DEBUG::T003", "T003", "PayECRAccess.OnPayECRProcess", AppDecorator.Log.MessageType.Debug);
						}

						Log.LogText(LogChannel, currProcessId, "DEBUG::T004", "T004", "PayECRAccess.OnPayECRProcess", AppDecorator.Log.MessageType.Debug);
						//---------------------
						// Trigger Event Result
						//---------------------
						inPEv = new InProgressEventArgs();
						// ..continue to Settlement..
						if ((data.HasCommand) && data.Command.Equals(IM20.OrgAPI.Base.PayCommand.Norm_Sale)
							&& (processResult.IsSuccess))
						{
							Log.LogText(LogChannel, currProcessId, "DEBUG::T005", "T005", "PayECRAccess.OnPayECRProcess", AppDecorator.Log.MessageType.Debug);
							inPEv.Message = "Sale Completed.";
							CallPayWaveProgressEvent(currProcessId, "PayECRProcessThreadWorking::Sale Completed", null, inPEv);
							Log.LogText(LogChannel, currProcessId, "DEBUG::T006", "T006", "PayECRAccess.OnPayECRProcess", AppDecorator.Log.MessageType.Debug);
						}
						// Settlement
						else if ((data.HasCommand) && data.Command.Equals(IM20.OrgAPI.Base.PayCommand.Norm_Settlement)
							&& (processResult.IsSuccess))
						{
							Log.LogText(LogChannel, currProcessId, "DEBUG::T007", "T007", "PayECRAccess.OnPayECRProcess", AppDecorator.Log.MessageType.Debug);
							inPEv.Message = "Settlement Completed.";
							CallPayWaveProgressEvent(currProcessId, "PayECRProcessThreadWorking::Settlement Completed", null, inPEv);
							Log.LogText(LogChannel, currProcessId, "DEBUG::T008", "T008", "PayECRAccess.OnPayECRProcess", AppDecorator.Log.MessageType.Debug);
						}
						// Show Normal Transaction Issue
						else if ((data.HasCommand)
							&& (respInfor != null))
						{
							Log.LogText(LogChannel, currProcessId, "DEBUG::T009", "T009", "PayECRAccess.OnPayECRProcess", AppDecorator.Log.MessageType.Debug);
							inPEv.Message = ResponseStatusCode.TranslateCode(respInfor.StatusCode);
							CallPayWaveProgressEvent(currProcessId, "PayECRProcessThreadWorking::Transaction Issue", null, inPEv);
							Log.LogText(LogChannel, currProcessId, "DEBUG::T010", "T010", "PayECRAccess.OnPayECRProcess", AppDecorator.Log.MessageType.Debug);
						}
						// Show abnormal Transaction Message
						else if ((data.HasCommand)
							&& (processResult.IsSuccess == false))
						{
							Log.LogText(LogChannel, currProcessId, "DEBUG::T011", "T011", "PayECRAccess.OnPayECRProcess", AppDecorator.Log.MessageType.Debug);

							string errMsg = "";
							string stage = "(-)";

							if (data.Command.Equals(IM20.OrgAPI.Base.PayCommand.Norm_Sale))
								stage = "(On Sale)";
							else if (data.Command.Equals(IM20.OrgAPI.Base.PayCommand.Norm_Settlement))
								stage = "(On Settlement)";

							errMsg = $"{stage}" + ((processResult.Error != null) ? processResult.Error.Message : "Fail trnasaction (99).");

							inPEv.Message = errMsg;
							CallPayWaveProgressEvent(currProcessId, "PayECRProcessThreadWorking::Error found", null, inPEv);

							Log.LogText(LogChannel, currProcessId, "DEBUG::T012", "T012", "PayECRAccess.OnPayECRProcess", AppDecorator.Log.MessageType.Debug);
						}
						else
						{
							Log.LogText(LogChannel, currProcessId, "DEBUG::T013", "T013", "PayECRAccess.OnPayECRProcess", AppDecorator.Log.MessageType.Debug);
							inPEv.Message = "Completed.";
							CallPayWaveProgressEvent(currProcessId, "PayECRProcessThreadWorking::Completed", null, inPEv);
							Log.LogText(LogChannel, currProcessId, "DEBUG::T014", "T014", "PayECRAccess.OnPayECRProcess", AppDecorator.Log.MessageType.Debug);
						}
					}
				}
				catch (Exception ex)
				{
					if ((ex is ThreadStateException) == false)
					{
						string errMsg = $@"{ex.Message}";
						if (errMsg?.Length > 3900)
							errMsg = errMsg.Substring(0, 3800);

						StatusHub.GetStatusHub().zNewStatus_IsCardMachineDataCommNormal(AppDecorator.DomainLibs.KioskStatus.KioskCommonStatus.No, $@"Error encountered; At {DateTime.Now:HH:mm:ss} ; {errMsg}");
					}

					if ((data.HasCommand) && data.Command.Equals(IM20.OrgAPI.Base.PayCommand.Norm_Sale))
						Log.LogError(LogChannel, currProcessId, ex, "EX02", "PayECRAccess.OnPayECRProcess",
							adminMsg: $@"Error when acquire card transaction; {ex.Message}");

					processResult = processResult ?? new TrxCallBackEventArgs();
					processResult.IsSuccess = false;
					processResult.Result = null;
					processResult.Error = ex;

					_lastProcessIsSuccess = false;

					if (onCompleteCallbackEventFlag == false)
					{
						try
						{
							Log.LogText(LogChannel, currProcessId, "DEBUG::exT001", "exT001", "PayECRAccess.OnPayECRProcess", AppDecorator.Log.MessageType.Debug);
							OnCompleteCallback?.Invoke(null, processResult);
							Log.LogText(LogChannel, currProcessId, "DEBUG::exT002", "exT002", "PayECRAccess.OnPayECRProcess", AppDecorator.Log.MessageType.Debug);
						}
						catch (Exception ex2)
						{
							Log.LogError(LogChannel, currProcessId, ex2, "EX03", "PayECRAccess.OnPayECRProcess");
							throw ex;
						}
					}
				}
				finally
				{
					_workInProgress = false;
				}
			}

            try
            {
				LocalSoftShutDown();
			}
			catch (Exception ex)
            {}
		}

		private ResponseInfo Send(string processId, string command, string host, string amount, AccountType accType, string qrId = null, string qrNo = null, string docNumbers = null, int defaultWaitingTimeSec = 120, int defaultFirstWaitingTimeSec = 60)
		{
			ResponseInfo retInfor = null;

			string resultStr = "", resultStatusRemark = "";
			int resultStatusCode = 99;

			string commStr = GetCommandText(command, host, amount, accType, qrId, qrNo);

			if (command?.Equals(IM20.OrgAPI.Base.PayCommand.Norm_Sale, StringComparison.InvariantCultureIgnoreCase) == true)
            {
				Log.LogText(LogChannel, processId, $@"Start -- Card Transaction; Insert / Wave Card ..", "A02", "PayECRAccess.Send",
									adminMsg: $@"Start -- Card Transaction; Insert / Wave Card ..");
			}

			//if (command?.Equals(IM20.OrgAPI.Base.PayCommand.Norm_Query, StringComparison.InvariantCultureIgnoreCase) == true)
   //         {

   //         }
			//else
   //         {

			_ecr.SendReceive(processId, commStr, ref resultStr, ref resultStatusCode, ref resultStatusRemark, _receiveBufferSize, defaultWaitingTimeSec, defaultFirstWaitingTimeSec);
			//NEW .. _ecr.SendReceiveX(command, processId, commStr, ref resultStr, ref resultStatusCode, ref resultStatusRemark, _receiveBufferSize, defaultWaitingTimeSec, defaultFirstWaitingTimeSec);

			//}


			resultStr = (resultStr ?? "").Trim();

			if ((resultStatusCode == 0) && (resultStr.Length > 0))
				retInfor = ProcessReceiveString(processId, resultStr, amount, qrId, docNumbers);
			else
			{
				throw new Exception($"Fail to acquire response message; Status Code : {resultStatusCode.ToString()}; Status : {resultStatusRemark ?? ""}");
			}

			return retInfor;
		}
		
		/// <summary>
		/// Eval the Last-response-info refer to last Send Data activity 
		/// </summary>
		/// <param name="ReceivedMessege"></param>
		private ResponseInfo ProcessReceiveString(string processId, string ReceivedMessege, string amount = "", string qrId = "", string docNumbers = null)
		{
			qrId = (qrId ?? "").Trim();
			docNumbers = (docNumbers ?? "").Trim();

			decimal dTest = 0M;

			ResponseInfo retInfor = new ResponseInfo();

			retInfor.ResetInfo();
			retInfor.ResponseMsg = ReceivedMessege.Substring(0, 4);
			retInfor.ReportTime = DateTime.Now;

			if (retInfor.ResponseMsg == "R200")
			{
				retInfor.ResponseMsg = "SALE";
				retInfor.AdditionalData = qrId;
				retInfor.CardNo = ReceivedMessege.Substring(4, 19);
				retInfor.ExpiryDate = ReceivedMessege.Substring(23, 4);
				retInfor.StatusCode = ReceivedMessege.Substring(27, 2);
				retInfor.ApprovalCode = ReceivedMessege.Substring(29, 6);
				retInfor.RRN = ReceivedMessege.Substring(35, 12);
				retInfor.TransactionTrace = ReceivedMessege.Substring(47, 6);
				retInfor.BatchNumber = ReceivedMessege.Substring(53, 6);
				retInfor.HostNo = ReceivedMessege.Substring(59, 2);
				retInfor.TID = ReceivedMessege.Substring(61, 8);
				retInfor.MID = ReceivedMessege.Substring(69, 15);
				retInfor.AID = ReceivedMessege.Substring(84, 14);
				retInfor.TC = ReceivedMessege.Substring(98, 16);
				retInfor.CardholderName = ReceivedMessege.Substring(114, 26);
				retInfor.CardType = ReceivedMessege.Substring(140, 2);
				//retInfor.CardAppLabel = ReceivedMessege.Substring(142, 16);

				retInfor.Amount = (amount ?? "");
				if (decimal.TryParse(amount, out dTest))
				{
					retInfor.CurrencyAmount = IM20.OrgAPI.Tools.Convertion.ToECRCurrency(amount, _defaultDecimalPoint);
				}
				retInfor.DocumentNumbers = docNumbers;
			}
			else if (retInfor.ResponseMsg == "R201")
			{
				retInfor.ResponseMsg = "VOID";
				retInfor.VoidAmount = ReceivedMessege.Substring(4, 12);

				if (decimal.TryParse(retInfor.VoidAmount, out dTest))
				{
					retInfor.VoidCurrencyAmount = IM20.OrgAPI.Tools.Convertion.ToECRCurrency(retInfor.VoidAmount, _defaultDecimalPoint);
				}

				retInfor.StatusCode = ReceivedMessege.Substring(16, 2);
				retInfor.ApprovalCode = ReceivedMessege.Substring(18, 6);
				retInfor.RRN = ReceivedMessege.Substring(24, 12);
				retInfor.TransactionTrace = ReceivedMessege.Substring(36, 6);
				retInfor.BatchNumber = ReceivedMessege.Substring(42, 6);
				retInfor.HostNo = ReceivedMessege.Substring(48, 2);
				if (ReceivedMessege.Length > 50)
				{
					retInfor.PartnerTrxID = ReceivedMessege.Substring(50, 32);
					retInfor.AlipayTrxID = ReceivedMessege.Substring(82, 64);
					retInfor.CustomerID = ReceivedMessege.Substring(146, 26);
				}
			}
			else if (retInfor.ResponseMsg == "R500")
			{
				retInfor.ResponseMsg = "SETTLEMENT";
				retInfor.HostNo = ReceivedMessege.Substring(4, 2);
				retInfor.StatusCode = ReceivedMessege.Substring(6, 2);
				retInfor.BatchNumber = ReceivedMessege.Substring(8, 6);
				retInfor.BatchCount = ReceivedMessege.Substring(14, 3);
				retInfor.BatchAmount = ReceivedMessege.Substring(17, 12);
				if (decimal.TryParse(retInfor.BatchAmount, out dTest))
				{
					retInfor.BatchCurrencyAmount = IM20.OrgAPI.Tools.Convertion.ToECRCurrency(retInfor.BatchAmount, _defaultDecimalPoint);
				}
			}
			else if (retInfor.ResponseMsg == "R290")
			{
				retInfor.ResponseMsg = "ALI_SALE";
				retInfor.StatusCode = ReceivedMessege.Substring(4, 2);
				retInfor.ApprovalCode = ReceivedMessege.Substring(6, 6);
				retInfor.TransactionTrace = ReceivedMessege.Substring(12, 6);
				retInfor.BatchNumber = ReceivedMessege.Substring(18, 6);
				retInfor.HostNo = ReceivedMessege.Substring(24, 2);
				retInfor.TID = ReceivedMessege.Substring(26, 8);
				retInfor.MID = ReceivedMessege.Substring(34, 15);

				retInfor.PartnerTrxID = ReceivedMessege.Substring(49, 32);
				retInfor.AlipayTrxID = ReceivedMessege.Substring(81, 64);
				if (ReceivedMessege.Length > 145)
					retInfor.CustomerID = ReceivedMessege.Substring(145, 26);
			}
			else if (retInfor.ResponseMsg == "R902")
			{
				retInfor.ResponseMsg = "ECHO";
				retInfor.StatusCode = ReceivedMessege.Substring(4, 2);
			}

			retInfor.Tag = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
			if (retInfor.StatusCode == "00")
			{
				if (retInfor.ResponseMsg == "SALE")
				{
					Log.LogText(LogChannel, processId, ResponseInfo.Duplicate(retInfor), "SALE_1", "PayECRAccess.ProcessReceiveString", extraMsg: "MsgObj: ResponseInfo");
				}
				else if (retInfor.ResponseMsg == "SETTLEMENT")
				{
					Log.LogText(LogChannel, processId, ResponseInfo.Duplicate(retInfor), "SETTLEMENT_1", "PayECRAccess.ProcessReceiveString", extraMsg: "MsgObj: ResponseInfo");
				}
				else 
				{
					Log.LogText(LogChannel, processId, ResponseInfo.Duplicate(retInfor), "OTHER_1", "PayECRAccess.ProcessReceiveString", extraMsg: "MsgObj: ResponseInfo");
				}
			}
			else
			{
				Log.LogText(LogChannel, processId, ResponseInfo.Duplicate(retInfor), "FAIL", "PayECRAccess.ProcessReceiveString", 
					AppDecorator.Log.MessageType.Error, extraMsg: "Fail response; MsgObj: ResponseInfo");
			}

			return retInfor;
		}

		/// <summary>
		///		Eval a command string base on parameters.
		/// </summary>
		/// <param name="command"></param>
		/// <param name="host"></param>
		/// <param name="amount"></param>
		/// <param name="qrId">
		///		QR Id or Additional Data (24 chars) for 'Normal Sale'.
		/// </param>
		/// <param name="qrNo"></param>
		/// <returns></returns>
		private string GetCommandText(string command, string host, string amount, AccountType accType, string qrId = null, string qrNo = null, string tipAmt = "")
		{
			//string retCommand = "";
			//string decAmtStr = "";

			command = (command ?? "").Trim().ToUpper();
			host = (host ?? "").Trim();
			qrId = (qrId ?? "").Trim();
			qrNo = (qrNo ?? "").Trim();

			host = host.PadLeft(2, '0');

			/* ----- Basic Validation ----- */
			if (command.Length != 4)
				throw new Exception($@"(Card Reader) Invalid command. Command ({command}) must be 4 chars valid string code.");

			if (command.Equals(PayCommand.Base_Echo, StringComparison.InvariantCultureIgnoreCase) && (command.Length == 4))
				return command;

			if (Array.IndexOf(_commandList, command) < 0)
				throw new Exception($@"(Card Reader) Invalid command. This command code ({command}) not supported.");

            //if (command.Equals(PayCommand.Norm_Settlement, StringComparison.InvariantCultureIgnoreCase))
            //{
            //    decAmtStr = ("").ToString().PadLeft(12, '0');
            //}
            //else
            //{
            //	decAmt = 0;
            //	if (decimal.TryParse((amount ?? ""), out decAmt) == false)
            //		throw new Exception("(Card Reader[clsPECSA]) Invalid cash amount. Must be a decimal value.");
            //	else if (decAmt <= 0)
            //		throw new Exception(string.Format("(Card Reader[clsPECSA]) Invalid cash amount. Amount{0} must greater or equal to {1}."
            //							, tryDec.ToString(), _minimumAmount.ToString()));

            //	decAmtStr = decAmt.ToString().PadLeft(12, '0');
            //}

            //if (command.Equals(PayCommand.Norm_Sale, StringComparison.InvariantCultureIgnoreCase) && (qrId.Length > 24))
            //{
            //	throw new Exception("(Card Reader[clsPECSA]) Additional (Sale) Data length cannot more than 24 chars.");
            //}
            //else 

   //         if ((qrId.Length > 12) && (!command.Equals(PayCommand.Norm_Sale, StringComparison.InvariantCultureIgnoreCase)))
			//	throw new Exception("(Card Reader with Invalid QR Id. QR Id cannot more than 12 chars.");
			
			//else if (qrNo.Length > 32)
			//	throw new Exception("(Card Reader with Invalid QR No. QR Number cannot more than 32 chars.");

			/* xxxxx xxxxx xxxxx xxxxx xxxxx*/
			if (command.Equals(PayCommand.Norm_Sale, StringComparison.InvariantCultureIgnoreCase))
			{
				return GetCardSaleCommandStr(command, host, amount, accType, qrId);
			}
			else if (command.Equals(PayCommand.Norm_Void, StringComparison.InvariantCultureIgnoreCase))
			{
				return GetVoidCommandStr(command, host, amount, qrId);
			}
			else if (command.Equals(PayCommand.Norm_Settlement, StringComparison.InvariantCultureIgnoreCase))
			{
				return GetSettlementCommandStr(command, host);
			}
			else if (command.Equals(PayCommand.Norm_Query, StringComparison.InvariantCultureIgnoreCase))
			{
				return GetQueryTransanctionCommandStr(command, host, qrId);
			}

			throw new Exception($@"Card Reader with Invalid command. This command code ({command}) not supported.");
			/* xxxxx xxxxx xxxxx xxxxx xxxxx */

			//retCommand = command.Trim();

            //if (retCommand.Equals(PayCommand.Norm_Void))
            //{
            //    retCommand += host.Trim();
            //    retCommand += decAmtStr;
            //    retCommand += qrId;
            //}
            //else
            //{
            //    retCommand += host.Trim();

            //    //IM20----
            //    if (command?.ToUpper().Equals("C200") == true)
            //        retCommand += ((int)accType).ToString().Trim();
            //    //--------

            //    retCommand += decAmtStr;

            //    if (command.Equals(PayCommand.Norm_Sale))
            //    {
            //        retCommand += qrId;
            //    }
            //    else if (command.Equals(PayCommand.Norm_ReFund) || command.Equals(PayCommand.Ali_ReFund))
            //    {
            //        retCommand += qrId.PadLeft(12, '0');
            //    }
            //    else
            //    {
            //        retCommand += qrId;
            //    }

            //    retCommand += tipAmt;
            //}

            //return retCommand;

            //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
            string GetCardSaleCommandStr(string commandX, string hostX, string amountX, AccountType accTypeX, string docX)
            {
				docX = (docX ?? "").Trim();
				/////-------------------------------------------------------------------------------------------------------------------------
				///// Validation
				/////-------------
				if (hostX.Length != 2)
					throw new Exception($@"(Card Reader) Invalid Host Code. Host ({hostX}) must be 2 chars valid string code.");

				if (string.IsNullOrWhiteSpace(docX))
                {
					throw new Exception("(Card Reader) Additional (Sale) Data cannot be empty.");
				}
				else if (docX.Length > 24)
				{
					throw new Exception("(Card Reader) Additional (Sale) Data length cannot more than 24 chars.");
				}

				decimal decAmtX = 0;
				if (decimal.TryParse((amountX ?? ""), out decAmtX) == false)
					throw new Exception("(Card Reader) Invalid cash amount. Must be a decimal value.");

				else if (decAmtX <= 0)
					throw new Exception($@"(Card Reader) Invalid cash amount. Amount({decAmtX:#,###.##}) must greater or equal to {_minimumAmount:#,###.##}");

				/////-------------------------------------------------------------------------------------------------------------------------
				
				string decAmtStrX = decAmtX.ToString().PadLeft(12, '0');
				/////-------------------------------------------------------------------------------------------------------------------------

				// Add Command
				string retCommandX = commandX.Trim();

				// Add Host
				retCommandX += hostX.Trim();

				// Add Type Transaction Type
				retCommandX += ((int)accTypeX).ToString().Trim();

				// Add Transaction Amount
				retCommandX += decAmtStrX;

				// Add Transaction Document No.
				retCommandX += docX;
				
				return retCommandX;
            }

			string GetVoidCommandStr(string commandX, string hostX, string amountX, string traceNoX)
            {
				/////-------------------------------------------------------------------------------------------------------------------------

				if (hostX.Length != 2)
					throw new Exception($@"(Card Reader) Invalid Host Code. Host ({hostX}) must be 2 chars valid string code");

				decimal decAmtX = 0;
				if (decimal.TryParse((amountX ?? ""), out decAmtX) == false)
					throw new Exception("(Card Reader) Invalid cash amount. Must be a decimal value");

				else if (decAmtX <= 0)
					throw new Exception($@"(Card Reader) Invalid cash amount. Amount({decAmtX:#,###.##}) must greater or equal to {_minimumAmount:#,###.##}");

				else if (string.IsNullOrWhiteSpace(traceNoX))
					throw new Exception("(Card Reader) Invalid Trace No. Trace No cannot be empty");

				else if (traceNoX.Length > 6) 
					throw new Exception("(Card Reader) Invalid Trace No. Trace No cannot more than 6 chars");
				
				string decAmtStrX = decAmtX.ToString().PadLeft(12, '0');
				/////-------------------------------------------------------------------------------------------------------------------------

				// Add Command
				string retCommandX = commandX.Trim();

				// Add Host
				retCommandX += hostX.Trim();

				// Add Transaction Amount
				retCommandX += decAmtStrX;

				// Add Trace No.
				retCommandX += traceNoX;

				return retCommandX;
			}

			string GetQueryTransanctionCommandStr(string commandX, string hostX, string docX)
            {
				docX = (docX ?? "").Trim();
				/////-------------------------------------------------------------------------------------------------------------------------
				///// Validation
				/////-------------
				if (hostX.Length != 2)
					throw new Exception($@"(Card Reader) Invalid Host Code. Host ({hostX}) must be 2 chars valid string code.");

				if (string.IsNullOrWhiteSpace(docX))
				{
					throw new Exception("(Card Reader) Additional No. cannot be empty.");
				}
				else if (docX.Length > 24)
				{
					throw new Exception("(Card Reader) Additional No. length cannot more than 24 chars.");
				}
				/////-------------------------------------------------------------------------------------------------------------------------

				// Add Command
				string retCommandX = commandX.Trim();

				// Add Host retCommandX += hostX.Trim();

				// Add Transaction Document No.
				retCommandX += docX;

				return retCommandX;
			}

			string GetSettlementCommandStr(string commandX, string hostX)
			{
				/////-------------------------------------------------------------------------------------------------------------------------
				///// Validation
				/////-------------
				if (hostX.Length != 2)
					throw new Exception($@"(Card Reader) Invalid Host Code. Host ({hostX}) must be 2 chars valid string code.");
				/////-------------------------------------------------------------------------------------------------------------------------

				// Add Command
				string retCommandX = commandX.Trim();

				// Add Host
				retCommandX += hostX.Trim();

				return retCommandX;
			}
		}

		/// <summary>
		///		Used to close the Paywave hardware port properly. Use this only when system is ready to end transaction.
		/// </summary>
		public void SoftShutDown()
		{
			//if (_ecr != null)
			//{
			//	_ecr.Stop = true;
			//	Log.LogText(LogChannel, "*", "Start SoftShutDown - 1 of 2", "A01", "PayECRAccess:SoftShutDown");
			//	_ecr.OnInProgress -= PayWaveProgressHandle;

			//	Log.LogText(LogChannel, "*", "Start SoftShutDown - 2 of 2", "A03", "PayECRAccess:SoftShutDown");
			//	DateTime startShutDownTime = DateTime.Now;
			//	int maximumDelaySec = 45;

			//	while ((DateTime.Now.Subtract(startShutDownTime).TotalSeconds < maximumDelaySec) && (_ecr.IsReadyToShutDown == false))
			//		System.Threading.Thread.Sleep(3000);

			//	Log.LogText(LogChannel, "*", "End SoftShutDown - 1 of 2", "A11", "PayECRAccess:SoftShutDown");
			//	_ecr.Dispose();
			//	_ecr = null;
			//	Log.LogText(LogChannel, "*", "End SoftShutDown - 2 of 2", "A12", "PayECRAccess:SoftShutDown");
			//}
		}

		/// <summary>
		///		Used to close the Paywave hardware port properly. Use this only when system is ready to end transaction.
		/// </summary>
		private bool _theEndOfAccess = false;
		private void LocalSoftShutDown()
		{
			if (_ecr != null)
			{
				_ecr.ForceToStop();
				Log.LogText(LogChannel, "*", "Start SoftShutDown - 1 of 2", "A01", "PayECRAccess:SoftShutDown");
				_ecr.OnInProgress -= PayWaveProgressHandle;

				Log.LogText(LogChannel, "*", "Start SoftShutDown - 2 of 2", "A03", "PayECRAccess:SoftShutDown");
				DateTime startShutDownTime = DateTime.Now;
				int maximumDelaySec = 45;

				Log.LogText(LogChannel, "*", "End SoftShutDown - 1 of 2", "A11", "PayECRAccess:SoftShutDown");
				_ecr.Dispose();

				while ((DateTime.Now.Subtract(startShutDownTime).TotalSeconds < maximumDelaySec) && (_ecr.IsReadyToShutDown == false))
					System.Threading.Thread.Sleep(3000);

				_ecr = null;
				Log.LogText(LogChannel, "*", "End SoftShutDown - 2 of 2", "A12", "PayECRAccess:SoftShutDown");
				_theEndOfAccess = true;
			}
		}

		public void Dispose()
		{
			_stopPayECRProcess = true;

			lock (_payECSDataList)
			{
				Monitor.PulseAll(_payECSDataList);
			}

			//if (_ecr != null)
			//{
			//	_ecr.Stop = true;
			//	_ecr.OnInProgress -= PayWaveProgressHandle;
			//	_ecr.Dispose();
			//	_ecr = null;
			//}

			DateTime waitDelayTime = DateTime.Now.AddSeconds(60);

			Log.LogText(LogChannel, "*", "Start", "A01", "PayECRAccess.Dispose");

			// Below delay allow to wait (5 seconds) for PayECRProcessThreadWorking() function to stop;
			while ((waitDelayTime.Subtract(DateTime.Now).TotalMilliseconds > 0) && (_theEndOfAccess == false))
			{
				Thread.Sleep(300);
			}
			//--------------------------------------------------------------------------------------

			Log.LogText(LogChannel, "*", "End", "A10", "PayECRAccess.Dispose");
		}
	}

	//public class TrxCallBackEventArgs : EventArgs
	//{
	//	public string ProcessId { get; set; } = null;
	//	public bool IsSuccess { get; set; } = false;
	//	public Exception Error { get; set; } = null;
	//	public ResponseInfo Result { get; set; } = null;

	//	public TrxCallBackEventArgs Duplicate()
	//	{
	//		TrxCallBackEventArgs ev = new TrxCallBackEventArgs() { Error = this.Error, IsSuccess = this.IsSuccess, Result = this.Result };
	//		return ev;
	//	}
	//}
}