using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Train.Kiosk.Common.Data
{
    [Serializable]
    public class KomuterPrintTicketModel
    {
        public string MCurrencies_Id { get; set; }
        public string BookingNo { get; set; }
        public string PaymentMethod { get; set; }
        public Guid TripBookingHeaders_Id { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public DateTime DepartureDateTime { get; set; }
        public DateTime ArrivalDateTime { get; set; }
        public string TicketNo { get; set; }
        public string Journey { get; set; }
        public string TicketMessage { get; set; }
        public string CompanyRegNo { get; set; }
        public string CompanyAddress { get; set; }
        public string TicketTypes_Id { get; set; }
        public decimal TicketPrice { get; set; }
        public string KomuterPackages_Id { get; set; }
        public string DepartureStationDescription { get; set; }
        public string ArrivalStationDescription { get; set; }
        public string QRLink { get; set; }
        public int NumberOfTicket { get; set; }
        public string PurchaseCounterId { get; set; }
        public Guid TripPaymentHeaders_Id { get; set; }
        public string CreationId { get; set; }
        public DateTime CreationDateTime { get; set; }
        public string LastModifiedId { get; set; }
        public DateTime LastModifiedDateTime { get; set; }
        public string SeatStatus { get; set; }
        public string PassengerName { get; set; }
        public string PassengerIC { get; set; }
        public string TrainNo { get; set; }
        public string TrainLabel { get; set; }
        public string CoachLabel { get; set; }
        public string SeatNo { get; set; }
        public string Departure { get; set; }
        public string Destination { get; set; }
    }
}
