using System;
using System.Windows.Controls;

namespace kvdt_kiosk.Views.Kvdt.ReturnJourney
{
    /// <summary>
    /// Interaction logic for ReturnJourneyScreen.xaml
    /// </summary>
    public partial class ReturnJourneyScreen : UserControl
    {
        public ReturnJourneyScreen()
        {
            InitializeComponent();

            TxtDate.Text = DateTime.Now.ToString("ddd dd - MM");

            LoadGridPassengerReturnJourney();
        }

        private void LoadGridPassengerReturnJourney()
        {
            GridJourneyPassenger.Children.Clear();
            PassengerReturnJourney passengerReturnJourney = new PassengerReturnJourney();
            GridJourneyPassenger.Children.Add(passengerReturnJourney);
        }

    }
}
