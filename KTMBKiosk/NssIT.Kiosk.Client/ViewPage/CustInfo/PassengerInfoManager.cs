using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using NssIT.Kiosk.Client.Base;
using NssIT.Train.Kiosk.Common.Data;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;

namespace NssIT.Kiosk.Client.ViewPage.CustInfo
{
    public class PassengerInfoManager
    {
        private string _logChannel = "ViewPage";

        public event EventHandler<RoutedEventArgs> OnPassengerInfoTextBoxGotFocus;
        public event EventHandler<MyKadScanRequestEventArgs> OnPassengerInfoScanMyKadClick;
        public event EventHandler<PromoCodeVerificationEventArgs> OnPromoCodeApplyClick;

        private const int _maxPassengerCount = 20;
        private Dispatcher _pageDispatcher = null;

        private System.Windows.Media.Brush _focusBorderEffectColor = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0xFF, 0x00, 0x00));
        private System.Windows.Media.Brush _normalBorderEffectColor = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0xE4, 0xE4, 0xE4));

        private TextBlock _txtError = null;
        private TextBlock _txtConfirmWait = null;

        private bool _hasConfirm = false;
        private int _passengerCount = 1;

        private StackPanel _stkPassengerInfoContainer = null;
        private List<uscPassengerInfo> _uscPassengerInfoList = new List<uscPassengerInfo>();

        private CustSeatDetail[] _departCustSeatDetailList = null;
        private CustSeatDetail[] _returnCustSeatDetailList = null;
        private TicketTypeModel[] _ticketTypeList = null;

        private LanguageCode _language = LanguageCode.English;
        private ResourceDictionary _langMal = null;
        private ResourceDictionary _langEng = null;

        public string _departTrainSeatModelId = null;
        public string _returnTrainSeatModelId = null;

        public PassengerInfoManager(Page page, TextBlock txtError, TextBlock txtConfirmWait, StackPanel stkPassengerInfo)
        {
            _langMal = CommonFunc.GetXamlResource(@"ViewPage\CustInfo\rosCustInfoMalay.xaml");
            _langEng = CommonFunc.GetXamlResource(@"ViewPage\CustInfo\rosCustInfoEnglish.xaml");

            _pageDispatcher = page.Dispatcher;
            _stkPassengerInfoContainer = stkPassengerInfo;
            
            _txtError = txtError;
            _txtConfirmWait = txtConfirmWait;
        }

        private void PassengerInfoTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (OnPassengerInfoTextBoxGotFocus != null)
                    OnPassengerInfoTextBoxGotFocus.Invoke(sender, e);
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg($@"{ex.Message}; (EXIT10000830)");
                App.Log.LogError(_logChannel, "-", new Exception($@"{ex.Message}; (EXIT10000830)", ex), "EX01", "PassengerInfoManager.PassengerInfoTextBox_GotFocus");
            }
        }

        private uscPassengerInfo _lastPassengerInfoForMyKad = null;
        private uscPassengerInfo _lastVerifiedPromoCode = null;

        private void PassengerInfo_OnScanMyKadClick(object sender, MyKadScanRequestEventArgs e)
        {
            if (sender is uscPassengerInfo pssgInfo)
            {
                if (OnPassengerInfoScanMyKadClick != null)
                {
                    try
                    {
                        _lastPassengerInfoForMyKad = pssgInfo;
                        OnPassengerInfoScanMyKadClick.Invoke(pssgInfo, e);
                    }
                    catch (Exception ex)
                    {
                        App.ShowDebugMsg($@"{ex.Message}; (EXIT10000832)");
                        App.Log.LogError(_logChannel, "-", new Exception($@"{ex.Message}; (EXIT10000832)", ex), "EX01", "PassengerInfoManager.PassengerInfo_OnScanMyKadClick");
                    }
                }
            }
        }

        private void PassgInfoCtrl_OnPromoCodeApplyClick(object sender, PromoCodeVerificationEventArgs e)
        {
            if (sender is uscPassengerInfo pssgInfo)
            {
                if (OnPromoCodeApplyClick != null)
                {
                    try
                    {
                        _lastVerifiedPromoCode = pssgInfo;
                        OnPromoCodeApplyClick.Invoke(pssgInfo, e);
                    }
                    catch (Exception ex)
                    {
                        App.ShowDebugMsg($@"{ex.Message}; (EXIT10000833)");
                        App.Log.LogError(_logChannel, "-", new Exception($@"{ex.Message}; (EXIT10000833)", ex), "EX01", "PassengerInfoManager.PassengerInfo_OnScanMyKadClick");
                    }
                }
            }
        }

        public void UpdateMyKadInfo(PassengerIdentity passIdentt)
        {
            if (_lastPassengerInfoForMyKad != null)
            {
                if (passIdentt.IsIDReadSuccess)
                {
                    _lastPassengerInfoForMyKad.PassId = passIdentt.IdNumber;
                    _lastPassengerInfoForMyKad.PassName = passIdentt.Name;
                    _lastPassengerInfoForMyKad.SetNextTextBoxFocusAfterMyKadSuccess();
                    if (passIdentt.Sex == Gender.Male)
                        _lastPassengerInfoForMyKad.Gender = "M";
                    else
                        _lastPassengerInfoForMyKad.Gender = "F";

                    if ((string.IsNullOrWhiteSpace(passIdentt.PNRNo) == false) && (passIdentt.PNRTicketTypeList?.Length > 0))
                    {
                        _lastPassengerInfoForMyKad.SetPNRTicketTypeList(passIdentt.PNRNo, passIdentt.PNRTicketTypeList);
                        _lastPassengerInfoForMyKad.RenderAllTicketTypeUIList(true);
                    }
                }
                _lastPassengerInfoForMyKad = null;
            }
        }

        public void UpdatePromoCodeVerificationResult(bool isSuccess = false, string errorMessage = null)
        {
            if (_lastVerifiedPromoCode != null)
            {
                if (string.IsNullOrWhiteSpace(errorMessage) == false)
                {
                    _lastVerifiedPromoCode.ApplyLastPromoCodeVerificationResult(false, errorMessage);
                }
                else if (isSuccess == true)
                {
                    _lastVerifiedPromoCode.ApplyLastPromoCodeVerificationResult(true, null);
                }
            }
        }

        public void SetTextBoxFocusWhenMyKadFail()
        {
            if (_lastPassengerInfoForMyKad != null)
            {
                _lastPassengerInfoForMyKad.SetTextBoxFocusAfterMyKadFail();
                _lastPassengerInfoForMyKad = null;
            }
        }

        public void RequestAmentPassengerInfo(PassengerDetailErrorModel[] passengerDetailErrorList)
        {
            _hasConfirm = false;
            _txtConfirmWait.Visibility = System.Windows.Visibility.Collapsed;

            foreach (var ctrl in _stkPassengerInfoContainer.Children)
            {
                if (ctrl is uscPassengerInfo passgInfo)
                {
                    passgInfo.HideErrorMessage();
                    passgInfo.ResetAllSelectionBorder();
                    passgInfo.CheckForAmendment(passengerDetailErrorList);
                }
            }

            if (_language == LanguageCode.Malay)
                _txtError.Text = string.Format(_langMal["COMMON_PROMO_CODE_ERROR_Label"]?.ToString());
            else
                _txtError.Text = string.Format(_langEng["COMMON_PROMO_CODE_ERROR_Label"]?.ToString());

            _txtError.Visibility = System.Windows.Visibility.Visible;
        }

        private uscPassengerInfo GetFreeUscPassengerInfo()
        {
            uscPassengerInfo retCtrl = null;
            if (_uscPassengerInfoList.Count == 0)
                retCtrl = new uscPassengerInfo();
            else
            {
                retCtrl = _uscPassengerInfoList[0];
                _uscPassengerInfoList.RemoveAt(0);
            }
            return retCtrl;
        }

        private void ClearAllPassengerInfo()
        {
            int nextCtrlInx = 0;
            do
            {
                if (_stkPassengerInfoContainer.Children.Count > nextCtrlInx)
                {
                    if (_stkPassengerInfoContainer.Children[nextCtrlInx] is uscPassengerInfo ctrl)
                    {
                        ctrl.OnTextBoxGotFocus -= PassengerInfoTextBox_GotFocus;
                        ctrl.OnScanMyKadClick -= PassengerInfo_OnScanMyKadClick;
                        ctrl.OnPromoCodeApplyClick -= PassgInfoCtrl_OnPromoCodeApplyClick;
                        _stkPassengerInfoContainer.Children.RemoveAt(nextCtrlInx);
                        _uscPassengerInfoList.Add(ctrl);
                    }
                    else
                        nextCtrlInx++;
                }
            } while (_stkPassengerInfoContainer.Children.Count > nextCtrlInx);
        }

        public void LoadPassengerContainer(
            CustSeatDetail[] departSeatList, 
            CustSeatDetail[] returnSeatList, 
            TicketTypeModel[] ticketTypeList, 
            LanguageCode language,
            string departTrainSeatModelId,
            string returnTrainSeatModelId,
            string originStationCode, 
            string originStationName, 
            string destinationStationCode, 
            string destinationStationName, 
            out TextBox firstFocusTextBox)
        {
            firstFocusTextBox = null;
            _language = language;

            _departTrainSeatModelId = departTrainSeatModelId;

            if (returnSeatList?.Count() > 0)
                _returnTrainSeatModelId = returnTrainSeatModelId;
            else
                _returnTrainSeatModelId = null;
            
            _departCustSeatDetailList = departSeatList;
            _returnCustSeatDetailList = returnSeatList;
            _ticketTypeList = ticketTypeList;

            if (departSeatList.Length > _maxPassengerCount)
                _passengerCount = _maxPassengerCount;
            else
                _passengerCount = departSeatList.Length;

            _hasConfirm = false;
            _txtError.Text = "";
            _txtError.Visibility = System.Windows.Visibility.Collapsed;
            _txtConfirmWait.Visibility = System.Windows.Visibility.Collapsed;
            ClearAllPassengerInfo();

            bool showMyKadScanner = App.SysParam.PrmMyKadScanner;

            //Read all passenger into ui container
            for (int inx = 0; inx < _passengerCount; inx++)
            {
                List<PassengerSeat> pssgSeatList = new List<PassengerSeat>();

                uscPassengerInfo passgInfoCtrl = GetFreeUscPassengerInfo();
                passgInfoCtrl.LanguageCode = _language;
                _stkPassengerInfoContainer.Children.Add(passgInfoCtrl);
                passgInfoCtrl.OnTextBoxGotFocus += PassengerInfoTextBox_GotFocus;
                passgInfoCtrl.OnScanMyKadClick += PassengerInfo_OnScanMyKadClick;
                passgInfoCtrl.OnPromoCodeApplyClick += PassgInfoCtrl_OnPromoCodeApplyClick;

                passgInfoCtrl.ResetData();
                passgInfoCtrl.PassengerLineNo = (inx + 1).ToString();
                passgInfoCtrl.IsScanMyKadAvailable = showMyKadScanner;

                if (firstFocusTextBox is null)
                    firstFocusTextBox = passgInfoCtrl.FirstFocusTextBox;
                //-------------------------------------------------------------------------------------------------
                // Add Depart/Return Seat Info to The Passenger
                pssgSeatList.Add(new PassengerSeat()
                {
                    SeatLayoutModelId = _departCustSeatDetailList[inx].SeatLayoutModel_Id, 
                    SeatNo = _departCustSeatDetailList[inx].SeatNo, 
                    TicketTypesId = _departCustSeatDetailList[inx].TicketType, 
                    TrainSeatModelId = departTrainSeatModelId, 
                    TravelDirect = TravelDirection.Depart, 
                    OriginStationCode = originStationCode, 
                    OriginStationName = originStationName, 
                    DestinationStationCode = destinationStationCode, 
                    DestinationStationName = destinationStationName
                });
                if (_returnCustSeatDetailList?.Length > 0)
                {
                    pssgSeatList.Add(new PassengerSeat()
                    {
                        SeatLayoutModelId = _returnCustSeatDetailList[inx].SeatLayoutModel_Id,
                        SeatNo = _returnCustSeatDetailList[inx].SeatNo,
                        TicketTypesId = _returnCustSeatDetailList[inx].TicketType,
                        TrainSeatModelId = returnTrainSeatModelId,
                        TravelDirect = TravelDirection.Return,
                        OriginStationCode = originStationCode,
                        OriginStationName = originStationName,
                        DestinationStationCode = destinationStationCode,
                        DestinationStationName = destinationStationName
                    });
                }
                passgInfoCtrl.LoadPassengerTickets(pssgSeatList.ToArray());
                //-------------------------------------------------------------------------------------------------
                // Add Ticket Type to The Passenger
                passgInfoCtrl.SetDefaultTicketTypeList(_ticketTypeList);
                passgInfoCtrl.RenderDefaultTicketTypeUIList();

                //foreach (var tkt in _ticketTypeList)
                //{
                //    passgInfoCtrl.AddTicketType(tkt.Id, tkt.Description);
                //}
            }
        }

        /// <summary>
        /// Return true if successfully confirm.
        /// </summary>
        /// <param name="passengerInfoList"></param>
        /// <returns></returns>
        public bool ConfirmPassengerInfo(out PassengerInfo[] outPassengerInfoList)
        {
            outPassengerInfoList = null;
            List<PassengerInfo> psInfoList = new List<PassengerInfo>();

            if (_hasConfirm == false)
            {
                try
                {
                    foreach (var ctrl in _stkPassengerInfoContainer.Children)
                        if (ctrl is uscPassengerInfo passgInfo)
                        {
                            passgInfo.HideErrorMessage();
                            passgInfo.ResetAllSelectionBorder();
                        }

                    int recInx = 0;
                    foreach (var ctrl in _stkPassengerInfoContainer.Children)
                    {
                        if (ctrl is uscPassengerInfo passgInfo)
                        {
                            PassengerInfo resInfo = passgInfo.GetValidPassengerInfo(out string errorMsg);
                            if (resInfo != null)
                            {
                                resInfo.PassengerInfoIndex = recInx;
                                psInfoList.Add(resInfo);
                            }
                            else
                            {
                                if (string.IsNullOrWhiteSpace(errorMsg))
                                {
                                    errorMsg = "Unknown error; Unable to check passenger info; (EXIT10000831)";
                                }

                                passgInfo.ShowError(errorMsg);
                                throw new Exception(errorMsg);
                            }
                            recInx++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _txtError.Text = ex.Message;
                    _txtError.Visibility = System.Windows.Visibility.Visible;
                    _txtConfirmWait.Visibility = System.Windows.Visibility.Collapsed;
                    return false;
                }

                _hasConfirm = true;
                _txtError.Text = "";
                _txtError.Visibility = System.Windows.Visibility.Collapsed;
                _txtConfirmWait.Visibility = System.Windows.Visibility.Visible;

                outPassengerInfoList = (from row in psInfoList
                                        orderby row.PassengerInfoIndex
                                        select row).ToArray();

                App.ShowDebugMsg($@"ConfirmPassengerInfo -- Success; Data Count: {outPassengerInfoList.Length}");

                return true;
            }
            else
                App.ShowDebugMsg("ConfirmPassengerInfo -- Already Confirm");

            return false;
        }

        //public void UpdateCustomerInfo(string passengerLineNo, string id, string name)
        //{
        //    _pageDispatcher.Invoke(new Action(() => {
        //        foreach (var ctrl in _stkPassengerInfoContainer.Children)
        //        {
        //            if (ctrl is uscPassengerInfo passgInfo)
        //            { 
        //                if (passgInfo.PassengerLineNo.Equals(passengerLineNo))
        //                {
        //                    passgInfo.PassId = id;
        //                    passgInfo.PassName = name;
        //                }
        //            }
        //        }
        //    }));
        //}
    }
}
