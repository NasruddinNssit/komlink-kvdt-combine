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
    /// Interaction logic for uscCoverage.xaml
    /// </summary>
    public partial class uscCoverage : UserControl
    {
        private System.Windows.Media.Brush _emptyImageBackColor = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x01, 0xFF, 0xFF, 0xFF));

        private LanguageCode _language = LanguageCode.English;
        private InsuranceModel _coverage = null;

        private WebImageCacheX.GetImageFromCache _getImageFromCacheDelgHandle = null;

        public uscCoverage()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            BdCoverageIcon.Background = _emptyImageBackColor;
            
            if (_language == LanguageCode.Malay)
            {
                TxtShortDesc.Text = _coverage.CoverageShortDescription2;
                TxtLongDesc.Text = _coverage.CoverageLongDescription2;
            }
            else
            {
                TxtShortDesc.Text = _coverage.CoverageShortDescription;
                TxtLongDesc.Text = _coverage.CoverageLongDescription;
            }
            GetSeatStatusImg(_coverage.URL);
        }

        public void InitData(LanguageCode language, InsuranceModel coverage, WebImageCacheX.GetImageFromCache getImageFromCacheDelgHandle)
        {
            _language = language;
            _coverage = coverage;
            _getImageFromCacheDelgHandle = getImageFromCacheDelgHandle;
        }

        private async Task<ImageBrush> GetSeatStatusImg(string url)
        {
            ImageBrush imgBrh = null;
            if (string.IsNullOrWhiteSpace(url) == false)
            {
                if (_getImageFromCacheDelgHandle != null)
                {
                    imgBrh = new ImageBrush();
                    imgBrh.ImageSource = await _getImageFromCacheDelgHandle(url);
                    BdCoverageIcon.Background = imgBrh;
                }
                else
                {
                    BdCoverageIcon.Background = _emptyImageBackColor;
                }
            }
            return imgBrh;
        }

        private void RichTextBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            
        }
    }
}
