using kvdt_kiosk.Models;
using kvdt_kiosk.Views.Kvdt.MyKad;
using kvdt_kiosk.Views.Windows;
using System.Windows;
using System.Windows.Controls;

namespace kvdt_kiosk.Views.Kvdt.ReturnJourney
{
    /// <summary>
    /// Interaction logic for PassengerReturnJourney.xaml
    /// </summary>
    public partial class PassengerReturnJourney : UserControl
    {
        public PassengerReturnJourney()
        {
            InitializeComponent();


            GetChildPessangerInfo();

            GetSeniorPessangerInfo();
        }

        private void GetChildPessangerInfo()
        {
            var totalChildSeat = UserSession.ChildSeat;

            for (int i = 0; i < totalChildSeat; i++)
            {
                ChildPassengerInfoTextbox passengerInfoTextbox = new ChildPassengerInfoTextbox();

                passengerInfoTextbox.Name = "TxtChild" + i;
                passengerInfoTextbox.TxtPassengerName.Text = "Child Passenger " + (i + 1);
                GridChildPessangerInfo.Children.Add(passengerInfoTextbox);
            }

            GridChildPessangerInfo.Rows = totalChildSeat;
        }

        private void GetSeniorPessangerInfo()
        {
            var totalSeniorSeat = UserSession.SeniorSeat;

            for (int i = 0; i < totalSeniorSeat; i++)
            {
                SeniorPassengerInfoTextbox passengerInfoTextbox = new SeniorPassengerInfoTextbox();

                passengerInfoTextbox.Name = "TxtSenior" + i;
                passengerInfoTextbox.TxtPassengerName.Text = "Senior Passenger " + (i + 1);

                GridSeniorPessangerInfo.Children.Add(passengerInfoTextbox);
            }

            GridSeniorPessangerInfo.Rows = totalSeniorSeat;
        }

        private void BtnCancel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            UserSession.ChildSeat = 0;
            UserSession.SeniorSeat = 0;
            UserSession.AdultSeat = 0;
            PassengerInfo.PassengerName = "";

            Window returnJourneyPassenger = Window.GetWindow(this);
            returnJourneyPassenger.Effect = null;
            returnJourneyPassenger.Opacity = 1;
            returnJourneyPassenger.Close();
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            Window returnJourneyWindow = Window.GetWindow(this);
            Window mainwindow = Window.GetWindow(returnJourneyWindow.Owner);
            mainwindow.Owner.Effect = null;
            mainwindow.Owner.Opacity = 1;

            Window returnJourneyPassenger = Window.GetWindow(this);
            returnJourneyPassenger.Owner.Effect = null;
            returnJourneyPassenger.Owner.Opacity = 1;

            returnJourneyPassenger.Close();

            returnJourneyWindow.Close();

            //show parcel
            ParcelWindows parcelWindows = new ParcelWindows();

            parcelWindows.Height = 1025;
            parcelWindows.Width = 800;

            parcelWindows.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            //parcelWindows.WindowStyle = WindowStyle.None;

            parcelWindows.ShowDialog();
        }

        private void BtnScan_Click(object sender, RoutedEventArgs e)
        {
            MyKadValidationWindow myKadValidationWindow = new MyKadValidationWindow();
            MyKadValidationScreen myKadValidationScreen = new MyKadValidationScreen();
            BtnScan.Style = (Style)FindResource("BtnSelected");

            myKadValidationWindow.Owner = Window.GetWindow(this);
            myKadValidationWindow.Content = myKadValidationScreen;
            myKadValidationWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            foreach (ChildPassengerInfoTextbox childPassengerInfoTextbox in GridChildPessangerInfo.Children)
            {
                if (childPassengerInfoTextbox.TxtPassengerName.Text == "Child Passenger 1")
                {
                    myKadValidationScreen.TxtScanText.Text = "SCAN MY KAD FOR: " + childPassengerInfoTextbox.TxtPassengerName.Text;
                    break;
                }

                if (childPassengerInfoTextbox.TxtPassengerName.Text == "Child Passenger 2")
                {
                    myKadValidationScreen.TxtScanText.Text = "SCAN MY KAD FOR: " + childPassengerInfoTextbox.TxtPassengerName.Text;
                    break;
                }

                if (childPassengerInfoTextbox.TxtPassengerName.Text == "Child Passenger 3")
                {
                    myKadValidationScreen.TxtScanText.Text = "SCAN MY KAD FOR: " + childPassengerInfoTextbox.TxtPassengerName.Text;
                    break;
                }

                if (childPassengerInfoTextbox.TxtPassengerName.Text == "Child Passenger 4")
                {
                    myKadValidationScreen.TxtScanText.Text = "SCAN MY KAD FOR: " + childPassengerInfoTextbox.TxtPassengerName.Text;
                    break;
                }

                if (childPassengerInfoTextbox.TxtPassengerName.Text == "Child Passenger 5")
                {
                    myKadValidationScreen.TxtScanText.Text = "SCAN MY KAD FOR: " + childPassengerInfoTextbox.TxtPassengerName.Text;
                    break;
                }

                if (childPassengerInfoTextbox.TxtPassengerName.Text == "Child Passenger 6")
                {
                    myKadValidationScreen.TxtScanText.Text = "SCAN MY KAD FOR: " + childPassengerInfoTextbox.TxtPassengerName.Text;
                    break;
                }

            }

            myKadValidationWindow.ShowDialog();
        }
    }
}
