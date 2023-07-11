using kvdt_kiosk.Models;
using kvdt_kiosk.Services;
using Serilog;
using System;
using System.Windows;
using System.Windows.Controls;

namespace kvdt_kiosk.Views.PurchaseTicket
{
    /// <summary>
    /// Interaction logic for ViewMapScreen.xaml
    /// </summary>
    public partial class ViewMapScreen : UserControl
    {
        private readonly APIServices aPIServices = new APIServices(new APIURLServices(), new APISignatureServices());
        public ViewMapScreen()
        {
            InitializeComponent();
            LoadLanguage();

            LoadMap();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Window parentWindow = Window.GetWindow(this);
                parentWindow.Owner.Effect = null;
                parentWindow.Visibility = Visibility.Collapsed;
                parentWindow.Owner.Opacity = 1;
                parentWindow.Owner = null;
                parentWindow.Close();

                Log.Logger.Information(UserSession.SessionId + " - " + "Close Map Screen");
            }
            catch (Exception ex)
            {

                Log.Logger.Error(UserSession.SessionId + " - " + ex + " Error in ViewMapScreen.xaml.cs");
            }
        }

        private void LoadLanguage()
        {
            try
            {
                Dispatcher.InvokeAsync(() =>
                {
                    if (App.Language == "ms")
                    {
                        BtnClose.Content = "Tutup Peta";
                    }
                });
            }
            catch (Exception ex)
            {
                Log.Logger.Error(UserSession.SessionId + " - " + ex + " Error in ViewMapScreen.xaml.cs");
            }
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
            catch (Exception ex)
            {
                Log.Logger.Error(UserSession.SessionId + " - " + ex + " Error in ViewMapScreen.xaml.cs");
            }
        }
    }
}
