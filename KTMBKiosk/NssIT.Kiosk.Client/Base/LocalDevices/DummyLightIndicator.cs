using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.Base.LocalDevices
{
    public class DummyLightIndicator : ITowerLight
    {
        public void Dispose() { }
        public void ShowAvailableState() { }
        public void ShowBusyState() { }
        public void ShowErrorState() { }
        public void ShowErrorStateWithBlinking() { }
        public void SwitchOff() { }
    }
}
