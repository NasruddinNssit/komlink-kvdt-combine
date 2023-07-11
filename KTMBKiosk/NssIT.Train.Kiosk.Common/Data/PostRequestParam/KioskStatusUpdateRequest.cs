using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Train.Kiosk.Common.Data.PostRequestParam
{
    [Serializable]
    public class KioskStatusUpdateRequest : IPostRequestParam
    {
        public string MachineId { get; set; }
        public bool IsCleanupExistingMachineStatus { get; set; } = false;
        public KioskLatestStatusModel[] LatestStatusList { get; set; }
    }
}