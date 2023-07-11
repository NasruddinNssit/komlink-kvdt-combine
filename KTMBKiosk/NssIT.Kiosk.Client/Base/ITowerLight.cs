using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.Base
{
    public interface ITowerLight: IDisposable
    {
        void ShowAvailableState();
        void ShowBusyState();
        void ShowErrorState();
        void ShowErrorStateWithBlinking();
        void SwitchOff();
    }
}
