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

namespace Komlink.Views
{
    /// <summary>
    /// Interaction logic for Pass.xaml
    /// </summary>
    public partial class Pass : UserControl
    {

        private readonly string passName;
        private readonly string startDate;
        private readonly string endDate;
        public Pass(string passName, string startDate, string endDate)
        {
            InitializeComponent();
            this.passName = passName;
            this.startDate = startDate;
            this.endDate = endDate;

            setText();
        }

        private void setText()
        {
            PassNameText.Text = passName;
            PassDateText.Text = startDate + " - " + endDate;
        }
    }
}
