using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Train.Kiosk.Common.Data
{
    [Serializable]
    public class SeatLayoutModel
    {
        public Guid Id { get; set; }
        public Guid CoachDetailId { get; set; }
        public int SeatIndex { get; set; }
        public string SeatNo { get; set; }
        public string Status { get; set; }
        public string Gender { get; set; }
        public decimal Price { get; set; }
        public decimal Surcharge { get; set; }
        public string ServiceType { get; set; }
        public string SeatTypeDescription { get; set; }
        public string IconURL { get; set; }
        public string IsOKU { get; set; }
        public string PriceFormat { get; set; }

        public Guid? SurchargeType { get; set; }
        public string SeatType { get; set; }
        public Guid TrainCoachTypes_Id { get; set; }

        public Guid? TripBookingDetails_Id { get; set; }
    }
}
