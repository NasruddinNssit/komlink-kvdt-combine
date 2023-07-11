using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using NssIT.Train.Kiosk.Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using NssIT.Train.Kiosk.Common.Helper;

namespace NssIT.Kiosk.Client.Base
{
    public class WebImageCache
    {
        public delegate Task<BitmapImage> GetImageFromCache(string imageUrl);

        private string _logChannel = "ViewPage";

        private static NssIT.Kiosk.AppDecorator.Config.Setting _sysSetting = NssIT.Kiosk.AppDecorator.Config.Setting.GetSetting();

        private Dictionary<string, BitmapImage> _imagesCache = new Dictionary<string, BitmapImage>();

        private string _baseWebApiUrl = null;
        public string BaseWebApiUrl 
        { 
            get
            {
                if (_baseWebApiUrl is null)
                {
                    NssIT.Kiosk.AppDecorator.Config.Setting sysSetting = NssIT.Kiosk.AppDecorator.Config.Setting.GetSetting();
                    if (string.IsNullOrWhiteSpace(sysSetting.WebApiURL))
                        _baseWebApiUrl = null;
                    else
                        _baseWebApiUrl = sysSetting.WebApiURL;
                }

                if (string.IsNullOrWhiteSpace(_baseWebApiUrl))
                    return "";
                else
                    return _baseWebApiUrl;
            }
        }

        private string BaseImageWebApi
        {
            get
            {
                return BaseWebApiUrl.Replace(@"/api", "");
            }
        }

        public WebImageCache()
        { }

        private SemaphoreSlim _imagesCacheLock = new SemaphoreSlim(1);
        public async Task<BitmapImage> GetImage(string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                return null;

            string apiUrl = TrimApiUrl(imageUrl.Trim());

            if (_imagesCache.TryGetValue(apiUrl, out BitmapImage imgFound) == true)
                return imgFound;

            try
            {
                BitmapImage bitmapImage = new BitmapImage();

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("RequestSignature", SecurityHelper.getSignature());

                    string urlStr = $@"{BaseImageWebApi}{apiUrl}";

                    using (var response = await client.GetAsync(urlStr))
                    {
                        response.EnsureSuccessStatusCode();

                        byte[] imgByteArr = await response.Content.ReadAsByteArrayAsync();

                        bitmapImage = new BitmapImage();
                        using (var mem = new System.IO.MemoryStream(imgByteArr))
                        {
                            mem.Position = 0;
                            bitmapImage.BeginInit();
                            bitmapImage.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                            bitmapImage.StreamSource = mem;
                            bitmapImage.EndInit();
                        }
                        bitmapImage.Freeze();

                        if (_imagesCache.ContainsKey(apiUrl) == false)
                        {
                            try
                            {
                                await _imagesCacheLock.WaitAsync();

                                if (_imagesCache.ContainsKey(apiUrl) == false)
                                    _imagesCache.Add(apiUrl, bitmapImage);
                            }
                            finally
                            {
                                if (_imagesCacheLock.CurrentCount == 0)
                                    _imagesCacheLock.Release();
                            }
                        }

                        return bitmapImage;
                    }
                    
                }
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg($@"{ex.Message}; (EXIT10000523B)");
                App.Log.LogError(_logChannel, "-", new Exception($@"{ex.Message}; (EXIT10000523B); imageUrl: {imageUrl}", ex), "EX01", "ImageCache.GetImage");
            }

            return null;

            //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            string TrimApiUrl(string url)
            {
                if (url is null)
                    return "";

                if (url.Substring(0, 1).Equals(@"/") || url.Substring(0, 1).Equals(@"\"))
                {
                    if (url.Length == 1)
                        url = "";
                    else
                        url = url.Substring(1);
                }
                return url;
            }
        }

        public void ClearAllCache()
        {
            if (_imagesCacheLock.CurrentCount == 0)
                _imagesCacheLock.Release();

            _imagesCache.Clear();
        }
    }
}
