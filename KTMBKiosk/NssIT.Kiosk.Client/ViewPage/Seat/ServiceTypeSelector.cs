using NssIT.Train.Kiosk.Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace NssIT.Kiosk.Client.ViewPage.Seat
{
    public class ServiceTypeSelector
    {
        private string _logChannel = "ViewPage";
        public event EventHandler<ServiceTypeSelectedEventArgs> OnSeatServiceSelected;

        private Brush _selectedBackgroundColor = new SolidColorBrush(Color.FromArgb(0xFF, 0x7A, 0x79, 0x99));
        private Brush _unSelectedBackgroundColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));

        private Brush _selectedTextColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
        private Brush _unSelectedTextColor = new SolidColorBrush(Color.FromArgb(0xFF, 0x44, 0x44, 0x44));

        private Border[] _seatSvcCtrlList = null;
        private string[] _seatServiceTypeList = null;

        public ServiceTypeSelector(Border[] seatSvcCtrlList)
        {
            _seatSvcCtrlList = seatSvcCtrlList;

            if (_seatSvcCtrlList != null)
            {
                foreach (Border _stSvc in _seatSvcCtrlList)
                {
                    _stSvc.Visibility = System.Windows.Visibility.Collapsed;
                    _stSvc.MouseLeftButtonUp += SeatServiceType_MouseLeftButtonUp;
                }
            }
        }

        private void SeatServiceType_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                if ((sender is Border db) && (db.Child is TextBlock txb))
                {
                    SetSeatSvcCtrl(db, isSelected: true);

                    try
                    {
                        OnSeatServiceSelected?.Invoke(null, new ServiceTypeSelectedEventArgs(txb.Text));
                    }
                    catch(Exception ex)
                    {
                        App.ShowDebugMsg($@"{ex.Message}; (EXIT10000582)");
                        App.Log.LogError(_logChannel, "-", new Exception($@"{ex.Message}; (EXIT10000582)", ex), "EX01", "ServiceTypeSelector.SeatServiceType_MouseLeftButtonUp");
                        App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000582)");
                    }
                }
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg($@"{ex.Message}; (EXIT10000581)");
                App.Log.LogError(_logChannel, "-", new Exception($@"{ex.Message}; (EXIT10000581)", ex), "EX02", "ServiceTypeSelector.SeatServiceType_MouseLeftButtonUp");
                App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000581)");
            }
        }

        public void InitData(string[] seatServiceTypeList)
        {
            _seatServiceTypeList = seatServiceTypeList;
        }

        public void LoadControls()
        {
            if (_seatSvcCtrlList != null)
            {
                foreach (Border _stSvc in _seatSvcCtrlList)
                    _stSvc.Visibility = System.Windows.Visibility.Collapsed;

                if (_seatServiceTypeList != null)
                {
                    Border seatSvcCtrl = null;
                    Border firstSeatSvcCtrl = null;
                    int ctrlInx = 0;
                    foreach (string typ in _seatServiceTypeList)
                    {
                        seatSvcCtrl = null;

                        if (_seatSvcCtrlList.Length >= (ctrlInx + 1))
                            seatSvcCtrl = _seatSvcCtrlList[ctrlInx];

                        if ((seatSvcCtrl != null) && (seatSvcCtrl.Child is TextBlock txtBk))
                        {
                            seatSvcCtrl.Visibility = System.Windows.Visibility.Visible;
                            txtBk.Text = typ;

                            if (firstSeatSvcCtrl == null)
                            {
                                firstSeatSvcCtrl = seatSvcCtrl;
                                SetSeatSvcCtrl(firstSeatSvcCtrl, isSelected: true);
                            }
                            else
                                SetSeatSvcCtrl(seatSvcCtrl, isSelected: false);
                        }
                        ctrlInx++;
                    }
                }
            }
        }

        private Border _lastSelectedSeatSvcCtrl = null;
        private void SetSeatSvcCtrl(Border seatSvcCtrl, bool isSelected)
        {
            
            if ((seatSvcCtrl != null) && (seatSvcCtrl.Child is TextBlock txtBk))
            {
                if (isSelected == true)
                {
                    ResetPreviousSelectedButton();
                    seatSvcCtrl.Background = _selectedBackgroundColor;
                    txtBk.Foreground = _selectedTextColor;
                    _lastSelectedSeatSvcCtrl = seatSvcCtrl;
                }
                else
                {
                    seatSvcCtrl.Background = _unSelectedBackgroundColor;
                    txtBk.Foreground = _unSelectedTextColor;
                }
            }

            void ResetPreviousSelectedButton()
            {
                if ((_lastSelectedSeatSvcCtrl != null) &&
                (_lastSelectedSeatSvcCtrl.Child is TextBlock txtBkX))
                {
                    _lastSelectedSeatSvcCtrl.Background = _unSelectedBackgroundColor;
                    txtBkX.Foreground = _unSelectedTextColor;
                }
            }
        }
    }
}
