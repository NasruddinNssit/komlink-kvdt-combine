using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using NssIT.Kiosk.Client.Base;
using NssIT.Kiosk.Client.ViewPage.Payment;
using NssIT.Kiosk.Tools.ThreadMonitor;
using NssIT.Train.Kiosk.Common.Data;
using NssIT.Train.Kiosk.Common.Data.Response.BTnG;
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
using System.Windows.Threading;

namespace NssIT.Kiosk.Client.ViewPage.Komuter
{
    /// <summary>
    /// Interaction logic for pgSalesPanel.xaml
    /// </summary>
    public partial class pgSalesPanel : Page
    {
        public delegate void ActivatePaymentSelectionDelg();

        private string _logChannel = "ViewPage";

        public event EventHandler<JourneyTypeChangeEventArgs> OnJourneyTypeChanged;
        public event EventHandler OnStartPayment;

        private JourneyTypeSelector _journeyTypeSelector = null;
        private SelectedTicketViewer _selectedTicketViewer = null;

        private Brush _activatedBackgroundColor = new SolidColorBrush(Color.FromArgb(255, 4, 84, 159));
        private Brush _deactivatedBackgroundColor = new SolidColorBrush(Color.FromArgb(255, 7, 68, 129));

        private string _destinationTag = @"[SELECT DESTINATION]";
        private SalesStage _currentSalesStage = SalesStage.StationStage;

        private LanguageCode _language = LanguageCode.English;
        private ResourceDictionary _languageResource = null;

        private CultureInfo _providerMalDate = new CultureInfo("ms-MY");
        private CultureInfo _providerEngDate = new CultureInfo("en-US");

        private WebImageCacheX _imageCache = null;

        private uscPaymentGateway.StartBTngPaymentDelg _startBTngPaymentDelgHandle = null;

        public pgSalesPanel()
        {
            InitializeComponent();

            BtnBoostX.Visibility = Visibility.Collapsed;
            BtnTnGX.Visibility = Visibility.Collapsed;

            _journeyTypeSelector = new JourneyTypeSelector(this, StkTicketTypeContainer);
            _selectedTicketViewer = new SelectedTicketViewer(this, StkTicketLineItem);

            _journeyTypeSelector.OnJourneyTypeChanged += _journeyTypeSelector_OnJourneyTypeChanged;

            _imageCache = new WebImageCacheX(12);
            DispatcherTimer LiveTime = new DispatcherTimer();
            LiveTime.Interval = TimeSpan.FromSeconds(1);
            LiveTime.Tick += timer_Tick;
            LiveTime.Start();
        }

        public void Init(uscPaymentGateway.StartBTngPaymentDelg startBTngPaymentDelgHandle)
        {
            _startBTngPaymentDelgHandle = startBTngPaymentDelgHandle;
        }

        private void _journeyTypeSelector_OnJourneyTypeChanged(object sender, JourneyTypeChangeEventArgs e)
        {
            if (_currentSalesStage != SalesStage.PackageStage)
                return;

            try
            {
                App.TimeoutManager.ResetTimeout();
                RaiseOnJourneyTypeChanged(e);
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg($@"(EXIT10001160); {ex.Message}");
                App.Log.LogError(_logChannel, "-", new Exception($@"(EXIT10001160);", ex), "EX01", "pgSalesPanel._journeyTypeSelector_OnJourneyTypeChanged");
            }

            //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            void RaiseOnJourneyTypeChanged(JourneyTypeChangeEventArgs e2)
            {
                try
                {
                    OnJourneyTypeChanged?.Invoke(null, e2);
                }
                catch (Exception ex)
                {
                    App.ShowDebugMsg($@"(EXIT10001161); {ex.Message}");
                    App.Log.LogError(_logChannel, "-", new Exception($@"(EXIT10001161);", ex), "EX01", "pgSalesPanel.RaiseOnJourneyTypeChanged");
                }
            }
        }

        private void ScvJourneyType_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            App.TimeoutManager.ResetTimeout();
        }

        private void BtnCrediCard_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            //OnStartPayment
            try
            {
                App.TimeoutManager.ResetTimeout();
                RaiseOnStartPayment();
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg($@"(EXIT10001167); {ex.Message}");
                App.Log.LogError(_logChannel, "-", new Exception($@"(EXIT10001167);", ex), "EX01", "pgSalesPanel._journeyTypeSelector_OnJourneyTypeChanged");
            }

            //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            void RaiseOnStartPayment()
            {
                try
                {
                    OnStartPayment?.Invoke(null, new EventArgs());
                }
                catch (Exception ex)
                {
                    App.ShowDebugMsg($@"(EXIT10001166); {ex.Message}");
                    App.Log.LogError(_logChannel, "-", new Exception($@"(EXIT10001166);", ex), "EX01", "pgSalesPanel.RaiseOnStartPayment");
                }
            }
        }

        public bool TriggerPaymentInProgress()
        {
            if (_currentSalesStage != SalesStage.ChoosePayment)
                return false;

            if (_currentSalesStage == SalesStage.ChoosePayment)
            {
                _currentSalesStage = SalesStage.PaymentInProgress;
                return true;
            }
            else
                return false;
        }

        public void SetLanguage(LanguageCode language, ResourceDictionary langResource)
        {
            _language = language;
            _languageResource = langResource;
            this.Resources.MergedDictionaries.Clear();
            this.Resources.MergedDictionaries.Add(_languageResource);
        }

        public void ResetSelection(string originId, string originName)
        {
            FromStation.Text = originName;

            if (_languageResource != null)
                ToStation.Text = (_languageResource["SELECT_DESTINATION_Label"].ToString()) ?? _destinationTag;
            else
                ToStation.Text = _destinationTag;

            _journeyTypeSelector.HideJourneyTypeList();
        }

        public void UpdateDestination(string destinationId, string destinationName)
        {
            ToStation.Text = destinationName;
        }

        public void ShowTicketPackage(KomuterSummaryModel komuterSummary)
        {
            _journeyTypeSelector.InitJourneyTypeList(komuterSummary);
            _journeyTypeSelector.LoadJourneyTypeList();
        }

        public void ResetJourneyTypeSelection()
        {
            _journeyTypeSelector.ResetJourneyTypeSelection();
        }

        public void ShowOnlySelectedPackage(string packageId)
        {
            _journeyTypeSelector.ShowOnlySelectedJourneyType(packageId);
        }

        public void ShowTicketDurationForPayment(string duration)
        {
            TxtDateAvailableStr.Visibility = Visibility.Visible;
            TxtDateAvailableStr.Text = duration ?? "--";
        }

        public void ShowTripDescriptionForPayment()
        {
            TxtTripDescription.Visibility = Visibility.Visible;
            TxtTripDescription.Text = $@"{FromStation.Text}  >  {ToStation.Text}";
        }

        public void ShowSelectedTicketItemList(KomuterTicket[] komuterTicketList)
        {
            _selectedTicketViewer.InitJourneyTypeList(komuterTicketList);
            _selectedTicketViewer.LoadSelectedTicketList();
        }

        public void ShowSalesTotalAmount(string current, decimal totalAmount)
        {
            TxtTotalAmount.Visibility = Visibility.Visible;
            TxtTotalAmount.Text = $@"{current ?? "*"} {totalAmount:#,###.00}";
        }

        //_selectedTicketViewer.LoadSelectedTicketList();

        private void timer_Tick(object sender, EventArgs e)
        {
            LblTime.Content = DateTime.Now.ToString("HH:mm");
            CultureInfo provider = (_language == LanguageCode.Malay) ? _providerMalDate: _providerEngDate ;
            LblDate.Content = DateTime.Now.ToString("dd MMM yyyy", provider);
        }

        public void ActivateDestinationSelection()
        {
            GrdDestinationLightShowBar.Visibility = Visibility.Visible;
            GrdJourneyTypeShowBar.Visibility = Visibility.Hidden;
            GrdPaymentShowBar.Visibility = Visibility.Hidden;

            WrpPaymentGatewayContainer.Visibility = Visibility.Hidden;
            TxtSalesMessage.Visibility = Visibility.Hidden;

            TxtTripDescription.Visibility = Visibility.Collapsed;
            TxtDateAvailableStr.Visibility = Visibility.Collapsed;
            TxtTotalAmount.Visibility = Visibility.Collapsed;

            TxtJourneyTitle.Visibility = Visibility.Collapsed;
            TxtPaymentTitle.Visibility = Visibility.Collapsed;

            grid_Destination.Background = _activatedBackgroundColor;
            grid_Journey.Background = _deactivatedBackgroundColor;
            grdPay.Background = _deactivatedBackgroundColor;
            _currentSalesStage = SalesStage.StationStage;

            _selectedTicketViewer.HideJourneyTypeList();
        }

        public void ActivateTicketPackageSelection()
        {
            RwDef2GrdMain.Height = new GridLength(2.5, GridUnitType.Star);

            GrdDestinationLightShowBar.Visibility = Visibility.Hidden;
            GrdJourneyTypeShowBar.Visibility = Visibility.Visible;
            GrdPaymentShowBar.Visibility = Visibility.Hidden;

            WrpPaymentGatewayContainer.Visibility = Visibility.Hidden;
            TxtSalesMessage.Visibility = Visibility.Hidden;

            TxtTripDescription.Visibility = Visibility.Collapsed;
            TxtDateAvailableStr.Visibility = Visibility.Collapsed;
            TxtTotalAmount.Visibility = Visibility.Collapsed;
            TxtJourneyTitle.Visibility = Visibility.Visible;
            TxtPaymentTitle.Visibility = Visibility.Collapsed;

            grid_Destination.Background = _deactivatedBackgroundColor;
            grid_Journey.Background = _activatedBackgroundColor;
            grdPay.Background = _deactivatedBackgroundColor;
            _currentSalesStage = SalesStage.PackageStage;

            _selectedTicketViewer.HideJourneyTypeList();
        }

        private RunThreadMan _loadBTnGThreadWorker = null;
        public void ActivatePaymentSelection()
        {
            RwDef2GrdMain.Height = new GridLength(0.6, GridUnitType.Star);

            GrdDestinationLightShowBar.Visibility = Visibility.Hidden;
            GrdJourneyTypeShowBar.Visibility = Visibility.Hidden;
            GrdPaymentShowBar.Visibility = Visibility.Visible;
            TxtJourneyTitle.Visibility = Visibility.Visible;
            TxtPaymentTitle.Visibility = Visibility.Visible;

            BdLoadingeWallet.Visibility = Visibility.Visible;
            WrpPaymentGatewayContainer.Visibility = Visibility.Visible;
            TxtSalesMessage.Visibility = Visibility.Visible;

            grid_Destination.Background = _deactivatedBackgroundColor;
            grid_Journey.Background = _deactivatedBackgroundColor;
            grdPay.Background = _activatedBackgroundColor;
            _currentSalesStage = SalesStage.ChoosePayment;

            //...stop here .. need to test later ..
            try
            {
                _imageCache.ClearCacheOnTimeout();
                ClearWrpPaymentGatewayContainer();
                LoadBTnGOption();
            }
            catch (Exception ex)
            {
                App.Log.LogError(_logChannel, "-", ex, "EX01", "pgSalesPanel.ActivatePaymentSelection");
            }
            
            return;
            //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            void LoadBTnGOption()
            {
                if ((_loadBTnGThreadWorker != null) && (_loadBTnGThreadWorker.IsEnd == false))
                    _loadBTnGThreadWorker.AbortRequest(out _, 3000);

                _loadBTnGThreadWorker = new RunThreadMan(new ThreadStart(LoadBTnGOptionThreadWorking), "pgSalesPanel.LoadBTnGOption", 70, _logChannel);
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
                                    pgt.Width = 200.0;
                                    pgt.Height = 68.0;
                                    //Margin="10,5,10,5"
                                    pgt.Margin = new Thickness(5, 5, 5, 5);
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

        enum SalesStage
        {
            StationStage = 0,
            PackageStage = 1,
            //PaxStage = 2,
            ChoosePayment = 3,
            PaymentInProgress = 4
        }

        
    }
}
