using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfBoostTouchNGoSample.Data
{
    public class SaleCustomFieldInfo
    {
        /// <summary>
        /// An Id that used to identify a machine in network, like SignalR Connection Id.
        /// </summary>
        public string MachineNetworkID { get; set; }

        /// <summary>
        /// An Id that used to identify a machine in database.
        /// </summary>
        public string MachineID { get; set; }

        /// <summary>
        /// Use to record new transaction time, like new sale transaction time.
        /// </summary>
        public DateTime CreationTime { get; set; } = DateTime.Now;

        /// <summary>
        /// Transaction amount.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Number like Booking No., Transaction No., Invoice No., and etc.
        /// </summary>
        public string DocumentNo { get; set; }
    }
}
