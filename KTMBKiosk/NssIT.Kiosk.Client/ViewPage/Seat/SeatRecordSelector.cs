using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using NssIT.Train.Kiosk.Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace NssIT.Kiosk.Client.ViewPage.Seat
{
    public class SeatRecordSelector
    {
        private string _logChannel = "ViewPage";
        public event EventHandler<SeatRecordSelectedEventArgs> OnSelectRecord;

        private int _bufferSize = 20;
        private uscSeatRecord[] _recordBufferList = null;
        private StackPanel _recordContainer = null;

        private int _nextFreeBufferIndex = 0;
        private int _maxPax = 1;
        private uscSeatRecord _currentRecord = null;

        public SeatRecordSelector(int maxBufferSize, StackPanel recordContainer)
        {
            if (maxBufferSize < 20)
                _bufferSize = 20;
            else
                _bufferSize = maxBufferSize;

            recordContainer.Children.Clear();
            _recordContainer = recordContainer;
        }

        private uscSeatRecord[] RecordBufferList
        {
            get
            {
                if (_recordBufferList is null)
                {
                    uscSeatRecord sr = null;
                    _recordBufferList = new uscSeatRecord[_bufferSize];
                    for (int inx = 0; inx < _bufferSize; inx++)
                    {
                        sr = new uscSeatRecord();
                        sr.OnSelectRecord += SelectedSeat_OnSelectRecord; ;
                        _recordBufferList[inx] = sr;
                    }
                }
                return _recordBufferList;
            }
        }

        private void SelectedSeat_OnSelectRecord(object sender, SeatRecordSelectedEventArgs e)
        {
            try
            {
                if ((e.CoachIndex >= 0) && (e.SeatId.Equals(Guid.Empty) == false))
                {
                    RaiseOnSeatSelect(e);

                    if (_currentRecord != null)
                        _currentRecord.UnSelected();

                    uscSeatRecord sr = GetRecordCtrl(e.SeatId);
                    _currentRecord = sr;
                    _currentRecord.Selected();
                }
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg($@"{ex.Message}; (EXIT10000512)");
                App.Log.LogError(_logChannel, "-", new Exception($@"{ex.Message}; (EXIT10000512)", ex), "EX01", "SeatRecordSelector.SelectedSeat_OnSelectRecord");
                App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000512)");
            }

            void RaiseOnSeatSelect(SeatRecordSelectedEventArgs e2)
            {
                try
                {
                    OnSelectRecord?.Invoke(null, e2);
                }
                catch (Exception ex)
                {
                    App.ShowDebugMsg($@"{ex.Message}; (EXIT10000511)");
                    App.Log.LogError(_logChannel, "-", new Exception($@"{ex.Message}; (EXIT10000511)", ex), "EX01", "SeatRecordSelector.RaiseOnSeatSelect");
                    App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; ((EXIT10000511)");
                }
            }
        }

        private uscSeatRecord GetFreeRecordCtrl()
        {
            if (_nextFreeBufferIndex < _bufferSize)
            {
                uscSeatRecord ret = RecordBufferList[_nextFreeBufferIndex];
                _nextFreeBufferIndex++;
                return ret;
            }
            else
                return null;
        }

        private void ResetAllRecord()
        {
            _nextFreeBufferIndex = 0;

            foreach (var child in _recordContainer.Children)
                if (child is uscSeatRecord rec)
                    rec.Reset();

            _recordContainer.Children.Clear();
        }

        public void InitSelectedSeatRecordData(int maxPax, System.Windows.ResourceDictionary languageResource)
        {
            if (_maxPax <= _bufferSize)
                _maxPax = (maxPax > 0) ? maxPax : 1;
            else
                _maxPax = _bufferSize;

            uscSeatRecord.LanguageResource = languageResource;
        }

        public void LoadSelectedSeatRecord()
        {
            ResetAllRecord();

            int lineNoCount = 0;
            uscSeatRecord firstRecord = null;
            uscSeatRecord rec = null;
            _currentRecord = null;
            for (int inx = 0; inx < _maxPax; inx++)
            {
                rec = GetFreeRecordCtrl();
                if (rec != null)
                {
                    rec.Reset();
                    rec.ItemNo = ++lineNoCount;
                    _recordContainer.Children.Add(rec);

                    if (firstRecord is null)
                        firstRecord = rec;
                }
            }

            if (firstRecord != null)
            {
                firstRecord.Selected();
                _currentRecord = firstRecord;
                _currentRecord.Selected();
            }
        }

        public void ChooseASeat(SeatLayoutModel seat, int coachIndex, string coachLabel, string currency)
        {
            if ((seat.Id.Equals(Guid.Empty) == false) && (coachIndex >= 0))
            {
                if ((_currentRecord != null) && (_currentRecord.IsEmptyData))
                {
                    _currentRecord.UpdateData(seat.Id, coachIndex, coachLabel, seat.SeatNo, currency, seat.Price, seat.Surcharge, seat.SeatTypeDescription, seat.ServiceType);
                }
                else
                {
                    if (GetEmptyRecordCtrl() is uscSeatRecord rc)
                        rc.UpdateData(seat.Id, coachIndex, coachLabel, seat.SeatNo, currency, seat.Price, seat.Surcharge, seat.SeatTypeDescription, seat.ServiceType);
                }

                SetNewCurrentRecord();
            }
        }

        public void DeleteSeatSelection(Guid seatId)
        {
            if (GetRecordCtrl(seatId) is uscSeatRecord rc)
                rc.DeleteData();

            SetNewCurrentRecord();
        }

        private void SetNewCurrentRecord()
        {
            uscSeatRecord freeRecord = GetEmptyRecordCtrl();
            if (freeRecord != null)
            {
                if (_currentRecord != null)
                    _currentRecord.UnSelected();
                _currentRecord = freeRecord;
                _currentRecord.Selected();
            }
        }

        private uscSeatRecord GetEmptyRecordCtrl()
        {
            foreach (var child in _recordContainer.Children)
            {
                if (child is uscSeatRecord rec)
                    if (rec.IsEmptyData)
                        return rec;
            }
            return null;
        }

        public Guid[] GetSelectedSeatIdList()
        {
            List<Guid> seatIdList = new List<Guid>();
            foreach (var child in _recordContainer.Children)
            {
                if (child is uscSeatRecord rec)
                    if (rec.IsEmptyData == false)
                        seatIdList.Add(rec.SeatId);
            }
            return seatIdList.ToArray();
        }

        public CustSeatDetail[] GetCompletedSelectedSeatInfoList()
        {
            List<CustSeatDetail> seatIdList = new List<CustSeatDetail>();
            foreach (var child in _recordContainer.Children)
            {
                if (child is uscSeatRecord rec)
                    if (rec.IsEmptyData == false)
                        seatIdList.Add(new CustSeatDetail() { 
                            SeatLayoutModel_Id = rec.SeatId, ServiceType = rec.ServiceType, SeatNo = rec.SeatNo, 
                            SeatTypeDescription = rec .SeatTypeDescription, Price = rec.Price, Surcharge = rec.Surcharge
                        });
            }
            return seatIdList.ToArray();
        }

        private uscSeatRecord GetRecordCtrl(Guid seatId)
        {
            if (seatId.Equals(Guid.Empty))
                return null;

            foreach (var child in _recordContainer.Children)
                if ((child is uscSeatRecord rec) && (rec.SeatId.Equals(seatId)))
                    return rec;

            return null;
        }
    }
}
