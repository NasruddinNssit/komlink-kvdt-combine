using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.IM30.CardEntityData
{
    public class GetLastTransactionResp : ICardResponse
    {
        public TransactionTypeEn TransactionType { get; private set; } = TransactionTypeEn.Unknown;
        public TransactionCodeEn TransactionCode { get; set; } = TransactionCodeEn.Unknown;
        public decimal Amount { get; set; } = 0.00M;
        public decimal PenaltyAmount { get; set; } = 0.00M;

        public ResponseCodeEn ResponseResult { get; set; } = ResponseCodeEn.Fail;
        public string ApprovalCode { get; set; } = null;
        public string InvoiceNo { get; set; } = null;

        /// <summary>
        /// Merchant Name and Address
        /// </summary>
        public string MerchantNameAddr { get; set; } = null;

        /// <summary>
        /// Terminal Identification Number
        /// </summary>
        public int TID { get; set; } = 0;

        public string MerchantNumber { get; set; } = null;

        public string CardIssuerName { get; set; } = null;
        public string CardNumber { get; set; } = null;
        public string CardToken { get; set; } = null;

        public DateTime? ExpiryDate { get; set; } = null;
        public string BatchNumber { get; set; } = null;


        /// <summary>
        /// Reader generated transaction datetime
        /// </summary>
        public DateTime? ReaderTransactionDateTime { get; set; } = null;

        /// <summary>
        /// Retrieval Reference Number
        /// </summary>
        public string RRN { get; set; } = null;

        public string CardIssuerID { get; set; } = null;
        public string CardHolderName { get; set; } = null;

        /// <summary>
        /// Application ID
        /// </summary>
        public string AID { get; set; } = null;
        public string ApplicationName { get; set; } = null;
        public string Cryptogram { get; set; } = null;

        /// <summary>
        /// Transaction verification result
        /// </summary>
        public string TVR { get; set; } = null;

        /// <summary>
        /// Card Verification Method
        /// </summary>
        public string CVM { get; set; } = null;

        /// <summary>
        /// Merchant Copy CVM Description
        /// </summary>
        public string CVMCopyDesc { get; set; } = null;

        /// <summary>
        /// Customer Copy CVM Description
        /// </summary>
        public string CustomerCopyCVM { get; set; } = null;

        /// <summary>
        /// Standard Unique Trace Number 
        /// </summary>
        public string STAN { get; set; } = null;

        public PaymentIndicatorEn PaymentIndicator { get; set; } = PaymentIndicatorEn.Unknown;

        public string ACGEntryStationCode = null;
        public DateTime? ACGEntryDateTime { get; set; } = null;
        public string ACGEntryOperatorId { get; set; } = null;
        public string ACGEntryGateNo { get; set; } = null;

        public string ACGExitStationCode = null;
        public DateTime? ACGExitDateTime { get; set; } = null;
        public string ACGExitOperatorId { get; set; } = null;
        public string ACGExitGateNo { get; set; } = null;
        
        public string TnGDEbitTranType { get; set; } = null;
        public decimal? TnGAmountChange { get; set; } = null;
        public decimal? TnGAfterCardBalance { get; set; } = null;
        public int? TnGCSCTransactionNo { get; set; } = null;
        public string TnGOperatorId { get; set; } = null;

        /// <summary>
        /// Card Transaction datetime. Datetime from application
        /// </summary>
        public DateTime? TransactionDateTime { get; set; } = null;

        public decimal? MainPurseAmount { get; set; } = null;
        public int? MainTranNo { get; set; } = null;

        public decimal? FirstSPLastTravelDate { get; set; } = null;
        public int? FirstSPDailyTravelTripCount { get; set; } = null;

        public decimal? SecondSPLastTravelDate { get; set; } = null;
        public int? SecondSPDailyTravelTripCount { get; set; } = null;

        public string POSTransactionID { get; set; } = null;

        public GetLastTransactionResp(TransactionTypeEn transactionType)
        {
            TransactionType = transactionType;
        }
    }
}