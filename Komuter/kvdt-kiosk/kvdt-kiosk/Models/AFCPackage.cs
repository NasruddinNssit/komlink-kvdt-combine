using System.Collections.Generic;

namespace kvdt_kiosk.Models
{
    public class AFCPackage
    {
        public bool Status { get; set; }
        public List<string> Messages { get; set; }
        public object Code { get; set; }
        public List<Packages> Data { get; set; }
    }

    public class Packages
    {
        public string PackageId { get; set; }
        public string PackageName { get; set; }
        public string PackageName2 { get; set; }
        public string PackageJourney { get; set; }
    }
}
