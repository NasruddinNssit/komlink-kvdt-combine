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
    /// Interaction logic for SeatingTitle.xaml
    /// </summary>
    public partial class SeatingTitleUc : UserControl
    {
        public SeatingTitleUc()
        {
            InitializeComponent();

             DateTime dateTime = DateTime.Now;

            SeatingDateNow.Text = dateTime.ToString("ddd, dd-MM");
        }

        public string Text
        {
            get { return SeatingTitleText.Text; }

            set { SeatingTitleText.Text = value; }
        }
    }
}
