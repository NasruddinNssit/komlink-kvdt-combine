using kvdt_kiosk.Models;
using kvdt_kiosk.Services;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace kvdt_kiosk.Views.Kvdt.MyKad
{
    /// <summary>
    /// Interaction logic for MyKadValidationScreen.xaml
    /// </summary>
    public partial class MyKadValidationScreen : UserControl
    {
        public MyKadValidationScreen()
        {
            InitializeComponent();

            ScanWaitTimer();
            CheckDataThread();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);
            parentWindow.Effect = null;
            parentWindow.Opacity = 1;
            parentWindow.Close();
        }


        private void ScanWaitTimer()
        {

            int count = 30;
            System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += (sender, e) =>
            {
                if (count == 0)
                {
                    dispatcherTimer.Stop();

                    Window parentWindow = Window.GetWindow(this);
                    parentWindow.Close();
                    parentWindow.Effect = null;
                    parentWindow.Opacity = 1;

                    //close ReturnJourneyPassengerWindow

                }
                else
                {
                    count--;
                    TxtCount.Text = count.ToString();
                }
            };

            dispatcherTimer.Interval = new System.TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Stop();
        }

        private async Task<string> RetrievingMyKadInfoAsync()
        {
            APIServices aPIServices = new APIServices(new APIURLServices(), new APISignatureServices());

            string myKadInfo = await aPIServices.GetMyKadInfo();


            if (myKadInfo.Contains("Boolean ReadFP)-"))
            {
                return null;
            }

            if (myKadInfo.Contains("SCardAccessMode"))
            {
                return null;
            }

            if (!string.IsNullOrEmpty(myKadInfo))
            {
                //take after hypen only - to get passenger name
                if (myKadInfo.Contains("-"))
                {
                    var passengerName = myKadInfo.Substring(myKadInfo.IndexOf("-") + 1);
                    PassengerInfo.PassengerName = passengerName;

                    TxtInstruction.Text = "My Kad Scan Successfully";
                    TxtInstruction.Foreground = System.Windows.Media.Brushes.Green;
                    TxtInstruction.FontWeight = FontWeights.Bold;
                    BtnClose.Visibility = Visibility.Hidden;

                    TxtScanText.Text = "Passenger Name: " + PassengerInfo.PassengerName;

                    await Task.Delay(3000);
                }

                //take before hypen only - to get passenger ic number
                if (myKadInfo.Contains("-"))
                {
                    var icNumber = myKadInfo.Substring(0, myKadInfo.IndexOf("-"));
                    PassengerInfo.PassengerICNumber = icNumber;
                }

                return myKadInfo;
            }

            return null;
        }

        private void CheckDataThread()
        {
            System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();

            dispatcherTimer.Tick += async (sender, e) =>
            {
                string myKadInfo = await RetrievingMyKadInfoAsync();
                if (myKadInfo == null || myKadInfo.Contains("SCardAccessMode"))
                {
                    return;
                }

                dispatcherTimer.Stop();

                CloseWindow();
            };
            dispatcherTimer.Interval = new System.TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
        }

        void CloseWindow()
        {
            Window parentWindow = Window.GetWindow(this);
            parentWindow.Close();
            parentWindow.Effect = null;
            parentWindow.Opacity = 1;
        }
    }
}
