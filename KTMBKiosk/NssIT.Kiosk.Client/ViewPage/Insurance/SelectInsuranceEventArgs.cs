using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.ViewPage.Insurance
{
    public class SelectInsuranceEventArgs : EventArgs, IDisposable
    {
        public uscCoverageGroup SelectedInsurance { get; private set; }

        public SelectInsuranceEventArgs(uscCoverageGroup CoverageGroup)
        {
            SelectedInsurance = CoverageGroup;
        }

        public void Dispose()
        {
            SelectedInsurance = null;
        }


    }
}
