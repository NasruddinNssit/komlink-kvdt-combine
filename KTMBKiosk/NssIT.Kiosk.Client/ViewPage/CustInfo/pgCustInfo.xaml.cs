using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI;
using NssIT.Kiosk.Client.Base;
using NssIT.Kiosk.Log.DB.StatusMonitor;
using NssIT.Train.Kiosk.Common.Data;
using NssIT.Train.Kiosk.Common.Data.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// Interaction logic for pgCustInfo.xaml
    /// </summary>
    public partial class pgCustInfo : Page, ICustInfo, IKioskViewPage
    {
        private const string LogChannel = "ViewPage";

        private CustInfoKeyBoardEntry _keyBoardDataEntry = null;
        private PassengerInfoManager _passengerInfoMan = null;

        private LanguageCode _language = LanguageCode.English;
        private ResourceDictionary _langMal = null;
        private ResourceDictionary _langEng = null;

        private pgMyKad _myKadPage = null;
        private pgPromoCodeVerification _promoCodeVerificationPage = null;
        private string _seatBookingId = null;

        private CustSeatDetail[] _departPassengerSeatDetailList = null;
        private CustSeatDetail[] _returnPassengerSeatDetailList = null;
        private TicketTypeModel[] _ticketTypeList = null;
        private string _originStationCode = null;
        private string _originStationName = null;
        private string _destinationStationCode = null;
        private string _destinationStationName = null;

        private string _departTrainSeatModelId = null;
        private string _returnTrainSeatModelId = null;

        public pgCustInfo()
        {
            InitializeComponent();

            _langMal = CommonFunc.GetXamlResource(@"ViewPage\CustInfo\rosCustInfoMalay.xaml");
            _langEng = CommonFunc.GetXamlResource(@"ViewPage\CustInfo\rosCustInfoEnglish.xaml");

            _myKadPage = new pgMyKad();
            _promoCodeVerificationPage = new pgPromoCodeVerification();

            _keyBoardDataEntry = new CustInfoKeyBoardEntry(KbKeys);
            _passengerInfoMan = new PassengerInfoManager(this, TxtError, TxtConfirmWait, StkPassengerInfo);
            _passengerInfoMan.OnPassengerInfoTextBoxGotFocus += OnPassengerInfoTextBox_GotFocus;
            _passengerInfoMan.OnPassengerInfoScanMyKadClick += _passengerInfoMan_OnPassengerInfoScanMyKadClick;
            _passengerInfoMan.OnPromoCodeApplyClick += _passengerInfoMan_OnPromoCodeApplyClick;

            _myKadPage.OnEndScan += _myKadPage_OnEndScan;
            _promoCodeVerificationPage.OnEndPromoCodeVerification += _promoCodeVerificationPage_OnEndPromoCodeVerification;
        }

        public void InitPassengerInfo(UICustInfoAck uiCustInfo)
        {
            GetTicketTypeResult tickTypeRes = (GetTicketTypeResult)uiCustInfo.MessageData;
            TicketTypeModel[] ticketTypeList = tickTypeRes.Data;

            if (uiCustInfo.Session != null)
                _language = uiCustInfo.Session.Language;
            else
                _language = LanguageCode.English;

            UserSession userSession = uiCustInfo.Session;

            _originStationCode = userSession.OriginStationCode;
            _originStationName = userSession.OriginStationName;
            _destinationStationCode = userSession.DestinationStationCode;
            _destinationStationName = userSession.DestinationStationName;

            _departPassengerSeatDetailList = userSession.DepartPassengerSeatDetailList;
            _returnPassengerSeatDetailList = userSession.ReturnPassengerSeatDetailList;
            _seatBookingId = userSession.SeatBookingId;

            _departTrainSeatModelId = userSession.DepartTrainSeatModelId;

            if (_returnPassengerSeatDetailList?.Count() > 0)
                _returnTrainSeatModelId = userSession.ReturnTrainSeatModelId;
            else
                _returnTrainSeatModelId = null;
            
            if ((_departPassengerSeatDetailList == null) || (_departPassengerSeatDetailList.Length == 0))
                throw new Exception("No valid Depart Seat data; (EXIT10000805)");

            if (userSession.TravelMode == TravelMode.DepartOrReturn)
            {
                if ((_returnPassengerSeatDetailList == null) || (_returnPassengerSeatDetailList.Length == 0))
                    throw new Exception("No valid Return Seat data for Depart & Return travel mode; ((EXIT10000806)");

                if ((_returnPassengerSeatDetailList.Length == _departPassengerSeatDetailList.Length) == false)
                    throw new Exception("Number of Seat for Depart and Return trips are not matched; (EXIT10000807)");
            }

            _ticketTypeList = ticketTypeList;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //App.TimeoutManager.ExtendCustomerInfoTimeout(App.CustomerInfoTimeoutExtensionSec);
                App.MainScreenControl.MiniNavigator.AttachToFrame(frmNav, _language);
                App.MainScreenControl.MiniNavigator.IsPreviousVisible = Visibility.Hidden;

                GrdScreenShield.Visibility = Visibility.Collapsed;

                SvPassengerInfoList.ScrollToVerticalOffset(0);

                this.Resources.MergedDictionaries.Clear();
                if (_language == LanguageCode.Malay)
                    this.Resources.MergedDictionaries.Add(_langMal);
                else
                    this.Resources.MergedDictionaries.Add(_langEng);

                App.TimeoutManager.ResetCustomerInfoTimeoutCounter();

                _hasConfirmed = false;

                FrmIdentityEntry.Content = null;
                FrmIdentityEntry.NavigationService.RemoveBackEntry();
                GrdPopUp.Visibility = Visibility.Collapsed;

                _passengerInfoMan.LoadPassengerContainer(_departPassengerSeatDetailList, _returnPassengerSeatDetailList, _ticketTypeList, 
                    _language, _departTrainSeatModelId, _returnTrainSeatModelId,  _originStationCode, _originStationName, _destinationStationCode, _destinationStationName,
                    out TextBox firstFocusTextBox);

                if (firstFocusTextBox != null)
                    _keyBoardDataEntry.FocusedTextBox(firstFocusTextBox);
                else
                    _keyBoardDataEntry.ResetTextBoxFocus();
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg("Error on pgCustInfo.Page_Loaded");
                App.Log.LogError(LogChannel, "-", ex, "EX01", "pgCustInfo.Page_Loaded");
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                App.TimeoutManager.RemoveCustomerInfoTimeoutExtension();

                FrmIdentityEntry.Content = null;
                FrmIdentityEntry.NavigationService.RemoveBackEntry();
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg("Error on pgCustInfo.Page_Unloaded");
                App.Log.LogError(LogChannel, "-", ex, "EX01", "pgCustInfo.Page_Unloaded");
            }
        }

        private void OnPassengerInfoTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is TextBox txtBox)
                {
                    _keyBoardDataEntry.FocusedTextBox(txtBox);
                }
                else if (sender is null)
                {
                    _keyBoardDataEntry.ResetTextBoxFocus();
                }
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg("Error on pgCustInfo.OnPassengerInfoTextBox_GotFocus");
                App.Log.LogError(LogChannel, "-", ex, "EX01", "pgCustInfo.OnPassengerInfoTextBox_GotFocus");
            }
        }

        private bool _hasConfirmed = false;
        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_hasConfirmed == true)
                    return;
                
                bool checkResult = _passengerInfoMan.ConfirmPassengerInfo(out PassengerInfo[] passengerInfoList);

                if (checkResult)
                {
                    App.TimeoutManager.RemoveCustomerInfoTimeoutExtension();

                    App.ShowDebugMsg("pgCustInfo.Confirm_Click : Confirm Success");

                    CustSeatDetail[] submtList = new CustSeatDetail[passengerInfoList.Length];

                    int arrInx = -1;
                    foreach (PassengerInfo pssg in passengerInfoList)
                    {
                        arrInx++;
                        submtList[arrInx] = new CustSeatDetail() 
                        { 
                            Contact = pssg.Contact, 
                            CustIC = pssg.Id, 
                            CustName = pssg.Name, 
                            Gender = pssg.Gender, 
                            TicketType = pssg.TicketType, 
                            DepartPromoCode = pssg.DepartPromoCode, 
                            ReturnPromoCode = pssg.ReturnPromoCode, 
                            PNR = pssg.PNR
                        };
                    }

                    _hasConfirmed = true;
                    Submit(submtList);
                }
                else
                    App.ShowDebugMsg("pgCustInfo.Confirm_Click : Confirm not success");
                
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg("Error on pgCustInfo.Confirm_Click");
                App.Log.LogError(LogChannel, "-", ex, "EX01", "pgCustInfo.Confirm_Click");
            }
        }

        private void Submit(CustSeatDetail[] custSeatDetailList)
        {
            ShieldPage();
            System.Windows.Forms.Application.DoEvents();

            Thread submitWorker = new Thread(new ThreadStart(new Action(() => {
                try
                {
                    // Check Printer Status xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                    bool isPrinterError = false;
                    bool isPrinterWarning = false;
                    string statusDescription = null;
                    string locStateDesc = null;

                    try
                    {
                        App.AppHelp.PrinterApp.GetPrinterStatus(out isPrinterError, out isPrinterWarning, out statusDescription, out locStateDesc);
                    }
                    catch (Exception ex)
                    {
                        App.Log.LogError(LogChannel, "-", ex, "EX02", "pgCustInfo.Submit");
                        throw new Exception($@"{ex.Message}; (EXIT10000808); {NssIT.Kiosk.Client.AppHelper.PrinterErrorTag}");
                    }

                    if (isPrinterError || isPrinterWarning)
                    {
                        StatusHub.GetStatusHub().zNewStatus_IsPrinterStandBy(AppDecorator.DomainLibs.KioskStatus.KioskCommonStatus.No, statusDescription);

                        if (string.IsNullOrWhiteSpace(statusDescription))
                            statusDescription = "Printer Error (X01); (EXIT10000809)";

                        App.Log.LogText(LogChannel, "-", $@"Error; (EXIT10000810); {statusDescription}", "A11", "pgCustInfo.Submit", AppDecorator.Log.MessageType.Error);
                        throw new Exception($@"{statusDescription}; (EXIT10000810); {NssIT.Kiosk.Client.AppHelper.PrinterErrorTag}");
                    }
                    else
                    {
                        StatusHub.GetStatusHub().zNewStatus_IsPrinterStandBy(AppDecorator.DomainLibs.KioskStatus.KioskCommonStatus.Yes, "Printer Standing By");
                    }

                    if (AppDecorator.Config.Setting.GetSetting().DisablePrinterTracking == false)
                    {
                        if (string.IsNullOrWhiteSpace(App.SysParam.FinalizedPrinterName) == true)
                        {
                            App.Log.LogText(LogChannel, "-", $@"Error; (EXIT10000811); Default (or correct) printer not found", "A11", "pgCustInfo.Submit", AppDecorator.Log.MessageType.Error);
                            throw new Exception($@"{statusDescription}; (EXIT10000811); Default (or correct) printer not found; {NssIT.Kiosk.Client.AppHelper.PrinterErrorTag}");
                        }
                    }
                    //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx


                    App.NetClientSvc.SalesService.SubmitPassengerInfo(custSeatDetailList, out bool isServerResponded);

                    if (isServerResponded == false)
                        App.MainScreenControl.Alert(detailMsg: "Local Server not responding (EXIT10000801)");
                }
                catch (Exception ex)
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(_seatBookingId) == false)
                        {
                            App.NetClientSvc.SalesService.RequestSeatRelease(_seatBookingId);
                            Thread.Sleep(1000);
                        }
                    }
                    catch { }

                    try
                    {
                        
                        App.NetClientSvc.SalesService.EndUserSession();
                    }
                    catch { }
                    
                    App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000803)");
                    App.Log.LogError(LogChannel, "", new Exception("(EXIT10000803)", ex), "EX01", "pgCustInfo.Submit");
                }
            })));
            submitWorker.IsBackground = true;
            submitWorker.Start();
        }

        public void SetPromoCodeVerificationResult(UICustPromoCodeVerifyAck codeVerificationAnswer)
        {
            _promoCodeVerificationPage.SetPromoCodeVerificationResult(codeVerificationAnswer);
        }

        public void UpdatePNRTicketTypeResult(UICustInfoPNRTicketTypeAck custPNRTicketTypeResult)
        {
            _myKadPage.UpdatePNRTicketTypeResult(custPNRTicketTypeResult);
        }

        public void RequestAmentPassengerInfo(UICustInfoUpdateFailAck uiFailUpdateCustInfo)
        {
            try
            {
                _hasConfirmed = false;
                GrdScreenShield.Visibility = Visibility.Collapsed;
                App.TimeoutManager.ExtendCustomerInfoTimeout(App.CustomerInfoTimeoutExtensionSec);
                PassengerSubmissionResult sRest = (PassengerSubmissionResult)uiFailUpdateCustInfo.MessageData;
                _passengerInfoMan.RequestAmentPassengerInfo(sRest.Data.UpdatePassengerResult.PassengerDetailErrorModels);
                System.Windows.Forms.Application.DoEvents();
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg("Error on pgCustInfo.RequestAmentPassengerInfo");
                App.Log.LogError(LogChannel, "-", ex, "EX01", "pgCustInfo.RequestAmentPassengerInfo");
            }
        }

        public void ShieldPage()
        {
            GrdScreenShield.Visibility = Visibility.Visible;
        }

        private void _passengerInfoMan_OnPassengerInfoScanMyKadClick(object sender, MyKadScanRequestEventArgs e)
        {
            try
            {
                App.TimeoutManager.ExtendCustomerInfoTimeout(App.CustomerInfoTimeoutExtensionSec);

                uscPassengerInfo passengerInfoCtrl = (uscPassengerInfo)sender;
                GrdPopUp.Visibility = Visibility.Visible;

                System.Windows.Forms.Application.DoEvents();

                //App.TimeoutManager.ExtendCustomerInfoTimeout(App.CustomerInfoTimeoutExtensionSec);

                _myKadPage.InitPageData(_language, passengerInfoCtrl.PassengerLineNo, _seatBookingId, e._tripScheduleSeatLayoutDetails_Ids);
                FrmIdentityEntry.NavigationService.Navigate(_myKadPage);

                System.Windows.Forms.Application.DoEvents();
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg("Error on pgCustInfo.ScanMyKad_Click");
                App.Log.LogError(LogChannel, "-", ex, "EX01", "pgCustInfo.ScanMyKad_Click");
            }
        }

        private void _passengerInfoMan_OnPromoCodeApplyClick(object sender, PromoCodeVerificationEventArgs e)
        {
            try
            {
                App.TimeoutManager.ExtendCustomerInfoTimeout(App.CustomerInfoTimeoutExtensionSec);
                GrdPopUp.Visibility = Visibility.Visible;

                System.Windows.Forms.Application.DoEvents();

                ResourceDictionary resDist;
                if (_language == LanguageCode.Malay)
                    resDist = _langMal;
                else
                    resDist = _langEng;

                _promoCodeVerificationPage.InitPromoCode(e.TrainSeatModelId, e.SeatLayoutModelId, e.TicketTypeId, e.PassengerIC, e.PromoCode, resDist);
                FrmIdentityEntry.NavigationService.Navigate(_promoCodeVerificationPage);

                System.Windows.Forms.Application.DoEvents();
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg("Error on pgCustInfo.ScanMyKad_Click");
                App.Log.LogError(LogChannel, "-", ex, "EX01", "pgCustInfo.ScanMyKad_Click");
            }
        }

        private void _myKadPage_OnEndScan(object sender, EndOfMyKadScanEventArgs e)
        {
            try
            {
                App.TimeoutManager.ExtendCustomerInfoTimeout(App.CustomerInfoTimeoutExtensionSec);

                PassengerIdentity pssgId = null;
                pssgId = e.Identity;
                if (pssgId is null)
                {
                    App.ShowDebugMsg("pgCustInfo._myKadPage_OnEndScan : Unable to read Identity");
                }
                else
                {
                    if (pssgId.IsIDReadSuccess)
                    {
                        App.ShowDebugMsg($@"pgCustInfo._myKadPage_OnEndScan : IC: {pssgId.IdNumber}; Name: {pssgId.Name}; Date Of Birth : {pssgId.DateOfBirth:dd MMM yyyy}; Age : {pssgId.Age}; Gender : {Enum.GetName(typeof(Gender), pssgId.Sex)}");
                    }
                    else if (string.IsNullOrWhiteSpace(pssgId.Message) == false)
                    {
                        App.ShowDebugMsg($@"pgCustInfo._myKadPage_OnEndScan : Error: {pssgId.Message}; ");
                    }
                    else
                    {
                        App.ShowDebugMsg($@"pgCustInfo._myKadPage_OnEndScan : Not response");
                    }
                }

                App.ShowDebugMsg("pgCustInfo._myKadPage_OnEndScan : End of MyKad Scanning");

                this.Dispatcher.Invoke(new Action(() => {
                    FrmIdentityEntry.Content = null;
                    FrmIdentityEntry.NavigationService.RemoveBackEntry();
                    GrdPopUp.Visibility = Visibility.Collapsed;

                    System.Windows.Forms.Application.DoEvents();

                    if ((pssgId != null) && (pssgId.IsIDReadSuccess))
                    {
                        _passengerInfoMan.UpdateMyKadInfo(pssgId);
                    }
                    else
                    {
                        _passengerInfoMan.SetTextBoxFocusWhenMyKadFail();
                    }
                }));

                System.Windows.Forms.Application.DoEvents();
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg("Error on pgCustInfo._myKadPage_OnEndScan");
                App.Log.LogError(LogChannel, "-", ex, "EX01", "pgCustInfo._myKadPage_OnEndScan");
            }
            finally
            {
                System.Windows.Forms.Application.DoEvents();
            }
        }
        private void _promoCodeVerificationPage_OnEndPromoCodeVerification(object sender, EndOfPromoCodeVerificationEventArgs e)
        {
            try
            {
                App.TimeoutManager.ExtendCustomerInfoTimeout(App.CustomerInfoTimeoutExtensionSec);

                if (e.IsSuccess)
                {
                    App.ShowDebugMsg($@"Promo Code Pass Verification.");
                }
                else
                {
                    App.ShowDebugMsg($@"Promo Code fail Verification.");
                }

                this.Dispatcher.Invoke(new Action(() => {
                    FrmIdentityEntry.Content = null;
                    FrmIdentityEntry.NavigationService.RemoveBackEntry();
                    GrdPopUp.Visibility = Visibility.Collapsed;
                    System.Windows.Forms.Application.DoEvents();
                    
                    if (string.IsNullOrWhiteSpace(e.ErrorMessage) == false)
                    {
                        _passengerInfoMan.UpdatePromoCodeVerificationResult(errorMessage: e.ErrorMessage);
                    }
                    else if (e.IsSuccess == true)
                    {
                        _passengerInfoMan.UpdatePromoCodeVerificationResult(isSuccess: true);
                    }
                }));

                System.Windows.Forms.Application.DoEvents();
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg("Error on pgCustInfo._promoCodeVerificationPage_OnEndPromoCodeVerification");
                App.Log.LogError(LogChannel, "-", ex, "EX01", "pgCustInfo._promoCodeVerificationPage_OnEndPromoCodeVerification");
            }
            finally
            {
                System.Windows.Forms.Application.DoEvents();
            }
        }


        private void SvPassengerInfoList_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            App.TimeoutManager.ExtendCustomerInfoTimeout(App.CustomerInfoTimeoutExtensionSec);
        }

        //private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        //{
        //    _passengerInfoMan.ResetBorderFocusEffect((TextBox)sender);
        //}
    }
}
