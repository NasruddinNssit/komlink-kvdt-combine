using kvdt_kiosk.Models;
using kvdt_kiosk.Services;
using kvdt_kiosk.Views.Kvdt.SeatingScreen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace kvdt_kiosk.Views.SeatingScreen
{
    /// <summary>
    /// Interaction logic for pgKomuterPax.xaml
    /// </summary>
    public sealed partial class PgKomuterPax : Page
    {

        public event EventHandler<TicketSelectionChangedEventArgs> OnOkClick;
        public event EventHandler OnCancelClick;
        private readonly DispatcherTimer _dispatcherTimer = new DispatcherTimer();
        private readonly List<uscKomuterTicketType> _uscTicketTypeControlList = new List<uscKomuterTicketType>();
        private AFCSeatInfo _ticketPackage = null;
        public ResourceDictionary _langResource = null;

        private const string _currency = "MYR";
        private static int _maxNoOfPax = 0;

        public PgKomuterPax()
        {
            InitializeComponent();
            StkTicketTypeContainer.Children.Clear();
            initData();

            LoadLanguage();
        }
        
        private void initData()
        {
            var aPIServices = new APIServices(new APIURLServices(), new APISignatureServices());
            _ticketPackage = aPIServices.GetAFCSeatInfo(UserSession.AFCService, UserSession.FromStationId, UserSession.ToStationId, UserSession.JourneyTypeId).Result as AFCSeatInfo;

            if (_ticketPackage.Data.TicketTypes.Count > 0)
            {
                var childTicketType = _ticketPackage.Data.TicketTypes.FirstOrDefault(x => x.TicketTypeId == "Child");
                var seniorTicketType = _ticketPackage.Data.TicketTypes.FirstOrDefault(x => x.TicketTypeId == "SC");

                if (childTicketType != null)
                {
                    PassengerInfo.MinChildAge = childTicketType.AgeMinAFC;
                    PassengerInfo.MaxChildAge = childTicketType.AgeMaxAFC;
                }
                else
                {
                    PassengerInfo.MaxChildAge = 0;
                }

                if (seniorTicketType != null)
                {
                    PassengerInfo.MinSeniorAge = seniorTicketType.AgeMinAFC;
                    PassengerInfo.MaxSeniorAge = seniorTicketType.AgeMaxAFC;
                }
                else
                {
                    PassengerInfo.MinSeniorAge = 0;
                }

            }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            initData();
            SystemConfig.IsResetIdleTimer = true;
            try
            {
                LoadKomuterPax();

                _maxNoOfPax = _ticketPackage.Data.MaximumPassengerCount;

                TxtMaxPax.Text = $@"(Max. {_maxNoOfPax} Person)";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            _dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 200);
            _dispatcherTimer.Tick += DispatcherTimer_Tick;
            _dispatcherTimer.Start();

            await Task.Delay(100);
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (!PassengerInfo.IsPaxSelected)
            {
                BtnOk.IsEnabled = false;
                BtnOk.Cursor = Cursors.No;
            }
            else
            {
                BtnOk.IsEnabled = true;
                BtnOk.Cursor = Cursors.Hand;
            }
        }

        private void LoadLanguage()
        {
            Dispatcher.InvokeAsync(() =>
            {
                if (App.Language != "ms") return;
                lblPassenger.Text = "Penumpang";
                lblReset.Text = "Semula";
                lblOk.Text = "OK";
                lblCancel.Text = "BATAL";
                lblTotalTicketSelected.Text = "Jumlah Tiket Yang Dipilih: ";
                lblTotalAmount.Text = "Jumlah Harga";

            });
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
                if (_ticketPackage.Data != null)
                {
                    foreach (var item in _ticketPackage.Data.TicketTypes)
                    {
                        uscKomuterTicketType tkType = GetFreeUscKomuterTicketType();
                        _maxNoOfPax = _ticketPackage.Data.MaximumPassengerCount;
                        tkType.InitData(_currency, item.TicketTypeId, item.TicketTypeName, item.UnitPrice, _maxNoOfPax, item.VerifyMalaysianAFC, item.IsVerifyAgeAFCRequired, item.AgeMinAFC, item.AgeMaxAFC);
                        tkType.OnNoOfPaxChanged += Pax_OnNoOfPaxChanged;
                        tkType.LoadTicketType();
                        StkTicketTypeContainer.Children.Add(tkType);
                    }
                }
            }
            TxtCurrency.Text = _currency;
            lblTotalSelected.Text = "0";
            TxtTotalAmount.Text = $@"{0: #,###.00}";
            TxtJourneyDesc.Text = UserSession.JourneyType;
            TxtAvailableDuration.Text = DateTime.Now.ToString("ddd, dd-MM");
        }

        private void Pax_OnNoOfPaxChanged(object sender, NoOfPaxChangedEventArgs e)
        {
            try
            {
                var ticketSelectedCount = GetNoOfPaxSelected();
                var totalTicketAmount = 0.0M;
                var reminder = _maxNoOfPax - ticketSelectedCount;
                reminder = (reminder < 0) ? 0 : reminder;
                foreach (var ctrl in StkTicketTypeContainer.Children)
                {
                    if (!(ctrl is uscKomuterTicketType tType)) continue;
                    // Maximum Pax Reconsilation
                    var maxNoOfPaxReconsideration = tType.NoOfPaxSelected + reminder;
                    tType.RecalibrateNoOfPax(maxNoOfPaxReconsideration);

                    tType.ReadData(out int selectedNoOfPax, out string currency, out string ticketTypeId, out string ticketTypeDescription, out decimal ticketPrice, out bool v1, out bool v2, out int v3, out int v4);
                    totalTicketAmount += (ticketPrice * selectedNoOfPax);
                }

                lblTotalSelected.Text = ticketSelectedCount.ToString();
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
            SystemConfig.IsResetIdleTimer = true;

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
                            UserSession.TempDataForChildSeat = x.SelectedNoOfPax;
                            UserSession.IsVerifyAgeAFCRequiredForChild = x.IsVerifyAgeKomuterRequired;
                        }
                        else if (x.TicketTypeId == "SC")
                        {
                            UserSession.SeniorSeat = x.SelectedNoOfPax;
                            UserSession.TempDataForSeniorSeat = x.SelectedNoOfPax;
                            UserSession.IsVerifyAgeAFCRequiredForSenior = x.IsVerifyAgeKomuterRequired;
                        }

                        else if (x.TicketTypeId == "OKUW")
                        {
                            UserSession.OKUSeat = x.SelectedNoOfPax;
                            UserSession.TempDataForOKUSeat = x.SelectedNoOfPax;
                            UserSession.IsVerifyAgeAFCRequiredForOKU = x.VerifyMalaysianKomuter;
                        }

                        if (UserSession.TicketOrderTypes == null)
                        {
                            UserSession.TicketOrderTypes = new List<TicketOrderType>();
                        }

                        UserSession.TicketOrderTypes.Add(new TicketOrderType
                        {
                            TicketTypeId = x.TicketTypeId,
                            TicketTypeName = x.TicketTypeDescription,
                            UnitPrice = x.TicketPrice,
                            AgeMaxAFC = x.AgeMaxKomuter,
                            AgeMinAFC = x.AgeMinKomuter,
                            NoOfPax = x.SelectedNoOfPax,
                            VerifyMalaysianAFC = x.VerifyMalaysianKomuter,
                            TotalPrice = x.TicketPrice * x.SelectedNoOfPax
                        });

                    });

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
            SystemConfig.IsResetIdleTimer = true;

            //Dispatcher.InvokeAsync(() =>
            //{
            //    Window current = Window.GetWindow(this);
            //    current.Owner.Opacity = 1;
            //    current.Owner.Effect = null;
            //    current.Close();
            //});

        }

        private async void ResetPax_Click(object sender, RoutedEventArgs e)
        {
            BtnReset.Content = "Loading...";
            BtnReset.FontSize = 24;
            BtnReset.IsEnabled = false;
            await Task.Delay(50);

            try
            {
                foreach (var ctrl in StkTicketTypeContainer.Children)
                    if (ctrl is uscKomuterTicketType tType)
                        tType.ResetSelection();

                lblTotalSelected.Text = "0";
                TxtTotalAmount.Text = $@"{0: #,###.00}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            SystemConfig.IsResetIdleTimer = true;
            UserSession.TicketOrderTypes = null;
            BtnReset.Content = "RESET";
            BtnReset.IsEnabled = true;

            PassengerInfo.IsPaxSelected = false;
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            SystemConfig.IsResetIdleTimer = true;
        }
    }
}
