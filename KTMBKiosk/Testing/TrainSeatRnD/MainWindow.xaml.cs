using NssIT.Kiosk.Client.ViewPage.Seat;
using NssIT.Train.Kiosk.Common.Data;
using NssIT.Train.Kiosk.Common.Data.PostRequestParam;
using NssIT.Train.Kiosk.Common.Data.Response;
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

namespace TrainSeatRnD
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private LibShowMessageWindow.MessageWindow _msg = null;
        private pgSeat2 _pgSeat = null;
        public MainWindow()
        {
            InitializeComponent();
            _msg = new LibShowMessageWindow.MessageWindow();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _pgSeat = new pgSeat2();
                frmSeat.NavigationService.Navigate(_pgSeat);
            }
            catch (Exception ex)
            {
                _msg.ShowMessage(ex.ToString());
            }
        }

        private void ReadData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WebApiConn api = new WebApiConn();
                dynamic res = api.GetTestData(new SeatRequest() 
                { 
                    Id = @"ZSofBuN99OMqlzRZfls7w8nLw/ln3XdqfauTwXXy/O1HM86a6pHOa93zsqOSey0KNjf7DwNr79HzPgxzF2/LdNwdgMvWUNutl8FpLcA5IJfyBI4pOaJF5g7FRf/5b16Zort5AaiLqAmM/0EkaiPDh0qGwn3++q4U6fHWmxnIsbQ=", 
                    MCounters_Id = @"19100"
                });

                if (res is TrainSeatResult tSeat)
                {
                    if (tSeat.Code.Equals("0")
                        && (tSeat.Data.ErrorCode == 0)
                        && (tSeat.Data.CoachModels.Length > 0)
                        && (tSeat.Data.CoachModels[0].SeatLayoutModels.Length > 0))
                    {
                        _msg.ShowMessage("Query Success");

                        _pgSeat.Debug_CreateNInitPage(tSeat.Data);
                        _pgSeat.Debug_LoadSimulation();
                    }
                    else
                    {
                        _msg.ShowMessage("Query Not Success");
                        System.Diagnostics.Debugger.Launch();
                    }
                }

                string tt1 = "";
            }
            catch (Exception ex)
            {
                _msg.ShowMessage(ex.ToString());
            }
        }


    }
}
