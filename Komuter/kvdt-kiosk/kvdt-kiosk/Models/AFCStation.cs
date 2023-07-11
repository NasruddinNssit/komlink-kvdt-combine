using System.Collections.Generic;

namespace kvdt_kiosk.Models
{
    public class AFCStation
    {
        public List<AFCStationDetails> AFCStationModels { get; set; }
        public List<AFCRouteModel> AFCRouteModels { get; set; }
    }

    public class AFCStationDetails
    {
        public string Id { get; set; }
        public string Station { get; set; }
        public string State { get; set; }
        public string Region { get; set; }
        public string Country { get; set; }
        public string ColorCode { get; set; }
        public string ApplyToDestination { get; set; }
        public string ApplyToOrigin { get; set; }
        public bool IsInterchange { get; set; }
        public List<string> RouteId { get; set; }
    }

    public class AFCRouteModel
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public string ColorCode { get; set; }
        public string IsInterchange { get; set; }
    }
}
