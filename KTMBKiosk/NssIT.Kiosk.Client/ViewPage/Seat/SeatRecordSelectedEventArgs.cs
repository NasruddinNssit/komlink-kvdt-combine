using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.ViewPage.Seat
{
    public class SeatRecordSelectedEventArgs : EventArgs
    {
        public int CoachIndex { get; private set; } = -1;
        public Guid SeatId { get; private set; } = Guid.Empty;

        public SeatRecordSelectedEventArgs(int coachIndex, Guid seatId)
        {
            CoachIndex = coachIndex;
            SeatId = seatId;
        }
    }
}
