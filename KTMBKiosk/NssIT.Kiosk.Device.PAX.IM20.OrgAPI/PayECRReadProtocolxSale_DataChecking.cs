using NssIT.Kiosk.AppDecorator.DomainLibs.Common.CreditDebitCharge;
using NssIT.Kiosk.Device.PAX.IM20.OrgAPI.Base;
using NssIT.Kiosk.Device.PAX.IM20.OrgAPI.Base.ExpectReading;
using NssIT.Kiosk.Log.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NssIT.Kiosk.Device.PAX.IM20.OrgAPI.PayECR;
using static NssIT.Kiosk.Device.PAX.IM20.OrgAPI.PayECRReadProtocolxSale;

namespace NssIT.Kiosk.Device.PAX.IM20.OrgAPI
{
    public class PayECRReadProtocolxSale_DataChecking : IDisposable
    {
        private const string LogChannel = "PAX_IM20_API";

        private PayECRComPort _go_SerialPort;
        private DbLog _log = null;

        private string _processId = "*";

        private ShowInProgressDelg _showInProgressDelgHandle = null;

        private ITransExpectData _expDataSTXString = new ExpectSTXStringData("STX-Data");
        private ITransExpectData _expDataEOT = new ExpectSingleByteData((byte)4, "EOT", answerUponDataFound: null);
        private ITransExpectData _expDataACK = new ExpectSingleByteData((byte)6, "ACK", answerUponDataFound: null);
        private ITransExpectData _expDataNAK = new ExpectSingleByteData((byte)21, "NAK", answerUponDataFound: null);
        private ITransExpectData _expDataENQ = new ExpectSingleByteData((byte)5, "ENQ", answerUponDataFound: "\x06", answerWithCleanCOMBuffer: true);
        private ITransExpectData _expDataCrdCLES = new ExpectManyBytesData("CLES", "Card-CLES", answerUponDataFound: "\x06", answerWithCleanCOMBuffer: true);
        private ITransExpectData _expDataCrdCARD = new ExpectManyBytesData("CARD", "Card-CARD", answerUponDataFound: "\x06", answerWithCleanCOMBuffer: true);
        private ITransExpectData _expDataCrdMAGS = new ExpectManyBytesData("MAGS", "Card-MAGS", answerUponDataFound: "\x06", answerWithCleanCOMBuffer: true);
        private ITransExpectData _expDataCrdSCAN = new ExpectManyBytesData("SCAN", "Card-SCAN", answerUponDataFound: "\x06", answerWithCleanCOMBuffer: true);
        private ITransExpectData _expDataCrdPIN = new ExpectManyBytesData("PIN", "Card-PIN", answerUponDataFound: "\x06", answerWithCleanCOMBuffer: true);
        private ITransExpectData _expDataCrdPEF = new ExpectManyBytesData("PEF", "Card-PEF", answerUponDataFound: "\x06", answerWithCleanCOMBuffer: true);

        public PayECRReadProtocolxSale_DataChecking(ShowInProgressDelg showInProgressDelgHandle)
        {
            _showInProgressDelgHandle = showInProgressDelgHandle;
        }

        public void Dispose()
        {
            _log = null;
            _go_SerialPort = null;
            _showInProgressDelgHandle = null;

            try { _expDataSTXString.EndDispose(); } catch { }

            try { _expDataEOT.EndDispose(); } catch { }
            try { _expDataACK.EndDispose(); } catch { }
            try { _expDataNAK.EndDispose(); } catch { }
            try { _expDataENQ.EndDispose(); } catch { }

            try { _expDataCrdCLES.EndDispose(); } catch { }
            try { _expDataCrdCARD.EndDispose(); } catch { }
            try { _expDataCrdMAGS.EndDispose(); } catch { }
            try { _expDataCrdSCAN.EndDispose(); } catch { }
            try { _expDataCrdPIN.EndDispose(); } catch { }
            try { _expDataCrdPEF.EndDispose(); } catch { }
        }

        public void InitReader(PayECRComPort goSerialPort, string processId)
        {
            _log = DbLog.GetDbLog();
            _go_SerialPort = goSerialPort;
            _processId = processId;

            _expDataSTXString.InitReader(_go_SerialPort, _processId);

            _expDataEOT.InitReader(_go_SerialPort, _processId);
            _expDataACK.InitReader(_go_SerialPort, _processId);
            _expDataNAK.InitReader(_go_SerialPort, _processId);
            _expDataENQ.InitReader(_go_SerialPort, _processId);

            _expDataCrdCLES.InitReader(_go_SerialPort, _processId);
            _expDataCrdCARD.InitReader(_go_SerialPort, _processId);
            _expDataCrdMAGS.InitReader(_go_SerialPort, _processId);
            _expDataCrdSCAN.InitReader(_go_SerialPort, _processId);
            _expDataCrdPIN.InitReader(_go_SerialPort, _processId);
            _expDataCrdPEF.InitReader(_go_SerialPort, _processId);
        }

        public void CheckReceivedData(byte[] data, int existingStatusCode, bool isExpectingSTXData,
            out bool isSTXDataStringFound,
            out string cardDetectedMsg,
            out string statusRemark,
            out bool isProceedReceiveNextData,
            out bool isEOTReceived,
            out bool isNAKReceived,
            out bool isNewResponseFound,
            out bool isExpectingSTXDataOnNextReading,
            out bool isDisallowStop,
            out bool isRecognizedData)
        {
            isProceedReceiveNextData = true;
            isSTXDataStringFound = false;
            isExpectingSTXDataOnNextReading = false;
            isEOTReceived = false;
            isNAKReceived = false;
            cardDetectedMsg = null;
            statusRemark = null;
            isNewResponseFound = false;
            isDisallowStop = false;
            isRecognizedData = false;

            int dataLen = data.Length;

            if (dataLen > 0)
                isNewResponseFound = true;

            // ----- <EOT> -----
            if ((dataLen == 1) && (_expDataEOT.IsFound(data)))
            {
                isRecognizedData = true;
                isEOTReceived = true;
                isProceedReceiveNextData = false;
                AppDecorator.Log.MessageType mTy = AppDecorator.Log.MessageType.Info;

                // Note : existingStatusCode == 0 means Response Data String Found then read data LRC successully.
                if (existingStatusCode == 0)
                {
                    statusRemark = "EOT Success";
                    cardDetectedMsg = "<EOT> End of Transmission detected";
                }
                else
                {
                    statusRemark = "IM20 bug X01 occur with EOT";
                    cardDetectedMsg = "IM20 bug X01 occur with <EOT>; Receive EOT before success receive of card response data";
                    mTy = AppDecorator.Log.MessageType.Error;
                }

                _log?.LogText(LogChannel, _processId, $@"#Received-Process : {cardDetectedMsg}", "B50", "PayECRReadProtocolxSale_DataChecking.CheckReceivedData", mTy);
            }

            // ----- <ENQ> -----
            else if ((dataLen == 1) && (_expDataENQ.IsFound(data)))
            {
                isRecognizedData = true;
                cardDetectedMsg = "<ENQ> Enquiry detected";
                isExpectingSTXDataOnNextReading = true;
            }

            // ----- <ACK> -----
            else if ((dataLen == 1) && (_expDataACK.IsFound(data)))
            {
                isRecognizedData = true;
                isNewResponseFound = false;
                cardDetectedMsg = "<ACK> Ack. detected";
            }

            // ----- <NAK> -----
            else if ((dataLen == 1) && (_expDataNAK.IsFound(data)))
            {
                isRecognizedData = true;
                isNAKReceived = true;
                isProceedReceiveNextData = false;
                cardDetectedMsg = "<NAK> Negative Ack. detected";
                statusRemark = "Read NAK";
            }

            else
            {
                // ----- PIN -----
                if ((dataLen == 3) && (_expDataCrdPIN.IsFound(data)))
                {
                    isRecognizedData = true;
                    cardDetectedMsg = $@"(PIN Request)";
                }
                // ----- PEF -----
                else if ((dataLen == 3) && (_expDataCrdPEF.IsFound(data)))
                {
                    isRecognizedData = true;
                    cardDetectedMsg = $@"(PIN Entry Found)";
                }
                // ----- CLES -----
                else if ((dataLen == 4) && (_expDataCrdCLES.IsFound(data)))
                {
                    isRecognizedData = true;
                    cardDetectedMsg = $@"(Card Tapped)";
                }
                // ----- CARD -----
                else if ((dataLen == 4) && (_expDataCrdCARD.IsFound(data)))
                {
                    isRecognizedData = true;
                    cardDetectedMsg = $@"(Card Inserted and Verified)";
                }
                // ----- MAGS -----
                else if ((dataLen == 4) && (_expDataCrdMAGS.IsFound(data)))
                {
                    isRecognizedData = true;
                    cardDetectedMsg = $@"(Mag. Stripe and Verified)";
                }
                // ----- SCAN -----
                else if ((dataLen == 4) && (_expDataCrdSCAN.IsFound(data)))
                {
                    isRecognizedData = true;
                    cardDetectedMsg = $@"(Barcode Scan and Verified)";
                }
                // ----- STX Data -----
                else if ((dataLen > 2) && (_expDataSTXString.IsFound(data)))
                {
                    isRecognizedData = true;
                    cardDetectedMsg = $@"(Card Response Data Found)";
                    isSTXDataStringFound = true;
                }

                if (string.IsNullOrWhiteSpace(cardDetectedMsg) == false)
                    isDisallowStop = true;
            }
        
            //------------------------------------------------------------
            if (string.IsNullOrWhiteSpace(cardDetectedMsg) == false)
            {
                _log.LogText(LogChannel, _processId, $@"{cardDetectedMsg}", "C01", "PayECRReadProtocolxSale_DataChecking.CheckReceivedData");
                ShowInProgress(new InProgressEventArgs() { Message = $@"..progressing - {cardDetectedMsg}" });
            }
            // ..log unknown data
            else
            {
                string dUnk = PayECRComPort.AsciiOctets2String(data);
                cardDetectedMsg = $@"unknown data (xxx)..";
                _log?.LogText(LogChannel, _processId, $@"{cardDetectedMsg}; Data: {dUnk}", "C50", "PayECRReadProtocolxSale_DataChecking.CheckReceivedData", AppDecorator.Log.MessageType.Error);
                ShowInProgress(new InProgressEventArgs() { Message = $@"..progressing - unknown data (xxx).." });
            }

            //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            if (isExpectingSTXData && (isSTXDataStringFound == false) && (statusRemark is null))
            {
                statusRemark = "IM20 bug X01 occur";
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
