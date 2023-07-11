using NssIT.Kiosk.Client.Base;
using NssIT.Train.Kiosk.Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace NssIT.Kiosk.Client.ViewPage.Seat
{
    public class SeatSelector
    {
        private string _logChannel = "ViewPage";

        public delegate Guid[] GetSelectedSeatIdList();

        public event EventHandler<SelectSeatEventArgs> OnSeatSelect;
        public event EventHandler<UnSelectSeatEventArgs> OnSeatUnSelect;

        private string _rowPreFixName = "StkRow";
        private string _seatPreFixTag = "TagChair";
        private GetSelectedSeatIdList _delGetSelectedSeatIdList = null;
        private WebImageCache.GetImageFromCache _getImageFromCacheDelgHandle = null;

        public int _fixColumn = 0;
        public int _fixRow = 0;

        public int _lastCoachColumn = 0;
        public int _lastCoachRow = 0;

        public TrainCoachSeatModel _trainCoachSeat;
        public uscSeatChair[,] _seat2DList;
        public StackPanel[] _rows;
        public SeatLegendURLs _seatLegendURLs = new SeatLegendURLs();

        public int _maxPax = 1;

        public Guid[] SelectedSeatIdListX
        {
            get => _delGetSelectedSeatIdList();
        }
        public SeatSelector(int fixColumn, int fixRow, StackPanel stkCoach, GetSelectedSeatIdList delGetSelectedSeatIdList,  
            WebImageCache.GetImageFromCache getImageFromCacheDeglHandle)
        {
            _fixColumn = fixColumn;
            _fixRow = fixRow;

            _rows = new StackPanel[fixRow];
            _seat2DList = new uscSeatChair[fixRow, fixColumn];
            _delGetSelectedSeatIdList = delGetSelectedSeatIdList;
            _getImageFromCacheDelgHandle = getImageFromCacheDeglHandle;

            int startChairIndexPosi = _seatPreFixTag.Length;
            for (int rowInx=0; rowInx < fixRow; rowInx++)
            {
                string rowName = $@"{_rowPreFixName}{rowInx.ToString()}";
                if (stkCoach.FindName(rowName) is StackPanel seatRow)
                {
                    _rows[rowInx] = seatRow;

                    for (int colInx = 0; colInx < fixColumn; colInx++)
                    {
                        if (seatRow.Children[colInx] is uscSeatChair seatChair)
                        {
                            int seatInx = int.Parse(seatChair.Tag.ToString().Substring(startChairIndexPosi));
                            _seat2DList[rowInx, seatInx] = seatChair;
                            seatChair.Visibility = System.Windows.Visibility.Collapsed;
                            seatChair.OnSeatSelect += SeatChair_OnSeatSelect;
                            seatChair.OnSeatUnSelect += SeatChair_OnSeatUnSelect;
                            //seatChair.BaseWebApiUrl = baseWebApiUrl;
                        }
                    }
                }
            }
            string tt1 = ".";
        }

        private void SeatChair_OnSeatSelect(object sender, SelectSeatEventArgs e)
        {
            try
            {
                Guid[] selSetList = SelectedSeatIdListX;
                if (selSetList.Length < _maxPax)
                {
                    e.AgreeSelection = true;
                    App.TimeoutManager.ResetTimeout();
                    RaiseOnSeatSelect();
                }
                else
                {
                    e.AgreeSelection = false;
                }
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg($@"{ex.Message}; (EXIT10000595)");
                App.Log.LogError(_logChannel, "-", new Exception($@"{ex.Message}; (EXIT10000595)", ex), "EX01", "SeatSelector.SeatChair_OnSeatSelect");
                App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000595)");
            }

            void RaiseOnSeatSelect()
            {
                try
                {
                    OnSeatSelect?.Invoke(null, e);
                }
                catch (Exception ex)
                {
                    App.ShowDebugMsg($@"{ex.Message}; (EXIT10000594)");
                    App.Log.LogError(_logChannel, "-", new Exception($@"{ex.Message}; (EXIT10000594)", ex), "EX01", "SeatSelector.RaiseOnSeatSelect");
                    App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000594)");
                }
            }
        }

        private void SeatChair_OnSeatUnSelect(object sender, UnSelectSeatEventArgs e)
        {
            try
            {
                e.AgreeUnSelection = true;
                App.TimeoutManager.ResetTimeout();
                RaiseOnSeatUnSelect();
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg($@"{ex.Message}; (EXIT10000593)");
                App.Log.LogError(_logChannel, "-", new Exception($@"{ex.Message}; (EXIT10000593)", ex), "EX01", "SeatSelector.SeatChair_OnSeatUnSelect");
                App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000593)");
            }

            bool RaiseOnSeatUnSelect()
            {
                bool ret = false;
                try
                {
                    OnSeatUnSelect?.Invoke(null, e);
                }
                catch (Exception ex)
                {
                    App.ShowDebugMsg($@"{ex.Message}; (EXIT10000592)");
                    App.Log.LogError(_logChannel, "-", new Exception($@"{ex.Message}; (EXIT10000592)", ex), "EX01", "SeatSelector.RaiseOnSeatUnSelect");
                    App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000592)");
                }
                return ret;
            }
        }

        public int CurrentCoachIndex { get; set; }

        public void InitSelector(TrainCoachSeatModel trainCoachSeat, int maxPax)
        {
            _trainCoachSeat = trainCoachSeat;
            _maxPax = maxPax;
        }

        public void SeatCalibrate(CoachModel coach , int coachIndex)
        {
            if (coach != null)
            {
                ResetAllSeat();

                _seatLegendURLs.Reset();
                _seatLegendURLs.SeatBlockedIconURL = _trainCoachSeat.SeatBlockedIconURL;
                _seatLegendURLs.SeatFemaleIconURL = _trainCoachSeat.SeatFemaleIconURL;
                _seatLegendURLs.SeatMaleIconURL = _trainCoachSeat.SeatMaleIconURL;
                _seatLegendURLs.SeatReservedIconURL = _trainCoachSeat.SeatReservedIconURL;
                _seatLegendURLs.SeatSelectedIconURL = _trainCoachSeat.SeatSelectedIconURL;
                _seatLegendURLs.SeatSoldIconURL = _trainCoachSeat.SeatSoldIconURL;

                CurrentCoachIndex = coachIndex;
                int coachColumn = coach.SeatColumn;
                int coachRow = coach.SeatRow;
                SeatLayoutModel[] seatList = coach.SeatLayoutModels;

                int seatInx = 0;

                for (int rowInx = 0; rowInx < coachRow; rowInx++)
                {
                    for (int colInx = 0; colInx < coachColumn; colInx++)
                    {
                        uscSeatChair chair = _seat2DList[rowInx, colInx];
                        chair.Visibility = System.Windows.Visibility.Visible;
                        chair.SetSeatData(seatList[seatInx], _seatLegendURLs, SelectedSeatIdListX.ToList(), _trainCoachSeat.TVMDisplayGender, _getImageFromCacheDelgHandle);
                        seatInx++;
                    }
                }
                _lastCoachColumn = coachColumn;
                _lastCoachRow = coachRow;
            }
        }

        private void ResetAllSeat()
        {
            for (int rowInx = 0; rowInx < _fixRow; rowInx++)
            {
                for (int colInx = 0; colInx < _fixColumn; colInx++)
                {
                    uscSeatChair seatChair = _seat2DList[rowInx, colInx];
                    seatChair.Clear();
                    seatChair.Visibility = Visibility.Collapsed;
                }
            }
        }
    }
}