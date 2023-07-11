using NssIT.Kiosk.Client.Base;
using NssIT.Train.Kiosk.Common.Common;
using NssIT.Train.Kiosk.Common.Constants;
using NssIT.Train.Kiosk.Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace NssIT.Kiosk.Client.ViewPage.Seat
{
    public class SeatLegendCalibrator
    {
        private int _bufferSize = 20;
        private uscLegend[] _legendBufferList = null;
        //private string _webApiBaseUrl = null;
        private WrapPanel _legendContainer = null;

        private WebImageCache.GetImageFromCache _getImageFromCacheDelgHandle = null;

        private string _seatReservedIconURL = null;
        private string _seatSelectedIconURL = null;
        private string _seatSoldIconURL = null;

        private string _seatSoldMaleIconURL = null;
        private string _seatSoldFemaleIconURL = null;

        private string _currency = "";

        private int _nextFreeBufferIndex = 0;

        /// <summary>
        /// </summary>
        /// <param name="maxBufferSize">Minimum is 20</param>
        public SeatLegendCalibrator(int maxBufferSize, WrapPanel legendContainer, WebImageCache.GetImageFromCache getImageFromCacheDelgHandle)
        {
            if (maxBufferSize < 20)
                _bufferSize = 20;
            else
                _bufferSize = maxBufferSize;

            //_webApiBaseUrl = webApiBaseUrl;

            legendContainer.Children.Clear();
            _legendContainer = legendContainer;
            _getImageFromCacheDelgHandle = getImageFromCacheDelgHandle;
        }

        private uscLegend[] LegendBufferList
        {
            get
            {
                if (_legendBufferList is null)
                {
                    _legendBufferList = new uscLegend[_bufferSize];
                    for (int inx = 0; inx < _bufferSize; inx++)
                    {
                        _legendBufferList[inx] = new uscLegend(_getImageFromCacheDelgHandle);
                    }
                }
                return _legendBufferList;
            }
        }

        private uscLegend GetFreeLegend()
        {
            if (_nextFreeBufferIndex < _bufferSize)
            {
                uscLegend ret = LegendBufferList[_nextFreeBufferIndex];
                _nextFreeBufferIndex++;
                return ret;
            }
            else
                return null;            
        }

        private void ResetLegend()
        {
            _nextFreeBufferIndex = 0;

            foreach (var child in _legendContainer.Children)
                if (child is uscLegend rec)
                    rec.Reset();

            _legendContainer.Children.Clear();
        }

        public void InitLegend(TrainCoachSeatModel trainCoachSeat)
        {
            _currency = trainCoachSeat.Currency ?? "";
            _seatReservedIconURL = string.IsNullOrWhiteSpace(trainCoachSeat.SeatReservedIconURL) ? null : trainCoachSeat.SeatReservedIconURL;
            _seatSelectedIconURL = string.IsNullOrWhiteSpace(trainCoachSeat.SeatSelectedIconURL) ? null : trainCoachSeat.SeatSelectedIconURL;
            _seatSoldIconURL = string.IsNullOrWhiteSpace(trainCoachSeat.SeatSoldIconURL) ? null : trainCoachSeat.SeatSoldIconURL;
            _seatSoldMaleIconURL = string.IsNullOrWhiteSpace(trainCoachSeat.SeatMaleIconURL) ? null : trainCoachSeat.SeatMaleIconURL;
            _seatSoldFemaleIconURL = string.IsNullOrWhiteSpace(trainCoachSeat.SeatFemaleIconURL) ? null : trainCoachSeat.SeatFemaleIconURL;
        }

        /// <summary>
        /// </summary>
        /// <param name="coach"></param>
        /// <param name="tvmDisplayGender">Value refer to NssIT.Train.Kiosk.Common.Constants.YesNo</param>
        public void CalibrateCoachLegend(CoachModel coach, string tvmDisplayGender)
        {
            ResetLegend();

            if ((_seatReservedIconURL != null) && (GetFreeLegend() is uscLegend lg1))
            {
                lg1.UpdateLegend(_seatReservedIconURL, "Reserved");
                _legendContainer.Children.Add(lg1);
            }

            if ((_seatSelectedIconURL != null) && (GetFreeLegend() is uscLegend lg2))
            {
                lg2.UpdateLegend(_seatSelectedIconURL, "Selected");
                _legendContainer.Children.Add(lg2);
            }

            if ((_seatSoldIconURL != null) && (GetFreeLegend() is uscLegend lg3))
            {
                lg3.UpdateLegend(_seatSoldIconURL, "Unavailable");
                _legendContainer.Children.Add(lg3);
            }

            if (tvmDisplayGender?.Equals(YesNo.Yes) == true)
            {
                if ((_seatSoldMaleIconURL != null) && (GetFreeLegend() is uscLegend lg4))
                {
                    lg4.UpdateLegend(_seatSoldMaleIconURL, "Male");
                    _legendContainer.Children.Add(lg4);
                }
                if ((_seatSoldFemaleIconURL != null) && (GetFreeLegend() is uscLegend lg5))
                {
                    lg5.UpdateLegend(_seatSoldFemaleIconURL, "Female");
                    _legendContainer.Children.Add(lg5);
                }
            }

            if (coach?.SeatLayoutModels?.Length > 0)
            {
                //NoCharge
                var svcTypeGrp = (from seat in coach.SeatLayoutModels
                        where (seat != null)
                        group seat by new { seat.SeatTypeDescription, seat.IsOKU, tickAmt = (seat.Price + seat.Surcharge)} into svcGrp
                        orderby svcGrp.Key.SeatTypeDescription
                        select new 
                        {
                            svcGrp.Key.SeatTypeDescription,
                            svcGrp.Key.IsOKU,
                            amount = svcGrp.Key.tickAmt, 
                            iconUrl = svcGrp.ToList().Find(s => (string.IsNullOrWhiteSpace(s.IconURL) == false))?.IconURL
                        }).ToArray();

                foreach (var svcType in svcTypeGrp)
                {
                    if (string.IsNullOrWhiteSpace(svcType.iconUrl) == false)
                    {
                        if ((GetFreeLegend() is uscLegend lg4))
                        {
                            lg4.Reset();

                            if (svcType.IsOKU?.Equals(YesNo.Yes) == true)
                                lg4.UpdateLegend(svcType.iconUrl, svcType.SeatTypeDescription);
                            else
                                lg4.UpdateLegend(svcType.iconUrl, svcType.SeatTypeDescription, _currency, svcType.amount);

                            _legendContainer.Children.Add(lg4);
                        }
                    }
                }
                string tt1 = "";
            }
        }
    }
}
