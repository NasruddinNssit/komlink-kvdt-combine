using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Train.Kiosk.Common.Data.PostRequestParam
{
    [Serializable]
    public class TripRequest : IPostRequestParam
    {
        public string TrainService { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public DateTime DepartureDate { get; set; }
        public string Channel { get; set; }
        public string MCounters_Id { get; set; }
    }
}