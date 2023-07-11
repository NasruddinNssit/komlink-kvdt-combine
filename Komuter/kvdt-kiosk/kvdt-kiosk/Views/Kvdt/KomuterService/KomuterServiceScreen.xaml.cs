using kvdt_kiosk.Services;
using System.Windows.Controls;

namespace kvdt_kiosk.Views.Kvdt.KomuterService
{
    /// <summary>
    /// Interaction logic for KomuterServiceScreen.xaml
    /// </summary>
    public partial class KomuterServiceScreen : UserControl
    {
        public KomuterServiceScreen()
        {
            InitializeComponent();
            LoadService();
        }

        //load all service
        private void LoadService()
        {
            APIServices aPIServices = new APIServices(new APIURLServices(), new APISignatureServices());
            var result = aPIServices.GetAFCServices().Result;


            if (result?.Data?.Objects?.Count > 0)
            {
                foreach (var item in result.Data.Objects)
                {
                    AFCServiceButton btn = new AFCServiceButton();

                    btn.txtServiceName.Text = item.Description;

                    btn.Margin = new System.Windows.Thickness(10, 10, 10, 10);
                    btn.Padding = new System.Windows.Thickness(10, 10, 10, 10);
                    btn.txtServiceName.FontSize = 35;

                    btn.Height = 150;

                    GridAFC.Height = GridAFC.Height + btn.Height + 10;

                    GridAFC.Children.Add(btn);
                }
            }

        }
    }
}
