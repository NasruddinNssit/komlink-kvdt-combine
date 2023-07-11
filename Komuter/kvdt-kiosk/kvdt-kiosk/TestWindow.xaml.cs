using System.IO.Ports;
using System.Linq;
using System.Net.NetworkInformation;
using System.Windows;

namespace kvdt_kiosk
{
    /// <summary>
    /// Interaction logic for TestWindow.xaml
    /// </summary>
    public partial class TestWindow : Window
    {
        public TestWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }


        string GetMachineId()
        {
            var macAddresses = NetworkInterface.GetAllNetworkInterfaces()
                .Where(nic => nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .Select(nic => nic.GetPhysicalAddress().ToString());

            return string.Join("|", macAddresses);
        }



        public static string GetMacAddress()
        {
            string macAddresses = string.Empty;
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    macAddresses += nic.GetPhysicalAddress().ToString();
                    break;
                }
            }
            return macAddresses;
        }

        private void GetACommPorts()
        {

            //check usb port
            string[] ports = SerialPort.GetPortNames();
            foreach (string port in ports)
            {
                TxtRichTextBox.AppendText(port + "\n");
            }

            //check com port
            //for (int i = 1; i < 10; i++)
            //{
            //    string port = "COM" + i;
            //    TxtRichTextBox.AppendText(port + "\n");
            //}



        }

        private void BtnTest_Click(object sender, RoutedEventArgs e)
        {
            GetACommPorts();
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            TxtRichTextBox.Document.Blocks.Clear();
        }
    }
}
