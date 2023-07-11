using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService.Sales
{
    [Serializable]
    public class TicketItem 
    {
        public string TicketTypeId { get; set; } = null;
        public int Quantity { get; set; } = 0;
        public TicketItemDetail[] DetailList { get; set; } = null;
    }
}
