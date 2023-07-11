using NssIT.Kiosk.Client.ViewPage.TicketSummary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace NssIT.Kiosk.Client.ViewPage.Date
{
    public class PaxSelector
    {
        private string _logChannel = "ViewPage";

        public event EventHandler<PaxSelectEventArgs> OnPaxSelect;

        private Brush _selectedColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xFB, 0xD0, 0x12));
        private Brush _deselectedColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xCC, 0xCC, 0xCC));

        private Border[] _paxButtonList = null;
        private Border _lastSelecetdBdPaxButton = null;

        public PaxSelector(params Border[] bdPaxArr)
        {
            _paxButtonList = bdPaxArr.ToArray();

            foreach(Border bdPax in _paxButtonList)
                bdPax.MouseLeftButtonUp += BdPax_MouseLeftButtonUp;
        }

        private void BdPax_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                ResetLastPaxButton();

                if (sender is Border)
                    _lastSelecetdBdPaxButton = sender as Border;
                else
                    if (sender is TextBlock tb)
                    if (tb.Parent is Border)
                        _lastSelecetdBdPaxButton = tb.Parent as Border;
                    else
                        _lastSelecetdBdPaxButton = null;

                if (_lastSelecetdBdPaxButton != null)
                {
                    _lastSelecetdBdPaxButton.Background = _selectedColor;
                    RaiseOnPaxSelect(int.Parse(_lastSelecetdBdPaxButton.Tag.ToString()));
                }
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg($@"{ex.Message}; EX101; PaxSelector.BdPax_MouseLeftButtonDown");
                App.Log.LogError(_logChannel, "", ex, "EX101","PaxSelector.BdPax_MouseLeftButtonDown");
            }

            void RaiseOnPaxSelect(int numberOfPax)
            {
                try
                {
                        OnPaxSelect?.Invoke(null, new PaxSelectEventArgs(numberOfPax));
                }
                catch (Exception ex)
                {
                    App.ShowDebugMsg($@"{ex.Message}; EX101; PaxSelector.RaiseOnPaxSelect");
                    App.Log.LogError(_logChannel, "", ex, "EX101", "PaxSelector.RaiseOnPaxSelect");
                }
            }
        }

        private void ResetLastPaxButton()
        {
            if (_lastSelecetdBdPaxButton != null)
                _lastSelecetdBdPaxButton.Background = _deselectedColor;
            _lastSelecetdBdPaxButton = null;
        }

        /// <summary>
        /// Return number of Pax selected at initialization stage
        /// </summary>
        /// <param name="maxPaxAllowed"></param>
        /// <returns></returns>
        public int InitSelector(int maxPaxAllowed)
        {
            if (maxPaxAllowed < 1)
                maxPaxAllowed = 1;

            ResetLastPaxButton();
            _paxButtonList[0].Background = _selectedColor;
            _lastSelecetdBdPaxButton = _paxButtonList[0];

            foreach (Border bdPax in _paxButtonList)
                bdPax.Visibility = System.Windows.Visibility.Collapsed;

            for (int inx=0; inx < maxPaxAllowed; inx++)
            {
                _paxButtonList[inx].Visibility = System.Windows.Visibility.Visible;
            }

            return 1;
        }

    }
}
