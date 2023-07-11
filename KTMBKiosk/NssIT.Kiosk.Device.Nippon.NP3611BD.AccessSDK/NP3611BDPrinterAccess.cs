using NssIT.Kiosk.Device.Nippon.NP3611BD.AccessSDK.Command;
using NssIT.Kiosk.Device.Nippon.NP3611BD.AccessSDK.Command.Working;
using NssIT.Kiosk.Device.Nippon.NP3611BD.OrgAPI;
using NssIT.Kiosk.Log.DB;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Device.Nippon.NP3611BD.AccessSDK
{
    public class NP3611BDPrinterAccess : IDisposable
	{
		private const string LogChannel = "PRINTER_NP3611BD_ACCESS";
		private DbLog _log = null;

		private Thread _printerThreadWorker = null;
		private TimeSpan _MaxWaitPeriod = new TimeSpan(0, 0, 5);

		private ConcurrentQueue<INP3611Command<INP3611CommandParameter, INP3611CommandSuccessAnswer>> _commandList 
			= new ConcurrentQueue<INP3611Command<INP3611CommandParameter, INP3611CommandSuccessAnswer>>();

		private ConcurrentDictionary<Guid, INP3611Command<INP3611CommandParameter, INP3611CommandSuccessAnswer>> _answerList 
			= new ConcurrentDictionary<Guid, INP3611Command<INP3611CommandParameter, INP3611CommandSuccessAnswer>>();

		private string _printerName = null;
		private string _finalizedPrinterName = null;
		private bool _checkPrinterPaperLowState = true;
		private bool _disposed = false;
		
		private static SemaphoreSlim _manLock = new SemaphoreSlim(1);
		private static SemaphoreSlim _answerLock = new SemaphoreSlim(1);

		private static NP3611BDPrinterAccess _printerAccess = null;

		/// <summary>
		/// Return null is api creation in progress;
		/// </summary>
		public bool? IsApiCreatedSuccessfully { get; private set; } = null;
		public Exception ApiError { get; private set; } = null;

		public DbLog Log
		{
			get
			{
				return _log ?? (_log = DbLog.GetDbLog());
			}
		}

		public static NP3611BDPrinterAccess GetPrinterAccess(string printerName, bool checkPrinterPaperLowState)
		{
			if (_printerAccess == null)
			{
				try
				{
					_manLock.WaitAsync().Wait();
					if (_printerAccess == null)
					{
						_printerAccess = new NP3611BDPrinterAccess(printerName, checkPrinterPaperLowState);
					}
					return _printerAccess;
				}
				finally
				{
					if (_manLock.CurrentCount == 0)
						_manLock.Release();
				}
			}
			else
				return _printerAccess;
		}

		public void Dispose()
		{
			if (!_disposed)
			{
				_disposed = true;

				lock (_commandList)
				{
					Log.LogText(LogChannel, "*", "Start1 - Dispose", "A01", "NP3611BDPrinterAccess.Dispose");

					try
					{
						while (_commandList.Count > 0)
							_commandList.TryDequeue(out INP3611Command<INP3611CommandParameter, INP3611CommandSuccessAnswer> x);

						Monitor.PulseAll(_commandList);
						Thread.Sleep(100);
					}
					catch { }

					Log.LogText(LogChannel, "*", "End1 - Dispose", "A05", "NP3611BDPrinterAccess.Dispose");
				}

				Log.LogText(LogChannel, "*", "Start2 - Dispose", "A06", "NP3611BDPrinterAccess.Dispose");

				bool lockSuccess = false;
				try
				{
					lockSuccess = _answerLock.WaitAsync().Wait(5000);

					if (lockSuccess)
                    {
						_answerList.Clear();
						Thread.Sleep(100);
					}
				}
				catch { }
				finally
                {
					if ((lockSuccess) && (_answerLock.CurrentCount == 0))
						_answerLock.Release();
				}

				Log.LogText(LogChannel, "*", "End2 - Dispose", "A10", "NP3611BDPrinterAccess.Dispose");
				
			}

		}

		private NP3611BDPrinterAccess(string printerName, bool checkPrinterPaperLowState)
		{
			_printerName = printerName;
			_checkPrinterPaperLowState = checkPrinterPaperLowState;

			_printerThreadWorker = new Thread(AccessThreadWorking);
			_printerThreadWorker.IsBackground = true;
			_printerThreadWorker.Start();
		}

		public string FinalizedPrinterName => _finalizedPrinterName;        

		private void AccessThreadWorking()
        {
			NP3611BDPrinter printer = null;
			Exception firstEx = null;

			///// .. LastTimePeriod to Keep the answer; keep last 5 minutes Answers
			int keepLastAnswerListPeriodSec = 60 * 5;
			DateTime? nextAnswerListCleanUpTime = null;

			try
			{
				nextAnswerListCleanUpTime = GetNextAnswerListCleanUpTime();
				while (!_disposed)
				{
					//--------------------------------------------------------------------------------------------------------------------
					// Initiate Printer API
					if ((printer is null) && (firstEx is null))
                    {
						for (int reTry = 0; reTry < 5; reTry++)
                        {
                            try
                            {
								NP3611BDPrinter api = GetPrinterAPI(_printerName, _checkPrinterPaperLowState);

								if (api is NP3611BDPrinter)
                                {
									printer = api;
									break;
								}
							}
							catch(Exception ex)
                            {
								if (firstEx is null)
									firstEx = ex;

								Thread.Sleep(300);
							}
                        }

						if (printer is null)
                        {
							if (firstEx is Exception)
								ApiError = firstEx;
							else
								ApiError = new Exception("Unknown exception when initiate printer api");

							IsApiCreatedSuccessfully = false;
						}
						else
                        {
							_finalizedPrinterName = printer.PrinterName;
							ApiError = null;
							IsApiCreatedSuccessfully = true;
						}
					}
					//--------------------------------------------------------------------------------------------------------------------
					// 
					if (GetExecCommand() is INP3611Command<INP3611CommandParameter, INP3611CommandSuccessAnswer> printerCommand)
					{
						if (_disposed)
							break;

						try
						{
							if (printer is null)
                            {
								printerCommand.ExecStatus.EndExecution(isCommandEndSuccessFul: false, new Exception("No printer API found when try to access"));
								UpdateAnswer(printerCommand);
							}
							else
                            {
								printerCommand.StartAccessSDKCommand(printer);
								UpdateAnswer(printerCommand);
							}
							
						}
						catch (Exception ex)
						{
							Log.LogText(LogChannel, "*", printerCommand, "EX01", "NP3611BDPrinterAccess.AccessThreadWorking", AppDecorator.Log.MessageType.Error,
								extraMsg: $@"{ex.Message}; MsgObj: {printerCommand.ModelTag}");
							Log.LogError(LogChannel, "*", ex, "EX02", "NP3611BDPrinterAccess.AccessThreadWorking");
						}
					}
					else
                    {
						if (nextAnswerListCleanUpTime.Value.Ticks < DateTime.Now.Ticks)
                        {
							CleanUpAnswerList(DateTime.Now.AddSeconds(keepLastAnswerListPeriodSec * -1));
							nextAnswerListCleanUpTime = GetNextAnswerListCleanUpTime();
						}
                    }
					//--------------------------------------------------------------------------------------------------------------------
				}

				Log.LogText(LogChannel, "*", "Quit AccessThreadWorking", "A20", "NP3611BDPrinterAccess.AccessThreadWorking");
			}
			catch (Exception ex)
			{
				Log.LogError(LogChannel, "*", ex, "EX10", "NP3611BDPrinterAccess.AccessThreadWorking");
			}
            finally
            {
				try
				{
					printer?.Dispose();
				}
				catch { }
			}

			return;
			//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

			NP3611BDPrinter GetPrinterAPI(string printerNameX, bool checkPrinterPaperLowStateX)
			{
				NP3611BDPrinter api = null;
				api = NssIT.Kiosk.Device.Nippon.NP3611BD.OrgAPI.NP3611BDPrinter.GetPrintQueue(printerNameX, checkPrinterPaperLowStateX);
				return api;
			}

			void UpdateAnswer(INP3611Command<INP3611CommandParameter, INP3611CommandSuccessAnswer> answer)
            {
				bool lockSuccess = false;
				try
				{
					lockSuccess = _answerLock.WaitAsync().Wait(10 * 1000);

					if (lockSuccess)
					{
						_answerList.TryAdd(answer.CommandId, answer);
					}

				}
				catch { }
				finally
				{
					if ((lockSuccess) && (_answerLock.CurrentCount == 0))
						_answerLock.Release();
				}
			}

			INP3611Command<INP3611CommandParameter, INP3611CommandSuccessAnswer> GetExecCommand()
			{
				INP3611Command<INP3611CommandParameter, INP3611CommandSuccessAnswer> retcommand = null;
				bool commandFound = false;

				if (_disposed == false)
				{
					try
					{
						lock (_commandList)
						{
							if (_commandList.Count == 0)
							{
								Monitor.Wait(_commandList, _MaxWaitPeriod);
							}

							commandFound = _commandList.TryDequeue(out retcommand);
						}
					}
					// Used to handle "_lightParamList" is null after disposed
					catch (Exception ex) { string byPassStr = ex.Message; }
				}

				if (commandFound)
					return retcommand;
				else
					return null;
			}

			DateTime GetNextAnswerListCleanUpTime()
            {
				return DateTime.Now.AddSeconds(keepLastAnswerListPeriodSec);
            }

			void CleanUpAnswerList(DateTime keepLastTime)
			{
				
				bool lockSuccess2 = false;
				try
				{
					lockSuccess2 = _answerLock.WaitAsync().Wait(10 * 1000);

					if (lockSuccess2)
					{
						//lock (_answerList)
						//{
							KeyValuePair<Guid, INP3611Command<INP3611CommandParameter, INP3611CommandSuccessAnswer>>[] ansList = _answerList.ToArray();

							foreach (KeyValuePair<Guid, INP3611Command<INP3611CommandParameter, INP3611CommandSuccessAnswer>> keyPair in ansList)
							{
								try
								{
									if ((keyPair.Value?.ExecStatus) is null)
									{
										bool tt1 = _answerList.TryRemove(keyPair.Key, out _);
										string tt2 = "debug";
									}
									else if ((keyPair.Value.ExecStatus.CommandCreatedTime.Ticks < keepLastTime.Ticks))
									{
										bool tt1 = _answerList.TryRemove(keyPair.Key, out _);
										string tt2 = "debug";
									}
								}
								catch (Exception ex)
								{
									try
									{
										bool tt1 = _answerList.TryRemove(keyPair.Key, out _);
									}
									catch { }
									string tt3 = ex.Message;
								}
							}
						//}

						//INP3611Command<INP3611CommandParameter, INP3611CommandSuccessAnswer> answerX = null;

						//Guid[] removeIdList = (from keyPair in _answerList
						//		   where (keyPair.Value?.ExecStatus.CommandCreatedTime.Ticks < keepLastTime.Ticks)
						//		   select keyPair.Value.CommandId).ToArray();

						//if (removeIdList != null)
						//	foreach (Guid commId in removeIdList)
						//	{
						//		bool tt1 = _answerList.TryRemove(commId, out answerX);
						//	}
					}

				}
				catch { }
				finally
				{
					if ((lockSuccess2) && (_answerLock.CurrentCount == 0))
						_answerLock.Release();
				}
			}

			/*
			//if (printerCommand is AutoCreatePrinterAccess accessWorker1)
			//{
			//	accessWorker1.StartAccessSDKCommand(null);
			//	if (accessWorker1.ExecStatus.IsCommandEndSuccessful)
			//  {
			//		printer = accessWorker1.Answer.PrinterApi;
			//		accessWorker1.Answer.DetachPrinterApi();
			//		UpdateAnswer((INP3611Command<INP3611CommandParameter, INP3611CommandSuccessAnswer>)accessWorker1);
			//	}
			//}
			//else if (printerCommand is ManualCreatePrinterAccess accessWorker2)
			//{
			//	accessWorker2.StartAccessSDKCommand(null);
			//	if (accessWorker2.ExecStatus.IsCommandEndSuccessful)
			//	{
			//		printer = accessWorker2.Answer.PrinterApi;
			//		accessWorker2.Answer.DetachPrinterApi();
			//		UpdateAnswer((INP3611Command<INP3611CommandParameter, INP3611CommandSuccessAnswer>)accessWorker2);
			//	}
			//}
			//else if (printerCommand is DetectDefaultPrinterNameAccess accessWorker3)
			//{
			//	accessWorker3.StartAccessSDKCommand(null);
			//	UpdateAnswer((INP3611Command<INP3611CommandParameter, INP3611CommandSuccessAnswer>) accessWorker3);
			//}
			//else
			//{
			//}
			*/
		}

		/// <summary>
		/// Return a executed result.
		/// </summary>
		/// <param name="command"></param>
		/// <param name="waitDelaySec"></param>
		/// <returns></returns>
		public INP3611Command<INP3611CommandParameter, INP3611CommandSuccessAnswer> 
			ExecCommand(INP3611Command<INP3611CommandParameter, INP3611CommandSuccessAnswer> command, int waitDelaySec = 20)
        {
            if (command == null)
                throw new Exception("Command cannot be NULL at NP3611BDPrinterAccess.ExecCommand");
			//-----------------------------

			Guid commandId = command.CommandId;

			//-----------------------------
			// Send Command to Execute
			if ((_disposed == false))
			{
				Thread threadWorker = new Thread(new ThreadStart(AppendCommandThreadWorking));
				threadWorker.IsBackground = true;
				threadWorker.Start();
			}
			//-----------------------------
			// Wait for answer
			DateTime endWaitTime = DateTime.Now.AddSeconds(waitDelaySec);

			INP3611Command<INP3611CommandParameter, INP3611CommandSuccessAnswer> answer = null;
			INP3611Command<INP3611CommandParameter, INP3611CommandSuccessAnswer> ans = null;
			bool lockSuccess = false;

			while (endWaitTime.Ticks > DateTime.Now.Ticks)
            {
				Thread.Sleep(3);
				System.Windows.Forms.Application.DoEvents();

				ans = null;

				lockSuccess = false;
				try
				{
					lockSuccess = _answerLock.WaitAsync().Wait(2000);

					if (lockSuccess)
                    {
						if (_answerList.TryGetValue(commandId, out ans) == true)
						{
							answer = ans;
							break;
						}
					}
					
				}
				catch { }
				finally
                {
					if ((lockSuccess) && (_answerLock.CurrentCount == 0))
						_answerLock.Release();
				}
            }

			//-----------------------------
			Thread threadWorker2 = new Thread(new ThreadStart(ClearAnswerThreadWorking));
			threadWorker2.IsBackground = true;
			threadWorker2.Start();
			//-----------------------------
			// Validate answer
			if (answer != null)
			{
				if (answer.ExecStatus.IsErrorFound)
				{
					if (answer.ExecStatus.ErrorObject != null)
						throw answer.ExecStatus.ErrorObject;
					else
						throw new Exception("Error when execute command on printer access");
				}
                else
                {
					return answer;
				}
			}

			//-----------------------------
			if (_disposed == false)
				throw new Exception("Timeout; Fail to execute printer access command");
			else 
				return null;

			//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

			void AppendCommandThreadWorking()
            {
				lock (_commandList)
				{
					_commandList.Enqueue(command);
					Monitor.PulseAll(_commandList);
				}
			}

			void ClearAnswerThreadWorking()
			{
				bool lockSuccess2 = false;
				try
				{
					lockSuccess2 = _answerLock.WaitAsync().Wait(10 * 1000);

					if (lockSuccess2)
					{
						bool tt1 = _answerList.TryRemove(commandId, out answer);
					}

				}
				catch { }
				finally
				{
					if ((lockSuccess2) && (_answerLock.CurrentCount == 0))
						_answerLock.Release();
				}
			}
		}
	}

	
}
