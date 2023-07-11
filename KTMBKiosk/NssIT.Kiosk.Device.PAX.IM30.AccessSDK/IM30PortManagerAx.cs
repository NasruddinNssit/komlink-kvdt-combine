using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.Base;
using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.CardEntityData.CreditDebit;
using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.CardEntityData.Sys;
using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.TransParams;
using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.TransResult;
using NssIT.Kiosk.AppDecorator.DomainLibs.KioskStatus;
using NssIT.Kiosk.AppDecorator.Global;
using NssIT.Kiosk.AppDecorator.Log;
using NssIT.Kiosk.Device.PAX.IM30.AccessSDK.Base;
using NssIT.Kiosk.Device.PAX.IM30.AccessSDK.Base.AxCommandSet;
using NssIT.Kiosk.Device.PAX.IM30.OrgAPI.Trans;
using NssIT.Kiosk.Device.PAX.IM30.OrgAPI.Trans.CardSettlement;
using NssIT.Kiosk.Device.PAX.IM30.OrgAPI.Trans.GetDeviceInfo;
using NssIT.Kiosk.Device.PAX.IM30.OrgAPI.Trans.GetLastTransaction;
using NssIT.Kiosk.Device.PAX.IM30.OrgAPI.Trans.Ping;
using NssIT.Kiosk.Device.PAX.IM30.OrgAPI.Trans.Reboot;
using NssIT.Kiosk.Device.PAX.IM30.OrgAPI.Trans.Sale;
using NssIT.Kiosk.Device.PAX.IM30.OrgAPI.Trans.StopTransaction;
using NssIT.Kiosk.Device.PAX.IM30.OrgAPI.Trans.VoidTransaction;
using NssIT.Kiosk.Log.DB;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Device.PAX.IM30.AccessSDK
{
    ///// Duplicate from KTMB Kiosk NssIT.Kiosk.Sqlite.DB.AccessDB.DatabaseAx

    /// <summary>
    /// ClassCode:EXIT28.01; Card Reader Port Manager
    /// </summary>
    public class IM30PortManagerAx : IDisposable 
    {
		private const string _logChannel = "IM30_PortManagerAx";
        public const int SettlementMaxWaitTimeSec = 60 * 60 * 3;

		private DbLog _log = null;

		private string _processId = "*";

		private Thread _accessThreadWorker = null;
		private TimeSpan _MaxWaitPeriod = new TimeSpan(0, 0, 1);

		private StartCardTransAxComm _startCardTransactionAxComm = null;
		private I2ndCardCommandParam _2ndCardCommandParam = null;
		private IIM30Trans _currentTrans = null;
		private IIM30TransResult _cardSaleTransResult = null;
		private IIM30TransResult _finalResult = null;
		private IM30DataModel _currentCardInfo = null;

        private object _transLock = new object();
        private object _2ndCardCommandParamLock = new object();

        private ShowMessageLogDelg _debugShowMessageDelgHandler = null;
		//private OnTransactionFinishedDelg _onTransactionFinishedHandler = null;
		private OnCardDetectedDelg _onLocalCardDetectedHandler = null;

		private OnTransactionFinishedDelg _onClientTransactionFinishedHandler = null;
		private OnCardDetectedDelg _onClientCardDetectedHandler = null;

		private ConcurrentDictionary<Guid, IIM30TransResult> _answerList
			= new ConcurrentDictionary<Guid, IIM30TransResult>();

		//private const int _minCommandIntervalMillisec = 3000;
		/////CYA-TESTING .. 
		private const int _minCommandIntervalMillisec = 1500;
        private DateTime _nextNewTransactionTime = DateTime.Now;
        private bool _disposed = false;
		private bool _is2ndParamHasBeenUtilized = false;
		private bool _isOnClientTransactionFinishedEventHasBeenHandled = false;

        public bool IsReaderInitializing { get; private set; } = true;

        public bool IsPortMalfunction { get; private set; }

		private bool? _isAllowed2ndCardCommandParam = null;
		private bool _isAllowNewTrans = true;

		private static ConcurrentDictionary<string, IM30PortManagerAx> _im30PortManList = null;
        private IsCardMachineDataCommNormalDelg _statusMon_IsCardMachineDataCommNormalDelgHandler = null;

        /// <summary>
        /// Return null is api creation in progress;
        /// </summary>
        public bool? IsApiCreatedSuccessfully { get; private set; } = null;

		public bool? IsMaintenanceInProgress { get; private set; } = null;

		public string COMPort { get; private set; } = null;
		public Exception ApiError { get; private set; } = null;
		public GetDeviceInfoResp GetDeviceInfo { get; private set; } = null;

        public IIM30TransResult FinalResult
		{
			get
			{
				if (_currentTrans is null)
					return null;

				if (_currentTrans.IsCurrentWorkingEnded == false)
					return null;

				return _finalResult;
			}
		}

		public bool? IsCurrentWorkingEnded
		{
			get
			{
				if (_currentTrans is null)
					return null;

				return _currentTrans.IsCurrentWorkingEnded;
			}
		}

		private bool _isManThreadWorkerHasStart = false;

        private Thread _manThreadWorker = null;
		private IM30PortManagerAx(string comPort)
        {
			_log = DbLog.GetDbLog();
            COMPort = comPort.ToUpper().Trim();
			//_onTransactionFinishedHandler = new OnTransactionFinishedDelg(OnTransactionFinishedDelgWorking);
			_onLocalCardDetectedHandler = new OnCardDetectedDelg(OnLocalCardDetectedWorking);

			_manThreadWorker = new Thread(new ThreadStart(ManagerThreadWorking))
											{ IsBackground = true, Priority = ThreadPriority.AboveNormal };
			_manThreadWorker.Start();

			Thread initWorker = new Thread(new ThreadStart(PortInitThreadWorking)) 
				{ IsBackground = true, Priority = ThreadPriority.AboveNormal };
			initWorker.Start();
        }

        public void SetOnClientTransactionFinishedHandler(OnTransactionFinishedDelg onClientTransactionFinishedHandler)
        {
			_onClientTransactionFinishedHandler = onClientTransactionFinishedHandler;
		}

		public void SetOnClientCardDetectedHandler(OnCardDetectedDelg onClientCardDetectedHandler)
		{
			_onClientCardDetectedHandler = onClientCardDetectedHandler;
		}

		public void SetOnDebugShowMessageHandler(ShowMessageLogDelg onDebugShowMessageDelgHandler)
		{
			_debugShowMessageDelgHandler = onDebugShowMessageDelgHandler;
		}

        public void SetStatusMonitor(IsCardMachineDataCommNormalDelg statusMon_IsCardMachineDataCommNormalDelgHandler)
        {
            _statusMon_IsCardMachineDataCommNormalDelgHandler = statusMon_IsCardMachineDataCommNormalDelgHandler;
        }

        private void SendStatusMonitor(KioskCommonStatus status, string remark)
        {
            _statusMon_IsCardMachineDataCommNormalDelgHandler?.Invoke(status, remark);
        }

        public bool IsReaderStandingBy(out bool isDisposed, out bool isMalFunction)
        {
            isDisposed = false;
            isMalFunction = false;
            
            if (_isDisposed)
            {
                isDisposed = _isDisposed;
                return false;
            }
            else if (IsPortMalfunction)
            {
                isMalFunction = IsPortMalfunction;
                return false;
            }
            else if (IsReaderInitializing)
            {
                return false;
            }
            else if (IsMaintenanceInProgress == true)
            {
                return false;
            }
            else if (_currentTrans?.IsCurrentWorkingEnded == false)
            {
                return false;
            }
            else if (_isAllowNewTrans == false)
            {

                return false;
            }
            return true;
        }

		/// <summary>
		/// 2 Commands transactions
		/// </summary>
		/// <param name="startAxComm"></param>
		/// <param name="isPendingOutstandingTransaction"></param>
		/// <param name="isStartSuccessful"></param>
		/// <param name="isNAKBusy"></param>
		/// <param name="error"></param>
		/// <returns>Return true if start success full</returns>
		public bool StartCardSaleTrans(
            StartCardTransAxComm startAxComm,
			out bool isPendingOutstandingTransaction, 
			out Exception error)
        {
			isPendingOutstandingTransaction = true;
			bool isStartSuccessful = false;
			error = null;

			if (_isDisposed)
            {
				error = new Exception("-Card Reader Port has been disposed~Start Card Sale Trans~Start Card Sale Trans.");
				return false;
			}

			else if (IsReaderInitializing)
			{
                error = new Exception("-Card Reader Port is initializing (A). Please wait~");
                return false;
            }

			else if (IsMaintenanceInProgress == true)
			{
                error = new Exception("-Card Reader Port under maintenance (A). Please wait~");
                return false;
            }

			else if (IsPortMalfunction)
            {
				error = new Exception("-Card Reader Port is malfunction. Invalid COM Port, or application needs to be restarted~Start Card Sale Trans.");
				return false;
			}

			else if (startAxComm is null)
            {
				error = new Exception("-Invalid parameter for Start Card Reader Transaction~");
				return false;
			}
			else if (_currentTrans?.IsCurrentWorkingEnded == false)
            {
				error = new Exception("-Card Reader busy with previous transaction (A)~");
				return false;
			}
			else if (_isAllowNewTrans == false)
            {
				error = new Exception("-Card Reader busy with a transaction (B)~May need to wait for Internal Maintenance");
				return false;
			}

			isPendingOutstandingTransaction = false;

            try
            {
				_currentTrans?.ShutdownX();
				_is2ndParamHasBeenUtilized = false;
				_startCardTransactionAxComm = startAxComm;
                _processId = $@"NewSale_{DateTime.Now:dd_HHmmss_fff}";

                _log.LogText(_logChannel, _processId, _startCardTransactionAxComm, "P01:StartCardSaleTrans", "IM30PortManagerAx.StartCardSaleTrans");
				
				lock(_transLock)
                {
                    _isAllowed2ndCardCommandParam = null;
                    _2ndCardCommandParam = null;
                    _finalResult = null;
					_currentCardInfo = null;
					_isOnClientTransactionFinishedEventHasBeenHandled = false;
                    _isAllowNewTrans = false;

                    _currentTrans = new IM30CardSale(
						COMPort, 
						null, _onLocalCardDetectedHandler,
						_processId, 
						startAxComm.GateDirection, startAxComm.KomLinkFirstSPNo, startAxComm.KomLinkSecondSPNo,
                        startAxComm.MaxCardDetectedWaitingTimeSec,
						startAxComm.MaxSaleDecisionWaitingTimeSec,
						_debugShowMessageDelgHandler);
					
					Monitor.PulseAll(_transLock);
                }

				///// Wait Transaction to start successfully -- Check ACK ------------------------
				Thread tWorker1 = new Thread(new ThreadStart(new Action(() =>
				{
					DateTime tOutS1 = DateTime.Now.AddSeconds(8);
					while ((_currentTrans.IsTransStartSuccessful.HasValue == false)
						&& (tOutS1.Ticks > DateTime.Now.Ticks)
					)
					{
						Thread.Sleep(10);
					}
				})))
				{ IsBackground = true, Priority = ThreadPriority.AboveNormal };
				tWorker1.Start();
				tWorker1.Join();

				///// Check Transaction Starting status ------------------------------------------
				if ((_currentTrans.IsTransStartSuccessful.HasValue == false)
					||
					(_currentTrans.IsTransStartSuccessful == false)
					)
				{
					isStartSuccessful = false;
                    _log.LogText(_logChannel, _processId, "Sale Start Successfull", "B11", "IM30PortManagerAx.StartCardSaleTrans");
                }
				else
				{
					_isAllowed2ndCardCommandParam = true;
					isStartSuccessful = true;

                    _log.LogText(_logChannel, _processId, "Fail to Start new Sale", "B13", "IM30PortManagerAx.StartCardSaleTrans");
                }
				if (isStartSuccessful)
					return true;

				/////  When isStartSuccessful is false
				if (_currentTrans.FinalResult?.Error != null)
                {
					error = new Exception($@"-Fail to start Reader Card Transaction~at Start Card Sale Trans.; {_currentTrans.FinalResult.Error.Message}", _currentTrans.FinalResult.Error);
				}
				else if ((_currentTrans.FinalResult != null) && (_currentTrans.FinalResult.IsTimeout))
                {
					error = new Exception("-Timeout when start Reader Card Transaction~at Start Card Sale Trans.");
				}
				else if ((_currentTrans.FinalResult != null) && (_currentTrans.FinalResult.IsManualStopped))
				{
					error = new Exception("-Start Reader Card Transaction object has been disposed~at Start Card Sale Trans.");
				}
				else
					error = new Exception("-Unknown error when Start Reader Card Transaction~at Start Card Sale Trans.");

				return false;
			}
			catch (Exception ex)
            {
                _log.LogError(_logChannel, _processId, ex, "EX01", "IM30PortManagerAx.StartCardSaleTrans");

                if (error is null)
					error = new Exception($@"-Error when Start Ax Card Sale Trans~{ex.Message}", ex);
			}

			return false;
		}

		private void OnClientTransactionFinishedDelgWorking(IIM30TransResult finalResult)
        {
            if (IsReaderInitializing)
                return;

            if (_isOnClientTransactionFinishedEventHasBeenHandled == false)
			{
				if (_onClientTransactionFinishedHandler != null)
                {
					try
                    {
                        _log.LogText(_logChannel, _processId, finalResult, "A01", "IM30PortManagerAx.OnClientTransactionFinishedDelgWorking");

                        _onClientTransactionFinishedHandler.Invoke(finalResult);
					}
					catch (Exception ex)
                    {
                        _log.LogError(_logChannel, _processId, ex, "EX01", "IM30PortManagerAx.OnClientTransactionFinishedDelgWorking");
                    }
				}
				_isOnClientTransactionFinishedEventHasBeenHandled = true;
			}
		}

		private void OnLocalCardDetectedWorking(IM30DataModel cardInfo)
		{
			if (IsReaderInitializing)
				return;

            OnCardDetectedDelg onClientCardDetectedDelgHandler = _onClientCardDetectedHandler;

            if (onClientCardDetectedDelgHandler == null)
                return;

			else if (cardInfo is null)
                return;

            _currentCardInfo = cardInfo;

            Thread tWorker = new Thread(new ThreadStart(new Action(() =>
            {
                try
                {
                    onClientCardDetectedDelgHandler?.Invoke(cardInfo);
                }
                catch (Exception ex)
                {
                    _log.LogError(_logChannel, _processId, ex, "EX01", "IM30PortManagerAx.OnLocalCardDetectedWorking");
                }
                finally
                {
                    onClientCardDetectedDelgHandler = null;
                }
            })))
            {
                IsBackground = true,
                Priority = ThreadPriority.AboveNormal
            };
            tWorker.Start();
        }

		public IIM30TransResult RunSoloCommand(IIM30AxCommand axCommand,
            out bool isPendingOutstandingTransaction,
            out Exception error)
		{
            isPendingOutstandingTransaction = true;
            bool isStartSuccessful = false;
            IIM30Trans newTrans = null;
            error = null;

            if (_isDisposed)
            {
                error = new Exception("-Card Reader Port has been disposed~Run Single Command Trans.~.");
                return null;
            }
            else if (IsReaderInitializing)
            {
                error = new Exception("-Card Reader Port is initializing (B). Please wait~");
                return null;
            }
            else if (IsMaintenanceInProgress == true)
            {
                error = new Exception("-Card Reader Port under maintenance (B). Please wait~");
                return null;
            }
            else if (IsPortMalfunction)
            {
                error = new Exception("-Card Reader Port is malfunction. Invalid COM Port, or application needs to be restarted~Run Single Command Trans.");
                return null;
            }
            
            else if (_currentTrans?.IsCurrentWorkingEnded == false)
            {
                error = new Exception("-Card Reader busy with previous transaction (K)~");
                return null;
            }

			isPendingOutstandingTransaction = false;

            if (_isAllowNewTrans == false)
            {
                error = new Exception("-Card Reader busy with a transaction (K)~May need to wait for Internal Maintenance");
                return null;
            }
			else
			{
                if (axCommand is null)
                {
                    error = new Exception("-Invalid Single Card Reader Command~");
                    return null;
                }
                else if (axCommand is StartCardTransAxComm)
                {
                    error = new Exception("-Start Card Trans. not supported in Run Single Command. Please use StartCardSaleTrans(..) method~");
                    return null;
                }

                try
                {
                    _log.LogText(_logChannel, _processId, axCommand, "P01:RunSoloCommand", "IM30PortManagerAx.RunSoloCommand");

                    if (axCommand is CardSettlementAxComm pComm1)
                    {
                        _processId = $@"SoloCardComd_{DateTime.Now:dd_HHmmss_fff}";
                        newTrans = new IM30CardSettlement(COMPort, _processId.ToString(), null
                                        , noActionMaxWaitSec: SettlementMaxWaitTimeSec, _debugShowMessageDelgHandler);
                    }
                    else if (axCommand is GetDeviceInfoAxComm pComm2)
                    {
                        _processId = $@"SoloCardComd_{DateTime.Now:dd_HHmmss_fff}";
                        newTrans = new IM30GetDeviceInfo(COMPort, _processId.ToString(), null
                                        , noActionMaxWaitSec: 20, _debugShowMessageDelgHandler);
                    }
                    else if (axCommand is GetLastTransAxComm pComm3)
                    {
                        _processId = $@"SoloCardComd_{DateTime.Now:dd_HHmmss_fff}";
                        newTrans = new IM30GetLastTransaction(COMPort, _processId.ToString(), null
                                        , noActionMaxWaitSec: 20, _debugShowMessageDelgHandler);
                    }
                    else if (axCommand is PingAxComm pComm4)
                    {
                        _processId = $@"SoloCardComd_{DateTime.Now:dd_HHmmss_fff}";
                        newTrans = new IM30Ping(COMPort, null, _processId.ToString()
                                    , noActionMaxWaitSec: 10, _debugShowMessageDelgHandler);
                    }
                    else if (axCommand is RebootAxComm pComm5)
                    {
                        _processId = $@"SoloCardComd_{DateTime.Now:dd_HHmmss_fff}";
                        newTrans = new IM30Reboot(COMPort, null, _processId.ToString()
                        , noActionMaxWaitSec: 10, _debugShowMessageDelgHandler);
                    }
                    else if (axCommand is StopTransactionAxComm pComm6)
                    {
                        _processId = $@"SoloCardComd_{DateTime.Now:dd_HHmmss_fff}";
                        newTrans = new IM30StopTransaction(COMPort, _processId.ToString(), null
                        , noActionMaxWaitSec: 15, _debugShowMessageDelgHandler);
                    }
                    else if (axCommand is VoidTransactionAxComm pComm7)
                    {
                        _processId = $@"SoloCardComd_{DateTime.Now:dd_HHmmss_fff}";
                        newTrans = new IM30VoidTransaction(COMPort, pComm7.InvoiceNo, pComm7.CardToken, pComm7.VoidAmount,
                            null,
                            noActionMaxWaitSec: 60, _debugShowMessageDelgHandler);
                    }
                }
                catch (Exception ex)
                {
                    _log.LogError(_logChannel, _processId, ex, "EX01", "IM30PortManagerAx.RunSoloCommand");

                    error = new Exception("-Error when init. new Card Reader transaction (Solo) command~", ex);
                    return null;
                }
            }

			if (newTrans is null)
			{
                error = new Exception("-Card Reader Command Type not supported in Reader Access~");
                return null;
            }

            try
            {
                _currentTrans?.ShutdownX();
				Guid processId = Guid.NewGuid();

                lock (_transLock)
                {
                    _finalResult = null;
					_isAllowed2ndCardCommandParam = false;
                    _isOnClientTransactionFinishedEventHasBeenHandled = false;
                    _isAllowNewTrans = false;

                    _currentTrans = newTrans;
                    Monitor.PulseAll(_transLock);
                }

                _log.LogText(_logChannel, _processId, $@"Trans.{_currentTrans?.GetType().Name} has been started", "S01", "IM30PortManagerAx.RunSoloCommand");

                ///// Wait Transaction to start successfully in ManagerThreadWorking(..) -- Check ACK ------------------------
                Thread tWorker1 = new Thread(new ThreadStart(new Action(() =>
                {
                    DateTime tOutS1 = DateTime.Now.AddSeconds(20);
                    while ((_currentTrans.IsTransStartSuccessful.HasValue == false)
                        && (tOutS1.Ticks > DateTime.Now.Ticks)
                    )
                    {
                        Thread.Sleep(10);
                    }
                })))
                { IsBackground = true, Priority = ThreadPriority.AboveNormal };
                tWorker1.Start();
                tWorker1.Join();

                ///// Check Transaction Starting status ------------------------------------------
                if ((_currentTrans.IsTransStartSuccessful.HasValue == false)
                    ||
                    (_currentTrans.IsTransStartSuccessful == false)
                    )
                {
                    isStartSuccessful = false;
                }
                else
                {
                    _isAllowed2ndCardCommandParam = false;
                    isStartSuccessful = true;
                }

				if (isStartSuccessful == false)
				{
                    error = new Exception("-Unable to receive positive response from Card Reader (Ax)~");
                    return null;
                }

                ///// Settlement use delegation/event to receive data xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                if (newTrans is IM30CardSettlement)
				{
                    return new IM30CardSettlementStartedResult();
				}

                ///// Immediate Response for 2nd Command Parameter xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                // CYA-DEBUG .. need to handle timeout for below code
                while ((_isDisposed == false) && (_currentTrans.IsCurrentWorkingEnded == false))
                {
                    Thread.Sleep(100);
                }
                if (_isDisposed)
                {
                    if (error == null)
                        error = new Exception($@"-Reader Port Manager object disposed when sending 2nd (Ax)  Command parameter for Card Sale Transaction~");
                    return null;
                }
                //--------------------------------------------------------------------

                if (_currentTrans.IsCurrentWorkingEnded)
                {
                    //CYA-DEBUG .. need to handle timeout for below code
                    while ((_isDisposed == false) && (_finalResult == null))
                    {
                        Thread.Sleep(100);
                    }

                    if (_isDisposed)
                    {
                        if (error == null)
                            error = new Exception($@"-Reader Port Manager object disposed when sending 2nd Command parameter for Card Sale (Ax) Transaction~");
                        return null;
                    }

                    if (_finalResult != null)
                    {
                        return _finalResult;
                    }
                    else
                    {
                        if (error == null)
                            error = new Exception($@"-Timeout Card Reader (Ax) Transaction (END-2)~");
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                _log.LogError(_logChannel, _processId, ex, "EX01", "IM30PortManagerAx.RunSoloCommand");

                if (error is null)
                    error = new Exception($@"-Error when Start Ax Card Sale Trans~{ex.Message}", ex);
            }

            if (error == null)
                error = new Exception($@"-Unknown error when execute Single Card Reader (Ax) Command. (END)~");

            return null;
        }

        private IIM30TransResult PortInitCommand(out Exception error)
        {
            bool isStartSuccessful = false;
            IIM30Trans newTrans = null;
            error = null;

            if (_isDisposed)
            {
                error = new Exception("-Card Reader Port has been disposed~Port Init.~.");
                return null;
            }
            else if (IsMaintenanceInProgress == true)
            {
                error = new Exception("-Card Reader Port under maintenance (B). Please wait~");
                return null;
            }
            else if (IsPortMalfunction)
            {
                error = new Exception("-Card Reader Port is malfunction. Invalid COM Port, or application needs to be restarted~Port Init.");
                return null;
            }

            else if (_currentTrans?.IsCurrentWorkingEnded == false)
            {
                error = new Exception("-Card Reader busy with previous transaction (K)~Port Init.");
                return null;
            }

            if (_isAllowNewTrans == false)
            {
                error = new Exception("-Card Reader busy with a transaction (K)~May need to wait for Internal Maintenance; Port Init.");
                return null;
            }
            else
            {
                    _processId = $@"InitReader_{DateTime.Now:dd_HHmmss_fff}";
                    newTrans = new IM30GetDeviceInfo(COMPort, _processId.ToString(), null
                                    , noActionMaxWaitSec: 20, _debugShowMessageDelgHandler);
            }

            if (newTrans is null)
            {
                error = new Exception("-Card Reader Command Type not supported in Reader Access~Port Init.");
                return null;
            }

            try
            {
                _currentTrans?.ShutdownX();
                Guid processId = Guid.NewGuid();

                lock (_transLock)
                {
                    _finalResult = null;
                    _isAllowed2ndCardCommandParam = false;
                    _isOnClientTransactionFinishedEventHasBeenHandled = false;
                    _isAllowNewTrans = false;

                    _currentTrans = newTrans;
                    Monitor.PulseAll(_transLock);
                }

                ///// Wait Transaction to start successfully in ManagerThreadWorking(..) -- Check ACK ------------------------
                Thread tWorker1 = new Thread(new ThreadStart(new Action(() =>
                {
                    DateTime tOutS1 = DateTime.Now.AddSeconds(20);
                    while ((_currentTrans.IsTransStartSuccessful.HasValue == false)
                        && (tOutS1.Ticks > DateTime.Now.Ticks)
                    )
                    {
                        Thread.Sleep(10);
                    }
                })))
                { IsBackground = true, Priority = ThreadPriority.AboveNormal };
                tWorker1.Start();
                tWorker1.Join();

                ///// Check Transaction Starting status ------------------------------------------
                if ((_currentTrans.IsTransStartSuccessful.HasValue == false)
                    ||
                    (_currentTrans.IsTransStartSuccessful == false)
                    )
                {
                    isStartSuccessful = false;
                    _log.LogError(_logChannel, _processId, new Exception("Fail to start GetDeviceInfo"), "B01", "IM30PortManagerAx.PortInitCommand");
                }
                else
                {
                    _isAllowed2ndCardCommandParam = false;
                    isStartSuccessful = true;
                    _log.LogError(_logChannel, _processId, new Exception("Start GetDeviceInfo successful"), "B05", "IM30PortManagerAx.PortInitCommand");
                }

                if (isStartSuccessful == false)
                {
                    error = new Exception("-Unable to receive ACK. response from Card Reader (Ax)~Port Init.");
                    return null;
                }

                ///// Immediate Response for 2nd Command Parameter xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                // CYA-DEBUG .. need to handle timeout for below code
                while ((_isDisposed == false) && (_currentTrans.IsCurrentWorkingEnded == false))
                {
                    Thread.Sleep(100);
                }
                if (_isDisposed)
                {
                    if (error == null)
                        error = new Exception($@"-Reader Port Manager object disposed when sending 2nd (Ax)  Command parameter for Card Sale Transaction~Port Init.");
                    return null;
                }
                //--------------------------------------------------------------------

                if (_currentTrans.IsCurrentWorkingEnded)
                {
                    //CYA-DEBUG .. need to handle timeout for below code
                    while ((_isDisposed == false) && (_finalResult == null))
                    {
                        Thread.Sleep(100);
                    }

                    if (_isDisposed)
                    {
                        if (error == null)
                            error = new Exception($@"-Reader Port Manager object disposed when sending 2nd Command parameter for Card Sale (Ax) Transaction~Port Init.");
                        return null;
                    }

                    if (_finalResult != null)
                    {
                        return _finalResult;
                    }
                    else
                    {
                        if (error == null)
                            error = new Exception($@"-Timeout Card Reader (Ax) Transaction (END-2)~Port Init.");
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                _log.LogError(_logChannel, _processId, ex, "EX01", "IM30PortManagerAx.PortInitCommand");

                if (error is null)
                    error = new Exception($@"-Error when Start Ax Card Sale Trans~Port Init.#{ex.Message}", ex);
            }

            if (error == null)
                error = new Exception($@"-Unknown error when execute Single Card Reader (Ax) Command. (END)~Port Init.");

            return null;
        }

        private bool CheckIsTnGTransactionAllowed(out Exception error)
        {
            error = null;

            if (_isDisposed)
            {
                error = new Exception($@"-Reader Port Manager may already disposed~COM Port: {COMPort}");
                return false;
            }
            else if (GetDeviceInfo is null)
            {
                error = new Exception($@"-IM30PortMan's Reader Info not able to find~COM Port: {COMPort}");
                return false;
            }
            else if (string.IsNullOrWhiteSpace(GetDeviceInfo.TnGSamIdOperatorID))
            {
                error = new Exception($@"-Invalid TnG SAM Card's Operator ID~COM Port: {COMPort}; The SAM card may not exist in the Card Reader");
                return false;
            }
            else if (GetDeviceInfo.TnGSamIdStationCode < 0)
            {
                error = new Exception($@"-Invalid TnG SAM Card's Station Code~COM Port: {COMPort}; The SAM card may not exist in the Card Reader");
                return false;
            }
            else if (GetDeviceInfo.TnGSamIdGateNo < 0)
            {
                error = new Exception($@"-Invalid TnG SAM Card's Gate No.~COM Port: {COMPort}; The SAM card may not exist in the Card Reader");
                return false;
            }
            return true;
        }

        /// <summary>
        /// </summary>
        /// <param name="commParam"></param>
        /// <param name="isTransNotStarted"></param>
        /// <param name="isTransHasAlreadyEnded"></param>
        /// <param name="isNotAbleToSendParam"></param>
        /// <param name="isFinalResultCollectFromEvent">This value valid onlt when return is true</param>
        /// <param name="transResult"></param>
        /// <returns>Return true when parameter has sent successfully</returns>
        public bool Send2ndCommandParameter(
			I2ndCardCommandParam commParam, 
			out bool? isFinalResultCollectFromEvent,
			out IIM30TransResult transResult,
			out Exception error)
		{
			isFinalResultCollectFromEvent = null;
			transResult = null;
			error = null;

			bool? isReqToWaitForImmediateResult_Checking = null;

            try
            {

                if (_isDisposed)
                {
                    error = new Exception("-Card Reader Port Manager has been closed~");
                    return false;
                }

                else if (IsReaderInitializing)
                {
                    error = new Exception("-Card Reader Port is initializing (C). Please wait~");
                    return false;
                }

                else if (IsPortMalfunction)
                {
                    error = new Exception("-Card Reader Port is malfunction. Invalid COM Port, or application needs to be restarted~Start Card Sale Trans~Send 2nd Command Parameter");
                    return false;
                }

                else if (_currentTrans is null)
                {
                    error = new Exception("-Card Sale Transaction not found when sending 2nd parameter~");
                    return false;
                }

                else if (_currentTrans.IsCurrentWorkingEnded == true)
                {
                    error = new Exception("-Previous Card Sale Transaction has ended when sending 2nd parameter~");
                    return false;
                }

                else if (_currentTrans.IsTransStartSuccessful == false)
                {
                    error = new Exception("-Sending 2nd card parameter denied to Card Reader Port Manager. Sale Transaction may not start successful~");
                    return false;
                }

                else if ((_currentTrans is IM30CardSale) == false)
                {
                    error = new Exception("-2nd command parameter not accepted, The current active command is not Card Sale Command~");
                    return false;
                }

                else if (_2ndCardCommandParam != null)
                {
                    error = new Exception($@"-2nd Card Sale Command has already existed. Sending deny~Existing Param.: {_2ndCardCommandParam.GetType().Name}");
                    return false;
                }

                else if (_isAllowed2ndCardCommandParam.HasValue == false)
                {
                    error = new Exception("-Not ready to accept Card Sale Transaction for 2nd parameter~Reader busy; Start Trans. still initiating. Please try again");
                    return false;
                }

                else if (_isAllowed2ndCardCommandParam == false)
                {
                    error = new Exception("-Card Sale Transaction may not start successfully~Parameter not accepted");
                    return false;
                }

                else if ((_currentTrans is IM30CardSale) == false)
                {
                    error = new Exception("-The current active transaction is not a Card Sale Transaction~");
                    return false;
                }

                else /* if (_isAllowed2ndCardCommandParam == true) */
                {
                    if (_currentTrans is IM30CardSale)
                    {
                        Exception errX = null;
                        bool? isFinalResultCollectFromEventX = null;
                        bool isImmediateReturnTrue = false;

                        //CheckIsTnGTransactionAllowed
                        if ((commParam is TnGEntryCheckinParam)
                            ||
                            (commParam is TnGExitCheckoutParam)
                            )
                        {
                            if (CheckIsTnGTransactionAllowed(out Exception errG) == false)
                            {
                                error = errG;
                                return false;
                            }
                        }

                        _log.LogText(_logChannel, _processId, commParam, "P01:Send2ndCommandParameter", "IM30PortManagerAx.Send2ndCommandParameter");

                        Thread tWork1 = new Thread(new ThreadStart(new Action(() =>
                        {
                            lock (_2ndCardCommandParamLock)
                            {
                                if (_isAllowed2ndCardCommandParam == false)
                                {
                                    errX = new Exception("-Card Sale Transaction may already stopped~Parameter not accepted");
                                }
                                else if (commParam is CreditDebitChargeParam)
                                {
                                    _processId = commParam.PosTransId;
                                    _2ndCardCommandParam = commParam;
                                    _isAllowed2ndCardCommandParam = false;
                                    isFinalResultCollectFromEventX = true;
                                    isReqToWaitForImmediateResult_Checking = false;
                                    isImmediateReturnTrue = true;
                                }
                                else if (commParam is TnGEntryCheckinParam)
                                {
                                    _processId = commParam.PosTransId;
                                    _2ndCardCommandParam = commParam;
                                    _isAllowed2ndCardCommandParam = false;
                                    isFinalResultCollectFromEventX = false;
                                    isReqToWaitForImmediateResult_Checking = true;
                                }
                                else if (commParam is TnGExitCheckoutParam)
                                {
                                    _processId = commParam.PosTransId;
                                    _2ndCardCommandParam = commParam;
                                    _isAllowed2ndCardCommandParam = false;
                                    isFinalResultCollectFromEventX = false;
                                    isReqToWaitForImmediateResult_Checking = true;
                                }
                                else if (commParam is StopCardTransParam)
                                {
                                    _processId = $@"StopTrans_{DateTime.Now:dd_HHmmss_fff}";
                                    _2ndCardCommandParam = commParam;
                                    _isAllowed2ndCardCommandParam = false;
                                    isFinalResultCollectFromEventX = false;
                                    isReqToWaitForImmediateResult_Checking = true;
                                }
                                else
                                {
                                    errX = new Exception($@"-Unsupported Card Sale Transaction to 2nd Command parameter~Param (B)~Name: {commParam.GetType().Name}");
                                }
                            }
                        })))
                        { IsBackground = true, Priority = ThreadPriority.AboveNormal };
                        tWork1.Start();
                        tWork1.Join();

                        error = errX;
                        isFinalResultCollectFromEvent = isFinalResultCollectFromEventX;

                        if (isImmediateReturnTrue)
                            return true;

                        if (error != null)
                            return false;
                    }
                    else
                    {
                        error = new Exception($@"-Transaction not expects 2nd Command parameter~Transaction Name: {_currentTrans.GetType().Name}");
                        return false;
                    }
                }

                if ((isReqToWaitForImmediateResult_Checking.HasValue == false) || (isReqToWaitForImmediateResult_Checking == false))
                {
                    if (error == null)
                        error = new Exception($@"-Fatal error when sending 2nd Command parameter for Card Sale Transaction~");
                    return false;
                }

                ///// Immediate Response for 2nd Command Parameter xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                // CYA-DEBUG .. need to handle timeout for below code
                while ((_isDisposed == false) && (_currentTrans.IsCurrentWorkingEnded == false))
                {
                    Thread.Sleep(100);
                }
                if (_isDisposed)
                {
                    if (error == null)
                        error = new Exception($@"-Reader Port Manager object disposed when sending 2nd Command parameter for Card Sale Transaction~");
                    return false;
                }
                //--------------------------------------------------------------------

                if (_currentTrans.IsCurrentWorkingEnded)
                {
                    //CYA-DEBUG .. need to handle timeout for below code
                    while ((_isDisposed == false) && (_finalResult == null))
                    {
                        Thread.Sleep(20);
                    }

                    if (_isDisposed)
                    {
                        if (error == null)
                            error = new Exception($@"-Reader Port Manager object disposed when sending 2nd Command parameter for Card Sale Transaction~");
                        return false;
                    }

                    if (_finalResult != null)
                    {
                        transResult = _finalResult;
                        return true;
                    }
                    else
                    {
                        if (error == null)
                            error = new Exception($@"-Timeout for Card Sale Transaction (END-2)~");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                _log.LogError(_logChannel, _processId, ex, "EX01", "IM30PortManagerAx.Send2ndCommandParameter");

                if (error == null)
                    error = new Exception($@"-Fatal error when sending 2nd Command parameter for Card Sale Transaction (END-A)~", ex);
            }

			if (error == null)
				error = new Exception($@"-Fatal error when sending 2nd Command parameter for Card Sale Transaction (END-B)~");
			return false;
		}

		public static IM30PortManagerAx GetAxPortManager(string comPort)
		{
			string port = comPort.Trim().ToUpper();
			IM30PortManagerAx outIM30Ax = null;

			if (_im30PortManList?.TryGetValue(port, out outIM30Ax) == true)
            {
				return outIM30Ax;
			}
			else
            {
				Thread tWorker = new Thread(new ThreadStart(new Action(() =>
				{
					lock(SysGlobalLock.IM30Lock)
                    {
						if (_im30PortManList is null)
							_im30PortManList = new ConcurrentDictionary<string, IM30PortManagerAx>();

                        if (_im30PortManList.TryGetValue(port, out outIM30Ax) == false)
                        {
							outIM30Ax = new IM30PortManagerAx(port);
							_im30PortManList.TryAdd(port, outIM30Ax);
                        }
					}
				})))
				{ 
					IsBackground = true, 
					Priority = ThreadPriority.AboveNormal 
				};
				tWorker.Start();
				tWorker.Join();
			}

			return outIM30Ax;
		}

		private void ManagerThreadWorking()
        {
            try
            {
				IM30CardSale currCardSale = null;
                StartCardTransAxComm startTransAxComm = null;

				Guid lastWorkingId = Guid.Empty;

				bool isNewTransactionFound = false;
				bool isProceedTrans = true;
				bool is2ndCommandParamHasSent = false;
				bool isReqStopTransWithMaintenance = false;
				bool isReqGetLastTrans = false;
                bool isStopCommandHasBeenSent = false;

                IsMaintenanceInProgress = false;

				while (_isDisposed == false)
                {
					_isManThreadWorkerHasStart = true;
                    isNewTransactionFound = false;
					///// Get New Transaction
					lock (_transLock)
                    {
                        if ((_currentTrans == null) || (_currentTrans?.WorkingId == lastWorkingId))
                        {
                            lock (_transLock)
                            {
                                Monitor.Wait(_transLock, 500);
                            }
                        }
                    }

					try
                    {
						///// Execute New Transaction
						if ((_currentTrans != null) && (_currentTrans.WorkingId != lastWorkingId))
						{
                            isStopCommandHasBeenSent = false;
                            isNewTransactionFound = true;
							lastWorkingId = _currentTrans.WorkingId;
                            startTransAxComm = null;
							currCardSale = null;

							isProceedTrans = true;
							is2ndCommandParamHasSent = false;
							isReqStopTransWithMaintenance = false;
							isReqGetLastTrans = false;

							/////###############################################################################################################################################################
							///// Card Reader Transaction ######################################################################################################################################
							try
							{
                                //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                                //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                                //Execute Card Sale transaction Command here
                                if (_currentTrans is IM30CardSale)
								{
									if (_startCardTransactionAxComm != null)
									{
										startTransAxComm = _startCardTransactionAxComm;
										currCardSale = (IM30CardSale)_currentTrans;

                                        WaitForNextTransTime("Start New Card Sale Trans.");
                                        if (currCardSale.StartTransaction(out Exception errS1) == false)
										{
											_isAllowed2ndCardCommandParam = false;
											isProceedTrans = false;
											isReqStopTransWithMaintenance = true;

                                            _log.LogError(_logChannel, _processId, new Exception("-Fail to Start Card Sale Trans.~"), "XA01", "IM30PortManagerAx.ManagerThreadWorking");
                                        }
									}
									else
									{
										isProceedTrans = false;
										_currentTrans.ShutdownX();
                                        _log.LogError(_logChannel, _processId, new Exception("-Parameter not found when Start Reader Card transaction~"), "XA03", "IM30PortManagerAx.ManagerThreadWorking");
                                    }
								}
								//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
								//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
								//Execute Single Command transaction here - Only Supported command is working
								else if(
									(_currentTrans is IM30CardSettlement)
									||
                                    (_currentTrans is IM30GetDeviceInfo)
                                    ||
                                    (_currentTrans is IM30GetLastTransaction)
                                    ||
                                    (_currentTrans is IM30Ping)
                                    ||
                                    (_currentTrans is IM30Reboot)
                                    ||
                                    (_currentTrans is IM30StopTransaction)
                                    ||
                                    (_currentTrans is IM30VoidTransaction)
                                )
								{
                                    WaitForNextTransTime("Start New Single Card Reader Trans.");
                                    if (_currentTrans.StartTransaction(out Exception errS1) == false)
                                    {
                                        _log.LogError(_logChannel, _processId, new Exception("-Fail to Execute Card Reader Command~"), "XA05", "IM30PortManagerAx.ManagerThreadWorking");
                                        _isAllowed2ndCardCommandParam = false;
                                        isProceedTrans = false;
                                        isReqStopTransWithMaintenance = true;
                                    }
                                }
								else
								{
                                    isProceedTrans = false;
                                    _currentTrans.ShutdownX();
                                }
								//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
								//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
								// Check Command Transaction Started ACK Receiving Status
								if (isProceedTrans)
								{
									DateTime tOutS1 = DateTime.Now.AddSeconds(7);

									while ((_isDisposed == false)
										&& (_currentTrans.IsTransStartSuccessful.HasValue == false)
										&& (tOutS1.Ticks > DateTime.Now.Ticks)
										&& (_currentTrans.IsCurrentWorkingEnded == false)
									)
									{
										Thread.Sleep(2);
									}

									if (_isDisposed == false)
									{
                                        if ((_currentTrans.IsTransStartSuccessful == true)
											&&
											(_currentTrans.IsCurrentWorkingEnded == false)
										)
                                        {
                                            isProceedTrans = true;

                                            SendStatusMonitor(KioskCommonStatus.Yes, "Normal Data Communication");
                                        }

                                        else
                                        {
                                            SendStatusMonitor(KioskCommonStatus.No, "Machine fail to respond");

                                            isProceedTrans = false;
                                            isReqStopTransWithMaintenance = true;

                                            if (_finalResult is null)
                                            {
                                                _finalResult = _currentTrans.FinalResult;
                                                if (_finalResult == null)
                                                    _finalResult = _currentTrans.NewErrFinalResult("Card Reader not responded (not ACK) properly");

                                                lock (_2ndCardCommandParamLock)
                                                {
                                                    _isAllowed2ndCardCommandParam = false;
                                                }

                                                if (
                                                    ((_currentTrans is IM30CardSale) && (_2ndCardCommandParam is null))
                                                    ||
                                                    ((_currentTrans is IM30CardSale) && (_2ndCardCommandParam is CreditDebitChargeParam))
                                                    ||
                                                    (_currentTrans is IM30CardSettlement)
                                                )
                                                    OnClientTransactionFinishedDelgWorking(_finalResult);
                                            }
                                        }
                                    }
                                    
								}

								//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
								//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
								// Send 2nd Command for Card Sale
								if ((_isDisposed == false) && (isProceedTrans) && (currCardSale != null))
								{
                                    //CYA-DEBUG .. need to timeout below code
                                    //Wait for _2ndCardCommandParam to be existed
                                    while ((_disposed == false) && (_2ndCardCommandParam is null) && (_currentTrans.IsCurrentWorkingEnded == false))
                                    {
                                        Thread.Sleep(2);
                                    }

                                    lock (_2ndCardCommandParamLock)
									{
										_isAllowed2ndCardCommandParam = false;
                                    }

                                    if ((_disposed == false) && (_currentTrans.IsCurrentWorkingEnded == false))
									{
                                        //Send 2nd Card Sale Command Parameter to API
                                        if ((_disposed == false) && (_2ndCardCommandParam != null))
										{
											// Sent Stop 2nd Command Parameter
											if (_2ndCardCommandParam is StopCardTransParam)
											{
												bool isReqSuccess = false;

												try
												{
													currCardSale.RequestStopTransaction(out isReqSuccess);
												}
												catch (Exception ex)
												{
                                                    _log.LogError(_logChannel, _processId, ex, "EX11", "IM30PortManagerAx.ManagerThreadWorking");
                                                    // when fail .. then wait until transaction time out
                                                }

                                                isStopCommandHasBeenSent = isReqSuccess;

                                                if (isReqSuccess == false)
												{
                                                    SendStatusMonitor(KioskCommonStatus.No, "Machine fail to respond");

                                                    isProceedTrans = false;
													isReqStopTransWithMaintenance = false;
													if (_finalResult is null)
													{
														_finalResult = _currentTrans.FinalResult;
														if (_finalResult == null)
															_finalResult = new IM30CardSaleResult(isManualStopped: true);
													}
												}
											}

											// Sent Credit/Debit 2nd Command Parameter
											else if (_2ndCardCommandParam is CreditDebitChargeParam)
											{
												bool isReqSuccessCD = false;

												try
												{
													isReqSuccessCD = currCardSale.Send2ndTransCommand(_2ndCardCommandParam);
												}
												catch (Exception ex)
												{
                                                    _log.LogError(_logChannel, _processId, ex, "EX15", "IM30PortManagerAx.ManagerThreadWorking");
                                                    // when fail .. then wait until transaction time out
                                                }

                                                is2ndCommandParamHasSent = isReqSuccessCD;

                                                if (isReqSuccessCD == false)
												{
                                                    SendStatusMonitor(KioskCommonStatus.No, "Machine fail to respond");

                                                    isProceedTrans = false;
													isReqStopTransWithMaintenance = true;

													if (_finalResult is null)
													{
														_finalResult = _currentTrans.FinalResult;
														if (_finalResult == null)
															_finalResult = new IM30CardSaleResult(new Exception("-Unable to execute 2nd Credit/Debit Sale Command~Port Manager"), null);
														OnClientTransactionFinishedDelgWorking(_finalResult);
													}
												}
											}

											// Sent TnG 2nd Command Parameter for Check-in
											else if (_2ndCardCommandParam is TnGEntryCheckinParam)
											{
												bool isReqSuccessTnG1 = false;

												try
												{
													isReqSuccessTnG1 = currCardSale.Send2ndTransCommand(_2ndCardCommandParam);
												}
												catch (Exception ex)
												{
                                                    _log.LogError(_logChannel, _processId, ex, "EX17", "IM30PortManagerAx.ManagerThreadWorking");
                                                    // when fail .. then wait until transaction time out
                                                }

                                                is2ndCommandParamHasSent = isReqSuccessTnG1;

                                                if (isReqSuccessTnG1 == false)
												{
                                                    SendStatusMonitor(KioskCommonStatus.No, "Machine fail to respond");

                                                    isProceedTrans = false;
													isReqStopTransWithMaintenance = true;

													if (_finalResult is null)
													{
														_finalResult = _currentTrans.FinalResult;
														if (_finalResult == null)
															_finalResult = new IM30CardSaleResult(new Exception("-Unable to execute 2nd TnG Chech-in Sale Command~Port Manager"), null);
													}
												}
											}

                                            // Sent TnG 2nd Command Parameter for Checkout
                                            else if (_2ndCardCommandParam is TnGExitCheckoutParam)
                                            {
                                                bool isReqSuccessTnG2 = false;

                                                try
                                                {
                                                    isReqSuccessTnG2 = currCardSale.Send2ndTransCommand(_2ndCardCommandParam);
                                                }
                                                catch (Exception ex)
                                                {
                                                    _log.LogError(_logChannel, _processId, ex, "EX19", "IM30PortManagerAx.ManagerThreadWorking");
                                                    // is fail .. then wait until transaction time out
                                                }

                                                is2ndCommandParamHasSent = isReqSuccessTnG2;

                                                if (isReqSuccessTnG2 == false)
                                                {
                                                    SendStatusMonitor(KioskCommonStatus.No, "Machine fail to respond");

                                                    isProceedTrans = false;
                                                    isReqStopTransWithMaintenance = true;

                                                    if (_finalResult is null)
                                                    {
                                                        _finalResult = _currentTrans.FinalResult;
                                                        if (_finalResult == null)
                                                            _finalResult = new IM30CardSaleResult(new Exception("-Unable to execute 2nd TnG Chechout Sale Command~Port Manager"), null);
                                                    }
                                                }
                                            }


                                            _is2ndParamHasBeenUtilized = true;
										}

										else if ((_disposed == false) && (_2ndCardCommandParam == null))
										{
											isProceedTrans = false;
											isReqStopTransWithMaintenance = true;

											if (_finalResult is null)
											{
												_finalResult = _currentTrans.FinalResult;
												if (_finalResult == null)
													_finalResult = new IM30CardSaleResult(new Exception("-Time out expecting 2nd Comand Parameter for Sale Command~Port Manager"), null);
											}
										}
									}

									else if (_disposed == false)/* if (_currentTrans.IsCurrentWorkingEnded == true) */
                                    {
										isProceedTrans = false;
										isReqStopTransWithMaintenance = true;

										if (_finalResult is null)
										{
											_finalResult = _currentTrans.FinalResult;
											if (_finalResult == null)
												_finalResult = new IM30CardSaleResult(new Exception("-Transaction Ended. Unable to execute 2nd Sale Command~Port Manager"), null);
										}

										if (_2ndCardCommandParam is CreditDebitChargeParam)
											OnClientTransactionFinishedDelgWorking(_finalResult);

										else if (_2ndCardCommandParam is null)
										{
                                            OnClientTransactionFinishedDelgWorking(_finalResult);
                                        }
									}
								}

								//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
								//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
								// Get Final Result
								if ((_isDisposed == false) && isProceedTrans)
								{
									//Wait for Final Result
									//CYA-DEBUG need to timeout below code
									while ((_isDisposed == false) && (_currentTrans.FinalResult is null) && (_currentTrans.IsCurrentWorkingEnded == false))
									{
										Thread.Sleep(2);
									}

									if (_isDisposed == false)
									{
                                        bool isResultSuccess = false;
                                        //Get Final Result if success
                                        if ((_currentTrans.FinalResult != null)
											&&
											((_currentTrans.FinalResult.IsSuccess == true)
												||
												(currCardSale?.IsFinalResultFromReader == true)
											)
										)
                                        {
                                            if (_finalResult is null)
                                            {
                                                _finalResult = _currentTrans.FinalResult;
                                                isResultSuccess = true;
                                                if (
                                                   ((_currentTrans is IM30CardSale) && (_2ndCardCommandParam is null))
                                                   ||
                                                   ((_currentTrans is IM30CardSale) && (_2ndCardCommandParam is CreditDebitChargeParam))
                                                   ||
                                                   (_currentTrans is IM30CardSettlement)
                                                )
                                                    OnClientTransactionFinishedDelgWorking(_finalResult);
                                            }
                                        }

                                        if (_finalResult is null)
                                        {
                                            if (isStopCommandHasBeenSent == false)
                                                SendStatusMonitor(KioskCommonStatus.No, "Machine fail to respond");

                                            //Request Get Last Transaction --------------
                                            if (((_currentTrans is IM30CardSale) && (is2ndCommandParamHasSent))
                                                || 
                                                (_currentTrans is IM30VoidTransaction)
                                            )
                                            {
                                                isReqGetLastTrans = true;
                                            }
                                            //-------------------------------------------								
                                            else
                                            {
                                                isReqStopTransWithMaintenance = true;
                                                if (_finalResult is null)
                                                {
                                                    _finalResult = _currentTrans.FinalResult;
                                                    if (_finalResult == null)
                                                        _finalResult = _currentTrans.NewErrFinalResult("-No valid card response data; Port Ax. Manager~");
                                                                                                        
                                                    if (
                                                        ((_currentTrans is IM30CardSale) && (_2ndCardCommandParam is null))
                                                        ||
                                                        ((_currentTrans is IM30CardSale) && (_2ndCardCommandParam is CreditDebitChargeParam))
                                                        ||
                                                        (_currentTrans is IM30CardSettlement)
                                                    )
                                                        OnClientTransactionFinishedDelgWorking(_finalResult);
                                                }
                                            }
                                        }
                                        
                                        if (
                                            ((isResultSuccess == false) || (_currentTrans.IsCurrentWorkingEnded == false))
                                            &&
                                            (_isDisposed == false)
                                        )
                                        {
                                            isReqStopTransWithMaintenance = true;
                                        }
                                    }
								}
								//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
								//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
							}
							catch (Exception ex)
							{
                                if ((ex is ThreadStateException) == false)
                                {
                                    string errMsg = $@"{ex.Message}";
                                    if (errMsg?.Length > 3900)
                                        errMsg = errMsg.Substring(0, 3800);

                                    SendStatusMonitor(KioskCommonStatus.No, $@"Error encountered; At {DateTime.Now:HH:mm:ss} ; {errMsg}");
                                }

                                _log.LogError(_logChannel, _processId, ex, "EX31", "IM30PortManagerAx.ManagerThreadWorking");

                                if ((_finalResult == null)
                                    &&
                                    (isStopCommandHasBeenSent)
                                )
                                    isReqStopTransWithMaintenance = true;

                                else if ((_finalResult == null)
									&&
									((is2ndCommandParamHasSent) || (_currentTrans is IM30VoidTransaction))
								)
									isReqGetLastTrans = true;
								else
									isReqStopTransWithMaintenance = true;
                            }
							finally
							{
                                /////-------------------------------------------------------------------------------------------------------------------
                                ///// Handle empty-fail result 
                                if ((_isDisposed == false) && (_finalResult is null))
                                {
                                    if (isStopCommandHasBeenSent == false)
                                        SendStatusMonitor(KioskCommonStatus.No, "Machine fail to respond");

                                    _finalResult = _currentTrans.FinalResult;

                                    if (isStopCommandHasBeenSent)
                                    {
                                        if (_finalResult is null)
                                            Thread.Sleep(2);

                                        _finalResult = _currentTrans.FinalResult;

                                        if (_finalResult is null)
                                        {
                                            _finalResult = _currentTrans.NewErrFinalResult("-Fail card sale transaction (A)~");
                                            isReqStopTransWithMaintenance = true;
                                        }
                                    }
                                    else if ((is2ndCommandParamHasSent) || (_currentTrans is IM30VoidTransaction))
                                    {
                                        isReqGetLastTrans = true;
                                    }
                                    else if (currCardSale != null)
                                    {
                                        if (_finalResult == null)
                                            _finalResult = _currentTrans.NewErrFinalResult("-Fail card sale transaction (B)~");

                                        if (_2ndCardCommandParam is CreditDebitChargeParam)
                                            OnClientTransactionFinishedDelgWorking(_finalResult);

                                        isReqStopTransWithMaintenance = true;
                                    }
                                    else if (_currentTrans is IM30CardSettlement)
                                    {
                                        if (_finalResult == null)
                                            _finalResult = _currentTrans.NewErrFinalResult("-Fail Settlement~");

                                        OnClientTransactionFinishedDelgWorking(_finalResult);

                                        isReqStopTransWithMaintenance = true;
                                    }
                                    else
                                    {
                                        if (_finalResult == null)
                                            _finalResult = _currentTrans.NewErrFinalResult("-Fail card reader (single command) transaction~");

                                        isReqStopTransWithMaintenance = true;
                                    }
                                }
								/////--------------------------------------------------------------------------------------------------------------------
                                if ((_currentTrans?.IsCurrentWorkingEnded == false)
									&&
									(
										(isReqGetLastTrans == false)
										&&
										(isReqStopTransWithMaintenance == false)
									)
								)
								{
									DateTime tOutZ = DateTime.Now.AddSeconds(3);
									while ((_isDisposed == false) && (_currentTrans?.IsCurrentWorkingEnded == false) && (tOutZ.Ticks > DateTime.Now.Ticks))
                                    {
										Thread.Sleep(50);
									}

									if (_currentTrans?.IsCurrentWorkingEnded == false)
										isReqStopTransWithMaintenance = true;
								}
							}
							/////################################################################################################################################################################
							///// Card Reader Maintenance ######################################################################################################################################
							IsMaintenanceInProgress = true;
							///// Get Last Transaction -----------------------------------------------------
							if ((_isDisposed == false) && (isReqGetLastTrans))
							{
								try
								{
                                    _nextNewTransactionTime = DateTime.Now.AddMilliseconds(_minCommandIntervalMillisec);
                                    RecoverLastTransaction(_currentTrans, _2ndCardCommandParam);
									isReqStopTransWithMaintenance = false;
								}
								catch (Exception ex)
								{
									isReqStopTransWithMaintenance = true;

                                    _log.LogError(_logChannel, _processId, ex, "EX35", "IM30PortManagerAx.ManagerThreadWorking");

                                    if (_finalResult is null)
										_finalResult = _currentTrans.NewErrFinalResult($@"-Fail transaction; Unable to recover (E)~{ex.Message}");

                                    if (
                                        ((_currentTrans is IM30CardSale) && (_2ndCardCommandParam is null))
                                        ||
                                        ((_currentTrans is IM30CardSale) && (_2ndCardCommandParam is CreditDebitChargeParam))
                                        ||
                                        (_currentTrans is IM30CardSettlement)
                                    )
                                        OnClientTransactionFinishedDelgWorking(_finalResult);
                                }
							}
							else if ((_isDisposed == true) && (isReqGetLastTrans))
							{
								//isReqStopTransWithMaintenance = true;
							}
							///// Stop Transaction and Maintain Card Reader --------------------------------
							if (isReqStopTransWithMaintenance)
							{
								try
								{
                                    _nextNewTransactionTime = DateTime.Now.AddMilliseconds(_minCommandIntervalMillisec);
                                    StopTransWithMaintenance(_currentTrans, COMPort);
								}
								catch (Exception ex)
								{
                                    _log.LogError(_logChannel, _processId, ex, "EX37", "IM30PortManagerAx.ManagerThreadWorking");
                                }
							}
							else
							{
								try
								{
									_currentTrans?.ShutdownX();
								}
								catch (Exception ex)
								{
                                    _log.LogError(_logChannel, _processId, ex, "EX39", "IM30PortManagerAx.ManagerThreadWorking");
                                }
							}
                            ///// --------------------------------------------------------------------------
                            /////################################################################################################################################################################

                        } /* End of start new transaction when _currentTrans is not null" */
                    }
					catch (Exception ex)
                    {
                        _log.LogError(_logChannel, _processId, ex, "EX51", "IM30PortManagerAx.ManagerThreadWorking");
                    }
					finally
                    {
						IsMaintenanceInProgress = false;
						
						if (_currentTrans?.IsCurrentWorkingEnded == false)
							_currentTrans?.ShutdownX();

                        _isAllowNewTrans = true;

                        ///// .. Latency delay time to reader for next new start transaction
                        if ((isNewTransactionFound) && (_isDisposed == false))
                        {
                            _nextNewTransactionTime = DateTime.Now.AddMilliseconds(_minCommandIntervalMillisec);
                        }

                        if ((_isDisposed == false) && (isNewTransactionFound == false) && (_isDisposed == false))
                        {
							Thread.Sleep(20);
						}
                    }
				} /* End of while (_isDisposed == false) */
			}
			catch (Exception ex)
            {
                _log.LogError(_logChannel, _processId, ex, "EX71", "IM30PortManagerAx.ManagerThreadWorking");
            }
			return;
			/////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
			bool RecoverLastTransaction(IIM30Trans currentTransX, I2ndCardCommandParam commParamX)
            {
				if (_finalResult != null)
				{
                    if (
                        ((currentTransX is IM30CardSale) && (commParamX is null))
                        ||
                        ((currentTransX is IM30CardSale) && (commParamX is CreditDebitChargeParam))
                        ||
                        (currentTransX is IM30CardSettlement)
                    )
                        OnClientTransactionFinishedDelgWorking(_finalResult);

                    return false;
				}

				bool isMaintenanceSuccess = false;
				///// End existing transaction --------------------------------------------------------
				try
				{
					isMaintenanceSuccess = false;
					currentTransX.ShutdownX();

					DateTime tOut01 = DateTime.Now.AddSeconds(7);
					while ((_isDisposed == false) && (currentTransX.IsCurrentWorkingEnded == false) && (tOut01.Ticks > DateTime.Now.Ticks))
                    {
						Thread.Sleep(10);
                    }

					if (_isDisposed)
						return false;

					if (currentTransX.IsCurrentWorkingEnded)
					{
						isMaintenanceSuccess = true;
                        _log.LogText(_logChannel, _processId, $@"Current Trans. ({currentTransX.GetType().Name}) Ended", "B01", "IM30PortManagerAx.RecoverLastTransaction");
                    }
					else
                    {
                        _log.LogText(_logChannel, _processId, $@"Unable to end current Trans. ({currentTransX.GetType().Name})", "B02", "IM30PortManagerAx.RecoverLastTransaction");
                    }
				}
				catch { }
				/////-----------------------------------------------------------------------------------
				if (currentTransX.IsPerfectCompleteEnd == false)
					isMaintenanceSuccess = MaintainIM30(COMPort);
				else
					isMaintenanceSuccess = true;
                /////-----------------------------------------------------------------------------------

                if (_isDisposed)
					return false;

				//--------------------------------------------------------------------------------------
				// Validate Type of recovery ; Only 
				if (((currentTransX is IM30CardSale) 
					|| (currentTransX is IM30VoidTransaction)
				) == false)
                {
					if (_finalResult is null)
						_finalResult = currentTransX.NewErrFinalResult("-Fail transaction; Unable to recover (A)~Transaction Recovery Type not supported");

                    //.. Not card sale ..
                    //if (commParamX is CreditDebitChargeParam)
                    //	OnClientTransactionFinishedDelgWorking(_finalResult);

                    if (currentTransX is IM30CardSettlement)
                        OnClientTransactionFinishedDelgWorking(_finalResult);


                    return false;
				}

				//--------------------------------------------------------------------------------------
				//Get Last Transaction -----------------------------------
				bool isRecoverySuccess = false;
				if (isMaintenanceSuccess)
                {
					IIM30TransResult locFinalErrResult = null;

					if (_finalResult == null)
                    {
						IM30GetLastTransaction transG = null;

						int maxTry = 2;
						for (int tryInx=0; tryInx < maxTry; tryInx++)
                        {
							try
							{
								transG = new IM30GetLastTransaction(COMPort, commParamX.PosTransId, null
											, noActionMaxWaitSec: 20, _debugShowMessageDelgHandler);

                                WaitForNextTransTime("Get Last Trans. when recovery", addExtraMillisec: 3000);
                                if (transG.StartTransaction(out Exception ex2))
								{
									//CYA-DEBUG need to timeout below code
									while ((_isDisposed == false) && (transG.IsCurrentWorkingEnded == false))
									{
										Thread.Sleep(10);
									}

									if ((transG.FinalResult?.IsSuccess == true))
									{
										//validate the locFinalResult to match the PosTransNo
										if (currentTransX is IM30CardSale)
										{
											// ----- Check Result match with CreditDebit Charge ------------------------------------------------------------
											if (commParamX is CreditDebitChargeParam paramCD)
											{
												bool isMatch = IsResultExpected_CreditDebitCharge(transG.FinalResult.ResultData, paramCD);

												if (isMatch)
                                                {
													_finalResult = new IM30CardSaleResult(transG.FinalResult.ResultData);
													isRecoverySuccess = true;
													break;
												}
												else
                                                {
													_finalResult = currentTransX.NewErrFinalResult("-Fail transaction; Expected Result not found~Credit/Debit Card Charge");
													break;
												}
											}

											// ----- Check Result match with TnG Check-in Charge ------------------------------------------------------------
											else if (commParamX is TnGEntryCheckinParam paramTnG1)
											{

												bool isMatch = IsResultExpected_TnGCheckIn(transG.FinalResult.ResultData, paramTnG1);

												if (isMatch)
												{
													_finalResult = new IM30CardSaleResult(transG.FinalResult.ResultData);
													isRecoverySuccess = true;
													break;
												}
												else
												{
													_finalResult = currentTransX.NewErrFinalResult("-Fail transaction; Expected Result not found~TnG Check-in Charge");
													break;
												}
											}
                                            // ----- Check Result match with TnG Checkout Charge ------------------------------------------------------------
                                            else if (commParamX is TnGExitCheckoutParam paramTnG2)
                                            {

                                                bool isMatch = IsResultExpected_TnGCheckout(transG.FinalResult.ResultData, paramTnG2);

                                                if (isMatch)
                                                {
                                                    _finalResult = new IM30CardSaleResult(transG.FinalResult.ResultData);
                                                    isRecoverySuccess = true;
                                                    break;
                                                }
                                                else
                                                {
                                                    _finalResult = currentTransX.NewErrFinalResult("-Fail transaction; Expected Result not found~TnG Checkout Charge");
                                                    break;
                                                }
                                            }

                                            else
                                            {
												_finalResult = currentTransX.NewErrFinalResult($@"-Fail transaction; Parameter type not supported for getting last card transaction result~{commParamX.GetType().Name}");
												break;
											}
										}
										else if (currentTransX is IM30VoidTransaction voidTransB)
										{
                                            bool isMatch = IsResultExpected_CreditDebitVoidTrans(transG.FinalResult.ResultData, voidTransB);

                                            if (isMatch)
                                            {
                                                _finalResult = new IM30VoidTransactionResult(transG.FinalResult.ResultData);
                                                break;
                                            }
                                            else
                                            {
                                                _finalResult = currentTransX.NewErrFinalResult("-Fail to void transaction; Expected Result not found~Void Credit/Debit Charge");
                                                break;
                                            }

                                        }
										else
										{
											_finalResult = currentTransX.NewErrFinalResult("-Fail transaction; Unable to recover (B)~Transaction Recovery Type not supported");
											break;
										}
									}
									else
									{
										locFinalErrResult = currentTransX.NewErrFinalResult("-Fail transaction; Unable to recover~Unable to capture result");
									}
								}
								else
								{
									locFinalErrResult = currentTransX.NewErrFinalResult($@"-Unable to Recover Last Transaction (A); Port Manager~{ex2?.Message}");
								}
							}
							catch (Exception ex)
							{
                                _log.LogError(_logChannel, _processId, ex, "EX51", "IM30PortManagerAx.RecoverLastTransaction");
                            }
                            finally
                            {
								try
								{
									transG?.ShutdownX();
								}
								catch { }
							}
						}

						if ((_finalResult is null) && (locFinalErrResult != null))
							_finalResult = locFinalErrResult;

						else if (_finalResult is null)
							_finalResult = currentTransX.NewErrFinalResult("-Fail Transaction; Unable to Recover Last Result (G); Port Manager~");

						if (_finalResult != null)
						{
                            if (
                                ((currentTransX is IM30CardSale) && (commParamX is null))
                                ||
                                ((currentTransX is IM30CardSale) && (commParamX is CreditDebitChargeParam))
                            )
                                OnClientTransactionFinishedDelgWorking(_finalResult);
                        }
					}
				}
				else
                {
					if (_finalResult == null)
					{
						_finalResult = currentTransX.NewErrFinalResult("-No valid card response data has found; Fail reader recovery; Fail to recover response data; Port Manager~");

                        if (
                            ((currentTransX is IM30CardSale) && (commParamX is null))
                            ||
                            ((currentTransX is IM30CardSale) && (commParamX is CreditDebitChargeParam))
                        )
                            OnClientTransactionFinishedDelgWorking(_finalResult);
                    }
				}
				//--------------------------------------------------------

				return isRecoverySuccess;
            }

            /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            bool IsResultExpected_CreditDebitCharge(IM30DataModel ResultData, CreditDebitChargeParam paramCD)
            {
                if ((ResultData is null) || (paramCD is null) || string.IsNullOrWhiteSpace(paramCD.PosTransId))
                    return false;

                if (ResponseCodeDef.IsEqualResponse(ResultData.ResponseCode, ResponseCodeDef.Approved) == false)
                    return false;

                IM30FieldElementModel expC1 = (from fd in ResultData.FieldElementCollection
                                               where FieldTypeDef.IsEqualType(fd.FieldTypeCode, FieldTypeDef.TransCodeOfLastTrans)
                                               select fd).FirstOrDefault();

                if ((expC1 is null) || string.IsNullOrWhiteSpace(expC1.Data))
                    return false;

                if (expC1.Data.Trim().ToUpper().Equals(TransactionCodeDef.ChargeAmount, StringComparison.InvariantCultureIgnoreCase) == false)
                    return false;

                IM30FieldElementModel expFd = (from fd in ResultData.FieldElementCollection
                                               where FieldTypeDef.IsEqualType(fd.FieldTypeCode, FieldTypeDef.PosTransId)
                                               select fd).FirstOrDefault();

                if ((expFd is null) || string.IsNullOrWhiteSpace(expFd.Data))
                    return false;

                return expFd.Data.Trim().ToUpper().Equals(paramCD.PosTransId.Trim().ToUpper(), StringComparison.InvariantCultureIgnoreCase);
			}


			bool IsResultExpected_TnGCheckIn(IM30DataModel ResultData, TnGEntryCheckinParam paramCD)
			{
                if ((ResultData is null) || (paramCD is null) || string.IsNullOrWhiteSpace(paramCD.PosTransId))
                    return false;

                if (ResponseCodeDef.IsEqualResponse(ResultData.ResponseCode, ResponseCodeDef.Approved) == false)
                    return false;

                IM30FieldElementModel expC1 = (from fd in ResultData.FieldElementCollection
                                               where FieldTypeDef.IsEqualType(fd.FieldTypeCode, FieldTypeDef.TransCodeOfLastTrans)
                                               select fd).FirstOrDefault();

                if ((expC1 is null) || string.IsNullOrWhiteSpace(expC1.Data))
                    return false;

                if (expC1.Data.Trim().ToUpper().Equals(TransactionCodeDef.Entry, StringComparison.InvariantCultureIgnoreCase) == false)
                    return false;

                IM30FieldElementModel expFd = (from fd in ResultData.FieldElementCollection
                                               where FieldTypeDef.IsEqualType(fd.FieldTypeCode, FieldTypeDef.PosTransId)
                                               select fd).FirstOrDefault();

                if ((expFd is null) || string.IsNullOrWhiteSpace(expFd.Data))
                    return false;

                return expFd.Data.Trim().ToUpper().Equals(paramCD.PosTransId.Trim().ToUpper(), StringComparison.InvariantCultureIgnoreCase);
            }

            bool IsResultExpected_TnGCheckout(IM30DataModel ResultData, TnGExitCheckoutParam paramCD)
            {
                if ((ResultData is null) || (paramCD is null) || string.IsNullOrWhiteSpace(paramCD.PosTransId))
                    return false;

                if (ResponseCodeDef.IsEqualResponse(ResultData.ResponseCode, ResponseCodeDef.Approved) == false)
                    return false;

                IM30FieldElementModel expC1 = (from fd in ResultData.FieldElementCollection
                                               where FieldTypeDef.IsEqualType(fd.FieldTypeCode, FieldTypeDef.TransCodeOfLastTrans)
                                               select fd).FirstOrDefault();

                if ((expC1 is null) || string.IsNullOrWhiteSpace(expC1.Data))
                    return false;

				if (expC1.Data.Trim().ToUpper().Equals(TransactionCodeDef.Exit, StringComparison.InvariantCultureIgnoreCase) == false)
                    return false;

                IM30FieldElementModel expFd = (from fd in ResultData.FieldElementCollection
                                               where FieldTypeDef.IsEqualType(fd.FieldTypeCode, FieldTypeDef.PosTransId)
                                               select fd).FirstOrDefault();

                if ((expFd is null) || string.IsNullOrWhiteSpace(expFd.Data))
                    return false;

                return expFd.Data.Trim().ToUpper().Equals(paramCD.PosTransId.Trim().ToUpper(), StringComparison.InvariantCultureIgnoreCase);
            }

            bool IsResultExpected_CreditDebitVoidTrans(IM30DataModel ResultData, IM30VoidTransaction voidTrans)
            {
                if ((ResultData is null) || string.IsNullOrWhiteSpace(voidTrans.InvoiceNo) || string.IsNullOrWhiteSpace(voidTrans.CardToken))
                    return false;

                if (ResponseCodeDef.IsEqualResponse(ResultData.ResponseCode, ResponseCodeDef.Approved) == false)
                    return false;

                IM30FieldElementModel expC1 = (from fd in ResultData.FieldElementCollection
                                               where FieldTypeDef.IsEqualType(fd.FieldTypeCode, FieldTypeDef.TransCodeOfLastTrans)
                                               select fd).FirstOrDefault();

                if ((expC1 is null) || string.IsNullOrWhiteSpace(expC1.Data))
                    return false;

                if (expC1.Data.Trim().ToUpper().Equals(TransactionCodeDef.Void, StringComparison.InvariantCultureIgnoreCase) == false)
                    return false;

                IM30FieldElementModel exp65 = (from fd in ResultData.FieldElementCollection
                                               where FieldTypeDef.IsEqualType(fd.FieldTypeCode, FieldTypeDef.InvoiceNo)
                                               select fd).FirstOrDefault();

                IM30FieldElementModel expC2 = (from fd in ResultData.FieldElementCollection
                                               where FieldTypeDef.IsEqualType(fd.FieldTypeCode, FieldTypeDef.CardToken)
                                               select fd).FirstOrDefault();

                IM30FieldElementModel exp40 = (from fd in ResultData.FieldElementCollection
                                               where FieldTypeDef.IsEqualType(fd.FieldTypeCode, FieldTypeDef.TransAmount)
                                               select fd).FirstOrDefault();

                if ((exp65 is null) || (expC2 is null) || (exp40 is null))
                    return false;

                else if (string.IsNullOrWhiteSpace(exp65.Data) || string.IsNullOrWhiteSpace(expC2.Data) || string.IsNullOrWhiteSpace(exp40.Data))
                    return false;

                CreditDebitVoidTransactionResp vRest = new CreditDebitVoidTransactionResp(ResultData, isDataGetFromLastTransaction: true);

                if ((vRest.IsDataFound) && (vRest.DataError is null))
                {
                    if (vRest.InvoiceNo.Equals(voidTrans.InvoiceNo.Trim(), StringComparison.InvariantCultureIgnoreCase) == false)
                        return false;

                    else if (vRest.CardToken.Equals(voidTrans.CardToken.Trim(), StringComparison.InvariantCultureIgnoreCase) == false)
                        return false;

                    else if (vRest.Amount != voidTrans.TransAmount)
                        return false;

                    return true;
                }
                
                return false;
            }

            /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            bool StopTransWithMaintenance(IIM30Trans currentTransX, string comPortX)
            {
				bool isMaintenanceSuccess = false;

				///// End existing transaction --------------------------------------------------------
				if (currentTransX?.IsTransEndDisposed == false)
                {
					try
					{
						isMaintenanceSuccess = false;
						currentTransX.ShutdownX();

						DateTime tOut01 = DateTime.Now.AddSeconds(7);
						while ((_isDisposed == false) && (currentTransX.IsCurrentWorkingEnded == false) && (tOut01.Ticks > DateTime.Now.Ticks))
						{
							Thread.Sleep(10);
						}

						if (_isDisposed)
							return false;

						if (currentTransX.IsCurrentWorkingEnded)
						{
							isMaintenanceSuccess = true;
                            _log.LogText(_logChannel, _processId, $@"Current Trans. ({currentTransX.GetType().Name}) Ended", "B01", "IM30PortManagerAx.StopTransWithMaintenance");
                        }
						else
						{
                            _log.LogText(_logChannel, _processId, $@"Unable to end current Trans. ({currentTransX.GetType().Name})", "B02", "IM30PortManagerAx.StopTransWithMaintenance");
                        }
                    }
					catch { }
				}
				/////-----------------------------------------------------------------------------------
				if (currentTransX.IsPerfectCompleteEnd == false)
					isMaintenanceSuccess = MaintainIM30(comPortX);
				else
					isMaintenanceSuccess = true;
                /////-----------------------------------------------------------------------------------
                return isMaintenanceSuccess;
			}
			/////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
			bool MaintainIM30(string comPortX)
			{
				if (_isDisposed)
                {
					return false;
                }

				bool isMaintenanceSuccess = false;
				///// Stop Transaction if found any existing card sale transaction is not ended.
				for (int tryCount = 0; tryCount < 3; tryCount++)
				{
					try
					{
						isMaintenanceSuccess = false;
						IM30StopTransaction newStopTrans = new IM30StopTransaction(comPortX, "MaintainIM3001B_" + DateTime.Now.ToString("HHmmss_fffffff"), null
							, noActionMaxWaitSec: 7, _debugShowMessageDelgHandler);

                        WaitForNextTransTime("Stop Card Reader Trans. when maintenance", addExtraMillisec: (300 + (tryCount * 2000)) );
                        if (newStopTrans.StartTransaction(out Exception ex2) == false)
						{
							continue;
						}

						DateTime tOut02 = DateTime.Now.AddSeconds(8);
						while ((_isDisposed == false) && (newStopTrans.IsCurrentWorkingEnded == false) && (tOut02.Ticks > DateTime.Now.Ticks))
						{
							Thread.Sleep(10);
						}

						if (_isDisposed)
							return false;

						if ((newStopTrans.IsCurrentWorkingEnded) && (newStopTrans.FinalResult?.IsSuccess == true))
						{
							newStopTrans.ShutdownX();
							isMaintenanceSuccess = true;
							break;
						}
						else
							newStopTrans.ShutdownX();
					}
					catch (Exception ex)
					{
                        _log.LogError(_logChannel, _processId, ex, "EX31", "IM30PortManagerAx.MaintainIM30");
                    }
				}

				//Reboot IM30
				if (isMaintenanceSuccess == false)
				{
                    for (int tryCount = 0; tryCount < 3; tryCount++)
					{
						if (isMaintenanceSuccess)
							break;

                        try
                        {
                            IM30Reboot rebootTrans = new IM30Reboot(comPortX, null, "MaintainIM302B_" + DateTime.Now.ToString("HHmmss_fffffff")
                                , noActionMaxWaitSec: 7, _debugShowMessageDelgHandler);

                            WaitForNextTransTime("Reboot Card Reader", addExtraMillisec: 7000);
                            if (rebootTrans.StartTransaction(out Exception ex2) == true)
                            {
                                DateTime tOut02 = DateTime.Now.AddSeconds(15);
                                while ((_isDisposed == false) && (rebootTrans.IsCurrentWorkingEnded == false) && (tOut02.Ticks > DateTime.Now.Ticks))
                                {
                                    Thread.Sleep(10);
                                }

                                if (_isDisposed)
                                    return false;

                                if ((rebootTrans.IsCurrentWorkingEnded) && (rebootTrans.FinalResult?.IsSuccess == true))
                                {
                                    isMaintenanceSuccess = true;

                                    DateTime tOut03 = DateTime.Now.AddSeconds(100); /* Wait for 1 minutes and 40 seconds for reboot to be completed */
                                    while ((_isDisposed == false) && (tOut03.Ticks > DateTime.Now.Ticks))
                                    {
                                        Thread.Sleep(1000);
                                    }
                                }

                                rebootTrans.ShutdownX();
                            }
                        }
                        catch (Exception ex)
                        {
                            _log.LogError(_logChannel, _processId, ex, "EX101", "IM30PortManagerAx.MaintainIM30");
                        }
                    }

					//Set reader Malfunction flag if fail to repair
					if (isMaintenanceSuccess == false)
					{
						IsPortMalfunction = true;
					}
				}

				return isMaintenanceSuccess;
			}
			/////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
			void ShutDownTransactionOnly(IIM30Trans currentTransX)
            {
				if(currentTransX?.IsTransEndDisposed == false)
                {
					currentTransX.ShutdownX();
				}
            }
            /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
			void WaitForNextTransTime(string tag, int addExtraMillisec = 0)
			{
				if (addExtraMillisec > 0)
					_nextNewTransactionTime = _nextNewTransactionTime.AddMilliseconds(addExtraMillisec);

				while (_nextNewTransactionTime.Ticks > DateTime.Now.Ticks)
					Thread.Sleep(10);

                _nextNewTransactionTime = DateTime.Now.AddMilliseconds(_minCommandIntervalMillisec);

                _log.LogText(_logChannel, _processId, $@"Tag:{tag}; addExtraMillisec:{addExtraMillisec}; nextNewTransactionTime: {_nextNewTransactionTime:yyyy-MM-dd HH:MM:ss.fff}", "B01", "IM30PortManagerAx.WaitForNextTransTime");
            }
        }

        private void PortInitThreadWorking()
		{
			if (IsReaderInitializing == false)
            {
                return;
            }
            ///// Looping for getting the Device Info 10 times else Set MalFunction flag
            int maxRetryCount = 10;
			int tCount = 0;
			bool isDone = false;
			string errMsg = null;
			GetDeviceInfoResp dvInfo = null;

            do
			{
				Thread.Sleep(3000);

				try
				{
                    ///// Make Sure Manager Thread has Start
                    if (_isManThreadWorkerHasStart == false)
                    {
                        DateTime tOut = DateTime.Now.AddSeconds(30);
                        while ((_isManThreadWorkerHasStart == false) && (tOut.Ticks > DateTime.Now.Ticks))
                            Thread.Sleep(100);
                    }

                    ///// Make Sure Manager Thread is not under maintenance
                    if (IsMaintenanceInProgress == true)
                    {
                        DateTime tOut = DateTime.Now.AddMinutes(15);
                        while ((IsMaintenanceInProgress == true) && (tOut.Ticks > DateTime.Now.Ticks))
                            Thread.Sleep(100);
                    }

                    if ((_isManThreadWorkerHasStart == true) && (IsMaintenanceInProgress == false))
                    {
                        IIM30TransResult trnResult = PortInitCommand(out Exception error);
                        if ((trnResult?.IsSuccess == true) && (trnResult.ResultData != null))
                        {
                            if ((TransactionCodeDef.IsEqualTrans(trnResult.ResultData?.TransactionCode, TransactionCodeDef.GetDeviceInfo) == false)
								||
                                (ResponseCodeDef.IsEqualResponse(trnResult.ResultData?.ResponseCode, ResponseCodeDef.Approved) == false)
                                )
                            {
                                _log.LogError(_logChannel, _processId, new Exception("Fail to Get Device Info"), "EX01", "IM30PortManagerAx.PortInitThreadWorking");
                                isDone = false;
                            }
							else
							{
                                dvInfo = new GetDeviceInfoResp(trnResult.ResultData);

								if (dvInfo.DataError != null)
								{
                                    isDone = false;
                                    _log.LogError(_logChannel, _processId, new Exception("Error when Get Device Info", dvInfo.DataError), "EX05", "IM30PortManagerAx.PortInitThreadWorking");
                                }
								else
								{
									GetDeviceInfo = dvInfo;
                                    isDone = true;
								}
                            }
                        }
                        else
                        {
                            if ((error != null) && string.IsNullOrWhiteSpace(errMsg))
                            {
                                _log.LogError(_logChannel, _processId, error, "EX07", "IM30PortManagerAx.PortInitThreadWorking");
                                errMsg = error.Message;
                            }
                            else
                            {
                                _log.LogError(_logChannel, _processId, new Exception("Unknown error; when init the COM Port to get device info"), "EX10", "IM30PortManagerAx.PortInitThreadWorking");
                            }
                        }
                    }
                    else
                        tCount = maxRetryCount;
                }
				catch(Exception ex)
				{
                    _log.LogError(_logChannel, _processId, ex, "EX31", "IM30PortManagerAx.PortInitThreadWorking");
                }

                tCount++;
			} while ((tCount < maxRetryCount) && (isDone == false));

			if (isDone == false)
			{
				IsPortMalfunction = true;
			}
			IsReaderInitializing = false;
        }

        private bool _isDisposed = false;
		public void Dispose()
        {
			_isDisposed = true;
            _log = null;
    }
	}
}
