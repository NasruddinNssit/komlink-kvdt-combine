using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace kvdt_kiosk.Views.SeatingScreen.New.Kvdt
{
    /// <summary>
    /// Interaction logic for uscKomuterTicketType.xaml
    /// </summary>
    public partial class uscKomuterTicketType : UserControl
    {
        public event EventHandler<NoOfPaxChangedEventArgs> OnNoOfPaxChanged;

        private List<uscPaxButton> _uscPaxButtonList = new List<uscPaxButton>();
        private string _currency = "";
        private string _ticketTypeId = "";
        private string _ticketTypeDescription = "";
        private decimal _ticketPrice = 0.0M;
        private int _maxNoOfPax = 1;
        private bool _verifyMalaysianKomuter = false;
        private bool _isVerifyAgeKomuterRequired = false;
        private int _ageMinKomuter = int.MinValue;
        private int _ageMaxKomuter = int.MaxValue;

        public int NoOfPaxSelected { get; set; }
        public uscKomuterTicketType()
        {
            InitializeComponent();
            WpnPax.Children.Clear();
        }

        private void PaxButton_OnNoOfPaxChanged(object sender, NoOfPaxChangedEventArgs e)
        {
            e.AgreeChanged = true;
            NoOfPaxSelected = e.NoOfPax;
            RaiseOnNoOfPaxChanged();


            void RaiseOnNoOfPaxChanged()
            {
                try
                {
                    OnNoOfPaxChanged?.Invoke(null, e);

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }


        }

        public void LoadTicketType()
        {
            ClearAllPaxButton();
            for (int i = 0; i < _maxNoOfPax; i++)
            {
                uscPaxButton usc = GetFreeUscPaxButton();
                usc.NormalEnabled();
                usc.OnNoOfPaxChanged += PaxButton_OnNoOfPaxChanged;
                usc.NoOfPax = (i + 1);
                WpnPax.Children.Add(usc);
            }

            TxtTicketTypeDesc.Text = _ticketTypeDescription ?? "";
            TxtCurrency.Text = _currency ?? "";
            TxtPrice.Text = $@"{_ticketPrice:#,###.00}";
        }

        public void ReadData(out int selectedNoOfPax, out string currency, out string ticketTypeId, out string ticketTypeDescription, out decimal ticketPrice,
            out bool verifyMalaysianKomuter, out bool isVerifyAgeKomuterRequired, out int ageMinKomuter, out int ageMaxKomuter)
        {
            selectedNoOfPax = NoOfPaxSelected;
            currency = _currency;
            ticketTypeId = _ticketTypeId;
            ticketTypeDescription = _ticketTypeDescription;
            ticketPrice = _ticketPrice;

            verifyMalaysianKomuter = _verifyMalaysianKomuter;
            isVerifyAgeKomuterRequired = _isVerifyAgeKomuterRequired;
            ageMinKomuter = _ageMinKomuter;
            ageMaxKomuter = _ageMaxKomuter;
        }


        public void RecalibrateNoOfPax(int reconsiderMaxNoOfPax)
        {
            foreach (var ctrl in WpnPax.Children)
            {
                if (ctrl is uscPaxButton btnPax)
                {
                    if (btnPax.NoOfPax == NoOfPaxSelected)
                    {
                        //By Pass
                    }
                    else if (btnPax.NoOfPax <= reconsiderMaxNoOfPax)
                        btnPax.NormalEnabled();
                    else
                        btnPax.Disabled();
                }
            }
        }

        public void ResetSelection()
        {
            NoOfPaxSelected = 0;
            foreach (var ctrl in WpnPax.Children)
            {
                if (ctrl is uscPaxButton btnPax)
                {
                    btnPax.NormalEnabled();
                }
            }
        }

        public void InitData(string currency, string ticketTypeId, string ticketTypeDescription, decimal ticketPrice, int maxNoOfPax,
          bool verifyMalaysianKomuter, bool isVerifyAgeKomuterRequired, int ageMinKomuter, int ageMaxKomuter)
        {
            NoOfPaxSelected = 0;
            _currency = currency;
            _ticketTypeId = ticketTypeId;
            _ticketTypeDescription = ticketTypeDescription;
            _ticketPrice = ticketPrice;
            _maxNoOfPax = maxNoOfPax;

            _verifyMalaysianKomuter = verifyMalaysianKomuter;
            _isVerifyAgeKomuterRequired = isVerifyAgeKomuterRequired;
            _ageMinKomuter = ageMinKomuter;
            _ageMaxKomuter = ageMaxKomuter;
        }

        private uscPaxButton GetFreeUscPaxButton()
        {
            uscPaxButton retCtrl = null;
            if (_uscPaxButtonList.Count == 0)
                retCtrl = new uscPaxButton();
            else
            {
                retCtrl = _uscPaxButtonList[0];
                _uscPaxButtonList.RemoveAt(0);
            }

            return retCtrl;
        }

        private void ClearAllPaxButton()
        {
            int nextCtrlInx = 0;
            do
            {
                if (WpnPax.Children.Count > nextCtrlInx)
                {
                    if (WpnPax.Children[nextCtrlInx] is uscPaxButton ctrl)
                    {
                        ctrl.OnNoOfPaxChanged -= PaxButton_OnNoOfPaxChanged;
                        WpnPax.Children.RemoveAt(nextCtrlInx);
                        _uscPaxButtonList.Add(ctrl);
                    }
                    else
                        nextCtrlInx++;
                }
            } while (WpnPax.Children.Count > nextCtrlInx);
        }


    }
}
