using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.Client.Base;
using NssIT.Kiosk.Log.DB;
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

namespace NssIT.Kiosk.Client.ViewPage.StationTerminal
{
    /// <summary>
    /// Interaction logic for pgRouteMap.xaml
    /// </summary>
    public partial class pgRouteMap : Page
    {
        private string _logChannel = "ViewPage";
        private LanguageCode _language = LanguageCode.English;

        private ResourceDictionary _langMal = null;
        private ResourceDictionary _langEng = null;

        public event EventHandler OnExit;

        private DbLog Log { get; set; }

        public pgRouteMap()
        {
            InitializeComponent();
            Log = DbLog.GetDbLog();

            _langMal = CommonFunc.GetXamlResource(@"ViewPage\StationTerminal\rosStationMalay.xaml");
            _langEng = CommonFunc.GetXamlResource(@"ViewPage\StationTerminal\rosStationEng.xaml");
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        { 
        }

        private void BdExit_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                OnExit?.Invoke(null, new EventArgs());
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg(ex.Message);
                Log.LogError(_logChannel, "-", new Exception("Unhandle event error exception", ex), "EX01", "pgRouteMap.BdExit_MouseLeftButtonDown");
            }
        }

        public void SetLanguage(LanguageCode language)
        {
            _language = language;
            this.Resources.MergedDictionaries.Clear();

            if (_language == LanguageCode.Malay)
                this.Resources.MergedDictionaries.Add(_langMal);
            else
                this.Resources.MergedDictionaries.Add(_langEng);
        }
    }
}
