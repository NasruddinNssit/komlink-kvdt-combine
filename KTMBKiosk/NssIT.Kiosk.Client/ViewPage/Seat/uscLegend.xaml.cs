using NssIT.Kiosk.Client.Base;
using NssIT.Train.Kiosk.Common.Constants;
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

namespace NssIT.Kiosk.Client.ViewPage.Seat
{
    /// <summary>
    /// Interaction logic for uscLegend.xaml
    /// </summary>
    public partial class uscLegend : UserControl
    {
        private string _logChannel = "ViewPage";

        //private string _webApiBaseUrl = "";

        private WebImageCache.GetImageFromCache _getImageFromCacheDelgHandle;

        //public uscLegend()
        //{
        //    InitializeComponent();
        //}

        public uscLegend(WebImageCache.GetImageFromCache getImageFromCacheDelgHandle)
        {
            InitializeComponent();
            //_webApiBaseUrl = webApiBaseUrl;
            _getImageFromCacheDelgHandle = getImageFromCacheDelgHandle;
        }

        //private string BaseImageWebApi
        //{
        //    get
        //    {
        //        return _webApiBaseUrl.Replace(@"/api", "");
        //    }
        //}

        public void UpdateLegend(string imgUrl, string legendDesc, string currency, decimal amount)
        {
            TxtServiceTypeDesc.Text = legendDesc ?? "";

            if (amount > 0)
            {
                TxtPrice.Text = $@"({currency} {amount:#,###.00})";
                TxtPrice.Visibility = Visibility.Visible;
            }
            else
            {
                TxtPrice.Visibility = Visibility.Collapsed;
            }

            SetSeatServiceTypeImage(imgUrl);
        }

        public void UpdateLegend(string imgUrl, string legendDesc)
        {
            TxtServiceTypeDesc.Text = legendDesc ?? "";
            TxtPrice.Visibility = Visibility.Collapsed;
            SetSeatServiceTypeImage(imgUrl);
        }

        public void Reset()
        {
            TxtServiceTypeDesc.Text = "";
            TxtPrice.Text = "";
            TxtPrice.Visibility = Visibility.Collapsed;
            SetSeatServiceTypeImage(null);
        }

        private async void SetSeatServiceTypeImage(string imgUrl)
        {
            string apiUrl = imgUrl ?? "";
            try
            {
                if (string.IsNullOrWhiteSpace(apiUrl) == false)
                {
                    //if (apiUrl.Substring(0, 1).Equals(@"/") || apiUrl.Substring(0, 1).Equals(@"\"))
                    //{
                    //    if (apiUrl.Length == 1)
                    //        apiUrl = "";
                    //    else
                    //        apiUrl = apiUrl.Substring(1);
                    //}

                    //string urlStr = $@"{BaseImageWebApi}{apiUrl}";
                    //BitmapImage bitImg = new BitmapImage(new Uri(urlStr));
                    ////bitImg.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                    ////bitImg.CacheOption = BitmapCacheOption.OnLoad;
                    //BdSeatImg.Background = new ImageBrush() { Stretch = Stretch.UniformToFill, ImageSource = bitImg };

                    ImageBrush imgBrh = new ImageBrush();
                    imgBrh.ImageSource = await _getImageFromCacheDelgHandle(imgUrl);
                    BdSeatImg.Background = imgBrh;
                }
                else
                {
                    BdSeatImg.Background = null;
                }
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg($@"{ex.Message}; (EXIT10000561)");
                App.Log.LogError(_logChannel, "-", new Exception($@"{ex.Message}; (EXIT10000561)", ex), "EX01", "uscLegend.SetSeatServiceTypeImage");
                App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000561)");
            }
        }


    }
}
