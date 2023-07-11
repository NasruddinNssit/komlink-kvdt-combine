using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.ViewPage.Komuter
{
    public class StationSelectedEventArgs : EventArgs
    {
        public string StationName { get; private set; }
        /// <summary>
        /// Station Id Refer to database
        /// </summary>
        public string StationId { get; private set; }

        public StationSelectedEventArgs(string stationId, string stationName)
        {
            StationId = stationId;
            StationName = stationName;
        }
    }
}
