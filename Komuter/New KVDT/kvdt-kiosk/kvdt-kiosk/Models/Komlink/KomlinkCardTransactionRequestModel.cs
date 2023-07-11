using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kvdt_kiosk.Models.Komlink
{
    
    public class KomlinkCardTransactionRequesModel
    {
        public string KomlinkCardDetails { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
    }

}
