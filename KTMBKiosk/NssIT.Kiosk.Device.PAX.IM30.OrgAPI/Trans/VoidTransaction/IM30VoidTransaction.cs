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
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Device.PAX.IM30.OrgAPI.Trans.VoidTransaction
{
    public class IM30VoidTransaction : IIM30Trans, IDisposable
    {
        private const string _logChannel = "IM30_API";

        private int _noActionMaxWaitSec = 60 * 60 * 1; /*Default to one hours*/
        private ConcurrentQueue<bool> _dataReceivedNotes = new ConcurrentQueue<bool>();
        private ShowMessageLogDelg _logStateDEBUGDelgHandle = null;
        private VoidTransactionState _processState = VoidTransactionState.New;
        private IM30COMPort.OnDataReceivedNoteDelg _onDataReceivedNoteDelgHandle = null;
        private OnTransactionFinishedDelg _onTransactionFinishedHandle = null;
        private List<VoidTransactionState> _processEndingStatesList = new List<VoidTransactionState>(new VoidTransactionState[]
            {VoidTransactionState.Busy, VoidTransactionState.Ending, VoidTransactionState.ErrorHalt, VoidTransactionState.Timeout });
        private bool? _isMainProcessHasAlreadyStop = null;
        private string _transactionID = Guid.NewGuid().ToString();

        public Guid WorkingId { get; } = Guid.NewGuid();
        public TransactionTypeEn TransactionType => TransactionTypeEn.System;
        public bool IsCurrentWorkingEnded { get; private set; } = false;
        public bool? IsTransStartSuccessful { get; private set; } = null;
        public IIM30TransResult FinalResult { get; private set; } = null;
        public bool IsTransEndDisposed { get { return IsDisposed; } }
        public string InvoiceNo { get; private set; }
        public string CardToken { get; private set; }
        public decimal TransAmount { get; private set; }

        public string COMPort { get; private set; }
        public bool IsPerfectCompleteEnd { get; private set; } = false;

        ///// Static Properties xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        public static IM30VoidTransaction LastIM30VoidTransactionObj { get; private set; } = null;
        /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

        private DbLog _log = null;
        public DbLog Log
        {
            get
            {
                return _log ?? (_log = DbLog.GetDbLog());
            }
        }

        public IM30VoidTransaction(string comPort, string invoiceNo, string cardToken, decimal transAmount,
            OnTransactionFinishedDelg onTransactionFinishedHandle, int noActionMaxWaitSec = 60,
            ShowMessageLogDelg debugShowMessageDelgHandler = null)
        {
            if (string.IsNullOrWhiteSpace(comPort))
                throw new Exception("Invalid COM port specification when doing Void Transaction in card reader");

            if (string.IsNullOrWhiteSpace(invoiceNo))
                throw new Exception("Invalid Invoice No. specification when doing Void Transaction in card reader");

            if (string.IsNullOrWhiteSpace(cardToken))
                throw new Exception("Invalid Credit/Debit Card Token specification when doing Void Transaction in card reader");

            if (transAmount <= 0)
                throw new Exception("Invalid Transaction Amount specification when doing Void Transaction in card reader");

            COMPort = comPort;
            _onTransactionFinishedHandle = onTransactionFinishedHandle;
            InvoiceNo = invoiceNo.Trim();
            CardToken = cardToken.Trim();
            TransAmount = transAmount;

            if (noActionMaxWaitSec > 10)
                _noActionMaxWaitSec = noActionMaxWaitSec;

            _onDataReceivedNoteDelgHandle = new IM30COMPort.OnDataReceivedNoteDelg(DataReceivedNoteDelgWorking);

            _logStateDEBUGDelgHandle = debugShowMessageDelgHandler;

            LastIM30VoidTransactionObj = this;
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
                        (RequestResponseIndicatorDef.RequestAndResponse, TransactionCodeDef.Void, ResponseCodeDef.Approved, MoreIndicatorDef.LastMessage);

            msgData.AddFieldElement(1, FieldTypeDef.InvoiceNo, InvoiceNo);
            msgData.AddFieldElement(2, FieldTypeDef.TransAmount, TransAmount);
            msgData.AddFieldElement(3, FieldTypeDef.CardToken, CardToken);

            return msgData;
        }

        public IIM30TransResult NewErrFinalResult(string errorMessage)
        {
            return new IM30VoidTransactionResult(new Exception(errorMessage), null);
        }

        public bool StartTransaction(out Exception error)
        {
            error = null;

            // Execution Validation
            if (IsDisposed)
            {
                error = new Exception("-Card reader for Void Transaction command already disposed~");
                return false;
            }
            else if (_processState != VoidTransactionState.New)
            {
                if (IsEndingProcess || IsCurrentWorkingEnded)
                {
                    error = new Exception($@"-Card reader for Void Transaction error. Void Transaction command already finished~ Process ID : {_transactionID}; InvoiceNo: {InvoiceNo}; CardToken: {CardToken}");
                }
                else
                {
                    error = new Exception($@"-Card reader for Void Transaction error. Void Transaction command has already started~ Process ID : {_transactionID}; InvoiceNo: {InvoiceNo}; CardToken: {CardToken}");
                }
                return false;
            }
            //--------------------------------------------------------------------------------------------------------------------------------

            UpdateProcessState(VoidTransactionState.Start, "-Start (S01)~");
            Exception transError = null;
            _isMainProcessHasAlreadyStop = false;
            Thread tWorker = new Thread(new ThreadStart(new Action(() =>
            {
                string msgLog = "";
                DateTime transQuitTime = DateTime.Now.AddSeconds(_noActionMaxWaitSec);

                try
                {
                    LogState($@"--------- Start Card Reader COM Port Sequences ---------; Process Id {_transactionID}; InvoiceNo: {InvoiceNo}; CardToken: {CardToken}");
                    Log.LogText(_logChannel, _transactionID, $@"COM Port: {COMPort}; --------- Start Card Reader COM Port Sequences : IM30VoidTransaction ---------- Process Id {_transactionID}; InvoiceNo: {InvoiceNo}; CardToken: {CardToken} ---------",
                        "K01", "IM30VoidTransaction.StartTransaction");

                    IM30DataModel dataModel = GetNewIM30Data();

                    LogState("-- Command --\r\n" + JsonConvert.SerializeObject(dataModel, Formatting.Indented));
                    Log.LogText(_logChannel, _transactionID, dataModel, $@"K03#COM Port: {COMPort}", "IM30VoidTransaction.StartTransaction");

                    byte[] dataResult = IM30RequestResponseDataWorks.RenderData(dataModel);
                    msgLog = $@"{DateTime.Now:HH:mm:ss.fff_fff_1}; End Data Rendering";

                    msgLog = $@"{DateTime.Now:HH:mm:ss.fff_fff_1}; Start COM Port Transaction";
                    using (IM30COMPort im30Port = new IM30COMPort(COMPort, "-Void Transaction~", _onDataReceivedNoteDelgHandle, _logStateDEBUGDelgHandle))
                    {
                        msgLog = $@"{DateTime.Now:HH:mm:ss.fff_fff_1}; OpenPort - Start; COM Port: {COMPort}; {"\r\n"}";
                        UpdateProcessState(VoidTransactionState.OpeningPort, "-Opening Port (01)~");
                        im30Port.OpenPort();
                        msgLog += $@"{DateTime.Now:HH:mm:ss.fff_fff_1}; OpenPort - End{"\r\n"}";

                        msgLog += $@"{DateTime.Now:HH:mm:ss.fff_fff_1}; Send Void Transaction command - Start{"\r\n"}";
                        UpdateProcessState(VoidTransactionState.SendingVoidTransCommand, "-Sending Void Transaction command (A)~");
                        im30Port.WriteDataPort(dataResult, "-Sending Void Transaction command (B)~");
                        msgLog += $@"{DateTime.Now:HH:mm:ss.fff_fff_1}; Send Void Transaction command - End{"\r\n"}";
                        UpdateProcessState(VoidTransactionState.ExpectingVoidTransCommandAck, "-Expecting Void Transaction Command Ack (AK01)~");

                        LogState(msgLog);
                        Log.LogText(_logChannel, _transactionID, msgLog, "K05", "IM30VoidTransaction.StartTransaction");
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
                                UpdateProcessState(VoidTransactionState.Timeout, "-Reach Timeout (T1)~");
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
                    UpdateProcessState(VoidTransactionState.ErrorHalt, "-Error at Global Thread (T1)~");
                    LogState($@"'-Card reader Error; Fail Void Transaction~'; {ex.Message}; Last State : {_processState}; Process ID : {_transactionID}; InvoiceNo: {InvoiceNo}");
                    Log.LogError(_logChannel, _transactionID, 
                        new Exception($@"{ex.Message}; COM Port: {COMPort}; -Card reader Error; Fail Void Transaction~; Last State : {_processState}; InvoiceNo: {InvoiceNo}", ex), 
                        "EX10", "IM30VoidTransaction.StartTransaction");
                }
                finally
                {
                    // ----------------------------------------------------------
                    // Manage outstanding messages
                    if (msgLog?.Length > 2)
                    {
                        LogState($@"-Void Transaction; Suspected Outstanding Messages~{"\r\n"}{msgLog}{"\r\n"}Last State : {_processState}");
                        Log.LogText(_logChannel, _transactionID, $@"COM Port: {COMPort}; -Void Transaction; Suspected Outstanding Messages~{"\r\n"}{msgLog}{"\r\n"}Last State : {_processState}",
                            "G01", "IM30VoidTransaction.StartTransaction", AppDecorator.Log.MessageType.Error);
                        msgLog = null;
                    }
                    // ----------------------------------------------------------
                    // Manage Unexpected Result
                    if (FinalResult is null)
                    {
                        if (transError is null)
                        {
                            if (_processState == VoidTransactionState.Busy)
                                FinalResult = new IM30VoidTransactionResult(new Exception($@"-Fail Void Transaction; Reader busy~"), null);
                            else
                                FinalResult = new IM30VoidTransactionResult(new Exception($@"-Fail Void Transaction; Unable to work properly; (A)~'; Last Process State : {_processState}"), null);

                        }
                        else
                            FinalResult = new IM30VoidTransactionResult(new Exception($@"-Fail Void Transaction; Unable to work properly; (B)~; Last Process State : {_processState}", transError), null);
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
                        Log.LogError(_logChannel, _transactionID, new Exception($@"{ex2.Message}; COM Port: {COMPort}", ex2), "EX01", "IM30VoidTransaction.StartTransaction");
                    }
                    // ----------------------------------------------------------
                    LogState($@"--------- End Card Reader COM Port Sequences ---------; Process ID : {_transactionID}; InvoiceNo: {InvoiceNo}");
                    Log.LogText(_logChannel, _transactionID, 
                        $@"COM Port: {COMPort}; --------- End Card Reader COM Port Sequences : IM30VoidTransaction ---------; Process ID : {_transactionID}; InvoiceNo: {InvoiceNo}", 
                        "H01", "IM30VoidTransaction.StartTransaction");
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
                        Log.LogError(_logChannel, _transactionID, new Exception($@"{ex.Message}; COM Port: {COMPort}; Data Txt: {hisDataStr}", ex), "EX01", "IM30VoidTransaction.ReadCOMPortData");
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
                    Log.LogError(_logChannel, _transactionID, new Exception($@"{ex.Message}; COM Port: {COMPort}", ex), "EX01", "IM30VoidTransaction.OnTransactionFinishedDelgWorking");
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
                Log.LogText(_logChannel, _transactionID, $@"COM Port: {COMPort}; Data Recv.Length: {recData.Length}", "A01", "IM30VoidTransaction.ProcessResponseData");
                //-----------------------------------------------------------------------------
                // Expect ACK
                if (recData.Length == 1)
                {
                    dStr = IM30Tools.TranslateAsciiCode(recData[0]);

                    if ((int)ASCIICodeEn.ACK == (int)recData[0])
                    {
                        IsTransStartSuccessful = true;
                        LogState($@"Received Char : {dStr}");
                        Log.LogText(_logChannel, _transactionID, $@"COM Port: {COMPort}; Received Char : {dStr}", "B01", "IM30VoidTransaction.ProcessResponseData");
                        UpdateProcessState(VoidTransactionState.ExpectingFinalVoidTransResponse, "-Expecting card activities (CA1)~");
                    }
                    else if ((int)ASCIICodeEn.NAK == (int)recData[0])
                    {
                        LogState($@"-Card reader is busy~' Previous State :  {_processState}; Received Char : {dStr}");
                        Log.LogText(_logChannel, _transactionID, $@"COM Port: {COMPort}; -Card reader is busy~' Previous State :  {_processState}; Received Char : {dStr}", "B05", "IM30VoidTransaction.ProcessResponseData");
                        UpdateProcessState(VoidTransactionState.Busy, "-Busy Reading Found (D1)~");
                    }
                    else
                    {
                        LogState($@"-Unregconized card reader reading~' Char : {dStr}");
                        Log.LogText(_logChannel, _transactionID, $@"COM Port: {COMPort}; -Unrecognized card reader reading~' Char : {dStr}",
                            "X10", "IM30VoidTransaction.ProcessResponseData", AppDecorator.Log.MessageType.Error);
                    }
                }
                //------------------------------------------------------------------------------
                // Invalid Data Length
                else if (recData.Length < 5)
                {
                    dStr = $@"Received Unknown Data (Hex) : {BitConverter.ToString(recData)}{"\r\n"}";
                    dStr += $@"Received Unknown Data (Text) : {System.Text.Encoding.ASCII.GetString(recData)}";
                    LogState($@"{dStr}");
                    Log.LogError(_logChannel, _transactionID, new Exception($@"COM Port: {COMPort}; {dStr}"), "X05", "IM30VoidTransaction.ProcessResponseData");
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

                        //..im30DataMod always for VoidTransactionState.ExpectingFinalVoidTransResponse
                        FinalResult = new IM30VoidTransactionResult(im30DataMod);
                        IsPerfectCompleteEnd = true;

                        comPort.WriteDataPort(PortProtocalDef.ACKData, "ACK for Void Transaction Final Response");
                        LogState("Write ACK for Void Transaction Final Response ..");

                        UpdateProcessState(VoidTransactionState.Ending, "-Normal Ending (N1)~");
                    }
                    // Error Found --------------------------------------------------------------
                    else if (error2 != null)
                    {
                        FinalResult = new IM30VoidTransactionResult(error2, im30DataMod);
                        UpdateProcessState(VoidTransactionState.ErrorHalt, "-Data reading error (ED1)~");
                        LogState("-- Error --\r\n" + JsonConvert.SerializeObject(error2, Formatting.Indented));
                        Log.LogText(_logChannel, _transactionID, error2, $@"ED1#COM Port: {COMPort}", "IM30VoidTransaction.ProcessResponseData", AppDecorator.Log.MessageType.Error);
                    }
                    // Unknown Error ------------------------------------------------------------
                    else
                    {
                        dStr = $@"Error; 
Received Unknown Data (Hex) : {BitConverter.ToString(recData)}
Received Unknown Data (Text) : {System.Text.Encoding.ASCII.GetString(recData)}";
                        LogState($@"{dStr}");
                        Log.LogText(_logChannel, _transactionID, $@"COM Port: {COMPort}; {dStr}",
                            "X21", "IM30VoidTransaction.ProcessResponseData", AppDecorator.Log.MessageType.Error);

                        FinalResult = new IM30VoidTransactionResult(new Exception($@"-Unknown data reading error from card reader~ Last Process State : {_processState}"), null);
                        UpdateProcessState(VoidTransactionState.ErrorHalt, "-Unknown data reading error (ED2)~");
                        LogState("-- Unknown Error --\r\n" + "-Unknown error when reading data from card reader~");
                        Log.LogText(_logChannel, _transactionID, $@"COM Port: {COMPort}; " + "-- Unknown Error --\r\n" + "-Unknown error when reading data from card reader~",
                            "X22", "IM30VoidTransaction.ProcessResponseData", AppDecorator.Log.MessageType.Error);
                    }
                }
                //--------------------------------------------------------------------------------
            }
            catch (Exception ex)
            {
                Log.LogError(_logChannel, _transactionID, new Exception($@"{ex.Message}; COM Port: {COMPort}", ex), "EX20", "IM30VoidTransaction.ProcessResponseData");
            }
        }

        public void LogState(string logMsg)
        {
            if (IsDisposed)
                return;

            _logStateDEBUGDelgHandle?.Invoke(logMsg);
        }

        private object _updateProcessStateLock = new object();
        private void UpdateProcessState(VoidTransactionState latestState, string locationTag = null)
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

                        Log.LogText(_logChannel, _transactionID, $@"COM Port: {COMPort}; VoidTransactionState: {latestState}; Loct.:{locationTag}", "A01", "IM30VoidTransaction.UpdateProcessState");

                        if ((from stt in _processEndingStatesList
                             where stt == latestState
                             select stt).Count() > 0)
                        {
                            Log.LogText(_logChannel, _transactionID, $@"COM Port: {COMPort}; Ending State; Loct.:{locationTag}", "A03", "IM30VoidTransaction.UpdateProcessState");
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
                                UpdateProcessState(VoidTransactionState.Ending, "-Shutdown Ending~");
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
                Dispose();
                //_msg.ShowMessage(ex.ToString());
            }
        }

        public static void Shutdown()
        {
            try
            {
                LastIM30VoidTransactionObj?.ShutdownX();
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

        public enum VoidTransactionState
        {
            New = 0,
            Start = 1,
            OpeningPort = 2,
            SendingVoidTransCommand = 3,
            ExpectingVoidTransCommandAck = 4,
            ExpectingFinalVoidTransResponse = 6,

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
