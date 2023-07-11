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

namespace NssIT.Kiosk.Device.PAX.IM30.OrgAPI.Trans.GetLastTransaction
{
    public class IM30GetLastTransaction : IIM30Trans, IDisposable
    {
        private const string _logChannel = "IM30_API";

        private int _noActionMaxWaitSec = 30;
        private ConcurrentQueue<bool> _dataReceivedNotes = new ConcurrentQueue<bool>();
        private ShowMessageLogDelg _logStateDEBUGDelgHandle = null;
        private GetLastTransactionState _processState = GetLastTransactionState.New;
        private IM30COMPort.OnDataReceivedNoteDelg _onDataReceivedNoteDelgHandle = null;
        private OnTransactionFinishedDelg _onTransactionFinishedHandle = null;
        private List<GetLastTransactionState> _processEndingStatesList = new List<GetLastTransactionState>(new GetLastTransactionState[]
            {GetLastTransactionState.Busy, GetLastTransactionState.Ending, GetLastTransactionState.ErrorHalt, GetLastTransactionState.Timeout });
        private bool? _isMainProcessHasAlreadyStop = null;

        public Guid WorkingId { get; } = Guid.NewGuid();
        public TransactionTypeEn TransactionType => TransactionTypeEn.System;
        public bool IsCurrentWorkingEnded { get; private set; } = false;
        public bool? IsTransStartSuccessful { get; private set; } = null;
        public IIM30TransResult FinalResult { get; private set; } = null;
        public bool IsTransEndDisposed { get { return IsDisposed; } }
        public bool IsPerfectCompleteEnd { get; private set; } = false;

        public string COMPort { get; private set; }

        private string _transactionID = Guid.NewGuid().ToString();

        ///// Static Properties xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        public static IM30GetLastTransaction LastIM30GetLastTransactionObj { get; private set; } = null;
        /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

        private DbLog _log = null;
        public DbLog Log
        {
            get
            {
                return _log ?? (_log = DbLog.GetDbLog());
            }
        }

        public IM30GetLastTransaction(string comPort, string transactionID,
            OnTransactionFinishedDelg onTransactionFinishedHandle, int noActionMaxWaitSec = 60
            , ShowMessageLogDelg debugShowMessageDelgHandler = null)
        {
            if (string.IsNullOrWhiteSpace(comPort))
                throw new Exception("Invalid COM port specification when Get Last Transaction from reader");

            COMPort = comPort;
            _onTransactionFinishedHandle = onTransactionFinishedHandle;
            _transactionID = transactionID;

            if (noActionMaxWaitSec > 10)
                _noActionMaxWaitSec = noActionMaxWaitSec;

            _onDataReceivedNoteDelgHandle = new IM30COMPort.OnDataReceivedNoteDelg(DataReceivedNoteDelgWorking);

            _logStateDEBUGDelgHandle = debugShowMessageDelgHandler;

            LastIM30GetLastTransactionObj = this;
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

            _logStateDEBUGDelgHandle?.Invoke("#####CYA-DEBUG .. IM30GetLastTransaction.DataReceivedNoteDelgWorking");
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
                        (RequestResponseIndicatorDef.RequestAndResponse, TransactionCodeDef.GetLastTransaction, ResponseCodeDef.Approved, MoreIndicatorDef.LastMessage);

            return msgData;
        }

        public IIM30TransResult NewErrFinalResult(string errorMessage)
        {
            return new IM30GetLastTransactionResult(new Exception(errorMessage), null);
        }

        public bool StartTransaction(out Exception error)
        {
            error = null;

            // Execution Validation
            if (IsDisposed)
            {
                error = new Exception("-Card reader Get Last Transaction command already disposed~");
                return false;
            }
            else if (_processState != GetLastTransactionState.New)
            {
                if (IsEndingProcess || IsCurrentWorkingEnded)
                {
                    error = new Exception($@"-Card reader Get Last Transaction error. Get Last Transaction command already finished~ Process ID : {_transactionID}");
                }
                else
                {
                    error = new Exception($@"-Card reader Get Last Transaction error. Get Last Transaction command has already started~ Process ID : {_transactionID}");
                }
                return false;
            }
            //--------------------------------------------------------------------------------------------------------------------------------

            UpdateProcessState(GetLastTransactionState.Start, "-Start (S01)~");
            Exception transError = null;
            _isMainProcessHasAlreadyStop = false;
            Thread tWorker = new Thread(new ThreadStart(new Action(() =>
            {
                string msgLog = "";
                DateTime transQuitTime = DateTime.Now.AddSeconds(_noActionMaxWaitSec);

                try
                {
                    LogState($@"--------- Start Card Reader COM Port Sequences ---------; Process Id {_transactionID}");
                    Log.LogText(_logChannel, _transactionID, $@"COM Port: {COMPort}; --------- Start Card Reader COM Port Sequences : IM30GetLastTransaction---------", "K01", "IM30GetLastTransaction.StartTransaction");

                    IM30DataModel dataModel = GetNewIM30Data();

                    LogState("-- Command --\r\n" + JsonConvert.SerializeObject(dataModel, Formatting.Indented));
                    Log.LogText(_logChannel, _transactionID, dataModel, $@"K03#COM Port: {COMPort}", "IM30GetLastTransaction.StartTransaction");

                    byte[] dataResult = IM30RequestResponseDataWorks.RenderData(dataModel);
                    msgLog = $@"{DateTime.Now:HH:mm:ss.fff_fff_1}; End Data Rendering";

                    msgLog = $@"{DateTime.Now:HH:mm:ss.fff_fff_1}; Start COM Port Transaction";
                    using (IM30COMPort im30Port = new IM30COMPort(COMPort, "-Get Last Transaction~", _onDataReceivedNoteDelgHandle, _logStateDEBUGDelgHandle))
                    {
                        msgLog = $@"{DateTime.Now:HH:mm:ss.fff_fff_1}; OpenPort - Start -- COM Port: {COMPort};{"\r\n"}";
                        UpdateProcessState(GetLastTransactionState.OpeningPort, "-Opening Port (01)~");
                        im30Port.OpenPort();
                        msgLog += $@"{DateTime.Now:HH:mm:ss.fff_fff_1}; OpenPort - End{"\r\n"}";

                        msgLog += $@"{DateTime.Now:HH:mm:ss.fff_fff_1}; Send Get Last Transaction command - Start{"\r\n"}";
                        UpdateProcessState(GetLastTransactionState.SendingGetLastTransactionCommand, "-Sending Get Reader Info command (A)~");
                        im30Port.WriteDataPort(dataResult, "-Sending Get Reader Info command (B)~");
                        msgLog += $@"{DateTime.Now:HH:mm:ss.fff_fff_1}; Send Get Last Transaction command - End{"\r\n"}";
                        UpdateProcessState(GetLastTransactionState.ExpectingGetLastTransactionCommandAck, "-Expecting Get Last Transaction Command Ack (AK01)~");

                        LogState(msgLog);
                        Log.LogText(_logChannel, _transactionID, msgLog, "K05", "IM30GetLastTransaction.StartTransaction");
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
                                UpdateProcessState(GetLastTransactionState.Timeout, "-Reach Timeout (T1)~");
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
                    UpdateProcessState(GetLastTransactionState.ErrorHalt, "-Error at Global Thread (T1)~");
                    LogState($@"'-Card reader Error; Fail Get Last Transaction~'; {ex.Message}; Last State : {_processState}; Process ID : {_transactionID}");
                    Log.LogError(_logChannel, _transactionID, new Exception($@"{ex.Message}; COM Port: {COMPort}; -Card reader Error; Fail Get Last Transaction~; Last State : {_processState}", ex), "EX10", "IM30GetLastTransaction.StartTransaction");
                }
                finally
                {
                    // ----------------------------------------------------------
                    // Manage outstanding messages
                    if (msgLog?.Length > 2)
                    {
                        LogState($@"-Get Reader Info; Suspected Outstanding Messages~{"\r\n"}{msgLog}{"\r\n"}Last State : {_processState}");
                        Log.LogText(_logChannel, _transactionID, $@"COM Port: {COMPort}; -Get Reader Info; Suspected Outstanding Messages~{"\r\n"}{msgLog}{"\r\n"}Last State : {_processState}", "G01", "IM30GetLastTransaction.StartTransaction");
                        msgLog = null;
                    }
                    // ----------------------------------------------------------
                    // Manage Unexpected FinalResult
                    if (FinalResult is null)
                    {
                        if (transError is null)
                        {
                            if (_processState == GetLastTransactionState.Busy)
                                FinalResult = new IM30GetLastTransactionResult(new Exception($@"-Fail Get Last Transaction; Reader busy~"), null);
                            else
                                FinalResult = new IM30GetLastTransactionResult(new Exception($@"-Fail Get Last Transaction; Unable to work properly; (A)~'; Last Process State : {_processState}"), null);

                        }
                        else
                            FinalResult = new IM30GetLastTransactionResult(new Exception($@"-Fail Get Last Transaction; Unable to work properly; (B)~; Last Process State : {_processState}", transError), null);
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
                        Log.LogError(_logChannel, _transactionID, new Exception($@"{ex2.Message}; COM Port: {COMPort}", ex2), "EX01", "IM30GetLastTransaction.StartTransaction");
                    }
                    // ----------------------------------------------------------
                    LogState($@"--------- End Card Reader COM Port Sequences ---------; Process ID : {_transactionID}");
                    Log.LogText(_logChannel, _transactionID, $@"COM Port: {COMPort}; --------- End Card Reader COM Port Sequences : Get Last Transaction ---------", "H01", "IM30GetLastTransaction.StartTransaction");
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
                        Log.LogError(_logChannel, _transactionID, new Exception($@"{ex.Message}; COM Port: {COMPort}; Data Txt: {hisDataStr}", ex), "EX01", "IM30GetLastTransaction.ReadCOMPortData");
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
                    Log.LogError(_logChannel, _transactionID, new Exception($@"{ex.Message}; COM Port: {COMPort}", ex), "EX01", "IM30GetLastTransaction.OnTransactionFinishedDelgWorking");
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
                Log.LogText(_logChannel, _transactionID, $@"COM Port: {COMPort}; Data Recv.Length: {recData.Length}", "A01", "IM30GetLastTransaction.ProcessResponseData");
                //-----------------------------------------------------------------------------
                // Expect ACK
                if (recData.Length == 1)
                {
                    dStr = IM30Tools.TranslateAsciiCode(recData[0]);

                    if ((int)ASCIICodeEn.ACK == (int)recData[0]) /* ACK always for -> GetLastTransactionState.ExpectingGetLastTransactionCommandAck */
                    {
                        IsTransStartSuccessful = true;
                        LogState($@"Received Char : {dStr}");
                        Log.LogText(_logChannel, _transactionID, $@"COM Port: {COMPort}; Received Char : {dStr}", "B01", "IM30GetLastTransaction.ProcessResponseData");
                        UpdateProcessState(GetLastTransactionState.ExpectingFinalGetLastTransactionResponse, "-Expecting card activities (CA1)~");
                    }
                    else if ((int)ASCIICodeEn.NAK == (int)recData[0])
                    {
                        LogState($@"-Card reader is busy~' Previous State :  {_processState}; Received Char : {dStr}");
                        Log.LogText(_logChannel, _transactionID, $@"COM Port: {COMPort}; -Card reader is busy~' Previous State: {_processState}; Received Char : {dStr}", "B05", "IM30GetLastTransaction.ProcessResponseData");
                        UpdateProcessState(GetLastTransactionState.Busy, "-Busy Reading Found (D1)~");
                    }
                    else
                    {
                        LogState($@"-Unregconized card reader reading~' Char : {dStr}");
                        Log.LogText(_logChannel, _transactionID, $@"COM Port: {COMPort}; -Unregconized card reader reading~' Char : {dStr}", "B10", "IM30GetLastTransaction.ProcessResponseData");
                    }
                }
                //------------------------------------------------------------------------------
                // Invalid Data Length
                else if (recData.Length < 5)
                {
                    dStr = $@"Received Unknown Data (Hex) : {BitConverter.ToString(recData)}{"\r\n"}";
                    dStr += $@"Received Unknown Data (Text) : {System.Text.Encoding.ASCII.GetString(recData)}";
                    LogState($@"{dStr}");
                    Log.LogError(_logChannel, _transactionID, new Exception($@"COM Port: {COMPort}; {dStr}"), "X05", "IM30GetLastTransaction.ProcessResponseData");
                }
                //------------------------------------------------------------------------------
                // Validate Serial Data
                else
                {
                    // Pack Raw Data into IM30DataModel ----------------------------------------
                    // ...
                    if (IM30RequestResponseDataWorks.ConvertToIM30DataModel(recData, out IM30DataModel im30DataMod, out Exception error2))
                    {
                        LogState($@"COM Port: {COMPort}; -- Response --{"\r\n"}" + JsonConvert.SerializeObject(im30DataMod, Formatting.Indented));

                        //..im30DataMod always for IM30GetLastTransaction.ExpectingFinalGetLastTransactionResponse
                        FinalResult = new IM30GetLastTransactionResult(im30DataMod);
                        IsPerfectCompleteEnd = true;

                        comPort.WriteDataPort(PortProtocalDef.ACKData, "ACK to Get Last Transaction Final Response");
                        LogState("Write ACK for Get Last Transaction Final Response ..");

                        UpdateProcessState(GetLastTransactionState.Ending, "-Normal Ending (N1)~");
                    }
                    // Error Found --------------------------------------------------------------
                    else if (error2 != null)
                    {
                        FinalResult = new IM30GetLastTransactionResult(error2, im30DataMod);
                        UpdateProcessState(GetLastTransactionState.ErrorHalt, "-Data reading error (ED1)~");
                        LogState("-- Error --\r\n" + JsonConvert.SerializeObject(error2, Formatting.Indented));
                        Log.LogText(_logChannel, _transactionID, error2, $@"ED1#COM Port: {COMPort}", "IM30GetLastTransaction.ProcessResponseData", MessageType.Error);
                    }
                    // Unknown Error ------------------------------------------------------------
                    else
                    {
                        dStr = $@"Error; 
Received Unknown Data (Hex) : {BitConverter.ToString(recData)}
Received Unknown Data (Text) : {System.Text.Encoding.ASCII.GetString(recData)}";
                        LogState($@"{dStr}");
                        Log.LogText(_logChannel, _transactionID, $@"COM Port: {COMPort}; {dStr}",
                            "X21", "IM30GetLastTransaction.ProcessResponseData", MessageType.Error);

                        FinalResult = new IM30GetLastTransactionResult(new Exception($@"-Unknown data reading error from card reader~ Last Process State : {_processState}"), null);
                        UpdateProcessState(GetLastTransactionState.ErrorHalt, "-Unknown data reading error (ED2)~");
                        LogState("-- Unknown Error --\r\n" + "-Unknown error when reading data from card reader~");
                        Log.LogText(_logChannel, _transactionID, $@"COM Port: {COMPort}; " + "-- Unknown Error --\r\n" + "-Unknown error when reading data from card reader~",
                            "X22", "IM30GetLastTransaction.ProcessResponseData", MessageType.Error);
                    }
                }
                //--------------------------------------------------------------------------------
            }
            catch (Exception ex)
            {
                Log.LogError(_logChannel, _transactionID, new Exception($@"{ex.Message}; COM Port: {COMPort}", ex), "EX20", "IM30GetLastTransaction.ProcessResponseData");
            }
        }

        public void LogState(string logMsg)
        {
            if (IsDisposed)
                return;

            _logStateDEBUGDelgHandle?.Invoke(logMsg);
        }

        private object _updateProcessStateLock = new object();
        private void UpdateProcessState(GetLastTransactionState latestState, string locationTag = null)
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

                        Log.LogText(_logChannel, _transactionID, $@"COM Port: {COMPort}; GetLastTransactionState: {latestState}; Loct.:{locationTag}", "A01", "IM30GetLastTransaction.UpdateProcessState");

                        if ((from stt in _processEndingStatesList
                             where stt == latestState
                             select stt).Count() > 0)
                        {
                            Log.LogText(_logChannel, _transactionID, $@"COM Port: {COMPort}; Ending State; Loct.:{locationTag}", "A03", "IM30GetLastTransaction.UpdateProcessState");
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
                            if ((IsCurrentWorkingEnded == false) && (_isMainProcessHasAlreadyStop.HasValue))
                            {
                                UpdateProcessState(GetLastTransactionState.Ending, "-Shutdown Ending~");
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
                LastIM30GetLastTransactionObj?.ShutdownX();
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
                _log = null;
            }
        }

        public enum GetLastTransactionState
        {
            New = 0,
            Start = 1,
            OpeningPort = 2,
            SendingGetLastTransactionCommand = 3,
            ExpectingGetLastTransactionCommandAck = 4,
            ExpectingFinalGetLastTransactionResponse = 6,

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
