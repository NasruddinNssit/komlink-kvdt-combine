using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.IM30.Base
{
    public class FieldTypeDef
    {
        ///<summary>Penalty Amount</summary>
        public static string PenaltyAmount => "P1";

        ///<summary>Direction; 0 - For Kiosk or Counter; 1 - Entry; 2 - Exit; Refer to DirectionDef</summary>
        public static string Direction => "P2";

        ///<summary>First SP No (KomLink card only)</summary>
        public static string KomFirstSPNo => "P3";

        ///<summary>Second SP No (KomLink card only)</summary>
        public static string KomSecondSPNo => "P4";

        ///<summary>Transaction Code of Last Transaction</summary>
        public static string TransCodeOfLastTrans => "C1";

        ///<summary>Card Token (Credit/ Debit card only)</summary>
        public static string CardToken => "C2";

        ///<summary>Card Serial Number (TnG & KomLink card); TK: TnG and KomLink</summary>
        public static string TKCardSerialNo => "T0";

        ///<summary>Identification Code (TnG card only)</summary>
        public static string TnGIdentificationCode => "T1";

        ///<summary>Company Code (TnG card only)</summary>
        public static string TnGCompanyCode => "T2";

        ///<summary>Code1 (TnG card only)</summary>
        public static string TnGCode1 => "T3";

        ///<summary>Account Code (TnG card only)</summary>
        public static string TnGAccountCode => "T4";

        ///<summary>Luhn Key (TnG card only)</summary>
        public static string TnGLuhnKey => "T5";

        ///<summary>Main Purse Amount (TnG & KomLink card); TK: TnG and KomLink</summary>
        public static string TKMainPurseAmount => "T6";

        ///<summary>Main Tran No (TnG & KomLink card); TK: TnG and KomLink</summary>
        public static string TKMainTranNo => "T7";

        ///<summary>Blacklist Flag (TnG & KomLink card); 0 - Not blacklist; 1 - Blacklisted; TK: TnG and KomLink</summary>
        public static string TKBlacklistFlag => "T8";

        ///<summary>Debit Tran Type (TnG card only)</summary>
        public static string TnGDebitTranType => "TA";

        ///<summary>CSC Transaction Number (TnG card only)</summary>
        public static string TnGCSCTransactionNo => "TB";

        ///<summary>Amount Charged (TnG card only, read from card)</summary>
        public static string TnGAmountCharged => "TC";

        ///<summary>After Card Balance (TnG card only, read from card)</summary>
        public static string TnGAfterCardBalance => "TD";

        ///<summary>Operator ID (TnG card only, read from card)</summary>
        public static string TnGOperatorID => "TE";

        ///<summary>Transaction Date Time (YYMMDDhhmmss) (TnG & KomLink card only); TK: TnG and KomLink</summary>
        public static string TKTransactionDateTime => "TF";

        ///<summary>Back Up Purse Amount (KomLink card)</summary>
        public static string KomBackUpPurseAmount => "TG";

        ///<summary>Back Up Tran No (KomLink card)</summary>
        public static string KomBackUpTranNo => "TH";

        ///<summary>Entry Station Code (TnG & KomLink card); TK: TnG and KomLink</summary>
        public static string TKEntryStationCode => "H1";

        ///<summary>Entry Date Time (YYMMDDhhmmss) (TnG & KomLink card); TK: TnG and KomLink</summary>
        public static string TKEntryDateTime => "H2";

        ///<summary>Entry Operator ID (TnG card only)</summary>
        public static string TnGEntryOperatorID => "H3";

        ///<summary>Entry Gate No (TnG & KomLink card); TK: TnG and KomLink</summary>
        public static string TKEntryGateNo => "H4";

        ///<summary>Exit Station Code (TnG & KomLink card); TK: TnG and KomLink</summary>
        public static string TKExitStationCode => "H5";

        ///<summary>Exit Date Time (YYMMDDhhmmss) (TnG & KomLink card); TK: TnG and KomLink</summary>
        public static string TKExitDateTime => "H6";

        ///<summary>Exit Operator ID (TnG card only)</summary>
        public static string TnGExitOperatorID => "H7";

        ///<summary>Exit Gate No (TnG & KomLink card); TK: TnG and KomLink</summary>
        public static string TKExitGateNo => "H8";

        ///<summary>SAM ID</summary>
        public static string SamId => "H9";

        ///<summary>Station Code (From SAM)</summary>
        public static string TnGSamIdStationCode => "HA";

        ///<summary>Operator ID (From SAM)</summary>
        public static string TnGSamIdOperatorID => "HB";

        ///<summary>Gate No (From SAM)</summary>
        public static string TnGSamIdGateNo => "HC";

        ///<summary>Blacklist SAM ID (KomLink card only)</summary>
        public static string KomBlacklistSamId => "K0";

        ///<summary>KVDT Card No (KomLink card only)</summary>
        public static string KomKVDTCardNo => "K1";

        ///<summary>Is Card Cancelled (KomLink card only); 0 - No; 1 - Yes</summary>
        public static string KomIsCardCancelled => "K2";

        ///<summary>Is 1st SP Available (KomLink card only); 0 - No; 1 - Yes</summary>
        public static string KomIs1stSPAvailable => "K3";

        ///<summary>Is 2nd SP Available (KomLink card only); 0 - No; 1 - Yes</summary>
        public static string KomIs2ndSPAvailable => "K4";

        ///<summary>First SP Info (KomLink card only)</summary>
        public static string Kom1stSPInfo => "K5";

        ///<summary>Second SP Info (KomLink card only)</summary>
        public static string Kom2ndSPInfo => "K6";

        ///<summary>Issuer SAM ID (KomLink card only)</summary>
        public static string KomIssuerSamId => "K7";

        ///<summary>Gender (KomLink card only); F - Female; M - Male; U - Unknown</summary>
        public static string KomGender => "K8";

        ///<summary>Card Issued Date (YYMMDD) (KomLink card only)</summary>
        public static string KomCardIssuedDate => "K9";

        ///<summary>Card Expired Date (YYMMDD) (KomLink card only)</summary>
        public static string KomCardExpiredDate => "KA";

        ///<summary>PNR No. (KomLink card only)</summary>
        public static string KomPNRNo => "KB";

        ///<summary>Card Type (KomLink card only)</summary>
        public static string KomCardType => "KC";

        ///<summary>Card Type Expired Date (YYMMDD) (KomLink card only)</summary>
        public static string KomCardTypeExpiredDate => "KD";

        ///<summary>Is Malaysian (KomLink card only); 0 - No; 1 - Yes</summary>
        public static string KomIsMalaysian => "KE";

        ///<summary>DOB (YYMMDD) (KomLink card only)</summary>
        public static string KomDOB => "KF";

        ///<summary>LRC Key (KomLink card only)</summary>
        public static string KomLRCKey => "KG";

        ///<summary>ID Type (KomLink card only); I: IC; P: Passport; U: Unknown</summary>
        public static string KomIdType => "KH";

        ///<summary>ID No (KomLink card only)</summary>
        public static string KomIdNo => "KI";

        ///<summary>Passenger Name (KomLink card only)</summary>
        public static string KomPassengerName => "KJ";

        ///<summary>Blacklist Start Date Time (YYMMDD) (KomLink card only)</summary>
        public static string KomBlacklistStartDateTime => "KK";

        ///<summary>Blacklist Code (KomLink card only)</summary>
        public static string KomBlacklistCode => "KL";

        ///<summary>Trigger Blacklist Card (KomLink card only); 0 - No; 1 - Yes</summary>
        public static string KomTriggerBlacklistCard => "KM";

        ///<summary>Blacklisted Date Time (YYMMDDhhmmss) (KomLink card only)</summary>
        public static string KomBlacklistedDateTime => "KN";

        ///<summary>Refill SAM ID (KomLink card only)</summary>
        public static string KomRefillSamId => "KO";

        ///<summary>Refill Date Time (YYMMDDhhmmss) (KomLink card only)</summary>
        public static string KomRefillDateTime => "KP";

        ///<summary>Action: Indicate different action (KomLink card only)</summary>
        public static string KomAction => "KQ";

        ///<summary>SP Sale Doc No (KomLink card only)</summary>
        public static string KomSPSaleDocNo => "KR";

        ///<summary>SP Max Travel Amt Per Day Per Trip (KomLink card only)</summary>
        public static string KomSPMaxTravelAmtPerDayPerTrip => "KS";

        ///<summary>SP Start Date (KomLink card only)</summary>
        public static string KomSPStartDate => "KT";

        ///<summary>SP End Date (KomLink card only)</summary>
        public static string KomSPEndDate => "KU";

        ///<summary>SP Service Code (KomLink card only)</summary>
        public static string KomSPServiceCode => "KV";

        ///<summary>SP Package Code (KomLink card only)</summary>
        public static string KomSPPackageCode => "KW";

        ///<summary>SP Type (KomLink card only)</summary>
        public static string KomSPType => "KX";

        ///<summary>SP Max Trip Count Per Day (KomLink card only)</summary>
        public static string KomSPMaxTripCountPerDay => "KY";

        ///<summary>SP Is Avoid Checking (KomLink card only); 0 - No; 1 - Yes</summary>
        public static string KomSPIsAvoidChecking => "KZ";

        ///<summary>SP Is Avoid Trip Duration Check (KomLink card only); 0 - No; 1 - Yes</summary>
        public static string KomSPIsAvoidTripDurationCheck => "L1";

        ///<summary>SP Origin Station No (KomLink card only)</summary>
        public static string KomSPOriginStationNo => "L2";

        ///<summary>SP Destination Station No (KomLink card only)</summary>
        public static string KomSPDestinationStationNo => "L3";

        ///<summary>First SP Last Travel Date (YYMMDD) (KomLink card only)</summary>
        public static string Kom1stSPLastTravelDate => "L4";

        ///<summary>First SP Daily Travel Trip Count (KomLink card only)</summary>
        public static string Kom1stSPDailyTravelTripCount => "L5";

        ///<summary>First SP Set Unavailable (KomLink card only); 0 - No; 1 - Yes</summary>
        public static string Kom1stSPSetUnavailable => "L6";

        ///<summary>Second SP Set Unavailable (KomLink card only); 0 - No; 1 - Yes</summary>
        public static string Kom2ndSPSetUnavailable => "L9";

        ///<summary>Is SP1 Available (KomLink card only); 0 - No; 1 - Yes</summary>
        public static string KomIsSP1Available => "LA";

        ///<summary>Is SP2 Available (KomLink card only); 0 - No; 1 - Yes</summary>
        public static string KomIsSP2Available => "LB";

        ///<summary>Is SP3 Available (KomLink card only); 0 - No; 1 - Yes</summary>
        public static string KomIsSP3Available => "LC";

        ///<summary>Is SP4 Available (KomLink card only); 0 - No; 1 - Yes</summary>
        public static string KomIsSP4Available => "LD";

        ///<summary>Is SP5 Available (KomLink card only); 0 - No; 1 - Yes</summary>
        public static string KomIsSP5Available => "LE";

        ///<summary>Is SP6 Available (KomLink card only); 0 - No; 1 - Yes</summary>
        public static string KomIsSP6Available => "LF";

        ///<summary>Is SP7 Available (KomLink card only); 0 - No; 1 - Yes</summary>
        public static string KomIsSP7Available => "LG";

        ///<summary>Is SP8 Available (KomLink card only); 0 - No; 1 - Yes</summary>
        public static string KomIsSP8Available => "LH";

        ///<summary>SP 1 Info (KomLink card only) Refer to Section 13.2 SP Info.</summary>
        public static string KomSP1Info => "LI";

        ///<summary>SP 2 Info (KomLink card only)</summary>
        public static string KomSP2Info => "LJ";

        ///<summary>SP 3 Info (KomLink card only)</summary>
        public static string KomSP3Info => "LK";

        ///<summary>SP 4 Info (KomLink card only)</summary>
        public static string KomSP4Info => "LL";

        ///<summary>SP 5 Info (KomLink card only)</summary>
        public static string KomSP5Info => "LM";

        ///<summary>SP 6 Info (KomLink card only)</summary>
        public static string KomSP6Info => "LN";

        ///<summary>SP 7 Info (KomLink card only) Refer to Section 13.2 SP Info.</summary>
        public static string KomSP7Info => "LO";

        ///<summary>SP 8 Info(KomLink card only)</summary>
        public static string KomSP8Info => "LP";

        ///<summary>SP Sale Date (YYMMDD)</summary>
        public static string KomSPSaleDate => "LQ";

        ///<summary>SP Issuer SAM ID</summary>
        public static string KomSPIssuerSamId => "LR";

        ///<summary>Approval Code</summary>
        public static string ApprovalCode => "01";

        ///<summary>Response Text</summary>
        public static string ResponseText => "02";

        ///<summary>Transaction Date; YYMMDD; Date Generate By IM30</summary>
        public static string TransactionDate => "03";

        ///<summary>Transaction Time; HHMMSS; Time Generate By IM30</summary>
        public static string TransactionTime => "04";

        ///<summary>Status Code</summary>
        public static string StatusCode => "05";

        ///<summary>Merchant Number</summary>
        public static string MerchantNumber => "06";

        ///<summary>Terminal Number</summary>
        public static string TerminalNumber => "08";

        ///<summary>Terminal Identification Number</summary>
        public static string TID => "16";

        ///<summary>Primary Account Number</summary>
        public static string PrimaryAccountNumber => "30";

        ///<summary>Expiration Date; YYMM</summary>
        public static string ExpirationDate => "31";

        ///<summary>Amount, Transaction</summary>
        public static string TransAmount => "40";

        ///<summary>Batch Number</summary>
        public static string BatchNo => "50";

        ///<summary>Invoice Number</summary>
        public static string InvoiceNo => "65";

        ///<summary>
        ///POS Transaction ID. Unique ID for each transaction where Terminal use to retrieve record from its database, 
        ///if record is available, means duplicate request from POS and Terminal will return transaction information from database 
        ///without create new transaction to prevent double charge
        ///</summary>
        public static string PosTransId => "77";
        ///<summary>Merchant Name and Address, organized in multiple lines</summary>
        public static string MerchantNameAndAddress => "D0";

        ///<summary>Merchant Number</summary>
        public static string MerchantNo => "D1";

        ///<summary>Card Issuer Name</summary>
        public static string CardIssuerName => "D2";

        ///<summary>Retrieval Reference Number</summary>
        public static string RRN => "D3";

        ///<summary>Issuer ID</summary>
        public static string IssuerId => "D4";

        ///<summary>Card Holder Name</summary>
        public static string CardHolderName => "D5";

        ///<summary>STAN (Standard Tracking Number)</summary>
        public static string STAN => "D9";

        ///<summary>Device Serial Number</summary>
        public static string DeviceSerialNo => "DA";

        ///<summary>Total Settle Batch</summary>
        public static string SettleTotalNoOfBatch => "S1";

        ///<summary>Settle Batch Name (For each settled batch)</summary>
        public static string SettleBatchName => "S2";

        ///<summary>Settle Response Code (For each settled batch)</summary>
        public static string SettleResponseCode => "S3";

        ///<summary>Total Sales Count (For each settled batch)</summary>
        public static string SettleTotalSalesCount => "S4";

        ///<summary>Total Sales Amount (For each settled batch)</summary>
        public static string SettleTotalSalesAmount => "S5";

        ///<summary>Total Refund Count (For each settled batch)</summary>
        public static string SettleTotalRefundCount => "S6";

        ///<summary>Total Refund Amount (For each settled batch)</summary>
        public static string SettleTotalRefundAmount => "S7";

        ///<summary>Settle Date Time (YYMMDDhhmmss) (For each settled batch)</summary>
        public static string SettleDateTime => "S8";

        ///<summary>Settle Response Text (For each settled batch)</summary>
        public static string SettleResponseText => "S9";

        ///<summary>AID. Example “A0000006150001”; Application ID</summary>
        public static string AID => "E0";

        ///<summary>Application Name. Example “MyDebit”</summary>
        public static string ApplicationName => "E1";

        ///<summary>Cryptogram. Example “EDDC9E1F70D10DFD”</summary>
        public static string Cryptogram => "E2";

        ///<summary>TVR. Example “8000008000”</summary>
        public static string TVR => "E3";

        ///<summary>Card Verification Method (CVM) ; 0  - Signature; 1  – Pin; 2  – No Pin, No Signature; 3  - CDCVM</summary>
        public static string CardVerificationMethod => "E4";

        ///<summary>Merchant Copy CVM Description</summary>
        public static string MerchantCopyCVMDesc => "E5";

        ///<summary>Customer Copy CVM Description</summary>
        public static string CustomerCopyCVMDesc => "E6";

        ///<summary>Payment Indicator; 0  - Unknown; 1  - Contact Card (ICC); 2  - Contactless Card</summary>
        public static string PaymentIndicator => "F1";

        ///<summary>Maintenance message</summary>
        public static string MaintenanceMessage => "M1";

        private static object _lock = new object();
        private static Dictionary<string, IM30FieldElementStructure> _fieldProperties = null;
        private static Dictionary<string, IM30FieldElementStructure> FieldProperties
        {
            get
            {
                if (_fieldProperties != null)
                    return _fieldProperties;
                else
                {
                    Thread tWorker = new Thread(new ThreadStart(new Action(() =>
                    {
                        try
                        {
                            lock (_lock)
                            {
                                if (_fieldProperties == null)
                                {
                                    Dictionary<string, IM30FieldElementStructure> fProp = GetNewFieldProperties();
                                    _fieldProperties = fProp;
                                }
                            }
                        }
                        catch (Exception ex)
                        { }
                    })));
                    tWorker.IsBackground = true;
                    tWorker.Start();
                    tWorker.Join();

                    return _fieldProperties;
                }
            }
        }

        private static Dictionary<string, IM30FieldElementStructure> GetNewFieldProperties()
        {
            Dictionary<string, IM30FieldElementStructure> retD = new Dictionary<string, IM30FieldElementStructure>();

            retD.Add(PenaltyAmount, new IM30FieldElementStructure(PenaltyAmount, "PenaltyAmount", FieldTypeAttributeEn.M, 12));
            retD.Add(Direction, new IM30FieldElementStructure(Direction, "Direction", FieldTypeAttributeEn.N, 1));
            retD.Add(KomFirstSPNo, new IM30FieldElementStructure(KomFirstSPNo, "KomFirstSPNo", FieldTypeAttributeEn.N, 1));
            retD.Add(KomSecondSPNo, new IM30FieldElementStructure(KomSecondSPNo, "KomSecondSPNo", FieldTypeAttributeEn.N, 1));
            retD.Add(TransCodeOfLastTrans, new IM30FieldElementStructure(TransCodeOfLastTrans, "TransCodeOfLastTrans", FieldTypeAttributeEn.ANS, 2));
            retD.Add(CardToken, new IM30FieldElementStructure(CardToken, "CardToken", FieldTypeAttributeEn.ANS, 50, isDefinableFieldLength: true));
            retD.Add(TKCardSerialNo, new IM30FieldElementStructure(TKCardSerialNo, "TKCardSerialNo", FieldTypeAttributeEn.N, 10, isDefinableFieldLength: true));
            retD.Add(TnGIdentificationCode, new IM30FieldElementStructure(TnGIdentificationCode, "TnGIdentificationCode", FieldTypeAttributeEn.N, 4));
            retD.Add(TnGCompanyCode, new IM30FieldElementStructure(TnGCompanyCode, "TnGCompanyCode", FieldTypeAttributeEn.N, 2));
            retD.Add(TnGCode1, new IM30FieldElementStructure(TnGCode1, "TnGCode1", FieldTypeAttributeEn.N, 2));
            retD.Add(TnGAccountCode, new IM30FieldElementStructure(TnGAccountCode, "TnGAccountCode", FieldTypeAttributeEn.N, 9));
            retD.Add(TnGLuhnKey, new IM30FieldElementStructure(TnGLuhnKey, "TnGLuhnKey", FieldTypeAttributeEn.N, 2));
            retD.Add(TKMainPurseAmount, new IM30FieldElementStructure(TKMainPurseAmount, "TKMainPurseAmount", FieldTypeAttributeEn.M, 12));
            retD.Add(TKMainTranNo, new IM30FieldElementStructure(TKMainTranNo, "TKMainTranNo", FieldTypeAttributeEn.N, 6, isDefinableFieldLength: true));
            retD.Add(TKBlacklistFlag, new IM30FieldElementStructure(TKBlacklistFlag, "TKBlacklistFlag", FieldTypeAttributeEn.N, 1));
            retD.Add(TnGDebitTranType, new IM30FieldElementStructure(TnGDebitTranType, "TnGDebitTranType", FieldTypeAttributeEn.AN, 2));
            retD.Add(TnGCSCTransactionNo, new IM30FieldElementStructure(TnGCSCTransactionNo, "TnGCSCTransactionNo", FieldTypeAttributeEn.N, 5, isDefinableFieldLength: true));
            retD.Add(TnGAmountCharged, new IM30FieldElementStructure(TnGAmountCharged, "TnGAmountCharged", FieldTypeAttributeEn.M, 12));
            retD.Add(TnGAfterCardBalance, new IM30FieldElementStructure(TnGAfterCardBalance, "TnGAfterCardBalance", FieldTypeAttributeEn.M, 12));
            retD.Add(TnGOperatorID, new IM30FieldElementStructure(TnGOperatorID, "TnGOperatorID", FieldTypeAttributeEn.AN, 2));
            retD.Add(TKTransactionDateTime, new IM30FieldElementStructure(TKTransactionDateTime, "TKTransactionDateTime", FieldTypeAttributeEn.N_DT, 12));
            retD.Add(KomBackUpPurseAmount, new IM30FieldElementStructure(KomBackUpPurseAmount, "KomBackUpPurseAmount", FieldTypeAttributeEn.M, 12));
            retD.Add(KomBackUpTranNo, new IM30FieldElementStructure(KomBackUpTranNo, "KomBackUpTranNo", FieldTypeAttributeEn.N, 6, isDefinableFieldLength: true));
            retD.Add(TKEntryStationCode, new IM30FieldElementStructure(TKEntryStationCode, "TKEntryStationCode", FieldTypeAttributeEn.AN, 8, isDefinableFieldLength: true));
            retD.Add(TKEntryDateTime, new IM30FieldElementStructure(TKEntryDateTime, "TKEntryDateTime", FieldTypeAttributeEn.N_DT, 12));
            retD.Add(TnGEntryOperatorID, new IM30FieldElementStructure(TnGEntryOperatorID, "TnGEntryOperatorID", FieldTypeAttributeEn.AN, 2));
            retD.Add(TKEntryGateNo, new IM30FieldElementStructure(TKEntryGateNo, "TKEntryGateNo", FieldTypeAttributeEn.AN, 10, isDefinableFieldLength: true));
            retD.Add(TKExitStationCode, new IM30FieldElementStructure(TKExitStationCode, "TKExitStationCode", FieldTypeAttributeEn.AN, 8, isDefinableFieldLength: true));
            retD.Add(TKExitDateTime, new IM30FieldElementStructure(TKExitDateTime, "TKExitDateTime", FieldTypeAttributeEn.N_DT, 12));
            retD.Add(TnGExitOperatorID, new IM30FieldElementStructure(TnGExitOperatorID, "TnGExitOperatorID", FieldTypeAttributeEn.AN, 2));
            retD.Add(TKExitGateNo, new IM30FieldElementStructure(TKExitGateNo, "TKExitGateNo", FieldTypeAttributeEn.AN, 10, isDefinableFieldLength: true));
            retD.Add(SamId, new IM30FieldElementStructure(SamId, "SamId", FieldTypeAttributeEn.AN, 10, isDefinableFieldLength: true));
            retD.Add(TnGSamIdStationCode, new IM30FieldElementStructure(TnGSamIdStationCode, "TnGSamIdStationCode", FieldTypeAttributeEn.AN, 8, isDefinableFieldLength: true));
            retD.Add(TnGSamIdOperatorID, new IM30FieldElementStructure(TnGSamIdOperatorID, "TnGSamIdOperatorID", FieldTypeAttributeEn.AN, 2));
            retD.Add(TnGSamIdGateNo, new IM30FieldElementStructure(TnGSamIdGateNo, "TnGSamIdGateNo", FieldTypeAttributeEn.AN, 10, isDefinableFieldLength: true));
            retD.Add(KomBlacklistSamId, new IM30FieldElementStructure(KomBlacklistSamId, "KomBlacklistSamId", FieldTypeAttributeEn.AN, 4));
            retD.Add(KomKVDTCardNo, new IM30FieldElementStructure(KomKVDTCardNo, "KomKVDTCardNo", FieldTypeAttributeEn.N, 18, isDefinableFieldLength: true));
            retD.Add(KomIsCardCancelled, new IM30FieldElementStructure(KomIsCardCancelled, "KomIsCardCancelled", FieldTypeAttributeEn.N, 1));
            retD.Add(KomIs1stSPAvailable, new IM30FieldElementStructure(KomIs1stSPAvailable, "KomIs1stSPAvailable", FieldTypeAttributeEn.N, 1));
            retD.Add(KomIs2ndSPAvailable, new IM30FieldElementStructure(KomIs2ndSPAvailable, "KomIs2ndSPAvailable", FieldTypeAttributeEn.N, 1));
            retD.Add(Kom1stSPInfo, new IM30FieldElementStructure(Kom1stSPInfo, "Kom1stSPInfo", FieldTypeAttributeEn.ANS, 100, isDefinableFieldLength: true));
            retD.Add(Kom2ndSPInfo, new IM30FieldElementStructure(Kom2ndSPInfo, "Kom2ndSPInfo", FieldTypeAttributeEn.ANS, 100, isDefinableFieldLength: true));
            retD.Add(KomIssuerSamId, new IM30FieldElementStructure(KomIssuerSamId, "KomIssuerSamId", FieldTypeAttributeEn.AN, 4));
            retD.Add(KomGender, new IM30FieldElementStructure(KomGender, "KomGender", FieldTypeAttributeEn.AN, 1));
            retD.Add(KomCardIssuedDate, new IM30FieldElementStructure(KomCardIssuedDate, "KomCardIssuedDate", FieldTypeAttributeEn.N_D, 6));
            retD.Add(KomCardExpiredDate, new IM30FieldElementStructure(KomCardExpiredDate, "KomCardExpiredDate", FieldTypeAttributeEn.N_D, 6));
            retD.Add(KomPNRNo, new IM30FieldElementStructure(KomPNRNo, "KomPNRNo", FieldTypeAttributeEn.AN, 12));
            retD.Add(KomCardType, new IM30FieldElementStructure(KomCardType, "KomCardType", FieldTypeAttributeEn.AN, 4));
            retD.Add(KomCardTypeExpiredDate, new IM30FieldElementStructure(KomCardTypeExpiredDate, "KomCardTypeExpiredDate", FieldTypeAttributeEn.N_D, 6));
            retD.Add(KomIsMalaysian, new IM30FieldElementStructure(KomIsMalaysian, "KomIsMalaysian", FieldTypeAttributeEn.N, 1));
            retD.Add(KomDOB, new IM30FieldElementStructure(KomDOB, "KomDOB", FieldTypeAttributeEn.N_D, 6));
            retD.Add(KomLRCKey, new IM30FieldElementStructure(KomLRCKey, "KomLRCKey", FieldTypeAttributeEn.B, 1));
            retD.Add(KomIdType, new IM30FieldElementStructure(KomIdType, "KomIdType", FieldTypeAttributeEn.AN, 1));
            retD.Add(KomIdNo, new IM30FieldElementStructure(KomIdNo, "KomIdNo", FieldTypeAttributeEn.ANS, 15, isDefinableFieldLength: true));
            retD.Add(KomPassengerName, new IM30FieldElementStructure(KomPassengerName, "KomPassengerName", FieldTypeAttributeEn.ANS, 32, isDefinableFieldLength: true));
            retD.Add(KomBlacklistStartDateTime, new IM30FieldElementStructure(KomBlacklistStartDateTime, "KomBlacklistStartDateTime", FieldTypeAttributeEn.N_D, 6));
            retD.Add(KomBlacklistCode, new IM30FieldElementStructure(KomBlacklistCode, "KomBlacklistCode", FieldTypeAttributeEn.AN, 3));
            retD.Add(KomTriggerBlacklistCard, new IM30FieldElementStructure(KomTriggerBlacklistCard, "KomTriggerBlacklistCard", FieldTypeAttributeEn.N, 1));
            retD.Add(KomBlacklistedDateTime, new IM30FieldElementStructure(KomBlacklistedDateTime, "KomBlacklistedDateTime", FieldTypeAttributeEn.N_DT, 12));
            retD.Add(KomRefillSamId, new IM30FieldElementStructure(KomRefillSamId, "KomRefillSamId", FieldTypeAttributeEn.AN, 4));
            retD.Add(KomRefillDateTime, new IM30FieldElementStructure(KomRefillDateTime, "KomRefillDateTime", FieldTypeAttributeEn.N_DT, 12));
            retD.Add(KomAction, new IM30FieldElementStructure(KomAction, "KomAction", FieldTypeAttributeEn.N, 1));
            retD.Add(KomSPSaleDocNo, new IM30FieldElementStructure(KomSPSaleDocNo, "KomSPSaleDocNo", FieldTypeAttributeEn.ANS, 16, isDefinableFieldLength: true));
            retD.Add(KomSPMaxTravelAmtPerDayPerTrip, new IM30FieldElementStructure(KomSPMaxTravelAmtPerDayPerTrip, "KomSPMaxTravelAmtPerDayPerTrip", FieldTypeAttributeEn.N, 5, isDefinableFieldLength: true));
            retD.Add(KomSPStartDate, new IM30FieldElementStructure(KomSPStartDate, "KomSPStartDate", FieldTypeAttributeEn.N, 6));
            retD.Add(KomSPEndDate, new IM30FieldElementStructure(KomSPEndDate, "KomSPEndDate", FieldTypeAttributeEn.N, 6));
            retD.Add(KomSPServiceCode, new IM30FieldElementStructure(KomSPServiceCode, "KomSPServiceCode", FieldTypeAttributeEn.ANS, 4));
            retD.Add(KomSPPackageCode, new IM30FieldElementStructure(KomSPPackageCode, "KomSPPackageCode", FieldTypeAttributeEn.ANS, 4));
            retD.Add(KomSPType, new IM30FieldElementStructure(KomSPType, "KomSPType", FieldTypeAttributeEn.ANS, 4));
            retD.Add(KomSPMaxTripCountPerDay, new IM30FieldElementStructure(KomSPMaxTripCountPerDay, "KomSPMaxTripCountPerDay", FieldTypeAttributeEn.N, 3, isDefinableFieldLength: true));
            retD.Add(KomSPIsAvoidChecking, new IM30FieldElementStructure(KomSPIsAvoidChecking, "KomSPIsAvoidChecking", FieldTypeAttributeEn.N, 1));
            retD.Add(KomSPIsAvoidTripDurationCheck, new IM30FieldElementStructure(KomSPIsAvoidTripDurationCheck, "KomSPIsAvoidTripDurationCheck ", FieldTypeAttributeEn.N, 1));
            retD.Add(KomSPOriginStationNo, new IM30FieldElementStructure(KomSPOriginStationNo, "KomSPOriginStationNo", FieldTypeAttributeEn.ANS, 8, isDefinableFieldLength: true));
            retD.Add(KomSPDestinationStationNo, new IM30FieldElementStructure(KomSPDestinationStationNo, "KomSPDestinationStationNo", FieldTypeAttributeEn.ANS, 8, isDefinableFieldLength: true));
            retD.Add(Kom1stSPLastTravelDate, new IM30FieldElementStructure(Kom1stSPLastTravelDate, "Kom1stSPLastTravelDate", FieldTypeAttributeEn.N_D, 6));
            retD.Add(Kom1stSPDailyTravelTripCount, new IM30FieldElementStructure(Kom1stSPDailyTravelTripCount, "Kom1stSPDailyTravelTripCount", FieldTypeAttributeEn.N, 3, isDefinableFieldLength: true));
            retD.Add(Kom1stSPSetUnavailable, new IM30FieldElementStructure(Kom1stSPSetUnavailable, "Kom1stSPSetUnavailable", FieldTypeAttributeEn.N, 1));
            retD.Add(Kom2ndSPSetUnavailable, new IM30FieldElementStructure(Kom2ndSPSetUnavailable, "Kom2ndSPSetUnavailable", FieldTypeAttributeEn.N, 1));
            retD.Add(KomIsSP1Available, new IM30FieldElementStructure(KomIsSP1Available, "KomIsSP1Available", FieldTypeAttributeEn.N, 1));
            retD.Add(KomIsSP2Available, new IM30FieldElementStructure(KomIsSP2Available, "KomIsSP2Available", FieldTypeAttributeEn.N, 1));
            retD.Add(KomIsSP3Available, new IM30FieldElementStructure(KomIsSP3Available, "KomIsSP3Available", FieldTypeAttributeEn.N, 1));
            retD.Add(KomIsSP4Available, new IM30FieldElementStructure(KomIsSP4Available, "KomIsSP4Available", FieldTypeAttributeEn.N, 1));
            retD.Add(KomIsSP5Available, new IM30FieldElementStructure(KomIsSP5Available, "KomIsSP5Available", FieldTypeAttributeEn.N, 1));
            retD.Add(KomIsSP6Available, new IM30FieldElementStructure(KomIsSP6Available, "KomIsSP6Available", FieldTypeAttributeEn.N, 1));
            retD.Add(KomIsSP7Available, new IM30FieldElementStructure(KomIsSP7Available, "KomIsSP7Available", FieldTypeAttributeEn.N, 1));
            retD.Add(KomIsSP8Available, new IM30FieldElementStructure(KomIsSP8Available, "KomIsSP8Available", FieldTypeAttributeEn.N, 1));
            retD.Add(KomSP1Info, new IM30FieldElementStructure(KomSP1Info, "KomSP1Info", FieldTypeAttributeEn.ANS, 100, isDefinableFieldLength: true));
            retD.Add(KomSP2Info, new IM30FieldElementStructure(KomSP2Info, "KomSP2Info", FieldTypeAttributeEn.ANS, 100, isDefinableFieldLength: true));
            retD.Add(KomSP3Info, new IM30FieldElementStructure(KomSP3Info, "KomSP3Info", FieldTypeAttributeEn.ANS, 100, isDefinableFieldLength: true));
            retD.Add(KomSP4Info, new IM30FieldElementStructure(KomSP4Info, "KomSP4Info", FieldTypeAttributeEn.ANS, 100, isDefinableFieldLength: true));
            retD.Add(KomSP5Info, new IM30FieldElementStructure(KomSP5Info, "KomSP5Info", FieldTypeAttributeEn.ANS, 100, isDefinableFieldLength: true));
            retD.Add(KomSP6Info, new IM30FieldElementStructure(KomSP6Info, "KomSP6Info", FieldTypeAttributeEn.ANS, 100, isDefinableFieldLength: true));
            retD.Add(KomSP7Info, new IM30FieldElementStructure(KomSP7Info, "KomSP7Info", FieldTypeAttributeEn.ANS, 100, isDefinableFieldLength: true));
            retD.Add(KomSP8Info, new IM30FieldElementStructure(KomSP8Info, "KomSP8Info", FieldTypeAttributeEn.ANS, 100, isDefinableFieldLength: true));
            retD.Add(KomSPSaleDate, new IM30FieldElementStructure(KomSPSaleDate, "KomSPSaleDate", FieldTypeAttributeEn.N_D, 6));
            retD.Add(KomSPIssuerSamId, new IM30FieldElementStructure(KomSPIssuerSamId, "KomSPIssuerSamId", FieldTypeAttributeEn.AN, 4));
            retD.Add(ApprovalCode, new IM30FieldElementStructure(ApprovalCode, "ApprovalCode", FieldTypeAttributeEn.ANS, 6));
            retD.Add(ResponseText, new IM30FieldElementStructure(ResponseText, "ResponseText", FieldTypeAttributeEn.ANS, 40));
            retD.Add(TransactionDate, new IM30FieldElementStructure(TransactionDate, "TransactionDate", FieldTypeAttributeEn.N_DS, 6));
            retD.Add(TransactionTime, new IM30FieldElementStructure(TransactionTime, "TransactionTime", FieldTypeAttributeEn.N_TS, 6));
            retD.Add(StatusCode, new IM30FieldElementStructure(StatusCode, "StatusCode", FieldTypeAttributeEn.ANS, 2));
            retD.Add(MerchantNumber, new IM30FieldElementStructure(MerchantNumber, "MerchantNumber", FieldTypeAttributeEn.N, 12));
            retD.Add(TerminalNumber, new IM30FieldElementStructure(TerminalNumber, "TerminalNumber", FieldTypeAttributeEn.N, 4));
            retD.Add(TID, new IM30FieldElementStructure(TID, "TID", FieldTypeAttributeEn.N, 8));
            retD.Add(PrimaryAccountNumber, new IM30FieldElementStructure(PrimaryAccountNumber, "PrimaryAccountNumber", FieldTypeAttributeEn.N, 19, isDefinableFieldLength: true));
            retD.Add(ExpirationDate, new IM30FieldElementStructure(ExpirationDate, "ExpirationDate", FieldTypeAttributeEn.N_M, 4));
            retD.Add(TransAmount, new IM30FieldElementStructure(TransAmount, "TransAmount", FieldTypeAttributeEn.M, 12));
            retD.Add(BatchNo, new IM30FieldElementStructure(BatchNo, "BatchNo", FieldTypeAttributeEn.N, 6));
            retD.Add(InvoiceNo, new IM30FieldElementStructure(InvoiceNo, "InvoiceNo", FieldTypeAttributeEn.ANS, 6));
            retD.Add(PosTransId, new IM30FieldElementStructure(PosTransId, "PosTransId", FieldTypeAttributeEn.ANS, 50, isDefinableFieldLength: true));
            retD.Add(MerchantNameAndAddress, new IM30FieldElementStructure(MerchantNameAndAddress, "MerchantNameAndAddress", FieldTypeAttributeEn.ANS, 69));
            retD.Add(MerchantNo, new IM30FieldElementStructure(MerchantNo, "MerchantNo", FieldTypeAttributeEn.ANS, 15));
            retD.Add(CardIssuerName, new IM30FieldElementStructure(CardIssuerName, "CardIssuerName", FieldTypeAttributeEn.ANS, 30, isDefinableFieldLength: true));
            retD.Add(RRN, new IM30FieldElementStructure(RRN, "RRN", FieldTypeAttributeEn.ANS, 12));
            retD.Add(IssuerId, new IM30FieldElementStructure(IssuerId, "IssuerId", FieldTypeAttributeEn.N, 2));
            retD.Add(CardHolderName, new IM30FieldElementStructure(CardHolderName, "CardHolderName", FieldTypeAttributeEn.ANS, 26));
            retD.Add(STAN, new IM30FieldElementStructure(STAN, "STAN", FieldTypeAttributeEn.N, 6));
            retD.Add(DeviceSerialNo, new IM30FieldElementStructure(DeviceSerialNo, "DeviceSerialNo", FieldTypeAttributeEn.ANS, 13, isDefinableFieldLength: true));
            retD.Add(SettleTotalNoOfBatch, new IM30FieldElementStructure(SettleTotalNoOfBatch, "SettleTotalNoOfBatch", FieldTypeAttributeEn.N, 2, isDefinableFieldLength: true));
            retD.Add(SettleBatchName, new IM30FieldElementStructure(SettleBatchName, "SettleBatchName", FieldTypeAttributeEn.ANS, 20, isDefinableFieldLength: true));
            retD.Add(SettleResponseCode, new IM30FieldElementStructure(SettleResponseCode, "SettleResponseCode", FieldTypeAttributeEn.AN, 2));
            retD.Add(SettleTotalSalesCount, new IM30FieldElementStructure(SettleTotalSalesCount, "SettleTotalSalesCount", FieldTypeAttributeEn.N, 3));
            retD.Add(SettleTotalSalesAmount, new IM30FieldElementStructure(SettleTotalSalesAmount, "SettleTotalSalesAmount", FieldTypeAttributeEn.M, 12));
            retD.Add(SettleTotalRefundCount, new IM30FieldElementStructure(SettleTotalRefundCount, "SettleTotalRefundCount", FieldTypeAttributeEn.N, 3));
            retD.Add(SettleTotalRefundAmount, new IM30FieldElementStructure(SettleTotalRefundAmount, "SettleTotalRefundAmount", FieldTypeAttributeEn.M, 12));
            retD.Add(SettleDateTime, new IM30FieldElementStructure(SettleDateTime, "SettleDateTime", FieldTypeAttributeEn.N_DT, 12));
            retD.Add(SettleResponseText, new IM30FieldElementStructure(SettleResponseText, "SettleResponseText", FieldTypeAttributeEn.ANS, 40, isDefinableFieldLength: true));
            retD.Add(AID, new IM30FieldElementStructure(AID, "AID", FieldTypeAttributeEn.ANS, 16, isDefinableFieldLength: true));
            retD.Add(ApplicationName, new IM30FieldElementStructure(ApplicationName, "ApplicationName", FieldTypeAttributeEn.ANS, 16, isDefinableFieldLength: true));
            retD.Add(Cryptogram, new IM30FieldElementStructure(Cryptogram, "Cryptogram", FieldTypeAttributeEn.ANS, 16));
            retD.Add(TVR, new IM30FieldElementStructure(TVR, "TVR", FieldTypeAttributeEn.ANS, 10));
            retD.Add(CardVerificationMethod, new IM30FieldElementStructure(CardVerificationMethod, "CardVerificationMethod", FieldTypeAttributeEn.N, 1));
            retD.Add(MerchantCopyCVMDesc, new IM30FieldElementStructure(MerchantCopyCVMDesc, "MerchantCopyCVMDesc", FieldTypeAttributeEn.ANS, 64, isDefinableFieldLength: true));
            retD.Add(CustomerCopyCVMDesc, new IM30FieldElementStructure(CustomerCopyCVMDesc, "CustomerCopyCVMDesc", FieldTypeAttributeEn.ANS, 64, isDefinableFieldLength: true));
            retD.Add(PaymentIndicator, new IM30FieldElementStructure(PaymentIndicator, "PaymentIndicator", FieldTypeAttributeEn.ANS, 1));
            retD.Add(MaintenanceMessage, new IM30FieldElementStructure(MaintenanceMessage, "MaintenanceMessage", FieldTypeAttributeEn.ANS, 100, isDefinableFieldLength: true));

            return retD;
        }

        public static IM30FieldElementStructure GetTypeProperties(string typeCode)
        {
            if (FieldProperties.TryGetValue(typeCode, out IM30FieldElementStructure fieldElementStructure) == true)
                return fieldElementStructure;
            else
                return null;
        }


        public static bool IsEqualType(string type1, string type2)
        {
            if (string.IsNullOrWhiteSpace(type1) && string.IsNullOrWhiteSpace(type2))
                return true;

            else if ((string.IsNullOrWhiteSpace(type1) == false) && string.IsNullOrWhiteSpace(type2))
                return false;

            else if (string.IsNullOrWhiteSpace(type1) && (string.IsNullOrWhiteSpace(type2) == false))
                return false;

            else
                return type1.Trim().Equals(type2.Trim(), StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
