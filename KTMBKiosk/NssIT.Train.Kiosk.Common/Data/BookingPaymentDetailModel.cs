using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Train.Kiosk.Common.Data
{
    [Serializable]
    public class BookingPaymentDetailModel
    {
        public decimal Amount { get; set; }

        /// <summary>
        /// Refer to NssIT.Train.Kiosk.Common.Constants.FinancePaymentMethod
        /// </summary>
        public string FinancePaymentMethod { get; set; }

        /// <summary>
        /// Reference Number return from bank
        /// </summary>
        public string ReferenceNo { get; set; }
    }
}
