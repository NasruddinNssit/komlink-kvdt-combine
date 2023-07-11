using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.Client.Base;
using NssIT.Kiosk.Client.ViewPage.CustInfo;
using NssIT.Train.Kiosk.Common.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Packaging;
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

namespace NssIT.Kiosk.Client.ViewPage.Komuter
{
    /// <summary>
    /// Interaction logic for pgKomuterTicketTypeMyKadList.xaml
    /// </summary>
    public partial class pgKomuterTicketMyKadList : Page
    {
        public event EventHandler<TicketSelectionChangedEventArgs> OnOkClick;
        public event EventHandler OnCancelClick;

        private string _logChannel = "ViewPage";

        private List<uscKomuterMyKadGroup> _uscticketTypeControlList = new List<uscKomuterMyKadGroup>();
        private KomuterTicket[] _ticketDetailList = null;
        private ResourceDictionary _langResource = null;
        private LanguageCode _language = LanguageCode.English;
        private uscKomuterMyKad _uscCurrentKomuterMyKad = null;
        private uscKomuterMyKadGroup _uscCurrentKomuterMyKadGroup = null;

        private bool _pageLoaded = false;
        private string _packageId = null;
        private string _journeyDesc = null;
        private string _availableDuration = null;

        private pgKomuterMyKadScanner _myKadPage = null;
        private pgKomuterMyKadScanner.MyKadAppVerificationDelg _myKadAppVerificationDelgHandle = null;
        private pgKomuterMyKadScanner.GetRemoveCardMessageOnSuccessDelg _getRemoveCardMessageOnSuccessDelgHandle = null;

        public pgKomuterTicketMyKadList()
        {
            InitializeComponent();
            _myKadPage = new pgKomuterMyKadScanner();
            StkTicketDetailContainer.Children.Clear();

            _myKadPage.OnEndScan += _myKadPage_OnEndScan;

            _myKadAppVerificationDelgHandle = new pgKomuterMyKadScanner.MyKadAppVerificationDelg(MyKadAppVerificationDelgWorking);
            _getRemoveCardMessageOnSuccessDelgHandle = new pgKomuterMyKadScanner.GetRemoveCardMessageOnSuccessDelg(GetRemovingCardMessageOnSuccessDelgWorking);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _uscCurrentKomuterMyKad = null;
                FrmIdentityEntry.Content = null;
                FrmIdentityEntry.NavigationService.RemoveBackEntry();
                GrdPopUp.Visibility = Visibility.Collapsed;

                LoadTicketDetailList();

                TxtJourneyDesc.Text = _journeyDesc ?? "*";
                TxtAvailableDuration.Text = _availableDuration ?? "*";
                TxtErrorMsg.Text = "";
                TxtErrorMsg.Visibility = Visibility.Collapsed;

                App.TimeoutManager.ResetTimeout();

                _pageLoaded = true;
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg($@"(EXIT10001181); {ex.Message}");
                App.Log.LogError(_logChannel, "-", new Exception($@"(EXIT10001181);", ex), "EX01", "pgKomuterTicketMyKadList.Page_Loaded");
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _pageLoaded = false;

            FrmIdentityEntry.Content = null;
            FrmIdentityEntry.NavigationService.RemoveBackEntry();
        }

        private void MyKad_OnScanMyKad(object sender, KomuterMyKadScanEventArgs e)
        {
            try
            {
                if (_pageLoaded == false)
                    return;

                App.TimeoutManager.ResetTimeout();

                if ((e != null) && (e.KomuterMyKadGroup != null) && (e.KomuterMyKad != null))
                {
                    _uscCurrentKomuterMyKadGroup = e.KomuterMyKadGroup;
                    _uscCurrentKomuterMyKad = e.KomuterMyKad;

                    foreach (var ctrl in StkTicketDetailContainer.Children)
                        if (ctrl is uscKomuterMyKadGroup kadGrp)
                            kadGrp.ResetAllKadEntrySelection();

                    // uscPassengerInfo passengerInfoCtrl = (uscPassengerInfo)sender;
                    GrdPopUp.Visibility = Visibility.Visible;

                    System.Windows.Forms.Application.DoEvents();

                    e.KomuterMyKadGroup.ReadKomuterTicketSummaryInfo(out string ticketTypeDesc, out string ticketTypeId);

                    _myKadPage.InitPageData(_language, _uscCurrentKomuterMyKad.GetLineNo(), 
                        _myKadAppVerificationDelgHandle, _getRemoveCardMessageOnSuccessDelgHandle, 
                        ticketTypeDesc ?? "*");

                    FrmIdentityEntry.NavigationService.Navigate(_myKadPage);
                    System.Windows.Forms.Application.DoEvents();
                }
                else
                {
                    throw new Exception($@"Error; (EXIT10001182); e isNull: {(e != null).ToString()}; KomuterMyKadGroup isNull: {(e.KomuterMyKadGroup != null).ToString()}; KomuterMyKad isNull: {(e.KomuterMyKad != null).ToString()}");
                }
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg($@"(EXIT10001183); {ex.Message}");
                App.Log.LogError(_logChannel, "-", new Exception($@"(EXIT10001183);", ex), "EX01", "pgKomuterTicketMyKadList.Pax_OnNoOfPaxChanged");
            }
        }

        private void _myKadPage_OnEndScan(object sender, EndOfMyKadScanEventArgs e)
        {
            if (_pageLoaded == false) 
                return;

            try
            {
                App.TimeoutManager.ResetTimeout();

                if (_uscCurrentKomuterMyKad is null)
                    throw new Exception("Unable to proceed without Current Komuter MyKad knowledge; (EXIT10001184)");

                PassengerIdentity pssgId = null;
                pssgId = e.Identity;
                if (pssgId is null)
                    App.ShowDebugMsg("pgKomuterTicketMyKadList._myKadPage_OnEndScan : Unable to read Identity");
                else
                {
                    if (pssgId.IsIDReadSuccess)
                        App.ShowDebugMsg($@"pgKomuterTicketMyKadList._myKadPage_OnEndScan : IC: {pssgId.IdNumber}; Name: {pssgId.Name}; Date Of Birth : {pssgId.DateOfBirth:dd MMM yyyy}; Age : {pssgId.Age}; Gender : {Enum.GetName(typeof(Gender), pssgId.Sex)}");
                    else if (string.IsNullOrWhiteSpace(pssgId.Message) == false)
                        App.ShowDebugMsg($@"pgKomuterTicketMyKadList._myKadPage_OnEndScan : Error: {pssgId.Message}; ");
                    else
                        App.ShowDebugMsg($@"pgKomuterTicketMyKadList.ScanMyKad_Click : Not response");
                }

                App.ShowDebugMsg("pgKomuterTicketMyKadList.ScanMyKad_Click : End of MyKad Scanning");

                this.Dispatcher.Invoke(new Action(() => {
                    FrmIdentityEntry.Content = null;
                    FrmIdentityEntry.NavigationService.RemoveBackEntry();
                    GrdPopUp.Visibility = Visibility.Collapsed;

                    System.Windows.Forms.Application.DoEvents();

                    if ((pssgId != null) && (pssgId.IsIDReadSuccess))
                    {
                        _uscCurrentKomuterMyKad.UpdateIdentity(pssgId.Name, pssgId.IdNumber);
                        _uscCurrentKomuterMyKad.DeselecteEntry();

                        if ((CheckOutStandingMyKad(out uscKomuterMyKad pendingMyKad) == true) && (pendingMyKad != null))
                        {
                            pendingMyKad.SelectEntry();
                        }

                        TxtErrorMsg.Text = "";
                        TxtErrorMsg.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        _uscCurrentKomuterMyKad.SelectEntry();
                    }
                }));

                System.Windows.Forms.Application.DoEvents();
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg("Error on _myKadPage_OnEndScan._myKadPage_OnEndScan");
                App.Log.LogError(_logChannel, "-", ex, "EX01", "_myKadPage_OnEndScan._myKadPage_OnEndScan");
            }
            finally
            {
                System.Windows.Forms.Application.DoEvents();
            }
        }

        private void SvMyKadList_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            try
            {
                if (_pageLoaded == false)
                    return;

                App.TimeoutManager.ResetTimeout();
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg($@"(EXIT10001185); {ex.Message}");
                App.Log.LogError(_logChannel, "-", new Exception($@"(EXIT10001185);", ex), "EX01", "pgKomuterTicketMyKadList.SvMyKadList_ScrollChanged");
            }
        }

        public void LoadTicketDetailList()
        {
            ClearAllKomuterTicketDetail();
            if (_ticketDetailList != null)
            {
                if (_ticketDetailList.Length > 0)
                {
                    foreach (KomuterTicket tType in _ticketDetailList)
                    {
                        uscKomuterMyKadGroup tkGrp = GetFreeUscKomuterMyKadGroup();
                        tkGrp.InitData(tType, _langResource);
                        tkGrp.OnScanMyKad += MyKad_OnScanMyKad;
                        tkGrp.LoadDetail();
                        StkTicketDetailContainer.Children.Add(tkGrp);
                    }
                }
            }

            TxtJourneyDesc.Text = _journeyDesc ?? "*";
            TxtAvailableDuration.Text = _availableDuration ?? "-";

            System.Windows.Forms.Application.DoEvents();
        }

        /// <summary>
        /// </summary>
        /// <param name="packageId"></param>
        /// <param name="journeyDesc"></param>
        /// <param name="availableDuration"></param>
        /// <param name="myKadTicketDetailList">Only KomuterTicket with MyKad verifing condition</param>
        /// <param name="langResource"></param>
        public void InitTicketPackage(string packageId, string journeyDesc, string availableDuration, KomuterTicket[] myKadTicketDetailList, ResourceDictionary langResource, LanguageCode language)
        {
            _langResource = langResource;
            this.Resources.MergedDictionaries.Clear();
            this.Resources.MergedDictionaries.Add(_langResource);
            _language = language;

            _ticketDetailList = myKadTicketDetailList;
            _packageId = packageId;
            _journeyDesc = journeyDesc;
            _availableDuration = availableDuration;
        }

        /// <summary>
        /// Return false when fail verification. 
        /// </summary>
        /// <param name="pssgId"></param>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        private bool MyKadAppVerificationDelgWorking(PassengerIdentity pssgId, out string errorMsg)
        {
            errorMsg = null;

            string errMsg2 = null;
            bool isValid = true;
            bool isValidIC = false;
            string wellKnownErrMark = "##EX##";

            this.Dispatcher.Invoke(new Action(() => {
                try
                {
                    //----------------------------------------------------------------------------------------------------
                    // Data Verification
                    if (string.IsNullOrWhiteSpace(pssgId.Name) == true)
                        throw new Exception($@"{wellKnownErrMark}{_langResource["UANBLE_TO_READ_IC_Label"].ToString()}");
                    
                    else if (string.IsNullOrWhiteSpace(pssgId.IdNumber) == true)
                        throw new Exception($@"{wellKnownErrMark}{_langResource["UANBLE_TO_READ_IC_Label"].ToString()}");

                    else if (_uscCurrentKomuterMyKad is null)
                        throw new Exception($@"{wellKnownErrMark}Error (A). {_langResource["RESET_SALES_Label"].ToString()}");

                    else if (_uscCurrentKomuterMyKadGroup is null)
                        throw new Exception($@"{wellKnownErrMark}Error (B). {_langResource["RESET_SALES_Label"].ToString()}");

                    else if (_uscCurrentKomuterMyKadGroup.Ticket is null)
                        throw new Exception($@"{wellKnownErrMark}Error (C). {_langResource["RESET_SALES_Label"].ToString()}");

                    //----------------------------------------------------------------------------------------------------
                    KomuterTicket[] latestValidTicketArr = GetAllTicketItem();
                    //----------------------------------------------------------------------------------------------------
                    // MyKad Checking
                    if (_uscCurrentKomuterMyKadGroup.Ticket.VerifyMalaysianKomuter)
                    {
                        _uscCurrentKomuterMyKad.GetIdentity(out string uscName, out string uscICNo);
                        //------------------------------------------------------------------------------------------------
                        // Checking to avoid IC No. duplicated.
                        if (string.IsNullOrWhiteSpace(uscICNo) == false)
                        {
                            //..when existing usc IC No. is same as latest IC No. ..
                            if (uscICNo.Trim().Equals(pssgId.IdNumber.Trim()))
                            {
                                isValidIC = true;
                            }
                        }

                        // .. when uscICNo is empty/null OR uscICNo != pssgId.IdNumber
                        if (isValidIC == false)
                        {
                            if (latestValidTicketArr?.Length > 0)
                            {
                                if (_uscCurrentKomuterMyKadGroup.Ticket.VerifyMalaysianKomuter == true)
                                {
                                    foreach (KomuterTicket tick in latestValidTicketArr)
                                    {
                                        int sameIdCount = (from tickDet in tick.DetailList
                                                            where (string.IsNullOrWhiteSpace(tickDet.MyKadId) == false) && tickDet.MyKadId.Trim().Equals(pssgId.IdNumber.Trim())
                                                            select tickDet
                                                    ).ToArray().Length;

                                        if (sameIdCount > 0)
                                        {
                                            isValidIC = false;
                                            throw new Exception($@"{wellKnownErrMark}{_langResource["IC_DUPLICATED_Label"].ToString()}");
                                        }
                                    }
                                }
                            }
                        }
                        //------------------------------------------------------------------------------------------------
                        // Checking to avoid age is out of accepted range.
                        if (_uscCurrentKomuterMyKadGroup.Ticket.IsVerifyAgeKomuterRequired)
                        {
                            if (_uscCurrentKomuterMyKadGroup.Ticket.IsVerifyAgeKomuterRequired == true)
                            {
                                if ((_uscCurrentKomuterMyKadGroup.Ticket.AgeMinKomuter > pssgId.Age) || (_uscCurrentKomuterMyKadGroup.Ticket.AgeMaxKomuter < pssgId.Age))
                                {
                                    string firstStr = "";

                                    if (_langResource["AGE_FILTER_0_Label"].ToString().Equals("*") == false)
                                        firstStr = _langResource["AGE_FILTER_0_Label"].ToString() + " ";
                                    
                                    throw new Exception($@"{wellKnownErrMark}{firstStr}{_uscCurrentKomuterMyKadGroup.Ticket.TicketTypeDescription} {_langResource["AGE_FILTER_1_Label"].ToString()} {_uscCurrentKomuterMyKadGroup.Ticket.AgeMinKomuter} {_langResource["AGE_FILTER_2_Label"].ToString()} {_uscCurrentKomuterMyKadGroup.Ticket.AgeMaxKomuter} {_langResource["AGE_FILTER_3_Label"].ToString()}");
                                }
                            }
                        }
                    }
                    //----------------------------------------------------------------------------------------------------
                }
                catch (Exception ex)
                {
                    string msg = (ex.Message ?? "").Trim();

                    if (msg.IndexOf(wellKnownErrMark) >= 0)
                        msg = msg.Replace(wellKnownErrMark, "");
                    else
                        msg = $@"{ex.Message}; (EXIT10001186)";

                    errMsg2 = msg;
                    App.Log.LogError(_logChannel, "*", new Exception(errMsg2, ex), "EX01", "pgKomuterTicketMyKadList.MyKadAppVerificationDelgWorking");
                    isValid = false;
                }
            }));

            errorMsg = errMsg2;
            return isValid;
        }
        
        private void GetRemovingCardMessageOnSuccessDelgWorking(out string removeCardMessage1, out string removeCardMessage2)
        {
            removeCardMessage1 = _langResource["REMOVE_MYKAD_Label"].ToString();
            removeCardMessage2 = "";

            string cardMsg2 = "";

            this.Dispatcher.Invoke(new Action(() => {
                int ticketDetailCount = 0;
                int outstandingCount = 0;

                KomuterTicket[] ticketList = GetAllTicketItem();

                foreach (KomuterTicket tick in ticketList)
                {
                    ticketDetailCount += tick.SelectedNoOfPax;

                    foreach (KomuterTicketDetail tickDet in tick.DetailList)
                    {
                        if (string.IsNullOrEmpty(tickDet.Name) == true)
                            outstandingCount++;
                    }
                }

                if ((ticketDetailCount <= 1) || (outstandingCount == 0))
                {
                    TxtErrorMsg.Text = "";
                    TxtErrorMsg.Visibility = Visibility.Collapsed;
                    return;
                }

                else if (outstandingCount == 1)
                {
                    bool hasOutStanding = CheckOutStandingMyKad(out uscKomuterMyKad outstandingMyKad);

                    if ((hasOutStanding == false) || (outstandingMyKad is null) || (_uscCurrentKomuterMyKad is null))
                    {
                        TxtErrorMsg.Text = "";
                        TxtErrorMsg.Visibility = Visibility.Collapsed;
                        return;
                    }
                    if (_uscCurrentKomuterMyKad?.EntryId.Equals(outstandingMyKad?.EntryId) == true)
                    {
                        TxtErrorMsg.Text = "";
                        TxtErrorMsg.Visibility = Visibility.Collapsed;
                        return;
                    }
                    else
                        cardMsg2 = _langResource["REMOVE_MYKAD2_Label"].ToString();
                }
                else
                    cardMsg2 = _langResource["REMOVE_MYKAD2_Label"].ToString();

            }));

            removeCardMessage2 = cardMsg2;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                App.TimeoutManager.ResetTimeout();

                bool hasOutStanding = CheckOutStandingMyKad(out uscKomuterMyKad pendingMyKad);
                KomuterTicket[] ticketItemList = null;

                //CYA-TEST .. hasOutStanding = false;
                //------

                if (hasOutStanding == false)
                {
                    ticketItemList = GetAllTicketItem();
                    if (ticketItemList?.Length > 0)
                    {
                        TxtErrorMsg.Text = "";
                        TxtErrorMsg.Visibility = Visibility.Collapsed;

                        App.ShowDebugMsg($@"All MyKad finished scanning; Total MyKad Scanned : {ticketItemList.Length}");
                        System.Windows.Forms.Application.DoEvents();

                        RaiseOnOkClick(ticketItemList);
                    }
                }
                else if (pendingMyKad != null)
                {
                    ClearAllMyKadSelection();
                    pendingMyKad.SelectEntry();

                    if (_langResource != null)
                        TxtErrorMsg.Text = _langResource["MYKAD_OUTSTANDING_MSG_Label"].ToString();
                    else
                        TxtErrorMsg.Text = "Still have outstanding MyKad to scan.";

                    TxtErrorMsg.Visibility = Visibility.Visible;

                    System.Windows.Forms.Application.DoEvents();
                }
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg($@"(EXIT10001187); {ex.Message}");
                App.Log.LogError(_logChannel, "-", new Exception($@"(EXIT10001187);", ex), "EX01", "pgKomuterTicketMyKadList.Ok_Click");
            }

            return;
            //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            void RaiseOnOkClick(KomuterTicket[] pTicketItemList)
            {
                try
                {
                    if (_pageLoaded == false)
                        return;

                    KomuterTicket[] selectedTicketArr = pTicketItemList;
                    TicketSelectionChangedEventArgs evtArg = new TicketSelectionChangedEventArgs(_packageId, _journeyDesc, _availableDuration, selectedTicketArr);
                    OnOkClick?.Invoke(null, evtArg);
                }
                catch (Exception ex)
                {
                    App.ShowDebugMsg($@"Unhandled error exception; (EXIT10001188); {ex.Message}");
                    App.Log.LogError(_logChannel, "-", new Exception($@"Unhandled error exception; (EXIT10001188);", ex), "EX01", "pgKomuterTicketMyKadList.RaiseOnOkClick");
                }
            }

            void ClearAllMyKadSelection()
            {
                foreach (var ctrl in StkTicketDetailContainer.Children)
                {
                    try
                    {
                        if (ctrl is uscKomuterMyKadGroup kadGrp)
                        {
                            kadGrp.ResetAllKadEntrySelection();
                        }
                    }
                    catch (Exception ex)
                    {
                        App.ShowDebugMsg($@"Error when read Komuter Ticket Selection data; (EXIT10001189); {ex.Message}");
                        App.Log.LogError(_logChannel, "-", new Exception($@"Error when read Komuter Ticket Selection data; (EXIT10001189);", ex), "EX01", "pgKomuterTicketMyKadList.ClearAllMyKadSelection");
                    }
                }
            }
        }

        private KomuterTicket[] GetAllTicketItem()
        {
            List<KomuterTicket> tickList = new List<KomuterTicket>();

            /////if (CheckOutStandingMyKad(out uscKomuterMyKad pendingMyKad) == false)
            /////    return tickList.ToArray();

            foreach (var ctrl in StkTicketDetailContainer.Children)
            {
                try
                {
                    if (ctrl is uscKomuterMyKadGroup kadGrp)
                    {
                        KomuterTicket tickDetList = kadGrp.ReadLatestTicketInfo();
                        tickList.Add(tickDetList);
                    }
                }
                catch (Exception ex)
                {
                    App.ShowDebugMsg($@"Error when read Komuter Ticket Selection data; (EXIT10001190); {ex.Message}");
                    App.Log.LogError(_logChannel, "-", new Exception($@"Error when read Komuter Ticket Selection data; (EXIT10001190);", ex), "EX01", "pgKomuterTicketMyKadList.GetSelectedTicketItem");
                }
            }
            
            return tickList.ToArray();
        }

        /// <summary>
        /// Return true if outstanding has found.
        /// </summary>
        /// <param name="outstandingMyKad"></param>
        /// <returns></returns>
        private bool CheckOutStandingMyKad(out uscKomuterMyKad outstandingMyKad)
        {
            outstandingMyKad = null;
            foreach (var ctrl in StkTicketDetailContainer.Children)
            {
                try
                {
                    if (ctrl is uscKomuterMyKadGroup myKadGrp)
                    {
                        uscKomuterMyKad myKad = myKadGrp.GetNextOutstandingMyKad();
                        if (myKad != null)
                        {
                            outstandingMyKad = myKad;
                            return true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    App.ShowDebugMsg($@"Error when read Komuter Ticket Selection data; (EXIT10001191); {ex.Message}");
                    App.Log.LogError(_logChannel, "-", new Exception($@"Error when read Komuter Ticket Selection data; (EXIT10001191);", ex), "EX01", "pgKomuterTicketMyKadList.CheckOutStandingMyKad");
                }
            }
            return false;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_pageLoaded == false)
                    return;

                App.TimeoutManager.ResetTimeout();
                RaiseOnCancelClick();
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg($@"(EXIT10001192); {ex.Message}");
                App.Log.LogError(_logChannel, "-", new Exception($@"(EXIT10001192);", ex), "EX01", "pgKomuterTicketMyKadList.Cancel_Click");
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
                    App.ShowDebugMsg($@"Unhandled error exception; (EXIT10001193); {ex.Message}");
                    App.Log.LogError(_logChannel, "-", new Exception($@"Unhandled error exception; (EXIT10001193);", ex), "EX01", "pgKomuterTicketMyKadList.RaiseOnNoOfPaxChanged");
                }
            }
        }

        private void ResetMyKadEntry_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_pageLoaded == false)
                    return;

                App.TimeoutManager.ResetTimeout();

                foreach (var ctrl in StkTicketDetailContainer.Children)
                    if (ctrl is uscKomuterMyKadGroup kadGrp)
                        kadGrp.ResetAllEntry();

                System.Windows.Forms.Application.DoEvents();
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg($@"(EXIT10001194); {ex.Message}");
                App.Log.LogError(_logChannel, "-", new Exception($@"(EXIT10001194);", ex), "EX01", "pgKomuterTicketMyKadList.ResetMyKad_Click");
            }
        }

        private uscKomuterMyKadGroup GetFreeUscKomuterMyKadGroup()
        {
            uscKomuterMyKadGroup retCtrl = null;
            if (_uscticketTypeControlList.Count == 0)
                retCtrl = new uscKomuterMyKadGroup();
            else
            {
                retCtrl = _uscticketTypeControlList[0];
                _uscticketTypeControlList.RemoveAt(0);
            }
            return retCtrl;
        }

        private void ClearAllKomuterTicketDetail()
        {
            int nextCtrlInx = 0;
            do
            {
                if (StkTicketDetailContainer.Children.Count > nextCtrlInx)
                {
                    if (StkTicketDetailContainer.Children[nextCtrlInx] is uscKomuterMyKadGroup ctrl)
                    {
                        ctrl.OnScanMyKad -= MyKad_OnScanMyKad;
                        StkTicketDetailContainer.Children.RemoveAt(nextCtrlInx);
                        _uscticketTypeControlList.Add(ctrl);
                    }
                    else
                        nextCtrlInx++;
                }
            } while (StkTicketDetailContainer.Children.Count > nextCtrlInx);
        }

        
    }
}
