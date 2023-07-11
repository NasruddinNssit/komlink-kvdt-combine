using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
    /// Interaction logic for uscSeatSelectedRow.xaml
    /// </summary>
    public partial class uscSeatRecord : UserControl
    {
        private string _logChannel = "ViewPage";
        public event EventHandler<SeatRecordSelectedEventArgs> OnSelectRecord;

        private Brush _focusColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xFB, 0xD0, 0x12));
        private string _serviceType = null;
        private decimal _price = 0.00M; 
        private decimal _surcharge = 0.00M;

        public static ResourceDictionary LanguageResource { get; set; } = null;

        public uscSeatRecord()
        {
            InitializeComponent();
            this.MouseLeftButtonUp += UscSeatRecord_MouseLeftButtonUp;
            Reset();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (LanguageResource != null)
                {
                    this.Resources.MergedDictionaries.Clear();
                    this.Resources.MergedDictionaries.Add(LanguageResource);
                }
            }
            catch(Exception ex)
            {
                App.Log.LogError(_logChannel, "*", ex, "EX01", "uscSeatRecord.UserControl_Loaded");
            }
        }

        private void UscSeatRecord_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            RaiseOnSelectRecord();
        }

        public Guid SeatId { get; private set; } = Guid.Empty;
        public int CoachIndex { get; private set; } = -1;
        public string SeatNo { get => TxtSeatNo.Text; }
        public string SeatTypeDescription { get => TxtSeatTypeDesc.Text; }
        public string ServiceType { get => _serviceType; }
        public decimal Price { get => _price; }
        public decimal Surcharge { get => _surcharge; }

        public bool IsEmptyData
        { 
            get
            {
                if (SeatId.Equals(Guid.Empty))
                    return true;
                else
                    return false;
            }
        }

        public void Reset()
        {
            TxtItemNo.Text = "";
            BdSelectedBar.Background = null;

            DeleteData();
        }

        public void DeleteData()
        {
            CoachIndex = -1;
            SeatId = Guid.Empty;
            TxtCoach.Text = "";
            TxtSeatNo.Text = "";
            TxtCurrency.Text = "";
            TxtAmount.Text = "";
            TxtSeatTypeDesc.Text = "";
            _serviceType = null;
            _price = 0.0M;
            _surcharge = 0.0M;
        }

        public void UpdateData(Guid seatId, int coachInx, string coachLabel, string seatNo, string currency, decimal price, decimal surcharge, string seatTypeDesc, string serviceType)
        {
            //if (LanguageResource != null)
            //{
            //    this.Resources.MergedDictionaries.Clear();
            //    this.Resources.MergedDictionaries.Add(LanguageResource);
            //}

            DeleteData();
            SeatId = seatId;
            CoachIndex = coachInx;
            TxtCoach.Text = coachLabel ?? "";
            TxtSeatNo.Text = seatNo ?? "";
            TxtCurrency.Text = currency ?? "";
            TxtAmount.Text = $@"{(price + surcharge):#,###.00}";
            TxtSeatTypeDesc.Text = seatTypeDesc ?? "";
            _serviceType = serviceType;
            _price = price;
            _surcharge = surcharge;
        }

        private void RaiseOnSelectRecord()
        {
            try
            {
                if ((CoachIndex >= 0) && (SeatId.Equals(Guid.Empty) == false))
                    OnSelectRecord?.Invoke(null, new SeatRecordSelectedEventArgs(CoachIndex, SeatId));
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg($@"{ex.Message}; (EXIT10000541)");
                App.Log.LogError(_logChannel, "-", new Exception($@"{ex.Message}; (EXIT10000541)", ex), "EX01", "uscSeatRecord.RaiseOnSelectRecord");
                App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000541)");
            }
        }

        public int ItemNo
        {
            get
            {
                if (int.TryParse(TxtItemNo.Text, out int itemNo) == true)
                    return itemNo;
                else
                    return 0;
            }
            set
            {
                TxtItemNo.Text = value.ToString().Trim();
            }
        }

        public void Selected()
        {
            BdSelectedBar.Background = _focusColor;
            this.Focus();
        }

        public void UnSelected()
        {
            BdSelectedBar.Background = null;
        }

        
    }
}
