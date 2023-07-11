using NssIT.Train.Kiosk.Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.ViewPage.Seat
{

    public class UnSelectSeatEventArgs : EventArgs
    {
        public SeatLayoutModel Seat { get; private set; } = null;
        public bool AgreeUnSelection { get; set; } = true;

        public UnSelectSeatEventArgs(SeatLayoutModel seat)
        {
            Seat = seat;
        }
    }
}
