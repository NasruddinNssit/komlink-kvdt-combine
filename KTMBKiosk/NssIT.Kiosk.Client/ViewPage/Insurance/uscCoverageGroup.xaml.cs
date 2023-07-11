using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.Client.Base;
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

namespace NssIT.Kiosk.Client.ViewPage.Insurance
{
    /// <summary>
    /// Interaction logic for uscCoverageGroup.xaml
    /// </summary>
    public partial class uscCoverageGroup : UserControl
    {
        private string _logChannel = "ViewPage";

        public event EventHandler<SelectInsuranceEventArgs> OnInsuranceSelected;

        private System.Windows.Media.Brush _deSelectedBackColor = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0xF9, 0xF9, 0xF9));
        private System.Windows.Media.Brush _selectedBackColor = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0xF7, 0xFF, 0xE0));

        private List<uscCoverage> _uscCoverageList = new List<uscCoverage>();

        private LanguageCode _language = LanguageCode.English;
        private InsuranceModel[] _coverageList = null;
        private string _currency = null;
        private WebImageCacheX.GetImageFromCache _getImageFromCacheDelgHandle = null;

        public uscCoverageGroup()
        {
            InitializeComponent();
        }

        public decimal Price { get; private set; }
        public decimal Cost { get; private set; }
        public string CostFormat { get; private set; }

        public string CoverageShortDesc
        { 
            get
            {
                return TxtCoverageDesc.Text;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ClearAllPassengerInfo();

            if (_language == LanguageCode.Malay)
            {
                TxtCoverageDesc.Text = _coverageList[0].ShortDescription2;
            }
            else
            {
                TxtCoverageDesc.Text = _coverageList[0].ShortDescription;
            }

            TxtCoveragePrice.Text = $@"({_currency} {_coverageList[0].Price:#,##0.00})";

            foreach (InsuranceModel insr in _coverageList)
            {
                uscCoverage uscCov = GetFreeUscCoverage();
                uscCov.InitData(_language, insr, _getImageFromCacheDelgHandle);
                WpnCoverageGroup.Children.Add(uscCov);
            }
        }

        private void UserControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            try
            {
                OnInsuranceSelected?.Invoke(null, new SelectInsuranceEventArgs(this));
            }
            catch (Exception ex)
            {
                App.Log.LogError(_logChannel, "-", new Exception($@"{ex.Message}; (EXIT10001001)", ex), "EX01", "uscCoverageGroup.UserControl_MouseLeftButtonUp");
            }
        }

        public void InitData(LanguageCode language, string InsuranceHeaders_Id, InsuranceModel[] coverageList, 
            string currency, decimal price, decimal cost, string costFormat,
            WebImageCacheX.GetImageFromCache getImageFromCacheDeglHandle)
        {
            _language = language;
            InsuranceHeadersId = InsuranceHeaders_Id;
            Price = price;
            Cost = cost;
            CostFormat = costFormat;
            _coverageList = coverageList;
            _currency = currency;
            _getImageFromCacheDelgHandle = getImageFromCacheDeglHandle;
        }

        public string InsuranceHeadersId { get; private set; }

        private bool _isInsuranceSelected = false;
        public bool IsInsuranceSelected
        {
            get => _isInsuranceSelected;
            set
            {
                _isInsuranceSelected = value;

                this.Dispatcher.Invoke(new Action(() => 
                {
                    if (_isInsuranceSelected)
                    {
                        BdBase.Background = _selectedBackColor;
                        ImgSelected.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        BdBase.Background = _deSelectedBackColor;
                        ImgSelected.Visibility = Visibility.Collapsed;
                    }
                }));
            }
        }

        private uscCoverage GetFreeUscCoverage()
        {
            uscCoverage retCtrl = null;
            if (_uscCoverageList.Count == 0)
                retCtrl = new uscCoverage();
            else
            {
                retCtrl = _uscCoverageList[0];
                _uscCoverageList.RemoveAt(0);
            }
            return retCtrl;
        }

        private void ClearAllPassengerInfo()
        {
            int nextCtrlInx = 0;
            do
            {
                if (WpnCoverageGroup.Children.Count > nextCtrlInx)
                {
                    if (WpnCoverageGroup.Children[nextCtrlInx] is uscCoverage ctrl)
                    {
                        //ctrl.OnTextBoxGotFocus -= PassengerInfoTextBox_GotFocus;
                        //ctrl.OnScanMyKadClick -= PassengerInfo_OnScanMyKadClick;
                        //ctrl.OnPromoCodeApplyClick -= PassgInfoCtrl_OnPromoCodeApplyClick;
                        WpnCoverageGroup.Children.RemoveAt(nextCtrlInx);
                        _uscCoverageList.Add(ctrl);
                    }
                    else
                        nextCtrlInx++;
                }
            } while (WpnCoverageGroup.Children.Count > nextCtrlInx);
        }

        
    }
}
