using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.Client.Base;
using NssIT.Train.Kiosk.Common.Constants;
using NssIT.Train.Kiosk.Common.Data;
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

namespace NssIT.Kiosk.Client.ViewPage.CustInfo
{
    /// <summary>
    /// Interaction logic for uscPassengerInfo.xaml
    /// </summary>
    public partial class uscPassengerInfo : UserControl
    {
        private string _logChannel = "ViewPage";

        public event EventHandler<RoutedEventArgs> OnTextBoxGotFocus;
        public event EventHandler<MyKadScanRequestEventArgs> OnScanMyKadClick;
        public event EventHandler<PromoCodeVerificationEventArgs> OnPromoCodeApplyClick;

        private static System.Windows.Media.Brush _focusBorderEffectColor = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0xFF, 0x00, 0x00));
        private static System.Windows.Media.Brush _NormalBorderEffectColor = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0xE4, 0xE4, 0xE4));

        private static Thickness _bdSetFocusTickNess = new Thickness(3, 3, 3, 3);
        private static Thickness _bdNormalFocusTickNess = new Thickness(1, 1, 1, 1);
        private static Thickness _bdNormalTicketTypeTickNess = new Thickness(0, 0, 0, 0);

        private static System.Windows.Media.Brush _unSelectedBackGroundColor = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0xE4, 0xE4, 0xE4));
        private static System.Windows.Media.Brush _selectedBackGroundColor = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0x2B, 0x9C, 0xDB));

        private static System.Windows.Media.Brush _deActivatedFontColor = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0x77, 0x77, 0x77));
        //private static System.Windows.Media.Brush _activatedFontColor = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0x64, 0x64, 0x64));
        private static System.Windows.Media.Brush _activatedFontColor = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));

        private List<uscTicketType> _ticketTypeBufferList = new List<uscTicketType>();
        private TicketTypeModel[] _defaultTicketTypeList = new TicketTypeModel[0];
        private TicketTypeModel[] _pnrTicketTypeList = new TicketTypeModel[0];

        private ResourceDictionary _langMal = null;
        private ResourceDictionary _langEng = null;
        private ResourceDictionary _currentLang = null;

        private PassengerSeat _departPassengerSeat = null;
        private PassengerSeat _returnPassengerSeat = null;

        private Button _lastPromoCodeVerifyButton = null;
        private uscTicketType _selectedUscTicketType = null;

        private PassengerSeat[] _passengerSeatList = new PassengerSeat[0];

        private bool _allowEditIdentity = true;
        //private bool _allowEditTicketType = true;

        public uscPassengerInfo()
        {
            InitializeComponent();
            
            _langMal = CommonFunc.GetXamlResource(@"ViewPage\CustInfo\rosCustInfoMalay.xaml");
            _langEng = CommonFunc.GetXamlResource(@"ViewPage\CustInfo\rosCustInfoEnglish.xaml");
            _currentLang = _langEng;

            ClearTicketTypeList();
        }

        private void PassengerInfo_Loaded(object sender, RoutedEventArgs e)
        {
            _lastPromoCodeVerifyButton = null;

            TxtDepartPromoCode.Text = "";
            BtnDepartPromoApply.Visibility = Visibility.Visible;
            BtnDepartPromoReset.Visibility = Visibility.Visible;
            BtnDepartPromoCancel.Visibility = Visibility.Collapsed;

            TxtReturnPromoCode.Text = "";
            BtnReturnPromoApply.Visibility = Visibility.Visible;
            BtnReturnPromoReset.Visibility = Visibility.Visible;
            BtnReturnPromoCancel.Visibility = Visibility.Collapsed;

            HideErrorMessage();
        }

        private void PassengerInfo_LostFocus(object sender, RoutedEventArgs e)
        {
            ResetAllSelectionBorder();
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                ResetNonTextBoxSelectionBorder();

                bool validToEdit = true;

                if (_allowEditIdentity == false)
                {
                    if (sender is TextBox txtBox)
                    {
                        if (txtBox.Equals(TxtPassId) || txtBox.Equals(TxtPassName))
                            validToEdit = false;
                    }
                }

                if (OnTextBoxGotFocus != null)
                {
                    if (validToEdit)
                        OnTextBoxGotFocus.Invoke(sender, e);
                    else
                        OnTextBoxGotFocus.Invoke(null, e);
                }

            }
            catch (Exception ex)
            {
                App.ShowDebugMsg($@"Unhandle error exception; {ex.Message}; (EXIT10000820)");
                App.Log.LogError(_logChannel, "-", new Exception($@"{ex.Message}; (EXIT10000820)", ex), "EX01", "uscPassengerInfo.TextBox_GotFocus");
            }
        }

        private void ScanMyKad_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ResetNonTextBoxSelectionBorder();

                if (BtnMyKad.Content?.ToString().Equals(_currentLang["SCAN_MYKAD_RESETENTRY_Label"].ToString(), StringComparison.InvariantCultureIgnoreCase) == true)
                {
                    BtnMyKad.Content = _currentLang["SCAN_MYKAD_Label"].ToString();

                    _allowEditIdentity = true;
                    //_allowEditTicketType = true;
                    TxtPassId.IsReadOnly = false;
                    TxtPassName.IsReadOnly = false;

                    PNRNo = "";
                    TxtPassId.Text = "";
                    TxtPassName.Text = "";
                    Gender = "";

                    // Recalibrate Ticket Type Selection
                    _selectedUscTicketType?.UnSelect();
                    _selectedUscTicketType = null;
                    RenderDefaultTicketTypeUIList();

                    // Reset Promo Code Verification
                    RequestClarificationState_To_DepartPromoCode();
                    RequestClarificationState_To_ReturnPromoCode();
                }
                else
                {
                    if (OnScanMyKadClick != null)
                    {
                        OnScanMyKadClick.Invoke(this, new MyKadScanRequestEventArgs((from sit in _passengerSeatList select sit.SeatLayoutModelId).ToArray()));
                    }
                }
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg($@"Unhandle error exception; {ex.Message}; (EXIT10000821)");
                App.Log.LogError(_logChannel, "-", new Exception($@"{ex.Message}; (EXIT10000821)", ex), "EX01", "uscPassengerInfo.TextBox_GotFocus");
            }
        }
        
        private void RaiseOnPromoCodeApplyClick(Button sender)
        {
            string senderTag;
            string promoCode;
            TextBox currTextBox = null;
            Border currTextBorder = null;
            PassengerSeat passengerSeat = null;

            App.TimeoutManager.ExtendCustomerInfoTimeout(App.CustomerInfoTimeoutExtensionSec);

            if (sender.Name.Equals(BtnReturnPromoApply.Name))
            {
                senderTag = "Return";
                promoCode = TxtReturnPromoCode.Text;
                currTextBox = TxtReturnPromoCode;
                currTextBorder = BdReturnPromoCode;
                passengerSeat = _returnPassengerSeat;
            }
            else
            {
                senderTag = "Depart";
                promoCode = TxtDepartPromoCode.Text;
                currTextBox = TxtDepartPromoCode;
                currTextBorder = BdDepartPromoCode;
                passengerSeat = _departPassengerSeat;
            }

            // Validation
            if (string.IsNullOrWhiteSpace(PassId))
            {
                string errorMsg;

                if (LanguageCode == LanguageCode.Malay)
                    errorMsg = string.Format(_langMal["ADVICE_IC_NO_Label"]?.ToString(), PassengerLineNo);
                else
                    errorMsg = string.Format(_langEng["ADVICE_IC_NO_Label"]?.ToString(), PassengerLineNo);

                if (string.IsNullOrWhiteSpace(errorMsg))
                    errorMsg = $@"Please enter IC(MyKad)/Passport for Passenger {PassengerLineNo}";

                ShowError(errorMsg);

                ScrollToThisPassenger();

                BdPassId.BorderThickness = _bdSetFocusTickNess;
                BdPassId.BorderBrush = _focusBorderEffectColor;
                TxtPassId.Focus();
                Task.Delay(100).Wait();

                return;
            }
            else if (PassId.Trim().Length < 8)
            {
                string errorMsg;

                if (LanguageCode == LanguageCode.Malay)
                    errorMsg = string.Format(_langMal["CUST_ERROR_IC_NO_Label"]?.ToString(), PassengerLineNo);
                else
                    errorMsg = string.Format(_langEng["CUST_ERROR_IC_NO_Label"]?.ToString(), PassengerLineNo);

                if (string.IsNullOrWhiteSpace(errorMsg))
                    errorMsg = $@"Please enter a minimum of 8 alphanumeric for IC(MyKad)/Passport Passenger {PassengerLineNo}";

                ShowError(errorMsg);

                ScrollToThisPassenger();

                BdPassId.BorderThickness = _bdSetFocusTickNess;
                BdPassId.BorderBrush = _focusBorderEffectColor;
                TxtPassId.Focus();
                Task.Delay(100).Wait();

                return;
            }
            else if (string.IsNullOrWhiteSpace(TicketTypeId) == true)
            {
                string errorMsg;
                if (LanguageCode == LanguageCode.Malay)
                    errorMsg = string.Format(_langMal["ADVICE_TICKET_TYPE_Label"]?.ToString(), PassengerLineNo);
                else
                    errorMsg = string.Format(_langEng["ADVICE_TICKET_TYPE_Label"]?.ToString(), PassengerLineNo);

                ShowError(errorMsg);

                ScrollToThisPassenger();

                BdTicketType.BorderBrush = _focusBorderEffectColor;
                BdTicketType.BorderThickness = _bdSetFocusTickNess;
                BdTicketType.Focus();
                System.Windows.Forms.Application.DoEvents();
                Task.Delay(100).Wait();

                return;
            }
            else if (string.IsNullOrWhiteSpace(promoCode) == true)
            {
                string errorMsg;

                if (sender.Name.Equals(BtnReturnPromoApply.Name))
                {
                    if (LanguageCode == LanguageCode.Malay)
                        errorMsg = string.Format(_langMal["PLEASE_ENTER_RETURN_PROMO_CODE_Label"]?.ToString(), PassengerLineNo);
                    else
                        errorMsg = string.Format(_langEng["PLEASE_ENTER_RETURN_PROMO_CODE_Label"]?.ToString(), PassengerLineNo);
                }
                else
                {
                    if (LanguageCode == LanguageCode.Malay)
                        errorMsg = string.Format(_langMal["PLEASE_ENTER_DEPART_PROMO_CODE_Label"]?.ToString(), PassengerLineNo);
                    else
                        errorMsg = string.Format(_langEng["PLEASE_ENTER_DEPART_PROMO_CODE_Label"]?.ToString(), PassengerLineNo);
                }
                
                ShowError(errorMsg);

                ScrollToThisPassenger();

                currTextBox.Focus();
                currTextBorder.BorderThickness = _bdSetFocusTickNess;
                currTextBorder.BorderBrush = _focusBorderEffectColor;

                System.Windows.Forms.Application.DoEvents();
                Task.Delay(100).Wait();

                return;
            }

            //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            // Proceed to Check Promo Code
            HideErrorMessage();
            ResetAllSelectionBorder();
            _lastPromoCodeVerifyButton = sender;

            if (OnPromoCodeApplyClick != null)
            {
                OnPromoCodeApplyClick.Invoke(this, new PromoCodeVerificationEventArgs(passengerSeat.TrainSeatModelId, passengerSeat.SeatLayoutModelId, TicketTypeId, PassId, promoCode));
            }
        }

        private void TxtPassId_LostFocus(object sender, RoutedEventArgs e)
        {
            BdPassId.BorderBrush = _NormalBorderEffectColor;
            BdPassId.BorderThickness = _bdNormalFocusTickNess;
        }

        private void TxtPassName_LostFocus(object sender, RoutedEventArgs e)
        {
            BdPassName.BorderBrush = _NormalBorderEffectColor;
            BdPassName.BorderThickness = _bdNormalFocusTickNess;
        }

        private void TxtContact_LostFocus(object sender, RoutedEventArgs e)
        {
            BdContact.BorderBrush = _NormalBorderEffectColor;
            BdContact.BorderThickness = _bdNormalFocusTickNess;
        }

        private void BbGender_LostFocus(object sender, RoutedEventArgs e)
        {
            BdGender.BorderBrush = null;
            BdGender.BorderThickness = _bdNormalFocusTickNess;
        }

        private void BdTicketType_LostFocus(object sender, RoutedEventArgs e)
        {
            BdTicketType.BorderBrush = null;
            BdTicketType.BorderThickness = _bdNormalTicketTypeTickNess;
        }

        private void BdMale_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_allowEditIdentity)
            {
                Gender = "M";
                App.TimeoutManager.ExtendCustomerInfoTimeout(App.CustomerInfoTimeoutExtensionSec);
            }
        }

        private void BdFemale_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_allowEditIdentity)
            {
                //App.TimeoutManager.ResetTimeout();
                Gender = "F";
                App.TimeoutManager.ExtendCustomerInfoTimeout(App.CustomerInfoTimeoutExtensionSec);
            }
        }

        private void TxtDepartPromoCode_LostFocus(object sender, RoutedEventArgs e)
        {
            BdDepartPromoCode.BorderBrush = _NormalBorderEffectColor;
            BdDepartPromoCode.BorderThickness = _bdNormalFocusTickNess;
        }

        private void TxtReturnPromoCode_LostFocus(object sender, RoutedEventArgs e)
        {
            BdReturnPromoCode.BorderBrush = _NormalBorderEffectColor;
            BdReturnPromoCode.BorderThickness = _bdNormalFocusTickNess;
        }

        private LanguageCode _language = LanguageCode.English;
        public LanguageCode LanguageCode
        {
            get => _language;
            set
            {
                _language = value;

                this.Resources.MergedDictionaries.Clear();
                if (_language == LanguageCode.Malay)
                {
                    this.Resources.MergedDictionaries.Add(_langMal);
                    _currentLang = _langMal;
                }
                else
                {
                    this.Resources.MergedDictionaries.Add(_langEng);
                    _currentLang = _langEng;
                }
            }
        } 

        public void ShowError(string errorMsg)
        {
            TxtError.Text = errorMsg ?? "";
            TxtError.Visibility = Visibility.Visible;
        }

        public void CheckForAmendment(PassengerDetailErrorModel[] passengerDetailErrorList)
        {
            string errorMsg = null;
            string departPromoCodeError = null;
            string returnPromoCodeError = null;

            if (passengerDetailErrorList?.Length > 0)
            {
                if (_departPassengerSeat != null)
                {
                    PassengerDetailErrorModel errDepartDet = (from det in passengerDetailErrorList
                                                              where det.SeatLayoutModel_Id.Equals(_departPassengerSeat.SeatLayoutModelId)
                                                              && (det.PromoError.Equals(YesNo.Yes))
                                                              select det).FirstOrDefault();

                    if (errDepartDet != null)
                    {
                        errorMsg = string.IsNullOrWhiteSpace(errDepartDet.PromoErrorMessage) ? "Error Promo Code" : errDepartDet.PromoErrorMessage;

                        if (LanguageCode == LanguageCode.Malay)
                            departPromoCodeError = $@"Promo Kod Bertolak tidak sah ({errorMsg})";
                        else
                            departPromoCodeError = $@"Invalid Depart Promo Code ({errorMsg})";
                    }
                }

                if (_returnPassengerSeat != null)
                {
                    PassengerDetailErrorModel errReturnDet = (from det in passengerDetailErrorList
                                                              where det.SeatLayoutModel_Id.Equals(_returnPassengerSeat.SeatLayoutModelId)
                                                              && (det.PromoError.Equals(YesNo.Yes))
                                                              select det).FirstOrDefault();

                    if (errReturnDet != null)
                    {
                        errorMsg = string.IsNullOrWhiteSpace(errReturnDet.PromoErrorMessage) ? "Error Promo Code" : errReturnDet.PromoErrorMessage;

                        if (LanguageCode == LanguageCode.Malay)
                            returnPromoCodeError = $@"Promo Kod Kembali tidak sah ({errorMsg})";
                        else
                            returnPromoCodeError = $@"Invalid Return Promo Code ({errorMsg})";
                    }
                }

                if ((string.IsNullOrWhiteSpace(departPromoCodeError) == false) && (string.IsNullOrWhiteSpace(returnPromoCodeError) == false))
                {
                    BdDepartPromoCode.BorderThickness = _bdSetFocusTickNess;
                    BdDepartPromoCode.BorderBrush = _focusBorderEffectColor;

                    BdReturnPromoCode.BorderThickness = _bdSetFocusTickNess;
                    BdReturnPromoCode.BorderBrush = _focusBorderEffectColor;

                    TxtError.Text = $@"{departPromoCodeError}; {returnPromoCodeError}";
                    TxtError.Visibility = Visibility.Visible;
                }
                else if (string.IsNullOrWhiteSpace(departPromoCodeError) == false)
                {
                    BdDepartPromoCode.BorderThickness = _bdSetFocusTickNess;
                    BdDepartPromoCode.BorderBrush = _focusBorderEffectColor;

                    TxtError.Text = $@"{departPromoCodeError}";
                    TxtError.Visibility = Visibility.Visible;
                }
                else if (string.IsNullOrWhiteSpace(returnPromoCodeError) == false)
                {
                    BdReturnPromoCode.BorderThickness = _bdSetFocusTickNess;
                    BdReturnPromoCode.BorderBrush = _focusBorderEffectColor;

                    TxtError.Text = $@"{returnPromoCodeError}";
                    TxtError.Visibility = Visibility.Visible;
                }
            }
        }

        public void HideErrorMessage()
        {
            TxtError.Text = "";
            TxtError.Visibility = Visibility.Collapsed;
        }

        public void ResetAllSelectionBorder()
        {
            BdPassId.BorderBrush = _NormalBorderEffectColor;
            BdPassId.BorderThickness = _bdNormalFocusTickNess;
            BdPassName.BorderBrush = _NormalBorderEffectColor;
            BdPassName.BorderThickness = _bdNormalFocusTickNess;
            BdContact.BorderBrush = _NormalBorderEffectColor;
            BdContact.BorderThickness = _bdNormalFocusTickNess;
            BdGender.BorderBrush = null;
            BdGender.BorderThickness = _bdNormalFocusTickNess;
            BdTicketType.BorderBrush = null;
            BdTicketType.BorderThickness = _bdNormalTicketTypeTickNess;
            BdDepartPromoCode.BorderBrush = _NormalBorderEffectColor;
            BdDepartPromoCode.BorderThickness = _bdNormalFocusTickNess;
            BdReturnPromoCode.BorderBrush = _NormalBorderEffectColor;
            BdReturnPromoCode.BorderThickness = _bdNormalFocusTickNess;
        }

        private void ResetNonTextBoxSelectionBorder()
        {
            BdGender.BorderBrush = null;
            BdGender.BorderThickness = _bdNormalFocusTickNess;

            BdTicketType.BorderBrush = null;
            BdTicketType.BorderThickness = _bdNormalTicketTypeTickNess;
        }

        public TextBox FirstFocusTextBox { get => TxtPassId; }

        public string PassengerLineNo
        {
            get => TxtPassengerNo.Text;
            set 
            {
                TxtPassengerNo.Text = value ?? "";
            }
        }

        public void LoadPassengerTickets(PassengerSeat[] seatList)
        {
            TxtSeatNumbers.Text = "";
            TxtDepartPromoTrip.Text = "";
            TxtReturnPromoTrip.Text = "";
            TxtPassId.IsReadOnly = false;
            TxtPassName.IsReadOnly = false;
            _departPassengerSeat = null;
            _returnPassengerSeat = null;
            _allowEditIdentity = true;
            //_allowEditTicketType = true;
            _passengerSeatList = new PassengerSeat[0];

            if (seatList is null)
                return;
            //-----------------------------------------------------------------------------------------------------
            // Calibrate MyKad Scanning
            BtnMyKad.Content = _currentLang["SCAN_MYKAD_Label"].ToString();

            List<PassengerSeat> sitList = new List<PassengerSeat>();
            //-----------------------------------------------------------------------------------------------------
            // Show Depart Seat
            PassengerSeat seat = (from sit in seatList
                                  where sit.TravelDirect == TravelDirection.Depart
                                  select sit).FirstOrDefault();

            if ((seat is PassengerSeat depSeat) && (string.IsNullOrWhiteSpace(depSeat.SeatNo) == false))
            {
                TxtSeatNumbers.Text += $@"{depSeat.SeatNo},";
                _departPassengerSeat = depSeat;

                TxtDepartPromoTrip.Text = $@"{depSeat.OriginStationName} > {depSeat.DestinationStationName} - {depSeat.SeatNo}";
                sitList.Add(PassengerSeat.Duplicate(depSeat));
            }

            //-----------------------------------------------------------------------------------------------------
            // Show Return Seat
            seat = (from sit in seatList
                    where sit.TravelDirect == TravelDirection.Return
                    select sit).FirstOrDefault();

            if ((seat is PassengerSeat retSeat) && (string.IsNullOrWhiteSpace(retSeat.SeatNo) == false))
            {
                TxtSeatNumbers.Text += $@"{retSeat.SeatNo},";
                _returnPassengerSeat = retSeat;
                TxtReturnPromoTrip.Text = $@"{retSeat.DestinationStationName} > {retSeat.OriginStationName} - {retSeat.SeatNo}";
                
                GrdReturnPromo.Visibility = Visibility.Visible;
                sitList.Add(PassengerSeat.Duplicate(retSeat));
            }
            else
            {
                GrdReturnPromo.Visibility = Visibility.Collapsed;
            }
            //-----------------------------------------------------------------------------------------------------
            _passengerSeatList = sitList.ToArray();
            //-----------------------------------------------------------------------------------------------------
            if (TxtSeatNumbers.Text.Length > 0)
                TxtSeatNumbers.Text = TxtSeatNumbers.Text.Substring(0, (TxtSeatNumbers.Text.Length - 1));
        }

        public bool _isScanMyKadAvailable = false;
        public bool IsScanMyKadAvailable
        {
            get => _isScanMyKadAvailable;
            set
            {
                if (value == true)
                {
                    BtnMyKad.Visibility = Visibility.Visible;
                }
                else
                {
                    BtnMyKad.Visibility = Visibility.Collapsed;
                }
            }
        }

        /// <summary>
        /// Passenger Id
        /// </summary>
        public string PassId
        {
            get => TxtPassId.Text.Trim();
            set
            {
                TxtPassId.Text = value ?? "";
            }
        }

        /// <summary>
        /// Passenger Name
        /// </summary>
        public string PassName
        {
            get => TxtPassName.Text.Trim();
            set
            {
                TxtPassName.Text = value ?? "";
            }
        }

        public string Contact
        {
            get => TxtContact.Text.Trim();
            set
            {
                TxtContact.Text = value ?? "";
            }
        }

        public string DepartPromoCode
        {
            get => TxtDepartPromoCode.Text.Trim();
            set
            {
                TxtDepartPromoCode.Text = value ?? "";
            }
        }

        public string ReturnPromoCode
        {
            get => TxtReturnPromoCode.Text.Trim();
            set
            {
                TxtReturnPromoCode.Text = value ?? "";
            }
        }

        public string PNRNo
        {
            get => TxtPnr.Text.Trim();
            set
            {
                TxtPnr.Text = (value ?? "").Trim();

                if (string.IsNullOrWhiteSpace(TxtPnr.Text))
                {
                    TxtPNRLable.Visibility = Visibility.Collapsed;
                    GrdPNR.Visibility = Visibility.Collapsed;
                }
                else
                {
                    TxtPNRLable.Visibility = Visibility.Visible;
                    GrdPNR.Visibility = Visibility.Visible;
                }
            }
        }

        private string _gender = "";

        /// <summary>
        /// "M" : Male; "F" : Female; <Blank>/null : not selected
        /// </summary>
        public string Gender
        {
            get => _gender;
            set 
            {
                if (value != null)
                {
                    if (value.ToUpper().Equals("M"))
                        _gender = "M";
                    else if (value.ToUpper().Equals("F"))
                        _gender = "F";
                    else
                        _gender = "";
                }
                else
                    _gender = "";

                BdMale.Background = _unSelectedBackGroundColor;
                BdFemale.Background = _unSelectedBackGroundColor;

                TxtMale.Foreground = _deActivatedFontColor;
                TxtFemale.Foreground = _deActivatedFontColor;
                
                if (string.IsNullOrWhiteSpace(_gender) == false)
                {
                    if (_gender.Equals("M"))
                    {
                        BdMale.Background = _selectedBackGroundColor;
                        TxtMale.Foreground = _activatedFontColor;

                        BdGender.BorderBrush = null;
                        BdGender.BorderThickness = _bdNormalFocusTickNess;
                    }
                    else if (_gender.Equals("F"))
                    {
                        BdFemale.Background = _selectedBackGroundColor;
                        TxtFemale.Foreground = _activatedFontColor;

                        BdGender.BorderBrush = null;
                        BdGender.BorderThickness = _bdNormalFocusTickNess;
                    }
                }
            }
        }

        public string TicketTypeId
        {
            get
            {
                if (_selectedUscTicketType != null)
                    return _selectedUscTicketType.TicketTypeId;
                else
                    return null;
            }
        }

        private uscTicketType GetFreeUscTicketType()
        {
            uscTicketType retCtrl = null;
            if (_ticketTypeBufferList.Count == 0)
                retCtrl = new uscTicketType();
            else
            {
                retCtrl = _ticketTypeBufferList[0];
                _ticketTypeBufferList.RemoveAt(0);
            }
            return retCtrl;
        }
                
        private void TicketType_OnTicketTypeChange(object sender, TicketTypeChangeEventArgs e)
        {
            //if (_allowEditTicketType == false)
            //    return;

            App.TimeoutManager.ExtendCustomerInfoTimeout(App.CustomerInfoTimeoutExtensionSec);

            uscTicketType newUscTicketType = sender as uscTicketType;
            // Reset Existing Ticket Type selection
            if (_selectedUscTicketType != null)
                _selectedUscTicketType.UnSelect();

            BdTicketType.BorderBrush = null;
            BdTicketType.BorderThickness = _bdNormalTicketTypeTickNess;

            // Reset Clarification of DepartPromoCode & ReturnPromoCode
            if ((_selectedUscTicketType is null) 
                ||
                (newUscTicketType.TicketTypeId.Equals(_selectedUscTicketType.TicketTypeId) == false))
            {
                RequestClarificationState_To_DepartPromoCode();
                RequestClarificationState_To_ReturnPromoCode();
            }

            // Set new selected Ticket Type
            _selectedUscTicketType = newUscTicketType;
            _selectedUscTicketType.Select();
        }

        private void ClearTicketTypeList()
        {
            int nextCtrlInx = 0;
            do
            {
                if (WpnTicketType.Children.Count > nextCtrlInx)
                {
                    if (WpnTicketType.Children[nextCtrlInx] is uscTicketType ctrl)
                    {
                        ctrl.OnTicketTypeChange -= TicketType_OnTicketTypeChange;
                        WpnTicketType.Children.RemoveAt(nextCtrlInx);
                        _ticketTypeBufferList.Add(ctrl);
                    }
                    else
                        nextCtrlInx++;
                }
            } while (WpnTicketType.Children.Count > nextCtrlInx);
        }

        public void SetDefaultTicketTypeList(TicketTypeModel[] ticketTypeList)
        {
            if (ticketTypeList is null)
            {
                _defaultTicketTypeList = new TicketTypeModel[0];
            }
            else
                _defaultTicketTypeList = (from tickTyp in ticketTypeList
                                            select TicketTypeModel.Duplicate(tickTyp)
                                            ).ToArray();
        }

        public void SetPNRTicketTypeList(string pnrNo, TicketTypeModel[] pnrTicketTypeList)
        {
            if (pnrTicketTypeList is null)
            {
                _pnrTicketTypeList = new TicketTypeModel[0];
                PNRNo = "";
            }
            else
            {
                List<TicketTypeModel> pnrTKList = new List<TicketTypeModel>();
                foreach(TicketTypeModel pnrTickTyp in pnrTicketTypeList)
                {
                    if ((from delfTickTyp in _defaultTicketTypeList
                         where (delfTickTyp.Id.Equals(pnrTickTyp.Id, StringComparison.InvariantCultureIgnoreCase) == true)
                         select delfTickTyp).ToArray().Length == 0)
                    {
                        pnrTKList.Add(TicketTypeModel.Duplicate(pnrTickTyp));
                    }
                }

                _pnrTicketTypeList = pnrTKList.ToArray();
                if (_pnrTicketTypeList?.Length > 0)
                    PNRNo = pnrNo;
                else
                    PNRNo = "";
            }
        }

        public void RenderDefaultTicketTypeUIList()
        {
            ClearTicketTypeList();
            foreach (TicketTypeModel tkt in _defaultTicketTypeList)
            {
                if (GetFreeUscTicketType() is uscTicketType ctrl)
                {
                    ctrl.UnSelect();
                    ctrl.TicketTypeId = tkt.Id;
                    ctrl.TicketTypeDescription = tkt.Description;
                    ctrl.OnTicketTypeChange += TicketType_OnTicketTypeChange;
                    WpnTicketType.Children.Add(ctrl);
                }
            }
        }

        public void RenderAllTicketTypeUIList(bool withDefaultTicketType = true)
        {
            // Reset Existing Ticket Type selection
            if ((_selectedUscTicketType != null) && (withDefaultTicketType == true))
            {
                _selectedUscTicketType.UnSelect();
                _selectedUscTicketType = null;
            }
            
            ClearTicketTypeList();

            foreach (TicketTypeModel tkt in _defaultTicketTypeList)
            {
                if (GetFreeUscTicketType() is uscTicketType ctrl)
                {
                    ctrl.UnSelect();
                    ctrl.TicketTypeId = tkt.Id;
                    ctrl.TicketTypeDescription = tkt.Description;
                    ctrl.OnTicketTypeChange += TicketType_OnTicketTypeChange;
                    WpnTicketType.Children.Add(ctrl);
                }
            }

            bool setEdit = true;
            foreach (TicketTypeModel tkt in _pnrTicketTypeList)
            {
                if (GetFreeUscTicketType() is uscTicketType ctrl)
                {
                    ctrl.UnSelect();
                    ctrl.TicketTypeId = tkt.Id;
                    ctrl.TicketTypeDescription = tkt.Description;
                    ctrl.OnTicketTypeChange += TicketType_OnTicketTypeChange;
                    WpnTicketType.Children.Add(ctrl);

                    if (setEdit)
                    {
                        _allowEditIdentity = false;
                        //_allowEditTicketType = false;
                        TxtPassId.IsReadOnly = true;
                        TxtPassName.IsReadOnly = true;

                        RequestClarificationState_To_DepartPromoCode();
                        RequestClarificationState_To_ReturnPromoCode();

                        BtnMyKad.Content = _currentLang["SCAN_MYKAD_RESETENTRY_Label"].ToString();

                        if ((withDefaultTicketType) && (tkt.IsDefault) && (_selectedUscTicketType is null))
                        {
                            _selectedUscTicketType = ctrl;
                            ctrl.Select();
                        }
                        setEdit = false;
                    }
                }
            }
        }

        public void AddTicketType(string ticketTypeId, string ticketTypeDescription)
        {
            if (GetFreeUscTicketType() is uscTicketType ctrl)
            {
                ctrl.UnSelect();
                ctrl.TicketTypeId = ticketTypeId;
                ctrl.TicketTypeDescription = ticketTypeDescription;
                ctrl.OnTicketTypeChange += TicketType_OnTicketTypeChange;
                WpnTicketType.Children.Add(ctrl);
            }
        }

        public void SetNextTextBoxFocusAfterMyKadSuccess()
        {
            BdContact.BorderThickness = _bdSetFocusTickNess;
            BdContact.BorderBrush = _focusBorderEffectColor;
            TxtContact.Focus();
        }

        public void SetTextBoxFocusAfterMyKadFail()
        {
            BdPassId.BorderThickness = _bdSetFocusTickNess;
            BdPassId.BorderBrush = _focusBorderEffectColor;
            TxtPassId.Focus();
        }

        public void ResetData()
        {
            PassId = "";
            PassName = "";
            Contact = "";
            Gender = "";
            PNRNo = "";
            DepartPromoCode = "";
            ReturnPromoCode = "";

            TxtTopFocusing.Visibility = Visibility.Collapsed;
            TxtBottomFocusing.Visibility = Visibility.Collapsed;

            _selectedUscTicketType = null;

            ResetAllSelectionBorder();

            ClearTicketTypeList();
        }

        /// <summary>
        /// return a valid PassengerInfo if all data are valid. Else return null.
        /// </summary>
        /// <param name="errorMsg"></param>
        /// <param name="focusingControlElement"></param>
        /// <returns></returns>
        public PassengerInfo GetValidPassengerInfo(out string errorMsg)
        {
            errorMsg = null;

            if (string.IsNullOrWhiteSpace(PassId))
            {
                if (LanguageCode == LanguageCode.Malay)
                    errorMsg = string.Format(_langMal["ADVICE_IC_NO_Label"]?.ToString(), PassengerLineNo);
                else
                    errorMsg = string.Format(_langEng["ADVICE_IC_NO_Label"]?.ToString(), PassengerLineNo);

                if (string.IsNullOrWhiteSpace(errorMsg))
                    errorMsg = $@"Please enter IC(MyKad)/Passport for Passenger {PassengerLineNo}";

                ScrollToThisPassenger();

                TxtPassId.Focus();
                BdPassId.BorderThickness = _bdSetFocusTickNess;
                BdPassId.BorderBrush = _focusBorderEffectColor;
                System.Windows.Forms.Application.DoEvents();
                Task.Delay(100).Wait();
            }
            else if (string.IsNullOrWhiteSpace(PassName) && string.IsNullOrWhiteSpace(errorMsg))
            {
                if (LanguageCode == LanguageCode.Malay)
                    errorMsg = string.Format(_langMal["ADVICE_PASSENGER_NAME_Label"]?.ToString(), PassengerLineNo);
                else
                    errorMsg = string.Format(_langEng["ADVICE_PASSENGER_NAME_Label"]?.ToString(), PassengerLineNo);

                if (string.IsNullOrWhiteSpace(errorMsg))
                    errorMsg = $@"Please enter name of Passenger {PassengerLineNo}";

                ScrollToThisPassenger();

                TxtPassName.Focus();
                BdPassName.BorderThickness = _bdSetFocusTickNess;
                BdPassName.BorderBrush = _focusBorderEffectColor;
                System.Windows.Forms.Application.DoEvents();
                Task.Delay(100).Wait();
            }
            else if ((PassId.Trim().Length < 8) && string.IsNullOrWhiteSpace(errorMsg))
            {
                if (LanguageCode == LanguageCode.Malay)
                    errorMsg = string.Format(_langMal["CUST_ERROR_IC_NO_Label"]?.ToString(), PassengerLineNo);
                else
                    errorMsg = string.Format(_langEng["CUST_ERROR_IC_NO_Label"]?.ToString(), PassengerLineNo);

                if (string.IsNullOrWhiteSpace(errorMsg))
                    errorMsg = $@"Please enter a minimum of 8 alphanumeric for IC(MyKad)/Passport Passenger {PassengerLineNo}";

                ScrollToThisPassenger();

                TxtPassId.Focus();
                BdPassId.BorderThickness = _bdSetFocusTickNess;
                BdPassId.BorderBrush = _focusBorderEffectColor;
                System.Windows.Forms.Application.DoEvents();
                Task.Delay(100).Wait();
            }

            else if ((string.IsNullOrWhiteSpace(Gender)) && string.IsNullOrWhiteSpace(errorMsg))
            {
                if (LanguageCode == LanguageCode.Malay)
                    errorMsg = string.Format(_langMal["ADVICE_GENDER_Label"]?.ToString(), PassengerLineNo);
                else
                    errorMsg = string.Format(_langEng["ADVICE_GENDER_Label"]?.ToString(), PassengerLineNo);

                ScrollToThisPassenger();
                
                BdGender.Focus();
                BdGender.BorderBrush = _focusBorderEffectColor;
                BdGender.BorderThickness = _bdSetFocusTickNess;
                System.Windows.Forms.Application.DoEvents();
                Task.Delay(100).Wait();
            }

            else if (string.IsNullOrWhiteSpace(Contact) && string.IsNullOrWhiteSpace(errorMsg))
            {
                if (LanguageCode == LanguageCode.Malay)
                    errorMsg = string.Format(_langMal["ADVICE_MOBILE_NO_Label"]?.ToString(), PassengerLineNo);
                else
                    errorMsg = string.Format(_langEng["ADVICE_MOBILE_NO_Label"]?.ToString(), PassengerLineNo);

                if (string.IsNullOrWhiteSpace(errorMsg))
                    errorMsg = $@"Please enter contact number for Passenger {PassengerLineNo}";

                ScrollToThisPassenger();
                
                TxtContact.Focus();
                BdContact.BorderThickness = _bdSetFocusTickNess;
                BdContact.BorderBrush = _focusBorderEffectColor;
                System.Windows.Forms.Application.DoEvents();
                Task.Delay(100).Wait();
            }

            else if ((string.IsNullOrWhiteSpace(TicketTypeId)) && string.IsNullOrWhiteSpace(errorMsg))
            {
                if (LanguageCode == LanguageCode.Malay)
                    errorMsg = string.Format(_langMal["ADVICE_TICKET_TYPE_Label"]?.ToString(), PassengerLineNo);
                else
                    errorMsg = string.Format(_langEng["ADVICE_TICKET_TYPE_Label"]?.ToString(), PassengerLineNo);

                ScrollToThisPassenger();
                BdTicketType.Focus();
                BdTicketType.BorderBrush = _focusBorderEffectColor;
                BdTicketType.BorderThickness = _bdSetFocusTickNess;
                System.Windows.Forms.Application.DoEvents();
                Task.Delay(100).Wait();
            }

            else if ((string.IsNullOrWhiteSpace(Contact) == false) && string.IsNullOrWhiteSpace(errorMsg))
            {
                Contact = Contact.Trim();
                System.Windows.Forms.Application.DoEvents();

                int intRes = 0;
                string chr = "";
                string contactNo = "";

                for (int chrInx = 0; chrInx < Contact.Length; chrInx++)
                {
                    chr = Contact.Substring(chrInx, 1);
                    if (int.TryParse(chr, out intRes) == true)
                        contactNo += chr;
                }

                if (contactNo.Trim().Length < 10)
                {
                    if (LanguageCode == LanguageCode.Malay)
                        errorMsg = string.Format(_langMal["CUST_ERROR_MOBILE_NO_Label"]?.ToString(), PassengerLineNo);
                    else
                        errorMsg = string.Format(_langEng["CUST_ERROR_MOBILE_NO_Label"]?.ToString(), PassengerLineNo);

                    if (string.IsNullOrWhiteSpace(errorMsg))
                        errorMsg = $@"Passenger {PassengerLineNo} Mobile No. must be 10 numeric characters and without space";

                    ScrollToThisPassenger();

                    TxtContact.Focus();
                    BdContact.BorderThickness = _bdSetFocusTickNess;
                    BdContact.BorderBrush = _focusBorderEffectColor;
                    System.Windows.Forms.Application.DoEvents();
                    Task.Delay(100).Wait();
                }
            }

            if ((string.IsNullOrWhiteSpace(errorMsg)) && (string.IsNullOrWhiteSpace(DepartPromoCode) == false) && (BtnDepartPromoApply.Visibility == Visibility.Visible))
            {
                if (LanguageCode == LanguageCode.Malay)
                    errorMsg = string.Format(_langMal["PASSENGER_PROMO_CODE_ERROR_Label"]?.ToString(), "Bertolak", PassengerLineNo);
                else
                    errorMsg = string.Format(_langEng["PASSENGER_PROMO_CODE_ERROR_Label"]?.ToString(), "Depart", PassengerLineNo);


                ScrollToThisPassenger();
                
                TxtDepartPromoCode.Focus();
                BdDepartPromoCode.BorderThickness = _bdSetFocusTickNess;
                BdDepartPromoCode.BorderBrush = _focusBorderEffectColor;
                System.Windows.Forms.Application.DoEvents();
                Task.Delay(100).Wait();
            }

            else if ((string.IsNullOrWhiteSpace(errorMsg)) && (string.IsNullOrWhiteSpace(ReturnPromoCode) == false) && (BtnReturnPromoApply.Visibility == Visibility.Visible))
            {
                if (LanguageCode == LanguageCode.Malay)
                    errorMsg = string.Format(_langMal["PASSENGER_PROMO_CODE_ERROR_Label"]?.ToString(), "Kembali", PassengerLineNo);
                else
                    errorMsg = string.Format(_langEng["PASSENGER_PROMO_CODE_ERROR_Label"]?.ToString(), "Return", PassengerLineNo);

                ScrollToThisPassenger();

                TxtReturnPromoCode.Focus();
                BdReturnPromoCode.BorderThickness = _bdSetFocusTickNess;
                BdReturnPromoCode.BorderBrush = _focusBorderEffectColor;
                System.Windows.Forms.Application.DoEvents();
                Task.Delay(100).Wait();
            }

            if (string.IsNullOrWhiteSpace(errorMsg))
            {
                return new PassengerInfo()
                {
                    Contact = this.Contact.Trim(),
                    Gender = this.Gender,
                    Id = this.PassId.Trim(),
                    Name = PassName.Trim(),
                    TicketType = this.TicketTypeId,
                    DepartPromoCode = string.IsNullOrWhiteSpace(this.DepartPromoCode) ? string.Empty : this.DepartPromoCode,
                    ReturnPromoCode = string.IsNullOrWhiteSpace(this.ReturnPromoCode) ? string.Empty : this.ReturnPromoCode,
                    PNR = string.IsNullOrWhiteSpace(this.PNRNo) ? null : this.PNRNo
                };
            }

            return null;
        }

        public void ApplyLastPromoCodeVerificationResult(bool isSuccess, string errorMessage)
        {
            if (_lastPromoCodeVerifyButton == null)
                return;

            if (_lastPromoCodeVerifyButton.Name.Equals(BtnReturnPromoApply.Name))
            {
                if (isSuccess == false)
                {
                    errorMessage = string.IsNullOrWhiteSpace(errorMessage) ? "Unable/Fail to validate" : errorMessage;

                    string err2;
                    if (LanguageCode == LanguageCode.Malay)
                        err2 = string.Format(_langMal["PROMO_CODE_APPLY_ERROR_Label"]?.ToString(), "Kembali");
                    else
                        err2 = string.Format(_langEng["PROMO_CODE_APPLY_ERROR_Label"]?.ToString(), "Return");

                    ShowError($@"{err2}; ({errorMessage})");

                    BdReturnPromoCode.BorderThickness = _bdSetFocusTickNess;
                    BdReturnPromoCode.BorderBrush = _focusBorderEffectColor;
                }
                else
                {
                    SuccessClarification_To_ReturnPromoCode();
                }
            }
            else
            {
                if (isSuccess == false)
                {
                    errorMessage = string.IsNullOrWhiteSpace(errorMessage) ? "Unable/Fail to validate" : errorMessage;

                    string err2;
                    if (LanguageCode == LanguageCode.Malay)
                        err2 = string.Format(_langMal["PROMO_CODE_APPLY_ERROR_Label"]?.ToString(), "Bertolak");
                    else
                        err2 = string.Format(_langEng["PROMO_CODE_APPLY_ERROR_Label"]?.ToString(), "Depart");

                    ShowError($@"{err2}; ({errorMessage})");

                    BdDepartPromoCode.BorderThickness = _bdSetFocusTickNess;
                    BdDepartPromoCode.BorderBrush = _focusBorderEffectColor;
                }
                else
                {
                    SuccessClarificationState_To_DepartPromoCode();
                }
            }
        }

        private void BtnDepartPromoApply_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button btnPromoCode)
                    RaiseOnPromoCodeApplyClick(btnPromoCode);
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg($@"{ex.Message}; (EXIT10000822)");
                App.Log.LogError(_logChannel, "-", new Exception($@"{ex.Message}; (EXIT10000822)", ex), "EX01", "uscPassengerInfo.BtnDepartPromoApply_Click");
            }
        }

        private void BtnDepartPromoReset_Click(object sender, RoutedEventArgs e)
        {
            TxtDepartPromoCode.Text = "";
            App.TimeoutManager.ExtendCustomerInfoTimeout(App.CustomerInfoTimeoutExtensionSec);
        }

        private void BtnDepartPromoCancel_Click(object sender, RoutedEventArgs e)
        {
            TxtDepartPromoCode.Text = "";
            RequestClarificationState_To_DepartPromoCode();
            App.TimeoutManager.ExtendCustomerInfoTimeout(App.CustomerInfoTimeoutExtensionSec);
        }

        private void RequestClarificationState_To_DepartPromoCode()
        {
            BtnDepartPromoApply.Visibility = Visibility.Visible;
            BtnDepartPromoReset.Visibility = Visibility.Visible;
            BtnDepartPromoCancel.Visibility = Visibility.Collapsed;
        }

        private void SuccessClarificationState_To_DepartPromoCode()
        {
            BdDepartPromoCode.BorderBrush = _NormalBorderEffectColor;
            BdDepartPromoCode.BorderThickness = _bdNormalFocusTickNess;

            BtnDepartPromoApply.Visibility = Visibility.Collapsed;
            BtnDepartPromoReset.Visibility = Visibility.Collapsed;
            BtnDepartPromoCancel.Visibility = Visibility.Visible;
        }

        private void BtnReturnPromoApply_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button btnPromoCode)
                    RaiseOnPromoCodeApplyClick(btnPromoCode);
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg($@"{ex.Message}; (EXIT10000823)");
                App.Log.LogError(_logChannel, "-", new Exception($@"{ex.Message}; (EXIT10000823)", ex), "EX01", "uscPassengerInfo.BtnDepartPromoApply_Click");
            }
        }

        private void BtnReturnPromoReset_Click(object sender, RoutedEventArgs e)
        {
            TxtReturnPromoCode.Text = "";
        }

        private void BtnReturnPromoCancel_Click(object sender, RoutedEventArgs e)
        {
            TxtReturnPromoCode.Text = "";
            RequestClarificationState_To_ReturnPromoCode();
        }

        private void RequestClarificationState_To_ReturnPromoCode()
        {
            BtnReturnPromoApply.Visibility = Visibility.Visible;
            BtnReturnPromoReset.Visibility = Visibility.Visible;
            BtnReturnPromoCancel.Visibility = Visibility.Collapsed;
        }

        private void SuccessClarification_To_ReturnPromoCode()
        {
            BdReturnPromoCode.BorderBrush = _NormalBorderEffectColor;
            BdReturnPromoCode.BorderThickness = _bdNormalFocusTickNess;

            BtnReturnPromoApply.Visibility = Visibility.Collapsed;
            BtnReturnPromoReset.Visibility = Visibility.Collapsed;
            BtnReturnPromoCancel.Visibility = Visibility.Visible;
        }

        private void TxtPromoCode_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox txtBox)
            {
                if (txtBox.Name.Equals(TxtDepartPromoCode.Name))
                {
                    RequestClarificationState_To_DepartPromoCode();
                }
                else if (txtBox.Name.Equals(TxtReturnPromoCode.Name))
                {
                    RequestClarificationState_To_ReturnPromoCode();
                }
            }
        }

        private void TxtPassId_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (BtnDepartPromoCancel.Visibility == Visibility.Visible)
            {
                TxtDepartPromoCode.Text = "";
                RequestClarificationState_To_DepartPromoCode();
                App.TimeoutManager.ExtendCustomerInfoTimeout(App.CustomerInfoTimeoutExtensionSec);
            }
            
            if (GrdReturnPromo.Visibility == Visibility.Visible)
            {
                if (BtnReturnPromoCancel.Visibility == Visibility.Visible)
                {
                    TxtReturnPromoCode.Text = "";
                    RequestClarificationState_To_ReturnPromoCode();
                    App.TimeoutManager.ExtendCustomerInfoTimeout(App.CustomerInfoTimeoutExtensionSec);
                }
            }
        }

        private void ScrollToThisPassenger()
        {
            ////Below Code used to Auto scroll view.
            TxtTopFocusing.Visibility = Visibility.Visible;
            TxtTopFocusing.Focus();
            System.Windows.Forms.Application.DoEvents();

            TxtTopFocusing.Visibility = Visibility.Collapsed;
            System.Windows.Forms.Application.DoEvents();

            TxtPassId.Focus();
            System.Windows.Forms.Application.DoEvents();
            Task.Delay(100).Wait();
            ////----------------------------------------------
            ////Below Code used to Auto scroll view.
            TxtBottomFocusing.Visibility = Visibility.Visible;
            TxtBottomFocusing.Focus();
            System.Windows.Forms.Application.DoEvents();

            TxtBottomFocusing.Visibility = Visibility.Collapsed;
            System.Windows.Forms.Application.DoEvents();

            TxtPassId.Focus();
            System.Windows.Forms.Application.DoEvents();
            Task.Delay(100).Wait();
            ////----------------------------------------------
        }
    }
}
