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

namespace NssIT.Kiosk.Client.ViewPage.Insurance
{
    /// <summary>
    /// Interaction logic for pgToAgreeInsurance.xaml
    /// </summary>
    public partial class pgToAgreeInsurance : Page
    {
        private string _logChannel = "ViewPage";

        public event EventHandler OnConfirmDisagreeInsurance;
        public event EventHandler OnConfirmAgreeInsurance;

        private DbLog Log { get; set; }

        public pgToAgreeInsurance()
        {
            InitializeComponent();

            Log = DbLog.GetDbLog();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        { }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        { }

        public void SetPageConfiguration(ResourceDictionary lang)
        {
            if (lang != null)
            {
                this.Dispatcher.Invoke(new Action(() =>
                {
                    this.Resources.MergedDictionaries.Clear();
                    this.Resources.MergedDictionaries.Add(lang);
                }));
            }
        }

        private void ConfirmAgreeInsurance_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                e.Handled = true;
                OnConfirmAgreeInsurance?.Invoke(null, new EventArgs());
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg(ex.Message);
                Log.LogError(_logChannel, "-", new Exception("Unhandle event error exception", ex), "EX01", "pgToAgreeInsurance.ConfirmAgreeInsurance_Click");
            }
        }

        private void ConfirmDisagreeInsurance_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                e.Handled = true;
                OnConfirmDisagreeInsurance?.Invoke(null, new EventArgs());
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg(ex.Message);
                Log.LogError(_logChannel, "-", new Exception("Unhandle event error exception", ex), "EX01", "pgToAgreeInsurance.ConfirmDisagreeInsurance_Click");
            }
        }
    }
}
