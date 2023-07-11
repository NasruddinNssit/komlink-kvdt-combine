using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using NssIT.Kiosk.Client.Base;
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

namespace NssIT.Kiosk.Client.ViewPage.TicketSummary
{
    /// <summary>
    /// Interaction logic for pgTickSumm.xaml
    /// </summary>
    public partial class pgTickSumm : Page, ITicketSummary
    {

        private LanguageCode _language = LanguageCode.English;
        private ResourceDictionary _langMal = null;
        private ResourceDictionary _langEng = null;

        public pgTickSumm()
        {
            InitializeComponent();

            _langMal = CommonFunc.GetXamlResource(@"ViewPage\TicketSummary\rosTickSummMal.xaml");
            _langEng = CommonFunc.GetXamlResource(@"ViewPage\TicketSummary\rosTickSummEng.xaml");
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.Application.DoEvents();
            DependencyObject dependencyObject = VisualTreeHelper.GetParent(this);
            if (dependencyObject is ContentPresenter cp)
            {
                this.Width = cp.ActualWidth;
            }
        }

        public void UpdateSummary(UserSession session, TickSalesMenuItemCode currentEditItemCode)
        {
            this.Dispatcher.Invoke(new Action(() => {

                TxtOverralTripDesc.Text = "";

                if (string.IsNullOrWhiteSpace(session.OriginStationName) == false)
                    TxtOverralTripDesc.Text = session.OriginStationName;

                if (string.IsNullOrWhiteSpace(session.DestinationStationName) == false)
                    TxtOverralTripDesc.Text += "  >  " + session.DestinationStationName;

                //Set Language
                _language = session.Language;
                this.Resources.MergedDictionaries.Clear();
                if (_language == LanguageCode.Malay)
                    this.Resources.MergedDictionaries.Add(_langMal);
                else
                    this.Resources.MergedDictionaries.Add(_langEng);

                //Set Depart Portal
                ITicketSummaryPortal summBlock = (ITicketSummaryPortal)TSummDepart1;
                summBlock.UpdateSummary(session, TravelMode.DepartOnly);
                SetDepartDisplayEffect(currentEditItemCode);

                //Set Return Portal
                if (session.TravelMode == TravelMode.DepartOrReturn)
                {
                    if (string.IsNullOrWhiteSpace(session.SeatBookingId) == false)
                    {
                        TSummReturn1.Visibility = Visibility.Visible;
                        ITicketSummaryPortal summBlockX = (ITicketSummaryPortal)TSummReturn1;
                        summBlockX.UpdateSummary(session, TravelMode.ReturnOnly);
                        SetReturnDisplayEffect(currentEditItemCode);
                    }
                    else
                        TSummReturn1.Visibility = Visibility.Collapsed;
                }
                else
                    TSummReturn1.Visibility = Visibility.Collapsed;
            }));

            void SetDepartDisplayEffect(TickSalesMenuItemCode editItemCode)
            {
                ITicketSummaryPortal summBlockX = (ITicketSummaryPortal)TSummDepart1;
                if ((editItemCode == TickSalesMenuItemCode.DepartTrip) 
                    || (editItemCode == TickSalesMenuItemCode.DepartSeat)
                    || (editItemCode == TickSalesMenuItemCode.Passenger)
                    || (editItemCode == TickSalesMenuItemCode.Insurance))
                {
                    summBlockX.IsActive = true;
                }
                else
                {
                    summBlockX.IsActive = false;
                }
            }

            void SetReturnDisplayEffect(TickSalesMenuItemCode editItemCode)
            {
                ITicketSummaryPortal summBlockY = (ITicketSummaryPortal)TSummReturn1;
                if ((editItemCode == TickSalesMenuItemCode.ReturnTrip)
                    || (editItemCode == TickSalesMenuItemCode.ReturnSeat)
                    || (editItemCode == TickSalesMenuItemCode.Passenger)
                    || (editItemCode == TickSalesMenuItemCode.Insurance))
                {
                    summBlockY.IsActive = true;
                }
                else
                {
                    summBlockY.IsActive = false;
                }
            }
        }

        public void UpdateDepartDate(DateTime newDepartDate)
        {
            this.Dispatcher.Invoke(new Action(() => {
                ITicketSummaryPortal summBlock = (ITicketSummaryPortal)TSummDepart1;
                summBlock.UpdateDepartureDate(newDepartDate);
            }));
        }

        public void UpdateReturnDate(DateTime newReturntDate)
        {
            ITicketSummaryPortal summBlock = (ITicketSummaryPortal)TSummReturn1;
            summBlock.UpdateDepartureDate(newReturntDate);
        }

    }
}
