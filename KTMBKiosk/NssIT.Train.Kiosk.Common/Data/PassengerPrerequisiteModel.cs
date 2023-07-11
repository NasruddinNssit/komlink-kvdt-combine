using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//NssIT.Train.Kiosk.Common.Data.PassengerPrerequisiteInfo
namespace NssIT.Train.Kiosk.Common.Data
{
    [Serializable]
    public class PassengerPrerequisiteModel
    {
        public TicketTypeModel[] TicketTypeList { get; set; } = new TicketTypeModel[] { };
    }
}
