using Newtonsoft.Json;
using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.Base;
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

namespace NssIT.Kiosk.Device.PAX.IM30.OrgAPI.Trans.StopTransaction
{
    public class IM30StopTransaction : IIM30Trans, IDisposable
    {
        private const string _logChannel = "IM30_API";

        private int _noActionMaxWaitSec = 30;
        private ConcurrentQueue<bool> _dataReceivedNotes = new ConcurrentQueue<bool>();
        private ShowMessageLogDelg _logStateDEBUGDelgHandle = null;
        private StopTransState _processState = StopTransState.New;
        private IM30COMPort.OnDataReceivedNoteDelg _onDataReceivedNoteDelgHandle = null;
        private OnTransactionFinishedDelg _onTransactionFinishedHandle = null;
        private List<StopTransState> _processEndingStatesList = new List<StopTransState>(new StopTransState[]
            {StopTransState.Busy, StopTransState.Ending, StopTransState.ErrorHalt, StopTransState.Timeout });
        private bool? _isMainProcessHasAlreadyStop = null;

        public bool IsCurrentWorkingEnded { get; private set; } = false;
        public bool? IsTransStartSuccessful { get; private set; } = null;
        public IIM30TransResult FinalResult { get; private set; } = null;
        public bool IsTransEndDisposed { get { return IsDisposed; } }
        public bool IsPerfectCompleteEnd { get; private set; } = false;

        public string COMPort { get; private set; }

        private string _transactionID = Guid.NewGuid().ToString();

        public Guid WorkingId { get; } = Guid.NewGuid();
        public TransactionTypeEn TransactionType => TransactionTypeEn.StopTrans;
        ///// Static Properties xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        public static IM30StopTransaction LastStopTransactionObj { get; private set; } = null;
        /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

        private DbLog _log = null;
        public DbLog Log
        {
            get
            {
                return _log ?? (_log = DbLog.GetDbLog());
            }
        }

        public IM30StopTransaction(string comPort, string transactionID,
            OnTransactionFinishedDelg onTransactionFinishedHandle, int noActionMaxWaitSec = 60
            , ShowMessageLogDelg debugShowMessageDelgHandler = null)
        {
            if (string.IsNullOrWhiteSpace(comPort))
                throw new Exception("Invalid COM port specification when send Stop Transaction command to card reader");

            COMPort = comPort;
            _onTransactionFinishedHandle = onTransactionFinishedHandle;
            _transactionID = transactionID;

            if (noActionMaxWaitSec > 10)
                _noActionMaxWaitSec = noActionMaxWaitSec;

            _onDataReceivedNoteDelgHandle = new IM30COMPort.OnDataReceivedNoteDelg(DataReceivedNoteDelgWorking);

            _logStateDEBUGDelgHandle = debugShowMessageDelgHandler;

            LastStopTransactionObj = this;
        }

        public bool IsEndingProcess
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

        public IM30DataModel GetNewIM30Data()
        {
            IM30DataModel msgData = IM30RequestResponseDataWorks.CreateNewMessageData
                        (RequestResponseIndicatorDef.RequestAndResponse, TransactionCodeDef.StopTransaction, ResponseCodeDef.Approved, MoreIndicatorDef.LastMessage);

            return msgData;
        }

        public IIM30TransResult NewErrFinalResult(string errorMessage)
        {
            return new IM30StopTransResult(new Exception(errorMessage));
        }

        public bool StartTransaction(out Exception error)
        {
            error = null;

            // Execution Validation
            if (IsDisposed)
            {
                error = new Exception("-Card reader Stop Transaction command already disposed~");
                return false;
            }
            else if (_processState != StopTransState.New)
            {
                if (IsEndingProcess || IsCurrentWorkingEnded)
                {
                    error = new Exception($@"-Card reader Stop Transaction command error. Stop Transaction command already finished~ Process ID : {_transactionID}");
                }
                else
                {
                    error = new Exception($@"-Card reader Stop Transaction command error. Stop Transaction command has already started~ Process ID : {_transactionID}");
                }
                return false;
            }
            //--------------------------------------------------------------------------------------------------------------------------------

            UpdateProcessState(StopTransState.Start, "-Start (S01)~");
            Exception transError = null;
            _isMainProcessHasAlreadyStop = false;
            Thread tWorker = new Thread(new ThreadStart(new Action(() =>
            {
                string msgLog = "";
                DateTime transQuitTime = DateTime.Now.AddSeconds(_noActionMaxWaitSec);

                try
                {
                    LogState($@"--------- Start Card Reader COM Port Sequences ---------; Process Id {_transactionID}");
                    Log.LogText(_logChannel, _transactionID, $@"COM Port: {COMPort}; --------- Start Card Reader COM Port Sequences : IM30StopTransaction ---------", "K01", "IM30StopTransaction.StartTransaction");

                    IM30DataModel dataModel = GetNewIM30Data();

                    LogState("-- Command --\r\n" + JsonConvert.SerializeObject(dataModel, Formatting.Indented));
                    Log.LogText(_logChannel, _transactionID, dataModel, $@"K03#COM Port: {COMPort}", "IM30StopTransaction.StartTransaction");

                    byte[] dataResult = IM30RequestResponseDataWorks.RenderData(dataModel);
                    msgLog = $@"{DateTime.Now:HH:mm:ss.fff_fff_1}; End Data Rendering";

                    msgLog = $@"{DateTime.Now:HH:mm:ss.fff_fff_1}; Start COM Port Transaction";
                    using (IM30COMPort im30Port = new IM30COMPort(COMPort, "-Stop Card Transaction~", _onDataReceivedNoteDelgHandle, _logStateDEBUGDelgHandle))
                    {
                        msgLog = $@"{DateTime.Now:HH:mm:ss.fff_fff_1}; OpenPort - Start; COM Port: {COMPort}; {"\r\n"}";
                        UpdateProcessState(StopTransState.OpeningPort, "-Opening Port (01)~");
                        im30Port.OpenPort();
                        msgLog += $@"{DateTime.Now:HH:mm:ss.fff_fff_1}; OpenPort - End{"\r\n"}";

                        msgLog += $@"{DateTime.Now:HH:mm:ss.fff_fff_1}; Send Stop Transaction command - Start{"\r\n"}";
                        UpdateProcessState(StopTransState.SendingStopTransactionCommand, "-Sending Stop Transaction command (A)~");
                        im30Port.WriteDataPort(dataResult, "-Sending Stop Card Transaction command (B)~");
                        msgLog += $@"{DateTime.Now:HH:mm:ss.fff_fff_1}; Send Stop Card Transaction command - End{"\r\n"}";
                        UpdateProcessState(StopTransState.ExpectingStopTransCommandAck, "-Expecting Stop Card Transaction Command Ack (AK01)~");

                        LogState(msgLog);
                        Log.LogText(_logChannel, _transactionID, msgLog, "K05", "IM30StopTransaction.StartTransaction");
                        msgLog = "";

                        byte[] recData = null;
                        while ((IsCurrentWorkingEnded == false) && (IsEndingProcess == false) && (IsDisposed == false) && (_processEndingStatesList?.Count > 0))
                        {
                            if (_dataReceivedNotes.Count > 0)
                            {
                                _dataReceivedNotes.TryDequeue(out _);
                                recData = null;
                                //----------------------------------------------------------
                                // Read Data
                                recData = ReadCOMPortData(im30Port);
                                //----------------------------------------------------------
                                // Process Data
                                ProcessResponseData(recData, im30Port);
                                //----------------------------------------------------------
                            }
                            else if (DateTime.Now.Ticks > transQuitTime.Ticks)
                            {
                                UpdateProcessState(StopTransState.Timeout, "-Reach Timeout (T1)~");
                            }
                            else
                            {
                                Thread.Sleep(20);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    transError = ex;
                    UpdateProcessState(StopTransState.ErrorHalt, "-Error at Global Thread (T1)~");
                    LogState($@"'-Card reader Error; Fail Stop Card Transaction~'; {ex.Message}; Last State : {_processState}; Process ID : {_transactionID}");
                    Log.LogError(_logChannel, _transactionID, new Exception($@"{ex.Message}; COM Port: {COMPort}; -Card reader Error; Fail Stop Card Reader Transaction~; Last State : {_processState}", ex), 
                        "EX10", "IM30StopTransaction.StartTransaction");
                }
                finally
                {
                    // ----------------------------------------------------------
                    // Manage outstanding messages
                    if (msgLog?.Length > 2)
                    {
                        LogState($@"-Stop Card Transaction; Suspected Outstanding Messages~{"\r\n"}{msgLog}{"\r\n"}Last State : {_processState}");
                        Log.LogText(_logChannel, _transactionID, $@"COM Port: {COMPort}; -Stop Card Reader Transaction; Suspected Outstanding Messages~{"\r\n"}{msgLog}{"\r\n"}Last State : {_processState}",
                            "G01", "IM30StopTransaction.StartTransaction", AppDecorator.Log.MessageType.Error);
                        msgLog = null;
                    }
                    // ----------------------------------------------------------
                    // Manage Unexpected Result
                    if (FinalResult is null)
                    {
                        if (transError is null)
                        {
                            if (_processState == StopTransState.Busy)
                                FinalResult = new IM30StopTransResult(new Exception($@"-Fail Stop Card Transaction; Reader busy~"));
                            else
                                FinalResult = new IM30StopTransResult(new Exception($@"-Fail Stop Card Transaction; Unable to work properly; (A)~'; Last Process State : {_processState}"));

                        }
                        else
                            FinalResult = new IM30StopTransResult(new Exception($@"-Fail Stop Card Transaction; Unable to work properly; (B)~; Last Process State : {_processState}", transError));
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
                    catch (Exception ex2)
                    {
                        Log.LogError(_logChannel, _transactionID, new Exception($@"{ex2.Message}; COM Port: {COMPort}", ex2), "EX01", "IM30StopTransaction.StartTransaction");
                    }
                    // ----------------------------------------------------------
                    LogState($@"--------- End Card Reader COM Port Sequences ---------; Process ID : {_transactionID}");
                    Log.LogText(_logChannel, _transactionID, $@"COM Port: {COMPort}; --------- End Card Reader COM Port Sequences : IM30StopTransaction ---------", "H01", "IM30StopTransaction.StartTransaction");
                    _isMainProcessHasAlreadyStop = true;
                }
            })))
            { IsBackground = true };
            tWorker.Start();

            return true;
            /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            ///
            byte[] ReadCOMPortData(IM30COMPort comPortX)
            {
                byte[] retData = null;
                try
                {
                    retData = comPortX.ReadPort(3);
                }
                catch (Exception ex)
                {
                    if (retData?.Length > 0)
                    {
                        string hisDataStr = IM30COMPort.AsciiOctets2String(retData);
                        Log.LogError(_logChannel, _transactionID, new Exception($@"{ex.Message}; COM Port: {COMPort}; Data Txt: {hisDataStr}", ex), "EX01", "IM30StopTransaction.ReadCOMPortData");
                    }

                    retData = null;
                }
                return retData;
            }
        }

        private void OnTransactionFinishedDelgWorking(IIM30TransResult finalResult)
        {
            if (_onTransactionFinishedHandle == null)
                return;

            Thread tWorker = new Thread(new ThreadStart(new Action(() =>
            {
                try
                {
                    _onTransactionFinishedHandle?.Invoke(finalResult);
                }
                catch (Exception ex)
                {
                    Log.LogError(_logChannel, _transactionID, new Exception($@"{ex.Message}; COM Port: {COMPort}", ex), "EX01", "IM30StopTransaction.OnTransactionFinishedDelgWorking");
                }
                finally
                {
                    _onTransactionFinishedHandle = null;
                }
            })))
            {
                IsBackground = true,
                Priority = ThreadPriority.AboveNormal
            };
            tWorker.Start();
            tWorker.Join();
        }

        private void ProcessResponseData(byte[] recData, IM30COMPort comPort)
        {
            if ((recData == null) || (recData?.Length == 0))
                return;

            else if (IsDisposed)
                return;

            string dStr = "";
            try
            {
                LogState($@"Data received .....");
                Log.LogText(_logChannel, _transactionID, $@"COM Port: {COMPort}; Data Recv.Length: {recData.Length}", "A01", "IM30StopTransaction.ProcessResponseData");
                //-----------------------------------------------------------------------------
                // Expect ACK
                if (recData.Length == 1)
                {
                    dStr = IM30Tools.TranslateAsciiCode(recData[0]);

                    if ((int)ASCIICodeEn.ACK == (int)recData[0]) 
                    {
                        IsTransStartSuccessful = true;
                        IsPerfectCompleteEnd = true;
                        FinalResult = new IM30StopTransResult();
                        LogState($@"Received Char : {dStr}");
                        Log.LogText(_logChannel, _transactionID, $@"COM Port: {COMPort}; Received Char : {dStr}", "B01", "IM30StopTransaction.ProcessResponseData");
                        UpdateProcessState(StopTransState.Ending, "-Expecting card activities (CA1)~");
                    }
                    else if ((int)ASCIICodeEn.NAK == (int)recData[0])
                    {
                        FinalResult = new IM30StopTransResult(new Exception($@"-Fail Stop Card Transaction; Reader busy (E1)~"));
                        LogState($@"-Card reader is busy~' Previous State :  {_processState}; Received Char : {dStr}");
                        Log.LogText(_logChannel, _transactionID, $@"COM Port: {COMPort}; -Card reader is busy~' Previous State :  {_processState}; Received Char : {dStr}", "B05", "IM30StopTransaction.ProcessResponseData");
                        UpdateProcessState(StopTransState.Busy, "-Busy Reading Found (D1)~");
                    }
                    else
                    {
                        FinalResult = new IM30StopTransResult(new Exception($@"-Fail Stop Card Transaction; Reader busy (E2)~Char : {dStr}"));
                        LogState($@"-Unregconized card reader reading~' Char : {dStr}");
                        Log.LogText(_logChannel, _transactionID, $@"COM Port: {COMPort}; -Unrecognized card reader reading~' Char : {dStr}",
                            "X10", "IM30StopTransaction.ProcessResponseData", AppDecorator.Log.MessageType.Error);

                        UpdateProcessState(StopTransState.Busy, "-Busy Reading Found (D2)~");
                    }
                }
                //------------------------------------------------------------------------------
                else
                {
                    dStr = $@"Error; 
Received Unknown Data (Hex) : {BitConverter.ToString(recData)}
Received Unknown Data (Text) : {System.Text.Encoding.ASCII.GetString(recData)}";
                    LogState($@"{dStr}");
                    Log.LogText(_logChannel, _transactionID, $@"COM Port: {COMPort}; {dStr}",
                            "X21", "IM30StopTransaction.ProcessResponseData", AppDecorator.Log.MessageType.Error);

                    FinalResult = new IM30StopTransResult(new Exception("-Error when Stop Card Transaction~"));
                    UpdateProcessState(StopTransState.Ending);
                    LogState("-- Unknown Error --\r\n" + "-Unknown error when reading data from card reader~");
                    Log.LogText(_logChannel, _transactionID, $@"COM Port: {COMPort}; " + "-- Unknown Error --\r\n" + "-Unknown error when reading data from card reader~",
                        "X22", "IM30StopTransaction.ProcessResponseData", AppDecorator.Log.MessageType.Error);
                }
                //--------------------------------------------------------------------------------
            }
            catch (Exception ex)
            {
                Log.LogError(_logChannel, _transactionID, new Exception($@"{ex.Message}; COM Port: {COMPort}", ex), "EX20", "IM30StopTransaction.ProcessResponseData");
            }
        }

        public void LogState(string logMsg)
        {
            if (IsDisposed)
                return;

            _logStateDEBUGDelgHandle?.Invoke(logMsg);
        }

        private object _updateProcessStateLock = new object();
        private void UpdateProcessState(StopTransState latestState, string locationTag = null)
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
                    if (IsEndingProcess == true)
                    {
                        // By Pass .. Not allowed to change;
                    }
                    else if (latestState > _processState)
                    {
                        _processState = latestState;

                        Log.LogText(_logChannel, _transactionID, $@"COM Port: {COMPort}; StopTransState: {latestState}; Loct.:{locationTag}", "A01", "IM30StopTransaction.UpdateProcessState");

                        if ((from stt in _processEndingStatesList
                             where stt == latestState
                             select stt).Count() > 0)
                        {
                            Log.LogText(_logChannel, _transactionID, $@"COM Port: {COMPort}; Ending State; Loct.:{locationTag}", "A03", "IM30StopTransaction.UpdateProcessState");
                        }
                    }
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
                            if (IsCurrentWorkingEnded == false)
                            {
                                UpdateProcessState(StopTransState.Ending, "-Shutdown Ending~");
                                Thread.Sleep(1000);
                                //_msg.ShowMessage("Request Stop Sale Transaction");
                            }
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
                //_msg.ShowMessage(ex.ToString());
            }
        }

        public static void Shutdown()
        {
            try
            {
                LastStopTransactionObj?.ShutdownX();
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
                    DateTime tOut = DateTime.Now.AddSeconds(10);

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

                _processEndingStatesList?.Clear();
                _onDataReceivedNoteDelgHandle = null;
                _logStateDEBUGDelgHandle = null;
                _updateProcessStateLock = null;
                _onTransactionFinishedHandle = null;
                FinalResult = null;
            }
        }

        public enum StopTransState
        {
            New = 0,
            Start = 1,
            OpeningPort = 2,
            SendingStopTransactionCommand = 3,
            ExpectingStopTransCommandAck = 4,

            /// <summary>
            /// On the way to end process
            /// </summary>
            Ending = 100,

            Timeout = 9000,
            Busy = 9010,
            ErrorHalt = 9999
        }
    }
}
