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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Device.PAX.IM30.OrgAPI.Trans.Ping
{
    
    public class IM30Ping :  IIM30Trans, IDisposable
    {
        private const string _logChannel = "IM30_API";

        private int _noActionMaxWaitSec = 60;
        private string _pingSuccessMsg = "OK";
        private PingProcessState _processState = PingProcessState.New;
        private IM30COMPort.OnDataReceivedNoteDelg _onDataReceivedNoteDelgHandle = null;
        private ConcurrentQueue<bool> _dataReceivedNotes = new ConcurrentQueue<bool>();
        private OnTransactionFinishedDelg _onTransactionFinishedHandle = null;
        private List<PingProcessState> _processEndingStatesList = new List<PingProcessState>(new PingProcessState[] 
            {PingProcessState.Busy, PingProcessState.Ending, PingProcessState.ErrorHalt, PingProcessState.Timeout });

        public Guid WorkingId { get; } = Guid.NewGuid();
        public TransactionTypeEn TransactionType => TransactionTypeEn.System;
        public string ProcessID { get; private set; } = Guid.NewGuid().ToString();
        public bool IsCurrentWorkingEnded { get; private set; } = false;
        public IIM30TransResult FinalResult { get; private set;}
        public bool IsTransEndDisposed { get { return IsDisposed; } }
        public bool? IsTransStartSuccessful { get; private set; } = null;
        public bool IsPerfectCompleteEnd { get; private set; } = false;

        public bool IsShutdown => IsDisposed;

        private ShowMessageLogDelg _logStateDEBUGDelgHandle = null;
        public string COMPort { get; private set; }

        ///// Static Properties xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        public static IM30Ping LastIM30Ping { get; private set; } = null;
        /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

        private DbLog _log = null;
        public DbLog Log
        {
            get
            {
                return _log ?? (_log = DbLog.GetDbLog());
            }
        }

        public IM30Ping(string comPort, OnTransactionFinishedDelg onTransactionFinishedHandle, string processID, int noActionMaxWaitSec = 60
            , ShowMessageLogDelg debugShowMessageDelgHandler = null)
        {
            if (string.IsNullOrWhiteSpace(comPort))
                throw new Exception("Invalid COM port specification when Ping to reader");

            COMPort = comPort;
            _onTransactionFinishedHandle = onTransactionFinishedHandle;
            ProcessID = processID;
            if (noActionMaxWaitSec > 10)
                _noActionMaxWaitSec = noActionMaxWaitSec;

            _onDataReceivedNoteDelgHandle = new IM30COMPort.OnDataReceivedNoteDelg(DataReceivedNoteDelgWorking);

            _logStateDEBUGDelgHandle = debugShowMessageDelgHandler;
            LastIM30Ping = this;
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
                        (RequestResponseIndicatorDef.RequestAndResponse, TransactionCodeDef.Ping, ResponseCodeDef.Approved, MoreIndicatorDef.LastMessage);

            return msgData;
        }

        public IIM30TransResult NewErrFinalResult(string errorMessage)
        {
            return new IM30PingResult(new Exception(errorMessage));
        }

        public bool StartTransaction(out Exception error)
        {
            error = null;

            // Execution Validation
            if (IsDisposed)
            {
                error = new Exception("-Card reader Ping Command already disposed~");
                return false;
            }
            else if (_processState != PingProcessState.New)
            {
                if (IsEndingProcess || IsCurrentWorkingEnded)
                {
                    error = new Exception($@"-Card reader ping error. Ping command already finished~ Process ID : {ProcessID}");
                }
                else
                {
                    error = new Exception($@"-Card reader ping error. Ping command has already started~ Process ID : {ProcessID}");
                }
                return false;
            }
            //--------------------------------------------------------------------------------------------------------------------------------

            UpdateProcessState(PingProcessState.Start);
            Exception transError = null;

            Thread tWorker = new Thread(new ThreadStart(new Action(() =>
            {
                string msgLog = "";
                DateTime transQuitTime = DateTime.Now.AddSeconds(_noActionMaxWaitSec);

                try
                {
                    LogState($@"--------- Start Card Reader COM Port Sequences ---------; Process Id {ProcessID}");
                    Log.LogText(_logChannel, ProcessID, $@"COM Port: {COMPort}; --------- Start Card Reader COM Port Sequences : IM30Ping ---------", "K01", "IM30Ping.StartTransaction");

                    IM30DataModel dataModel = GetNewIM30Data();

                    LogState("-- Command --\r\n" + JsonConvert.SerializeObject(dataModel, Formatting.Indented));
                    Log.LogText(_logChannel, ProcessID, dataModel, $@"K03#COM Port: {COMPort}", "IM30Ping.StartTransaction");

                    byte[] dataResult = IM30RequestResponseDataWorks.RenderData(dataModel);
                    msgLog = $@"{DateTime.Now:HH:mm:ss.fff_fff_1}; End Data Rendering";

                    msgLog = $@"{DateTime.Now:HH:mm:ss.fff_fff_1}; Start COM Port Transaction";
                    using (IM30COMPort im30Port = new IM30COMPort(COMPort, "*PING*", _onDataReceivedNoteDelgHandle, _logStateDEBUGDelgHandle))
                    {
                        msgLog = $@"{DateTime.Now:HH:mm:ss.fff_fff_1}; OpenPort - Start; COM Port: {COMPort}{"\r\n"}";
                        UpdateProcessState(PingProcessState.OpeningPort);
                        im30Port.OpenPort();
                        msgLog += $@"{DateTime.Now:HH:mm:ss.fff_fff_1}; OpenPort - End{"\r\n"}";

                        msgLog += $@"{DateTime.Now:HH:mm:ss.fff_fff_1}; Send Ping - Start{"\r\n"}";
                        UpdateProcessState(PingProcessState.SendingPingCommand);
                        im30Port.WriteDataPort(dataResult, "IM30Ping.StartTransaction(--Send Ping Command--)");
                        msgLog += $@"{DateTime.Now:HH:mm:ss.fff_fff_1}; Send Ping - End{"\r\n"}";
                        UpdateProcessState(PingProcessState.ExpectingPingCommandAck);

                        LogState(msgLog);
                        Log.LogText(_logChannel, ProcessID, msgLog, "K05", "IM30Ping.StartTransaction");
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
                                UpdateProcessState(PingProcessState.Timeout, "-Reach Timeout (T1)-");
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
                    UpdateProcessState(PingProcessState.ErrorHalt, "-Error at Global Thread (T1)-");
                    LogState($@"'-Card reader Error; Fail to ping card reader properly~'; {ex.Message}; Last State : {_processState}; Process ID : {ProcessID}");
                    Log.LogError(_logChannel, ProcessID, new Exception($@"{ex.Message}; COM Port: {COMPort}; -Card reader Error; Fail to Ping Card Reader~; Last State : {_processState}", ex), 
                        "EX10", "IM30Ping.StartTransaction");
                }
                finally
                {
                    // ----------------------------------------------------------
                    // Manage outstanding messages
                    if (msgLog?.Length > 2)
                    {
                        LogState($@"-Ping card reader; Suspected Outstanding Messages~{"\r\n"}{msgLog}{"\r\n"}Last State : {_processState}");
                        Log.LogText(_logChannel, ProcessID, $@"COM Port: {COMPort}; -Ping Card Reader; Suspected Outstanding Messages~{"\r\n"}{msgLog}{"\r\n"}Last State : {_processState}", 
                            "G01", "IM30Ping.StartTransaction");
                        msgLog = null;
                    }
                    // ----------------------------------------------------------
                    // Manage Unexpected Result
                    if (FinalResult is null)
                    {
                        if (transError is null)
                        {
                            if (_processState == PingProcessState.Busy)
                                FinalResult = new IM30PingResult(new Exception($@"-Fail to ping card reader; Reader busy~"));
                            else
                                FinalResult = new IM30PingResult(new Exception($@"-Fail to ping card reader; Unable to work properly; (A)~'; Last Process State : {_processState}"));

                        }
                        else
                            FinalResult = new IM30PingResult(new Exception($@"-Fail to ping card reader; Unable to work properly; (B)~; Last Process State : {_processState}", transError));
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
                        Log.LogError(_logChannel, ProcessID, new Exception($@"{ex2.Message}; COM Port: {COMPort}", ex2), "EX01", "IM30Ping.StartTransaction");
                    }
                    // ----------------------------------------------------------
                    LogState($@"--------- End Card Reader COM Port Sequences ---------; Process ID : {ProcessID}");
                    Log.LogText(_logChannel, ProcessID, $@"COM Port: {COMPort}; --------- End Card Reader COM Port Sequences : IM30Ping ---------", "H01", "IM30Ping.StartTransaction");
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
                        Log.LogError(_logChannel, ProcessID, new Exception($@"{ex.Message}; COM Port: {COMPort}; Data Txt: {hisDataStr}", ex), "EX01", "IM30Ping.ReadCOMPortData");
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
                    Log.LogError(_logChannel, ProcessID, new Exception($@"{ex.Message}; COM Port: {COMPort}", ex), "EX01", "IM30Ping.OnTransactionFinishedDelgWorking");
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
                Log.LogText(_logChannel, ProcessID, $@"COM Port: {COMPort}; Data Recv.Length: {recData.Length}", "A01", "IM30Ping.ProcessResponseData");
                //-----------------------------------------------------------------------------
                // Expect ACK
                if (recData.Length == 1)
                {
                    dStr = IM30Tools.TranslateAsciiCode(recData[0]);
                    
                    if ((int)ASCIICodeEn.ACK == (int)recData[0])
                    {
                        IsTransStartSuccessful = true;
                        IsPerfectCompleteEnd = true;
                        LogState($@"Ping Success; Received Char : {dStr}");
                        Log.LogText(_logChannel, ProcessID, $@"COM Port: {COMPort}; Ping Success; Received Char : {dStr}", "B01", "IM30Ping.ProcessResponseData");
                        FinalResult = new IM30PingResult();
                    }
                    else if ((int)ASCIICodeEn.NAK == (int)recData[0])
                    {
                        LogState($@"-Unable to ping reader; Ping Denied-' Previous State :  {_processState}");
                        Log.LogText(_logChannel, ProcessID, $@"COM Port: {COMPort}; -Unable to ping reader; Ping Denied-' Previous State: {_processState}; Received Char : {dStr}", "B05", "IM30Ping.ProcessResponseData");
                        FinalResult = new IM30PingResult(new Exception($@"-:Unable to ping reader; Ping Denied:-' Previous State :  {_processState}"));
                    }
                    else
                    {
                        LogState($@"-Unrecognized card reader reading~' Char : {dStr}");
                        Log.LogText(_logChannel, ProcessID, $@"COM Port: {COMPort}; -Unrecognized card reader reading~' Char : {dStr}", "B10", "IM30Ping.ProcessResponseData");
                        FinalResult = new IM30PingResult(new Exception($@"-Unregconized card reader reading~' Char : {dStr}"));
                    }

                    UpdateProcessState(PingProcessState.Ending);
                }
                //------------------------------------------------------------------------------
                else
                {
                    dStr = $@"Error; 
Received Unknown Data (Hex) : {BitConverter.ToString(recData)}
Received Unknown Data (Text) : {System.Text.Encoding.ASCII.GetString(recData)}";
                    LogState($@"{dStr}");
                    Log.LogError(_logChannel, ProcessID, new Exception($@"COM Port: {COMPort}; {dStr}"), "X05", "IM30Ping.ProcessResponseData");

                    FinalResult = new IM30PingResult(new Exception("-:Error when Ping Reader:-"));
                    UpdateProcessState(PingProcessState.Ending);
                }
                //--------------------------------------------------------------------------------
            }
            catch (Exception ex)
            {
                Log.LogError(_logChannel, ProcessID, new Exception($@"{ex.Message}; COM Port: {COMPort}", ex), "EX20", "IM30Ping.ProcessResponseData");
            }
        }

        public void LogState(string logMsg)
        {
            if (IsDisposed)
                return;

            _logStateDEBUGDelgHandle?.Invoke(logMsg);
        }

        private object _updateProcessStateLock = new object();
        private void UpdateProcessState(PingProcessState latestState, string locationTag = null)
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
                lock(_updateProcessStateLock)
                {
                    if (IsEndingProcess == true)
                    {
                        // By Pass .. Not allowed to change;
                    }
                    else if (latestState > _processState)
                    {
                        _processState = latestState;

                        Log.LogText(_logChannel, ProcessID, $@"COM Port: {COMPort}; PingProcessState: {latestState}; Loct.:{locationTag}", "A01", "IM30Ping.UpdateProcessState");

                        if ((from stt in _processEndingStatesList
                             where stt == latestState
                             select stt).Count() > 0)
                        {
                            Log.LogText(_logChannel, ProcessID, $@"COM Port: {COMPort}; Ending State; Loct.:{locationTag}", "A03", "IM30Ping.UpdateProcessState");
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
                                UpdateProcessState(PingProcessState.Ending, "-Shutdown Ending~");
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
                LastIM30Ping?.ShutdownX();
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
                FinalResult = null;
                if (_dataReceivedNotes != null)
                {
                    while (_dataReceivedNotes.TryDequeue(out _))
                    { }
                    _dataReceivedNotes = null;
                }

                _onDataReceivedNoteDelgHandle = null;
                _log = null;
            }
        }

        enum PingProcessState
        {
            New = 0,
            Start = 1,
            OpeningPort = 2,
            SendingPingCommand = 3,
            ExpectingPingCommandAck = 4,

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
