using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI;
using NssIT.Kiosk.Client.Base;
using NssIT.Kiosk.Client.ViewPage.Trip;
using NssIT.Train.Kiosk.Common.Data;
using NssIT.Train.Kiosk.Common.Data.Response;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// Interaction logic for pgSeat2.xaml
    /// </summary>
    public partial class pgSeat2 : Page, ISeat2, IKioskViewPage
    {
        private string _logChannel = "ViewPage";

        private Brush _seatSeriviceBackgroundColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));

        private Brush _activateConfirmButtomColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xFB, 0xD0, 0x12));
        private Brush _deactivateConfirmButtomColor = new SolidColorBrush(Color.FromArgb(0xFF, 0x99, 0x99, 0x99));

        private const string _seatServiceType_All = @"All Services";

        private CultureInfo _currentDateProvider = null;
        private LanguageCode _language = LanguageCode.English;
        private ResourceDictionary _langMal = null;
        private ResourceDictionary _langEng = null;
        private CultureInfo _malDateProvider = new CultureInfo("ms-MY");
        private CultureInfo _engDateProvider = new CultureInfo("en-US");

        private SeatSelector _seatSelector = null;
        private CoachSelector _coachSelector = null;
        private ServiceTypeSelector _seatServiceSelector = null;
        private SeatLegendCalibrator _seatLegendCalibrator = null;
        private SeatRecordSelector _seatRecordSelector = null;
        private TrainCoachSeatModel _latestTrainCoachSeatData = null;
        private WebImageCache _imageCache = null;
        private WebImageCache.GetImageFromCache _getImageFromCacheDelgHandle = null;

        private string _trainNo = "";
        private string _trainService = "";
        private string _subDepartDestination = "";
        private string _overrollServiceDesc = "";
        private string _departureDateTimeString = "";

        private string[] _seatServiceTypeList = new string[0];
        private bool _pageIsReadyListenToEvent = false;

        private string _tradeCurrency = "*";
        private int _noOfPax = 1;

        public pgSeat2()
        {
            InitializeComponent();

            _langMal = CommonFunc.GetXamlResource(@"ViewPage\Seat\rosSeatMalay.xaml");
            _langEng = CommonFunc.GetXamlResource(@"ViewPage\Seat\rosSeatEnglish.xaml");

            //GetSelectedSeatIdList
            SeatSelector.GetSelectedSeatIdList getSListDelg = new SeatSelector.GetSelectedSeatIdList(GetSelectedSeatIdList);
            CoachSelector.GetSelectedSeatInfoList getSSInfoListDelg = new CoachSelector.GetSelectedSeatInfoList(GetSelectedSeatInfoList);

            _imageCache = new WebImageCache();
            _getImageFromCacheDelgHandle = new WebImageCache.GetImageFromCache(GetCacheImage);

            _coachSelector = new CoachSelector(StkTrain, getSSInfoListDelg);
            _seatSelector = new SeatSelector(10, 50, StkCoach, getSListDelg, _getImageFromCacheDelgHandle);
            _seatServiceSelector = new ServiceTypeSelector(new Border[] 
            {
                BdSeatServiceType0, BdSeatServiceType1, BdSeatServiceType2, BdSeatServiceType3, BdSeatServiceType4,
                BdSeatServiceType5, BdSeatServiceType6, BdSeatServiceType7, BdSeatServiceType8
            });
            _seatLegendCalibrator = new SeatLegendCalibrator(30, WpnLegend, _getImageFromCacheDelgHandle);
            _seatRecordSelector = new SeatRecordSelector(20, StkSelectedSeat);
            
            _coachSelector.OnSelectCoach += _coachSelector_OnSelectCoach;
            _seatServiceSelector.OnSeatServiceSelected += _seatServiceSelector_OnSeatServiceSelected;

            _seatSelector.OnSeatSelect += _seatSelector_OnSeatSelect;
            _seatSelector.OnSeatUnSelect += _seatSelector_OnSeatUnSelect;

            _seatRecordSelector.OnSelectRecord += _seatRecordSelector_OnSelectRecord;
        }

        public TripMode TravalMode
        {
            get; private set;
        }

        private async Task<BitmapImage> GetCacheImage(string imageUrl)
        {
            return await _imageCache.GetImage(imageUrl);
        }

        public string TrainSeatModelId { get; private set; }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ResourceDictionary lang = (_language == LanguageCode.Malay) ? _langMal : _langEng;

            _pageIsReadyListenToEvent = false;

            GrdScreenShield.Visibility = Visibility.Collapsed;

            this.Resources.MergedDictionaries.Clear();
            this.Resources.MergedDictionaries.Add(lang);

            if (TravalMode == TripMode.Return)
                TxtPageHeader.Text = lang["PICK_YOUR_RETURN_SEAT_Label"].ToString();
            else
                TxtPageHeader.Text = lang["PICK_YOUR_DEPART_SEAT_Label"].ToString();

            SvSeat.ScrollToVerticalOffset(0);
            ScvTrain.ScrollToVerticalOffset(0);
            SvSelectedSeat.ScrollToVerticalOffset(0);

            App.MainScreenControl.MiniNavigator.AttachToFrame(frmNav, _language);
            App.MainScreenControl.MiniNavigator.IsPreviousVisible = Visibility.Visible;
            
            _seatRecordSelector.LoadSelectedSeatRecord();
            _coachSelector.LoadControls();
            _coachSelector.ShowSeatServiceType(null);
            _seatServiceSelector.LoadControls();
            
            RefreshConfirmButton();

            TxtTrainNo.Text = _trainNo;
            TxtService.Text = _trainService;
            TxtSubDepartDestination.Text = _subDepartDestination;
            TxtServiceDesc.Text = _overrollServiceDesc;
            TxtTicketDateTime.Text = _departureDateTimeString;

            _pageIsReadyListenToEvent = true;
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        { }

        private void _seatSelector_OnSeatSelect(object sender, SelectSeatEventArgs e)
        {
            try
            {
                App.TimeoutManager.ResetTimeout();
                _seatRecordSelector.ChooseASeat(e.Seat, _coachSelector.CurrentCoachInx, _coachSelector.CurrentCoachData.CoachLabel, _tradeCurrency);
                _coachSelector.UpdateCoachSelectedSeatCount();
                RefreshConfirmButton();
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg($@"{ex.Message}; (EXIT10000501)");
                App.Log.LogError(_logChannel, "-", new Exception($@"{ex.Message}; (EXIT10000501)", ex), "EX01", "pgSeat2._seatSelector_OnSeatSelect");
                App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000501)");
            }
        }

        private void _seatSelector_OnSeatUnSelect(object sender, UnSelectSeatEventArgs e)
        {
            try
            {
                App.TimeoutManager.ResetTimeout();
                _seatRecordSelector.DeleteSeatSelection(e.Seat.Id);
                _coachSelector.UpdateCoachSelectedSeatCount();
                RefreshConfirmButton();
            }
            catch(Exception ex)
            {
                App.ShowDebugMsg($@"{ex.Message}; (EXIT10000502)");
                App.Log.LogError(_logChannel, "-", new Exception($@"{ex.Message}; (EXIT10000502)", ex), "EX01", "pgSeat2._seatSelector_OnSeatUnSelect");
                App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000502)");
            }
        }

        private void _seatServiceSelector_OnSeatServiceSelected(object sender, ServiceTypeSelectedEventArgs e)
        {
            if (_pageIsReadyListenToEvent == false)
                return;

            App.TimeoutManager.ResetTimeout();

            if (e.ServiceType?.Equals(_seatServiceType_All) == true)
                _coachSelector.ShowSeatServiceType(null);
            else
                _coachSelector.ShowSeatServiceType(e.ServiceType);
        }

        private void _coachSelector_OnSelectCoach(object sender, CoachSelectedEventArgs e)
        {
            try
            {
                App.TimeoutManager.ResetTimeout();
                _seatSelector.SeatCalibrate(e.CoachData, e.CoachControlIndex);
                _seatLegendCalibrator.CalibrateCoachLegend(e.CoachData, _latestTrainCoachSeatData?.TVMDisplayGender);
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg($@"{ex.Message}; (EXIT10000503)");
                App.Log.LogError(_logChannel, "-", new Exception($@"{ex.Message}; (EXIT10000503)", ex), "EX01", "pgSeat2._coachSelector_OnSelectCoach");
                App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000503)");
            }
        }

        private void _seatRecordSelector_OnSelectRecord(object sender, SeatRecordSelectedEventArgs e)
        {
            try
            {
                App.TimeoutManager.ResetTimeout();
                CoachModel coach = _coachSelector.GetCoachData(e.CoachIndex);
                _coachSelector.SetCurrentCoach(coach, e.CoachIndex);
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg($@"{ex.Message}; (EXIT10000504)");
                App.Log.LogError(_logChannel, "-", new Exception($@"{ex.Message}; (EXIT10000504)", ex), "EX01", "pgSeat2._seatRecordSelector_OnSelectRecord");
                App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000504)");
            }
        }

        private void ConfirmSeat_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Guid[] seatIdList = _seatRecordSelector.GetSelectedSeatIdList();
                if (seatIdList.Length == _noOfPax)
                {
                    CustSeatDetail[] custSeatDetailList = _seatRecordSelector.GetCompletedSelectedSeatInfoList();

                    if (TravalMode == TripMode.Return)
                        SubmitReturnSeat(custSeatDetailList, TrainSeatModelId);
                    else
                        SubmitDepartSeat(custSeatDetailList, TrainSeatModelId);                    
                }
            }
            catch (Exception ex)
            {
                App.ShowDebugMsg($@"{ex.Message}; (EXIT10000505)");
                App.Log.LogError(_logChannel, "-", new Exception($@"{ex.Message}; (EXIT10000505)", ex), "EX01", "pgSeat2.ConfirmSeat_Click");
                App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000505)");
            }
        }

        public Guid[] GetSelectedSeatIdList()
        {
            return _seatRecordSelector.GetSelectedSeatIdList();
        }

        public CustSeatDetail[] GetSelectedSeatInfoList()
        {
            return _seatRecordSelector.GetCompletedSelectedSeatInfoList();
        }

        public void InitDepartSeatData(UIDepartSeatListAck uiDepartSeatList, UserSession session)
        {
            ResourceDictionary langRos;
            TravalMode = TripMode.Depart;

            _imageCache.ClearAllCache();
            TrainSeatResult trainSeatRes = (TrainSeatResult)uiDepartSeatList.MessageData ;

            if (trainSeatRes.Data is null)
                throw new Exception("Invalid Train Seat data; ((EXIT10000506))");

            if (string.IsNullOrWhiteSpace(trainSeatRes.Data.Id))
                throw new Exception("Invalid Id for Train Seat; ((EXIT10000507))");

            TrainSeatModelId = trainSeatRes.Data.Id;
            _language = (session != null ? session.Language : LanguageCode.English);

            if (_language == LanguageCode.Malay)
            {
                _currentDateProvider = _malDateProvider;
                langRos = _langMal;
            }
            else
            {
                _currentDateProvider = _engDateProvider;
                langRos = _langEng;
            }
            _trainNo = string.Format(langRos["TRAIN_NO_Label"].ToString(), 1);

            if ((trainSeatRes.Data?.CoachModels?.Length > 0) && (trainSeatRes.Data.CoachModels[0].SeatLayoutModels?.Length > 0))
            {
                _latestTrainCoachSeatData = trainSeatRes.Data;
                _coachSelector.InitSelectorData(_latestTrainCoachSeatData, langRos);
                _seatSelector.CurrentCoachIndex = 0;
            }
            else
            {
                _latestTrainCoachSeatData = new TrainCoachSeatModel();
                _coachSelector.InitSelectorData(_latestTrainCoachSeatData, langRos);
                _seatSelector.CurrentCoachIndex = -1;
            }

            _noOfPax = (session.NumberOfPassenger <= 0) ? 1 : session.NumberOfPassenger;
            _seatSelector.InitSelector(trainSeatRes.Data, _noOfPax);
            _seatServiceTypeList = GetSeatServiceTypeList(trainSeatRes.Data);
            _seatServiceSelector.InitData(_seatServiceTypeList);
            _seatLegendCalibrator.InitLegend(trainSeatRes.Data);
            _seatRecordSelector.InitSelectedSeatRecordData(_noOfPax, langRos);
            if (trainSeatRes.Data != null)
                _tradeCurrency = (trainSeatRes.Data.Currency ?? "*");
            else
                _tradeCurrency = "*";

            _trainService = session.DepartVehicleService ?? "*";
            _subDepartDestination = $@"{session.OriginStationName ?? "*"}  >  {session.DestinationStationName ?? "*"}";
            _overrollServiceDesc = $@"{session.DepartVehicleService} {session.DepartServiceCategory} - {session.DepartVehicleNo}";

            if (session.DepartPassengerDepartDateTime.HasValue)
                _departureDateTimeString = $@"{session.DepartPassengerDepartDateTime.Value.ToString("dd MMM yyyy", _currentDateProvider)} {session.DepartPassengerDepartTimeStr}";
            else
                _departureDateTimeString = $@"* {session.DepartPassengerDepartTimeStr}";

        }

        public void InitReturnSeatData(UIReturnSeatListAck uiReturnSeatList, UserSession session)
        {
            ResourceDictionary langRos;
            TravalMode = TripMode.Return;

            _imageCache.ClearAllCache();
            TrainSeatResult trainSeatRes = (TrainSeatResult)uiReturnSeatList.MessageData;

            if (trainSeatRes.Data is null)
                throw new Exception("Invalid Train Seat data; (EXIT10000510)");

            if (string.IsNullOrWhiteSpace(trainSeatRes.Data.Id))
                throw new Exception("Invalid Id for Train Seat; (EXIT10000510A)");

            TrainSeatModelId = trainSeatRes.Data.Id;
            _language = (session != null ? session.Language : LanguageCode.English);

            if (_language == LanguageCode.Malay)
            {
                _currentDateProvider = _malDateProvider;
                langRos = _langMal;
            }
            else
            {
                _currentDateProvider = _engDateProvider;
                langRos = _langEng;
            }

            _trainNo = string.Format(langRos["TRAIN_NO_Label"].ToString(), 2);

            if ((trainSeatRes.Data?.CoachModels?.Length > 0) && (trainSeatRes.Data.CoachModels[0].SeatLayoutModels?.Length > 0))
            {
                _latestTrainCoachSeatData = trainSeatRes.Data;
                _coachSelector.InitSelectorData(_latestTrainCoachSeatData, langRos);
                _seatSelector.CurrentCoachIndex = 0;
            }
            else
            {
                _latestTrainCoachSeatData = new TrainCoachSeatModel();
                _coachSelector.InitSelectorData(_latestTrainCoachSeatData, langRos);
                _seatSelector.CurrentCoachIndex = -1;
            }

            _noOfPax = (session.NumberOfPassenger <= 0) ? 1 : session.NumberOfPassenger;
            _seatSelector.InitSelector(trainSeatRes.Data, _noOfPax);
            _seatServiceTypeList = GetSeatServiceTypeList(trainSeatRes.Data);
            _seatServiceSelector.InitData(_seatServiceTypeList);
            _seatLegendCalibrator.InitLegend(trainSeatRes.Data);
            _seatRecordSelector.InitSelectedSeatRecordData(_noOfPax, langRos);
            if (trainSeatRes.Data != null)
                _tradeCurrency = (trainSeatRes.Data.Currency ?? "*");
            else
                _tradeCurrency = "*";

            _trainService = session.ReturnVehicleService ?? "*";
            _subDepartDestination = $@"{session.DestinationStationName ?? "*"}  >  {session.OriginStationName ?? "*"}";
            _overrollServiceDesc = $@"{session.ReturnVehicleService} {session.ReturnServiceCategory} - {session.ReturnVehicleNo}";
            _departureDateTimeString = $@"*** {session.ReturnPassengerDepartTimeStr}";

            if (session.ReturnPassengerDepartDateTime.HasValue)
                _departureDateTimeString = $@"{session.ReturnPassengerDepartDateTime.Value.ToString("dd MMM yyyy", _currentDateProvider)} {session.ReturnPassengerDepartTimeStr}";
            else
                _departureDateTimeString = $@"* {session.ReturnPassengerDepartTimeStr}";
        }

        private void RefreshConfirmButton()
        {
            Guid[] seatIdList = _seatRecordSelector.GetSelectedSeatIdList();
            
            if (seatIdList.Length == _noOfPax)
            {
                BdComfirmSeat.Background = _activateConfirmButtomColor;
            }
            else
            {
                BdComfirmSeat.Background = _deactivateConfirmButtomColor;
            }
        }

        //where((string.IsNullOrWhiteSpace(seat.ServiceType) == false) && (seat.ServiceType.ToUpper().Equals("NOCHARGE") == false))
        private string[] GetSeatServiceTypeList(TrainCoachSeatModel trainSeat)
        {
            List<string> seatSvcTypList = new List<string>();
            if (trainSeat?.CoachModels?.Length > 0)
            {
                foreach(CoachModel coach in trainSeat.CoachModels)
                {
                    if (coach?.SeatLayoutModels?.Length > 0)
                    {
                        string[] stTypList =
                           (from seat in coach.SeatLayoutModels
                            where ((string.IsNullOrWhiteSpace(seat.ServiceType) == false) && (seat.ServiceType.ToUpper().Equals("NOCHARGE") == false))
                            
                            group seat by seat.ServiceType into newGroup
                            orderby newGroup.Key
                            select newGroup.Key).ToArray();

                        foreach(string seatType in stTypList)
                        {
                            if (seatSvcTypList.Find(s => s.Equals(seatType)) is null)
                            {
                                seatSvcTypList.Add(seatType);
                            }
                        }
                    }
                    
                }
            }

            string[] sortedList = (from typ in seatSvcTypList
                                   orderby typ
                                   select typ).ToArray();

            string[] retTypes = new string[sortedList.Length + 1];
            retTypes[0] = _seatServiceType_All;
            for (int inx=0; inx < sortedList.Length; inx++)
            {
                retTypes[inx + 1] = sortedList[inx];
            }

            return retTypes;
        }

        private void SubmitDepartSeat(CustSeatDetail[] custSeatDetailList, string trainSeatModelId)
        {
            ShieldPage();
            System.Windows.Forms.Application.DoEvents();

            Thread submitWorker = new Thread(new ThreadStart(new Action(() => {
                try
                {
                    App.NetClientSvc.SalesService.SubmitDepartSeatList(custSeatDetailList, trainSeatModelId,
                        out bool isServerResponded);

                    if (isServerResponded == false)
                        App.MainScreenControl.Alert(detailMsg: "Local Server not responding (EXIT10000508)");
                }
                catch (Exception ex)
                {
                    App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000509)");
                    App.Log.LogError(_logChannel, "", new Exception("(EXIT10000509)", ex), "EX01", "pgSeat2.SubmitDepartSeat");
                }
            })));
            submitWorker.IsBackground = true;
            submitWorker.Start();
        }

        private void SubmitReturnSeat(CustSeatDetail[] custSeatDetailList, string trainSeatModelId)
        {
            ShieldPage();
            System.Windows.Forms.Application.DoEvents();

            Thread submitWorker = new Thread(new ThreadStart(new Action(() => {
                try
                {
                    App.NetClientSvc.SalesService.SubmitReturnSeatList(custSeatDetailList, trainSeatModelId,
                        out bool isServerResponded);

                    if (isServerResponded == false)
                        App.MainScreenControl.Alert(detailMsg: "Local Server not responding (EXIT10000510B)");
                }
                catch (Exception ex)
                {
                    App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000510C)");
                    App.Log.LogError(_logChannel, "", new Exception("(EXIT10000510C)", ex), "EX01", "pgSeat2.SubmitReturnSeat");
                }
            })));
            submitWorker.IsBackground = true;
            submitWorker.Start();
        }

        public void ShieldPage()
        {
            GrdScreenShield.Visibility = Visibility.Visible;
        }

        ////////XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
        private bool _debugCreatePage = false;
        public void Debug_CreateNInitPage(TrainCoachSeatModel trainCoachSeat)
        {
            int maxPax = 3;
            _coachSelector.InitSelectorData(trainCoachSeat, _langEng);
            _seatSelector.InitSelector(trainCoachSeat, maxPax);
            _seatSelector.CurrentCoachIndex = 0;
            _seatServiceTypeList = GetSeatServiceTypeList(trainCoachSeat);
            _seatServiceSelector.InitData(_seatServiceTypeList);
            if (_debugCreatePage == false)
                _seatLegendCalibrator = new SeatLegendCalibrator(30, WpnLegend, _getImageFromCacheDelgHandle);
            _seatLegendCalibrator.InitLegend(trainCoachSeat);
            _seatRecordSelector.InitSelectedSeatRecordData(maxPax, _langEng);
            if (trainCoachSeat != null)
                _tradeCurrency = (trainCoachSeat.Currency ?? "*");
            else
                _tradeCurrency = "*";
            _debugCreatePage = true;
        }

        public void Debug_LoadSimulation()
        {
            try
            {
                _pageIsReadyListenToEvent = false;
                _coachSelector.LoadControls();

                _coachSelector.ShowSeatServiceType(null);
                _seatServiceSelector.LoadControls();
                _seatRecordSelector.LoadSelectedSeatRecord();

                _pageIsReadyListenToEvent = true;
            }
            catch (Exception ex)
            {
                //MessageBox.Show($@"Error on LoadSimulation_DebugTest{"\r\n"}{ex.ToString()}");
            }            
        }

        

        ////////XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX


    }
}
