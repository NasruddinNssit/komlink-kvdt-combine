using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.IM30.Base
{
    public class RequestResponseIndicatorDef
    {
        public static string RequestAndResponse => "0";
        public static string ResponseOnly => "1";
        public static string RequestOnly => "2";

        public static string GetIndicatorDesc(string indicatorCode)
        {
            if (indicatorCode is null)
                return "Unknown Indicator";

            string codeX = indicatorCode.Trim().ToUpper();

            if (codeX.Equals(RequestAndResponse, StringComparison.InvariantCultureIgnoreCase))
                return "RequestAndResponse";

            else if (codeX.Equals(ResponseOnly, StringComparison.InvariantCultureIgnoreCase))
                return "ResponseOnly";

            else if (codeX.Equals(RequestOnly, StringComparison.InvariantCultureIgnoreCase))
                return "RequestOnly";

            else
                return "Unknown Indicator";
        }

        public static bool IsEqualIndicator(string indicator1, string indicator2)
        {
            if (string.IsNullOrWhiteSpace(indicator1) && string.IsNullOrWhiteSpace(indicator2))
                return true;

            else if ((string.IsNullOrWhiteSpace(indicator1) == false) && string.IsNullOrWhiteSpace(indicator2))
                return false;

            else if (string.IsNullOrWhiteSpace(indicator1) && (string.IsNullOrWhiteSpace(indicator2) == false))
                return false;

            else
                return indicator1.Trim().Equals(indicator2.Trim(), StringComparison.InvariantCultureIgnoreCase);
        }
    }

    public class MoreIndicatorDef
    {
        public static string LastMessage => "0";
        public static string AnotherMessageOnFollowing => "1";

        public static string GetIndicatorDesc(string indicatorCode)
        {
            if (indicatorCode is null)
                return "Unknown Indicator";

            string codeX = indicatorCode.Trim().ToUpper();

            if (codeX.Equals(LastMessage, StringComparison.InvariantCultureIgnoreCase))
                return "LastMessage";

            else if (codeX.Equals(AnotherMessageOnFollowing, StringComparison.InvariantCultureIgnoreCase))
                return "AnotherMessageOnFollowing";

            else
                return "Unknown Indicator";
        }

        public static bool IsEqualIndicator(string indicator1, string indicator2)
        {
            if (string.IsNullOrWhiteSpace(indicator1) && string.IsNullOrWhiteSpace(indicator2))
                return true;

            else if ((string.IsNullOrWhiteSpace(indicator1) == false) && string.IsNullOrWhiteSpace(indicator2))
                return false;

            else if (string.IsNullOrWhiteSpace(indicator1) && (string.IsNullOrWhiteSpace(indicator2) == false))
                return false;

            else
                return indicator1.Trim().Equals(indicator2.Trim(), StringComparison.InvariantCultureIgnoreCase);
        }

    }

    public class TransactionCodeDef
    {
        public static string Void => "26";
        public static string Settlement => "50";
        public static string Ping => "94";
        public static string Entry => "E1";
        public static string Exit => "E2";
        public static string KomIssueNewCard => "K1";
        public static string KomIssueSeasonPass => "K2";
        public static string KomReadSeasonPass => "K3";
        public static string KomUpdateCardInfo => "K4";
        public static string KomCancelCard => "K5";
        public static string KomIncreaseValue => "K6";
        public static string KomDeductValue => "K7";
        public static string KomResetACGChecking => "K8";
        public static string KomBlacklistCard => "K9";
        public static string StartTransaction => "S1";
        public static string StopTransaction => "S2";
        public static string GetLastTransaction => "Q1";

        /// <summary>
        /// For Credit & Debit card only
        /// </summary>
        public static string ChargeAmount => "C1";
        public static string Maintenance => "M1";
        public static string Reboot => "R1";
        public static string GetDeviceInfo => "Q3";

        public static string GetCodeDesc(string code)
        {
            if (code is null)
                return "Unknown Code";

            string codeX = code.Trim().ToUpper();
            
            if (codeX.Equals(Void, StringComparison.InvariantCultureIgnoreCase))
                return "Void";

            else if (codeX.Equals(Settlement, StringComparison.InvariantCultureIgnoreCase))
                return "Settlement";

            else if (codeX.Equals(Ping, StringComparison.InvariantCultureIgnoreCase))
                return "Ping";

            else if (codeX.Equals(Entry, StringComparison.InvariantCultureIgnoreCase))
                return "Entry";

            else if (codeX.Equals(Exit, StringComparison.InvariantCultureIgnoreCase))
                return "Exit";

            else if (codeX.Equals(KomIssueNewCard, StringComparison.InvariantCultureIgnoreCase))
                return "KomIssueNewCard";

            else if (codeX.Equals(KomIssueSeasonPass, StringComparison.InvariantCultureIgnoreCase))
                return "KomIssueSeasonPass";

            else if (codeX.Equals(KomReadSeasonPass, StringComparison.InvariantCultureIgnoreCase))
                return "KomReadSeasonPass";

            else if (codeX.Equals(KomUpdateCardInfo, StringComparison.InvariantCultureIgnoreCase))
                return "KomUpdateCardInfo";

            else if (codeX.Equals(KomCancelCard, StringComparison.InvariantCultureIgnoreCase))
                return "KomCancelCard";

            else if (codeX.Equals(KomIncreaseValue, StringComparison.InvariantCultureIgnoreCase))
                return "KomIncreaseValue";

            else if (codeX.Equals(KomDeductValue, StringComparison.InvariantCultureIgnoreCase))
                return "KomDeductValue";

            else if (codeX.Equals(KomResetACGChecking, StringComparison.InvariantCultureIgnoreCase))
                return "KomResetACGChecking";

            else if (codeX.Equals(KomBlacklistCard, StringComparison.InvariantCultureIgnoreCase))
                return "KomBlacklistCard";

            else if (codeX.Equals(StartTransaction, StringComparison.InvariantCultureIgnoreCase))
                return "StartTransaction";

            else if (codeX.Equals(StopTransaction, StringComparison.InvariantCultureIgnoreCase))
                return "StopTransaction";

            else if (codeX.Equals(GetLastTransaction, StringComparison.InvariantCultureIgnoreCase))
                return "GetLastTransaction";

            else if (codeX.Equals(ChargeAmount, StringComparison.InvariantCultureIgnoreCase))
                return "ChargeAmount";

            else if (codeX.Equals(Maintenance, StringComparison.InvariantCultureIgnoreCase))
                return "Maintenance";

            else if (codeX.Equals(Reboot, StringComparison.InvariantCultureIgnoreCase))
                return "Reboot";

            else if (codeX.Equals(GetDeviceInfo, StringComparison.InvariantCultureIgnoreCase))
                return "GetDeviceInfo";

            else 
                return "Unknown Code";
        }

        public static bool IsEqualTrans(string code1, string code2)
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

    public class ResponseCodeDef
    {
        public static string Approved => "00";
        public static string VoiceReferral => "01";
        public static string REFERRAL => "02";
        public static string ErrorCallHelp_SN => "03";
        public static string PickUpCard => "04";
        public static string DoNotHonour => "05";
        public static string ErrorCallHelp_TR => "12";
        public static string ErrorCallHelp_AM => "13";
        public static string ErrorCallHelp_RE => "14";
        public static string ReEnterTransaction => "19";
        public static string NoTransactions => "21";
        public static string ErrorCallHelp_NT => "25";
        public static string ErrorCallHelp_FE => "30";
        public static string SaleCallHelp_NS => "31";
        public static string PleaseCall_LC => "41";
        public static string PleaseCall_SC => "43";
        public static string Decline => "51";
        public static string NoChequeAcc => "52";
        public static string NoSavingsAcc => "53";
        public static string ExpiredCard => "54";
        public static string IncorrectPIN => "55";
        public static string NoCardRecord => "56";
        public static string InvalidTransaction => "58";
        public static string ExceedsLimit => "61";
        public static string SecurityViolation => "63";
        public static string PinTriedExceed => "75";
        public static string ErrorCallHelp_DC => "76";
        public static string ReconcileError => "77";
        public static string TransactionNotFound => "78";
        public static string BatchAlreadyOpen => "79";
        public static string BadBatchNumber => "80";
        public static string BatchNotFound => "85";
        public static string BadTerminalID => "89";
        public static string ErrorCallHelp_NA => "91";
        public static string PleaseTryAgain_SQ => "94";
        public static string BatchTransferError => "95";
        public static string ErrorCallHelp_SE => "96";
        public static string Declined => "ND";
        public static string DestinationError => "ED";
        public static string NetworkRequestError => "EN";
        public static string TimeoutError => "TO";
        public static string TransactionNotAvailable => "NA";
        public static string LostCarrier => "LC";
        public static string CommunicationError => "CE";
        public static string ApprovedOffline_1 => "Y1";
        public static string ApprovedOffline_3 => "Y3";
        public static string ForceSettlement => "FS";



        public static string ReadCardError => "RE";
        public static string CardNotDetected => "NC";
        public static string FailedToUpdateCard => "WF";



        public static string GetResponseDesc(string responseCode, out string extMsg)
        {
            extMsg = null;

            if (responseCode is null)
                return "Unknown Response Code";

            string respX = responseCode.Trim().ToUpper();

            if (respX.Equals(Approved, StringComparison.InvariantCultureIgnoreCase))
                return "Approved";

            else if (respX.Equals(VoiceReferral, StringComparison.InvariantCultureIgnoreCase))
                return "VoiceReferral";

            else if (respX.Equals(REFERRAL, StringComparison.InvariantCultureIgnoreCase))
                return "REFERRAL";

            else if (respX.Equals(ErrorCallHelp_SN, StringComparison.InvariantCultureIgnoreCase))
                return "ErrorCallHelp_SN";

            else if (respX.Equals(PickUpCard, StringComparison.InvariantCultureIgnoreCase))
                return "PickUpCard";

            else if (respX.Equals(DoNotHonour, StringComparison.InvariantCultureIgnoreCase))
                return "DoNotHonour";

            else if (respX.Equals(ErrorCallHelp_TR, StringComparison.InvariantCultureIgnoreCase))
            {
                extMsg = "Invalid Transaction";
                return "ErrorCallHelp_TR";
            }
            else if (respX.Equals(ErrorCallHelp_AM, StringComparison.InvariantCultureIgnoreCase))
            {
                extMsg = "Invalid Amount";
                return "ErrorCallHelp_AM";
            }
            else if (respX.Equals(ErrorCallHelp_RE, StringComparison.InvariantCultureIgnoreCase))
            {
                extMsg = "Invalid Card Reader";
                return "ErrorCallHelp_RE";
            }
            else if (respX.Equals(ReEnterTransaction, StringComparison.InvariantCultureIgnoreCase))
                return "ReEnterTransaction";

            else if (respX.Equals(NoTransactions, StringComparison.InvariantCultureIgnoreCase))
                return "NoTransactions";

            else if (respX.Equals(ErrorCallHelp_NT, StringComparison.InvariantCultureIgnoreCase))
            {
                extMsg = "Unable to locate record on file";
                return "ErrorCallHelp_NT";
            }
            else if (respX.Equals(ErrorCallHelp_FE, StringComparison.InvariantCultureIgnoreCase))
            {
                extMsg = "Format Error";
                return "ErrorCallHelp_FE";
            }
            else if (respX.Equals(SaleCallHelp_NS, StringComparison.InvariantCultureIgnoreCase))
            {
                extMsg = "Bank not supported switch";
                return "SaleCallHelp_NS";
            }
            else if (respX.Equals(PleaseCall_LC, StringComparison.InvariantCultureIgnoreCase))
                return "PleaseCall_LC";

            else if (respX.Equals(PleaseCall_SC, StringComparison.InvariantCultureIgnoreCase))
                return "PleaseCall_SC";

            else if (respX.Equals(Decline, StringComparison.InvariantCultureIgnoreCase))
                return "Decline";

            else if (respX.Equals(NoChequeAcc, StringComparison.InvariantCultureIgnoreCase))
                return "NoChequeAcc";

            else if (respX.Equals(NoSavingsAcc, StringComparison.InvariantCultureIgnoreCase))
                return "NoSavingsAcc";

            else if (respX.Equals(ExpiredCard, StringComparison.InvariantCultureIgnoreCase))
                return "ExpiredCard";

            else if (respX.Equals(IncorrectPIN, StringComparison.InvariantCultureIgnoreCase))
                return "IncorrectPIN";

            else if (respX.Equals(NoCardRecord, StringComparison.InvariantCultureIgnoreCase))
                return "NoCardRecord";

            else if (respX.Equals(InvalidTransaction, StringComparison.InvariantCultureIgnoreCase))
            {
                extMsg = "Transaction not permitted to terminal";
                return "InvalidTransaction";
            }
            else if (respX.Equals(ExceedsLimit, StringComparison.InvariantCultureIgnoreCase))
                return "ExceedsLimit";

            else if (respX.Equals(SecurityViolation, StringComparison.InvariantCultureIgnoreCase))
                return "SecurityViolation";

            else if (respX.Equals(PinTriedExceed, StringComparison.InvariantCultureIgnoreCase))
                return "PinTriedExceed";

            else if (respX.Equals(ErrorCallHelp_DC, StringComparison.InvariantCultureIgnoreCase))
            {
                extMsg = "Invalid Product Codes";
                return "ErrorCallHelp_DC";
            }
            else if (respX.Equals(ReconcileError, StringComparison.InvariantCultureIgnoreCase))
                return "ReconcileError";

            else if (respX.Equals(TransactionNotFound, StringComparison.InvariantCultureIgnoreCase))
                return "TransactionNotFound";

            else if (respX.Equals(BatchAlreadyOpen, StringComparison.InvariantCultureIgnoreCase))
                return "BatchAlreadyOpen";

            else if (respX.Equals(BadBatchNumber, StringComparison.InvariantCultureIgnoreCase))
                return "BadBatchNumber";

            else if (respX.Equals(BatchNotFound, StringComparison.InvariantCultureIgnoreCase))
                return "BatchNotFound";

            else if (respX.Equals(BadTerminalID, StringComparison.InvariantCultureIgnoreCase))
                return "BadTerminalID";

            else if (respX.Equals(ErrorCallHelp_NA, StringComparison.InvariantCultureIgnoreCase))
            {
                extMsg = "Issuer or switch inoperative";
                return "ErrorCallHelp_NA";
            }
            else if (respX.Equals(PleaseTryAgain_SQ, StringComparison.InvariantCultureIgnoreCase))
            {
                extMsg = "Duplicate transmission";
                return "PleaseTryAgain_SQ";
            }
            else if (respX.Equals(BatchTransferError, StringComparison.InvariantCultureIgnoreCase))
                return "BatchTransferError";

            else if (respX.Equals(ErrorCallHelp_SE, StringComparison.InvariantCultureIgnoreCase))
            {
                extMsg = "System Malfunction";
                return "ErrorCallHelp_SE";
            }
            else if (respX.Equals(Declined, StringComparison.InvariantCultureIgnoreCase))
                return "Declined";

            else if (respX.Equals(DestinationError, StringComparison.InvariantCultureIgnoreCase))
                return "DestinationError";

            else if (respX.Equals(NetworkRequestError, StringComparison.InvariantCultureIgnoreCase))
                return "NetworkRequestError";

            else if (respX.Equals(TimeoutError, StringComparison.InvariantCultureIgnoreCase))
                return "TimeoutError";

            else if (respX.Equals(TransactionNotAvailable, StringComparison.InvariantCultureIgnoreCase))
                return "TransactionNotAvailable";

            else if (respX.Equals(LostCarrier, StringComparison.InvariantCultureIgnoreCase))
                return "LostCarrier";

            else if (respX.Equals(CommunicationError, StringComparison.InvariantCultureIgnoreCase))
                return "CommunicationError";

            else if (respX.Equals(ApprovedOffline_1, StringComparison.InvariantCultureIgnoreCase))
                return "ApprovedOffline_1";

            else if (respX.Equals(ApprovedOffline_3, StringComparison.InvariantCultureIgnoreCase))
                return "ApprovedOffline_3";

            else if (respX.Equals(ForceSettlement, StringComparison.InvariantCultureIgnoreCase))
                return "ForceSettlement";

            else if (respX.Equals(ReadCardError, StringComparison.InvariantCultureIgnoreCase))
                return "Read Card Error";

            else if (respX.Equals(CardNotDetected, StringComparison.InvariantCultureIgnoreCase))
                return "Card Not Detected";

            else if (respX.Equals(FailedToUpdateCard, StringComparison.InvariantCultureIgnoreCase))
                return "Failed To Update Card";

            else
                return "Unknown Response Code";
        }

        public static bool IsEqualResponse(string response1, string response2)
        {
            if (string.IsNullOrWhiteSpace(response1) && string.IsNullOrWhiteSpace(response2))
                return true;

            else if ((string.IsNullOrWhiteSpace(response1) == false) && string.IsNullOrWhiteSpace(response2))
                return false;

            else if (string.IsNullOrWhiteSpace(response1) && (string.IsNullOrWhiteSpace(response2) == false))
                return false;

            else
                return response1.Trim().Equals(response2.Trim(), StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
