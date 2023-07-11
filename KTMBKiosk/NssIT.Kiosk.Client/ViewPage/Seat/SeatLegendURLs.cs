using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.ViewPage.Seat
{
    public class SeatLegendURLs
    {
        public string SeatSelectedIconURL { get; set; }
        public string SeatSoldIconURL { get; set; }
        public string SeatMaleIconURL { get; set; }
        public string SeatFemaleIconURL { get; set; }
        public string SeatReservedIconURL { get; set; }
        public string SeatBlockedIconURL { get; set; }

        public void Reset()
        {
            SeatSelectedIconURL = "";
            SeatSoldIconURL = "";
            SeatMaleIconURL = "";
            SeatFemaleIconURL = "";
            SeatReservedIconURL = "";
            SeatBlockedIconURL = "";
        }
    }
}
