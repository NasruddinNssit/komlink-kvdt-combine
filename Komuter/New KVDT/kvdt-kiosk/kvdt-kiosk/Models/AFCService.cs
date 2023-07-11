using System;
using System.Collections.Generic;

namespace kvdt_kiosk.Models
{
    public class AFCService
    {
        public bool Status { get; set; }
        public List<string> Messages { get; set; }
        public object Code { get; set; }
        public AFCServicesDetails Data { get; set; }
    }

    public class AFCServicesDetails
    {
        public string GuidId { get; set; }
        public string StringId { get; set; }
        public List<object> Messages { get; set; }
        public object Object { get; set; }
        public List<Object> Objects { get; set; }
        public int Status { get; set; }
    }

    public class Object
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public object AFCACGCode { get; set; }
        public int FirstSPNo { get; set; }
        public int SecondSPNo { get; set; }
        public object Status { get; set; }
        public object AddOnFrom_MStations_Id { get; set; }
        public object AddOnTo_MStations_Id { get; set; }
        public string URL { get; set; }
        public DateTime CreationDateTime { get; set; }
        public object CreationId { get; set; }
        public DateTime LastModifiedDateTime { get; set; }
        public object LastModifiedId { get; set; }
        public int Version { get; set; }
    }
}
