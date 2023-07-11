using kvdt_kiosk.Models;
using kvdt_kiosk.Services;
using kvdt_kiosk.Views.MyKad;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace kvdt_kiosk.Views.Passenger
{
    /// <summary>
    /// Interaction logic for PassengerMyKadScan.xaml
    /// </summary>
    public partial class PassengerMyKadScan
    {
        private readonly DispatcherTimer _updateTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
        private readonly APIServices aPIServices = new APIServices(new APIURLServices(), new APISignatureServices());

        public PassengerMyKadScan()
        {
            InitializeComponent();
            LoadPassengerMyKadScan();
            CheckIfBtnScanActive();
            BorderCSS();
        }

        private void BorderCSS()
        {
            var shadow = new System.Windows.Media.Effects.DropShadowEffect
            {
                Color = System.Windows.Media.Color.FromRgb(0, 0, 0),
                // Direction = 1500,
                ShadowDepth = 0,
                Opacity = 0.7,
                BlurRadius = 90

            };
            BdSubFrame2.Effect = shadow;
        }

        private void LoadPassengerMyKadScan()
        {
            SystemConfig.IsResetIdleTimer = true;

            var totalChild = UserSession.ChildSeat;
            var totalSenior = UserSession.SeniorSeat;
            var totalOKU = UserSession.OKUSeat;
            var isChildNeedVerification = UserSession.IsVerifyAgeAFCRequiredForChild;
            var isSeniorNeedVerification = UserSession.IsVerifyAgeAFCRequiredForSenior;
            var isOKUNeedVerification = UserSession.IsVerifyAgeAFCRequiredForOKU;

            if (isChildNeedVerification)
            {
                for (var i = 0; i < totalChild; i++)
                {
                    var childTextBox = new ChildTextBox();
                    childTextBox.TxtNo.Text = i + 1 + ".";

                    childTextBox.TxtPassengerName.Text = i + 1 + "";
                    childTextBox.TxtPassengerName.Visibility = Visibility.Collapsed;

                    GridChildren.Rows = totalChild;

                    PassengerInfo.ScanFor = "KANAK-KANAK / CHILD: " + UserSession.ChildSeat;
                    GridChildren.Children.Add(childTextBox);
                }
            }
            else
            {
                lblChild.Visibility = Visibility.Collapsed;
                GridChildren.Visibility = Visibility.Collapsed;
            }

            if (isSeniorNeedVerification)
            {
                for (var i = 0; i < totalSenior; i++)
                {
                    var seniorTextBox = new SeniorTextBox();
                    seniorTextBox.TxtNo.Text = i + 1 + ".";

                    seniorTextBox.TxtPassengerName.Text = i + 1 + "";
                    seniorTextBox.TxtPassengerName.Visibility = Visibility.Collapsed;

                    GridSenior.Rows = totalSenior;

                    PassengerInfo.ScanFor = "SENIOR CITIZEN / WARGA EMAS: " + UserSession.SeniorSeat;
                    GridSenior.Children.Add(seniorTextBox);
                }
            }
            else
            {
                lblSenior.Visibility = Visibility.Collapsed;
                GridSenior.Visibility = Visibility.Collapsed;
            }

            if (isOKUNeedVerification)
            {
                for (var i = 0; i < totalOKU; i++)
                {
                    var okuTextBox = new OKUTextBox();
                    okuTextBox.TxtNo.Text = i + 1 + ".";

                    okuTextBox.TxtPassengerName.Text = i + 1 + "";
                    okuTextBox.TxtPassengerName.Visibility = Visibility.Collapsed;

                    GridOKU.Rows = totalOKU;

                    PassengerInfo.ScanFor = "OKU CITIZEN / Orang Kurang Upaya (OKU): " + UserSession.SeniorSeat;
                    GridOKU.Children.Add(okuTextBox);
                }
            }
            else
            {
                lblOKU.Visibility = Visibility.Hidden;
                GridOKU.Visibility = Visibility.Hidden;
            }



            if (totalChild == 0 || isChildNeedVerification == false)
            {
                lblChild.Visibility = Visibility.Collapsed;
                GridChildren.Visibility = Visibility.Collapsed;
            }
            else
            {
                lblChild.Visibility = Visibility.Visible;
                GridChildren.Visibility = Visibility.Visible;
            }

            if (totalSenior == 0 || isSeniorNeedVerification == false)
            {
                lblSenior.Visibility = Visibility.Collapsed;
                GridSenior.Visibility = Visibility.Collapsed;
            }
            else
            {
                lblSenior.Visibility = Visibility.Visible;
                GridSenior.Visibility = Visibility.Visible;
            }

            if (totalOKU == 0 || isOKUNeedVerification == false)
            {
                lblOKU.Visibility = Visibility.Collapsed;
                GridOKU.Visibility = Visibility.Collapsed;
            }
            else
            {
                lblOKU.Visibility = Visibility.Visible;
                GridOKU.Visibility = Visibility.Visible;
            }

        }

        private void BtnScan_Click(object sender, RoutedEventArgs e)
        {
            MyKadValidationScreen myKadValidationScreen = new MyKadValidationScreen();

            SystemConfig.IsResetIdleTimer = true;
            PassengerInfo.IsBtnScanScanning = true;

            var window = new Window
            {
                WindowStyle = WindowStyle.None,
                Height = 350,
                Width = 800,
                ResizeMode = ResizeMode.NoResize,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Content = new MyKad.MyKadValidationScreen()
            };
            window.ShowDialog();

        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            FrmSubFrame2.Content = null;
            BdSubFrame2.Visibility = Visibility.Collapsed;
            BdSubFrame2.Width = 0;
            PassengerInfo.IsBtnScanScanning = false;

            SystemConfig.IsResetIdleTimer = true;
        }

        private void PassengerMyKadScan_OnLoaded(object sender, RoutedEventArgs e)
        {
            //PassengerInfo.IsBtnScanScanning = true;
            //BtnScan.Content = "SCANNING...";

            //var window = new Window
            //{
            //    WindowStyle = WindowStyle.None,
            //    Height = 350,
            //    Width = 800,
            //    WindowStartupLocation = WindowStartupLocation.CenterScreen,
            //    Content = new MyKad.MyKadValidationScreen()
            //};
            //window.ShowDialog();


        }

        public async Task ChangeButtonToScanningAsync(bool isClicked)
        {
            while (!isClicked)
            {
                BtnScan.Content = "SCAN MYKAD";
            }

            await Task.Delay(50);

            BtnScan.Content = "Scanning...";
            BtnScan.Style = (Style)FindResource("BtnSelected");
        }

        private void CheckIfBtnScanActive()
        {
            _updateTimer.Tick += (sender, args) =>
            {
                if (PassengerInfo.IsBtnScanScanning)
                {
                    BtnScan.Content = "SCANNING...";
                    BtnScan.Style = (Style)FindResource("BtnSelected");
                }
                else
                {
                    BtnScan.Content = "SCAN MYKAD";
                }

                if (UserSession.ChildSeat != 0 && UserSession.SeniorSeat != 0)
                {
                    BtnReset.IsEnabled = false;
                }
                else
                {
                    BtnReset.IsEnabled = true;
                }
            };

            _updateTimer.Start();

        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            SystemConfig.IsResetIdleTimer = true;

            UserSession.ChildSeat = UserSession.TempDataForChildSeat;
            UserSession.SeniorSeat = UserSession.TempDataForSeniorSeat;
            UserSession.OKUSeat = UserSession.TempDataForOKUSeat;

            PassengerInfo.CurrentScanNumberForChild = 0;
            PassengerInfo.CurrentScanNumberForSenior = 0;
            PassengerInfo.ICNumber = null;
            PassengerInfo.PassengerName = null;
            PassengerInfo.IcScanned.Clear();

            UserSession.TicketOrderTypes = null;
            UserSession.UserAddons = null;

            foreach (var child in GridChildren.Children)
            {
                if (child is ChildTextBox childTextBox)
                {
                    childTextBox.TxtPassengerName.Text = childTextBox.TxtNo.Text.Replace(".", "");
                    childTextBox.TxtPassengerName.Visibility = Visibility.Collapsed;
                }
            }

            foreach (var senior in GridSenior.Children)
            {
                if (senior is SeniorTextBox seniorTextBox)
                {
                    seniorTextBox.TxtPassengerName.Text = seniorTextBox.TxtNo.Text.Replace(".", "");
                    seniorTextBox.TxtPassengerName.Visibility = Visibility.Collapsed;
                }
            }

            foreach (var oku in GridOKU.Children)
            {
                if (oku is OKUTextBox okuTextBox)
                {
                    okuTextBox.TxtPassengerName.Text = okuTextBox.TxtNo.Text.Replace(".", "");
                    okuTextBox.TxtPassengerName.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void Grid_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            SystemConfig.IsResetIdleTimer = true;
        }
    }
}
