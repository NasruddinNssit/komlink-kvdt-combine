using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.IM30.Base
{
    public class PortProtocalDef
    {
        public static byte[] ACKData = new byte[] { 0x06 };
        public static byte[] NAKData = new byte[] { 0x15 };
    }

    public class AccountDataSourceDef
    {
        public static string MagStripeReadTrack1 => "H";
        public static string MagStripeReadTrack2 => "D";
        public static string ManualKeyEnteredTrack1 => "X";
        public static string ManualKeyEnteredTrack2 => "T";
        public static string UnableToReadData => "@";

        public static string GetSourceDesc(string source)
        {
            if (source is null)
                return "Unknown Source";

            string sourceX = source.Trim().ToUpper();

            if (sourceX.Equals(MagStripeReadTrack1, StringComparison.InvariantCultureIgnoreCase))
                return "MagStripeReadTrack1";

            else if (sourceX.Equals(MagStripeReadTrack2, StringComparison.InvariantCultureIgnoreCase))
                return "MagStripeReadTrack2";

            else if (sourceX.Equals(ManualKeyEnteredTrack1, StringComparison.InvariantCultureIgnoreCase))
                return "ManualKeyEnteredTrack1";

            else if (sourceX.Equals(ManualKeyEnteredTrack2, StringComparison.InvariantCultureIgnoreCase))
                return "ManualKeyEnteredTrack2";

            else if (sourceX.Equals(UnableToReadData, StringComparison.InvariantCultureIgnoreCase))
                return "UnableToReadData";

            else
                return "Unknown Source";
        }

        public static bool IsEqualSource(string source1, string source2)
        {
            if (string.IsNullOrWhiteSpace(source1) && string.IsNullOrWhiteSpace(source2))
                return true;

            else if ((string.IsNullOrWhiteSpace(source1) == false) && string.IsNullOrWhiteSpace(source2))
                return false;

            else if (string.IsNullOrWhiteSpace(source1) && (string.IsNullOrWhiteSpace(source2) == false))
                return false;

            else
                return source1.Trim().Equals(source2.Trim(), StringComparison.InvariantCultureIgnoreCase);
        }
    }

    public class DirectionDef
    {
        /// <summary>
        /// Counter / Kiosk
        /// </summary>
        public static string Counter => "0";
        public static string Entry => "1";
        public static string Exit => "2";

        public static string GetDirectionDesc(string direction)
        {
            if (direction is null)
                return "Unknown ACG direction";

            string sourceX = direction.Trim().ToUpper();

            if (sourceX.Equals(Counter, StringComparison.InvariantCultureIgnoreCase))
                return "Counter";

            else if (sourceX.Equals(Entry, StringComparison.InvariantCultureIgnoreCase))
                return "Entry";

            else if (sourceX.Equals(Exit, StringComparison.InvariantCultureIgnoreCase))
                return "Exit";

            else
                return $@"Unrecognized ACG direction ({direction})";
        }
    }

    public class BlacklistFlagDef
    {
        public static string NotBlacklisted => "0";
        public static string Blacklisted => "1";

        public static string GetBlacklistFlagDesc(string flag)
        {
            if (flag is null)
                return "Unknown blacklist flag";

            string sourceX = flag.Trim().ToUpper();

            if (sourceX.Equals(NotBlacklisted, StringComparison.InvariantCultureIgnoreCase))
                return "Not Blacklisted";

            else if (sourceX.Equals(Blacklisted, StringComparison.InvariantCultureIgnoreCase))
                return "Blacklisted";

            else
                return $@"Unrecognized Blacklist flag({flag})";
        }
    }

    public class YesNoDef
    {
        public static string No => "0";
        public static string Yes => "1";

        public static string GetBlacklistFlagDesc(string answer)
        {
            if (answer is null)
                return "Unknown answer (Y/N)";

            string sourceX = answer.Trim().ToUpper();

            if (sourceX.Equals(No, StringComparison.InvariantCultureIgnoreCase))
                return "No";

            else if (sourceX.Equals(Yes, StringComparison.InvariantCultureIgnoreCase))
                return "Yes";

            else
                return $@"Unrecognized answer ({answer})";
        }
    }

    public class KomLinkIdTypeDef
    {
        // I: IC; P: Passport; U: Unknown

        public static string IC => "I";
        public static string Passport => "P";
        public static string Unknown => "U";

        public static string GetBlacklistFlagDesc(string idType)
        {
            if (idType is null)
                return "Unknown KomLink ID Type";

            string sourceX = idType.Trim().ToUpper();

            if (sourceX.Equals(IC, StringComparison.InvariantCultureIgnoreCase))
                return "IC";

            else if (sourceX.Equals(Passport, StringComparison.InvariantCultureIgnoreCase))
                return "Passport";

            else if (sourceX.Equals(Unknown, StringComparison.InvariantCultureIgnoreCase))
                return "KomLink ID type not exist";

            else
                return $@"Unrecognized KomLink ID Type({idType})";
        }
    }

    public class CardVerificationMethodDef
    {
        public static string Signature => "0";
        public static string Pin => "1";
        public static string NoPinNoSignature => "2";
        public static string CDCVM => "3";

        public static string GetMethodDesc(string method)
        {
            if (method is null)
                return "Unknown CVM Method";

            string sourceX = method.Trim().ToUpper();

            if (sourceX.Equals(Signature, StringComparison.InvariantCultureIgnoreCase))
                return "Signature";

            else if (sourceX.Equals(Pin, StringComparison.InvariantCultureIgnoreCase))
                return "Pin";

            else if (sourceX.Equals(NoPinNoSignature, StringComparison.InvariantCultureIgnoreCase))
                return "NoPinNoSignature";

            else if (sourceX.Equals(CDCVM, StringComparison.InvariantCultureIgnoreCase))
                return "CDCVM";

            else
                return "Unknown CVM Method";
        }
    }

    public class PaymentIndicatorDef
    {
        public static string Unknown => "0";
        public static string ContactCard_ICC => "1";
        public static string ContactlessCard => "2";

        public static string GetIndicatorDesc(string indicator)
        {
            if (indicator is null)
                return "Unknown Payment Indicator";

            string indicatorX = indicator.Trim().ToUpper();

            if (indicatorX.Equals(Unknown, StringComparison.InvariantCultureIgnoreCase))
                return "Unknown";

            else if (indicatorX.Equals(ContactCard_ICC, StringComparison.InvariantCultureIgnoreCase))
                return "ContactCard_ICC";

            else if (indicatorX.Equals(ContactlessCard, StringComparison.InvariantCultureIgnoreCase))
                return "ContactlessCard";

            else
                return "Unknown Payment Indicator";
        }
    }

    public class TerminalModeDef
    {
        public static string ProductionMode => "0";
        public static string OperationMode => "1";

        public static string GetModeDesc(string mode)
        {
            if (mode is null)
                return "Unknown Terminal Mode";

            string indicatorX = mode.Trim().ToUpper();

            if (indicatorX.Equals(ProductionMode, StringComparison.InvariantCultureIgnoreCase))
                return "ProductionMode";

            else if (indicatorX.Equals(OperationMode, StringComparison.InvariantCultureIgnoreCase))
                return "OperationMode";

            else
                return "Unknown Terminal Mode";
        }
    }

    public class IssuerDef
    {
        public static string VISA => "01";
        public static string VISA_06_INSTALLMENT => "02";
        public static string VISA_12_INSTALLMENT => "03";
        public static string VISA_18_INSTALLMENT => "04";
        public static string VISA_24_INSTALLMENT => "05";
        public static string VISA_36_INSTALLMENT => "06";
        public static string VISA_48_INSTALLMENT => "07";
        public static string MASTERCARD => "08";
        public static string MASTERCARD_06_INSTALLMENT => "09";
        public static string MASTERCARD_12_INSTALLMENT => "10";
        public static string MASTERCARD_18_INSTALLMENT => "11";
        public static string MASTERCARD_24_INSTALLMENT => "12";
        public static string MASTERCARD_36_INSTALLMENT => "13";
        public static string MASTERCARD_48_INSTALLMENT => "14";
        public static string MYDEBIT => "15";
        public static string AMEX => "16";
        public static string JCB => "17";
        public static string DINERS => "18";
        public static string UNIONPAY => "19";
        public static string NETS => "20";
        public static string ASP => "80";
        public static string WechatChina => "81";
        public static string UPIQR => "82";
        public static string TNG => "83";
        public static string BST => "84";
        public static string VCS => "85";
        public static string GRB => "86";
        public static string WechatMalaysia => "87";
        public static string SHP => "88";
        public static string BINF => "89";
        public static string DN => "90";
        public static string WNP => "91";
        public static string SPAY => "92 ";
        public static string AlipayPlus => "93";

        public static string GetIssuerDesc(string issuerId)
        {
            if (issuerId is null)
                return "Unknown Issuer";

            string issuerIdX = issuerId.Trim().ToUpper();

            if (issuerIdX.Equals(VISA, StringComparison.InvariantCultureIgnoreCase))
                return "VISA";

            else if (issuerIdX.Equals(VISA_06_INSTALLMENT, StringComparison.InvariantCultureIgnoreCase))
                return "VISA_06_INSTALLMENT";

            else if (issuerIdX.Equals(VISA_12_INSTALLMENT, StringComparison.InvariantCultureIgnoreCase))
                return "VISA_12_INSTALLMENT";

            else if (issuerIdX.Equals(VISA_18_INSTALLMENT, StringComparison.InvariantCultureIgnoreCase))
                return "VISA_18_INSTALLMENT";

            else if (issuerIdX.Equals(VISA_24_INSTALLMENT, StringComparison.InvariantCultureIgnoreCase))
                return "VISA_24_INSTALLMENT";

            else if (issuerIdX.Equals(VISA_36_INSTALLMENT, StringComparison.InvariantCultureIgnoreCase))
                return "VISA_36_INSTALLMENT";

            else if (issuerIdX.Equals(VISA_48_INSTALLMENT, StringComparison.InvariantCultureIgnoreCase))
                return "VISA_48_INSTALLMENT";

            else if (issuerIdX.Equals(MASTERCARD, StringComparison.InvariantCultureIgnoreCase))
                return "MASTERCARD";

            else if (issuerIdX.Equals(MASTERCARD_06_INSTALLMENT, StringComparison.InvariantCultureIgnoreCase))
                return "MASTERCARD_06_INSTALLMENT";

            else if (issuerIdX.Equals(MASTERCARD_12_INSTALLMENT, StringComparison.InvariantCultureIgnoreCase))
                return "MASTERCARD_12_INSTALLMENT";

            else if (issuerIdX.Equals(MASTERCARD_18_INSTALLMENT, StringComparison.InvariantCultureIgnoreCase))
                return "MASTERCARD_18_INSTALLMENT";

            else if (issuerIdX.Equals(MASTERCARD_24_INSTALLMENT, StringComparison.InvariantCultureIgnoreCase))
                return "MASTERCARD_24_INSTALLMENT";

            else if (issuerIdX.Equals(MASTERCARD_36_INSTALLMENT, StringComparison.InvariantCultureIgnoreCase))
                return "MASTERCARD_36_INSTALLMENT";

            else if (issuerIdX.Equals(MASTERCARD_48_INSTALLMENT, StringComparison.InvariantCultureIgnoreCase))
                return "MASTERCARD_48_INSTALLMENT";

            else if (issuerIdX.Equals(MYDEBIT, StringComparison.InvariantCultureIgnoreCase))
                return "MYDEBIT";

            else if (issuerIdX.Equals(AMEX, StringComparison.InvariantCultureIgnoreCase))
                return "AMEX";

            else if (issuerIdX.Equals(JCB, StringComparison.InvariantCultureIgnoreCase))
                return "JCB";

            else if (issuerIdX.Equals(DINERS, StringComparison.InvariantCultureIgnoreCase))
                return "DINERS";

            else if (issuerIdX.Equals(UNIONPAY, StringComparison.InvariantCultureIgnoreCase))
                return "UNIONPAY";

            else if (issuerIdX.Equals(NETS, StringComparison.InvariantCultureIgnoreCase))
                return "NETS";

            else if (issuerIdX.Equals(ASP, StringComparison.InvariantCultureIgnoreCase))
                return "ASP";

            else if (issuerIdX.Equals(WechatChina, StringComparison.InvariantCultureIgnoreCase))
                return "WechatChina";

            else if (issuerIdX.Equals(UPIQR, StringComparison.InvariantCultureIgnoreCase))
                return "UPIQR";

            else if (issuerIdX.Equals(TNG, StringComparison.InvariantCultureIgnoreCase))
                return "TNG";

            else if (issuerIdX.Equals(BST, StringComparison.InvariantCultureIgnoreCase))
                return "BST";

            else if (issuerIdX.Equals(VCS, StringComparison.InvariantCultureIgnoreCase))
                return "VCS";

            else if (issuerIdX.Equals(GRB, StringComparison.InvariantCultureIgnoreCase))
                return "GRB";

            else if (issuerIdX.Equals(WechatMalaysia, StringComparison.InvariantCultureIgnoreCase))
                return "WechatMalaysia";

            else if (issuerIdX.Equals(SHP, StringComparison.InvariantCultureIgnoreCase))
                return "SHP";

            else if (issuerIdX.Equals(BINF, StringComparison.InvariantCultureIgnoreCase))
                return "BINF";

            else if (issuerIdX.Equals(DN, StringComparison.InvariantCultureIgnoreCase))
                return "DN";

            else if (issuerIdX.Equals(WNP, StringComparison.InvariantCultureIgnoreCase))
                return "WNP";

            else if (issuerIdX.Equals(SPAY, StringComparison.InvariantCultureIgnoreCase))
                return "SPAY";

            else if (issuerIdX.Equals(AlipayPlus, StringComparison.InvariantCultureIgnoreCase))
                return "AlipayPlus";

            else
                return "Unknown Issuer";
        }

        public static bool IsEqualIssuer(string issuerId1, string issuerId2)
        {
            if (string.IsNullOrWhiteSpace(issuerId1) && string.IsNullOrWhiteSpace(issuerId2))
                return true;

            else if ((string.IsNullOrWhiteSpace(issuerId1) == false) && string.IsNullOrWhiteSpace(issuerId2))
                return false;

            else if (string.IsNullOrWhiteSpace(issuerId1) && (string.IsNullOrWhiteSpace(issuerId2) == false))
                return false;

            else
                return issuerId1.Trim().Equals(issuerId2.Trim(), StringComparison.InvariantCultureIgnoreCase);
        }
    }

    public class StatusCodeDef
    {
        public static string IDLE => "00";
        public static string ENTER_AMOUNT => "01";
        public static string PRESENT_FOR_PAYMENT => "02";
        public static string ENTER_PIN => "03";
        public static string WAIT_HOST_RESPONSE => "04";
        public static string DISPLAY_FINAL_TXN_RESULT => "05";
        public static string PRINT => "06";
        public static string REMOVE_CARD => "07";
        public static string UNKNOWN => "99";

        public static string GetIssuerDesc(string issuerId)
        {
            if (issuerId is null)
                return "Unknown Status Code";

            string codeX = issuerId.Trim().ToUpper();

            if (codeX.Equals(IDLE, StringComparison.InvariantCultureIgnoreCase))
                return "IDLE";

            else if (codeX.Equals(ENTER_AMOUNT, StringComparison.InvariantCultureIgnoreCase))
                return "ENTER_AMOUNT";

            else if (codeX.Equals(PRESENT_FOR_PAYMENT, StringComparison.InvariantCultureIgnoreCase))
                return "PRESENT_FOR_PAYMENT";

            else if (codeX.Equals(ENTER_PIN, StringComparison.InvariantCultureIgnoreCase))
                return "ENTER_PIN";

            else if (codeX.Equals(WAIT_HOST_RESPONSE, StringComparison.InvariantCultureIgnoreCase))
                return "WAIT_HOST_RESPONSE";

            else if (codeX.Equals(DISPLAY_FINAL_TXN_RESULT, StringComparison.InvariantCultureIgnoreCase))
                return "DISPLAY_FINAL_TXN_RESULT";

            else if (codeX.Equals(PRINT, StringComparison.InvariantCultureIgnoreCase))
                return "PRINT";

            else if (codeX.Equals(REMOVE_CARD, StringComparison.InvariantCultureIgnoreCase))
                return "REMOVE_CARD";

            else if (codeX.Equals(UNKNOWN, StringComparison.InvariantCultureIgnoreCase))
                return "UNKNOWN";

            else
                return "Unknown Status Code";
        }

        public static bool IsEqualCode(string code1, string code2)
        {
            if (string.IsNullOrWhiteSpace(code1) && string.IsNullOrWhiteSpace(code2))
                return true;

            else if ((string.IsNullOrWhiteSpace(code1) == false) && string.IsNullOrWhiteSpace(code2))
                return false;

            else if (string.IsNullOrWhiteSpace(code1) && (string.IsNullOrWhiteSpace(code2) == false))
                return false;

            else
                return code1.Trim().Equals(code2.Trim(), StringComparison.InvariantCultureIgnoreCase);
        }
    }

}
