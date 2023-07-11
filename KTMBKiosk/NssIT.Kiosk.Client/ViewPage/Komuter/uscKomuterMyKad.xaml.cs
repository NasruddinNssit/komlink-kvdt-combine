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

namespace NssIT.Kiosk.Client.ViewPage.Komuter
{
    /// <summary>
    /// Interaction logic for uscKomuterMyKad.xaml
    /// </summary>
    public partial class uscKomuterMyKad : UserControl
    {
        private string _logChannel = "ViewPage";

        public event EventHandler<KomuterMyKadScanEventArgs> OnScanMyKad;

        private Brush _selectedBorderColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xFE, 0x4C, 0x70));

        private bool _isSelected = false;
        private string _myKadId = null;
        private Guid _entryId = Guid.NewGuid();
        private ResourceDictionary _langResource = null;

        public uscKomuterMyKad()
        {
            InitializeComponent();
            TxtName.Text = "";
            TxtLineNo.Text = "";
        }

        public Guid EntryId => _entryId;

        private void ScanMyKad_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Init(_langResource);
                OnScanMyKad?.Invoke(this, new KomuterMyKadScanEventArgs(this));
            }
            catch (Exception ex)
            {
                App.Log.LogText(_logChannel, "*", ex, "EX01", "uscKomuterMyKad.ScanMyKad_Click", AppDecorator.Log.MessageType.Error);
            }
        }

        public void SelectEntry()
        {
            BdContainer.BorderBrush = _selectedBorderColor;
            TxtName.Focus();
        }

        public void DeselecteEntry()
        {
            BdContainer.BorderBrush = null;
        }

        public void GetIdentity(out string name, out string idNo)
        {
            name = null;
            idNo = null;

            if ((string.IsNullOrWhiteSpace(TxtName.Text) == false) && (string.IsNullOrWhiteSpace(_myKadId) == false))
            {
                name = TxtName.Text;
                idNo = _myKadId;
            }

            return;
            //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            
        }

        public void UpdateIdentity(string name, string idNo, string lineNo = null)
        {
            TxtName.Text = name;
            _myKadId = idNo;
        }

        

        public void Init(ResourceDictionary langResource, string lineNo = null)
        {
            _langResource = langResource;
            TxtName.Text = "";

            if (string.IsNullOrWhiteSpace(lineNo) == false)
                TxtLineNo.Text = lineNo.Trim();

            if (_langResource != null)
                TxtScanTag.Text = _langResource["SCAN_MYKAD_Label"].ToString();

            _myKadId = null;
            DeselecteEntry();
        }

        public string GetLineNo()
        {
            return TxtLineNo.Text;
        }
    }
}
