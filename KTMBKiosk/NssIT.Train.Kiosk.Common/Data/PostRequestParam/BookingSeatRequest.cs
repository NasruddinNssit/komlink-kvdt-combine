using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Train.Kiosk.Common.Data.PostRequestParam
{
    [Serializable]
    public class BookingSeatRequest : IPostRequestParam
    {
        public string Booking_Id { get; set; }
        public string TrainSeatModel_Id { get; set; }
        public string MCounters_Id { get; set; }
        public string Channel { get; set; }
        public BookingSeatDetailModel[] BookingSeatDetailModels { get; set; } = new BookingSeatDetailModel[0];
    }
}
