using Newtonsoft.Json;
using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.Base;
using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.TransParams;
using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.TransResult;
using NssIT.Kiosk.AppDecorator.Log;
using NssIT.Kiosk.Device.PAX.IM30.OrgAPI.Base;
using NssIT.Kiosk.Log.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NssIT.Kiosk.Device.PAX.IM30.OrgAPI.Trans.Sale.IM30CardSale;

namespace NssIT.Kiosk.Device.PAX.IM30.OrgAPI.Trans.Sale
{
    public class IM30CardSale_TnGCard : ICardSaleProcess, IDisposable
    {
        private const string _logChannel = "IM30_API";

        private const int _maxResend2ndCommandCnt = 3;
        private ShowMessageLogDelg _logDEBUGStateDelgHandle = null;
        private UpdateProcessStateDelg _updateProcessStateDelgHandler = null;

        private IM30COMPort _im30Port = null;
        public I2ndCardCommandParam _cardTransParameter { get; private set; }
        private DateTime? _ackTimeout = null;

        private string _comPort = "-";
        private string _posTransId = "-";

        private DbLog _log = null;
        public DbLog Log
        {
            get
            {
                return _log ?? (_log = DbLog.GetDbLog());
            }
        }

        public IM30CardSale_TnGCard(IM30COMPort im30Port, I2ndCardCommandParam cardTransParam,
            UpdateProcessStateDelg updateProcessStateDelgHandler,
            ShowMessageLogDelg logDEBUGStateDelgHandle = null)
        {
            if (cardTransParam is null)
                throw new Exception("-Invalid Touch n Go Card parameter object~");

            _im30Port = im30Port;
            _cardTransParameter = cardTransParam;
            _updateProcessStateDelgHandler = updateProcessStateDelgHandler;
            _logDEBUGStateDelgHandle = logDEBUGStateDelgHandle;
        }

        public void Dispose()
        {
            _logDEBUGStateDelgHandle = null;
            _updateProcessStateDelgHandler = null;
            _im30Port = null;
            _cardTransParameter = null;
            _ackTimeout = null;
            _log = null;
        }

        public void Shutdown()
        {
            Dispose();
        }

        int _resend2ndCommandCnt = 0;
        int _sendNAKCnt = 0;
        public void ProcessResponseData(byte[] recData, CardSaleProcessState currentProcessState,
            out bool? isEndWithOutStopCommand,
            out IIM30TransResult finalResult, out CardSaleProcessState expectedNextProcessState)
        {
            isEndWithOutStopCommand = null;
            expectedNextProcessState = currentProcessState;
            finalResult = null;

            if ((recData == null) || (recData?.Length == 0))
                return;

            string dStr = "";
            try
            {
                LogState($@"COM Port: {_im30Port}; Card (TnG) Reader Data received - proceed to ProcessResponseData");
                Log.LogText(_logChannel, _posTransId, $@"COM Port: {_im30Port}; Card (TnG) Reader Data received - proceed to ProcessResponseData",
                    "A01", "IM30CardSale_TnGCard.ProcessResponseData");
                //-----------------------------------------------------------------------------
                if (recData.Length == 1)
                {
                    dStr = IM30Tools.TranslateAsciiCode(recData[0]);

                    //When NAK
                    if ((int)ASCIICodeEn.NAK == (int)recData[0])
                    {
                        LogState($@"Received Char : {dStr}");
                        LogState($@"-Card(TnG) reader is stop with suspected error~' Current State :  {currentProcessState}");
                        expectedNextProcessState = CardSaleProcessState.ErrorHalt;

                        Log.LogText(_logChannel, _posTransId, $@"COM Port: {_im30Port}; Receive NAK; Card(TnG) reader is stop with suspected error~' Current State :  {currentProcessState}'",
                                "X11", "IM30CardSale_TnGCard.ProcessResponseData", AppDecorator.Log.MessageType.Error);
                    }
                    //When others
                    else
                    {
                        LogState($@"-Unregconized card(TnG) reader reading~' Char : {dStr}");
                        Log.LogText(_logChannel, _posTransId, $@"COM Port: {_im30Port}; Receive NAK; -Unregconized card(TnG) reader reading~' Char : {dStr}'",
                                "X21", "IM30CardSale_TnGCard.ProcessResponseData", AppDecorator.Log.MessageType.Error);
                    }
                }
                //------------------------------------------------------------------------------
                // Invalid Data Length
                else if (recData.Length < 5)
                {
                    dStr = $@"Received Unknown Data (Hex) : {BitConverter.ToString(recData)}{"\r\n"}";
                    dStr += $@"Received Unknown Data (Text) : {System.Text.Encoding.ASCII.GetString(recData)}";
                    LogState($@"{dStr}");
                    Log.LogError(_logChannel, _posTransId, new Exception($@"COM Port: {_im30Port}; {dStr}"), "X05", "IM30CardSale_TnGCard.ProcessResponseData");
                }
                //------------------------------------------------------------------------------
                // Validate Serial Data
                else
                {
                    // Pack Raw Data into IM30DataModel ----------------------------------------
                    if (IM30RequestResponseDataWorks.ConvertToIM30DataModel(recData, out IM30DataModel im30DataMod, out Exception error2))
                    {
                        LogState("-- Response --\r\n" + JsonConvert.SerializeObject(im30DataMod, Formatting.Indented));
                        Log.LogText(_logChannel, _posTransId, im30DataMod, $@"K01:COM Port: {_im30Port}", "IM30CardSale_TnGCard.ProcessResponseData");

                        if (currentProcessState == CardSaleProcessState.ExpectingFinalCardResponse)
                        {
                            //ValidateSaleResponse(im30DataMod, out bool isSaleFinalRespX, out bool isSaleSuccessX);
                            if (finalResult is null)
                                finalResult = new IM30CardSaleResult(im30DataMod);
                            _im30Port.WriteDataPort(PortProtocalDef.ACKData, "ACK to Final Card Transaction Result response receiving");

                            //CYA-DEBUG .. Test Data not received ----------------------------------------------------------------------
                            /////expectedNextProcessState = CardSaleProcessState.ExpectingFinalCardResponse;
                            //-------------------------------------------------------------------------------------------------------------
                            expectedNextProcessState = CardSaleProcessState.FinishEnding;

                            Log.LogText(_logChannel, _posTransId, "Succuss Receive Final Card Response", $@"K02:COM Port: {_im30Port}", "IM30CardSale_TnGCard.ProcessResponseData");
                        }
                        else
                        {
                            LogState($@"Unhandle process at state {currentProcessState}");
                            Log.LogError(_logChannel, _posTransId, new Exception($@"COM Port: {_im30Port}; Unhandle process at state {currentProcessState}"),
                                "X05", "IM30CardSale_TnGCard.ProcessResponseData");
                        }
                    }
                    // Error Found --------------------------------------------------------------
                    else if (error2 != null)
                    {
                        if (im30DataMod != null)
                        {
                            LogState("-- Response --\r\n" + JsonConvert.SerializeObject(im30DataMod, Formatting.Indented));
                            Log.LogText(_logChannel, _posTransId, im30DataMod, $@"K17:COM Port: {_im30Port}", "IM30CardSale_TnGCard.ProcessResponseData", AppDecorator.Log.MessageType.Error);
                        }

                        if ((_sendNAKCnt == 0) && (currentProcessState == CardSaleProcessState.ExpectingFinalCardResponse))
                        {
                            _sendNAKCnt++;
                            expectedNextProcessState = CardSaleProcessState.ExpectingFinalCardResponse;
                            _im30Port.WriteDataPort(PortProtocalDef.NAKData, "NAK to Final Card Transaction Result response receiving");
                            Log.LogText(_logChannel, _posTransId, "Send NAK to Card Reader to request Final Card Response", $@"K18:COM Port: {_im30Port}", "IM30CardSale_TnGCard.ProcessResponseData", AppDecorator.Log.MessageType.Error);
                        }
                        else
                        {
                            //Note : 2nd NAK to IM30 will cause IM30 stop transaction
                            if ((_sendNAKCnt > 0) && (currentProcessState == CardSaleProcessState.ExpectingFinalCardResponse))
                            {
                                _sendNAKCnt++;
                                isEndWithOutStopCommand = true;
                                _im30Port.WriteDataPort(PortProtocalDef.NAKData, "-NAK to 2nd Final Card Transaction Result Response to end transaction~");
                            }

                            if (finalResult is null)
                                finalResult = new IM30CardSaleResult(error2, im30DataMod);

                            expectedNextProcessState = CardSaleProcessState.ErrorHaltWithoutStopCommand;

                            LogState("-- Error --\r\n" + JsonConvert.SerializeObject(error2, Formatting.Indented));
                            Log.LogText(_logChannel, _posTransId, error2, $@"ED1#COM Port: {_im30Port}", "IM30CardSale_TnGCard.ProcessResponseData", AppDecorator.Log.MessageType.Error);
                        }
                    }
                    // Unknown Error ------------------------------------------------------------
                    else
                    {
                        if (finalResult is null)
                            finalResult = new IM30CardSaleResult(new Exception($@"-Unknown data reading error from card reader~ Last Process State : {currentProcessState}"), null);
                        expectedNextProcessState = CardSaleProcessState.ErrorHalt;

                        Log.LogText(_logChannel, _posTransId, $@"-Unknown Error when reading data from card reader~Last Process State : {currentProcessState}",
                            $@"X30#COM Port: {_im30Port}", "IM30CardSale_TnGCard.ProcessResponseData", AppDecorator.Log.MessageType.Error);

                        LogState("-- Unknown Error --\r\n" + "-Unknown error when reading data from card reader~");

                        dStr = $@"Error; 
Received Unknown Data (Hex) : {BitConverter.ToString(recData)}
Received Unknown Data (Text) : {System.Text.Encoding.ASCII.GetString(recData)}";
                        LogState($@"{dStr}");
                        Log.LogText(_logChannel, _posTransId, $@"COM Port: {_im30Port}; {dStr}",
                            "X21", "IM30CardSale_TnGCard.ProcessResponseData", AppDecorator.Log.MessageType.Error);

                        Log.LogText(_logChannel, _posTransId, $@"COM Port: {_im30Port} -Unknown Error when reading data from card reader~",
                            "X22", "IM30CardSale_TnGCard.ProcessResponseData", AppDecorator.Log.MessageType.Error);
                    }
                }
                //--------------------------------------------------------------------------------
            }
            catch (Exception ex)
            {
                Log.LogError(_logChannel, _posTransId, new Exception($@"{ex.Message}; COM Port: {_im30Port}", ex), "EX35", "IM30CardSale_TnGCard.ProcessResponseData");
            }
            return;
            /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            ///
            //void ValidateSaleResponse(IM30DataModel im30DataModelX, out bool isSaleFinalResponseX, out bool isSaleSuccessfulX)
            //{
            //    isSaleFinalResponseX = false;
            //    isSaleSuccessfulX = false;

            //    if (im30DataModelX == null)
            //        return;

            //    if (TransactionCodeDef.IsEqualTrans(TransactionCodeDef.ChargeAmount, im30DataModelX.TransactionCode))
            //    {
            //        isSaleFinalResponseX = true;

            //        if (ResponseCodeDef.IsEqualResponse(ResponseCodeDef.Approved, im30DataModelX.ResponseCode))
            //            isSaleSuccessfulX = true;
            //        else
            //            isSaleSuccessfulX = false;
            //    }
            //}
        }

        public void ProceedAction(CardSaleProcessState currentState, CardSaleProcessState proceedActionState, bool isRepeatACKRequested = false)
        {
            _updateProcessStateDelgHandler.Invoke(proceedActionState, "-Set ProcessState at ProceedAction (TnGCard.X)~"
                , isReverseWorking: isRepeatACKRequested, isForceToSetNewACKTimeout: isRepeatACKRequested);
        }

        private bool _is2ndCommandAlreadySent = false;
        public void Send2ndCommand(bool isResend = false)
        {
            if ((_is2ndCommandAlreadySent == false) || (isResend == true))
            {
                if (_cardTransParameter is TnGEntryCheckinParam param1)
                {
                    _updateProcessStateDelgHandler.Invoke(CardSaleProcessState.SendingTnGCheckinCommand, "-Set ProcessState at TnGCard.Send2ndCommand(A)~"
                        , isReverseWorking: isResend);
                    DoSend2ndEntryCommand(param1, "-TnGCard.Send2ndCommand~");
                    _is2ndCommandAlreadySent = true;
                    _updateProcessStateDelgHandler.Invoke(CardSaleProcessState.ExpectingFinalCardResponse, "-Set ProcessState at TnGCard.Send2ndCommand(B)~"
                        , isReverseWorking: isResend, isForceToSetNewACKTimeout: isResend);
                }
                else if (_cardTransParameter is TnGExitCheckoutParam param2)
                {
                    _updateProcessStateDelgHandler.Invoke(CardSaleProcessState.SendingTnGCheckinCommand, "-Set ProcessState at TnGCard.Send2ndCommand(A)~"
                        , isReverseWorking: isResend);
                    DoSend2ndExitCommand(param2, "-TnGCard.Send2ndCommand~");
                    _is2ndCommandAlreadySent = true;
                    _updateProcessStateDelgHandler.Invoke(CardSaleProcessState.ExpectingFinalCardResponse, "-Set ProcessState at TnGCard.Send2ndCommand(B)~"
                        , isReverseWorking: isResend, isForceToSetNewACKTimeout: isResend);
                }
                else
                {
                    if (_cardTransParameter is null)
                    {
                        LogState($@"-Command not sent; No command parameter available; TnGCard.Send2ndCommand~");
                        Log.LogError(_logChannel, _posTransId, new Exception($@"COM Port: {_im30Port}; -Command not sent; No command parameter available; TnG Card~"),
                            "X01", "IM30CardSale_TnGCard.Send2ndCommand");
                    }
                    else
                    {
                        LogState($@"-Command not sent; Command parameter not recognized; TnGCard.Send2ndCommand~ {_cardTransParameter.GetType().FullName}");
                        Log.LogError(_logChannel, _posTransId,
                            new Exception($@"COM Port: {_im30Port}; -Command not sent; Command parameter not recognized; TnG Card~ {_cardTransParameter.GetType().FullName}"),
                            "X05", "IM30CardSale_TnGCard.Send2ndCommand");
                    }
                }
            }
            else
            {
                LogState($@"-Deny to send command; Command already sent; TnGCard.Send2ndCommand~");
                Log.LogError(_logChannel, _posTransId,
                    new Exception($@"COM Port: {_im30Port}; -Deny to send command; Command already sent; TnG Card~"),
                    "X05", "IM30CardSale_TnGCard.Send2ndCommand");
            }
        }

        private void DoSend2ndEntryCommand(TnGEntryCheckinParam param, string loctTag)
        {
            if (string.IsNullOrWhiteSpace(loctTag))
                loctTag = "..unknown location tag.";

            try
            {
                string msgLog = "";
                _resend2ndCommandCnt++;
                //------------------------------------------------------------------------
                IM30DataModel theCommandModr = IM30RequestResponseDataWorks.CreateNewMessageData
                            (RequestResponseIndicatorDef.RequestAndResponse, TransactionCodeDef.Entry, ResponseCodeDef.Approved, MoreIndicatorDef.LastMessage);
                int inx = 0;
                theCommandModr.AddFieldElement(++inx, FieldTypeDef.PenaltyAmount, param.PenaltyAmount);
                theCommandModr.AddFieldElement(++inx, FieldTypeDef.PosTransId, param.PosTransId);
                if (param.EntryStationCode != null)
                    theCommandModr.AddFieldElement(++inx, FieldTypeDef.TKEntryStationCode, param.EntryStationCode);
                theCommandModr.AddFieldElement(++inx, FieldTypeDef.TKEntryDateTime, param.EntryDateTime);
                //------------------------------------------------------------------------
                LogState($@"-theCommandData of TnGCard.DoSend2ndEntryCommand~ Loct.:{loctTag}--{"\r\n"}" + JsonConvert.SerializeObject(theCommandModr, Formatting.Indented));
                Log.LogText(_logChannel, _posTransId, theCommandModr, $@"A05:COM Port:{_im30Port}; 2nd Command", "IM30CardSale_TnGCard.DoSend2ndCommand");

                byte[] commandData = IM30RequestResponseDataWorks.RenderData(theCommandModr);

                msgLog += $@"**{DateTime.Now:HH:mm:ss.fff_fff_1}; -Send TnGCard 2nd Command - Start~{"\r\n"}";

                string dataStr = BitConverter.ToString(commandData).Replace("-", "");
                LogState($@"TnGCard 2nd Card Command Data:{dataStr}");

                _im30Port.SetProcessId(param.PosTransId);
                _im30Port.WriteDataPort(commandData, "-Send TnGCard 2nd Command (B) to Port~)");
                msgLog += $@"**{DateTime.Now:HH:mm:ss.fff_fff_1}; -Send TnGCard 2nd Command - End~{"\r\n"}";

                msgLog += $@"Loct.:{loctTag}";
                LogState(msgLog);
                Log.LogText(_logChannel, _posTransId, msgLog, $@"A05:COM Port:{_im30Port}", "IM30CardSale_TnGCard.DoSend2ndCommand");
            }
            catch (Exception ex)
            {
                LogState(ex.ToString());
                Log.LogError(_logChannel, _posTransId, ex, $@"X05:COM Port:{_im30Port}", "IM30CardSale_TnGCard.DoSend2ndCommand");
            }
        }

        private void DoSend2ndExitCommand(TnGExitCheckoutParam param, string loctTag)
        {
            if (string.IsNullOrWhiteSpace(loctTag))
                loctTag = "..unknown location tag.";

            try
            {
                string msgLog = "";
                _resend2ndCommandCnt++;
                //------------------------------------------------------------------------
                IM30DataModel theCommandModr = IM30RequestResponseDataWorks.CreateNewMessageData
                            (RequestResponseIndicatorDef.RequestAndResponse, TransactionCodeDef.Exit, ResponseCodeDef.Approved, MoreIndicatorDef.LastMessage);
                int inx = 0;
                theCommandModr.AddFieldElement(++inx, FieldTypeDef.TransAmount, param.FareAmount);
                theCommandModr.AddFieldElement(++inx, FieldTypeDef.PenaltyAmount, param.PenaltyAmount);
                theCommandModr.AddFieldElement(++inx, FieldTypeDef.PosTransId, param.PosTransId);
                if (param.ExitStationCode != null)
                    theCommandModr.AddFieldElement(++inx, FieldTypeDef.TKExitStationCode, param.ExitStationCode);
                theCommandModr.AddFieldElement(++inx, FieldTypeDef.TKExitDateTime, param.ExitDateTime);
                //------------------------------------------------------------------------
                LogState($@"-theCommandData of TnGCard.DoSend2ndExitCommand~ Loct.:{loctTag}--{"\r\n"}" + JsonConvert.SerializeObject(theCommandModr, Formatting.Indented));
                Log.LogText(_logChannel, _posTransId, theCommandModr, $@"A05:COM Port:{_im30Port}; 2nd Command", "IM30CardSale_TnGCard.DoSend2ndCommand");

                byte[] commandData = IM30RequestResponseDataWorks.RenderData(theCommandModr);

                msgLog += $@"**{DateTime.Now:HH:mm:ss.fff_fff_1}; -Send TnGCard 2nd Command - Start~{"\r\n"}";

                string dataStr = BitConverter.ToString(commandData).Replace("-", "");
                LogState($@"TnGCard 2nd Card Command Data:{dataStr}");

                _im30Port.WriteDataPort(commandData, "-Send TnGCard 2nd Command (B) to Port~)");
                msgLog += $@"**{DateTime.Now:HH:mm:ss.fff_fff_1}; -Send TnGCard 2nd Command - End~{"\r\n"}";

                msgLog += $@"Loct.:{loctTag}";
                LogState(msgLog);
                Log.LogText(_logChannel, _posTransId, msgLog, $@"A05:COM Port:{_im30Port}", "IM30CardSale_TnGCard.DoSend2ndCommand");
            }
            catch (Exception ex)
            {
                LogState(ex.ToString());
                Log.LogError(_logChannel, _posTransId, ex, $@"X05:COM Port:{_im30Port}", "IM30CardSale_TnGCard.DoSend2ndCommand");
            }
        }

        private void LogState(string logMsg)
        {
            _logDEBUGStateDelgHandle?.Invoke(logMsg);
        }
    }
}
