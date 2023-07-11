using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kvdt_kiosk.Models
{
    public class AFCBookingModel
    {
        public string MCounters_Id { get; set; }

        public string CounterUserId { get; set; }

        public string HandheldUserId { get; set; }

        public string PurchaseStationId { get; set; }

        public string Channel { get; set; }

        public string AFCServiceHeaders_Id { get; set; }

        public string PackageJourney { get; set; }

        public string From_MStations_Id { get; set; }

        public string To_MStations_Id { get; set; }

        public IList<AFCBookingDetailModel> AFCBookingDetailModels { get; set; }

        public IList<AFCBookingAddOnDetailModel> AFCBookingAddOnDetailModels { get; set; }

    }
}
