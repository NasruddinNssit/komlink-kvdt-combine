using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kvdt_kiosk.Models
{
    public class AFCTicketPrintModel
    {
        public string MCurrencies_Id { get; set; }
        public string BookingNo { get; set; }
        public string TransactionNo { get; set; }
        public string PaymentMethod { get; set; }
        public Guid TripKomuterBookingHeaders_Id { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public DateTime TicketEffectiveFromDate { get; set; }
        public DateTime TicketEffectiveToDate { get; set; }
        public string TicketNo { get; set; }
        public string Journey { get; set; }
        public string TicketMessage { get; set; }
        public string CompanyRegNo { get; set; }
        public string CompanyAddress { get; set; }
        public string TicketTypes_Id { get; set; }
        public string AFCAddOns_Id { get; set; }
        public string IsAddOn { get; set; }
        public decimal TicketPrice { get; set; }
        public string KomuterPackages_Id { get; set; }
        public string From_MStations_Id { get; set; }
        public string To_MStations_Id { get; set; }
        public string QRLink { get; set; }
        public int NumberOfTicket { get; set; }
        public string PurchaseCounterId { get; set; }
        public Guid TripPaymentHeaders_Id { get; set; }
        public string CreationId { get; set; }
        public DateTime CreationDateTime { get; set; }
        public string LastModifiedId { get; set; }
        public DateTime LastModifiedDateTime { get; set; }
        public string SeatStatus { get; set; }
        public decimal RefundAmount { get; set; }
        public string PurchaseChannel { get; set; }
        public string KomuterPackagesDescription { get; set; }
        public string KomuterPackagesDescription2 { get; set; }
        public string IsTravelDateExtend { get; set; }
        public string PassengerIC { get; set; }
        public string PassengerName { get; set; }
        public string DepartureStationDescription { get; set; }
        public string ArrivalStationDescription { get; set; }
        public string PaymentMethodDetails { get; set; }
        public byte[] QRTicketData { get; set; }

    }

    public class AfcReceiptModel
    {
        public string CompanyAddress { get; set; }

        public string CompanyRegNo { get; set; }
        public string TicketMessage { get; set; }
        public string DepartureStationDescription { get; set; }
        public string ArrivalStationDescription { get; set; }

        public DateTime TicketEffectiveFromDate { get; set; }

        public string BookingNo { get; set; }

        public int NumberOfTicket { get; set; }

        public string TicketId { get; set; }
        public string PaymentMethod { get; set; }  

        public string TotalAmount { get; set; }

        public string CreationID { get; set; }
        public DateTime CreationDate { get; set;}
    }
}
