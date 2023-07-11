using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Komlink.Models
{
    public class KomlinkTransaction
    {
        public bool Status { get; set; }
        public IList<string> Messages { get; set; }
        public object Code { get; set; }
        public List<KomlinkTransactionDetail> Data { get; set; }
    }

    public class KomlinkTransactionDetail
    {
        public DateTime TransactionDateTime { get; set; }
        public string Station { get; set; }
        public decimal TotalAmount { get; set; }
        public string TransactionType { get; set; }
        public string TicketType { get; set; }

    }

    public class KomlinkTransactionDetailItem
    {
        public String Date { get; set; }
        public string Time { get;set; }
        public string Station { get; set; }
        public string TotalAmount { get; set; }
        public string TransactionType { get; set; }
        public string TicketType { get; set; }

    }


}
