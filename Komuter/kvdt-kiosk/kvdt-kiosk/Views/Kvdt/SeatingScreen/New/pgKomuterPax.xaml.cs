using kvdt_kiosk.Models;
using kvdt_kiosk.Services;
using kvdt_kiosk.Views.Kvdt.ReturnJourney;
using kvdt_kiosk.Views.Kvdt.SeatingScreen.New;
using kvdt_kiosk.Views.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace kvdt_kiosk.Views.SeatingScreen.New.Kvdt
{
    /// <summary>
    /// Interaction logic for pgKomuterPax.xaml
    /// </summary>
    public partial class pgKomuterPax : Page
    {


        public event EventHandler<TicketSelectionChangedEventArgs> OnOkClick;
        public event EventHandler OnCancelClick;


        private List<uscKomuterTicketType> _uscTicketTypeControlList = new List<uscKomuterTicketType>();
        private AFCTicketType _ticketPackage = null;
        private ResourceDictionary _langResource = null;

        private string _currency = "MYR";
        private int _maxNoOfPax = 6;
        public pgKomuterPax()
        {
            InitializeComponent();
            StkTicketTypeContainer.Children.Clear();
            initData();
        }

        private void initData()
        {
            APIServices aPIServices = new APIServices(new APIURLServices(), new APISignatureServices());
            _ticketPackage = aPIServices.GetAFCTicketType("TVM").Result as AFCTicketType;


        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            initData();
            try
            {
                LoadKomuterPax();

                TxtMaxPax.Text = $@"(Max. {_maxNoOfPax} Person)";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void SvTicketTypePax_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            try
            {
                //App.TimeoutManager.ResetTimeout();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        public void LoadKomuterPax()
        {

            ClearAllKomuterTicketType();
            if (_ticketPackage != null)
            {
                if (_ticketPackage.Data?.Count > 0)
                {
                    foreach (AFCTicketTypeDetails aFCTicketTypeDetails in _ticketPackage.Data)
                    {
                        uscKomuterTicketType tkType = GetFreeUscKomuterTicketType();
                        tkType.InitData(_currency, aFCTicketTypeDetails.TicketTypeId, aFCTicketTypeDetails.TicketTypeName, aFCTicketTypeDetails.UnitPrice, _maxNoOfPax, aFCTicketTypeDetails.VerifyMalaysianAFC, aFCTicketTypeDetails.IsVerifyAgeAFCRequired, aFCTicketTypeDetails.AgeMinAFC, aFCTicketTypeDetails.AgeMaxAFC);
                        tkType.OnNoOfPaxChanged += Pax_OnNoOfPaxChanged;
                        tkType.LoadTicketType();
                        StkTicketTypeContainer.Children.Add(tkType);
                    }
                }
            }
            TxtCurrency.Text = _currency;
            TxtTicketQty.Text = "0";
            TxtTotalAmount.Text = $@"{0: #,###.00}";
            TxtJourneyDesc.Text = UserSession.JourneyType;
            TxtAvailableDuration.Text = DateTime.Now.ToString("dd MMM");

        }

        private void Pax_OnNoOfPaxChanged(object sender, NoOfPaxChangedEventArgs e)
        {
            try
            {
                int ticketSelectedCount = GetNoOfPaxSelected();
                decimal totalTicketAmount = 0.0M;
                int reminder = _maxNoOfPax - ticketSelectedCount;
                reminder = (reminder < 0) ? 0 : reminder;
                foreach (var ctrl in StkTicketTypeContainer.Children)
                {
                    if (ctrl is uscKomuterTicketType tType)
                    {
                        // Maximum Pax Reconsilation
                        int maxNoOfPaxReconsideration = tType.NoOfPaxSelected + reminder;
                        tType.RecalibrateNoOfPax(maxNoOfPaxReconsideration);

                        tType.ReadData(out int selectedNoOfPax, out string currency, out string ticketTypeId, out string ticketTypeDescription, out decimal ticketPrice, out bool v1, out bool v2, out int v3, out int v4);
                        totalTicketAmount += (ticketPrice * selectedNoOfPax);


                    }
                }

                TxtTicketQty.Text = ticketSelectedCount.ToString();
                TxtTotalAmount.Text = $@"{totalTicketAmount: #,###.00}";

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        public int GetNoOfPaxSelected()
        {
            int noOfPaxSelected = 0;
            foreach (var ctrl in StkTicketTypeContainer.Children)
            {
                if (ctrl is uscKomuterTicketType tType)
                {
                    noOfPaxSelected += tType.NoOfPaxSelected;
                }
            }
            return noOfPaxSelected;
        }


        private uscKomuterTicketType GetFreeUscKomuterTicketType()
        {
            uscKomuterTicketType retCtrl = null;
            if (_uscTicketTypeControlList.Count == 0)
            {
                retCtrl = new uscKomuterTicketType();
            }
            else
            {
                retCtrl = _uscTicketTypeControlList[0];
                _uscTicketTypeControlList.RemoveAt(0);
            }
            return retCtrl;
        }



        private void ClearAllKomuterTicketType()
        {
            int nextCtrlInx = 0;
            do
            {
                if (StkTicketTypeContainer.Children.Count > nextCtrlInx)
                {
                    if (StkTicketTypeContainer.Children[nextCtrlInx] is uscKomuterTicketType ctrl)
                    {
                        ctrl.OnNoOfPaxChanged -= Pax_OnNoOfPaxChanged;
                        StkTicketTypeContainer.Children.RemoveAt(nextCtrlInx);
                        _uscTicketTypeControlList.Add(ctrl);
                    }
                    else
                        nextCtrlInx++;
                }
            } while (StkTicketTypeContainer.Children.Count > nextCtrlInx);
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (GetSelectedTicketItem()?.Length > 0)
                {
                    RaiseOnOkClick();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return;

            void RaiseOnOkClick()
            {
                try
                {
                    KomuterTicket[] selectedTicketArr = GetSelectedTicketItem();
                    TicketSelectionChangedEventArgs evtArg = new TicketSelectionChangedEventArgs("Child", "Child", "2 may", selectedTicketArr);

                    evtArg.TicketList.ToList().ForEach(x =>
                    {
                        if (x.TicketTypeId == "Child")
                        {
                            UserSession.ChildSeat = x.SelectedNoOfPax;
                        }
                        else if (x.TicketTypeId == "SC")
                        {
                            UserSession.SeniorSeat = x.SelectedNoOfPax;
                        }

                    });

                    OnOkClick?.Invoke(null, evtArg);

                    MyDispatcher.Invoke(new System.Action(() =>
                    {
                        ReturnJourneyPassengerWindow returnJourneyPassengerWindow = new ReturnJourneyPassengerWindow();
                        PassengerReturnJourney passengerReturnJourneyScreen = new PassengerReturnJourney();

                        returnJourneyPassengerWindow.Owner = Window.GetWindow(this);
                        returnJourneyPassengerWindow.Content = passengerReturnJourneyScreen;
                        returnJourneyPassengerWindow.WindowStyle = WindowStyle.None;
                        returnJourneyPassengerWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;

                        returnJourneyPassengerWindow.ShowDialog();
                    }));

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error");
                }
            }


            KomuterTicket[] GetSelectedTicketItem()
            {
                List<KomuterTicket> tickList = new List<KomuterTicket>();
                foreach (var ctrl in StkTicketTypeContainer.Children)
                {
                    try
                    {
                        if (ctrl is uscKomuterTicketType tType)
                        {
                            tType.ReadData(out int selectedNoOfPax, out string currency, out string ticketTypeId, out string ticketTypeDescription, out decimal ticketPrice,
                                out bool verifyMalaysianKomuter, out bool isVerifyAgeKomuterRequired, out int ageMinKomuter, out int ageMaxKomuter);

                            if (selectedNoOfPax > 0)
                            {
                                tickList.Add(new KomuterTicket(selectedNoOfPax, currency, ticketTypeId, ticketTypeDescription, ticketPrice, verifyMalaysianKomuter, isVerifyAgeKomuterRequired, ageMinKomuter, ageMaxKomuter));

                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message + "Error", "Error");
                    }
                }

                return tickList.ToArray();
            }

        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            MyDispatcher.Invoke(() =>
            {
                Window returnJourneyWindow = Window.GetWindow(this);

                returnJourneyWindow.Owner.Effect = null;
                returnJourneyWindow.Owner.Opacity = 1;

                returnJourneyWindow.Close();

            });
        }

        private void ResetPax_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (var ctrl in StkTicketTypeContainer.Children)
                    if (ctrl is uscKomuterTicketType tType)
                        tType.ResetSelection();

                TxtTicketQty.Text = "0";
                TxtTotalAmount.Text = $@"{0: #,###.00}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
