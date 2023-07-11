using kvdt_kiosk.Services;
using System.Windows;
using System.Windows.Controls;

namespace kvdt_kiosk.Views.Kvdt
{
    /// <summary>
    /// Interaction logic for LanguageScreen.xaml
    /// </summary>
    public partial class LanguageScreen : UserControl
    {
        public LanguageScreen()
        {
            InitializeComponent();
        }

        private void BtnEnglish_Click(object sender, RoutedEventArgs e)
        {
            ChooseActionScreen chooseActionScreen = new ChooseActionScreen();

            MyDispatcher.Invoke(() =>
            {
                this.Content = chooseActionScreen;
            });
        }
    }
}
