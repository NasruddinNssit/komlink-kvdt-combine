using System.Drawing.Printing;
using System.Windows;
using System.Windows.Controls;

namespace kvdt_kiosk.Views.Kvdt.Printer
{
    /// <summary>
    /// Interaction logic for PrinterScreen.xaml
    /// </summary>
    public partial class PrinterScreen : UserControl
    {
        public PrinterScreen()
        {
            InitializeComponent();
        }

        private void BtnCheck_Click(object sender, RoutedEventArgs e)
        {
            foreach (string printer in PrinterSettings.InstalledPrinters)
            {
                TxtPrinterList.AppendText(printer + "\n");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string printerName = TxtPrinterName.Text;

            if (string.IsNullOrEmpty(printerName))
            {
                MessageBox.Show("Please enter printer name");
                return;
            }

            if (PrinterSettings.InstalledPrinters.Count == 0)
            {
                MessageBox.Show("No printer installed");
                return;
            }

            PrinterSettings printerSettings = new PrinterSettings();
            printerSettings.PrinterName = printerName;

            MessageBox.Show("Printer set to " + printerName);

            //print
            PrintDocument printDocument = new PrintDocument();
            printDocument.PrinterSettings = printerSettings;

            //print what ever in TxtPrinterList
            printDocument.PrintPage += PrintDocument_PrintPage;

            printDocument.Print();
        }

        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            string ticketInfo = "Ticket Number: 123\nDestination: Kuala Lumpur\nDate: 01/01/2022\nTime: 10:00 AM";

            e.Graphics.DrawString(ticketInfo, new System.Drawing.Font("Arial", 12, System.Drawing.FontStyle.Regular), System.Drawing.Brushes.Black, 10, 10);
        }

        private void BtnSetDefault_Click(object sender, RoutedEventArgs e)
        {
            string printerName = TxtPrinterName.Text;

            if (string.IsNullOrEmpty(printerName))
            {
                MessageBox.Show("Please enter printer name");
                return;
            }

            if (PrinterSettings.InstalledPrinters.Count == 0)
            {
                MessageBox.Show("No printer installed");
                return;
            }


            PrinterSettings printerSettings = new PrinterSettings();
            printerSettings.PrinterName = printerName;

            MessageBox.Show("Printer set to " + printerName);
        }
    }
}
