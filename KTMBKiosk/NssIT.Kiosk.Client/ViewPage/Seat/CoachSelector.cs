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

namespace NssIT.Kiosk.Client.ViewPage.Seat
{
    public class CoachSelector
    {
        private string _logChannel = "ViewPage";

        public delegate CustSeatDetail[] GetSelectedSeatInfoList();
        public event EventHandler<CoachSelectedEventArgs> OnSelectCoach;

        private string _coachPreFixName = "GrdCoach";
        private int _maxCoach = 30;
        private int _totalCoachCount = 0;

        private Grid[] _allCoachWrapperList = new Grid[0];
        private CoachModel[] _coachDataList = null;

        private CurrentCoach _currentCoach = null;

        public int CurrentCoachInx { get => _currentCoach.CurrentCoachInx; } 
        public CoachModel CurrentCoachData { get => _currentCoach.CurrentCoachData; } 

        class CurrentCoach
        {
            public int CurrentCoachInx { get; private set; } = -1;
            public CoachModel CurrentCoachData { get; private set; } = null;

            public void UpdateCurrentCoach(CoachModel coachData, int coachInx)
            {
                if ((coachData != null) && (coachInx >= 0))
                {
                    CurrentCoachData = coachData;
                    CurrentCoachInx = coachInx;
                }
                else
                {
                    CurrentCoachData = null;
                    CurrentCoachInx = -1;
                }
            }
        }

        private GetSelectedSeatInfoList _getSelectedSeatInfoListDelgHandle = null;
        public CoachSelector(StackPanel stkTrain, GetSelectedSeatInfoList getSelectedSeatInfoListDelHandle)
        {
            _getSelectedSeatInfoListDelgHandle = getSelectedSeatInfoListDelHandle;
            _currentCoach = new CurrentCoach();
            _allCoachWrapperList = new Grid[_maxCoach];
            if (stkTrain != null)
            {
                for (int chInx = 0; chInx < _maxCoach; chInx++)
                {
                    string cohName = $@"{_coachPreFixName}{chInx.ToString()}";
                    if (stkTrain.FindName(cohName) is Grid coachWrapper)
                    {
                        coachWrapper.Visibility = System.Windows.Visibility.Collapsed;

                        _allCoachWrapperList[chInx] = coachWrapper;

                        uscCoach coachCtrl = GetCoachCtrl(coachWrapper);
                        coachCtrl.OnSelectCoach += CoachCtrl_OnSelectCoach;
                    }
                }
            }
        }

        private void CoachCtrl_OnSelectCoach(object sender, CoachSelectedEventArgs e)
        {
            try
            {
                if (GetCoachCtrl(_allCoachWrapperList[e.CoachControlIndex]) != null)
                {
                    // Reset Previous Coach -----------------
                    if (CurrentCoachInx >= 0)
                    {
                        uscCoach coachCtrlX = GetCoachCtrl(_allCoachWrapperList[CurrentCoachInx]);

                        if (coachCtrlX != null)
                            coachCtrlX.UnSelected();
                    }
                    //---------------------------------------
                    RaiseOnSelectCoach(e.CoachData, e.CoachControlIndex);
                }
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg($@"{ex.Message}; (EXIT10000521)");
                App.Log.LogError(_logChannel, "-", new Exception($@"{ex.Message}; (EXIT10000521)", ex), "EX01", "CoachSelector.CoachCtrl_OnSelectCoach");
                App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000521)");
            }
        }

        public void SetCurrentCoach(CoachModel coach, int coachControlIndex)
        {
            // Reset Previous Coach -----------------
            if (CurrentCoachInx >= 0)
            {
                uscCoach coachCtrlX = GetCoachCtrl(_allCoachWrapperList[CurrentCoachInx]);

                if (coachCtrlX != null)
                    coachCtrlX.UnSelected();
            }
            //---------------------------------------
            RaiseOnSelectCoach(coach, coachControlIndex);
        }

        public void RaiseOnSelectCoach(CoachModel coachData, int coachControlIndex)
        {
            try
            {
                _currentCoach.UpdateCurrentCoach(coachData, coachControlIndex);

                uscCoach coachCtrl = GetCoachCtrl(_allCoachWrapperList[coachControlIndex]);
                coachCtrl.Selected();
                OnSelectCoach?.Invoke(null, new CoachSelectedEventArgs(coachData, coachControlIndex));
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg($@"{ex.Message}; (EXIT10000522)");
                App.Log.LogError(_logChannel, "-", new Exception($@"{ex.Message}; (EXIT10000522)", ex), "EX01", "CoachSelector.RaiseOnSelectCoach");
                App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000522)");
            }
        }

        public void InitSelectorData(TrainCoachSeatModel trainCoachSeat, System.Windows.ResourceDictionary languageResource)
        {
            uscCoach.LanguageResource = languageResource;
            _coachDataList = null;
            _currentCoach.UpdateCurrentCoach(null, -1);

            _coachDataList = trainCoachSeat?.CoachModels;

            if (trainCoachSeat?.CoachModels?.Length > 0)
                _totalCoachCount = trainCoachSeat.CoachModels.Length;
            else
                _totalCoachCount = 0;
        }

        public void LoadControls()
        {
            foreach (Grid coachWrapper in _allCoachWrapperList)
            {
                uscCoach coachCtrl = GetCoachCtrl(coachWrapper);

                if (coachCtrl != null)
                {
                    coachCtrl.UnSelected();
                    coachCtrl.UpdateSelectedSeatCount(null);
                }

                coachWrapper.Visibility = System.Windows.Visibility.Collapsed;
            }

            if ((_coachDataList is null) || (_coachDataList.Length == 0))
                return;

            int coachInx = 0;
            foreach (CoachModel coachData in _coachDataList)
            {
                uscCoach coachCtrl = GetCoachCtrl(_allCoachWrapperList[coachInx]);
                
                if ((coachData != null) && (coachCtrl != null))
                {
                    coachCtrl.SetCoachData(coachData, coachInx);
                    _allCoachWrapperList[coachInx].Visibility = System.Windows.Visibility.Visible;
                }
                coachInx++;
            }

            if (_totalCoachCount > 0)
            {
                uscCoach coachCtrl = GetCoachCtrl(_allCoachWrapperList[0]);
                if (coachCtrl != null)
                {
                    RaiseOnSelectCoach(coachCtrl.CoachData, 0);
                }
            }
        }

        public void UpdateCoachSelectedSeatCount()
        {
            uscCoach coachCtrlX = GetCoachCtrl(_allCoachWrapperList[CurrentCoachInx]);

            if ((coachCtrlX != null) && (_getSelectedSeatInfoListDelgHandle != null))
            {
                CustSeatDetail[] seatList = _getSelectedSeatInfoListDelgHandle();
                coachCtrlX.UpdateSelectedSeatCount(seatList);
            }
        }

        public CoachModel GetCoachData(int coachIndex)
        {
            if ((coachIndex >= 0) && (coachIndex < _maxCoach))
            {
                uscCoach coachCtrl = GetCoachCtrl(_allCoachWrapperList[coachIndex]);

                if (coachCtrl != null)
                {
                    return coachCtrl.CoachData;
                }
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="seatServiceType"> Note: A ""/null for All Type </param>
        public void ShowSeatServiceType(string seatServiceType)
        {
            if (_coachDataList is null)
                return;

            int coachInx = 0;
            foreach (CoachModel coachData in _coachDataList)
            {
                uscCoach coachCtrl = GetCoachCtrl(_allCoachWrapperList[coachInx]);

                if ((coachData != null) && (coachCtrl != null))
                {
                    if (string.IsNullOrWhiteSpace(seatServiceType) == false)
                    {
                        int typExistCount = (from seat in coachCtrl.CoachData.SeatLayoutModels
                                             where ((seat.ServiceType != null) && seat.ServiceType.Equals(seatServiceType))
                                             select seat.Id).ToArray().Count();

                        if (typExistCount > 0)
                            coachCtrl.IsHighLighted = true;
                        else
                            coachCtrl.IsHighLighted = false;
                    }
                    else
                    {
                        coachCtrl.IsHighLighted = false;
                    }
                }
                coachInx++;
            }
        }

        private uscCoach GetCoachCtrl(Grid coachWrapper)
        {
            uscCoach coachCtrl = null;
            foreach (var ctrl in coachWrapper.Children)
            {
                if (ctrl is uscCoach)
                {
                    coachCtrl = (uscCoach)ctrl;
                    return coachCtrl;
                }
            }
            return null;
        }
    }
}
