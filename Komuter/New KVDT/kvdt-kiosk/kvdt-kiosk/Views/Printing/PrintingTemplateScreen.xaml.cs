using kvdt_kiosk.Models;
using kvdt_kiosk.Views.Welcome;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace kvdt_kiosk.Views.Printing
{
    /// <summary>
    /// Interaction logic for PrintingTemplateScreen.xaml
    /// </summary>
    public partial class PrintingTemplateScreen : UserControl
    {

        public event EventHandler OnPrintReceipt;
        public bool IsPrintingDone { get; set; } = false;
        public PrintingTemplateScreen()
        {
            InitializeComponent();
            LoadLanguage();
        }

        private void LoadLanguage()
        {
            if (App.Language == "ms")
            {
                lblPrinting.Text = "CETAK TIKET ANDA";
                lblTransNo.Text = "No Rujukan: " + UserSession.SessionId;
                lblThankYou.Text = "Terima Kasih Dan Selamat Jalan";
                lblNiceDay.Text = "Semoga Hari Yang Menyenangkan";

                BtnContact.Content = "HUBUNGI BANTUAN";
                BtnPrint.Content = "CETAK RESIT";
            }
            //lblTransNo.Text = "Reference No: " + UserSession.SessionId;
        }


        public void UpdatePaymentComplete(string transactionNo)
        {
            lblTransNo.Text = transactionNo;
        }

        private void DisplayMessageAsync()
        {
            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = new System.TimeSpan(0, 0, 1)
            };

            timer.Tick += async (sender, args) =>
            {
                if (UserSession.IsPrintingDone == false)
                {
                    BtnPrint.Content = "PLEASE WAIT ...";
                    if (App.Language.Equals("ms"))
                    {
                        BtnPrint.Content = "SILA TUNGGU ...";
                    }
                    BtnPrint.IsEnabled = false;

                    lblThankYou.Text = "";
                    lblNiceDay.Text = "";

                    timer.Stop();

                    await Task.Delay(10000);

                    lblPrinting.Text = "Take Your Ticket";
                    lblThankYou.Text = "Thank You And Have A";
                    lblNiceDay.Text = "Nice Day";

                    timer = null;

                    BtnPrint.Content = "PRINT RECEIPT";
                    if (App.Language.Equals("ms"))
                    {
                        BtnPrint.Content = "CETAK RESIT";
                    }
                    BtnPrint.IsEnabled = true;

                    await Task.Delay(10000);
                    BtnPrint.Content = "PRINT RECEIPT";
                    BtnPrint.IsEnabled = true;

                    if (IsPrintingDone)
                    {
                        await Task.Delay(5000);

                        App.komlinkCardDetailScreen = null;
                        App._purchaseTicketScreen = null;
                        App._parcelLayout = null;

                        
                        var welcomeScreen = new WelcomeScreen();
                        var window = Window.GetWindow(this);

                        window.Content = welcomeScreen;
                    }

                    if (!IsPrintingDone)
                    {
                        await Task.Delay(8000);

                        App.komlinkCardDetailScreen = null;
                        App._purchaseTicketScreen = null;
                        App._parcelLayout = null;

                        
                        var welcomeScreen = new WelcomeScreen();
                        var window = Window.GetWindow(this);

                        window.Content = welcomeScreen;
                    }
                }

            };

            timer.Start();
        }

        private void PrintingTemplateScreen_OnLoaded(object sender, RoutedEventArgs e)
        {
            TextAnimation();
            DisplayMessageAsync();
        }

        private void TextAnimation()
        {
            DoubleAnimation doubleAnimation = new DoubleAnimation();
            doubleAnimation.From = 1;

            doubleAnimation.To = 0.1;
            doubleAnimation.AutoReverse = true;
            doubleAnimation.RepeatBehavior = RepeatBehavior.Forever;
            doubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(1));

            lblPrinting.BeginAnimation(TextBlock.OpacityProperty, doubleAnimation);
        }

        //private void BtnPrint_Click(object sender, RoutedEventArgs e)
        //{
        //    OnPrintReceipt.Invoke(null, null);
        //}

        private async void BtnPrint_Click_1(object sender, RoutedEventArgs e)
        {
            BtnPrint.Content = "PRINTING...";
            if (App.Language.Equals("ms"))
            {
                BtnPrint.Content = "MENCETAK ...";
            }
            await Task.Delay(500);

            OnPrintReceipt.Invoke(null, null);

            BtnPrint.Content = "PRINTED";
            if (App.Language.Equals("ms"))
            {
                BtnPrint.Content = "DICETAK ...";
            }

            IsPrintingDone = true;

            App.komlinkCardDetailScreen = null;
            App._purchaseTicketScreen = null;
            App._parcelLayout = null;

            await Task.Delay(8000);
            var welcomeScreen = new WelcomeScreen();
            
            var window = Window.GetWindow(this);

            window.Content = welcomeScreen;
        }
    }
}
