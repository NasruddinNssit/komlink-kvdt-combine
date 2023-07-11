using NssIT.Train.Kiosk.Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.ViewPage.Komuter
{
    public class JourneyTypeChangeEventArgs : EventArgs, IDisposable 
    {
        public KomuterPackageModel KomuterPackage { get; private set; }

        public bool AgreeChanged { get; set; } = false;

        public JourneyTypeChangeEventArgs(KomuterPackageModel komuterPackage, bool agreeChanged)
        {
            KomuterPackage = komuterPackage;
            AgreeChanged = agreeChanged;
        }

        public void Dispose()
        {
            KomuterPackage = null;
        }
    }
}