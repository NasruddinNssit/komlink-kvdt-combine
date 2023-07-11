using kvdt_kiosk.Models;
using kvdt_kiosk.Services;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace kvdt_kiosk.Views.MyKad
{
    /// <summary>
    /// Interaction logic for MyKadValidationScreen.xaml
    /// </summary>
    public partial class MyKadValidationScreen : IDisposable
    {
        private readonly DispatcherTimer scanTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        public string IcNumber { get; set; }
        public int Count { get; set; } = 30;
        public int Age { get; set; }
        public bool StartNewScan { get; set; } = false;

        private readonly APIServices apiServices = new APIServices(new APIURLServices(), new APISignatureServices());

        public MyKadValidationScreen()
        {
            InitializeComponent();

            ReadMyKad();

            BlinkingVerifyButton();
        }

        private void ReadMyKad()
        {
            TxtCount.Text = Count.ToString();

            if (Count < 10)
            {
                TxtCount.Foreground = new SolidColorBrush(Colors.Red);
            }

            scanTimer.Tick += (sender, args) =>
            {
                if (Count == 0)
                {
                    scanTimer.Stop();
                    CloseWindow();
                }
                else
                {
                    Count--;
                    TxtCount.Text = Count.ToString();
                    if (Count < 10)
                    {
                        TxtCount.Foreground = new SolidColorBrush(Colors.Red);
                    }
                }
            };

            scanTimer.Start();
        }


        private void BtnClose_OnClick(object sender, RoutedEventArgs e)
        {
            CloseWindow();
        }

        private void CloseWindow()
        {
            var window = Window.GetWindow(this);
            PassengerInfo.IsBtnScanScanning = false;
            window?.Close();
        }

        public void Dispose()
        {

        }

        private async void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            scanTimer.Stop();

            PassengerInfo.ICNumber = IcNumber;
            PassengerInfo.Age = Age;
            PassengerInfo.PassengerName = TxtScanText.Text;

            if (PassengerInfo.PassengerType == "Child")
            {
                PassengerInfo.CurrentScanNumberForChild = PassengerInfo.CurrentScanNumberForChild + 1;
                UserSession.ChildSeat = UserSession.ChildSeat - 1;
            }

            if (PassengerInfo.PassengerType == "Senior")
            {
                PassengerInfo.CurrentScanNumberForSenior = PassengerInfo.CurrentScanNumberForSenior + 1;
                UserSession.SeniorSeat = UserSession.SeniorSeat - 1;
            }

            if (PassengerInfo.PassengerType == "Adult")
            {
                PassengerInfo.CurrentScanNumberForAdult = PassengerInfo.CurrentScanNumberForAdult + 1;
                // UserSession.SeniorSeat = UserSession.SeniorSeat - 1;
            }

            if (UserSession.ChildSeat == 0 && UserSession.SeniorSeat == 0)
            {
                CloseWindow();
            }

            TxtInstruction.Text = "Please insert MyKad into the reader";
            TxtInstruction.Foreground = System.Windows.Media.Brushes.Black;

            await Task.Delay(50);

            var newCount = 10;
            BtnOK.Visibility = Visibility.Collapsed;
            BtnClose.Visibility = Visibility.Visible;

            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            timer.Tick += async (sender1, args) =>
            {

                newCount--;
                TxtInstruction.Text = "Please insert MyKad into the reader. Next scan will start in " + newCount + " seconds";
                TxtInstruction.Foreground = Brushes.Black;

                if (newCount <= 5)
                {
                    TxtInstruction.Foreground = Brushes.Red;
                }

                if (newCount != 0) return;
                timer.Stop();
                TxtInstruction.Text = "Please insert MyKad into the reader";
                TxtInstruction.Foreground = Brushes.Black;
                await Task.Delay(1000);
                scanTimer.Start();
            };

            timer.Start();

        }

        public async void ReadDataFromMyKad()
        {
            var result = await apiServices.GetMyKadInfo();

            if (result.Contains("Error") || result.Contains("99") || result.Contains("error"))
            {
                TxtInstruction.Text = "Status: MyKad Not Found";
                TxtInstruction.Text += Environment.NewLine;
                TxtInstruction.Text += "Click close button, then insert MyKad and start validate again";
                TxtInstruction.Foreground = Brushes.Red;

                BtnClose.Visibility = Visibility.Visible;
                BtnStatus.Visibility = Visibility.Collapsed;
                TxtScanText.Text = "Card Not Found";
                scanTimer.Stop();
                return;
            }
            else
            {
                IcNumber = result.Substring(0, result.IndexOf("-", StringComparison.Ordinal));

                var DOB = IcNumber.Substring(0, 6);

                var year = DOB.Substring(0, 2);

                if (Convert.ToInt32(year) >= 0 && Convert.ToInt32(year) <= 18)
                {
                    year = "20" + year;
                }
                else
                {
                    year = "19" + year;
                }


                Age = year.Length == 4 ? DateTime.Now.Year - Convert.ToInt32(year) : 0;
                PassengerInfo.Age = Age;
            }

            if (Age <= PassengerInfo.MaxChildAge)
            {
                PassengerInfo.PassengerType = "Child";

                lblPassengerInfo.Text = "KANAK-KANAK / CHILD";
                lblPassengerInfo.Foreground = new SolidColorBrush(Color.FromArgb(255, 44, 119, 232));
                lblPassengerInfo.FontWeight = FontWeights.Bold;

                var count = PassengerInfo.CurrentScanNumberForChild + 1;

                TxtScanText.Text = "SCAN MYKAD: Passenger " + count;
            }

            if (Age >= PassengerInfo.MinSeniorAge && Age <= PassengerInfo.MaxSeniorAge)
            {
                PassengerInfo.PassengerType = "Senior";

                lblPassengerInfo.Text = "WARGA EMAS / SENIOR";
                lblPassengerInfo.Foreground = new SolidColorBrush(Color.FromArgb(255, 44, 119, 232));
                lblPassengerInfo.FontWeight = FontWeights.Bold;

                var count = PassengerInfo.CurrentScanNumberForSenior + 1;

                TxtScanText.Text = "SCAN MYKAD: Passenger " + count;
            }

            if (Age > PassengerInfo.MaxChildAge && Age < PassengerInfo.MinSeniorAge)
            {
                PassengerInfo.PassengerType = "Adult";

                lblPassengerInfo.Text = "DEWASA / ADULT";
                lblPassengerInfo.Foreground = new SolidColorBrush(Color.FromArgb(255, 44, 119, 232));
                lblPassengerInfo.FontWeight = FontWeights.Bold;

                var count = PassengerInfo.CurrentScanNumberForAdult + 1;

                TxtScanText.Text = "SCAN MYKAD: Passenger " + count;
            }

            foreach (var ic in PassengerInfo.IcScanned)
            {
                if (ic == IcNumber)
                {
                    TxtInstruction.Text = "MyKad already scanned. Please click close button and start a new scan";
                    TxtInstruction.Foreground = Brushes.Red;

                    BtnClose.Visibility = Visibility.Visible;
                    BtnStatus.Visibility = Visibility.Collapsed;
                    scanTimer.Stop();

                    return;
                }
            }

            if (Age > PassengerInfo.MaxChildAge && Age < PassengerInfo.MinSeniorAge)
            {
                PassengerInfo.PassengerType = "Adult";

                //TxtInstruction.Text = "MyKad scanned is not eligible for this promotion either Child or Senior. Take out the card and please scan again";
                //TxtInstruction.Foreground = Brushes.Red;

                //BtnClose.Visibility = Visibility.Visible;
                //BtnStatus.Visibility = Visibility.Collapsed;

                //scanTimer.Stop();

                //return;

                if ((UserSession.SeniorSeat == 0 && UserSession.ChildSeat != 0) && (PassengerInfo.PassengerType == "Adult" || PassengerInfo.PassengerType == "Senior"))
                {
                    TxtInstruction.Text = "KANAK-KANAK / CHILD ticket only available for age " + PassengerInfo.MinChildAge + " to " + PassengerInfo.MaxChildAge + " years old.";
                    TxtInstruction.Foreground = Brushes.Red;

                    scanTimer.Stop();
                    BtnClose.Visibility = Visibility.Visible;
                    BtnStatus.Visibility = Visibility.Collapsed;

                    return;
                }

                if ((UserSession.SeniorSeat != 0 && UserSession.ChildSeat == 0) && (PassengerInfo.PassengerType == "Adult" || PassengerInfo.PassengerType == "Child"))
                {
                    TxtInstruction.Text = "WARGA EMAS / SENIOR ticket only available for age " + PassengerInfo.MinSeniorAge + " to " + PassengerInfo.MaxSeniorAge + " years old.";
                    TxtInstruction.Foreground = Brushes.Red;

                    scanTimer.Stop();
                    BtnClose.Visibility = Visibility.Visible;
                    BtnStatus.Visibility = Visibility.Collapsed;

                    return;
                }
            }

            if ((UserSession.SeniorSeat == 0 && UserSession.ChildSeat != 0) && (PassengerInfo.PassengerType == "Adult" || PassengerInfo.PassengerType == "Senior"))
            {
                TxtInstruction.Text = "KANAK-KANAK / CHILD ticket only available for age " + PassengerInfo.MinChildAge + " to " + PassengerInfo.MaxChildAge + " years old.";
                TxtInstruction.Foreground = Brushes.Red;

                scanTimer.Stop();
                BtnClose.Visibility = Visibility.Visible;
                BtnStatus.Visibility = Visibility.Collapsed;

                return;
            }

            if ((UserSession.SeniorSeat != 0 && UserSession.ChildSeat == 0) && (PassengerInfo.PassengerType == "Adult" || PassengerInfo.PassengerType == "Child"))
            {
                TxtInstruction.Text = "WARGA EMAS / SENIOR ticket only available for age " + PassengerInfo.MinSeniorAge + " to " + PassengerInfo.MaxSeniorAge + " years old.";
                TxtInstruction.Foreground = Brushes.Red;

                scanTimer.Stop();
                BtnClose.Visibility = Visibility.Visible;
                BtnStatus.Visibility = Visibility.Collapsed;

                return;
            }

            BtnStatus.Content = "Next Scan";
            BtnStatus.Style = (Style)FindResource("BtnOk");

            TxtInstruction.Text = "Scan successful";
            TxtInstruction.Text += Environment.NewLine;
            TxtInstruction.Text += "Take out your MyKad and insert next MyKad";
            TxtInstruction.Text += Environment.NewLine;
            TxtInstruction.Text += "Click OK button to scan next passenger";

            TxtInstruction.Foreground = System.Windows.Media.Brushes.Green;

            BtnClose.Visibility = Visibility.Collapsed;
            BtnStatus.Visibility = Visibility.Visible;
            BtnStatus.Content = "OK";
            BtnStatus.Style = (Style)FindResource("BtnYellow");
            BtnStatus.Foreground = Brushes.Black;

            TxtScanText.Text = result.Substring(result.IndexOf("-", StringComparison.Ordinal) + 1);

            PassengerInfo.ICNumber = IcNumber;
            PassengerInfo.PassengerName = TxtScanText.Text;
            PassengerInfo.IcScanned.Add(IcNumber);

            TxtScanText.Background = new SolidColorBrush(Color.FromArgb(255, 251, 208, 18));
            TxtScanText.Padding = new Thickness(10);

            scanTimer.Stop();

            if (PassengerInfo.PassengerType == "Child")
            {
                PassengerInfo.CurrentScanNumberForChild = PassengerInfo.CurrentScanNumberForChild + 1;
                UserSession.ChildSeat = UserSession.ChildSeat - 1;
            }

            if (PassengerInfo.PassengerType == "Adult")
            {
                PassengerInfo.CurrentScanNumberForAdult = PassengerInfo.CurrentScanNumberForAdult + 1;
                // UserSession.ChildSeat = UserSession.ChildSeat - 1;
            }

            if (PassengerInfo.PassengerType == "Senior")
            {
                PassengerInfo.CurrentScanNumberForSenior = PassengerInfo.CurrentScanNumberForSenior + 1;
                UserSession.SeniorSeat = UserSession.SeniorSeat - 1;
            }

            StartNewScan = true;

            if (UserSession.ChildSeat == 0 && UserSession.SeniorSeat == 0)
            {
                CloseWindow();
            }

        }

        private async void BtnStatus_Click(object sender, RoutedEventArgs e)
        {
            RemoveBlinkingVerifyButton();

            if (StartNewScan == true)
            {
                Count = 30;
                scanTimer.Start();
            }

            StartNewScan = false;
            BtnStatus.Foreground = Brushes.White;

            BtnStatus.Content = "Verifying...";
            BtnStatus.Style = (Style)FindResource("BtnBlue");

            TxtScanText.Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            TxtScanText.Padding = new Thickness(0);
            TxtScanText.Text = "Scanning In Progress...";
            TxtInstruction.Text = "Please wait...";
            TxtInstruction.Foreground = Brushes.Black;

            await Task.Delay(50);
            ReadDataFromMyKad();
        }

        private void BlinkingVerifyButton()
        {
            var blink = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = new Duration(TimeSpan.FromSeconds(1)),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };

            BtnStatus.BeginAnimation(OpacityProperty, blink);
        }

        private void RemoveBlinkingVerifyButton()
        {
            BtnStatus.BeginAnimation(OpacityProperty, null);
        }
    }
}