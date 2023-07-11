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
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NssIT.Kiosk.Client.ViewPage.Komuter
{
    /// <summary>
    /// Interaction logic for uscKomuterMyKadTicketType.xaml
    /// </summary>
    public partial class uscKomuterMyKadGroup : UserControl
    {
        private string _logChannel = "ViewPage";

        public event EventHandler<KomuterMyKadScanEventArgs> OnScanMyKad;

        private List<uscKomuterMyKad> _uscMyKadList = new List<uscKomuterMyKad>();

        private KomuterTicket _komuterTicket = null;
        private ResourceDictionary _langResource = null;

        public uscKomuterMyKadGroup()
        {
            InitializeComponent();
            StkMyKadList.Children.Clear();
        }

        public KomuterTicket Ticket => _komuterTicket;

        private void MyKad_OnScanMyKad(object sender, KomuterMyKadScanEventArgs e)
        {
            try
            {
                if (e != null)
                {
                    e.UpdateKomuterMyKadGroup(this);
                    OnScanMyKad?.Invoke(this, e);
                }
            }
            catch (Exception ex)
            {
                App.Log.LogError(_logChannel, "*", ex, "EX01", "uscKomuterMyKadGroup.MyKad_OnScanMyKad");
            }
        }

        public void LoadDetail()
        {
            this.Dispatcher.Invoke(new Action(() => 
            {
                ClearAllTicketDetail();
                _komuterTicket.InitTicketDetail();
                for (int inx = 0; inx < _komuterTicket.SelectedNoOfPax; inx++)
                {
                    uscKomuterMyKad usc = GetFreeUscKomuterMyKad();
                    usc.Init(_langResource, (inx + 1).ToString().Trim());
                    usc.OnScanMyKad += MyKad_OnScanMyKad;
                    StkMyKadList.Children.Add(usc);
                }
                TxtTicketTypeDesc.Text = _komuterTicket.TicketTypeDescription ?? "*";
            }));            
        }

        public void ClearAllEntrySelection()
        {
            this.Dispatcher.Invoke(new Action(() => {
                foreach (var ctrl in StkMyKadList.Children)
                {
                    if (ctrl is uscKomuterMyKad idCard)
                    {
                        idCard.DeselecteEntry();
                    }
                }
            }));
        }

        public KomuterTicket ReadLatestTicketInfo()
        {
            int inx = -1;

            this.Dispatcher.Invoke(new Action(() => {
                foreach (var ctrl in StkMyKadList.Children)
                {
                    if (ctrl is uscKomuterMyKad idCard)
                    {
                        inx++;
                        idCard.GetIdentity(out string name, out string myKadId);
                        _komuterTicket.DetailList[inx].Name = name;
                        _komuterTicket.DetailList[inx].MyKadId = myKadId;
                    }
                }
            }));
            return _komuterTicket;
        }

        //public void ReCalibrateNoOfPax(int reconsiderMaxNoOfPax)
        //{
        //    foreach (var ctrl in WpnPax.Children)
        //    {
        //        if (ctrl is uscPaxButton btnPax)
        //        {
        //            if (btnPax.NoOfPax == NoOfPaxSelected)
        //            {
        //                //By Pass
        //            }
        //            else if (btnPax.NoOfPax <= reconsiderMaxNoOfPax)
        //                btnPax.NormalEnabled();
        //            else
        //                btnPax.Disabled();
        //        }
        //    }
        //}

        public void InitData(KomuterTicket komuterTicket, ResourceDictionary langResource)
        {
            _komuterTicket = komuterTicket;
            _langResource = langResource;
        }

        public void ReadKomuterTicketSummaryInfo(out string ticketTypeDescription, out string ticketTypeId)
        {
            ticketTypeDescription = _komuterTicket?.TicketTypeDescription;
            ticketTypeId = _komuterTicket?.TicketTypeId;
        }

        public void ResetAllEntry()
        {
            this.Dispatcher.Invoke(new Action(() => {
                foreach (var ctrl in StkMyKadList.Children)
                {
                    if (ctrl is uscKomuterMyKad uscMyKad)
                    {
                        uscMyKad.Init(_langResource);
                    }
                }
            }));
        }

        public void ResetAllKadEntrySelection()
        {
            this.Dispatcher.Invoke(new Action(() => {
                foreach (var ctrl in StkMyKadList.Children)
                {
                    if (ctrl is uscKomuterMyKad myKad)
                    {
                        myKad.DeselecteEntry();
                    }
                }
            }));            
        }

        /// <summary>
        /// Return only uscKomuterMyKadthat has outstanding entry.
        /// </summary>
        /// <returns></returns>
        public uscKomuterMyKad GetNextOutstandingMyKad()
        {
            foreach (var ctrl in StkMyKadList.Children)
            {
                if (ctrl is uscKomuterMyKad myKad)
                {
                    myKad.GetIdentity(out string name, out string myKadId);

                    if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(myKadId))
                    {
                        return myKad;
                    }
                }
            }
            return null;
        }

        private uscKomuterMyKad GetFreeUscKomuterMyKad()
        {
            uscKomuterMyKad retCtrl = null;
            if (_uscMyKadList.Count == 0)
                retCtrl = new uscKomuterMyKad();
            else
            {
                retCtrl = _uscMyKadList[0];
                _uscMyKadList.RemoveAt(0);
            }
            return retCtrl;
        }

        private void ClearAllTicketDetail()
        {
            int nextCtrlInx = 0;
            do
            {
                if (StkMyKadList.Children.Count > nextCtrlInx)
                {
                    if (StkMyKadList.Children[nextCtrlInx] is uscKomuterMyKad ctrl)
                    {
                        ctrl.OnScanMyKad -= MyKad_OnScanMyKad;
                        StkMyKadList.Children.RemoveAt(nextCtrlInx);
                        _uscMyKadList.Add(ctrl);
                    }
                    else
                        nextCtrlInx++;
                }
            } while (StkMyKadList.Children.Count > nextCtrlInx);
        }
    }
}
