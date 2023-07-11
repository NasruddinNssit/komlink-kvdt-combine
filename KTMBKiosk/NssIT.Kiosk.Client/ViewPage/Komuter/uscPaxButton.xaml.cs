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
    /// Interaction logic for uscPaxButton.xaml
    /// </summary>
    public partial class uscPaxButton : UserControl
    {
        private string _logChannel = "ViewPage";

        public event EventHandler<NoOfPaxChangedEventArgs> OnNoOfPaxChanged;

        private Brush _enabledBackgroundColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
        private Brush _enabledForegroundColor = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0x00, 0x00));

        private Brush _disabledBackgroundColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xCC, 0xCC, 0xCC));
        private Brush _disabledForegroundColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xEE, 0xEE, 0xEE));

        private Brush _selectedBackgroundColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xFB, 0xD0, 0x12));
        private Brush _selectedForegroundColor = new SolidColorBrush(Color.FromArgb(0xFF, 0x44, 0x44, 0x44));

        private int _noOfPax = 1;
        private ButtonState State { get; set; } = ButtonState.Disabled;

        public uscPaxButton()
        {
            InitializeComponent();
            Disabled();
        }

        private void Pax_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (State == ButtonState.Enabled)
                {
                    if (RaiseOnNoOfPaxChanged(NoOfPax) == true)
                    {
                        Selected();
                    }
                }
                else if (State == ButtonState.Selected)
                {
                    RaiseOnNoOfPaxChanged(0);
                    NormalEnabled();
                }
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg($@"(EXIT10001122); {ex.Message}");
                App.Log.LogError(_logChannel, "-", new Exception($@"(EXIT10001122);", ex), "EX01", "uscPaxButton.Pax_Click");
            }

            //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            bool RaiseOnNoOfPaxChanged(int noOfPax)
            {
                NoOfPaxChangedEventArgs arg = new NoOfPaxChangedEventArgs(noOfPax, agreeChanged: false);
                try
                {
                    OnNoOfPaxChanged?.Invoke(null, arg);
                }
                catch (Exception ex)
                {
                    App.ShowDebugMsg($@"Unhandled error exception; (EXIT10001121); {ex.Message}");
                    App.Log.LogError(_logChannel, "-", new Exception($@"Unhandled error exception; (EXIT10001121);", ex), "EX01", "uscPaxButton.RaiseOnNoOfPaxChanged");
                }

                return arg.AgreeChanged;
            }
        }

        public int NoOfPax
        {
            get => _noOfPax;
            set
            {
                _noOfPax = value;
                TxtNoOfPax.Text = _noOfPax.ToString();
            }
        }

        public void NormalEnabled()
        {
            BdPaxButton.Background = _enabledBackgroundColor;
            TxtNoOfPax.Foreground = _enabledForegroundColor;
            State = ButtonState.Enabled;
        }

        public void Disabled()
        {
            BdPaxButton.Background = _disabledBackgroundColor;
            TxtNoOfPax.Foreground = _disabledForegroundColor;
            State = ButtonState.Disabled;
        }

        private void Selected()
        {
            BdPaxButton.Background = _selectedBackgroundColor;
            TxtNoOfPax.Foreground = _selectedForegroundColor;
            State = ButtonState.Selected;
        }

        enum ButtonState
        {
            Disabled = 0,
            Enabled = 1,
            Selected = 2
        }
        
    }
}
