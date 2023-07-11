using Komlink.Models;
using Komlink.Services;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Komlink.Views.Komlink
{
    /// <summary>
    /// Interaction logic for KomlinkCardScreen.xaml
    /// </summary>
    public partial class KomlinkCardScanScreen : UserControl
    {

        ExitButton exitButton;
        BigAlert bigAlert;
        public KomlinkCardScanScreen()
        {
            InitializeComponent();
            exitButton = new ExitButton();
            ExitButtonGrid.Children.Add(exitButton);
            //LoadAlert();
            //InitializeUserComponent();

            //UserSession.CreateSession += CreateSession;

            //this.Focus();
            //this.KeyDown += User;
        }

    

        private void User(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Enter)
            {
                KomlinkCardDetailModel komlinkCardDetails = GetKomlinkCardDetail();

                UserSession.CreateSessionAction(komlinkCardDetails);
            }
        }

        private void CreateSession(KomlinkCardDetailModel details)
        {



            if (details != null)
            {

                KomlinkCardDetailScreen komlinkCardDetailScreen = new KomlinkCardDetailScreen();
                Window.GetWindow(this).Content = komlinkCardDetailScreen;

                //if (details.TicketType.Equals("Adult"))
                //{
                //    ValidationScreen validationScreen = new ValidationScreen();
                //    Window.GetWindow(this).Content = validationScreen;
                //}
                //else
                //{
                //    KomlinkCardDetailScreen komlinkCardDetailScreen = new KomlinkCardDetailScreen(details);
                //    Window.GetWindow(this).Content = komlinkCardDetailScreen;
                //}
            }

        }

        private Models.KomlinkCardDetailModel GetKomlinkCardDetail()
        {
            APIServices aPIServices = new APIServices(new APIURLServices(), new APISignatureServices());
            var result = aPIServices.GetKomlinkCardDetail().Result;

            return result?.Data;
        }



    }
}
