using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Train.Kiosk.Common.Data.PostRequestParam
{
    [Serializable]
    public class KomuterTicketBookingRequest : IPostRequestParam
    {
        public string Channel { get; set; }

        public string MCounter_Id { get; set; }

        //string UserId

        public string OriginStationId { get; set; }

        public string DestinationStationId { get; set; }

        public string DestinationStationName { get; set; }

        public string OriginStationName { get; set; }

        public string KomuterPackageId { get; set; }

        public KomuterNewBookingTicketListModel[] KomuterNewBookingTicketList { get; set; }
    }
}
