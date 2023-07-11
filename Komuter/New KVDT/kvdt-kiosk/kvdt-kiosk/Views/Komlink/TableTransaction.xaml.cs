using kvdt_kiosk.Models.Komlink;
using Serilog;
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

namespace kvdt_kiosk.Views.Komlink
{
    /// <summary>
    /// Interaction logic for TableTransaction.xaml
    /// </summary>
    public partial class TableTransaction : UserControl
    {
        public TableTransaction()
        {
            InitializeComponent();

            LoadLanguage();
        }

        private void LoadLanguage()
        {
            try
            {
                if (App.Language == "ms")
                {
                    DateText.Text = "Tarikh";
                    TimeText.Text = "Masa";
                    TransactionText.Text = "Transaksi";
                    StationText.Text = "Stesen";
                    TicketTypeText.Text = "Jenis Tiket";
                    AmountText.Text = "Amaun";
                }

            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, UserSession.SessionId + " Error in TableTransaction.xaml.cs");

            }
        }

        public void AddData(List<KomlinkTransactionDetailItem> data)
        {
            ItemsControlData.ItemsSource = data;
        }


        public void SetHeightTable()
        {

        }
    }
}
