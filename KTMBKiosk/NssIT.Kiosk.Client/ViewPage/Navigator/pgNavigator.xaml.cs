using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using NssIT.Kiosk.Client.Base;
using NssIT.Kiosk.Client.ViewPage.Menu.Section;
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

namespace NssIT.Kiosk.Client.ViewPage.Navigator
{
    /// <summary>
    /// Interaction logic for pgNavigator.xaml
    /// </summary>
    public partial class pgNavigator : Page, INav
    {
        private const string LogChannel = "MainWindow";

        private ResourceDictionary _langMal = null;
        private ResourceDictionary _langEng = null;

        private LanguageCode _currentLanguage = LanguageCode.English;

        public event EventHandler<MenuItemPageNavigateEventArgs> OnPageNavigateChanged;

        public pgNavigator()
        {
            InitializeComponent();

            _langMal = CommonFunc.GetXamlResource(@"ViewPage\Navigator\rosNavigatorMal.xaml");
            _langEng = CommonFunc.GetXamlResource(@"ViewPage\Navigator\rosNavigatorEng.xaml");
        }

        private void BdExit_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            RaiseOnPageNavigateChanged(PageNavigateDirection.Exit);
        }

        private void BdPrevious_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            RaiseOnPageNavigateChanged(PageNavigateDirection.Previous);
        }

        public void SetLanguage(LanguageCode language)
        {
            try
            {
                if (_currentLanguage != language)
                {
                    this.Resources.MergedDictionaries.Clear();

                    if (language == LanguageCode.Malay)
                        this.Resources.MergedDictionaries.Add(_langMal);
                    else
                        this.Resources.MergedDictionaries.Add(_langEng);

                    _currentLanguage = language;
                }
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg($@"Error: {ex.Message}; (EXIT10000401A); pgNavigator.SetLanguage");
                App.Log.LogError(LogChannel, "-", new Exception("(EXIT10000401A)", ex), "EX01", "pgNavigator.SetLanguage");
            }
        }

        private Frame _lastContainer = null;
        public void AttachToFrame(Frame container, LanguageCode? language = null)
        {
            if (container != null)
            {
                if (_lastContainer != null)
                {
                    _lastContainer.Content = null;
                    _lastContainer.NavigationService.RemoveBackEntry();
                }

                if (language.HasValue)
                    SetLanguage(language.Value);

                container.NavigationService.Navigate(this);
                
                _lastContainer = container;
            }
        }

        //public void Reset()
        //{
        //    ShowNavigator();
        //    IsPreviousVisible = Visibility.Visible;
        //}

        public void ShowNavigator()
        {
            GrdNav.Visibility = Visibility.Visible;
        }

        public void HideNavigator()
        {
            GrdNav.Visibility = Visibility.Hidden;
        }

        public Visibility IsPreviousVisible
        {
            get
            {
                return BdPrevious.Visibility;
            }
            set
            {
                BdPrevious.Visibility = value;
            }
        }

        private void RaiseOnPageNavigateChanged(PageNavigateDirection pageNav)
        {
            try
            {
                if (OnPageNavigateChanged != null)
                {
                    OnPageNavigateChanged.Invoke(null, new MenuItemPageNavigateEventArgs(pageNav));
                }
            }
            catch (Exception ex)
            {
                App.Log.LogError(LogChannel, "-", new Exception("Unhandle error event exception", ex), "EX01", "pgNavigator.RaiseOnPageNavigateChanged");
            }
        }
    }
}
