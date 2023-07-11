using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using NssIT.Kiosk.Client.Base;
using NssIT.Kiosk.Tools.ThreadMonitor;
using NssIT.Train.Kiosk.Common.Data;
using NssIT.Train.Kiosk.Common.Data.Response.BTnG;
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
using static NssIT.Kiosk.Client.ViewPage.Payment.uscPaymentGateway;

namespace NssIT.Kiosk.Client.ViewPage.Payment
{
    /// <summary>
    /// Interaction logic for pgPaymentInfo2.xaml
    /// </summary>
    public partial class pgPaymentInfo2 : Page
    {
        public delegate void StartCreditCardPayWavePaymentDelg();
        
        private string _logChannel = "Payment";
        private UserSession _userSession = null;
        private PaymentSummaryItemCalibrator _paySummaryItemCalibrator = null;

        private StartCreditCardPayWavePaymentDelg _startCreditCardPayWavePaymentDelgHandle = null;
        private StartBTngPaymentDelg _startBTngPaymentDelgHandle = null;

        private LanguageCode _language = LanguageCode.English;
        private ResourceDictionary _langMal = null;
        private ResourceDictionary _langEng = null;

        private WebImageCacheX _imageCache = null;

        public pgPaymentInfo2()
        {
            InitializeComponent();

            _langMal = CommonFunc.GetXamlResource(@"ViewPage\Payment\rosPaymentMalay.xaml");
            _langEng = CommonFunc.GetXamlResource(@"ViewPage\Payment\rosPaymentEnglish.xaml");

            _paySummaryItemCalibrator = new PaymentSummaryItemCalibrator(StkSummaryItem);
            _imageCache = new WebImageCacheX(12);
        }

        private RunThreadMan _loadBTnGThreadWorker = null;
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _imageCache.ClearCacheOnTimeout();

                App.MainScreenControl.MiniNavigator.AttachToFrame(frmNav, _language);
                App.MainScreenControl.MiniNavigator.IsPreviousVisible = Visibility.Hidden;

                ResourceDictionary langRes = (_language == LanguageCode.Malay) ? _langMal : _langEng;

                this.Resources.MergedDictionaries.Clear();
                this.Resources.MergedDictionaries.Add(langRes);

                _paySummaryItemCalibrator.LoadCalibrator(_userSession, langRes);
                SitPaySumm.ShowPaymentSummary(_userSession.TradeCurrency, _userSession.GrossTotal, langRes);

                ClearWrpPaymentGatewayContainer();
                BdLoadingeWallet.Visibility = Visibility.Visible;

                BdBoost.Visibility = Visibility.Collapsed;
                BdTng.Visibility = Visibility.Collapsed;
                
                LoadBTnGOption();
            }
            catch(Exception ex)
            {
                App.ShowDebugMsg($@"Error: {ex.Message}; pgPaymentInfo2.Page_Loaded");
                App.Log.LogError(_logChannel, "-", ex, "EX02", "pgPaymentInfo2.Page_Loaded");
                App.MainScreenControl.Alert(detailMsg: $@"Fail to start payment; (EXIT10000918)");
            }
            return;
            //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            void LoadBTnGOption()
            {
                if ((_loadBTnGThreadWorker != null) && (_loadBTnGThreadWorker.IsEnd == false))
                    _loadBTnGThreadWorker.AbortRequest(out _, 3000);

                _loadBTnGThreadWorker = new RunThreadMan(new ThreadStart(LoadBTnGOptionThreadWorking), "pgPaymentInfo2.LoadBTnGOption", 70, _logChannel);
            }

            void LoadBTnGOptionThreadWorking()
            {
                try
                {
                    bool dataFound = false;

                    if (App.NetClientSvc.BTnGService.QueryAllPaymentGateway(out BTnGGetPaymentGatewayResult payGateResult, out bool isServerResponded) == true)
                    {
                        if (payGateResult.Data?.PaymentGatewayList?.Length > 0)
                        {
                            this.Dispatcher.Invoke(new Action(() =>
                            {
                                foreach (BTnGPaymentGatewayDetailModel payGate in payGateResult.Data.PaymentGatewayList)
                                {
                                    dataFound = true;

                                    uscPaymentGateway pgt = GetFreeUscPaymentGateway();
                                    pgt.InitBTnGPaymentGateway(payGate.PaymentGateway, payGate.PaymentGatewayName, payGate.Logo, _imageCache, payGate.PaymentMethod, _startBTngPaymentDelgHandle);
                                    WrpPaymentGatewayContainer.Children.Add(pgt);
                                }
                                System.Windows.Forms.Application.DoEvents();
                            }));
                        }
                    }

                    if (dataFound)
                        App.ShowDebugMsg("pgPaymentInfo2.LoadBTnGOptionThreadWorking : Payment Gateway List found");
                    else
                        App.ShowDebugMsg("pgPaymentInfo2.LoadBTnGOptionThreadWorking : No Payment Gateway has found");
                }
                catch (ThreadAbortException ex2)
                {
                    App.ShowDebugMsg("pgPaymentInfo2.LoadBTnGOptionThreadWorking : Query Aborted");
                }
                catch (Exception ex)
                {
                    App.ShowDebugMsg(ex.ToString());
                }
                finally
                {
                    this.Dispatcher.Invoke(new Action(() =>
                    {
                        BdLoadingeWallet.Visibility = Visibility.Collapsed;
                    }));
                }
            }
        }

        public void InitPaymentInfo(UserSession userSession, StartCreditCardPayWavePaymentDelg startCreditCardPayWavePaymentDelgHandle, StartBTngPaymentDelg startBTngPaymentDelgHandle)
        {
            _language = (userSession != null ? userSession.Language : LanguageCode.English);
            _userSession = userSession;
            _startCreditCardPayWavePaymentDelgHandle = startCreditCardPayWavePaymentDelgHandle;
            _startBTngPaymentDelgHandle = startBTngPaymentDelgHandle;
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _userSession = null;
        }

        private void CreditCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (_startCreditCardPayWavePaymentDelgHandle != null)
            {
                _startCreditCardPayWavePaymentDelgHandle();
            }
        }

        private List<uscPaymentGateway> _uscPaymentGatewayList = new List<uscPaymentGateway>();
        private uscPaymentGateway GetFreeUscPaymentGateway()
        {
            uscPaymentGateway retCtrl = null;
            if (_uscPaymentGatewayList.Count == 0)
                retCtrl = new uscPaymentGateway();
            else
            {
                retCtrl = _uscPaymentGatewayList[0];
                _uscPaymentGatewayList.RemoveAt(0);
            }
            return retCtrl;
        }

        private void ClearWrpPaymentGatewayContainer()
        {
            int nextCtrlInx = 0;
            do
            {
                if (WrpPaymentGatewayContainer.Children.Count > nextCtrlInx)
                {
                    if (WrpPaymentGatewayContainer.Children[nextCtrlInx] is uscPaymentGateway ctrl) 
                    {
                        if (ctrl.IsCreditCard == false)
                        {
                            //ctrl.OnTextBoxGotFocus -= PassengerInfoTextBox_GotFocus;
                            WrpPaymentGatewayContainer.Children.RemoveAt(nextCtrlInx);
                            _uscPaymentGatewayList.Add(ctrl);
                        }
                        else
                            nextCtrlInx++;
                    }
                    else
                        nextCtrlInx++;
                }
            } while (WrpPaymentGatewayContainer.Children.Count > nextCtrlInx);
        }
    }
}
