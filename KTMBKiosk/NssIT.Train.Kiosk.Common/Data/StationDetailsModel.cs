using NssIT.Kiosk.AppDecorator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Train.Kiosk.Common.Data
{
    [Serializable]
    public class StationDetailsModel
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public string State { get; set; }
        public string Region { get; set; }
        public string Country { get; set; }
        public string Currency { get; set; }
        public IList<string> TrainService { get; set; } = new List<string>();
    }
}