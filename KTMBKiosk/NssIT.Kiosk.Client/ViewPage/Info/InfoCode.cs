using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.ViewPage.Info
{
    public enum InfoCode
    {
        OriginInfo = 0,
        DestinationInfo = 5,
        TravelDateInfo = 10,

        DepartTripInfo = 15,
        DepartSeatInfo = 20,
        DepartPickupNDrop = 25,

        ReturnTripInfo = 35,
        ReturnSeatInfo = 40,
        ReturnPickupNDrop = 45,

        PassengerInfo = 60

    }
}
