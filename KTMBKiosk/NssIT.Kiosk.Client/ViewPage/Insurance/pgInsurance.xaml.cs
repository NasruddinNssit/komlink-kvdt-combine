using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI;
using NssIT.Kiosk.Client.Base;
using NssIT.Train.Kiosk.Common.Data.Response;
using System;
using System.Collections.Generic;
using System.Globalization;
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

namespace NssIT.Kiosk.Client.ViewPage.Insurance
{
    /// <summary>
    /// Interaction logic for pgInsurance.xaml
    /// </summary>
    public partial class pgInsurance : Page, IInsurance, IKioskViewPage
    {
        private const string LogChannel = "ViewPage";

        private LanguageCode _language = LanguageCode.English;
        private ResourceDictionary _langMal = null;
        private ResourceDictionary _langEng = null;

        private GetInsuranceModel _insuranceModel = null;
        private string _currency = null;

        private InsuranceCoverageManager _insrCovMan = null;
        private WebImageCacheX _imageCache = null;
        private WebImageCacheX.GetImageFromCache _getImageFromCacheDelgHandle = null;

        private UserSession _latestSession = null;

        private CultureInfo providerMy = new CultureInfo("ms-MY");
        private CultureInfo providerUS = new CultureInfo("en-US");

        private pgToDisagreeInsurance _toDisagreeInsrPage = null;
        private pgToAgreeInsurance _toAgreeInsrPage = null;

        public pgInsurance()
        {
            InitializeComponent();

            _toDisagreeInsrPage = new pgToDisagreeInsurance();
            _toDisagreeInsrPage.OnConfirmDisagreeInsurance += _toDecideInsrPage_OnConfirmDisagreeInsurance;
            _toDisagreeInsrPage.OnAgreeInsurance += _toDisagreeInsrPage_OnAgreeInsurance;

            _toAgreeInsrPage = new pgToAgreeInsurance();
            _toAgreeInsrPage.OnConfirmDisagreeInsurance += _toDecideInsrPage_OnConfirmDisagreeInsurance;
            _toAgreeInsrPage.OnConfirmAgreeInsurance += _toAgreeInsrPage_OnConfirmAgreeInsurance;

            _langMal = CommonFunc.GetXamlResource(@"ViewPage\Insurance\rosInsuranceMalay.xaml");
            _langEng = CommonFunc.GetXamlResource(@"ViewPage\Insurance\rosInsuranceEnglish.xaml");

            _imageCache = new WebImageCacheX(1);
            _insrCovMan = new InsuranceCoverageManager(this, StpInsrCollection, TxtInsurancePricePerPerson, TxtInsuranceCost, TxtInsuranceShortDescNm);
            _getImageFromCacheDelgHandle = new WebImageCacheX.GetImageFromCache(GetCacheImage);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                UnShieldPage();

                SvwInsrCollection.ScrollToVerticalOffset(0);

                ResourceDictionary currLang = null;
                this.Resources.MergedDictionaries.Clear();

                if (_language == LanguageCode.Malay)
                    currLang = _langMal;
                else
                    currLang = _langEng;

                this.Resources.MergedDictionaries.Add(currLang);
                _toDisagreeInsrPage.SetPageConfiguration(currLang);
                _toAgreeInsrPage.SetPageConfiguration(currLang);

                BdOriginPromoDiscount.Visibility = Visibility.Collapsed;
                BdReturnPromoDiscount.Visibility = Visibility.Collapsed;

                string msgX = "";
                msgX = currLang["INSURANCE_MESSAGE_11_Label"].ToString().Replace("\"", "").Replace("'", "").Trim().ToString();
                if (string.IsNullOrWhiteSpace(msgX))
                    TxtInsuranceMessage11.Visibility = Visibility.Collapsed;
                else
                    TxtInsuranceMessage11.Visibility = Visibility.Visible;

                msgX = currLang["INSURANCE_MESSAGE_12_Label"].ToString().Replace("\"", "").Replace("'", "").Trim().ToString();
                if (string.IsNullOrWhiteSpace(msgX))
                    TxtInsuranceMessage12.Visibility = Visibility.Collapsed;
                else
                    TxtInsuranceMessage12.Visibility = Visibility.Visible;

                TxtErrorMsg.Text = "";
                App.MainScreenControl.MiniNavigator.AttachToFrame(frmNav, _language);
                App.MainScreenControl.MiniNavigator.IsPreviousVisible = Visibility.Hidden;

                _insrCovMan.LoadInsurance(_language, _insuranceModel, _currency, 
                    _latestSession.DepartPassengerSeatDetailList.Length, _latestSession.DepartTotalAmount, _latestSession.DepartCurrency,
                    (_latestSession.ReturnPassengerSeatDetailList?.Length > 0) ? _latestSession.ReturnPassengerSeatDetailList.Length : 0, _latestSession.ReturnTotalAmount, _latestSession.ReturnCurrency,
                    _getImageFromCacheDelgHandle);

                TxtOriginDestination.Text = $@"{_latestSession.OriginStationName} > {_latestSession.DestinationStationName}";
                TxtOriginServiceVehicle.Text = $@"{_latestSession.DepartVehicleService} {_latestSession.DepartServiceCategory} - {_latestSession.DepartVehicleNo}";

                TxtInsuranceCurrency.Text = _latestSession.DepartCurrency ?? "RM";
                TxtInsuranceCostCurrency.Text = _latestSession.DepartCurrency ?? "RM";

                if (_latestSession.DepartPassengerDepartDateTime.HasValue)
                {
                    if (_language == LanguageCode.Malay)
                        TxtDepartDepartDateTime.Text = _latestSession.DepartPassengerDepartDateTime?.ToString("dd MMM yyyy hh:mm:ss tt", providerMy);
                    else
                        TxtDepartDepartDateTime.Text = _latestSession.DepartPassengerDepartDateTime?.ToString("dd MMM yyyy hh:mm:ss tt", providerUS);
                }
                else
                    TxtDepartDepartDateTime.Text = "*****";

                if (_latestSession.DepartPassengerSeatDetailList?.Length > 0)
                {
                    TxtOriginTotalTicketCount.Text = string.IsNullOrWhiteSpace(_latestSession.DepartPassengerSeatDetailList?.Length.ToString()) ? "0" : _latestSession.DepartPassengerSeatDetailList.Length.ToString();

                    decimal totalPromoDiscount = (from tick in _latestSession.DepartPassengerSeatDetailList
                                                    select tick.PromoDiscountAmount).Sum();
                    if (totalPromoDiscount > 0)
                    {
                        BdOriginPromoDiscount.Visibility = Visibility.Visible;
                        TxtOriginPromoDiscountCurrency.Text = "- "+ _latestSession.DepartCurrency;
                        TxtOriginPromoDiscountAmount.Text = $@"{totalPromoDiscount:#,##0.00}";
                    }

                    decimal totalTicketPrice = (from tick in _latestSession.DepartPassengerSeatDetailList
                                                select tick.OriginalTicketPrice).Sum();

                    TxtOriginTicketCurrency.Text = _latestSession.DepartCurrency;
                    TxtOriginTicketPrice.Text = $@"{totalTicketPrice:#,##0.00}";
                    TxtOriginNettTotalTicketAmount.Text = $@"{_latestSession.DepartCurrency} {_latestSession.DepartTotalAmount:#,##0.00}";
                }

                if (_latestSession.ReturnPassengerSeatDetailList?.Length > 0)
                {
                    BdReturnSummary.Visibility = Visibility.Visible;

                    TxtReturnDestination.Text = $@"{_latestSession.DestinationStationName} > {_latestSession.OriginStationName}";
                    TxtReturnServiceVehicle.Text = $@"{_latestSession.ReturnVehicleService} {_latestSession.ReturnServiceCategory} - {_latestSession.ReturnVehicleNo}";

                    if (_language == LanguageCode.Malay)
                        TxtReturnDepartDateTime.Text = _latestSession.ReturnPassengerDepartDateTime?.ToString("dd MMM yyyy hh:mm:ss tt", providerMy);
                    else
                        TxtReturnDepartDateTime.Text = _latestSession.ReturnPassengerDepartDateTime?.ToString("dd MMM yyyy hh:mm:ss tt", providerUS);

                    if (_latestSession.ReturnPassengerSeatDetailList?.Length > 0)
                    {
                        TxtReturnTotalTicketCount.Text = string.IsNullOrWhiteSpace(_latestSession.ReturnPassengerSeatDetailList?.Length.ToString()) ? "0" : _latestSession.ReturnPassengerSeatDetailList.Length.ToString();

                        decimal totalPromoDiscount = (from tick in _latestSession.ReturnPassengerSeatDetailList
                                                      select tick.PromoDiscountAmount).Sum();
                        if (totalPromoDiscount > 0)
                        {
                            BdReturnPromoDiscount.Visibility = Visibility.Visible;
                            TxtReturnPromoDiscountCurrency.Text = "- " + _latestSession.ReturnCurrency;
                            TxtReturnPromoDiscountAmount.Text = $@"{totalPromoDiscount:#,##0.00}";
                        }

                        decimal totalTicketPrice = (from tick in _latestSession.ReturnPassengerSeatDetailList
                                                    select tick.OriginalTicketPrice).Sum();

                        TxtReturnTicketCurrency.Text = _latestSession.ReturnCurrency;
                        TxtReturnTotalTicketAmount.Text = $@"{totalTicketPrice:#,##0.00}";
                        TxtReturnNettTotalTicketAmount.Text = $@"{_latestSession.ReturnCurrency} {_latestSession.ReturnTotalAmount:#,##0.00}";
                    }
                }
                else
                {
                    BdReturnSummary.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg("Error on pgInsurance.Page_Loaded");
                App.Log.LogError(LogChannel, "-", ex, "EX01", "pgInsurance.Page_Loaded");
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            UnShieldPage();
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                App.TimeoutManager.ResetTimeout();

                TxtErrorMsg.Text = "";

                StkInProgress.Visibility = Visibility.Collapsed;

                frmExcludeInsurance.Content = null;
                frmExcludeInsurance.NavigationService.RemoveBackEntry();
                System.Windows.Forms.Application.DoEvents();

                frmExcludeInsurance.NavigationService.Navigate(_toAgreeInsrPage);
                frmExcludeInsurance.Visibility = Visibility.Visible;
                GrdScreenShield.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                TxtErrorMsg.Text = ex.Message;
                App.Log.LogError(LogChannel, "-", ex, "EX01", "pgInsurance.Confirm_Click");
            }
        }

        private void WithoutInsurance_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                App.TimeoutManager.ResetTimeout();

                TxtErrorMsg.Text = "";

                StkInProgress.Visibility = Visibility.Collapsed;

                frmExcludeInsurance.Content = null;
                frmExcludeInsurance.NavigationService.RemoveBackEntry();
                System.Windows.Forms.Application.DoEvents();

                frmExcludeInsurance.NavigationService.Navigate(_toDisagreeInsrPage);
                frmExcludeInsurance.Visibility = Visibility.Visible;
                GrdScreenShield.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                TxtErrorMsg.Text = ex.Message;
                App.Log.LogError(LogChannel, "-", ex, "EX01", "pgInsurance.WithoutInsurance_Click");
            }
        }

        private void _toDecideInsrPage_OnConfirmDisagreeInsurance(object sender, EventArgs e)
        {
            try
            {
                App.TimeoutManager.ResetTimeout();
                UnShieldPage();
                TxtErrorMsg.Text = "";

                System.Windows.Forms.Application.DoEvents();

                SubmitWithoutInsurance();
                ShieldPage();
            }
            catch (Exception ex)
            {
                TxtErrorMsg.Text = ex.Message;
                App.Log.LogError(LogChannel, "-", ex, "EX01", "pgInsurance._toDecideInsrPage_OnConfirmDisagreeInsurance");
            }
        }

        private void _toDisagreeInsrPage_OnAgreeInsurance(object sender, EventArgs e)
        {
            try
            {
                App.TimeoutManager.ResetTimeout();
                TxtErrorMsg.Text = "";
                UnShieldPage();
            }
            catch (Exception ex)
            {
                TxtErrorMsg.Text = ex.Message;
                App.Log.LogError(LogChannel, "-", ex, "EX01", "pgInsurance._toDisagreeInsrPage_OnAgreeInsurance");
            }
        }

        private void _toAgreeInsrPage_OnConfirmAgreeInsurance(object sender, EventArgs e)
        {
            try
            {
                App.TimeoutManager.ResetTimeout();
                UnShieldPage();
                TxtErrorMsg.Text = "";

                System.Windows.Forms.Application.DoEvents();

                SubmitInsurance();
                ShieldPage();
            }
            catch (Exception ex)
            {
                TxtErrorMsg.Text = ex.Message;
                App.Log.LogError(LogChannel, "-", ex, "EX01", "pgInsurance._toAgreeInsrPage_OnConfirmAgreeInsurance");
            }
        }

        public void InitInsuranceInfo(UIETSInsuranceListAck uiInsrLstAck)
        {
            _latestSession = uiInsrLstAck.Session;
            _language = uiInsrLstAck.Session.Language;
            _insuranceModel = ((GetInsuranceResult)uiInsrLstAck.MessageData).Data;
            _currency = uiInsrLstAck.Session.DepartCurrency ?? "RM";
        }

        private async Task<BitmapImage> GetCacheImage(string imageUrl)
        {
            return await _imageCache.GetImage(imageUrl);
        }

        private void SubmitInsurance()
        {
            string insrHdId = _insrCovMan.GetSelectedInsuranceHeaderId();

            if (string.IsNullOrWhiteSpace(insrHdId) == true)
                throw new Exception("Please select a valid insurance");

            Thread submitWorker = new Thread(new ThreadStart(new Action(() => {
                try
                {
                    App.NetClientSvc.SalesService.SubmitETSInsurance(_latestSession.SeatBookingId, insrHdId, isAgreeToBuyInsurance: true, out bool isServerResponded);

                    if (isServerResponded == false)
                    {
                        App.NetClientSvc.SalesService.EndUserSession();
                        App.MainScreenControl.Alert(detailMsg: "Local Server not responding (EXIT10000813)");
                    }
                }
                catch (Exception ex)
                {
                    try
                    {

                        App.NetClientSvc.SalesService.EndUserSession();
                    }
                    catch { }

                    App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000814)");
                    App.Log.LogError(LogChannel, "", new Exception("(EXIT10000814)", ex), "EX01", "pgInsurance.SubmitInsurance");
                }
            })));
            submitWorker.IsBackground = true;
            submitWorker.Start();
        }

        private void SubmitWithoutInsurance()
        {
            Thread submitWorker = new Thread(new ThreadStart(new Action(() => {
                try
                {
                    App.NetClientSvc.SalesService.SubmitETSInsurance(_latestSession.SeatBookingId, null, isAgreeToBuyInsurance: false, out bool isServerResponded);

                    if (isServerResponded == false)
                    {
                        App.NetClientSvc.SalesService.EndUserSession();
                        App.MainScreenControl.Alert(detailMsg: "Local Server not responding (EXIT10000813)");
                    }
                }
                catch (Exception ex)
                {
                    try
                    {

                        App.NetClientSvc.SalesService.EndUserSession();
                    }
                    catch { }

                    App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000814)");
                    App.Log.LogError(LogChannel, "", new Exception("(EXIT10000814)", ex), "EX01", "pgInsurance.SubmitInsurance");
                }
            })));
            submitWorker.IsBackground = true;
            submitWorker.Start();
        }

        public void ShieldPage()
        {
            frmExcludeInsurance.Visibility = Visibility.Collapsed;
            GrdScreenShield.Visibility = Visibility.Visible;
            StkInProgress.Visibility = Visibility.Visible;
        }

        private void UnShieldPage()
        {
            frmExcludeInsurance.Content = null;
            frmExcludeInsurance.NavigationService.RemoveBackEntry();
            frmExcludeInsurance.Visibility = Visibility.Collapsed;

            GrdScreenShield.Visibility = Visibility.Collapsed;
            StkInProgress.Visibility = Visibility.Collapsed;
        }

        private void SvInsuranceList_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            try
            {
                App.TimeoutManager.ResetTimeout();
            }
            catch (Exception ex)
            {
                App.Log.LogError(LogChannel, "-", ex, "EX01", "pgInsurance.SvInsuranceList_ScrollChanged");
            }
        }
    }
}
