using NssIT.Train.Kiosk.Common.Data;
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
    /// Interaction logic for uscJourneyType.xaml
    /// </summary>
    public partial class uscJourneyTypeButton : UserControl
    {
        private string _logChannel = "ViewPage";
        public event EventHandler<JourneyTypeChangeEventArgs> OnJourneyTypeChanged;

        private Brush _normalBackgroundColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xCC, 0xCC, 0xCC));
        private Brush _normalForegroundColor = new SolidColorBrush(Color.FromArgb(0xFF, 0x44, 0x44, 0x44));

        private Brush _selectedBackgroundColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xFB, 0xD0, 0x12));
        private Brush _selectedForegroundColor = new SolidColorBrush(Color.FromArgb(0xFF, 0x44, 0x44, 0x44));

        public KomuterPackageModel KomuterPackage { get; private set; }

        private ButtonState State { get; set; } = ButtonState.Normal;

        public uscJourneyTypeButton()
        {
            InitializeComponent();
        }

        private void BdJourneyType_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (State == ButtonState.Normal)
                {
                    App.TimeoutManager.ResetTimeout();
                    if (RaiseOnJourneyTypeChanged(KomuterPackage) == true)
                    {
                        Selected();
                    }
                }
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg($@"(EXIT10001141); {ex.Message}");
                App.Log.LogError(_logChannel, "-", new Exception($@"(EXIT10001141);", ex), "EX01", "uscJourneyTypeButton.JourneyType_Click");
            }

            //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            bool RaiseOnJourneyTypeChanged(KomuterPackageModel komuterPackage)
            {
                JourneyTypeChangeEventArgs arg = new JourneyTypeChangeEventArgs(komuterPackage, agreeChanged: false);
                try
                {
                    OnJourneyTypeChanged?.Invoke(null, arg);
                }
                catch (Exception ex)
                {
                    App.ShowDebugMsg($@"Unhandled error exception; (EXIT10001140); {ex.Message}");
                    App.Log.LogError(_logChannel, "-", new Exception($@"Unhandled error exception; (EXIT10001140);", ex), "EX01", "uscJourneyTypeButton.RaiseOnNoOfPaxChanged");
                }

                return arg.AgreeChanged;
            }
        }

        public void LoadJournetType()
        {
            TxtTypeDesc.Text = KomuterPackage.Description ?? "*";
            TxtAvalableDuration.Text = KomuterPackage.Duration ?? "-";
            NormalEnabled();
        }

        public void InitJournetType(KomuterPackageModel komuterPackage)
        {
            KomuterPackage = komuterPackage;
        }

        public void NormalEnabled()
        {
            BdJourneyType.Background = _normalBackgroundColor;
            TxtTypeDesc.Foreground = _normalForegroundColor;
            State = ButtonState.Normal;
        }

        private void Selected()
        {
            BdJourneyType.Background = _selectedBackgroundColor;
            TxtTypeDesc.Foreground = _selectedForegroundColor;
            State = ButtonState.Selected;
        }

        enum ButtonState
        {
            Normal = 0,
            Selected = 1
        }

        
    }
}
