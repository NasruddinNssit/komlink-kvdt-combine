using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
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
    /// Interaction logic for uscCoach.xaml
    /// </summary>
    public partial class uscCoach : UserControl
    {
        private string _logChannel = "ViewPage";

        public event EventHandler<CoachSelectedEventArgs> OnSelectCoach;

        private Brush _normalBorderColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xCC, 0xCC, 0xCC));
        private Brush _highlightedBorderColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xFB, 0xD0, 0x12));

        private Brush _selectedBackgroundColor = new SolidColorBrush(Color.FromArgb(0xFF, 0x7A, 0x79, 0x99));
        private Brush _unSelectedBackgroundColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));

        private Brush _selectedTextColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
        private Brush _unSelectedTextColor = new SolidColorBrush(Color.FromArgb(0xFF, 0x44, 0x44, 0x44));

        private int _coachControlIndex = -1;

        public static ResourceDictionary LanguageResource { get; set; }

        public uscCoach()
        {
            InitializeComponent();
            UnSelected();
        }

        public CoachModel CoachData { get; private set; }

        public void SetCoachData(CoachModel coachData, int coachControlIndex)
        {
            CoachData = coachData;
            _coachControlIndex = coachControlIndex;

            if (CoachData != null)
            {
                TxtCoachDesc.Text = CoachData.CoachLabel ?? "";
                TxtAvailableSeat.Text = $@"({CoachData.SeatAvailable.ToString()} Seats)";

                if (LanguageResource != null)
                    TxtAvailableSeat.Text = string.Format(LanguageResource["COACH_SEAT_AVAILABLE_Label"].ToString(), CoachData.SeatAvailable);

            }
            else
            {
                TxtCoachDesc.Text = "";
                TxtAvailableSeat.Text = "";
            }
        }

        private void Selected_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if ((CoachData != null) && (_coachControlIndex >= 0))
                {
                    RaiseOnSelectCoach();
                }
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg($@"{ex.Message}; (EXIT10000572)");
                App.Log.LogError(_logChannel, "-", new Exception($@"{ex.Message}; (EXIT10000572)", ex), "EX01", "uscCoach.Selected_Click");
                App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000572)");
            }

            void RaiseOnSelectCoach()
            {
                try
                {
                    OnSelectCoach?.Invoke(null, new CoachSelectedEventArgs(CoachData, _coachControlIndex));
                }
                catch (Exception ex)
                {
                    App.ShowDebugMsg($@"{ex.Message}; (EXIT10000571)");
                    App.Log.LogError(_logChannel, "-", new Exception($@"{ex.Message}; (EXIT10000571)", ex), "EX01", "uscCoach.RaiseOnSelectCoach");
                    App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000571)");
                }
            }
        }

        public int CoachIndex
        {
            get => _coachControlIndex;
        }

        public void Selected()
        {
            BdMain.Background = _selectedBackgroundColor;
            TxtCoachDesc.Foreground = _selectedTextColor;
            TxtAvailableSeat.Foreground = _selectedTextColor;
        }

        public void UnSelected()
        {
            BdMain.Background = _unSelectedBackgroundColor;
            TxtCoachDesc.Foreground = _unSelectedTextColor;
            TxtAvailableSeat.Foreground = _unSelectedTextColor;
        }

        /// <summary></summary>
        /// <param name="custSeatDetailList">Leave Null for hiding Seat Count of the Coach</param>
        public void UpdateSelectedSeatCount(CustSeatDetail[] custSeatDetailList)
        {
            if(custSeatDetailList?.Length > 0)
            {
                if (CoachData.Id != Guid.Empty)
                {
                    int seatCount = 0;
                    
                    List <SeatLayoutModel> ls = new List<SeatLayoutModel>(CoachData.SeatLayoutModels);
                    foreach (CustSeatDetail seat in custSeatDetailList)
                    {
                        if (ls.Find(s => ((s.Id == seat.SeatLayoutModel_Id) && (s.Id != Guid.Empty) )) is SeatLayoutModel)
                        {
                            seatCount++;
                        }
                    }

                    if (seatCount == 0)
                    {
                        HideSeatCount();
                    }
                    else
                    {
                        TxtSelectSeatCount.Text = seatCount.ToString();
                        GrdSelectedSeatCount.Visibility = Visibility.Visible;
                    }
                }
                else
                    HideSeatCount();
            }
            else
            {
                HideSeatCount();
            }

            void HideSeatCount()
            {
                TxtSelectSeatCount.Text = "";
                GrdSelectedSeatCount.Visibility = Visibility.Collapsed;
            }
        }

        private bool _isHighLighted = false;
        public bool IsHighLighted
        {
            get => _isHighLighted;
            set
            {
                _isHighLighted = value;
                if (_isHighLighted == true)
                {
                    BdMain.BorderBrush = _highlightedBorderColor;
                    BdMain.BorderThickness = new Thickness() { Left = 5, Top = 5, Right = 5, Bottom = 5 };
                }
                else
                {
                    BdMain.BorderBrush = _normalBorderColor;
                    BdMain.BorderThickness = new Thickness() { Left = 1, Top = 1, Right = 1, Bottom = 1 };
                }
            }
        }
    }
}
