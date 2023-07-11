using kvdt_kiosk.Models;
using kvdt_kiosk.Services;
using kvdt_kiosk.Views.Parcel;
using kvdt_kiosk.Views.PurchaseTicket;
using Serilog;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace kvdt_kiosk.Views
{
    /// <summary>
    /// Interaction logic for ChooseActionScreen.xaml
    /// </summary>
    public partial class ChooseActionScreen
    {
        private readonly APIServices aPIServices = new APIServices(new APIURLServices(), new APISignatureServices());
        public ChooseActionScreen()
        {
            InitializeComponent();

            LoadLanguage();

            LoadMap();
        }

        private void LoadLanguage()
        {
            try
            {
                Dispatcher.InvokeAsync(() =>
                {
                    if (App.Language == "ms")
                    {
                        TxtAction.Text = "Sila pilih menu anda";
                        lblParcel.Text = "     Pembelian\n   Parcel Sahaja";

                        lblPurchase.Text = "Pembelian\n     Tiket";
                    }
                });
            }
            catch (System.Exception ex)
            {
                Log.Logger.Error(ex, UserSession.SessionId + " Error in ChooseActionScreen.xaml.cs");
            }


        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            lblPurchase.Text = "Please Wait...";

            this.IsEnabled = false;
            await Task.Delay(250);
            this.IsEnabled = true;

            PurchaseTicketScreen purchaseTicketScreen = new PurchaseTicketScreen();
            Window.GetWindow(this).Content = purchaseTicketScreen;
            App._purchaseTicketScreen = purchaseTicketScreen;

            Log.Logger.Information("User selected Purchase Ticket");
        }

        private async void BtnParcel_Click(object sender, RoutedEventArgs e)
        {
            lblParcel.Text = "Please Wait...";

            this.IsEnabled = false;
            await Task.Delay(250);
            this.IsEnabled = true;

            Window.GetWindow(this).Content = new ParcelLayout();

            Log.Logger.Information("User selected Purchase Parcel");
        }

        private void LoadMap()
        {
            try
            {
                Dispatcher.InvokeAsync(async () =>
                {
                    var map = await aPIServices.GetAFCServices();
                    if (map != null)
                    {
                        foreach (var item in map.Data.Objects)
                        {
                            if (item.Id == UserSession.AFCService)
                            {
                                ImgMap.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(item.URL));
                            }
                        }
                    }
                });
            }
            catch (System.Exception ex)
            {
                Log.Logger.Error(ex, UserSession.SessionId + " Error in ChooseActionScreen.xaml.cs");
            }
        }
    }
}
