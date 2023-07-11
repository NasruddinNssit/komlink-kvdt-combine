using NssIT.Kiosk.AppDecorator.Common;
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
    /// Interaction logic for pgMap.xaml
    /// </summary>
    public partial class pgMap : Page
    {
        private LanguageCode _language = LanguageCode.English;
        private string _logChannel = "ViewPage";
        public event EventHandler OnResetClick;

        private Brush _activatedResetButtonColor = new SolidColorBrush(Color.FromArgb(0xFF, 0x55, 0x55, 0x55));
        private Brush _activatedResetFontColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xEE, 0xEE, 0xEE));

        private Brush _deactivatedResetButtonColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xFE, 0x4C, 0x70));
        private Brush _deactivatedResetFontColor = new SolidColorBrush(Color.FromArgb(0xFF, 0x44, 0x44, 0x44));
        private bool _resetIsActivated = true;

        public pgMap()
        {
            InitializeComponent();
        }

        public void ActivateReset()
        {
            _resetIsActivated = true;
            if (_langResource != null)
            {
                TxtReset.Text = _langResource["RESET_Label"].ToString();
            }
            else
                TxtReset.Text = "RESET";

            BdReset.Background = _activatedResetButtonColor;
            TxtReset.Foreground = _activatedResetFontColor;
        }

        public void DeactivateReset()
        {
            _resetIsActivated = false;

            if (_langResource != null)
            {
                TxtReset.Text = _langResource["BUSY_Label"].ToString();
            }
            else
                TxtReset.Text = "busy..";

            BdReset.Background = _deactivatedResetButtonColor;
            TxtReset.Foreground = _deactivatedResetFontColor;
        }

        public bool IsResetActivated { get => _resetIsActivated; }

        private void Page_Load(object sender, RoutedEventArgs e)
        {
            ActivateReset();
            App.MainScreenControl.MiniNavigator.AttachToFrame(frmNav);
            App.MainScreenControl.MiniNavigator.IsPreviousVisible = Visibility.Hidden;
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {

        }

        private void Reset_Click(object sender, MouseButtonEventArgs e)
        {
            if (_resetIsActivated == false)
                return;

            try
            {
                App.TimeoutManager.ResetTimeout();
                App.BookingTimeoutMan.ResetCounter();

                RaiseOnResetClick();
            }
            catch (Exception ex)
            {
                App.Log.LogError(_logChannel, "", new Exception("(EXIT10001170)", ex), "EX01", "pgMap.Reset_Click");
            }

            void RaiseOnResetClick()
            {
                try
                {
                    if (OnResetClick != null)
                    {
                        OnResetClick.Invoke(null, e);
                    }
                }
                catch (Exception ex)
                {
                    App.Log.LogError(_logChannel, "", new Exception("Unhandle error exception; (EXIT10001171)", ex), "EX01", "pgMap.RaiseOnResetClick");
                }
            }
        }

        private ResourceDictionary _langResource = null;
        public void SetLanguage(LanguageCode language, ResourceDictionary langResource)
        {
            _langResource = langResource;
            this.Resources.MergedDictionaries.Clear();
            this.Resources.MergedDictionaries.Add(_langResource);
            App.MainScreenControl.MiniNavigator.SetLanguage(language);
        }
    }
}
