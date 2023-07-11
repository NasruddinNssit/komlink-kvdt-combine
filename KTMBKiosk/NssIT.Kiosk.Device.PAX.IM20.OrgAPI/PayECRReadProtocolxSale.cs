using NssIT.Kiosk.AppDecorator.DomainLibs.Common.CreditDebitCharge;
using NssIT.Kiosk.Device.PAX.IM20.OrgAPI.Base;
using NssIT.Kiosk.Device.PAX.IM20.OrgAPI.Base.ExpectReading;
using NssIT.Kiosk.Log.DB;
using NssIT.Kiosk.Tools.CountDown;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static NssIT.Kiosk.Device.PAX.IM20.OrgAPI.PayECR;

namespace NssIT.Kiosk.Device.PAX.IM20.OrgAPI
{
    public class PayECRReadProtocolxSale : ITransProtocol, IDisposable
    {
        private const string LogChannel = "PAX_IM20_API";
        private const string _hxEOT = "\x04";
        private const string _hxENQ = "\x05";
        private const string _hxACK = "\x06";
        private const string _hxNAK = "\x15";
        //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        public int ResultStatusCode { get; private set; }
        public string ResultstatusRemark { get; private set; }
        public string ResultReadData { get; private set; }
        //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        private string _processId = "*";
        private bool _localTimeout = false;
        private bool _abortFlag = false;
        //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

        private StrongCountDownTimer _countDown = new StrongCountDownTimer("PayECRReadProtocolxSale");
        private PayECRReadProtocolxSale_DataChecking _dataChecking = null;
        private PayECRComPort _go_SerialPort = null;
        private DbLog _log = null;

        private ProcessHasDisposedDelg _processHasDisposedDelgHandle = null;
        private ShowInProgressDelg _showInProgressDelgHandle = null;
        private DisallowStopDelg _disallowStopDelgHandle = null;
        private ComputeLRCforByteDelg _computeLRCforByteDelgHandle = null;
        private CheckProcessHasStopDelg _checkProcessHasStopDelgHandle = null;

        public PayECRReadProtocolxSale(PayECRComPort goSerialPort, string processId,
            ProcessHasDisposedDelg processHasDisposedDelgHandle, DisallowStopDelg disallowStopDelgHandle, ShowInProgressDelg showInProgressDelgHandle,
            ComputeLRCforByteDelg computeLRCforByteDelg, CheckProcessHasStopDelg checkProcessHasStopDelgHandle)
        {
            _log = DbLog.GetDbLog();

            _processId = processId;
            _go_SerialPort = goSerialPort;
            _showInProgressDelgHandle = showInProgressDelgHandle;
            _processHasDisposedDelgHandle = processHasDisposedDelgHandle;
            _disallowStopDelgHandle = disallowStopDelgHandle;
            _computeLRCforByteDelgHandle = computeLRCforByteDelg;
            _checkProcessHasStopDelgHandle = checkProcessHasStopDelgHandle;

            _countDown.OnCountDown += _countDown_OnCountDown;
            _countDown.OnExpired += _countDown_OnExpired;

            _dataChecking = new PayECRReadProtocolxSale_DataChecking(_showInProgressDelgHandle);

            _dataChecking.InitReader(goSerialPort, processId);
        }

        public void EndDispose()
        {
            Dispose();
        }

        public void Dispose()
        {
            _go_SerialPort = null;
            _showInProgressDelgHandle = null;
            _processHasDisposedDelgHandle = null;
            _disallowStopDelgHandle = null;
            _computeLRCforByteDelgHandle = null;
            _checkProcessHasStopDelgHandle = null;

            _countDown.OnCountDown -= _countDown_OnCountDown;
            _countDown.OnExpired -= _countDown_OnExpired;

            try
            {
                _dataChecking.Dispose();
            }
            catch { }

            try
            {
                _countDown.Dispose();
            }
            catch { }

            _dataChecking = null;
            _countDown = null;
            _log = null;
        }

        public bool Run(int defaultTransactionWaitingTimeSec = 300, int defaultFirstWaitingTimeSec = 60)
        {
            //_go_SerialPort.WritePortTimeOut = _gi_timeout;
            //_go_SerialPort.ReadPortTimeOut = _gi_timeout;
            ResultStatusCode = 99;
            ResultstatusRemark = null;
            ResultReadData = null;

            _localTimeout = false;
            _abortFlag = false;

            //--------------------------------------------------------------------------------------------------------------
            //CYA-TEST .. defaultFirstWaitingTimeSec = 30;
            //--------------------------------------------------------------------------------------------------------------
            // ---------- TIME SETTING ----------
            //
            // defaultSuspendEndWaitingTimeSec is a period for Total Canceling & Ending Time.
            int defaultSuspendEndWaitingTimeSec = 20;
            // minWaitingTimeSec is a minimum period used to wait for response data after tap card.
            int minWaitingTimeSec = 60 + defaultSuspendEndWaitingTimeSec;

            if (defaultTransactionWaitingTimeSec < minWaitingTimeSec)
                defaultTransactionWaitingTimeSec = minWaitingTimeSec;

            // transactionWaitingTime is a maximum period used to wait for response data after tap card.
            int transactionWaitingTimeSec = (defaultTransactionWaitingTimeSec - defaultSuspendEndWaitingTimeSec);
            int finishingWaitingTimeSec = 10;

            //int maxStopDelayTimeSec = 9;
            int maxCancelDelayTimeSec = 10;

            DateTime nextLogAbnormalLoopTime = DateTime.Now;
            DateTime nextLogTimeoutWaitingTime = DateTime.Now;
            //--------------------------------------------------------------------------------------------------------------

            bool lb_success = false;
            string cardDetectedMsg = "**";
            bool responseFound = false;
            int lootCount = 0;
            byte[] recebuff;
            bool continueLoop = true;// to start the loop//  added to test kiosk purpose
            int defaultReadPortTimeoutSec = 10;
            int readPortTimeoutSec = defaultReadPortTimeoutSec;
            int dataReadRetryCount = 0;
            int actualDataLen = 0;

            DateTime timeOut = DateTime.Now;
            bool isExpectSTAData = false;
            bool isFinishing = false;

            //-----------------------------------------------------
            //CountDown Code
            string cDwnCode_StartDetectCard = "StartDetectCard";
            string cDwnCode_WaitCompleteCardResponse = "WaitCompleteCardResponse";
            string cDwnCode_WaitEOT = "WaitEOT";
            string cDwnCode_CancelInProgress = "CancelInProgress";
            /////string cDwnCode_StopInProgress = "StopInProgress";
            //-----------------------------------------------------
            
            /////  /////
            try
            {
                _log.LogText(LogChannel, _processId, "***** Begin run PayECRReadProtocolxSale *****", "A01", "PayECRReadProtocolxSale.Run");

                responseFound = false;
                _localTimeout = false;
            
                _countDown.ForceResetCounter();
                _countDown.ChangeCountDown("Start", cDwnCode_StartDetectCard, defaultFirstWaitingTimeSec, 850, out _);

                int dataLen = 0;
                while (continueLoop)
                {
                    lootCount += 1;
                    recebuff = new byte[0];

                    _log?.LogText(LogChannel, _processId, $@"Loop Count: {lootCount}", "B01", "PayECRReadProtocolxSale.Run", AppDecorator.Log.MessageType.Debug);

                    do
                    {
                        Thread.Sleep(50);
                    } while ((_go_SerialPort.BytesToRead < 1) && (CheckProcessHasStop() == false) && (_localTimeout == false));

                    //-------------------------------------------------------------------------------------------------------------------------------------------------
                    //-------------------------------------------------------------------------------------------------------------------------------------------------
                    // Read & Check Availabled Data
                    dataLen = _go_SerialPort.BytesToRead;
                    if (dataLen > 0)
                    {
                        //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                        // Read Data from COM
                        if (dataLen <= 2)
                            recebuff = new byte[] { (byte)_go_SerialPort.ReadPortChar() };

                        else if (isExpectSTAData)
                        {
                            readPortTimeoutSec = (int)timeOut.Subtract(DateTime.Now).TotalSeconds;

                            if (readPortTimeoutSec > (300))
                                readPortTimeoutSec = 300;

                            else if (readPortTimeoutSec < 0)
                                readPortTimeoutSec = 1;

                            recebuff = _go_SerialPort.ReadPort(readPortTimeoutSec);
                        }
                        else 
                            recebuff = _go_SerialPort.ReadPort(defaultReadPortTimeoutSec);

                        //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                        // Data Checking
                        try
                        {
                            actualDataLen = 0;
                            cardDetectedMsg = "**";
                            _dataChecking.CheckReceivedData(recebuff, ResultStatusCode, isExpectSTAData,
                                out bool isSTXDataStringFoundX,
                                out string cardDetectedMsgX,
                                out string statusRemarkX,
                                out bool isProceedReceiveNextDataX,
                                out bool isEOTReceivedX,
                                out bool isNAKReceivedX,
                                out bool isNewResponseFoundX,
                                out bool isExpectingSTXDataOnNextReadingX,
                                out bool isDisallowStopX,
                                out bool isRecognizedDataX);

                            _log?.LogText(LogChannel, _processId, 
                                $@"CheckReceivedData -> isProceedReceiveNextDataX: {isProceedReceiveNextDataX}; isSTXDataStringFoundX: {isSTXDataStringFoundX}; cardDetectedMsgX: {cardDetectedMsgX}; statusRemarkX: {statusRemarkX}; isEOTReceivedX: {isEOTReceivedX}; isNAKReceivedX: {isNAKReceivedX}; isNewResponseFoundX: {isNewResponseFoundX}; isExpectingSTXDataOnNextReadingX: {isExpectingSTXDataOnNextReadingX}; isDisallowStopX: {isDisallowStopX}; ", 
                                "E10", "PayECRReadProtocolxSale.Run");

                            cardDetectedMsg = string.IsNullOrWhiteSpace(cardDetectedMsgX) ? "**" : cardDetectedMsgX;
                            ResultstatusRemark = statusRemarkX;
                            //-------------------------------------------------------------------------------------------
                            // .. check control flags
                            continueLoop = isProceedReceiveNextDataX;

                            if (isDisallowStopX)
                                DisableStopFlag();
                            //-------------------------------------------------------------------------------------------
                            // .. check status data
                            if (isNewResponseFoundX && (responseFound == false)) 
                            {
                                _countDown.ForceResetCounter();
                                _countDown.ChangeCountDown("Response Found", cDwnCode_WaitCompleteCardResponse, transactionWaitingTimeSec, 850, out _);
                                responseFound = true;
                            }
                            // (isEOTReceivedX) // (isNAKReceivedX)
                            //-------------------------------------------------------------------------------------------
                            // .. check received data
                            if (isSTXDataStringFoundX)
                            {
                                actualDataLen = recebuff.Length;
                                ResultReadData = System.Text.ASCIIEncoding.ASCII.GetString(recebuff);

                                if (ComputeLRCforByte(recebuff))
                                {
                                    ResultstatusRemark = "LRC OK";
                                    _log?.LogText(LogChannel, _processId, "#Success :" + ResultstatusRemark, "D01", "PayECRReadProtocolxSale.Run");
                                    // .. send ACK
                                    _go_SerialPort.WritePort2(_hxACK, "Success_LRC_Checking");
                                    
                                    ResultReadData = ResultReadData.Substring(1, actualDataLen - 3);
                                    ResultStatusCode = 0;
                                    lb_success = true;

                                    if ((isFinishing == false) && (_countDown.LastCountDownCode.Equals(cDwnCode_WaitEOT, StringComparison.InvariantCultureIgnoreCase) == false))
                                    {
                                        isFinishing = true;
                                        _countDown.ForceResetCounter();
                                        _countDown.ChangeCountDown("Success Finish Waiting", cDwnCode_WaitEOT, finishingWaitingTimeSec, 850, out _);
                                    }
                                    ///// wait for <EOT> .. continueLoop = true;
                                }

                                // Retry for data reading
                                else
                                {
                                    if (dataReadRetryCount > 2)
                                    {
                                        _go_SerialPort.WritePort2(_hxEOT, "FailSale_LRC_Checking_For_3_Times");
                                        _log?.LogText(LogChannel, _processId, $@"Send EOT after retry for 3 times", "K02", "PayECRReadProtocolxSale.Run");
                                        // End data reading after retry for 3 times
                                        SendAbort("Fail to read after retry for 3 times");
                                        continueLoop = false;
                                    }
                                    else
                                    {
                                        dataReadRetryCount++;
                                        // Request resend card transaction data
                                        _go_SerialPort.WritePort2(_hxNAK, "RetryOnFail_LRC_Checking");
                                        _log?.LogText(LogChannel, _processId, $@"#Fail : {ResultstatusRemark}", "X21", "PayECRReadProtocolxSale.Run", AppDecorator.Log.MessageType.Error);
                                    }

                                    ResultstatusRemark = "LRC Error!";
                                    ResultStatusCode = 5;
                                }
                            }
                            else if (isNAKReceivedX)
                            {
                                ResultStatusCode = 3; ///// ResultstatusRemark = "Read NAK"; continueLoop = false; <---- Handled by isProceedReceiveNextDataX refer to _dataChecking.CheckReceivedData(..)
                            }
                            else if ((recebuff?.Length > 0) && (isExpectSTAData) && (isExpectingSTXDataOnNextReadingX == false))
                            {
                                string xData = PayECRComPort.AsciiOctets2String(recebuff); 
                                ResultstatusRemark = "IM20 bug X01 occur; Expected STA Data is Missing!";
                                ResultStatusCode = 99;
                                _log?.LogText(LogChannel, _processId, $@"#Fail : {ResultstatusRemark}; XData: {xData}", "X31", "PayECRReadProtocolxSale.Run", AppDecorator.Log.MessageType.Error);
                                dataReadRetryCount++;
                                _go_SerialPort.WritePort2(_hxNAK, "STAData_Missing");
                            }
                            else if ((recebuff?.Length > 0) && (isRecognizedDataX == false))
                            {
                                if (_go_SerialPort.BytesToRead == 0)
                                {
                                    // .. send ACK
                                    _go_SerialPort.WritePort2(_hxACK, "Unrecognized_CardSaleData");
                                }
                            }
                            //-------------------------------------------------------------------------------------------
                            isExpectSTAData = isExpectingSTXDataOnNextReadingX;
                        }
                        catch (Exception ex)
                        {
                            if (ResultStatusCode != 0)
                            {
                                ResultStatusCode = 99;
                                ResultstatusRemark = $@"Error; {ex.Message}";
                            }
                            _log?.LogError(LogChannel, _processId, ex, "EX50", "PayECRReadProtocolxSale.Run");
                        }
                        //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                    }

                    //-------------------------------------------------------------------------------------------------------------------------------------------------
                    //-------------------------------------------------------------------------------------------------------------------------------------------------
                    // No Data read with Timeout OR Stop Requested
                    else if (_localTimeout || CheckProcessHasStop())
                    {
                        if (responseFound == false)
                        {
                            // State 1 - Start Cancel Process
                            if (_countDown.LastCountDownCode.Equals(cDwnCode_StartDetectCard, StringComparison.InvariantCultureIgnoreCase)
                                || CheckProcessHasStop())
                            {
                                if (CheckProcessHasStop())
                                {
                                    SendAbort("CancelTransaction");
                                    ShowInProgress(new InProgressEventArgs() { Message = $"..progressing {cardDetectedMsg} - {lootCount.ToString()} - Stop" });
                                }
                                else
                                    ShowInProgress(new InProgressEventArgs() { Message = $"..progressing {cardDetectedMsg} - {lootCount.ToString()} - Timeout" });

                                _log?.LogText(LogChannel, _processId, $@"No response Found - Cancelling ..", "G01", "PayECRReadProtocolxSale.Run");

                                _countDown.ForceResetCounter();
                                if (_countDown.ChangeCountDown("Cancelling", cDwnCode_CancelInProgress, maxCancelDelayTimeSec, 700, out bool isAlreadyExpired) == true)
                                    _localTimeout = false;
                            }

                            //////////// State 2 - Ending/Stopping Process after Finished Cancel
                            //////////else if ((_localTimeout) && (_countDown.LastCountDownCode.Equals(cDwnCode_CancelInProgress, StringComparison.InvariantCultureIgnoreCase)))
                            //////////{
                            //////////    _log?.LogText(LogChannel, _processId, $@"No response Found - Ending/Stopping ..", "G05", "PayECRReadProtocolxSale.Run");
                            //////////    _countDown.ForceResetCounter();
                            //////////    if (_countDown.ChangeCountDown(@"Ending/Stopping", cDwnCode_StopInProgress, maxStopDelayTimeSec, 700, out bool isAlreadyExpired) == true)
                            //////////        _localTimeout = false;
                            //////////}

                            // State 2 - End After Timeout
                            else if (_localTimeout)
                            {
                                SendAbort("Ending/Stopping with no card detected");
                                if (CheckProcessHasStop())
                                {
                                    _log?.LogText(LogChannel, _processId, @"ABORT base on STOP", "G08", "PayECRReadProtocolxSale.Run");
                                }
                                else
                                {
                                    _log?.LogText(LogChannel, _processId, @"ABORT cause by timeout", "G11", "PayECRReadProtocolxSale.Run");
                                }

                                _log?.LogText(LogChannel, _processId, $@"Timeout; No response Found - Process END ..; LastCountDownCode: {_countDown.LastCountDownCode}; ProcessHasStop: {CheckProcessHasStop()}", "G51", "PayECRReadProtocolxSale.Run");

                                if (ResultStatusCode == 99)
                                    ResultStatusCode = 1;

                                continueLoop = false;
                            }

                            // Waiting
                            else
                            {
                                LogAbnormalLoop("Waiting for timeout");
                                Thread.Sleep(100);
                            }
                        }

                        // Timeout with data found
                        else if (_localTimeout)
                        {
                            continueLoop = false;

                            if (ResultStatusCode == 0)
                            {
                                // .. EOT not received
                                _go_SerialPort.WritePort2(_hxEOT, "Timeout_with_complete_data");
                                _log?.LogText(LogChannel, _processId, $@"Timeout with success data", "K01", "PayECRReadProtocolxSale.Run");
                            }
                            else
                            {
                                if (string.IsNullOrWhiteSpace(ResultstatusRemark))
                                {
                                    ResultStatusCode = 99;
                                    ResultstatusRemark = "Timeout with data response but fail data reading";
                                }
                                
                                SendAbort("ENDED; Timeout with data response but fail data reading");
                                _go_SerialPort.WritePort2(_hxEOT, "Timeout_with_data_detected");
                                _log?.LogText(LogChannel, _processId, $@"Send EOT on timeout", "K02", "PayECRReadProtocolxSale.Run");
                            }

                            _log?.LogText(LogChannel, _processId, $@"ENDED; ", "K10", "PayECRReadProtocolxSale.Run");
                        }

                        // Cancelling / Stopping looping ..
                        else
                        {
                            LogTimeoutWaiting("Wait for Cancelling/Stopping timeout");
                            Thread.Sleep(100);
                        }
                    }

                    //-------------------------------------------------------------------------------------------------------------------------------------------------
                    //-------------------------------------------------------------------------------------------------------------------------------------------------
                    // Log Abnormal Working
                    else
                    {
                        LogAbnormalLoop("Abnormal; No data area");
                    }
                }
            }
            finally
            {
                _countDown.ForceResetCounter();

                _log?.LogText(LogChannel, _processId, $@"***** End run PayECRReadProtocolxSale *****", "P01", "PayECRReadProtocolxSale.Run");
            }

            if (string.IsNullOrWhiteSpace(ResultstatusRemark) && (ResultStatusCode != 0))
            {
                ResultStatusCode = 99;
                ResultstatusRemark = "Abnormal data reading";
            }

            return lb_success;

            /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            
            void LogAbnormalLoop(string tag)
            {
                if (nextLogAbnormalLoopTime.Subtract(DateTime.Now).TotalMilliseconds < 0)
                {
                    _log?.LogText(LogChannel, _processId, $@"Tag: {tag}; Abnormal Loop check point", "A01", "PayECRReadProtocolxSale.LogAbnormalLoop");
                    nextLogAbnormalLoopTime = nextLogAbnormalLoopTime.AddSeconds(20);
                }
            }

            void LogTimeoutWaiting(string tag)
            {
                if (nextLogTimeoutWaitingTime.Subtract(DateTime.Now).TotalMilliseconds < 0)
                {
                    _log?.LogText(LogChannel, _processId, $@"Tag: {tag}; Timeout Waiting check point", "A01", "PayECRReadProtocolxSale.LogTimeoutWaiting");
                    nextLogTimeoutWaitingTime = nextLogTimeoutWaitingTime.AddSeconds(30);
                }
            }
        }

        private void _countDown_OnExpired(object sender, ExpiredEventArgs e)
        {
            _localTimeout = true;
            _log?.LogText(LogChannel, _processId, $@"CountDown Expired Code: {e.CountDownCode}", "A01", "PayECRReadProtocolxSale._countDown_OnExpired");
        }

        private void _countDown_OnCountDown(object sender, CountDownEventArgs e)
        {
            
        }

        private void SendAbort(string tag)
        {
            if (_abortFlag == false)
            {
                _abortFlag = true;
                _log?.LogText(LogChannel, _processId, $@"COM Abort Tag: {tag}", "A01", "PayECRReadProtocolxSale.Abort", AppDecorator.Log.MessageType.Debug);
                _go_SerialPort.WritePort2("ABORT", $@"SendAbort()::{tag}");
            }
        }

        private bool CheckProcessHasDisposed()
        {
            try
            {
                return _processHasDisposedDelgHandle.Invoke();
            }
            catch (Exception ex)
            {
                _log?.LogError(LogChannel, _processId, ex, "EX01", "PayECRReadProtocolxSale.CheckProcessHasDisposed");
            }

            throw new Exception("The 'processHasDisposedDelgHandle' is missing");
        }

        private void DisableStopFlag()
        {
            try
            {
                if (_disallowStopDelgHandle != null)
                    _disallowStopDelgHandle.Invoke();

                else
                    throw new Exception("The DisallowStopDelgHandle' is missing");
            }
            catch (Exception ex)
            {
                _log?.LogError(LogChannel, _processId, ex, "EX01", "PayECRReadProtocolxSale.DisableStopFlag");
                throw ex;
            }
        }

        private bool CheckProcessHasStop()
        {
            try
            {
                if (_checkProcessHasStopDelgHandle != null)
                    return _checkProcessHasStopDelgHandle.Invoke();

                else
                    throw new Exception("The CheckProcessHasStopDelgHandle' is missing");
            }
            catch (Exception ex)
            {
                _log?.LogError(LogChannel, _processId, ex, "EX01", "PayECRReadProtocolxSale.CheckProcessHasStop");
                throw ex;
            }
        }

        private bool ComputeLRCforByte(byte[] data)
        {
            try
            {
                if (_computeLRCforByteDelgHandle != null)
                    return _computeLRCforByteDelgHandle.Invoke(data);

                else
                    throw new Exception("The ComputeLRCforByteDelgHandle is missing");
            }
            catch (Exception ex)
            {
                _log?.LogError(LogChannel, _processId, ex, "EX01", "PayECRReadProtocolxSale.ComputeLRCforByte");
                throw ex;
            }
        }

        public void ShowInProgress(InProgressEventArgs args)
        {
            try
            {
                _showInProgressDelgHandle?.Invoke(args);
            }
            catch (Exception ex)
            {
                _log?.LogError(LogChannel, _processId, ex, "EX01", "PayECRReadProtocolxSale_DataChecking.ShowInProgress");
            }
        }
    }
}
