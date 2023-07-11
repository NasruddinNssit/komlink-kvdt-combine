using NssIT.Train.Kiosk.Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
    /// Interaction logic for pgKomuterPax.xaml
    /// </summary>
    public partial class pgKomuterPax : Page
    {
        public event EventHandler<TicketSelectionChangedEventArgs> OnOkClick;
        public event EventHandler OnCancelClick;

        private string _logChannel = "ViewPage";

        private List<uscKomuterTicketType> _uscticketTypeControlList = new List<uscKomuterTicketType>();
        private KomuterPackageModel _ticketPackage = null;
        private ResourceDictionary _langResource = null;
        private string _currency = "";
        private int _maxNoOfPax = 1;

        public pgKomuterPax()
        {
            InitializeComponent();
            StkTicketTypeContainer.Children.Clear();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadKomuterPax();
                
                if (_langResource != null)
                {
                    TxtMaxPax.Text = string.Format(_langResource["MAX_PAX_Label"].ToString(), _maxNoOfPax);
                }
                else
                    TxtMaxPax.Text = $@"(Max. {_maxNoOfPax} Person)";

            }
            catch (Exception ex)
            {
                App.ShowDebugMsg($@"(EXIT10001132); {ex.Message}");
                App.Log.LogError(_logChannel, "-", new Exception($@"(EXIT10001132);", ex), "EX01", "pgKomuterPax.Pax_OnNoOfPaxChanged");
            }
        }

        private void Pax_OnNoOfPaxChanged(object sender, NoOfPaxChangedEventArgs e)
        {
            try
            {
                App.TimeoutManager.ResetTimeout();

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
                        tType.ReCalibrateNoOfPax(maxNoOfPaxReconsideration);

                        tType.ReadData(out int selectedNoOfPax, out string currency, out string ticketTypeId, out string ticketTypeDescription, out decimal ticketPrice, out bool v1, out bool v2, out int v3, out int v4);
                        totalTicketAmount += (ticketPrice * selectedNoOfPax);
                    }
                }

                TxtTicketQty.Text = ticketSelectedCount.ToString();
                TxtTotalAmount.Text = $@"{totalTicketAmount : #,###.00}"; 

                System.Windows.Forms.Application.DoEvents();
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg($@"(EXIT10001130); {ex.Message}");
                App.Log.LogError(_logChannel, "-", new Exception($@"(EXIT10001130);", ex), "EX01", "pgKomuterPax.Pax_OnNoOfPaxChanged");
            }
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (GetSelectedTicketItem()?.Length > 0)
                {
                    App.TimeoutManager.ResetTimeout();
                    RaiseOnOkClick();
                }
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg($@"(EXIT10001134); {ex.Message}");
                App.Log.LogError(_logChannel, "-", new Exception($@"(EXIT10001134);", ex), "EX01", "pgKomuterPax.Pax_Click");
            }

            return;
            //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            void RaiseOnOkClick()
            {
                try
                {
                    KomuterTicket[] selectedTicketArr = GetSelectedTicketItem();
                    TicketSelectionChangedEventArgs evtArg = new TicketSelectionChangedEventArgs(_ticketPackage.Id, _ticketPackage.Description, _ticketPackage.Duration, selectedTicketArr);
                    OnOkClick?.Invoke(null, evtArg);
                }
                catch (Exception ex)
                {
                    App.ShowDebugMsg($@"Unhandled error exception; (EXIT10001135); {ex.Message}");
                    App.Log.LogError(_logChannel, "-", new Exception($@"Unhandled error exception; (EXIT10001135);", ex), "EX01", "pgKomuterPax.RaiseOnOkClick");
                }
            }

            KomuterTicket[] GetSelectedTicketItem()
            {
                List<KomuterTicket> tickList = new List<KomuterTicket>();

                foreach (var ctrl in StkTicketTypeContainer.Children)
                {
                    try
                    {
                        if (ctrl is uscKomuterTicketType tickTyp)
                        {
                            tickTyp.ReadData(out int selectedNoOfPax, out string currency, out string ticketTypeId, out string ticketTypeDescription, out decimal ticketPrice,
                                out bool verifyMalaysianKomuter, out bool isVerifyAgeKomuterRequired, out int ageMinKomuter, out int ageMaxKomuter);

                            if (selectedNoOfPax > 0)
                                tickList.Add(new KomuterTicket(selectedNoOfPax, currency, ticketTypeId, ticketTypeDescription, ticketPrice, verifyMalaysianKomuter, isVerifyAgeKomuterRequired, ageMinKomuter, ageMaxKomuter));
                        }
                    }
                    catch (Exception ex)
                    {
                        App.ShowDebugMsg($@"Error when read Komuter Ticket Selection data; (EXIT10001138); {ex.Message}");
                        App.Log.LogError(_logChannel, "-", new Exception($@"Error when read Komuter Ticket Selection data; (EXIT10001138);", ex), "EX01", "pgKomuterPax.GetSelectedTicketItem");
                    }
                }
                                
                return tickList.ToArray();
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                App.TimeoutManager.ResetTimeout();
                RaiseOnCancelClick();
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg($@"(EXIT10001136); {ex.Message}");
                App.Log.LogError(_logChannel, "-", new Exception($@"(EXIT10001136);", ex), "EX01", "uscPaxButton.Pax_Click");
            }

            //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            void RaiseOnCancelClick()
            {
                try
                {
                    OnCancelClick?.Invoke(null, new EventArgs());
                }
                catch (Exception ex)
                {
                    App.ShowDebugMsg($@"Unhandled error exception; (EXIT10001137); {ex.Message}");
                    App.Log.LogError(_logChannel, "-", new Exception($@"Unhandled error exception; (EXIT10001137);", ex), "EX01", "uscPaxButton.RaiseOnNoOfPaxChanged");
                }
            }
        }

        private void ResetPax_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                App.TimeoutManager.ResetTimeout();

                foreach (var ctrl in StkTicketTypeContainer.Children)
                    if (ctrl is uscKomuterTicketType tType)
                        tType.ResetSelection();

                TxtTicketQty.Text = "0";
                TxtTotalAmount.Text = $@"{0: #,###.00}";

                System.Windows.Forms.Application.DoEvents();
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg($@"(EXIT10001131); {ex.Message}");
                App.Log.LogError(_logChannel, "-", new Exception($@"(EXIT10001131);", ex), "EX01", "pgKomuterPax.ResetPax_Click");
            }
        }

        private void SvTicketTypePax_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            try
            {
                App.TimeoutManager.ResetTimeout();
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg($@"(EXIT10001133); {ex.Message}");
                App.Log.LogError(_logChannel, "-", new Exception($@"(EXIT10001133);", ex), "EX01", "pgKomuterPax.SvTicketTypePax_ScrollChanged");
            }
        }

        public void LoadKomuterPax()
        {
            ClearAllKomuterTicketType();
            if (_ticketPackage != null)
            {
                if (_ticketPackage.TicketTypes?.Length > 0)
                {
                    foreach(KomuterTicketTypeModel tType in _ticketPackage.TicketTypes)
                    {
                        uscKomuterTicketType tkTyp = GetFreeUscKomuterTicketType();

                        // .. to activate DEBUG-Testing remark below line .. => 
                        tkTyp.InitData(_currency, tType.Id, tType.Description, tType.Amount, _maxNoOfPax, tType.VerifyMalaysianKomuter, tType.IsVerifyAgeKomuterRequired, tType.AgeMinKomuter, tType.AgeMaxKomuter);

                        ////////////DEBUG-Testing -- Start.. 'Consider my IC is 48 years old.'
                        //////////if (App.SysParam.PrmIsDebugMode == true)
                        //////////{
                        //////////    //tkTyp.InitData(_currency, tType.Id, tType.Description, tType.Amount, _maxNoOfPax, verifyMalaysianKomuter: false, isVerifyAgeKomuterRequired: false, ageMinKomuter: 0, ageMaxKomuter: 0);
                        //////////    //tkTyp.InitData(_currency, tType.Id, tType.Description, tType.Amount, _maxNoOfPax, verifyMalaysianKomuter: false, isVerifyAgeKomuterRequired: true, ageMinKomuter: 48, ageMaxKomuter: 48);
                        //////////    //tkTyp.InitData(_currency, tType.Id, tType.Description, tType.Amount, _maxNoOfPax, verifyMalaysianKomuter: true, isVerifyAgeKomuterRequired: false, ageMinKomuter: 48, ageMaxKomuter: 48);

                        //////////    tkTyp.InitData(_currency, tType.Id, tType.Description, tType.Amount, _maxNoOfPax, verifyMalaysianKomuter: true, isVerifyAgeKomuterRequired: true, ageMinKomuter: 48, ageMaxKomuter: 48);
                        //////////    //tkTyp.InitData(_currency, tType.Id, tType.Description, tType.Amount, _maxNoOfPax, verifyMalaysianKomuter: true, isVerifyAgeKomuterRequired: true, ageMinKomuter: 48, ageMaxKomuter: 70);
                        //////////    //tkTyp.InitData(_currency, tType.Id, tType.Description, tType.Amount, _maxNoOfPax, verifyMalaysianKomuter: true, isVerifyAgeKomuterRequired: true, ageMinKomuter: 49, ageMaxKomuter: 70);
                        //////////    //tkTyp.InitData(_currency, tType.Id, tType.Description, tType.Amount, _maxNoOfPax, verifyMalaysianKomuter: true, isVerifyAgeKomuterRequired: true, ageMinKomuter: 0, ageMaxKomuter: 48);
                        //////////    //tkTyp.InitData(_currency, tType.Id, tType.Description, tType.Amount, _maxNoOfPax, verifyMalaysianKomuter: true, isVerifyAgeKomuterRequired: true, ageMinKomuter: 0, ageMaxKomuter: 47);
                        //////////    //tkTyp.InitData(_currency, tType.Id, tType.Description, tType.Amount, _maxNoOfPax, verifyMalaysianKomuter: true, isVerifyAgeKomuterRequired: true, ageMinKomuter: 0, ageMaxKomuter: 10);
                        //////////    //tkTyp.InitData(_currency, tType.Id, tType.Description, tType.Amount, _maxNoOfPax, verifyMalaysianKomuter: true, isVerifyAgeKomuterRequired: true, ageMinKomuter: 0, ageMaxKomuter: 70);
                        //////////}
                        ////////////DEBUG-Testing-END ----------------------------------------------------------

                        tkTyp.OnNoOfPaxChanged += Pax_OnNoOfPaxChanged;
                        tkTyp.LoadTicketType();
                        StkTicketTypeContainer.Children.Add(tkTyp);
                    }
                }
            }

            TxtCurrency.Text = _currency;
            TxtTicketQty.Text = "0";
            TxtTotalAmount.Text = $@"{0: #,###.00}";
            TxtJourneyDesc.Text = (_ticketPackage?.Description) ?? "*";
            TxtAvailableDuration.Text = (_ticketPackage?.Duration) ?? "-";

            System.Windows.Forms.Application.DoEvents();
        }

        public void InitTicketPackage(string currency, int maxNoOfPax, KomuterPackageModel package, ResourceDictionary langResource)
        {
            _langResource = langResource;
            this.Resources.MergedDictionaries.Clear();
            this.Resources.MergedDictionaries.Add(_langResource);
            _currency = currency;
            _ticketPackage = package;
            _maxNoOfPax = maxNoOfPax;
        }

        public int GetNoOfPaxSelected()
        {
            int noOfPaxSeleted = 0;
            foreach (var ctrl in StkTicketTypeContainer.Children)
            {
                if (ctrl is uscKomuterTicketType tType)
                {
                    noOfPaxSeleted += tType.NoOfPaxSelected;
                }
            }
            return noOfPaxSeleted;
        }

        private uscKomuterTicketType GetFreeUscKomuterTicketType()
        {
            uscKomuterTicketType retCtrl = null;
            if (_uscticketTypeControlList.Count == 0)
                retCtrl = new uscKomuterTicketType();
            else
            {
                retCtrl = _uscticketTypeControlList[0];
                _uscticketTypeControlList.RemoveAt(0);
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
                        _uscticketTypeControlList.Add(ctrl);
                    }
                    else
                        nextCtrlInx++;
                }
            } while (StkTicketTypeContainer.Children.Count > nextCtrlInx);
        }
    }
}