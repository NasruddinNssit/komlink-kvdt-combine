using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.Base;
using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.CardEntityData;
using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.CardEntityData.CreditDebit;
using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.CardEntityData.KomLink;
using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.CardEntityData.TnG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Device.PAX.IM30.AccessSDK
{
    public interface IIM30AccessSDK
    {
        event EventHandler<CardTransResponseEventArgs> OnTransactionResponse;

        bool IsReaderStandingBy(out bool isDisposed, out bool isMalFunction);

        /// <summary>
        /// Start card transastion for Credit/Debit Card and KomLink Card.; CheckingDirection is Neutral/Counter; This method used for Counter and Kiosk
        /// </summary>
        /// <param name="firstSPNo">First Season Pass Number</param>
        /// <param name="secondSPNo">Second Season Pass Number</param>
        /// <param name="maxCardDetectedWaitingTimeSec">Default to 1 hours</param>
        /// <param name="maxSaleDecisionWaitingTimeSec">Default to 5 minutes</param>
        /// <param name="deviceSerialNo">Reader's serial number</param>
        /// <param name="error">Output exception when error with return false. Else output null</param>
        /// <returns>true: command issued success; false: fail to issue command</returns>
        bool StartCardTransaction(int firstSPNo, int secondSPNo,
            int maxCardDetectedWaitingTimeSec,
            int maxSaleDecisionWaitingTimeSec,
            out string deviceSerialNo, out Exception error);

        /// <summary>
        /// Start ACG Checkout card transastion for Credit/Debit Card and KomLink Card.; CheckingDirection is Checkout; This method used for ACG, Counter
        /// </summary>
        /// <param name="firstSPNo"></param>
        /// <param name="secondSPNo"></param>
        /// <param name="maxCardDetectedWaitingTimeSec">Default to 1 hours</param>
        /// <param name="maxSaleDecisionWaitingTimeSec">Default to 5 minutes</param>
        /// <param name="deviceSerialNo">Reader's serial number</param>
        /// <param name="error"></param>
        /// <returns></returns>
        bool StartACGCheckoutCardTrans(int firstSPNo, int secondSPNo,
            int maxCardDetectedWaitingTimeSec,
            int maxSaleDecisionWaitingTimeSec,
            out string deviceSerialNo, out Exception error);

        /// <summary></summary>
        /// <param name="error">When fail, output error exception. Else output null</param>
        /// <returns>true: command issued success; false: fail to issue command</returns>
        bool StopCardTransaction(out Exception error);

        /// <summary></summary>
        /// <param name="transDatetime">Mandatory</param>
        /// <param name="kvdtCardNo">Mandatory</param>
        /// <param name="cardExpireDate">Mandatory</param>
        /// <param name="responseResult">When success, output result. Else output null</param>
        /// <param name="error">When fail, output error exception. Else output null</param>
        /// <param name="pnrNo">Optional</param>
        /// <param name="cardType">Optional</param>
        /// <param name="cardTypeExpireDate">Optional</param>
        /// <param name="gender">Optional</param>
        /// <param name="isMalaysian">>Optional</param>
        /// <param name="dob">Optional</param>
        /// <param name="idType"></param>
        /// <param name="idNo">Optional</param>
        /// <param name="passengerName">Optional; Maximum 32 Characters</param>
        /// <param name="topUpValue">Optional; In RM currency amount; 0.00 for no value should be top-up</param>
        /// <param name="spNo">Optional; To issue a new card with Season Pass, this value must in the range of 1 to 8</param>
        /// <param name="spMaxTravelAmtPDayPTrip">Optional; In RM; 0.00 for not take into consideration during Checkout ACG; Max. RM 650.00</param>
        /// <param name="spStartDate">Optional; Season Pass Start Date</param>
        /// <param name="spEndDate">Optional; Season Pass End Date</param>
        /// <param name="spSaleDocNo">If spNo > 0 then this is mandatory. Else Optional</param>
        /// <param name="spServiceCode">If spNo > 0 then this is mandatory. Else Optional</param>
        /// <param name="spPackageCode">Optional</param>
        /// <param name="spType">Optional</param>
        /// <param name="spMaxTripCountPDay">Optional; 0 for not take into consideration during Checkout ACG</param>
        /// <param name="spIsAvoidChecking">Optional</param>
        /// <param name="spIsAvoidTripDurationCheck">Optional</param>
        /// <param name="spOriginStationNo"></param>
        /// <param name="spDestinationStationNo"></param>
        /// <returns>Return true for successful transaction; false for failure transaction result</returns>
        bool KomLinkIssueNewCard(DateTime transDatetime,
            ulong kvdtCardNo, DateTime cardExpireDate,
            out KomLinkIssueNewCardResp responseResult, out Exception error,
            string pnrNo = null, string cardType = null,
            DateTime? cardTypeExpireDate = null, GenderEn gender = GenderEn.NA, bool isMalaysian = false,
            DateTime? dob = null, IDTypeEn idType = IDTypeEn.NA, string idNo = null,
            string passengerName = null, decimal topUpValue = 0,
            byte spNo = 0, decimal spMaxTravelAmtPDayPTrip = 0.00M,
            DateTime? spStartDate = null, DateTime? spEndDate = null,
            string spSaleDocNo = null, string spServiceCode = null,
            string spPackageCode = null, string spType = null,
            byte spMaxTripCountPDay = 0, bool spIsAvoidChecking = false, bool spIsAvoidTripDurationCheck = false,
            string spOriginStationNo = null, string spDestinationStationNo = null);

        bool KomLinkReadSeasonPassInfo(out KomLinkReadSeasonPassInfoResp responseResult, out Exception error);

        /// <summary></summary>
        /// <param name="transDatetime"></param>
        /// <param name="gender"></param>
        /// <param name="pnrNo"></param>
        /// <param name="cardType"></param>
        /// <param name="cardTypeExpireDate"></param>
        /// <param name="isMalaysian"></param>
        /// <param name="dob"></param>
        /// <param name="idType"></param>
        /// <param name="idNo"></param>
        /// <param name="passengerName">Max. 32 Characters</param>
        /// <param name="responseResult">When success, output result. Else output null</param>
        /// <param name="error">When fail, output error exception. Else output null</param>
        /// <returns>Return true for successful transaction; false for failure transaction result</returns>
        bool KomLinkUpdateCardInfo(
            DateTime transDatetime,
            GenderEn gender, string pnrNo,
            string cardType, DateTime cardTypeExpireDate,
            bool isMalaysian, DateTime dob,
            IDTypeEn idType, string idNo,
            string passengerName,
            out KomLinkUpdateCardInfoResp responseResult, out Exception error);

        /// <summary></summary>
        /// <param name="topUpAmount"></param>
        /// <param name="transDatetime"></param>
        /// <param name="responseResult">When success, output result. Else output null</param>
        /// <param name="error">When fail, output error exception. Else output null</param>
        /// <returns>Return true for successful transaction; false for failure transaction result</returns>
        bool KomLinkIncreaseValue(
            decimal topUpAmount, DateTime transDatetime,
            out KomLinkIncreaseValueResp responseResult, out Exception error);

        /// <summary></summary>
        /// <param name="deductAmount"></param>
        /// <param name="transDatetime"></param>
        /// <param name="responseResult">When success, output result. Else output null</param>
        /// <param name="error">When fail, output error exception. Else output null</param>
        /// <returns>Return true for successful transaction; false for failure transaction result</returns>
        bool KomLinkDeductValue(
            decimal deductAmount, DateTime transDatetime,
            out KomLinkDeductValueResp responseResult, out Exception error);

        /// <summary></summary>
        /// <param name="responseResult">When success, output result. Else output null</param>
        /// <param name="error">When fail, output error exception. Else output null</param>
        /// <returns>Return true for successful transaction; false for failure transaction result</returns>
        bool KomLinkResetACGChecking(
            out KomLinkResetACGCheckingResp responseResult, out Exception error);

        /// <summary></summary>
        /// <param name="spNo">Mandatory; To issue a new card with Season Pass, this value must in the range of 1 to 8</param>
        /// <param name="transDatetime">Mandatory; </param>
        /// <param name="spStartDate">Mandatory; Season Pass Start Date </param>
        /// <param name="spEndDate">Mandatory; Season Pass End Date </param>
        /// <param name="spSaleDocNo">Mandatory; </param>
        /// <param name="spServiceCode">Mandatory; </param>
        /// <param name="responseResult">When success, output result. Else output null</param>
        /// <param name="error">When fail, output error exception. Else output null</param>
        /// <param name="spMaxTravelAmtPDayPTrip">Optional; In RM; 0.00 for not take into consideration during Checkout ACG; Max. RM 650.00</param>
        /// <param name="spPackageCode">Optional; </param>
        /// <param name="spType">Optional; </param>
        /// <param name="spMaxTripCountPDay">Optional; </param>
        /// <param name="spIsAvoidChecking">Optional; </param>
        /// <param name="spIsAvoidTripDurationCheck">Optional; </param>
        /// <param name="spOriginStationNo">Optional; </param>
        /// <param name="spDestinationStationNo">Optional; </param>
        /// <returns>Return true for successful transaction; false for failure transaction result</returns>
        bool KomLinkIssueSeasonPass(
            byte spNo, DateTime transDatetime,
            DateTime spStartDate, DateTime spEndDate, string spSaleDocNo, string spServiceCode,
            out KomLinkIssueSeasonPassResp responseResult, out Exception error,
            decimal spMaxTravelAmtPDayPTrip = 0.00M,
            string spPackageCode = null, string spType = null,
            byte spMaxTripCountPDay = 0, bool spIsAvoidChecking = false, bool spIsAvoidTripDurationCheck = false,
            string spOriginStationNo = null, string spDestinationStationNo = null);

        /// <summary></summary>
        /// <param name="blacklistStartDatetime">Mandatory; Set datetime to start blacklist</param>
        /// <param name="blacklistCode">Mandatory; Reason code for blacklist the card</param>
        /// <param name="responseResult">When success, output result. Else output null</param>
        /// <param name="error">When fail, output error exception. Else output null</param>
        /// <param name="triggerBlacklistCard">Optional; true to blacklist card immediately; Default is false</param>
        /// <param name="blacklistedDatetime">if triggerBlacklistCard is true then this parameter is mandatory. Else optional</param>
        /// <returns>Return true for successful transaction; false for failure transaction result</returns>
        bool KomLinkBlackListCard(
            DateTime blacklistStartDatetime, string blacklistCode,
            out KomLinkBlackListCardResp responseResult, out Exception error,
            bool triggerBlacklistCard = false, DateTime? blacklistedDatetime = null);

        /// <summary></summary>
        /// <param name="transDatetime"></param>
        /// <param name="responseResult">When success, output result. Else output null</param>
        /// <param name="error">When fail, output error exception. Else output null</param>
        /// <returns>Return true for successful transaction; false for failure transaction result</returns>
        bool KomLinkRemoveBlackListCard(DateTime transDatetime,
            out KomLinkRemoveBlackListCardResp responseResult, out Exception error);

        /// <summary></summary>
        /// <param name="responseResult">When success, output result. Else output null</param>
        /// <param name="error">When fail, output error exception. Else output null</param>
        /// <returns>Return true for successful transaction; false for failure transaction result</returns>
        bool KomLinkCancelCard(out KomLinkCancelCardResp responseResult, out Exception error);

        /// <summary></summary>
        /// <param name="transDatetime"></param>
        /// <param name="responseResult">When success, output result. Else output null</param>
        /// <param name="error">When fail, output error exception. Else output null</param>
        /// <returns>Return true for successful transaction; false for failure transaction result</returns>
        bool KomLinkRemoveCancelCard(DateTime transDatetime,
            out KomLinkRemoveCancelCardResp responseResult, out Exception error);

        /// <summary></summary>
        /// <param name="gateNo"></param>
        /// <param name="transDatetime"></param>
        /// <param name="stationNo"></param>
        /// <param name="spNo"></param>
        /// <param name="spLastTravelDate"></param>
        /// <param name="chargeAmount">0 for no charge. Or positive value to charge. Negative value is invalid</param>
        /// <param name="spDailyTravelTripCount"></param>
        /// <param name="responseResult">When success, output result. Else output null</param>
        /// <param name="error">When fail, output error exception. Else output null</param>
        /// <returns>Return true for successful transaction; false for failure transaction result</returns>
        bool KomLinkACGCheckout(string gateNo, DateTime transDatetime, string stationNo,
            byte spNo, DateTime spLastTravelDate, byte spDailyTravelTripCount, decimal chargeAmount,
            out KomLinkACGCheckoutResp responseResult, out Exception error);

        /// <summary></summary>
        /// <param name="posTransactionNo"></param>
        /// <param name="chargeAmount"></param>
        /// <param name="error">When fail, output error exception. Else output null</param>
        /// <returns>Return true for successful transaction; false for failure transaction result</returns>
        bool CreditDebitChargeCard(string posTransactionNo, decimal chargeAmount, out Exception error);

        //GetDeviceSerialNo()
        //PingDevice()

        /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        ///// Below Methods executed without any "Start Card Transaction Command".  
        bool VoidCreditCardTransaction(string invoiceNo, decimal voidAmount, string cardToken, out CreditDebitVoidTransactionResp responseResult, out Exception error);
        bool GetLastTransaction(out ICardResponse responseResult, out Exception error);
        bool SettleCreditDebitSales(out Exception error);
        /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        ///// Below Methods for ACG 

        /// <summary>
        /// Start ACG Checkin card transastion for Credit/Debit Card and KomLink Card.; CheckingDirection is Checkout; This method used for ACG
        /// </summary>
        /// <param name="firstSPNo"></param>
        /// <param name="secondSPNo"></param>
        /// <param name="maxCardDetectedWaitingTimeSec">Default to 1 hours</param>
        /// <param name="maxSaleDecisionWaitingTimeSec">Default to 5 minutes</param>
        /// <param name="deviceSerialNo">Reader's serial number</param>
        /// <param name="error"></param>
        /// <returns></returns>
        bool StartACGCheckinCardTrans(int firstSPNo, int secondSPNo,
            int maxCardDetectedWaitingTimeSec,
            int maxSaleDecisionWaitingTimeSec,
            out string deviceSerialNo, out Exception error);

        bool TnGACGCheckin(DateTime transDatetime, decimal penaltyChargeAmount,
            out TnGACGCheckinResp responseResult, out Exception error);

        bool TnGACGCheckout(DateTime transDatetime, decimal fareAmount, decimal penaltyChargeAmount,
            out TnGACGCheckoutResp responseResult, out Exception error);
        /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
    }
}