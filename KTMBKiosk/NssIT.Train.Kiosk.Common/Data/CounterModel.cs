using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Train.Kiosk.Common.Data
{
    [Serializable]
    public class CounterModel
    {
        /// <summary>
        /// Station Id where kiosk has located
        /// </summary>
        public string StationId { get; set; } = string.Empty;

        /// <summary>
        /// Station Description where kiosk has located
        /// </summary>
        public string StationDescription { get; set; } = string.Empty;
    }
}
