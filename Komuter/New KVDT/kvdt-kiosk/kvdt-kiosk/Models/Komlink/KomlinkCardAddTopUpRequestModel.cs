
using NssIT.Kiosk.AppDecorator.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace kvdt_kiosk.Models.Komlink
{
    public class KomlinkCardAddTopUpRequestModel
    {
        public string KomlinKCardNo { get; set; }
        public decimal Amount { get; set; }
        public string PurchaseCounterId { get; set; } //ACG Id
        public string MCurrenciesId { get; set; }
        public string PurchaseStationId { get; set; }
    }

    public class KomlinkCardAddTopupResultModel
    {
        public string TransactionNo { get; set; }
        public string KomlnkPurchaseHeaders_Id { get; set; }

        public KomlinkCardReceiptGetModel[] komlinkCardReceiptGetModel { get; set; }
    }

    public class KomlinkCardCheckoutTopupRequestModel
    {
        public string KomlinkPurchaseHeaders_Id { get; set; }
        public string Currency_Id { get; set; }
        public decimal TotalAmount { get; set; }
        public string MCounters_Id { get; set; }
        public string Channel { get; set; }
        public string CounterUserId { get; set; }
        public string KomlinkCardNo { get; set; }
        public string PurchaseStationId { get; set; }


        public string FinancePaymentMethod { get; set; }
        public string ReferenceNo { get; set; }
        public string HostNo { get; set; } = string.Empty;
        public string InvoiceNo { get; set; } = string.Empty;
        public string CardId { get; set; } = string.Empty;

        public CreditCardResponse CreditCardResponseModel { get; set; }

        public PaymentGatewaySuccessPaidModel PaymentGatewaySuccessPaidModel { get; set; }
    }

    public class KomlinkCardUpdateTopupRequestModel
    {
        public string KomlinkPurchaseHeaders_Id { get; set; }
        public string CounterUserId { get; set; }
    }
    public class KomlinkCardCancelTopupRequestModel
    {
        public string KomlinkPurchaseHeaders_Id { get; set; }
        public string CounterUserId { get; set; }
    }

    public class MakePaymentUsingIM30RequestModel
    {
        public string TransactioNo { get; set; } 
        public decimal TotalAmount { get; set; }
    }

    public class PaymentGatewaySuccessPaidModel
    {
        ///<summary>PaymentGateway Header Related</summary>
        public string SalesTransactionNo { get; set; }
        ///<summary>Refer to KTMB.Library.Constants.Constants.PaymentGatewayHeaderStatus; PaymentGateway Header Related</summary>
        public string HeaderStatus { get; set; }

        ///<summary>Refer to KTMB.Library.Constants.Constants.PaymentGatewayDetailStatus; PaymentGateway Detail Related</summary>
        public string DetailStatus { get; set; }
        ///<summary>PaymentGateway Detail Related</summary>
        public string LineTag { get; set; }
        ///<summary>PaymentGateway Detail Related</summary>
        public string Remark { get; set; }
        ///<summary>PaymentGateway Detail Related</summary>
        public string TransObjectType { get; set; }
        ///<summary>PaymentGateway Detail Related</summary>
        public string TransObject { get; set; }
        ///<summary>PaymentGateway Detail Related</summary>
        public string User { get; set; }
    }

    public class CompleteTopUpKomlinkCardCCompiledResultModel
    {
       
        public KomlinkCardReceiptGetModel[] komlinkCardReceiptGetModel { get; set; }
    }

    [Serializable]
    public class KomlinkCardReceiptGetModel
    {
        public string TransactionType { get; set; }
        public bool HasNewCard { get; set; }
        public DateTime IssueDateTime { get; set; }
        public string KomlinkCardNumber { get; set; }
        public decimal NewKomlinkCardSalesAmount { get; set; }
        public bool HasSeasonPass { get; set; }
        public string SeasonPassType { get; set; }
        public string SeasonPassValidity { get; set; }
        public string SeasonPassServiceHeaderId { get; set; }
        public DateTime SeasonPassValidFrom { get; set; }
        public DateTime SeasonPassValidTo { get; set; }
        public string SeasonPassOrigin { get; set; }
        public string SeasonPassDestination { get; set; }
        public decimal SeasonPassAmount { get; set; }
        public bool HasTopUp { get; set; }
        public decimal TopUpAmount { get; set; }
        public decimal CardBalanceAmount { get; set; }
        public string PaymentMethod { get; set; }
        public decimal TotalAmount { get; set; }
        public string UserId { get; set; }
        public string CounterId { get; set; }
        public string TicketMessage { get; set; }
        public string CompanyRegNo { get; set; }
        public string CompanyAddress { get; set; }
        public string TransactionNo { get; set; }

        public bool IsRetryAttempt { get; set; }
    }

    //public class ServiceResult
    //{
    //    /// <summary>
    //    /// Id returned by service, if any.
    //    /// </summary>
    //    public Guid GuidId { get; set; }

    //    /// <summary>
    //    /// Id returned by service, if any.
    //    /// </summary>
    //    public string StringId { get; set; }

    //    /// <summary>
    //    /// Messages returned by service.
    //    /// </summary>
    //    public ServiceMessages Messages { get; }

    //    /// <summary>
    //    /// Other object returned by service, if any.
    //    /// </summary>
    //    public object Object { get; set; }

    //    /// <summary>
    //    /// Other objects returned by service, if any.
    //    /// </summary>
    //    public IEnumerable<object> Objects { get; set; }

    //    /// <summary>
    //    /// Status returned by service.
    //    /// </summary>
    //    public ServiceStatus Status { get; set; }

    //    //public ServiceResult()
    //    //{
    //    //    GuidId = Guid.Empty;
    //    //    StringId = string.Empty;
    //    //    Messages = new ServiceMessages();
    //    //    Status = ServiceStatus.None;
    //    //}
    //}

    //public class ServiceMessages
    //{
    //    public string Key { get; set; }
    //    public string Message { get; set; }
    //}

}
