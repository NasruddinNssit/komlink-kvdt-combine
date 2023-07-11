using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Train.Kiosk.Common.Data.PostRequestParam
{
    [Serializable]
    public class GetKomuterPackageRequest : IPostRequestParam
    {
        public string CounterId { get; set; }
        public string OriginStationId { get; set; }
        public string DestinationStationId { get; set; }
    }
}