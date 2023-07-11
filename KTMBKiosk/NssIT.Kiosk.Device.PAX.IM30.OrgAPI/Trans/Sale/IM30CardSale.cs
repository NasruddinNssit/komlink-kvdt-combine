using Newtonsoft.Json;
using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.Base;
using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.TransParams;
using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.TransResult;
using NssIT.Kiosk.AppDecorator.Log;
using NssIT.Kiosk.Device.PAX.IM30.OrgAPI.Base;
using NssIT.Kiosk.Device.PAX.IM30.OrgAPI.Trans;
using NssIT.Kiosk.Log.DB;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Device.PAX.IM30.OrgAPI.Trans.Sale
{
    public class IM30CardSale : IIM30Trans, IDisposable
    {
        private const string _logChannel = "IM30_API";

        //public delegate void LogStateDelg(string logMsg, bool showToUI = false);
        public delegate void UpdateProcessStateDelg(
            CardSaleProcessState latestState, string locationTag = null, 
            bool isReverseWorking = false, bool isForceToSetNewACKTimeout = false);
        
        ///// Timeout Definition xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        private int _noActionMaxWaitSec = 60 * 60 * 24;
        private int _expectingCardInfoResponse_MaxWaitingTimeSec = 60 * 60 * 6; /*Default to 6 hours*/ 
        private int _waitingForSaleDecisionAction_MaxWaitingTimeSec = 60 * 60 * 1; /*Default to  1 hour*/
        private int _waitingForStopCommandAck_MaxWaitingTimeSec = 5; /*Default to  10 sec*/

        public const int CreditDebitCard_MaxWaitingTimeSec = 60 * 5; /*Default to  5 minutes*/
        public const int TngCard_MaxWaitingTimeSec = 10; /*Default to 10 seconds*/
        public const int KomLinkCard_MaxWaitingTimeSec = 10; /*Default to 10 seconds*/

        private const int _ack_MaxWaitingTimeSec = 4;
        ///// Max. Counting xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx 
        private const int _maxSendingStartTransCommandCount = 2;
        private const int _maxSending2ndCommandCount = 2;
        private const int _maxSendingCardStopCommandCount = 2;
        /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx 

        private int MaxStopCommandSendingCount = 3;

        public string COMPort { get; private set; }

        private bool? _isMainProcessHasAlreadyStop = null;
        private bool _isStopTransactionCommandHasSent = false;
        private bool _isEndProcessWithoutStopCommand = false;

        public bool IsPerfectCompleteEnd { get; private set; } = false;

        private int _stopCommandSendingCount = 0;
        private DateTime _transQuitTime = DateTime.MaxValue;
        private DateTime? _ackTimeout = null;

        private Thread _startTransactionThread = null;
        private ConcurrentQueue<bool> _dataReceivedNotes = new ConcurrentQueue<bool>();
        private ShowMessageLogDelg _logStateDEBUGDelgHandler = null;
        private UpdateProcessStateDelg _updateProcessStateDelgHandler = null;
        private CardSaleProcessState _processState = CardSaleProcessState.New;
        private IM30COMPort.OnDataReceivedNoteDelg _onDataReceivedNoteDelgHandle = null;
        private OnTransactionFinishedDelg _onTransactionFinishedHandler = null;
        private OnCardDetectedDelg _onCardDetectedDelgHandler = null;

        private List<CardSaleProcessState> _processEndingStatesList = new List<CardSaleProcessState>(new CardSaleProcessState[]
            {CardSaleProcessState.Busy, CardSaleProcessState.FinishEnding, CardSaleProcessState.CommandStopEnding, 
                CardSaleProcessState.ErrorHalt, CardSaleProcessState.Timeout, CardSaleProcessState.AppDisposed
                , CardSaleProcessState.ErrorHaltWithoutStopCommand 
            });

        private List<CardSaleProcessState> _timeoutThenSendStopCommandStatusList = new List<CardSaleProcessState>(new CardSaleProcessState[]
            {CardSaleProcessState.ExpectingCardInfoResponse, CardSaleProcessState.WaitingForSaleDecisionAction});

        private List<CardSaleProcessState> _allACKStatusList = new List<CardSaleProcessState>(new CardSaleProcessState[]
            {CardSaleProcessState.ExpectingAckAfterStartTransCommand
                , CardSaleProcessState.ExpectingAckAfterChargeCreditDebitCardCommand
                , CardSaleProcessState.ExpectingAckAfterStopTransCommand
            });

        private ICardSaleProcess _cardTypeSaleProcess = null;
        private I2ndCardCommandParam _cardTransParameters = null;
        private GateDirectionEn? _gateDirection = null;
        private int? _komLinkFirstSPNo = null;
        private int? _komLinkSecondSPNo = null;

        public Guid WorkingId { get; } = Guid.NewGuid();
        public TransactionTypeEn TransactionType => TransactionTypeEn.StartTrans_1stComm;
        public IM30DataModel CardInfoResult { get; private set; }
        public IIM30TransResult FinalResult { get; private set; }
        public bool IsCurrentWorkingEnded { get; private set; } = false;
        public bool? IsTransStartSuccessful { get; private set; } = null;
        public bool IsShutdown => IsDisposed;
        public bool? IsRequestStopTransaction { get; private set; } = null;

        public bool IsCardCommandSent { get; private set; } = false;
        public bool IsFinalResultFromReader { get; private set; } = false;

        public bool IsTransEndDisposed { get { return IsDisposed; } }

        public string PosTransID { get; private set; } = $@"--NEW--{DateTime.Now:yyyyMMddHHmmss}";

        ///// Static Properties xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        public static IM30CardSale LastIM30CardSale { get; private set; } = null;
        /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

        private DbLog _log = null;
        public DbLog Log
        {
            get
            {
                return _log ?? (_log = DbLog.GetDbLog());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="comPort"></param>
        /// <param name="onTransactionFinishedHandle"></param>
        /// <param name="transactionID"></param>
        /// <param name="maxCardDetectedWaitingTimeSec">Max. time used to wait for card detected</param>
        /// <param name="maxSaleDecisionWaitingTimeSec">Max. time used to wait for application to proceed card sale transaction after card detected; Expecting Send2ndTransCommand(..) to be called.</param>
        public IM30CardSale(string comPort, 
            OnTransactionFinishedDelg onTransactionFinishedHandle,
            OnCardDetectedDelg onCardDetectedDelgHandler,
            string transactionID,
            GateDirectionEn gateDirection,
            int komLinkFirstSPNo,
            int komLinkSecondSPNo,
            int maxCardDetectedWaitingTimeSec = 60 * 60 * 6, /*Default to 6 hours*/
            int maxSaleDecisionWaitingTimeSec = 60 * 60 * 1,  /*Default to  1 hour*/
            ShowMessageLogDelg debugShowMessageDelgHandler = null)
        {
            if (string.IsNullOrWhiteSpace(comPort))
                throw new Exception("Invalid COM port specification when initiate Card Sale reader");

            else if ((komLinkFirstSPNo < 1) || (komLinkFirstSPNo > 8))
                throw new Exception("komLinkFirstSPNo must between 1 to 8");

            else if ((komLinkSecondSPNo < 1) || (komLinkFirstSPNo > 8))
                throw new Exception("komLinkSecondSPNo must between 1 to 8");

            else if (komLinkSecondSPNo == komLinkFirstSPNo)
                throw new Exception("komLinkFirstSPNo and komLinkSecondSPNo must not the same");

            COMPort = comPort;
            _gateDirection = gateDirection;
            _komLinkFirstSPNo = komLinkFirstSPNo;
            _komLinkSecondSPNo = komLinkSecondSPNo;
            _onTransactionFinishedHandler = onTransactionFinishedHandle;
            _onCardDetectedDelgHandler = onCardDetectedDelgHandler;

            if (string.IsNullOrWhiteSpace(transactionID))
                PosTransID = $@"NewDoc-{DateTime.Now:yyyyMMddHHmmss}";
            else
                PosTransID = transactionID.Trim();
            
            _expectingCardInfoResponse_MaxWaitingTimeSec = maxCardDetectedWaitingTimeSec;
            _waitingForSaleDecisionAction_MaxWaitingTimeSec = maxSaleDecisionWaitingTimeSec;

            if (_expectingCardInfoResponse_MaxWaitingTimeSec < 5)
                _expectingCardInfoResponse_MaxWaitingTimeSec = 5;

            if (_waitingForSaleDecisionAction_MaxWaitingTimeSec < 5)
                _waitingForSaleDecisionAction_MaxWaitingTimeSec = 5;


            _onDataReceivedNoteDelgHandle = new IM30COMPort.OnDataReceivedNoteDelg(DataReceivedNoteDelgWorking);

            _logStateDEBUGDelgHandler = debugShowMessageDelgHandler;
            _updateProcessStateDelgHandler = new UpdateProcessStateDelg(UpdateProcessStateDelgWorking);

            LastIM30CardSale = this;
        }

        public bool IsCardProcessFinished
        {
            get
            {
                if (_processEndingStatesList is null)
                    return true;

                else if (_processEndingStatesList.Count == 0)
                    return true;

                else if ((from stt in _processEndingStatesList
                     where stt == _processState
                     select stt).Count() == 0)
                    return false;

                else
                    return true;
            }
        }

        private void DataReceivedNoteDelgWorking()
        {
            if (IsDisposed)
                return;

            Thread tWorker = new Thread(new ThreadStart(DataReceivedNoteThreadWorking))
            {
                IsBackground = true,
                Priority = ThreadPriority.Highest
            };
            tWorker.Start();
            /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            void DataReceivedNoteThreadWorking()
            {
                _dataReceivedNotes.Enqueue(true);
            }
        }

        public void RequestStopTransaction(out bool isRequestSuccess)
        {
            isRequestSuccess = false;
            if (IsDisposed)
                return;

            if ((IsRequestStopTransaction.HasValue == false) && (_cardTransParameters is null))
            {
                IsRequestStopTransaction = true;
                isRequestSuccess = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="commandParam"></param>
        /// <returns>Return false when sale command parameter has been sent already or commandParam is null</returns>
        public bool Send2ndTransCommand(I2ndCardCommandParam commandParam)
        {
            if (IsDisposed)
                return false;

            if ((_cardTransParameters == null) && (commandParam != null) && (IsCurrentWorkingEnded == false))
            {
                _cardTransParameters = commandParam;
                PosTransID = _cardTransParameters.PosTransId;
                return true;
            }
            return false;
        }

        private bool SendStartTransCommand(IM30COMPort comPort, string loctTag, bool isResend = false)
        {
            bool isSendSuccess = false;
            try
            {
                IM30DataModel the1stDataModel = GetStartIM30Transaction();
                Log.LogText(_logChannel, PosTransID, the1stDataModel, $@"K03#COM Port: {COMPort}", "IM30CardSale.SendStartTransCommand");

                LogState("-- Command the1stDataModel --\r\n" + JsonConvert.SerializeObject(the1stDataModel, Formatting.Indented));
                byte[] the1stCommandData = IM30RequestResponseDataWorks.RenderData(the1stDataModel);

                LogState($@"**{DateTime.Now:HH:mm:ss.fff_fff_1}; Send Start Transaction Command - Start");
                Log.LogText(_logChannel, PosTransID, $@"**{DateTime.Now:HH:mm:ss.fff_fff_1}; Send Start Transaction Command - Start{"\r\n"}", 
                    $@"K05#COM Port: {COMPort}", "IM30CardSale.SendStartTransCommand");

                UpdateProcessStateDelgWorking(CardSaleProcessState.SendingStartTransCommand, "-Sending Start Trans. Command~", isReverseWorking: isResend);
                comPort.WriteDataPort(the1stCommandData, "IM30CardSale.StartTransaction(-Send Credit Card Sale Command~)");
                isSendSuccess = true;

                LogState($@"**{DateTime.Now:HH:mm:ss.fff_fff_1}; Send Start Transaction Command - End{"\r\n"}");
                Log.LogText(_logChannel, PosTransID, $@"**{DateTime.Now:HH:mm:ss.fff_fff_1}; Send Start Transaction Command - End",
                    $@"K07#COM Port: {COMPort}", "IM30CardSale.SendStartTransCommand");

                UpdateProcessStateDelgWorking(CardSaleProcessState.ExpectingAckAfterStartTransCommand, "-Sending Start Trans. Command~"
                    , isReverseWorking: isResend, isForceToSetNewACKTimeout: isResend);
                SetTimeout(_expectingCardInfoResponse_MaxWaitingTimeSec, "-New timeout for StartTransaction~");
            }
            catch (Exception ex)
            {
                Log.LogError(_logChannel, PosTransID, new Exception($@"{ex.Message}; COM Port: {COMPort}; -Send Start Trans. Command Error; Fail to Start Card Trans.~; {ex.Message}", ex), 
                    "EX01", "IM30CardSale.SendStartTransCommand");
            }

            return isSendSuccess;

            /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            IM30DataModel GetStartIM30Transaction()
            {
                IM30DataModel msgData = IM30RequestResponseDataWorks.CreateNewMessageData
                            (RequestResponseIndicatorDef.RequestAndResponse, TransactionCodeDef.StartTransaction, ResponseCodeDef.Approved, MoreIndicatorDef.LastMessage);

                msgData.AddFieldElement(1, FieldTypeDef.Direction, ((int)_gateDirection.Value).ToString());
                msgData.AddFieldElement(2, FieldTypeDef.KomFirstSPNo, _komLinkFirstSPNo.Value.ToString());
                msgData.AddFieldElement(3, FieldTypeDef.KomSecondSPNo, _komLinkSecondSPNo.Value.ToString());

                return msgData;
            }
        }

        private bool SendStopTransCommand(IM30COMPort comPort, string loctTag)
        {
            if (_isEndProcessWithoutStopCommand)
                return false;

            if (_isStopTransactionCommandHasSent)
                return false;

            if (string.IsNullOrWhiteSpace(loctTag))
                loctTag = "..unknown location tag.";

            string msgLog = "";

            _isStopTransactionCommandHasSent = true;
            _stopCommandSendingCount++;

            IM30DataModel theStopDataModel = IM30RequestResponseDataWorks.CreateNewMessageData
                        (RequestResponseIndicatorDef.RequestAndResponse, TransactionCodeDef.StopTransaction, ResponseCodeDef.Approved, MoreIndicatorDef.LastMessage);

            LogState($@"-- Command theStopDataModel --Loct.:{loctTag}--{"\r\n"}" + JsonConvert.SerializeObject(theStopDataModel, Formatting.Indented));
            Log.LogText(_logChannel, PosTransID, theStopDataModel, $@"K03#COM Port: {COMPort}", "IM30CardSale.SendStopTransCommand");

            byte[] theStopCommandData = IM30RequestResponseDataWorks.RenderData(theStopDataModel);

            msgLog += $@"**{DateTime.Now:HH:mm:ss.fff_fff_1}; Send Stop Transaction - Start{"\r\n"}; COM Port: {COMPort}";
            UpdateProcessStateDelgWorking(CardSaleProcessState.SendingStopTransCommand, "-Sending Stop Transaction Command (S01)~");
            comPort?.WriteDataPort(theStopCommandData, "IM30CardSale.<testing X01>(-Send Stop Transaction Command~)");
            msgLog += $@"**{DateTime.Now:HH:mm:ss.fff_fff_1}; Send Stop Transaction - End{"\r\n"}";
            msgLog += $@"{"\r\n"}Loct.:{loctTag}";
            LogState(msgLog);
            Log.LogText(_logChannel, PosTransID, msgLog, $@"K05#COM Port: {COMPort}", "IM30CardSale.SendStopTransCommand");

            return true;
        }

        public IIM30TransResult NewErrFinalResult(string errorMessage)
        {
            return new IM30CardSaleResult(new Exception(errorMessage), null);
        }

        public bool StartTransaction(out Exception error)
        {
            error = null;

            // Execution Validation
            if (IsDisposed)
            {
                error = new Exception("-Card Sale Reader object already disposed~");
                return false;
            }
            else if (_processState != CardSaleProcessState.New)
            {
                if (IsCardProcessFinished || IsCurrentWorkingEnded)
                {
                    error = new Exception($@"-Card Sale Reader error. Card Sale command already finished~ Process ID : {PosTransID}");
                }
                else
                {
                    error = new Exception($@"-Card Sale Reader error. Card Sale command has already started~ Process ID : {PosTransID}");
                }
                return false;
            }
            //--------------------------------------------------------------------------------------------------------------------------------
                        
            bool isTimeout = false;
            UpdateProcessStateDelgWorking(CardSaleProcessState.Start, "-Start (S01)~");
            Exception transError = null;
            _isMainProcessHasAlreadyStop = false;
            _startTransactionThread = new Thread(new ThreadStart(new Action(() =>
            {
                string msgLog = "";
                _transQuitTime = DateTime.Now.AddSeconds(_noActionMaxWaitSec);
                bool isEndProcessRequestedOnBusyNAKReceived = false;

                try
                {
                    LogState($@"--------- Start Card Reader COM Port Sequences ---------; Process Id: {PosTransID}", showToUI: true);
                    Log.LogText(_logChannel, PosTransID, $@"COM Port: {COMPort}; --------- Start Card Reader COM Port Sequences : IM30CardSale ---------; Process Id: {PosTransID}",
                        "B01", "IM30CardSale.StartTransaction");

                    using (IM30COMPort im30Port = new IM30COMPort(COMPort, PosTransID, _onDataReceivedNoteDelgHandle, _logStateDEBUGDelgHandler))
                    {
                        msgLog = $@"{DateTime.Now:HH:mm:ss.fff_fff_1}; OpenPort - Start; COM Port: {COMPort}{"\r\n"}";
                        UpdateProcessStateDelgWorking(CardSaleProcessState.OpeningPort, "~Opening Card Reader Port~");
                        im30Port.OpenPort();
                        msgLog += $@"**{DateTime.Now:HH:mm:ss.fff_fff_1}; OpenPort - End{"\r\n"}";
                        LogState(msgLog);
                        Log.LogText(_logChannel, PosTransID, msgLog, "B03", "IM30CardSale.StartTransaction");

                        bool isStartSuccess = SendStartTransCommand(im30Port, "Start init. new Start transaction command");

                        msgLog = "";
                        
                        bool tmpResult = false;
                        bool isProceedToStopChecking = true; /*This flag used to protected process from being hang or 'endless loop'*/
                        DateTime? stopReqTransTimeout = null;
                        byte[] recData = null;
                        int resendStartTransCommandCount = 0;
                        int resend2ndCommandCount = 0;
                        int resendStopCardTransCommandCount = 0;

                        try
                        {
                            while ((IsCurrentWorkingEnded == false) && (IsCardProcessFinished == false) && (IsDisposed == false) && (_processEndingStatesList?.Count > 0))
                            {
                                isProceedToStopChecking = true;

                                recData = null;
                                if (_dataReceivedNotes.Count > 0)
                                {
                                    _dataReceivedNotes.TryDequeue(out _);
                                    //----------------------------------------------------------
                                    // Read Data
                                    recData = ReadCOMPortData(im30Port);
                                }

                                if (recData?.Length > 0)
                                {
                                    isProceedToStopChecking = false;

                                    //----------------------------------------------------------
                                    // Process Response Data
                                    CardSaleProcessState expectedNextProcessState = _processState;
                                    if (_cardTypeSaleProcess == null)
                                    {
                                        ProcessStartingResponseData(recData, im30Port, out CardSaleProcessState expectedNextProcessState1);
                                        expectedNextProcessState = expectedNextProcessState1;
                                    }
                                    else
                                    {
                                        _cardTypeSaleProcess.ProcessResponseData(recData, _processState,
                                            out bool? isEndWithOutStopCommand,
                                            out IIM30TransResult finalResult,
                                            out CardSaleProcessState expectedNextProcessState2);

                                        if (finalResult != null)
                                        {
                                            IsFinalResultFromReader = true;
                                            FinalResult = finalResult;
                                            IsPerfectCompleteEnd = true;
                                        }

                                        expectedNextProcessState = expectedNextProcessState2;

                                        if (isEndWithOutStopCommand == true)
                                            _isEndProcessWithoutStopCommand = true;
                                    }
                                    //----------------------------------------------------------
                                    // Proceed To Next Action
                                    if ((IsCardProcessFinished == false) && (_processState != expectedNextProcessState))
                                    {
                                        if (_cardTypeSaleProcess != null)
                                        {
                                            _cardTypeSaleProcess.ProceedAction(_processState, expectedNextProcessState);
                                        }
                                        else
                                        {
                                            if (expectedNextProcessState == CardSaleProcessState.ExpectingStopTransCommand)
                                            {
                                                bool isStopCommSent = false;
                                                if (_isEndProcessWithoutStopCommand == false)
                                                {
                                                    _isStopTransactionCommandHasSent = false;
                                                    isStopCommSent = SendStopTransCommand(im30Port, "-Stop Card Transaction API on system requested~");
                                                    IsRequestStopTransaction = false;
                                                    if (isStopCommSent)
                                                    {
                                                        UpdateProcessStateDelgWorking(CardSaleProcessState.ExpectingAckAfterStopTransCommand, "-StopTransaction on requested~", isReverseWorking: true);
                                                        SetTimeout(_waitingForStopCommandAck_MaxWaitingTimeSec, $@"-Timeout(E)~Stop Command Sending Count: {_stopCommandSendingCount}");
                                                    }
                                                }
                                                if (isStopCommSent == false)
                                                    UpdateProcessStateDelgWorking(CardSaleProcessState.ErrorHalt, "-StopTransaction on requested~");
                                            }
                                            else
                                            {
                                                UpdateProcessStateDelgWorking(expectedNextProcessState, "-Set ProcessState at ProcessResponseData (X)~");

                                                if (expectedNextProcessState == CardSaleProcessState.WaitingForSaleDecisionAction)
                                                {
                                                    SetTimeout(_waitingForSaleDecisionAction_MaxWaitingTimeSec, "-New timeout for SaleDecisionAction waiting~");
                                                }
                                            }
                                        }
                                    }
                                    else if ((IsCardProcessFinished == false)
                                        && (_cardTypeSaleProcess != null)
                                        && (_processState == expectedNextProcessState)
                                        && ((from stt in _allACKStatusList
                                             where stt == _processState
                                             select stt).Count() > 0)
                                    )
                                    {
                                        _cardTypeSaleProcess.ProceedAction(_processState, expectedNextProcessState, isRepeatACKRequested: true);
                                    }

                                    //----------------------------------------------------------
                                }
                                else if ((_processState == CardSaleProcessState.WaitingForSaleDecisionAction)
                                    && (_cardTypeSaleProcess is null)
                                    && (isTimeout == false)
                                    && (_stopCommandSendingCount == 0)
                                    && (IsRequestStopTransaction.HasValue == false)
                                    && (_isStopTransactionCommandHasSent == false)
                                    && (_cardTransParameters != null))
                                {
                                    if (_cardTransParameters.TransactionType == TransactionTypeEn.CreditCard_2ndComm)
                                    {
                                        isProceedToStopChecking = false;
                                        _cardTypeSaleProcess = new IM30CardSale_CreditDebitCard(im30Port, _cardTransParameters, _updateProcessStateDelgHandler, _logStateDEBUGDelgHandler);
                                        IsCardCommandSent = true;
                                        _cardTypeSaleProcess.Send2ndCommand();
                                        SetTimeout(CreditDebitCard_MaxWaitingTimeSec, "-New timeout for ChargeCreditDebitCard~");
                                    }
                                    else if ((_cardTransParameters.TransactionType == TransactionTypeEn.TouchNGo_2ndComm_CheckIn)
                                            ||
                                            (_cardTransParameters.TransactionType == TransactionTypeEn.TouchNGo_2ndComm_Checkout)
                                    )
                                    {
                                        isProceedToStopChecking = false;
                                        _cardTypeSaleProcess = new IM30CardSale_TnGCard(im30Port, _cardTransParameters, _updateProcessStateDelgHandler, _logStateDEBUGDelgHandler);
                                        _cardTypeSaleProcess.Send2ndCommand();
                                        SetTimeout(TngCard_MaxWaitingTimeSec, "-New timeout for TouchNGoCard~");
                                    }
                                }
                                /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                                /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                                ///// Stop IM30 if IM30 is NAK busy.
                                if (_processState == CardSaleProcessState.Busy)
                                {
                                    isEndProcessRequestedOnBusyNAKReceived = true;
                                    _isEndProcessWithoutStopCommand = true;
                                }
                                /////----------------------------------------------------------------------------------------------------------------------
                                ///// Check Stop Request, Timeout, and Sleep for a moment.
                                else if (isProceedToStopChecking)
                                {
                                    tmpResult = false;
                                    /////------------------------------------------------------------------------------------------------------------------
                                    ///// Timeout ACK ----------
                                    if (_ackTimeout.HasValue && (_ackTimeout.Value.Ticks < DateTime.Now.Ticks))
                                    {
                                        // ACK timeout when waiting for Ack After StartTransCommand
                                        if (_processState == CardSaleProcessState.ExpectingAckAfterStartTransCommand)
                                        {
                                            //QA:TESTED -- Start
                                            if (resendStartTransCommandCount > _maxSendingStartTransCommandCount)
                                            {
                                                UpdateProcessStateDelgWorking(CardSaleProcessState.ErrorHalt, $@"-Fail to card sale start transaction~");
                                            }
                                            else
                                            {
                                                _isStopTransactionCommandHasSent = false;
                                                SendStopTransCommand(im30Port, "-Timeout Start Trans. Command (A)~");
                                                //..the stable delay is 8 seconds
                                                Thread.Sleep(8000);
                                                im30Port.CleanUp();

                                                tmpResult = SendStartTransCommand(im30Port, "-Resent Start Trans.  Command (A)~", isResend: true);
                                                resendStartTransCommandCount++;
                                            }
                                            //QA:TESTED -- End
                                        }
                                        // ACK timeout when waiting for Ack After ChargeCreditDebitCardCommand
                                        else if (_processState == CardSaleProcessState.ExpectingAckAfterChargeCreditDebitCardCommand)
                                        {
                                            //QA:TESTED -- Start
                                            if (_cardTypeSaleProcess != null)
                                            {
                                                if (resend2ndCommandCount > _maxSending2ndCommandCount)
                                                {
                                                    UpdateProcessStateDelgWorking(CardSaleProcessState.ErrorHalt, $@"-Fail to card sale start transaction~",
                                                        isReverseWorking: true);
                                                }
                                                else
                                                {
                                                    resend2ndCommandCount++;
                                                    _cardTypeSaleProcess.Send2ndCommand(isResend: true);
                                                }
                                            }
                                            else
                                            {
                                                UpdateProcessStateDelgWorking(CardSaleProcessState.ErrorHalt, $@"-Fail to card sale start transaction~",
                                                           isReverseWorking: true);
                                            }
                                            //QA:TESTED -- End
                                        }
                                        // ACK timeout when waiting for Ack After StopTransCommand
                                        else if (_processState == CardSaleProcessState.ExpectingAckAfterStopTransCommand)
                                        {
                                            //QA:TESTED -- Start
                                            if (_stopCommandSendingCount > _maxSendingCardStopCommandCount)
                                            {
                                                UpdateProcessStateDelgWorking(CardSaleProcessState.ErrorHalt, $@"-Fail to start card sale transaction~"
                                                    , isReverseWorking: true, isForceToSetNewACKTimeout: true);
                                            }
                                            else
                                            {
                                                _isStopTransactionCommandHasSent = false;
                                                SendStopTransCommand(im30Port, "-Timeout Start Trans. Command (A)~");
                                                UpdateProcessStateDelgWorking(CardSaleProcessState.ExpectingAckAfterStopTransCommand
                                                    , $@"-Resend Stop Card Transaction Command~"
                                                    , isReverseWorking: true
                                                    , isForceToSetNewACKTimeout: true);
                                            }
                                            //QA:TESTED -- End
                                        }
                                        //Others considered error..
                                        else
                                        {
                                            //CYA-DEBUG .. Log error here
                                            UpdateProcessStateDelgWorking(CardSaleProcessState.ErrorHalt, $@"-Unknown state for checking ACK timeout~ Process State: {_processState}" );
                                        }
                                    }
                                    ///// Timeout after 2nd Command has send; ACK/NAK not received or Final Card Response not received; Will use Get-Last-Transaction to get last result ----------
                                    else if ((DateTime.Now.Ticks > _transQuitTime.Ticks) && (_cardTypeSaleProcess != null))
                                    {
                                        isTimeout = true;
                                        UpdateProcessStateDelgWorking(CardSaleProcessState.Timeout, "-Card Transaction timeout (A)~");
                                    }
                                    /////------------------------------------------------------------------------------------------------------------------
                                    ///// Timeout, End process immediately 
                                    else if ((DateTime.Now.Ticks > _transQuitTime.Ticks) && (_isEndProcessWithoutStopCommand == true))
                                    {
                                        isTimeout = true;
                                        UpdateProcessStateDelgWorking(CardSaleProcessState.Timeout, "-Card Transaction timeout (B)~");
                                    }
                                    /////------------------------------------------------------------------------------------------------------------------
                                    ///// Stop Transaction Requested; Send Stop Command when Stop Transaction Requested 
                                    else if ((IsRequestStopTransaction == true)
                                            && (_cardTypeSaleProcess is null)
                                            && (_isEndProcessWithoutStopCommand == false)
                                            && (_isStopTransactionCommandHasSent == false))
                                    {
                                        //QA:TESTED -- Start
                                        if (stopReqTransTimeout.HasValue == false)
                                        {
                                            stopReqTransTimeout = DateTime.Now.AddSeconds(1);
                                            Thread.Sleep(20);
                                        }
                                        else if (stopReqTransTimeout.Value.Ticks < DateTime.Now.Ticks)
                                        {
                                            tmpResult = SendStopTransCommand(im30Port, "-Stop Card Transaction API on manual stop requested~");
                                            IsRequestStopTransaction = false;

                                            if (tmpResult)
                                            {
                                                SetTimeout(_waitingForStopCommandAck_MaxWaitingTimeSec, $@"-Stop Card Trans. Requested~Stop Command Sending Count: {_stopCommandSendingCount}");
                                                UpdateProcessStateDelgWorking(CardSaleProcessState.ExpectingAckAfterStopTransCommand, "-StopTransaction on requested~", 
                                                    isReverseWorking: true, isForceToSetNewACKTimeout: true);
                                            }
                                            else
                                                Thread.Sleep(20);
                                        }
                                        else
                                        {
                                            Thread.Sleep(20);
                                        }
                                        //QA:TESTED -- End
                                    }
                                    /////------------------------------------------------------------------------------------------------------------------
                                    ///// Timeout, send Stop Transaction Command 
                                    else if ((DateTime.Now.Ticks > _transQuitTime.Ticks) && (_isStopTransactionCommandHasSent == false) && (_isEndProcessWithoutStopCommand == false))
                                    {
                                        //QA:TESTED -- Start
                                        isTimeout = true;
                                        bool isStopCommSent = false;
                                        if ((from stt in _timeoutThenSendStopCommandStatusList
                                             where stt == _processState
                                             select stt).Count() > 0)
                                        {
                                            _isStopTransactionCommandHasSent = false;
                                            isStopCommSent = SendStopTransCommand(im30Port, "-Stop Card Transaction API on timeout~");
                                            IsRequestStopTransaction = false;

                                            if (isStopCommSent)
                                            {
                                                UpdateProcessStateDelgWorking(CardSaleProcessState.ExpectingAckAfterStopTransCommand, "-StopTransaction on requested~"
                                                    , isReverseWorking: true, isForceToSetNewACKTimeout: true);
                                            }
                                            else
                                            {
                                                // CYA-DEBUG .. log error here
                                                Thread.Sleep(20);
                                            }
                                        }

                                        if (isStopCommSent == false)
                                        {
                                            UpdateProcessStateDelgWorking(CardSaleProcessState.Timeout, "-Card Transaction timeout (F)~");
                                        }
                                        //QA:TESTED -- End
                                    }
                                    else
                                    {
                                        Thread.Sleep(10);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            //CYA-DEBUG .. Log error here..
                        }

                        //if (IsDisposed)
                        //{
                        if (_processState == CardSaleProcessState.ErrorHaltWithoutStopCommand)
                        {
                            //By Pass
                        }
                        else if (
                            ((_processState == CardSaleProcessState.ErrorHalt) 
                                ||
                                ((_processState >= CardSaleProcessState.ExpectingCardInfoResponse) && (_processState < CardSaleProcessState.IndividualCardTypeCommand))
                            ) 
                            && 
                            (_cardTypeSaleProcess is null)
                            )
                        {
                            _isStopTransactionCommandHasSent = false;
                            tmpResult = SendStopTransCommand(im30Port, "-Stop Card Transaction API on object disposed~");
                            if (tmpResult)
                                Thread.Sleep(8000);
                        }

                        im30Port.CleanUp();

                        if (IsDisposed)
                            UpdateProcessStateDelgWorking(CardSaleProcessState.AppDisposed, "-Card Reader Application disposed~");
                        //}
                    }
                }
                catch (Exception ex)
                {
                    transError = ex;
                    UpdateProcessStateDelgWorking(CardSaleProcessState.ErrorHalt, "-Error at Global Thread (T1)~");
                    LogState($@"'-Card Sale Reader Error; Fail Card Sale~'; {ex.Message}; Last State : {_processState}; Process ID : {PosTransID}");
                    Log.LogError(_logChannel, PosTransID, new Exception($@"{ex.Message}; COM Port: {COMPort}; -Card reader Error; Fail Card Sale~; Last State : {_processState}", ex), 
                        "EX10", "IM30CardSale.StartTransaction");
                }
                finally
                {
                    // ----------------------------------------------------------
                    // Manage outstanding messages
                    if (msgLog?.Length > 2)
                    {
                        LogState($@"-Card Sale reader; Suspected Outstanding Messages~{"\r\n"}{msgLog}{"\r\n"}Last State : {_processState}");
                        Log.LogText(_logChannel, PosTransID, $@"COM Port: {COMPort}; -Card Sale; Suspected Outstanding Messages~{"\r\n"}{msgLog}{"\r\n"}Last State : {_processState}",
                            "X25", "IM30CardSale.StartTransaction", AppDecorator.Log.MessageType.Error);
                        msgLog = null;
                    }
                    // ----------------------------------------------------------
                    // Manage Unexpected Result
                    if (FinalResult is null)
                    {
                        if (isEndProcessRequestedOnBusyNAKReceived)
                        {
                            FinalResult = new IM30CardSaleResult(new Exception($@"-Fail Card Sale; Reader busy (A)~"), null);
                        }
                        else if (isTimeout)
                        {
                            FinalResult = new IM30CardSaleResult(isManualStopped: false, isTimeout: true);
                        }
                        else if (IsDisposed || (_processState == CardSaleProcessState.CommandStopEnding))
                        {
                            FinalResult = new IM30CardSaleResult(isManualStopped: true);
                        }
                        else if (transError is null)
                        {
                            if (_processState == CardSaleProcessState.Busy)
                                FinalResult = new IM30CardSaleResult(new Exception($@"-Fail Card Sale; Card Sale Reader busy (B)~"), null);
                            else
                                FinalResult = new IM30CardSaleResult(new Exception($@"-Fail Card Sale (Card Sale Reader); Unable to work properly; (A)~'; Last Process State : {_processState}"), null);

                        }
                        else
                            FinalResult = new IM30CardSaleResult(new Exception($@"-Fail Card Sale (Card Sale Reader); Unable to work properly; (B)~; Last Process State : {_processState}", transError), null);
                    }
                    // ----------------------------------------------------------
                    // Finalizing
                    if (IsTransStartSuccessful.HasValue == false)
                        IsTransStartSuccessful = false;

                    IsCurrentWorkingEnded = true;
                    try
                    {
                        OnTransactionFinishedDelgWorking(FinalResult);
                    }
                    catch(Exception ex2)
                    {
                        //CYA-DEBUG .. handle error here
                    }
                    
                    // ----------------------------------------------------------
                    LogState($@"--------- End Card Reader COM Port Sequences ---------; Process ID : {PosTransID}", showToUI: true);
                    Log.LogText(_logChannel, PosTransID, $@"COM Port: {COMPort}; --------- End Card Reader COM Port Sequences : IM30CardSale ---------", "H01", "IM30CardSale.StartTransaction");
                    _isMainProcessHasAlreadyStop = true;
                }
            })))
            { IsBackground = true };
            _startTransactionThread.Start();

            return true;
            /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            byte[] ReadCOMPortData(IM30COMPort comPortX)
            {
                byte[] retData = null;
                try
                {
                    if (comPortX.BytesToRead == 0)
                        return null;

                    retData = comPortX.ReadPort(3);
                }
                catch (Exception ex)
                {
                    if (retData?.Length > 0)
                    {
                        string hisDataStr = IM30COMPort.AsciiOctets2String(retData);
                        Log.LogError(_logChannel, PosTransID, new Exception($@"{ex.Message}; COM Port: {COMPort}; Data Txt: {hisDataStr}", ex), "EX01", "IM30CardSale.ReadCOMPortData");
                    }

                    retData = null;
                }
                return retData;
            }
        }

        private void SetTimeout(int newTimeoutSec, string locationTag)
        {
            if (string.IsNullOrWhiteSpace(locationTag))
                locationTag = "-*~";

            DateTime newTimeout = DateTime.Now.AddSeconds(newTimeoutSec);
            _transQuitTime = newTimeout;
            string sMsg = $@"COM Port: {COMPort}; Change Card Reader timeout at =={locationTag}==; New Trans. Timeout: {newTimeout:yyyy-MM-dd HH:mm:ss}; New Delay Second: {newTimeoutSec}";
            LogState(sMsg);
            Log.LogText(_logChannel, PosTransID, sMsg, "A01", "IM30CardSale.SetTimeout");
        }

        private void OnTransactionFinishedDelgWorking(IIM30TransResult finalResult)
        {
            OnTransactionFinishedDelg onTransactionFinishedHandle = _onTransactionFinishedHandler;

            if (onTransactionFinishedHandle == null)
                return;

            Thread tWorker = new Thread(new ThreadStart(new Action(() =>
            {
                try
                {
                    onTransactionFinishedHandle?.Invoke(finalResult);
                }
                catch (Exception ex)
                {
                    //CYA-DEBUG
                }
                finally
                {
                    onTransactionFinishedHandle = null;
                }
            })))
            {
                IsBackground = true,
                Priority = ThreadPriority.AboveNormal
            };
            tWorker.Start();
        }

        private void OnCardDetectedDelgDelgWorking(IM30DataModel cardInfo)
        {
            OnCardDetectedDelg onCardDetectedDelgHandler = _onCardDetectedDelgHandler;

            if (onCardDetectedDelgHandler == null)
                return;

            Thread tWorker = new Thread(new ThreadStart(new Action(() =>
            {
                try
                {
                    onCardDetectedDelgHandler?.Invoke(cardInfo);
                }
                catch (Exception ex)
                {
                    //CYA-DEBUG
                }
                finally
                {
                    onCardDetectedDelgHandler = null;
                }
            })))
            {
                IsBackground = true,
                Priority = ThreadPriority.AboveNormal
            };
            tWorker.Start();
        }

        private int _sendNAKCnt = 0;
        private void ProcessStartingResponseData(byte[] recData, IM30COMPort comPort, out CardSaleProcessState expectedNextProcessState)
        {
            expectedNextProcessState = _processState;

            if ((recData == null) || (recData?.Length == 0))
                return;

            else if (IsDisposed)
                return;

            string dStr = "";
            try
            {
                LogState($@"Data received .....");
                Log.LogText(_logChannel, PosTransID, $@"COM Port: {COMPort}; Data Recv.Length: {recData.Length}", "A01", "IM30CardSale.ProcessStartingResponseData");
                //-----------------------------------------------------------------------------
                if (recData.Length == 1)
                {
                    dStr = IM30Tools.TranslateAsciiCode(recData[0]);
                    LogState($@"COM Port: {COMPort}; Received Char : {dStr}");
                    Log.LogText(_logChannel, PosTransID, $@"COM Port: {COMPort}; Received Char : {dStr}", "B01", "IM30CardSale.ProcessStartingResponseData");

                    /////-----------------------------------------------------------------------------------------------
                    ///// ACK
                    if ((int)ASCIICodeEn.ACK == (int)recData[0])
                    {
                        Log.LogText(_logChannel, PosTransID, $@"COM Port: {COMPort}; Received Char : {dStr}", "B50", "IM30CardSale.ProcessStartingResponseData");

                        if (_processState == CardSaleProcessState.ExpectingAckAfterStartTransCommand)
                        {
                            IsTransStartSuccessful = true;
                            //CYA-DEBUG .. Test ACK/NAK not received ----------------------------------------------------------------------
                            //expectedNextProcessState = CardSaleProcessState.ExpectingAckAfterStartTransCommand;
                            //-------------------------------------------------------------------------------------------------------------
                            expectedNextProcessState = CardSaleProcessState.ExpectingCardInfoResponse;
                            LogState($@"COM Port: {COMPort}; -Please touch card .....~");
                            Log.LogText(_logChannel, PosTransID, $@"COM Port: {COMPort}; -Please touch card .....~", "C01", "IM30CardSale.ProcessStartingResponseData");
                            //--------------------------------------------------------------------------------------
                        }
                        else if (_processState == CardSaleProcessState.ExpectingAckAfterStopTransCommand)
                        {
                            //CYA-DEBUG .. Test ACK/NAK not received ----------------------------------------------------------------------
                            /////expectedNextProcessState = CardSaleProcessState.ExpectingAckAfterStopTransCommand;
                            //-------------------------------------------------------------------------------------------------------------
                            expectedNextProcessState = CardSaleProcessState.CommandStopEnding;
                            IsPerfectCompleteEnd = true;
                        }
                        else
                        {
                            LogState($@"-Unexpected ACK card reader reading~");
                            Log.LogText(_logChannel, PosTransID, $@"COM Port: {COMPort}; -Please touch card .....~", "E01", "IM30CardSale.ProcessStartingResponseData");
                        }
                    }

                    /////-----------------------------------------------------------------------------------------------
                    ///// NAK
                    else if ((int)ASCIICodeEn.NAK == (int)recData[0])
                    {
                        if (_processState == CardSaleProcessState.ExpectingAckAfterStartTransCommand)
                        {
                            LogState($@"-Card reader is busy~' Previous State :  {_processState}");
                            Log.LogText(_logChannel, PosTransID, $@"COM Port: {COMPort}; -Card reader is NAK busy~' Current State :  {_processState}; Received Char : {dStr}", "F05A", "IM30CardSale.ProcessStartingResponseData");

                            expectedNextProcessState = CardSaleProcessState.Busy;
                        }
                        else if (_processState == CardSaleProcessState.ExpectingAckAfterStopTransCommand)
                        {
                            Log.LogText(_logChannel, PosTransID, $@"COM Port: {COMPort}; -Card NAK AfterStopTransCommand sent~' Current State :  {_processState}; Received Char : {dStr}", "F05B", "IM30CardSale.ProcessStartingResponseData");

                            if ((_stopCommandSendingCount < MaxStopCommandSendingCount) && (_isEndProcessWithoutStopCommand == false))
                            {
                                expectedNextProcessState = CardSaleProcessState.ExpectingStopTransCommand;
                            }
                            else
                            {
                                LogState($@"COM Port: {COMPort}; -Card reader is out of resend Stop Command count~'");
                                Log.LogText(_logChannel, PosTransID, $@"COM Port: {COMPort}; Receive NAK; -Card reader is out of resend Stop Command count~'", "F05C", "IM30CardSale.ProcessStartingResponseData");

                                expectedNextProcessState = CardSaleProcessState.ErrorHalt;
                            }
                        }
                    }
                    else
                    {
                        LogState($@"COM Port: {COMPort}; -Unregconized card reader reading~' Char : {dStr}");
                        Log.LogText(_logChannel, PosTransID, $@"COM Port: {COMPort}; -Unrecognized card reader reading~' Char : {dStr}",
                            "X10", "IM30CardSale.ProcessStartingResponseData", AppDecorator.Log.MessageType.Error);
                    }
                }
                //------------------------------------------------------------------------------
                // Invalid Data Length
                else if (recData.Length < 5)
                {
                    dStr = $@"Received Unknown Data (Hex) : {BitConverter.ToString(recData)}{"\r\n"}";
                    dStr += $@"Received Unknown Data (Text) : {System.Text.Encoding.ASCII.GetString(recData)}";
                    LogState($@"{dStr}");
                    Log.LogError(_logChannel, PosTransID, new Exception($@"COM Port: {COMPort}; {dStr}"), "X05", "IM30CardSale.ProcessStartingResponseData");
                }
                //------------------------------------------------------------------------------
                // Validate Serial Data
                else
                {
                    // Pack Raw Data into IM30DataModel ----------------------------------------
                    if (IM30RequestResponseDataWorks.ConvertToIM30DataModel(recData, out IM30DataModel im30DataMod, out Exception error2))
                    {
                        LogState($@"COM Port: {COMPort}; -- Response --\r\n" + JsonConvert.SerializeObject(im30DataMod, Formatting.Indented));
                        Log.LogText(_logChannel, PosTransID, im30DataMod, $@"K01:COM Port: {COMPort}", "IM30CardSale.ProcessStartingResponseData");

                        if (_processState == CardSaleProcessState.ExpectingCardInfoResponse)
                        {
                            if (CardInfoResult is null)
                            {
                                CardInfoResult = im30DataMod;
                                OnCardDetectedDelgDelgWorking(im30DataMod);
                            }

                            comPort.WriteDataPort(PortProtocalDef.ACKData, "ACK to Card Info response receiving");

                            if (TransactionCodeDef.IsEqualTrans(im30DataMod.TransactionCode, TransactionCodeDef.StartTransaction))
                            {
                                if (ResponseCodeDef.IsEqualResponse(im30DataMod.ResponseCode, ResponseCodeDef.Approved) == false)
                                {
                                    _isEndProcessWithoutStopCommand = true;
                                    expectedNextProcessState = CardSaleProcessState.ErrorHaltWithoutStopCommand;
                                }
                                else
                                {
                                    expectedNextProcessState = CardSaleProcessState.WaitingForSaleDecisionAction;
                                }
                            }
                        }
                        else
                        {
                            LogState($@"Unhandle process at state {_processState}");
                            Log.LogText(_logChannel, PosTransID, $@"Unhandle process at state {_processState}", $@"K05:COM Port: {COMPort}", "IM30CardSale.ProcessStartingResponseData");
                        }
                        //ValidateSaleResponse(im30DataMod, out bool isSaleFinalRespX, out bool isSaleSuccessX);
                    }
                    // Error Found --------------------------------------------------------------
                    else if (error2 != null)
                    {
                        if (im30DataMod != null)
                        {
                            LogState("-- Response --\r\n" + JsonConvert.SerializeObject(im30DataMod, Formatting.Indented));
                            Log.LogText(_logChannel, PosTransID, im30DataMod, $@"K17:COM Port: {COMPort}", "IM30CardSale.ProcessStartingResponseData", AppDecorator.Log.MessageType.Error);
                        }

                        if ((_sendNAKCnt == 0) && (_processState == CardSaleProcessState.ExpectingCardInfoResponse))
                        {
                            _sendNAKCnt++;
                            expectedNextProcessState = CardSaleProcessState.ExpectingCardInfoResponse;
                            comPort.WriteDataPort(PortProtocalDef.NAKData, "-NAK to Card Info Response result receiving~");
                        }
                        else
                        {
                            if ((_sendNAKCnt > 0) && (_processState == CardSaleProcessState.ExpectingCardInfoResponse))
                            {
                                _sendNAKCnt++;
                                comPort.WriteDataPort(PortProtocalDef.NAKData, "-NAK to 2nd Card Info Response to end transaction~");
                                _isEndProcessWithoutStopCommand = true;
                                expectedNextProcessState = CardSaleProcessState.ErrorHaltWithoutStopCommand;
                            }
                            else
                                expectedNextProcessState = CardSaleProcessState.ErrorHalt;

                            if (FinalResult is null)
                                FinalResult = new IM30CardSaleResult(error2, im30DataMod);

                            LogState("-- Error --\r\n" + JsonConvert.SerializeObject(error2, Formatting.Indented));
                            Log.LogText(_logChannel, PosTransID, error2, $@"ED1#COM Port: {COMPort}", "IM30CardSale.ProcessStartingResponseData", AppDecorator.Log.MessageType.Error);
                        }
                    }
                    // Unknown Error ------------------------------------------------------------
                    else
                    {
                        if (FinalResult is null)
                            FinalResult = new IM30CardSaleResult(new Exception($@"-Unknown data reading error from card reader~ Last Process State : {_processState}"), null);

                        expectedNextProcessState = CardSaleProcessState.ErrorHalt;
                        LogState("-- Unknown Error --\r\n" + "-Unknown error when reading data from card reader~");
                        Log.LogText(_logChannel, PosTransID, "-Unknown Error when reading data from card reader~", 
                            $@"X30#COM Port: {COMPort}", "IM30CardSale.ProcessStartingResponseData", AppDecorator.Log.MessageType.Error);

                        dStr = $@"Error; 
Received Unknown Data (Hex) : {BitConverter.ToString(recData)}
Received Unknown Data (Text) : {System.Text.Encoding.ASCII.GetString(recData)}";
                        LogState($@"{dStr}");
                        Log.LogText(_logChannel, PosTransID, $@"COM Port: {COMPort}; {dStr}",
                            "X21", "IM30CardSale.ProcessStartingResponseData", AppDecorator.Log.MessageType.Error);

                        Log.LogText(_logChannel, PosTransID, $@"COM Port: {COMPort} -Unknown Error when reading data from card reader~",
                            "X22", "IM30CardSale.ProcessStartingResponseData", AppDecorator.Log.MessageType.Error);
                    }
                }
                //--------------------------------------------------------------------------------
            }
            catch (Exception ex)
            {
                Log.LogError(_logChannel, PosTransID, new Exception($@"{ex.Message}; COM Port: {COMPort}", ex), "EX20", "IM30CardSale.ProcessStartingResponseData");
            }
            return;
            /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            ///
            void ValidateSaleResponse(IM30DataModel im30DataModelX, out bool isSaleFinalResponseX, out bool isSaleSuccessfulX)
            {
                isSaleFinalResponseX = false;
                isSaleSuccessfulX = false;

                if (im30DataModelX == null)
                    return;

                if (TransactionCodeDef.IsEqualTrans(TransactionCodeDef.ChargeAmount, im30DataModelX.TransactionCode))
                {
                    isSaleFinalResponseX = true;

                    if (ResponseCodeDef.IsEqualResponse(ResponseCodeDef.Approved, im30DataModelX.ResponseCode))
                        isSaleSuccessfulX = true;
                    else
                        isSaleSuccessfulX = false;
                }
            }
        }

        private void LogState(string logMsg, bool showToUI = false)
        {
            if (IsDisposed)
                return;

            _logStateDEBUGDelgHandler?.Invoke(logMsg);
        }

        private object _updateProcessStateLock = new object();
        private void UpdateProcessStateDelgWorking(CardSaleProcessState latestState, 
            string locationTag = null, bool isReverseWorking = false, bool isForceToSetNewACKTimeout = false)
        {
            if (IsDisposed)
                return;

            Thread tWorker = new Thread(new ThreadStart(UpdateProcessStateThreadWorking))
            {
                IsBackground = true,
                Priority = ThreadPriority.Highest
            };
            tWorker.Start();
            tWorker.Join();
            return;
            /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            void UpdateProcessStateThreadWorking()
            {
                lock (_updateProcessStateLock)
                {
                    bool hasChanged = false;
                    //CYA-DEBUG .. log parameter here ..

                    if (IsCardProcessFinished == true)
                    {
                        // By Pass .. Not allowed to change;
                    }
                    else if ((isReverseWorking) && (_processState != latestState))
                    {
                        _processState = latestState;
                        hasChanged = true;

                        Log.LogText(_logChannel, PosTransID, $@"COM Port: {COMPort}; CardSaleProcessState: {latestState}; Loct.:{locationTag}", "A01A", "IM30CardSale.UpdateProcessStateThreadWorking");
                    }
                    else if (latestState > _processState)
                    {
                        _processState = latestState;
                        hasChanged = true;

                        Log.LogText(_logChannel, PosTransID, $@"COM Port: {COMPort}; CardSaleProcessState: {latestState}; Loct.:{locationTag}", "A01B", "IM30CardSale.UpdateProcessStateThreadWorking");
                    }
                    //-----------------------------------------------------------
                    // Set ACK Timeout
                    if ((hasChanged || (isForceToSetNewACKTimeout))
                        &&
                        ((from stt in _allACKStatusList
                          where stt == _processState
                          select stt).Count() > 0)
                    )
                    {
                        SetACKTimeout(_ack_MaxWaitingTimeSec, locationTag);
                    }
                    // Remove ACK Timeout
                    else if (_ackTimeout.HasValue 
                            &&
                            ((from stt in _allACKStatusList
                                where stt == _processState
                                select stt).Count() == 0)
                    )
                    {
                        SetACKTimeout(null, locationTag);
                    }
                }
            }

            void SetACKTimeout(int? newTimeoutSec, string locationTag2)
            {
                if (string.IsNullOrWhiteSpace(locationTag2))
                    locationTag2 = "-*~";

                if (newTimeoutSec.HasValue)
                {
                    _ackTimeout = DateTime.Now.AddSeconds(newTimeoutSec.Value);
                    _transQuitTime = _transQuitTime.AddSeconds(newTimeoutSec.Value);

                    if (_transQuitTime.Ticks < _ackTimeout.Value.Ticks)
                        _transQuitTime = _ackTimeout.Value.AddSeconds(5);

                    if (_transQuitTime.Subtract(_ackTimeout.Value).TotalSeconds < 5)
                        _transQuitTime = _ackTimeout.Value.AddSeconds(5);

                    string ackMsg = $@"-Set ACK Card Reader timeout at~{locationTag2}; New ACK Timeout: {_ackTimeout:yyyy-MM-dd HH:mm:ss}; New Trans. Timeout: {_transQuitTime:yyyy-MM-dd HH:mm:ss}; New Delay Second: {newTimeoutSec}";
                    LogState(ackMsg);
                    Log.LogText(_logChannel, PosTransID, ackMsg, $@"A01:COM Port: {COMPort}", "IM30CardSale.SetACKTimeout");
                }
                else
                {
                    _ackTimeout = null;
                    LogState($@"-Remove ACK Card Reader timeout at~{locationTag2}");
                    Log.LogText(_logChannel, PosTransID, $@"-Remove ACK Card Reader timeout at~{locationTag2}", $@"A05:COM Port: {COMPort}", "IM30CardSale.SetACKTimeout");
                }
            }
        }

        public void ShutdownX()
        {
            try
            {
                if (IsDisposed == false)
                {
                    if (IsCurrentWorkingEnded == false)
                    {
                        //_msg.ShowMessage("Request Stop Sale Transaction");
                        Thread tWork = new Thread(new ThreadStart(new Action(() =>
                        {
                            RequestStopTransaction(out bool isRequestSuccess);
                            if (isRequestSuccess)
                                Thread.Sleep(1500);
                        })))
                        { IsBackground = true, Priority = ThreadPriority.Highest };
                        tWork.Start();
                        tWork.Join();
                        Dispose();
                    }
                    else
                    {
                        //_msg.ShowMessage("Existing Sale Transaction has already disposed");
                    }
                }
                else
                {
                    //_msg.ShowMessage("Existing Sale Transaction has not found");
                }
            }
            catch (Exception ex)
            {
                Dispose();
                //_msg.ShowMessage(ex.ToString());
            }
        }

        public static void Shutdown()
        {
            try
            {
                LastIM30CardSale?.ShutdownX();                
            }
            catch (Exception ex)
            {
                //_msg.ShowMessage(ex.ToString());
            }
        }

        public bool IsDisposed { get; private set; }
        public void Dispose()
        {
            //_msg // _msg is static and should not be null;
            // OnTransactionFinishedHandle should not dispose here ...
            if (IsDisposed == false)
            {
                IsDisposed = true;

                Thread tWork = new Thread(new ThreadStart(new Action(() =>
                {
                    DateTime tOut = DateTime.Now.AddSeconds(30);

                    while ((_isMainProcessHasAlreadyStop.HasValue) && (_isMainProcessHasAlreadyStop.Value == false) && (tOut.Ticks > DateTime.Now.Ticks))
                        Thread.Sleep(5);
                })))
                { IsBackground = true, Priority = ThreadPriority.Highest };
                tWork.Start();
                tWork.Join();

                if (_dataReceivedNotes != null)
                {
                    while (_dataReceivedNotes.TryDequeue(out _))
                    { }
                    _dataReceivedNotes = null;
                }

                _cardTypeSaleProcess?.Shutdown();

                if (((_startTransactionThread?.ThreadState & ThreadState.Aborted) == ThreadState.Aborted)
                    ||
                    ((_startTransactionThread?.ThreadState & ThreadState.AbortRequested) == ThreadState.AbortRequested)
                    ||
                    ((_startTransactionThread?.ThreadState & ThreadState.Stopped) == ThreadState.Stopped)
                    ||
                    ((_startTransactionThread?.ThreadState & ThreadState.StopRequested) == ThreadState.StopRequested)
                    ||
                    (_startTransactionThread is null)
                )
                {
                    _startTransactionThread = null;
                }
                else
                {
                    try
                    {
                        _startTransactionThread?.Abort();
                        Thread.Sleep(10);
                    }
                    catch { }
                    _startTransactionThread = null;
                }
                
                _isMainProcessHasAlreadyStop = null;

                _updateProcessStateDelgHandler = null;
                _onDataReceivedNoteDelgHandle = null;
                _logStateDEBUGDelgHandler = null;
                _cardTransParameters = null;
                CardInfoResult = null;
                FinalResult = null;
                IsRequestStopTransaction = null;
                _updateProcessStateLock = null;
                _onTransactionFinishedHandler = null;
                
                _ackTimeout = null;
                
                _processEndingStatesList?.Clear();
                _processEndingStatesList = null;

                _timeoutThenSendStopCommandStatusList.Clear();
                _timeoutThenSendStopCommandStatusList = null;

                _allACKStatusList.Clear();
                _allACKStatusList = null;

                _gateDirection = null;
                _komLinkFirstSPNo = null;
                _komLinkSecondSPNo = null;
                _log = null;
            }
        }

        

        public enum CardSaleProcessState
        {
            New = 0,
            Start = 1,
            OpeningPort = 2,

            SendingStartTransCommand = 3,
            ExpectingAckAfterStartTransCommand = 4,

            /// <summary>
            /// Waiting for customer touch card
            /// </summary>
            ExpectingCardInfoResponse = 21,

            /// <summary>
            /// Waiting for application decision action; This allowed program internal verification
            /// </summary>
            WaitingForSaleDecisionAction = 25,

            /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            /////xxxxx IndividualCardTypeCommand - 2nd Command xxxxx
            IndividualCardTypeCommand = 30,
            
            SendingTnGCheckinCommand = 41,
            SendingTnGCheckoutCommand = 42,

            SendingKomLinkCheckinCommand = 51,
            SendingKomLinkCheckoutCommand = 52,
            SendingKomLinkDeductValueCommand = 53,
            SendingKomLinkIncreaseValueCommand = 54,
            SendingKomLinkIssueNewCardCommand = 55,
            SendingKomLinkReadSeasonPassInfoCommand = 56,
            SendingKomLinkUpdateCardInfoCommand = 57,
            SendingKomLinkResetACGCheckingCommand = 58,
            SendingKomLinkIssueSeasonPassCommand = 59,
            SendingKomLinkBlacklistCardCommand = 60,
            SendingKomLinkRemoveBlacklistCardCommand = 61,
            SendingKomLinkCancelCardCommand = 62,
            SendingKomLinkRemoveCancelCardCommand = 63,

            SendingChargeCreditDebitCardCommand = 201,
            ExpectingAckAfterChargeCreditDebitCardCommand = 202,

            /// <summary>
            /// For TnG, KomLink, Credit/Debit Card
            /// </summary>
            ExpectingFinalCardResponse = 501,

            EndOfIndividualCardTypeCommand = 650,
            /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

            ExpectingStopTransCommand = 701,
            //ExpectingNAKStopTransCommand = 702,
            SendingStopTransCommand = 712,
            ExpectingAckAfterStopTransCommand = 713,

            /// <summary>
            /// Finishing the transaction process. On the way to end process
            /// </summary>
            FinishEnding = 1000,

            /// <summary>
            /// Stop Command has been sent
            /// </summary>
            CommandStopEnding = 1001,

            Timeout = 9000,

            /// <summary>
            /// Receive NAK after sent Start Transaction command; Without Stop Command needed
            /// </summary>
            Busy = 9010,  
            ///Malfunction = 9996,
            AppDisposed = 9997,
            ErrorHalt = 9998,
            ErrorHaltWithoutStopCommand = 9999
        }
    }
}
