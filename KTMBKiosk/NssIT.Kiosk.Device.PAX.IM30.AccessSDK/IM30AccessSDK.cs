using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.Base;
using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.CardEntityData;
using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.CardEntityData.CreditDebit;
using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.CardEntityData.KomLink;
using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.CardEntityData.Settlement;
using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.CardEntityData.Sys;
using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.CardEntityData.TnG;
using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.TransParams;
using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.TransResult;
using NssIT.Kiosk.AppDecorator.DomainLibs.KioskStatus;
using NssIT.Kiosk.AppDecorator.Log;
using NssIT.Kiosk.Device.PAX.IM30.AccessSDK.Base.AxCommandSet;
using NssIT.Kiosk.Log.DB;
using NssIT.Kiosk.Tools.ThreadMonitor;
using System;
using System.IO.Ports;
using System.Linq;
using System.Threading;

namespace NssIT.Kiosk.Device.PAX.IM30.AccessSDK
{
    public class IM30AccessSDK : IIM30AccessSDK, IDisposable
    {
        private const string _logChannel = "IM30_AccessSDK";

        public event EventHandler<CardTransResponseEventArgs> OnTransactionResponse;

        private bool _isDisposed = false;
        private string _deviceSerialNo = null;

        private DbLog _log = null;
        private IM30PortManagerAx _im30PortMan = null;
        private ShowMessageLogDelg _debugShowMessageDelgHandler = null;
        private string _processId = "NEW_" + DateTime.Now.ToString("dd_HHmmss_fff");
        private TransactionTypeEn _lastTransactionType = TransactionTypeEn.Unknown;

        private OnTransactionFinishedDelg _onTransactionFinishedDelgHandler = null;
        private NssIT.Kiosk.AppDecorator.DomainLibs.IM30.Base.OnCardDetectedDelg _onCardDetectedDelgHandler = null;
        private IsCardMachineDataCommNormalDelg _statusMon_IsCardMachineDataCommNormalDelgHandler = null;

        public string COMPort { get; private set; }

        public IM30AccessSDK(string im30COMPort, ShowMessageLogDelg debugShowMessageDelg)
        {
            if (string.IsNullOrWhiteSpace(im30COMPort))
                throw new Exception($@"Invalid COM Port specification");

            string comPort = im30COMPort.ToUpper().Trim();

            string[] portX = SerialPort.GetPortNames();

            if ((from prt in portX
                 where (prt?.Trim().Equals(comPort, StringComparison.InvariantCultureIgnoreCase) == true)
                 select prt).ToArray().Length == 0)
            {
                throw new Exception($@"Specified COM Port ({comPort}) not found");
            }

            _im30PortMan = IM30PortManagerAx.GetAxPortManager(comPort);

            if (_im30PortMan is null)
                throw new Exception($@"-Unable to get IM30 Port Access~");

            _log = DbLog.GetDbLog();

            COMPort = comPort;
            if (debugShowMessageDelg != null)
            {
                _debugShowMessageDelgHandler = debugShowMessageDelg;
                _im30PortMan.SetOnDebugShowMessageHandler(_debugShowMessageDelgHandler);
            }

            _onTransactionFinishedDelgHandler = new OnTransactionFinishedDelg(OnTransactionFinishedDelgWorking);
            _onCardDetectedDelgHandler = new OnCardDetectedDelg(OnCardDetectedDelgWorking);
        }

        public void SetStatusMonitor(IsCardMachineDataCommNormalDelg statusMon_IsCardMachineDataCommNormalDelgHandler)
        {
            _statusMon_IsCardMachineDataCommNormalDelgHandler = statusMon_IsCardMachineDataCommNormalDelgHandler;
            _im30PortMan?.SetStatusMonitor(statusMon_IsCardMachineDataCommNormalDelgHandler);
        }

        private void SendStatusMonitor(KioskCommonStatus status, string remark)
        {
            _statusMon_IsCardMachineDataCommNormalDelgHandler?.Invoke(status, remark);
        }

        public bool IsReaderStandingBy(out bool isDisposed, out bool isMalFunction)
        {
            isDisposed = false;
            isMalFunction = false;

            if (_im30PortMan == null)
                return false;

            bool isStdBy = _im30PortMan.IsReaderStandingBy(out bool isDisposedX, out bool isMalFunctionX);
            isDisposed = isDisposedX;
            isMalFunction = isMalFunctionX;

            return isStdBy;
        }

        public void Dispose()
        {
            _isDisposed = true;
            _debugShowMessageDelgHandler = null;
            _im30PortMan = null;
            _onTransactionFinishedDelgHandler = null;
            _onCardDetectedDelgHandler = null;
            _statusMon_IsCardMachineDataCommNormalDelgHandler = null;

            if (OnTransactionResponse != null)
            {
                Delegate[] delgList = OnTransactionResponse.GetInvocationList();
                foreach (EventHandler<CardTransResponseEventArgs> delg in delgList)
                    OnTransactionResponse -= delg;
            }
            _log = null;
        }

        public bool CreditDebitChargeCard(string posTransactionNo, decimal chargeAmount, out Exception error)
        {
            error = null;
            bool retRes = false;

            if (_isDisposed)
            {
                error = new Exception($@"IM30AccessSDK has been disposed. COM Port: {COMPort}");
                return false;
            }

            try
            {
                if (string.IsNullOrWhiteSpace(posTransactionNo))
                {
                    error = new Exception("-Please enter valid Credit/Debit Card (Charging) Document Number~");
                    retRes = false;
                }
                else if (chargeAmount < 0)
                {
                    error = new Exception("-Credit/Debit Card Charge amount must greater than zero~");
                    retRes = false;
                }
                else
                {
                    _log.LogText(_logChannel, posTransactionNo, $@"Doc.No.:{posTransactionNo}; Charge Amount: {chargeAmount:#,###.00}", "B10", "IM30AccessSDK.CreditDebitChargeCard",
                        adminMsg: $@"Doc.No.:{posTransactionNo}; Charge Amount: {chargeAmount:#,###.00}");

                    _lastTransactionType = TransactionTypeEn.CreditCard_2ndComm;
                    _processId = posTransactionNo.Trim();

                    if (_im30PortMan.Send2ndCommandParameter(new CreditDebitChargeParam(posTransactionNo.Trim(), chargeAmount)
                            , out bool? isFinalResultCollectFromEvent
                            , out IIM30TransResult transResult
                            , out Exception errorQ)
                    )
                    {
                        _log.LogText(_logChannel, _processId, "Credit/Debit Card Charge parameter has sent successfully", "B01", "IM30AccessSDK.CreditDebitChargeCar");
                        ShowMessageWorking("-Charge Credit Card parameters has been sent successful~");
                        retRes = true;
                    }
                    else if (errorQ != null)
                    {
                        error = errorQ;
                        ShowMessageWorking(error.ToString());
                        retRes = false;
                        _log.LogError(_logChannel, _processId, error, "EX11", "IM30AccessSDK.CreditDebitChargeCard");
                    }
                    else
                    {
                        error = new Exception("-Unknown Error; Fail to Send 2nd Card Command Parameter~Credit/Debit Card");
                        ShowMessageWorking(error.ToString());
                        _log.LogError(_logChannel, _processId, error, "EX21", "IM30AccessSDK.CreditDebitChargeCard");
                        retRes = false;
                    }
                }
            }
            catch (Exception ex)
            {
                _log.LogError(_logChannel, _processId, ex, "EX11", "IM30AccessSDK.CreditDebitChargeCard");
                if (error is null)
                    error = ex;
                ShowMessageWorking(ex.ToString());
            }

            if (error != null)
                _log.LogError(_logChannel, _processId, error, "EX100", "IM30AccessSDK.CreditDebitChargeCard");

            return retRes;
        }

        public bool GetLastTransaction(out ICardResponse responseResult, out Exception error)
        {
            error = null;
            responseResult = null;

            if (_isDisposed)
            {
                error = new Exception($@"IM30AccessSDK has been disposed. COM Port: {COMPort}");
                return false;
            }

            bool retRes = false;

            try
            {
                if (IsIM30PortManReady(out Exception er) == false)
                {
                    error = er;
                    return false;
                }

                _processId = $@"GetLastTrans_{DateTime.Now:dd_HHmmss_fff}";
                _im30PortMan.SetOnClientTransactionFinishedHandler(null);
                _im30PortMan.SetOnClientCardDetectedHandler(null);

                _lastTransactionType = TransactionTypeEn.GetLastTransaction;

                IIM30TransResult trnResult = _im30PortMan.RunSoloCommand(
                                            new GetLastTransAxComm()
                                            , out bool isPendingOutstandingTrans
                                            , out Exception errorQ);

                if (trnResult is IM30GetLastTransactionResult lastTrans)
                {
                    bool isTranslateSuccess = TranslateDataFromGetLastTransaction(lastTrans, out ICardResponse cardRespInfoB, out Exception errorB);

                    if (isTranslateSuccess)
                    {
                        responseResult = cardRespInfoB;
                        retRes = true;
                    }
                    else
                    {
                        error = errorB;
                    }
                }

                if (retRes == false)
                {
                    if ((trnResult?.IsSuccess == true)
                        &&
                        (ResponseCodeDef.IsEqualResponse(trnResult.ResultData.ResponseCode, ResponseCodeDef.TransactionNotAvailable)))
                    {
                        if (error == null)
                            error = new Exception("Last Transaction not available");
                    }
                    else if (errorQ != null)
                    {
                        if (error == null)
                            error = errorQ;
                    }
                    else
                    {
                        if (error == null)
                            error = new Exception("-Fail to Get Last Transaction(W)~");
                    }
                }

                if (error != null)
                    ShowMessageWorking(error.ToString());
            }
            catch (Exception ex)
            {
                _log.LogError(_logChannel, _processId, ex, "EX11", "IM30AccessSDK.GetLastTransaction");
                if (error is null)
                    error = ex;
            }

            if (error != null)
                _log.LogError(_logChannel, _processId, error, "EX100", "IM30AccessSDK.GetLastTransaction");

            return retRes;
        }

        private bool TranslateDataFromGetLastTransaction(IIM30TransResult trnResult, out ICardResponse cardRespInfo, out Exception error)
        {
            bool retRes = false;
            cardRespInfo = null;
            error = null;
            if ((trnResult?.IsSuccess == true)
                &&
                (ResponseCodeDef.IsEqualResponse(trnResult.ResultData.ResponseCode, ResponseCodeDef.Approved)))
            {
                IM30FieldElementModel expTransCode = (from fd in trnResult.ResultData.FieldElementCollection
                                                      where FieldTypeDef.IsEqualType(fd.FieldTypeCode, FieldTypeDef.TransCodeOfLastTrans)
                                                      select fd).FirstOrDefault();

                if ((expTransCode is null) || string.IsNullOrWhiteSpace(expTransCode.Data))
                {
                    error = new Exception("Transaction Code for Get Last Transaction sale info has not found");
                }
                else if (expTransCode.Data.Trim().ToUpper().Equals(TransactionCodeDef.ChargeAmount, StringComparison.InvariantCultureIgnoreCase))
                {
                    cardRespInfo = new CreditDebitChargeCardResp(trnResult.ResultData);
                    retRes = true;
                }
                else if (expTransCode.Data.Trim().ToUpper().Equals(TransactionCodeDef.Void, StringComparison.InvariantCultureIgnoreCase))
                {
                    cardRespInfo = new CreditDebitVoidTransactionResp(trnResult.ResultData, isDataGetFromLastTransaction: true);
                    retRes = true;
                }
                else if (expTransCode.Data.Trim().ToUpper().Equals(TransactionCodeDef.Entry, StringComparison.InvariantCultureIgnoreCase))
                {
                    CardTypeEn cardType = CardEntityDataTools.CheckCardType(trnResult.ResultData, out bool isSuccessData);

                    if (cardType == CardTypeEn.TouchNGo)
                    {
                        cardRespInfo = new TnGACGCheckinResp(trnResult.ResultData);
                        retRes = true;
                    }
                    else
                    {
                        error = new Exception($@"Card Type {cardType} for Get Last Transacion sale info is not supported");
                    }
                }
                else if (expTransCode.Data.Trim().ToUpper().Equals(TransactionCodeDef.Exit, StringComparison.InvariantCultureIgnoreCase))
                {
                    CardTypeEn cardType = CardEntityDataTools.CheckCardType(trnResult.ResultData, out bool isSuccessData);

                    if (cardType == CardTypeEn.TouchNGo)
                    {
                        cardRespInfo = new TnGACGCheckoutResp(trnResult.ResultData);
                        retRes = true;
                    }
                    else
                    {
                        error = new Exception($@"Card Type {cardType} for Get Last Transacion sale info is not supported");
                    }
                }
                else
                {
                    error = new Exception($@"Transaction Code ({expTransCode.Data.Trim()}) not supported when Get Last Transaction sale info");
                }
            }
            else
            {
                error = new Exception("-Fail to Get Last Transaction(W)~");
            }

            return retRes;
        }

        public bool KomLinkACGCheckout(string gateNo, DateTime transDatetime, string stationNo, byte spNo, DateTime spLastTravelDate, byte spDailyTravelTripCount, decimal chargeAmount, out KomLinkACGCheckoutResp responseResult, out Exception error)
        {
            throw new NotImplementedException();
        }

        public bool KomLinkBlackListCard(DateTime blacklistStartDatetime, string blacklistCode, out KomLinkBlackListCardResp responseResult, out Exception error, bool triggerBlacklistCard = false, DateTime? blacklistedDatetime = null)
        {
            throw new NotImplementedException();
        }

        public bool KomLinkCancelCard(out KomLinkCancelCardResp responseResult, out Exception error)
        {
            throw new NotImplementedException();
        }

        public bool KomLinkDeductValue(decimal deductAmount, DateTime transDatetime, out KomLinkDeductValueResp responseResult, out Exception error)
        {
            throw new NotImplementedException();
        }

        public bool KomLinkIncreaseValue(decimal topUpAmount, DateTime transDatetime, out KomLinkIncreaseValueResp responseResult, out Exception error)
        {
            throw new NotImplementedException();
        }

        public bool KomLinkIssueNewCard(DateTime transDatetime, ulong kvdtCardNo, DateTime cardExpireDate, out KomLinkIssueNewCardResp responseResult, out Exception error, string pnrNo = null, string cardType = null, DateTime? cardTypeExpireDate = null, GenderEn gender = GenderEn.NA, bool isMalaysian = false, DateTime? dob = null, IDTypeEn idType = IDTypeEn.NA, string idNo = null, string passengerName = null, decimal topUpValue = 0, byte spNo = 0, decimal spMaxTravelAmtPDayPTrip = 0.00M, DateTime? spStartDate = null, DateTime? spEndDate = null, string spSaleDocNo = null, string spServiceCode = null, string spPackageCode = null, string spType = null, byte spMaxTripCountPDay = 0, bool spIsAvoidChecking = false, bool spIsAvoidTripDurationCheck = false, string spOriginStationNo = null, string spDestinationStationNo = null)
        {
            throw new NotImplementedException();
        }

        public bool KomLinkIssueSeasonPass(byte spNo, DateTime transDatetime, DateTime spStartDate, DateTime spEndDate, string spSaleDocNo, string spServiceCode, out KomLinkIssueSeasonPassResp responseResult, out Exception error, decimal spMaxTravelAmtPDayPTrip = 0.00M, string spPackageCode = null, string spType = null, byte spMaxTripCountPDay = 0, bool spIsAvoidChecking = false, bool spIsAvoidTripDurationCheck = false, string spOriginStationNo = null, string spDestinationStationNo = null)
        {
            throw new NotImplementedException();
        }

        public bool KomLinkReadSeasonPassInfo(out KomLinkReadSeasonPassInfoResp responseResult, out Exception error)
        {
            throw new NotImplementedException();
        }

        public bool KomLinkRemoveBlackListCard(DateTime transDatetime, out KomLinkRemoveBlackListCardResp responseResult, out Exception error)
        {
            throw new NotImplementedException();
        }

        public bool KomLinkRemoveCancelCard(DateTime transDatetime, out KomLinkRemoveCancelCardResp responseResult, out Exception error)
        {
            throw new NotImplementedException();
        }

        public bool KomLinkResetACGChecking(out KomLinkResetACGCheckingResp responseResult, out Exception error)
        {
            throw new NotImplementedException();
        }

        public bool KomLinkUpdateCardInfo(DateTime transDatetime, GenderEn gender, string pnrNo, string cardType, DateTime cardTypeExpireDate, bool isMalaysian, DateTime dob, IDTypeEn idType, string idNo, string passengerName, out KomLinkUpdateCardInfoResp responseResult, out Exception error)
        {
            throw new NotImplementedException();
        }

        public bool StartACGCheckinCardTrans(int firstSPNo, int secondSPNo, int maxCardDetectedWaitingTimeSec, int maxSaleDecisionWaitingTimeSec, out string deviceSerialNo, out Exception error)
        {
            throw new NotImplementedException();
        }

        public bool StartACGCheckoutCardTrans(int firstSPNo, int secondSPNo, int maxCardDetectedWaitingTimeSec, int maxSaleDecisionWaitingTimeSec, out string deviceSerialNo, out Exception error)
        {
            throw new NotImplementedException();
        }

        private bool IsIM30PortManReady(out Exception error)
        {
            error = null;
            if (_im30PortMan.IsPortMalfunction)
            {
                error = new Exception("-IM30 Port Access is Malfunction. Check COM Port calibration. Or restart application~");
                return false;
            }
            else if (_im30PortMan.IsReaderInitializing)
            {
                error = new Exception("-IM30 Port Access still initializing. Please try again later~");
                return false;
            }
            else if (_im30PortMan.GetDeviceInfo is null)
            {
                error = new Exception("-Invalid Device Info (A)~");
                return false;
            }
            else if (string.IsNullOrWhiteSpace(_im30PortMan.GetDeviceInfo.DeviceSerialNo))
            {
                error = new Exception("-Invalid Device Info (B)~");
                return false;
            }

            _deviceSerialNo = _im30PortMan.GetDeviceInfo.DeviceSerialNo.Trim();

            return true;
        }

        public bool StartCardTransaction(int firstSPNo, int secondSPNo, int maxCardDetectedWaitingTimeSec, int maxSaleDecisionWaitingTimeSec,
            out string deviceSerialNo, out Exception error)
        {
            error = null;
            deviceSerialNo = null;

            if (_isDisposed)
            {
                error = new Exception($@"IM30AccessSDK has been disposed. COM Port: {COMPort}");
                return false;
            }

            try
            {
                if (IsIM30PortManReady(out Exception er) == false)
                {
                    error = er;
                    return false;
                }

                _log.LogText(_logChannel, _processId, "XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX Start Card Payment Transaction XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX", "B10", "IM30AccessSDK.StartCardTransaction");

                deviceSerialNo = _deviceSerialNo;

                _processId = $@"NewCardSale_{DateTime.Now:dd_HHmmss_fff}";
                _im30PortMan.SetOnClientTransactionFinishedHandler(_onTransactionFinishedDelgHandler);
                _im30PortMan.SetOnClientCardDetectedHandler(_onCardDetectedDelgHandler);

                _lastTransactionType = TransactionTypeEn.StartTrans_1stComm;

                bool isStartSuccessful = _im30PortMan.StartCardSaleTrans(
                                            new StartCardTransAxComm(GateDirectionEn.Counter, firstSPNo, secondSPNo
                                                , maxCardDetectedWaitingTimeSec, maxSaleDecisionWaitingTimeSec)
                                            , out bool isPendingOutstandingTrans
                                            , out Exception errorQ);

                if (isStartSuccessful == true)
                {
                    _log.LogText(_logChannel, _processId, "-Card Sale Start command has been sent~", "B10", "IM30AccessSDK.StartCardTransaction");
                    ShowMessageWorking($@"-Card Sale Start command has been sent~");
                    return true;
                }
                else
                {
                    if (errorQ != null)
                    {
                        error = errorQ;
                    }
                    else if (isPendingOutstandingTrans)
                    {
                        error = new Exception("-Outstanding Card Reader Transaction has not finished~");
                    }
                    else
                    {
                        error = new Exception("-Fail to start card sale transaction with unknown error~");
                    }

                    ShowMessageWorking(error.ToString());
                    _log.LogError(_logChannel, _processId, error, "X05", "IM30AccessSDK.StartCardTransaction");

                    {
                        string errMsg = $@"{error.Message}";
                        if (errMsg?.Length > 3900)
                            errMsg = errMsg.Substring(0, 3800);
                        SendStatusMonitor(KioskCommonStatus.No, $@"Error encountered; At {DateTime.Now:HH:mm:ss} ; {errMsg}");
                    }
                }
            }
            catch (Exception ex)
            {
                _log.LogError(_logChannel, _processId, ex, "EX11", "IM30AccessSDK.StartCardTransaction");
                if (error is null)
                    error = ex;

                if ((ex is ThreadStateException) == false)
                {
                    string errMsg = $@"{ex.Message}";
                    if (errMsg?.Length > 3900)
                        errMsg = errMsg.Substring(0, 3800);

                    SendStatusMonitor(KioskCommonStatus.No, $@"Error encountered; At {DateTime.Now:HH:mm:ss} ; {errMsg}");
                }
            }

            if (error != null)
                _log.LogError(_logChannel, _processId, error, "EX100", "IM30AccessSDK.StartCardTransaction");

            return false;
        }

        public bool StopCardTransaction(out Exception error)
        {
            error = null;
            bool retRes = false;

            if (_isDisposed)
            {
                error = new Exception($@"IM30AccessSDK has been disposed. COM Port: {COMPort}");
                return false;
            }

            try
            {
                _lastTransactionType = TransactionTypeEn.StopTrans;
                _processId = "Stop2ndComm_" + Guid.NewGuid().ToString();

                if (_im30PortMan.Send2ndCommandParameter(new StopCardTransParam()
                                , out bool? isFinalResultCollectFromEvent
                                , out IIM30TransResult transResult
                                , out Exception errorQ)
                )
                {
                    _log.LogText(_logChannel, _processId, "-Stop Card Trans. (2nd Command) parameters has been sent successful~", "B10", "IM30AccessSDK.StopCardTransaction");
                    ShowMessageWorking("-Stop Card Trans. (2nd Command) parameters has been sent successful~");

                    if (transResult is IM30CardSaleResult saleResult)
                    {
                        if (saleResult.IsManualStopped)
                        {
                            retRes = true;
                        }
                        else
                        {
                            error = new Exception("-Unable to stop (A) Card Sale Trans. Properly~");
                            ShowMessageWorking(error.ToString());
                        }
                    }
                    else if (transResult is IM30StopTransResult stopRes)
                    {
                        if (stopRes.IsSuccess)
                        {
                            retRes = true;
                            ShowMessageWorking("-Card Sale Trans. has stopped successful~");
                        }
                        else if (stopRes.Error != null)
                        {
                            error = stopRes.Error;
                            ShowMessageWorking(stopRes.Error.ToString());
                        }
                        else
                        {
                            error = new Exception("-Unable to stop (B) Card Sale Trans. Properly~");
                            ShowMessageWorking(error.ToString());
                        }
                    }
                }
                else if (errorQ != null)
                {
                    error = errorQ;
                    ShowMessageWorking(error.ToString());
                }
                else
                {
                    error = new Exception("-Unknown Error; Fail to Send 2nd Card Command Parameter~Stop Card Sale Trans.");
                    ShowMessageWorking(error.ToString());
                }
            }
            catch (Exception ex)
            {
                _log.LogError(_logChannel, _processId, ex, "EX11", "IM30AccessSDK.StopCardTransaction");
                if (error is null)
                    error = ex;
                ShowMessageWorking(ex.ToString());
            }

            if (error != null)
                _log.LogError(_logChannel, _processId, error, "EX100", "IM30AccessSDK.StopCardTransaction");

            return retRes;
        }

        public bool TnGACGCheckin(DateTime transDatetime, decimal penaltyChargeAmount, out TnGACGCheckinResp responseResult, out Exception error)
        {
            error = null;
            responseResult = null;
            bool retRes = false;

            if (_isDisposed)
            {
                error = new Exception($@"IM30AccessSDK has been disposed. COM Port: {COMPort}");
                return false;
            }

            try
            {
                retRes = false;

                if (penaltyChargeAmount < 0)
                {
                    error = new Exception("-Penalty Charge Amount must greater or equal than zero~");
                }
                else
                {
                    _lastTransactionType = TransactionTypeEn.TouchNGo_2ndComm_CheckIn;
                    _processId = "TnGIn_" + Guid.NewGuid().ToString();

                    if (_im30PortMan.Send2ndCommandParameter(new TnGEntryCheckinParam(_processId, penaltyChargeAmount, transDatetime)
                            , out bool? isFinalResultCollectFromEvent
                            , out IIM30TransResult transResult
                            , out Exception errorQ)
                    )
                    {
                        Exception errX = transResult?.Error ?? errorQ;

                        _log.LogText(_logChannel, _processId, "-TnG Card Check-in parameters has been sent successful~", "B01", "IM30AccessSDK.TnGACGCheckin");
                        ShowMessageWorking("-TnG Card Check-in parameters has been sent successful~");

                        if (transResult != null)
                        {
                            if ((transResult.ResultData is null) && transResult.IsTimeout)
                            {
                                error = new Exception("-Timeout; Fail to Send 2nd Card Command Parameter~TnG Checkin Card");
                                ShowMessageWorking(error.ToString());
                            }
                            else if ((transResult.ResultData is null) && transResult.IsManualStopped)
                            {
                                error = new Exception("-Transaction has stopped; Fail to Send 2nd Card Command Parameter~TnG Checkin Card");
                                ShowMessageWorking(error.ToString());
                            }
                            else if ((transResult.ResultData is null) && (transResult.Error != null))
                            {
                                error = transResult.Error;
                                ShowMessageWorking(error.ToString());
                            }
                            else if (transResult.ResultData is null)
                            {
                                error = new Exception("-Unknown Error (A); Fail to Send 2nd Card Command Parameter~TnG Checkin Card", errX);
                                ShowMessageWorking(error.ToString());
                            }
                            else if (TransactionCodeDef.IsEqualTrans(transResult.ResultData.TransactionCode, TransactionCodeDef.Entry))
                            {
                                if (TransactionCodeDef.IsEqualTrans(transResult.ResultData.ResponseCode, ResponseCodeDef.Approved))
                                {
                                    if ((from fe in transResult.ResultData.FieldElementCollection
                                         where FieldTypeDef.IsEqualType(fe.FieldTypeCode, FieldTypeDef.TnGEntryOperatorID)
                                         select fe).FirstOrDefault() != null)
                                    {
                                        ShowMessageWorking($@"TnG Check-in Final Result found; Process Id : {_processId}");

                                        responseResult = new TnGACGCheckinResp(transResult.ResultData);
                                        retRes = true;
                                    }
                                    else
                                    {
                                        responseResult = new TnGACGCheckinResp(new Exception("-Invalid TnG Check-in response-answer data~"));
                                    }
                                }
                                else
                                {
                                    responseResult = new TnGACGCheckinResp(new Exception("-Fail TnG Check-in~"));
                                }
                            }
                            else if (TransactionCodeDef.IsEqualTrans(transResult.ResultData.TransactionCode, TransactionCodeDef.GetLastTransaction))
                            {
                                IM30FieldElementModel expTransCode = (from fd in transResult.ResultData.FieldElementCollection
                                                                      where FieldTypeDef.IsEqualType(fd.FieldTypeCode, FieldTypeDef.TransCodeOfLastTrans)
                                                                      select fd).FirstOrDefault();

                                if (expTransCode.Data.Trim().ToUpper().Equals(TransactionCodeDef.Entry, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    CardTypeEn cardType = CardEntityDataTools.CheckCardType(transResult.ResultData, out bool isSuccessData);

                                    if (cardType == CardTypeEn.TouchNGo)
                                    {
                                        responseResult = new TnGACGCheckinResp(transResult.ResultData);
                                        retRes = true;
                                    }
                                    else
                                    {
                                        error = new Exception($@"Unmatch Card Type ({cardType}) refer to Get Last Transacion; TnG Check-in");
                                        ShowMessageWorking(error.ToString());
                                    }
                                }
                                else
                                {
                                    error = new Exception($@"Unmatch Trasnsaction Type ({expTransCode.Data}) refer to Get Last Transacion; TnG Check-in");
                                    ShowMessageWorking(error.ToString());
                                }
                            }
                            else
                            {
                                error = new Exception("-Unknown Error (B); Fail to Send 2nd Card Command Parameter~TnG Check-in", errX);
                                ShowMessageWorking(error.ToString());
                            }
                        }
                    }
                    else if (errorQ != null)
                    {
                        error = errorQ;
                        ShowMessageWorking(error.ToString());
                    }
                    else
                    {
                        error = new Exception("-Unknown Error (C); Fail to Send 2nd Card Command Parameter~TnG Check-in");
                        ShowMessageWorking(error.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                _log.LogError(_logChannel, _processId, ex, "EX11", "IM30AccessSDK.TnGACGCheckin");
                if (error is null)
                    error = ex;
                ShowMessageWorking(ex.ToString());
            }

            if (error != null)
                _log.LogError(_logChannel, _processId, error, "EX100", "IM30AccessSDK.TnGACGCheckin");

            return retRes;
        }

        public bool TnGACGCheckout(DateTime transDatetime, decimal fareAmount, decimal penaltyChargeAmount, out TnGACGCheckoutResp responseResult, out Exception error)
        {
            error = null;
            responseResult = null;
            bool retRes = false;

            if (_isDisposed)
            {
                error = new Exception($@"IM30AccessSDK has been disposed. COM Port: {COMPort}");
                return false;
            }

            try
            {
                retRes = false;

                if (penaltyChargeAmount < 0)
                {
                    error = new Exception("-Penalty Charge Amount must greater or equal than zero~");
                }
                else if (fareAmount < 0)
                {
                    error = new Exception("-Fare Amount must greater or equal than zero~");
                }
                else
                {
                    _lastTransactionType = TransactionTypeEn.TouchNGo_2ndComm_Checkout;
                    _processId = "TnGOut_" + Guid.NewGuid().ToString();

                    if (_im30PortMan.Send2ndCommandParameter(new TnGExitCheckoutParam(_processId, fareAmount, penaltyChargeAmount, transDatetime)
                            , out bool? isFinalResultCollectFromEvent
                            , out IIM30TransResult transResult
                            , out Exception errorQ)
                    )
                    {
                        Exception errX = transResult?.Error ?? errorQ;

                        _log.LogText(_logChannel, _processId, "-TnG Card Checkout parameters has been sent successful~", "B01", "IM30AccessSDK.TnGACGCheckout");
                        ShowMessageWorking("-TnG Card Checkout parameters has been sent successful~");

                        if (transResult != null)
                        {
                            if ((transResult.ResultData is null) && transResult.IsTimeout)
                            {
                                error = new Exception("-Timeout; Fail to Send 2nd Card Command Parameter~TnG Checkout");
                                ShowMessageWorking(error.ToString());
                            }
                            else if ((transResult.ResultData is null) && transResult.IsManualStopped)
                            {
                                error = new Exception("-Transaction has stopped; Fail to Send 2nd Card Command Parameter~TnG Checkout");
                                ShowMessageWorking(error.ToString());
                            }
                            else if ((transResult.ResultData is null) && (transResult.Error != null))
                            {
                                error = transResult.Error;
                                ShowMessageWorking(error.ToString());
                            }
                            else if (transResult.ResultData is null)
                            {
                                error = new Exception("-Unknown Error (A); Fail to Send 2nd Card Command Parameter~TnG Checkout", errX);
                                ShowMessageWorking(error.ToString());
                            }
                            else if (TransactionCodeDef.IsEqualTrans(transResult.ResultData.TransactionCode, TransactionCodeDef.Exit))
                            {
                                if (TransactionCodeDef.IsEqualTrans(transResult.ResultData.ResponseCode, ResponseCodeDef.Approved))
                                {
                                    if ((from fe in transResult.ResultData.FieldElementCollection
                                         where FieldTypeDef.IsEqualType(fe.FieldTypeCode, FieldTypeDef.TnGEntryOperatorID)
                                         select fe).FirstOrDefault() != null)
                                    {
                                        ShowMessageWorking($@"TnG Checkout Final Result found; Process Id : {_processId}");

                                        responseResult = new TnGACGCheckoutResp(transResult.ResultData);
                                        retRes = true;
                                    }
                                    else
                                    {
                                        responseResult = new TnGACGCheckoutResp(new Exception("-Invalid TnG Checkout response-answer data~"));
                                    }
                                }
                                else
                                {
                                    responseResult = new TnGACGCheckoutResp(new Exception("-Fail TnG Checkout~"));
                                }
                            }
                            else if (TransactionCodeDef.IsEqualTrans(transResult.ResultData.TransactionCode, TransactionCodeDef.GetLastTransaction))
                            {
                                IM30FieldElementModel expTransCode = (from fd in transResult.ResultData.FieldElementCollection
                                                                      where FieldTypeDef.IsEqualType(fd.FieldTypeCode, FieldTypeDef.TransCodeOfLastTrans)
                                                                      select fd).FirstOrDefault();

                                if (expTransCode.Data.Trim().ToUpper().Equals(TransactionCodeDef.Exit, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    CardTypeEn cardType = CardEntityDataTools.CheckCardType(transResult.ResultData, out bool isSuccessData);

                                    if (cardType == CardTypeEn.TouchNGo)
                                    {
                                        responseResult = new TnGACGCheckoutResp(transResult.ResultData);
                                        retRes = true;
                                    }
                                    else
                                    {
                                        error = new Exception($@"Unmatch Card Type ({cardType}) refer to Get Last Transacion; TnG Checkout");
                                        ShowMessageWorking(error.ToString());
                                    }
                                }
                                else
                                {
                                    error = new Exception($@"Unmatch Trasnsaction Type ({expTransCode.Data}) refer to Get Last Transacion; TnG Checkout");
                                    ShowMessageWorking(error.ToString());
                                }
                            }
                            else
                            {
                                error = new Exception("-Unknown Error (B); Fail to Send 2nd Card Command Parameter~TnG Checkout", errX);
                                ShowMessageWorking(error.ToString());
                            }
                        }
                    }
                    else if (errorQ != null)
                    {
                        error = errorQ;
                        ShowMessageWorking(error.ToString());
                    }
                    else
                    {
                        error = new Exception("-Unknown Error (C); Fail to Send 2nd Card Command Parameter~TnG Checkout");
                        ShowMessageWorking(error.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                _log.LogError(_logChannel, _processId, ex, "EX11", "IM30AccessSDK.TnGACGCheckout");
                if (error is null)
                    error = ex;
                ShowMessageWorking(ex.ToString());
            }

            if (error != null)
                _log.LogError(_logChannel, _processId, error, "EX100", "IM30AccessSDK.TnGACGCheckout");

            return retRes;
        }

        public bool SettleCreditDebitSales(out Exception error)
        {
            error = null;
            bool retRes = false;

            if (_isDisposed)
            {
                error = new Exception($@"IM30AccessSDK has been disposed. COM Port: {COMPort}");
                return false;
            }

            try
            {
                if (IsIM30PortManReady(out Exception er) == false)
                {
                    error = er;
                    return false;
                }

                _lastTransactionType = TransactionTypeEn.Settlement;
                _processId = "SettleMent_" + Guid.NewGuid().ToString();

                _im30PortMan.SetOnClientTransactionFinishedHandler(_onTransactionFinishedDelgHandler);
                _im30PortMan.SetOnClientCardDetectedHandler(null);

                IIM30TransResult trnResult = _im30PortMan.RunSoloCommand(
                                                    new CardSettlementAxComm(),
                                                    out bool isPendingOutstandingTransaction,
                                                    out Exception errorQ);

                if (trnResult?.IsSuccess == true)
                {
                    ShowMessageWorking("-Card Reader Settlement started Successful (GB01)~");
                    retRes = true;
                }
                else if (errorQ != null)
                {
                    error = errorQ;
                    ShowMessageWorking("-Fail to Card Settlement~; " + error.ToString());
                }
                else if (trnResult?.IsSuccess == false)
                {
                    error = new Exception("-Fail to execute Card Settlement command (GB01)~");
                    ShowMessageWorking(error.ToString());
                }
                else if (isPendingOutstandingTransaction)
                {
                    error = new Exception("-Found outstanding/previous Card Reader Transaction (GB01)~Card Settlement#");
                    ShowMessageWorking(error.ToString());
                }
                else
                {
                    error = new Exception("-Fail to start Card Reader (Single) command with unknown error (GB01)~Card Settlement#");
                    ShowMessageWorking(error.ToString());
                }
            }
            catch (Exception ex)
            {
                _log.LogError(_logChannel, _processId, ex, "EX11", "IM30AccessSDK.SettleCreditDebitSales");
                if (error is null)
                    error = ex;
                ShowMessageWorking(ex.ToString());
            }

            if (error != null)
                _log.LogError(_logChannel, _processId, error, "EX100", "IM30AccessSDK.SettleCreditDebitSales");

            return retRes;
        }

        public bool VoidCreditCardTransaction(string invoiceNo, decimal voidAmount, string cardToken, out CreditDebitVoidTransactionResp responseResult, out Exception error)
        {
            error = null;
            responseResult = null;

            if (_isDisposed)
            {
                error = new Exception($@"IM30AccessSDK has been disposed. COM Port: {COMPort}");
                return false;
            }

            bool retRes = false;

            try
            {
                if (IsIM30PortManReady(out Exception er) == false)
                {
                    error = er;
                    return false;
                }

                _im30PortMan.SetOnClientTransactionFinishedHandler(null);
                _im30PortMan.SetOnClientCardDetectedHandler(null);

                _lastTransactionType = TransactionTypeEn.VoidCreditCardTrans;
                _processId = $@"VoidTrans_{DateTime.Now:dd_HHmmss_fff}";

                IIM30TransResult trnResult = _im30PortMan.RunSoloCommand(
                                            new VoidTransactionAxComm(invoiceNo, voidAmount, cardToken)
                                            , out bool isPendingOutstandingTrans
                                            , out Exception errorQ);

                if (TransactionCodeDef.IsEqualTrans(trnResult.ResultData.TransactionCode, TransactionCodeDef.GetLastTransaction))
                {
                    bool isTranslateSuccess = TranslateDataFromGetLastTransaction(trnResult, out ICardResponse cardRespInfoB, out Exception errorB);

                    if (isTranslateSuccess)
                    {
                        if (cardRespInfoB is CreditDebitVoidTransactionResp voidTrans)
                        {
                            responseResult = voidTrans;
                            retRes = true;
                        }
                        else
                        {
                            error = new Exception("-Unable to read from last transaction (R01)~");
                        }
                    }
                    else
                    {
                        error = errorB;
                    }
                }
                else if (trnResult?.IsSuccess == true)
                {
                    responseResult = new CreditDebitVoidTransactionResp(trnResult.ResultData);

                    if (responseResult.ResponseResult == ResponseCodeEn.Success)
                    {
                        ShowMessageWorking("-Void Transaction from Card Reader has done (GB01)~");
                        retRes = true;
                    }
                    else
                    {
                        error = new Exception("-Fail to void Credit/Debit transaction (T)~Response data");
                        if (string.IsNullOrWhiteSpace(responseResult.ResponseText) && (responseResult.DataError is null))
                        {
                            responseResult = new CreditDebitVoidTransactionResp(error);
                        }
                        else if (responseResult.DataError != null)
                        {
                            if (string.IsNullOrWhiteSpace(responseResult.ResponseText))
                                responseResult = new CreditDebitVoidTransactionResp(responseResult.DataError);
                            else
                                responseResult = new CreditDebitVoidTransactionResp(new Exception(responseResult.ResponseText, responseResult.DataError));
                        }
                        else
                        {
                            if (string.IsNullOrWhiteSpace(responseResult.ResponseText))
                                responseResult = new CreditDebitVoidTransactionResp(error);
                            else
                                responseResult = new CreditDebitVoidTransactionResp(new Exception(responseResult.ResponseText, error));
                        }
                        ShowMessageWorking(responseResult.DataError.ToString());
                    }
                }
                else if (errorQ != null)
                {
                    error = errorQ;
                    ShowMessageWorking("-Fail to void Credit/Debit transaction(U)~; " + errorQ.ToString());
                }
                else if (trnResult?.IsSuccess == false)
                {
                    error = new Exception("-Fail to void Credit/Debit transaction(M) (GB01)~");
                    ShowMessageWorking(error.ToString());
                }
                else if (isPendingOutstandingTrans)
                {
                    error = new Exception("-Found outstanding/previous Card Reader Transaction (GB01)~void Credit/Debit transaction#");
                    ShowMessageWorking(error.ToString());
                }
                else if (trnResult.Error != null)
                {
                    error = trnResult.Error;
                    ShowMessageWorking(error.ToString());
                }
                else
                {
                    error = new Exception("-Unknown error; Fail to run Card Reader (Single) command (GB01)~void Credit/Debit transaction#");
                    ShowMessageWorking(error.ToString());
                }
            }
            catch (Exception ex)
            {
                _log.LogError(_logChannel, _processId, ex, "EX11", "IM30AccessSDK.VoidCreditCardTransaction");
                if (error is null)
                    error = ex;
            }

            if (error != null)
                _log.LogError(_logChannel, _processId, error, "EX100", "IM30AccessSDK.VoidCreditCardTransaction");

            return retRes;
        }

        private void ShowMessageWorking(string message)
        {
            try
            {
                _debugShowMessageDelgHandler?.Invoke(message + "\r\n");
            }
            catch (Exception ex)
            {
                string t1 = ex?.Message;
            }
        }

        private void OnCardDetectedDelgWorking(IM30DataModel cardInfo)
        {
            //_msg.ShowMessage($@"===== ===== Card Info found ===== ===== ===== ===== ====={"\r\n"}");

            CardTypeEn cardType = CardEntityDataTools.CheckCardType(cardInfo, out bool isSuccessData);
            if (cardType == CardTypeEn.CreditCard)
            {
                CreditDebitCardInfoResp cdCardResp = new CreditDebitCardInfoResp(cardInfo);
                SentTransactionResponseEvent(cdCardResp, TransactionTypeEn.CreditCardInfo, TransEventCodeEn.CardInfoResponse);

                _log.LogText(_logChannel, _processId, $@"Card No.:{cdCardResp.CardNumber} Detected", "B10", "IM30AccessSDK.OnCardDetectedDelgWorking",
                        adminMsg: $@"Card No.:{cdCardResp.CardNumber} Detected");
            }
            else if (cardType == CardTypeEn.TouchNGo)
            {
                TnGCardInfoResp cdCardResp = new TnGCardInfoResp(cardInfo);
                SentTransactionResponseEvent(cdCardResp, TransactionTypeEn.TouchNGoInfo, TransEventCodeEn.CardInfoResponse);
            }
            else
            {
                SentTransactionResponseEvent(new ErrorResponse(new Exception("-Unknown Card Type~")), _lastTransactionType, TransEventCodeEn.CardInfoResponse);
            }
        }

        private void OnTransactionFinishedDelgWorking(IIM30TransResult transResult)
        {
            if (transResult is null)
            {
                Exception exT = new Exception("-Fatal error; Card Reader Transaction result not found~");
                _log.LogError(_logChannel, _processId, exT, "X01", "IM30AccessSDK.OnTransactionFinishedDelgWorking");
                SentTransactionResponseEvent(new ErrorResponse(exT), _lastTransactionType, TransEventCodeEn.FailWithError);
                ShowMessageWorking($@"----- Invalid Transaction Result -----");
            }
            else /* (transResult != null) */
            {
                ShowMessageWorking($@"----- ----- Final Sale Transaction Result Received ----- -----");

                _log.LogText(_logChannel, _processId, new WithDataObj("-Sale Transaction Result Received; Card Info/Final Result~", transResult),
                    "A01", "IM30AccessSDK.OnTransactionFinishedDelgWorking");

                RunThreadMan tMan = new RunThreadMan(new Action(() =>
                {
                    try
                    {
                        if (transResult.IsSuccess)
                        {
                            ShowMessageWorking($@"Card Sale Success; Process Id : {_processId}");

                            if (TransactionCodeDef.IsEqualTrans(transResult.ResultData.TransactionCode, TransactionCodeDef.StartTransaction))
                            {
                                if (CardEntityDataTools.CheckCardType(transResult.ResultData, out bool isSuccessData) == CardTypeEn.CreditCard)
                                {
                                    CreditDebitCardInfoResp cdCardResp = new CreditDebitCardInfoResp(transResult.ResultData);
                                    SentTransactionResponseEvent(cdCardResp, TransactionTypeEn.CreditCard_2ndComm, TransEventCodeEn.CardInfoResponse);

                                    _log.LogText(_logChannel, _processId, $@"Card No.:{cdCardResp.CardNumber} Detected", "B10", "IM30AccessSDK.OnTransactionFinishedDelgWorking",
                                        adminMsg: $@"Card No.:{cdCardResp.CardNumber} Detected");
                                }
                            }
                            else if (TransactionCodeDef.IsEqualTrans(transResult.ResultData.TransactionCode, TransactionCodeDef.ChargeAmount))
                            {
                                ShowMessageWorking($@"Credit/Debit Card Final Result found; Process Id : {_processId}");

                                if (CardEntityDataTools.CheckCardType(transResult.ResultData, out bool isSuccessData) == CardTypeEn.CreditCard)
                                {
                                    CreditDebitChargeCardResp cardResp = new CreditDebitChargeCardResp(transResult.ResultData);
                                    if (cardResp.ResponseResult == ResponseCodeEn.Success)
                                    {
                                        _log.LogText(_logChannel, _processId, $@"Credit/Debit payment success", "B20", "IM30AccessSDK.OnTransactionFinishedDelgWorking",
                                            adminMsg: $@"Credit/Debit payment success");

                                        SentTransactionResponseEvent(cardResp, TransactionTypeEn.CreditCard_2ndComm, TransEventCodeEn.Success, "Paid Successful");
                                    }
                                    else
                                    {
                                        _log.LogText(_logChannel, _processId, $@"Credit/Debit payment failed", "B23", "IM30AccessSDK.OnTransactionFinishedDelgWorking",
                                            adminMsg: $@"Credit/Debit payment failed");

                                        SentTransactionResponseEvent(cardResp, TransactionTypeEn.CreditCard_2ndComm, TransEventCodeEn.FailWithError, "Fail payment");
                                    }
                                }
                                else
                                {
                                    SentTransactionResponseEvent(new CreditDebitChargeCardResp(new Exception("Invalid Credit/Debit card response-answer data")),
                                        TransactionTypeEn.CreditCard_2ndComm, TransEventCodeEn.FailWithError, "Invalid card");
                                }
                            }
                            //else if (TransactionCodeDef.IsEqualTrans(transResult.ResultData.TransactionCode, TransactionCodeDef.Entry))
                            //{
                            //    if (CardEntityDataTools.CheckCardType(transResult.ResultData, out bool isSuccessData) == CardTypeEn.TouchNGo)
                            //    {
                            //        TnGACGCheckinResp cardResp = new TnGACGCheckinResp(transResult.ResultData);
                            //        if (cardResp.ResponseResult == ResponseCodeEn.Success)
                            //        {
                            //            SentTransactionResponseEvent(cardResp, TransactionTypeEn.TouchNGo_2ndComm_CheckIn, TransEventCodeEn.Success);

                            //            ShowMessageWorking($@"TnG Card Check-in Final Result found; Process Id : {_processId}");
                            //        }
                            //        else
                            //        {
                            //            SentTransactionResponseEvent(cardResp, TransactionTypeEn.TouchNGo_2ndComm_CheckIn, TransEventCodeEn.FailWithError);
                            //        }
                            //    }
                            //    else
                            //    {
                            //        SentTransactionResponseEvent(new ErrorResponse(new Exception("Unrecognized response data")), _lastTransactionType, TransEventCodeEn.FailWithError);
                            //    }
                            //}
                            else if (TransactionCodeDef.IsEqualTrans(transResult.ResultData.TransactionCode, TransactionCodeDef.Settlement))
                            {
                                ShowMessageWorking($@"Settlement Final Result found; Process Id : {_processId}");
                                SettlementResp settResp = new SettlementResp(transResult.ResultData);
                                if (settResp.SettlementResult == SettlementStatusEn.Success)
                                {
                                    SentTransactionResponseEvent(settResp, TransactionTypeEn.Settlement, TransEventCodeEn.Success, "Settlement Done Successful");
                                }
                                else if (settResp.SettlementResult == SettlementStatusEn.Empty)
                                {
                                    SentTransactionResponseEvent(settResp, TransactionTypeEn.Settlement, TransEventCodeEn.EmptySettlement, "Empty Settlement");
                                }
                                else if (settResp.SettlementResult == SettlementStatusEn.PartiallyDone)
                                {
                                    SentTransactionResponseEvent(settResp, TransactionTypeEn.Settlement, TransEventCodeEn.PartialSettlement, "Partial Settlement");
                                }
                                else
                                {
                                    SentTransactionResponseEvent(settResp, TransactionTypeEn.Settlement, TransEventCodeEn.FailWithError, "Fail Settlement");
                                }
                            }
                            else if (TransactionCodeDef.IsEqualTrans(transResult.ResultData.TransactionCode, TransactionCodeDef.GetLastTransaction))
                            {
                                IM30FieldElementModel expTransCode = (from fd in transResult.ResultData.FieldElementCollection
                                                                      where FieldTypeDef.IsEqualType(fd.FieldTypeCode, FieldTypeDef.TransCodeOfLastTrans)
                                                                      select fd).FirstOrDefault();

                                if ((expTransCode is null) || string.IsNullOrWhiteSpace(expTransCode.Data))
                                {
                                    SentTransactionResponseEvent(new ErrorResponse(new Exception("Transaction Code for Get Last Transaction sale info has not found (R)")), _lastTransactionType, TransEventCodeEn.FailWithError);
                                }
                                else if (expTransCode.Data.Trim().ToUpper().Equals(TransactionCodeDef.ChargeAmount, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    SentTransactionResponseEvent(new CreditDebitChargeCardResp(transResult.ResultData),
                                        TransactionTypeEn.CreditCard_2ndComm, TransEventCodeEn.Success);
                                }
                                else
                                {
                                    SentTransactionResponseEvent(new ErrorResponse(new Exception($@"Transaction Code ({expTransCode.Data.Trim()}) not supported when Get Last Transaction for Card Sale Transaction")),
                                        _lastTransactionType, TransEventCodeEn.FailWithError);
                                }
                            }
                            else
                            {
                                SentTransactionResponseEvent(new ErrorResponse(new Exception("Unrecognized response data")), _lastTransactionType, TransEventCodeEn.FailWithError);
                            }
                        }
                        else if (transResult.IsTimeout)
                        {
                            SentTransactionResponseEvent(new ErrorResponse(new Exception("Transaction has timeout")), _lastTransactionType, TransEventCodeEn.Timeout);
                        }
                        else if (transResult.IsManualStopped)
                            SentTransactionResponseEvent(new ErrorResponse(new Exception("Transaction has stopped manually")), _lastTransactionType, TransEventCodeEn.ManualStop);

                        else
                        {
                            SentTransactionResponseEvent(new ErrorResponse(new Exception("Fail Transaction")), _lastTransactionType, TransEventCodeEn.FailWithError);
                        }
                    }
                    catch (Exception ex)
                    {
                        _log.LogError(_logChannel, _processId, ex, "EX11", "IM30AccessSDK.OnTransactionFinishedDelgWorking");
                    }

                    _log.LogText(_logChannel, _processId, "-End of Thread-", "G01", "IM30AccessSDK.OnTransactionFinishedDelgWorking:tMan Thread");

                    /*CYA-DEBUG .. }), _processId, lifeTimeSec: 400, _logChannel*/
                }), _processId, lifeTimeSec: 20, _logChannel
                , threadPriority: System.Threading.ThreadPriority.AboveNormal);
            }
        }

        private void SentTransactionResponseEvent(ICardResponse responseInfo, TransactionTypeEn transactionType, TransEventCodeEn transEventCode, string message = null)
        {
            try
            {
                bool isEventBlocked = false;
                if ((transEventCode == TransEventCodeEn.CardInfoResponse)
                    ||
                    (transEventCode == TransEventCodeEn.EmptySettlement)
                    ||
                    (transEventCode == TransEventCodeEn.PartialSettlement)
                )
                {
                    isEventBlocked = false;
                }
                else if (
                    (responseInfo is CreditDebitCardInfoResp)
                    ||
                    (responseInfo is CreditDebitChargeCardResp)
                    ||
                    (responseInfo is SettlementResp)
                    ||
                    (responseInfo is TnGCardInfoResp)
                )
                {
                    isEventBlocked = false;
                }
                else if (
                    (responseInfo is CreditDebitVoidTransactionResp)
                    ||
                    (responseInfo is GetDeviceInfoResp)
                    ||
                    (responseInfo is TnGACGCheckinResp)
                    ||
                    (responseInfo is TnGACGCheckoutResp)
                    ||
                    (responseInfo is GetLastTransactionResp)
                )
                {
                    isEventBlocked = true;
                }

                if (isEventBlocked == false)
                {
                    OnTransactionResponse?.Invoke(null, new CardTransResponseEventArgs(transactionType, transEventCode, responseInfo, message));
                }
            }
            catch (Exception ex)
            {
                _log.LogError(_logChannel, _processId, ex, "EX11", "IM30AccessSDK.SentTransactionResponseEvent");
            }
        }
    }
}
