using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Train.Kiosk.Common.Data
{
    [Serializable]
    public class SystemConfigurationModel
    {
        public string CompanyName { get; set; } = string.Empty;
        public string CompanyRegNo { get; set; } = string.Empty;
        public string CompanyAddress { get; set; } = string.Empty;
        public string TVMTicketMessage { get; set; } = string.Empty;

        /// <summary>
        /// Max Number of Pax for Komuter Kiosk 
        /// </summary>
        public int TVMKomTicket { get; set; } = 0;
        public int TVMShutTicket { get; set; } = 0;
        /// <summary>
        /// Max Number of Pax for ETS or Intercity
        /// </summary>
        public int TVMOtherTicket { get; set; } = 0;
    }
}
