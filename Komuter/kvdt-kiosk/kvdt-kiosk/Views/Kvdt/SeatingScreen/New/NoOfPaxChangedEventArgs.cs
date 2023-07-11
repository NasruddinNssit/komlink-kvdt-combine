using System;

namespace kvdt_kiosk.Views.SeatingScreen.New.Kvdt
{
    public class NoOfPaxChangedEventArgs : EventArgs
    {
        public int NoOfPax { get; set; }
        public bool AgreeChanged { get; set; } = false;

        public NoOfPaxChangedEventArgs(int noOfPax, bool agreeChanged)
        {
            NoOfPax = noOfPax;
            AgreeChanged = agreeChanged;
        }
    }
}
