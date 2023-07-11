using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyKadTest
{
    [Serializable]
    public class TicketTypeModel
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public bool IsDefault { get; set; } = false;

        public static TicketTypeModel Duplicate(TicketTypeModel ticketType)
        {
            return new TicketTypeModel() { Id = ticketType.Id, Description = ticketType.Description, IsDefault = ticketType.IsDefault };
        }
    }
}
