using NssIT.Train.Kiosk.Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.ViewPage.Seat
{
    public class CoachSelectedEventArgs : EventArgs
    {
        public CoachModel CoachData { get; private set; } = null;

        public int CoachControlIndex { get; private set; } = -1;

        public CoachSelectedEventArgs(CoachModel coachData, int coachControlIndex)
        {
            CoachData = coachData;
            CoachControlIndex = coachControlIndex;
        }
    }
}
