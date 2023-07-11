using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Train.Kiosk.Common.Data
{
    [Serializable]
    public class IntercityETSTicketModel
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
        public decimal PayableAmount { get; set; }
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
        public string TransactionNo { get; set; }
        public string DepartReturn { get; set; }
        public string DepartReturnTicket { get; set; }
        public int GroupRowCount { get; set; }
        public decimal RefundAmount { get; set; }
        public decimal AddOnCharge { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal TotalCharge { get; set; }
        public string PaymentMethodDetails { get; set; }
        public string Reprint { get; set; }
        public string PurchaseChannel { get; set; }
        public string PhoneNo { get; set; }
        public string Email { get; set; }
        public string IsRefund { get; set; }
        public string DisplayName { get; set; }
        public string ServiceCategoriesCode { get; set; }
        public decimal TempRefund { get; set; }
        public Guid TripScheduleHeaders_Id { get; set; }
        public string PassengerContactNo { get; set; }
        public decimal AmendAmount { get; set; }
        public byte[] QRTicketData { get; set; }

        /// <summary>
        /// YesNo
        /// </summary>
        public string IsInsurance { get; set; }
        public bool IsInsuranceAvailable { get; set; }
        public string MInsuranceHeaders_Id { get; set; }
        public string MInsuranceHeaders_ShortDescription { get; set; }
        public decimal InsuranceAmount { get; set; }
        public decimal InsuranceTotal { get; set; }

        public IntercityETSTicketModel Duplicate()
        {
            return new IntercityETSTicketModel()
            {
                MCurrencies_Id = this.MCurrencies_Id,
                BookingNo = this.BookingNo,
                PaymentMethod = this.PaymentMethod,
                TripBookingHeaders_Id = this.TripBookingHeaders_Id,
                TotalAmount = this.TotalAmount,
                Status = this.Status,
                DepartureDateTime = this.DepartureDateTime,
                ArrivalDateTime = this.ArrivalDateTime,
                TicketNo = this.TicketNo,
                Journey = this.Journey,
                TicketMessage = this.TicketMessage,
                CompanyRegNo = this.CompanyRegNo,
                CompanyAddress = this.CompanyAddress,
                TicketTypes_Id = this.TicketTypes_Id,
                TicketPrice = this.TicketPrice,
                KomuterPackages_Id = this.KomuterPackages_Id,
                DepartureStationDescription = this.DepartureStationDescription,
                ArrivalStationDescription = this.ArrivalStationDescription,
                QRLink = this.QRLink,
                NumberOfTicket = this.NumberOfTicket,
                PurchaseCounterId = this.PurchaseCounterId,
                TripPaymentHeaders_Id = this.TripPaymentHeaders_Id,
                CreationId = this.CreationId,
                CreationDateTime = this.CreationDateTime,
                LastModifiedId = this.LastModifiedId,
                LastModifiedDateTime = this.LastModifiedDateTime,
                SeatStatus = this.SeatStatus,
                PassengerName = this.PassengerName,
                PassengerIC = this.PassengerIC,
                TrainNo = this.TrainNo,
                TrainLabel = this.TrainLabel,
                CoachLabel = this.CoachLabel,
                SeatNo = this.SeatNo,
                Departure = this.Departure,
                Destination = this.Destination,
                TransactionNo = this.TransactionNo,
                DepartReturn = this.DepartReturn,
                DepartReturnTicket = this.DepartReturnTicket,
                GroupRowCount = this.GroupRowCount,
                RefundAmount = this.RefundAmount,
                AddOnCharge = this.AddOnCharge,
                GrandTotal = this.GrandTotal,
                TotalCharge = this.TotalCharge,
                PaymentMethodDetails = this.PaymentMethodDetails,
                Reprint = this.Reprint,
                PurchaseChannel = this.PurchaseChannel,
                PhoneNo = this.PhoneNo,
                Email = this.Email,
                IsRefund = this.IsRefund,
                DisplayName = this.DisplayName,
                ServiceCategoriesCode = this.ServiceCategoriesCode,
                TempRefund = this.TempRefund,
                TripScheduleHeaders_Id = this.TripScheduleHeaders_Id,
                PassengerContactNo = this.PassengerContactNo,
                AmendAmount = this.AmendAmount,
                QRTicketData = this.QRTicketData,
                IsInsurance = this.IsInsurance,
                IsInsuranceAvailable = this.IsInsuranceAvailable,
                MInsuranceHeaders_Id = this.MInsuranceHeaders_Id,
                InsuranceAmount = this.InsuranceAmount,
                InsuranceTotal = this.InsuranceTotal
            };
        }
    }
}
