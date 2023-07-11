using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService.Sales
{
    [Serializable]
    public class CustSeatDetail
    {
        //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        // Enter at Seat
        /// <summary>
        /// Refer to Seat ID
        /// </summary>
        public Guid SeatLayoutModel_Id { get; set; } = Guid.Empty;
        public string SeatNo { get; set; }
        public string ServiceType { get; set; }
        public string SeatTypeDescription { get; set; }
        public decimal Price { get; set; } = 0.0M;
        public decimal Surcharge { get; set; } = 0.0M;
        //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

        //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        // Enter at Passenger Page
        public string CustName { get; set; }
        public string CustIC { get; set; }
        /// <summary>
        /// Optional
        /// </summary>
        public string Contact { get; set; }
        /// <summary>
        /// M: Male; F: Female
        /// </summary>
        public string Gender { get; set; }
        /// <summary>
        /// Seat Type Id Like Adult, Child or ets. This Id is Maintain in Master file
        /// </summary>
        public string TicketType { get; set; }
        public string DepartPromoCode { get; set; }
        public string ReturnPromoCode { get; set; }
        public string PNR { get; set; }
        //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

        //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        // Payment - Final Stage; After Passenger Info has submitted 
        public string Currency { get; set; } = null;
        public string TicketPriceFormat { get; set; } = null;

        /// <summary>
        /// Original Ticket Price(Include Price of Ticket Type)
        /// </summary>
        public decimal OriginalTicketPrice { get; set; } = 0M;

        public decimal PromoDiscountAmount { get; set; } = 0M;

        /// <summary>
        /// OriginalTicketPrice - PromoDiscountAmount
        /// </summary>
        public decimal TicketPrice { get; set; } = 0M;
        
        public decimal InsuranceAmount { get; set; } = 0m;

        /// <summary>
        /// Ticket amount after discount.
        /// </summary>
        /// 
        public decimal NetTicketPrice { get; set; }
        //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
    }
}