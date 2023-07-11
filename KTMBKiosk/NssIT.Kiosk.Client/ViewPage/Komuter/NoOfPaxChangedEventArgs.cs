using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.ViewPage.Komuter
{
    public class NoOfPaxChangedEventArgs : EventArgs
    {
        public int NoOfPax { get; private set; }
        public bool AgreeChanged { get; set; } = false;

        public NoOfPaxChangedEventArgs(int noOfPax, bool agreeChanged)
        {
            NoOfPax = noOfPax;
            AgreeChanged = agreeChanged;
        }
    }
}