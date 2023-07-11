using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Train.Kiosk.Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kvdt_kiosk.Models
{
    public class AFCPaymentModel
    {
        public string Booking_Id { get; set; }
        public string MCurrencies_Id { get; set; }
        public decimal TotalAmount { get; set; }
        public string MCounters_Id { get; set; }
        public string Channel { get; set; }
        public string CounterUserId { get; set; }

        public string HandheldUserId { get; set; }
        public string PurchaseStationId { get; set; }

        public IList<BookingPaymentDetailModel> BookingPaymentDetailModels { get; set; } = new List<BookingPaymentDetailModel>();
        public CreditCardResponse CreditCardResponseModel { get; set; }

        public PaymentGatewaySuccessPaidModel PaymentGatewaySuccessPaidModel { get; set; }

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
}
