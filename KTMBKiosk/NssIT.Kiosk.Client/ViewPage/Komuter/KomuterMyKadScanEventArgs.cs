using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations.Model;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.ViewPage.Komuter
{
    public class KomuterMyKadScanEventArgs : EventArgs, IDisposable
    {
        private uscKomuterMyKad _myKad = null;
        private uscKomuterMyKadGroup _myKadGroup = null;

        public KomuterMyKadScanEventArgs(uscKomuterMyKad myKad)
        {
            _myKad = myKad;
        }

        public uscKomuterMyKad KomuterMyKad => _myKad;

        public uscKomuterMyKadGroup KomuterMyKadGroup => _myKadGroup;

        public void UpdateKomuterMyKadGroup(uscKomuterMyKadGroup myKadGroup) => _myKadGroup = myKadGroup;

        public void Dispose()
        {
            _myKad = null;
            _myKadGroup = null;
        }


    }
}
