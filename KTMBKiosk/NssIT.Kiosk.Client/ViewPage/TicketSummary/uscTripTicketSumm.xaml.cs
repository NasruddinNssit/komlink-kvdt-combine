using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using NssIT.Kiosk.Client.Base;
using System;
using System.Collections.Generic;
using System.Globalization;
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

namespace NssIT.Kiosk.Client.ViewPage.TicketSummary
{
    /// <summary>
    /// Interaction logic for uscTripTicketSumm.xaml
    /// </summary>
    public partial class uscTripTicketSumm : UserControl, ITicketSummaryPortal
    {
        private Brush _departLabelColor = new SolidColorBrush(Color.FromArgb(0xFF, 0x50, 0xFF, 0x96));
        private Brush _returnLabelColor = new SolidColorBrush(Color.FromArgb(0xFF, 0x1E, 0x97, 0xDB));

        private LanguageCode _language;
        private ResourceDictionary _langMal = null;
        private ResourceDictionary _langEng = null;

        private TravelMode _travelMode = AppDecorator.Common.TravelMode.DepartOnly;

        private Style _activeStyle = null;
        private Style _deactivateStyle = null;

        private string culIn = "ms-MY"; /* culIn = "en-US"; */

        private CultureInfo provider = null;

        public uscTripTicketSumm()
        {
            InitializeComponent();

            provider = new CultureInfo(culIn);
            _langMal = CommonFunc.GetXamlResource(@"ViewPage\TicketSummary\rosTickSummMal.xaml");
            _langEng = CommonFunc.GetXamlResource(@"ViewPage\TicketSummary\rosTickSummEng.xaml");

            _activeStyle = GrdMain.FindResource("ResActive") as Style;
            _deactivateStyle = GrdMain.FindResource("ResDeactivate") as Style;

            //PortalSelected = false;
            IsActive = false;
        }

        public bool IsValidToShow { get; set; } = false;

        private bool _isActive = false;
        public bool IsActive 
        {
            get => _isActive;
            set
            {
                _isActive = value;
                if (_isActive)
                {
                    GrdTickSumm.Style = _activeStyle;
                }
                else
                {
                    GrdTickSumm.Style = _deactivateStyle;
                }
            } 
        }

        private string TravelMode
        {
            set 
            { 
                TxtTravelDirection.Text = value ?? ""; 

                if (string.IsNullOrWhiteSpace(TxtTravelDirection.Text))
                    BdTravelDirection.Visibility = Visibility.Hidden;
                else
                    BdTravelDirection.Visibility = Visibility.Visible;
            }
        }
        private int TrainNo
        {
            set
            {
                ResourceDictionary recDict = null;
                if (_language == LanguageCode.Malay)
                    recDict = _langMal;
                else
                    recDict = _langEng;

                TxtTrainTag.Text = string.Format(recDict["TRAIN_NO_Label"].ToString(), value);

                if (value <= 0)
                    BdTrainTag.Visibility = Visibility.Hidden;
                else
                    BdTrainTag.Visibility = Visibility.Visible;
            }
        }
        private string Service
        {
            set
            {
                TxtTrainService.Text = (value ?? "").ToUpper();

                if (string.IsNullOrWhiteSpace(TxtTrainService.Text))
                    BdTrainService.Visibility = Visibility.Hidden;
                else
                    BdTrainService.Visibility = Visibility.Visible;
            }
        }

        private string _originDesc = null;
        private string OriginDesc
        {
            get => _originDesc;
            set
            {
                _originDesc = value;

                if (string.IsNullOrWhiteSpace(_destinationDesc))
                    TxtOriginDestination.Text = $@"{_originDesc}";
                else
                    TxtOriginDestination.Text = $@"{_originDesc} > {_destinationDesc}";
            }
        }

        private string _destinationDesc = null;
        private string DestinationDesc
        {
            get => _destinationDesc;
            set
            {
                _destinationDesc = value;

                if (string.IsNullOrWhiteSpace(_destinationDesc))
                    TxtOriginDestination.Text = $@"{_originDesc}";
                else
                    TxtOriginDestination.Text = $@"{_originDesc} > {_destinationDesc}";
            }
        }

        private void SetDepartureTime(string date, string time)
        {
            string ttm = time ?? "";
            if (string.IsNullOrWhiteSpace(time) == false)
                ttm = $@" ({time})";

            if (string.IsNullOrWhiteSpace(date))
                TxtDepartureDateTime.Text = "";
            else
                TxtDepartureDateTime.Text = $@"{date}{ttm}";
        }

        private string TrainServiceClass
        {
            set
            {
                TxtTrainServiceClass.Text = value ?? "";
            }
        }

        private string Currency
        {
            set
            {
                TxtCurrency.Text = value ?? "";
            }
        }

        private void SetTotalAmount(string currency, decimal amount)
        {
            if (amount > 0)
            {
                TxtTotalLable.Visibility = Visibility.Visible;
                TxtCurrency.Text = currency ?? "";
                TxtTotalAmount.Text = $@"{amount:#,###.00}";
            }
            else
            {
                TxtTotalLable.Visibility = Visibility.Hidden;
                TxtCurrency.Text = "";
                TxtTotalAmount.Text = "";
            }
        }

        public void SetLanguage(LanguageCode language)
        {
            _language = language;

            this.Resources.MergedDictionaries.Clear();
            if (_language == LanguageCode.Malay)
                this.Resources.MergedDictionaries.Add(_langMal);
            else
                this.Resources.MergedDictionaries.Add(_langEng);
        }

        public void Reset()
        {
            this.Dispatcher.Invoke(new Action(() => {
                //PortalSelected = false;
                IsActive = false;
                TravelMode = "";
            }));
        }

        //private bool PortalSelected
        //{
        //    set
        //    {
        //        this.Dispatcher.Invoke(new Action(() =>
        //        {
        //            if (value == true)
        //            {
        //                GrdTickSumm.Style = _activeStyle;
        //                //BdSelectedIndicator.Visibility = Visibility.Visible;
        //            }
        //            else
        //            {
        //                GrdTickSumm.Style = _deactivateStyle;
        //                //BdSelectedIndicator.Visibility = Visibility.Collapsed;
        //            }
        //        }));
        //    }
        //}

        public void UpdateDepartureDate(DateTime newDepartureDate)
        {
            this.Dispatcher.Invoke(new Action(() => {
                SetDepartureTime($@"{newDepartureDate.ToString("dd MMM yyyy", provider)}", "");
            }));
        }

        public void UpdateSummary(UserSession session, TravelMode travelMode)
        {
            if (session != null)
            {
                SetLanguage(session.Language);
                _travelMode = travelMode;

                ResourceDictionary recDict = null;
                if (_language == LanguageCode.Malay)
                    recDict = _langMal;
                else
                    recDict = _langEng;

                if (_travelMode == AppDecorator.Common.TravelMode.ReturnOnly)
                {
                    //TravelMode = $@"< {recDict["RETURN_Label"].ToString()}";
                    TrainNo = 2;

                    TxtTravelDirection.Foreground = _returnLabelColor;

                    if (string.IsNullOrWhiteSpace(session.ReturnVehicleService))
                        Service = "";
                    else
                        Service = session.ReturnVehicleService;

                    if (string.IsNullOrWhiteSpace(session.DestinationStationName))
                    {
                        TravelMode = "";
                        OriginDesc = "";
                    }
                    else
                    {
                        TravelMode = $@"< {recDict["RETURN_Label"].ToString()}";
                        OriginDesc = session.DestinationStationName;
                    }

                    if (string.IsNullOrWhiteSpace(session.OriginStationName))
                        DestinationDesc = "";
                    else
                        DestinationDesc = session.OriginStationName;

                    if (session.ReturnPassengerDepartDateTime.HasValue)
                        SetDepartureTime(session.ReturnPassengerDepartDateTime.Value.ToString("dd MMM yyyy"), session.ReturnPassengerDepartTimeStr ?? "");
                    else
                        SetDepartureTime("", "");

                    if (string.IsNullOrWhiteSpace(session.ReturnCurrency))
                        Currency = null;
                    else
                        Currency = session.ReturnCurrency;

                    if (string.IsNullOrWhiteSpace(session.ReturnVehicleNo))
                    {
                        TrainNo = 0;
                        TrainServiceClass = null;
                    }
                    else
                    {
                        TrainNo = 2;
                        TrainServiceClass = $@"{session.ReturnVehicleService} {session.ReturnServiceCategory} - {session.ReturnVehicleNo}";
                    }

                    SetTotalAmount(session.ReturnCurrency, session.ReturnTotalAmount);
                }
                else
                {
                    TxtTravelDirection.Foreground = _departLabelColor;

                    if (string.IsNullOrWhiteSpace(session.DepartVehicleService))
                        Service = "";
                    else
                        Service = session.DepartVehicleService;

                    if (string.IsNullOrWhiteSpace(session.OriginStationName))
                    {
                        TravelMode = "";
                        OriginDesc = "";
                    }
                    else
                    {
                        TravelMode = $@"{recDict["DEPART_Label"].ToString()} >";
                        OriginDesc = session.OriginStationName;
                    }

                    if (string.IsNullOrWhiteSpace(session.DestinationStationName))
                        DestinationDesc = "";
                    else
                        DestinationDesc = session.DestinationStationName;

                    if (session.DepartPassengerDepartDateTime.HasValue)
                        SetDepartureTime(session.DepartPassengerDepartDateTime.Value.ToString("dd MMM yyyy"), session.DepartPassengerDepartTimeStr ?? "");
                    else
                        SetDepartureTime("", "");

                    if (string.IsNullOrWhiteSpace(session.DepartCurrency))
                        Currency = null;
                    else
                        Currency = session.ReturnCurrency;

                    if (string.IsNullOrWhiteSpace(session.DepartVehicleNo))
                    {
                        TrainNo = 0;
                        TrainServiceClass = null;
                    }
                    else
                    {
                        TrainNo = 1;
                        TrainServiceClass = $@"{session.DepartVehicleService} {session.DepartServiceCategory} - {session.DepartVehicleNo}";
                    }

                    SetTotalAmount(session.DepartCurrency, session.DepartTotalAmount);
                }
            }
        }
    }
}
