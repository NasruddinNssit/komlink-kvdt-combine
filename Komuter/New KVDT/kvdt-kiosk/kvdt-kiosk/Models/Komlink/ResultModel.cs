using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kvdt_kiosk.Models.Komlink
{
    public class ResultModel
    {
        public bool Status { get; set; }
        public IList<string> Messages { get; set; }

        public string Code { get; set; }

        public dynamic Data { get; set; }
    }
}
