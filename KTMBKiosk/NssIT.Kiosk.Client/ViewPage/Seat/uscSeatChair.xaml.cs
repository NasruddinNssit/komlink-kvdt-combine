using NssIT.Kiosk.Client.Base;
using NssIT.Train.Kiosk.Common.Common;
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

namespace NssIT.Kiosk.Client.ViewPage.Seat
{
    /// <summary>
    /// Interaction logic for uscSeatChair.xaml
    /// </summary>
    public partial class uscSeatChair : UserControl
    {
        private string _logChannel = "ViewPage";

        public event EventHandler<SelectSeatEventArgs> OnSeatSelect;
        public event EventHandler<UnSelectSeatEventArgs> OnSeatUnSelect;

        private bool _isSelected = false;
        private SeatLegendURLs _seatLegendURLs = new SeatLegendURLs();

        private WebImageCache.GetImageFromCache _getImageFromCacheDelgHandle = null;

        public uscSeatChair()
        {
            InitializeComponent();
        }

        //public string BaseWebApiUrl { get; set; } = @"https://localhost:44305/api/";

        //private string BaseImageWebApi
        //{
        //    get
        //    {
        //        return BaseWebApiUrl.Replace(@"/api", "");
        //    }
        //}

        public SeatLayoutModel Seat { get; private set; }

        public async void SetSeatData(SeatLayoutModel seat, SeatLegendURLs seatLegendURLs, List<Guid> selectedSeatIdList, string tvmDisplayGender, WebImageCache.GetImageFromCache getImageFromCacheDelgHandle)
        {
            Seat = seat;
            _seatLegendURLs = seatLegendURLs ?? new SeatLegendURLs();
            _isSelected = false;
            _getImageFromCacheDelgHandle = getImageFromCacheDelgHandle;

            if (seat != null)
            {
                if (seat.IsOKU?.Equals(YesNo.Yes) == true)
                    TxtSeatNo.Text = "*OKU*";
                else
                    TxtSeatNo.Text = (seat.SeatNo ?? "").Trim();

                if ((seat.Status.Equals(SeatStatus.Available)) 
                    && (selectedSeatIdList.Find(r => r.Equals(seat.Id)).Equals(Guid.Empty) == false))
                {
                    SetSelected();
                }
                else if (seat.Status.Equals(SeatStatus.Blocked))
                {
                    if (seat.IsOKU?.Equals(YesNo.Yes) == true)
                        SetUnSelected();
                    else
                        BdSeat.Background = await GetSeatStatusImg(_seatLegendURLs.SeatBlockedIconURL);
                }
                else if (seat.Status.Equals(SeatStatus.Reserved))
                {
                    BdSeat.Background = await GetSeatStatusImg(_seatLegendURLs.SeatReservedIconURL);
                }
                else if (seat.Status.Equals(SeatStatus.SoldFemale))
                {
                    if ((tvmDisplayGender?.Equals(YesNo.Yes) == true) 
                        && (string.IsNullOrWhiteSpace(_seatLegendURLs.SeatFemaleIconURL) == false))
                    {
                        BdSeat.Background = await GetSeatStatusImg(_seatLegendURLs.SeatFemaleIconURL);
                    }
                    else
                        BdSeat.Background = await GetSeatStatusImg(_seatLegendURLs.SeatSoldIconURL);
                }
                else if (seat.Status.Equals(SeatStatus.SoldMale))
                {
                    if ((tvmDisplayGender?.Equals(YesNo.Yes) == true)
                        && (string.IsNullOrWhiteSpace(_seatLegendURLs.SeatMaleIconURL) == false))
                    {
                        BdSeat.Background = await GetSeatStatusImg(_seatLegendURLs.SeatMaleIconURL);
                    }
                    else
                        BdSeat.Background = await GetSeatStatusImg(_seatLegendURLs.SeatSoldIconURL);
                }
                else if (seat.Status.Equals(SeatStatus.Sold))
                {
                    BdSeat.Background = await GetSeatStatusImg(_seatLegendURLs.SeatSoldIconURL);
                }
                else if (string.IsNullOrWhiteSpace(seat.IconURL) == false)
                    SetUnSelected();
                else
                    BdSeat.Background = null;
            }
            else
            {
                BdSeat.Background = null;
                TxtSeatNo.Text = "";
            }
        }

        public void Clear()
        {
            _isSelected = false;
            TxtSeatNo.Text = "";
            BdSeat.Background = null;
        }

        private void Selected_Click(object sender, MouseButtonEventArgs e)
        {
            if (Seat != null)
            {
                if ((Seat.Status != null) && Seat.Status.Equals(SeatStatus.Available))
                {
                    if (_isSelected && RaiseOnSeatUnSelect())
                    {
                        /* Selected */
                    }
                    else if (RaiseOnSeatSelect())
                    {
                        /* Not Selected */
                    }
                }
            }

            bool RaiseOnSeatSelect()
            {
                bool ret = false;
                try
                {
                    SelectSeatEventArgs evt = new SelectSeatEventArgs(Seat);
                    OnSeatSelect?.Invoke(null, evt);

                    ret = evt.AgreeSelection;
                    if (evt.AgreeSelection)
                    {
                        SetSelected();
                    }
                }
                catch (Exception ex)
                {
                    App.ShowDebugMsg($@"{ex.Message}; (EXIT10000552)");
                    App.Log.LogError(_logChannel, "-", new Exception($@"{ex.Message}; (EXIT10000552)", ex), "EX01", "uscSeatChair.RaiseOnSeatSelect");
                    App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000552)");
                }

                return ret;
            }

            bool RaiseOnSeatUnSelect()
            {
                bool ret = false;
                try
                {
                    UnSelectSeatEventArgs evt = new UnSelectSeatEventArgs(Seat);
                    OnSeatUnSelect?.Invoke(null, evt);

                    ret = evt.AgreeUnSelection;
                    if (evt.AgreeUnSelection)
                    {
                        SetUnSelected();
                    }
                }
                catch (Exception ex)
                {
                    App.ShowDebugMsg($@"{ex.Message}; (EXIT10000551)");
                    App.Log.LogError(_logChannel, "-", new Exception($@"{ex.Message}; (EXIT10000551)", ex), "EX01", "uscSeatChair.RaiseOnSeatUnSelect");
                    App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000551)");
                }
                return ret;
            }
        }

        private async void SetSelected()
        {
            if (Seat == null)
                return;

            _isSelected = true;
            BdSeat.Background = await GetSeatStatusImg(_seatLegendURLs.SeatSelectedIconURL ?? "");
        }

        private async void SetUnSelected()
        {
            if (Seat == null)
                return;

            BdSeat.Background = await GetSeatStatusImg(Seat.IconURL ?? "");
            _isSelected = false;
        }

        private async Task<ImageBrush> GetSeatStatusImg(string url)
        {
            ImageBrush imgBrh = null;
            if (string.IsNullOrWhiteSpace(url) == false)
            {
                //string apiUrl = TrimApiUrl(url);
                //string urlStr = $@"{BaseImageWebApi}{apiUrl}";
                //BitmapImage bitImg = new BitmapImage(new Uri(urlStr));
                ////////bitImg.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                ////////bitImg.CacheOption = BitmapCacheOption.OnLoad;

                if (_getImageFromCacheDelgHandle != null)
                {
                    imgBrh = new ImageBrush();
                    imgBrh.ImageSource = await _getImageFromCacheDelgHandle(url);
                    BdSeat.Background = imgBrh;
                }
                else
                {
                    BdSeat.Background = null;
                }
            }
            return imgBrh;
        }

        private string TrimApiUrl(string url)
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
}
