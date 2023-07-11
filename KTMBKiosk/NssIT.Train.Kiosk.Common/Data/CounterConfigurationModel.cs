using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Train.Kiosk.Common.Data
{
    [Serializable]
    public class CounterConfigurationModel
    {
        public SystemConfigurationModel SystemConfiguration { get; set; }
        public CounterModel CounterInfo { get; set; }
        public StationMachineConfigModel StationMachineSetting { get; set; }
    }
}
