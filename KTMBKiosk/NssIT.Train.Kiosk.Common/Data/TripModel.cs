using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Train.Kiosk.Common.Data
{
    [Serializable]
    public class TripModel
    {
        public string Id { get; set; }
        public string TrainNo { get; set; }
        public string ServiceCategory { get; set; }
        public string DepartDateFormat { get; set; }
        public string DepartTimeFormat { get; set; }
        public int ArrivalDayOffset { get; set; }
        public string ArrivalTimeFormat { get; set; }
        public string Currency { get; set; }
        public string PriceFormat { get; set; }
        public int SeatAvailable { get; set; }
        public DateTime DepartLocalDateTime { get; set; }
        public DateTime ArrivalLocalDateTime { get; set; }
        public decimal Price { get; set; }
    }
}