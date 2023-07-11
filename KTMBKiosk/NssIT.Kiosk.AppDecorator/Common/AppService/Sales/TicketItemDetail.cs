using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService.Sales
{
    [Serializable]
    public class TicketItemDetail
    {
        public string TicketTypeId { get; set; } = null;
        public string Name { get; set; } = null;
        public string MyKadId { get; set; } = null;
    }
}