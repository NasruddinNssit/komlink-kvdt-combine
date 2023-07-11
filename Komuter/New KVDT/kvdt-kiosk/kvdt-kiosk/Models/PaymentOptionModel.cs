using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kvdt_kiosk.Models
{
    public class PaymentOptionModel
    {
        public string FinancePaymentMethod { get; set; }

        public decimal Amount { get; set; }

        public string LedgerAccount { get; set; }

        public string WarrantNo { get; set; }

    }
}
