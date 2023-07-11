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

namespace NssIT.Kiosk.Client.ViewPage.CustInfo
{
    /// <summary>
    /// Interaction logic for uscTicketType.xaml
    /// </summary>
    public partial class uscTicketType : UserControl
    {
        private const string LogChannel = "ViewPage";

        public event EventHandler<TicketTypeChangeEventArgs> OnTicketTypeChange;

        private static System.Windows.Media.Brush _deActivatedFontColor = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0x77, 0x77, 0x77));
        //private static System.Windows.Media.Brush _activatedFontColor = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0x64, 0x64, 0x64));
        private static System.Windows.Media.Brush _activatedFontColor = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));

        private static System.Windows.Media.Brush _unSelectedBackGroundColor = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0xE4, 0xE4, 0xE4));
        private static System.Windows.Media.Brush _selectedBackGroundColor = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0x2B, 0x9C, 0xDB));

        public string _ticketTypeId = null;
        public uscTicketType()
        {
            InitializeComponent();
        }

        private void BdTicketType_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                OnTicketTypeChange.Invoke(this, new TicketTypeChangeEventArgs(TicketTypeId, TicketTypeDescription));
            }
            catch (Exception ex)
            {
                App.Log.LogError(LogChannel, "-", ex, "EX01", "uscTicketType.BdTicketType_MouseLeftButtonUp");
            }
        }

        public void Select()
        {
            BdTicketType.Background = _selectedBackGroundColor;
            TxtTicketTypeDesc.Foreground = _activatedFontColor;
        }

        public void UnSelect()
        {
            BdTicketType.Background = _unSelectedBackGroundColor;
            TxtTicketTypeDesc.Foreground = _deActivatedFontColor;
        }

        public string TicketTypeDescription
        {
            get => TxtTicketTypeDesc.Text;
            set => TxtTicketTypeDesc.Text = value ?? "";
        }
        public string TicketTypeId
        {
            get => _ticketTypeId;
            set => _ticketTypeId = value;
        }
    }
}
