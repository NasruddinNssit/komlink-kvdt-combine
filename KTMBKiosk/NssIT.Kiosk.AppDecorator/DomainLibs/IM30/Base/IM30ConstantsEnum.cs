using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.IM30.Base
{
    public enum ASCIICodeEn
    {
        STX = 0x02,
        ETX = 0x03,
        EOT = 0x04,
        ENQ = 0x04,
        ACK = 0x06,
        NAK = 0x15,
    }

    public enum GateDirectionEn
    {
        Counter = 0,
        Entry = 1,
        Exit = 2
    }

    public enum FieldTypeAttributeEn
    {
        /// <summary>
        /// Alpha, Numeric and Special characters
        /// </summary>
        ANS,

        /// <summary>
        /// Alpha and Numeric characters
        /// </summary>
        AN,

        /// <summary>
        /// Numeric characters
        /// </summary>
        N,

        /// <summary>
        /// Numeric characters for Date YYMMDD
        /// </summary>
        N_D,

        /// <summary>
        /// Numeric characters for DateTime YYMMDDHHmmss
        /// </summary>
        N_DT,

        /// <summary>
        /// Numeric characters for Date String YYMMDD; Normally data combine with N_TS (Time String) to form a DateTime
        /// </summary>
        N_DS,

        /// <summary>
        /// Numeric characters for Time String HHmmss; Normally data combine with N_DS (Time String) to form a DateTime
        /// </summary>
        N_TS,

        /// <summary>
        /// Numeric characters for a Year-month YYMM
        /// </summary>
        N_M,

        /// <summary>
        /// Numeric Data plus '=' character
        /// </summary>
        Z,

        /// <summary>
        /// Price/Cost Amount / Money
        /// </summary>
        M,

        /// <summary>
        /// Binary Data
        /// </summary>
        B,

        /// <summary>
        /// Telephone Number Data
        /// </summary>
        T
    }

    public enum TransactionTypeEn
    {
        Unknown = 0,

        CreditCard_2ndComm = 1,

        KomLinkCard_2ndComm_CheckIn = 2,
        TouchNGo_2ndComm_CheckIn = 3,

        KomLinkCard_2ndComm_Checkout = 4,
        TouchNGo_2ndComm_Checkout = 5,
        
        StartTrans_1stComm = 10,

        CreditCardInfo = 11,
        KomLinkCardInfo = 12,
        TouchNGoInfo = 13,

        Settlement = 15,
        GetLastTransaction = 16,
        VoidCreditCardTrans = 17,

        StopTrans = 20,
        System = 100
    }

    public enum CardTypeEn
    {
        Unknown = 0,
        CreditCard = 1,
        KomLinkCard = 2,
        TouchNGo = 3,
   }

    public enum GenderEn
    {
        NA = 0, /* Not Applicable*/
        Male = 1,
        Female = 2
    }

    public enum IDTypeEn
    {
        /// <summary>
        /// Not applicable
        /// </summary>
        NA = 0,
        /// <summary>
        /// Malaysian Identity Card
        /// </summary>
        IC = 1,
        /// <summary>
        /// Passport Number
        /// </summary>
        PassportNo = 2
    }

    public enum CheckingDirectionEn
    {
        NA = 0,
        Checkin,
        Checkout
    }

    public enum ResponseCodeEn
    {
        Fail = 0,
        Success = 1,
        AcquireCustomerAction = 10
    }

    public enum SettlementStatusEn
    {
        Fail = 0,
        Success = 1,
        PartiallyDone = 2,
        Empty = 20
    }

    public enum BatchSettlementStatusEn
    {
        Fail = 0,
        Success = 1
    }

    public enum PaymentIndicatorEn
    {
        Unknown = 0,
        ContactCard = 1,
        ContactlessCard = 2
    }

    public enum TransEventCodeEn
    {
        Unknown = 0,

        ///// Success Group ------------------------------------------------------------------------------------

        /// <summary>
        /// Should send only one time for one card transaction
        /// </summary>
        Success = 1,

        ManualStop = 3,

        Timeout = 5,

        /// <summary>
        /// For Settlement
        /// </summary>
        EmptySettlement = 6,
        /// <summary>
        /// For Settlement
        /// </summary>
        PartialSettlement = 7,

        ///// Messaging / Action Request Group -----------------------------------------------------------------
        CardInfoResponse = 10,

        /// <summary>
        /// May exist in credit/Debit card transaction
        /// </summary>
        Message = 80,

        /// <summary>
        /// Like .. request PIN numbers for credit card transaction; May exist in credit/Debit card transaction
        /// </summary>
        AcquireCustomerAction = 81,

        /// <summary>
        /// Exist in credit/Debit card transaction
        /// </summary>
        Countdown = 100,

        ///// Fail / Error Group -----------------------------------------------------------------------------

        ErrorFound = 990,

        /// <summary>
        /// Should send only one time for one card transaction
        /// </summary>
        FailWithError = 999,

        ///// ------------------------------------------------------------------------------------------------
    }

    public enum TransactionCodeEn
    {
        Unknown = 0,
        StartTransaction,
        StopTransaction,
        ACGEntry,
        ACGExit,
        KomLinkIssueNewCard,
        KomLinkIssueSeasonPass,
        KomLinkReadSeasonPass,
        KomLinkUpdateCardInfo,
        KomLinkCancelCard,
        KomLinkIncreaseValue,
        KomLinkDeductValue,
        KomLinkResetACGChecking,
        KomLinkBlacklistCard,
        ChargeAmount,
        Void,
        Settlement,
        GetLastTransaction,
        Maintenance,
        Reboot,
        Ping,
        ReadDeviceSerialNumber
    }
}
