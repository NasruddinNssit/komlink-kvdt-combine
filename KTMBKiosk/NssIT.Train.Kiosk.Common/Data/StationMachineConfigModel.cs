using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Train.Kiosk.Common.Data
{
    [Serializable]
    public class StationMachineConfigModel
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public string MStates_Id { get; set; }
        public string ShuttleCheck { get; set; }
        public string KomuterCheck { get; set; }
        public string OthersCheck { get; set; }
        public string Status { get; set; }
    }
}
