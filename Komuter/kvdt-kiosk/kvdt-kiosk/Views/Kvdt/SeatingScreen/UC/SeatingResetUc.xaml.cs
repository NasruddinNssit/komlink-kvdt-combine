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

namespace kvdt_kiosk.Views.Kvdt.SeatingScreen
{
    /// <summary>
    /// Interaction logic for MainTop2.xaml
    /// </summary>
    /// 
    
    public partial class SeatingResetUc : UserControl
    {

        public event EventHandler ResetButtonClicked;
        public SeatingResetUc()
        {
            InitializeComponent();
        }


        private void Reset_Clicked(object sender, RoutedEventArgs e)
        {
            ResetButtonClicked?.Invoke(this, EventArgs.Empty);
        }

    }
}
