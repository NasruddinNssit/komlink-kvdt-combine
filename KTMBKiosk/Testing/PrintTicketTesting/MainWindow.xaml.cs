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

namespace PrintTicketTesting
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private LibShowMessageWindow.MessageWindow _msg = null;
        public MainWindow()
        {
            InitializeComponent();

            _msg = new LibShowMessageWindow.MessageWindow();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void TestPrintTicket_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //NssIT.Kiosk.Client.Reports.PDFTools.AdobeReaderFullFilePath = TxtAdobe.Text;

                //NssIT.Kiosk.Client.Reports.PDFTools.PrintPDFsX2(TxtPDFFileLocation.Text, "-", this.Dispatcher);
            }
            catch (Exception ex)
            {
                _msg.ShowMessage(ex.ToString());
            }
        }        
    }
}
