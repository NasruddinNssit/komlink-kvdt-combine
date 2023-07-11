using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Device.PosiFlex.MotionSensor1.OrgAPI
{
    public class SensorMotionEventArgs : EventArgs
    {
        public int RangeValue { get; private set; } = 4500;
        public bool IsSomeoneNearBy { get; private set; } = false;

        public SensorMotionEventArgs(int rangeValue, bool isSomeoneNearBy)
        {
            RangeValue = rangeValue;
            IsSomeoneNearBy = isSomeoneNearBy;
        }
    }
}
