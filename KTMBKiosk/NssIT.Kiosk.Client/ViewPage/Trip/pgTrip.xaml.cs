using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI;
using NssIT.Kiosk.Client.Base;
using System;
using System.Collections.Generic;
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

namespace NssIT.Kiosk.Client.ViewPage.Trip
{
    /// <summary>
    /// Interaction logic for pgTrip.xaml
    /// </summary>
    public partial class pgTrip : Page, ITrip, IKioskViewPage
	{
		private string _logChannel = "ViewPage";

		private LanguageCode _language = LanguageCode.English;

		private string _departureStationDesc = null;
		private string _destinationeStationDesc = null;
		private DateTime _selectedDay = DateTime.MinValue;

		private double _designHeight = 710D;
		private double _designWidth = 1080D;

		private DayCellSelector _daySelection = null;
		private TripMode _tripMode = TripMode.Depart;
		private string _fromStationCode = null;
		private string _toStationCode = null;
		private int _noOfPax = 1;

		private DateTime? _passengerDepartDateTime = null;

		private ResourceDictionary _langMal = null;
		private ResourceDictionary _langEng = null;

		private (double OrgTimeColWidth, double OrgArrivalTimeColWidth, double OrgFareColWidth, double OrgSeatColWidth, double OrgTripDetailColWidth, double OrgPickingColWidth) _lstTripViewDefault;

		private TripDetailViewList _tripDetailList = new TripDetailViewList();
		private TripDetailViewListHelper _tripListHelper = null;
		//private TripDetailEnt[] _tripDataList = null;

		public pgTrip()
		{
			InitializeComponent();

			try
			{
				_langMal = CommonFunc.GetXamlResource(@"ViewPage\Trip\rosTripMalay.xaml");
				_langEng = CommonFunc.GetXamlResource(@"ViewPage\Trip\rosTripEnglish.xaml");

				DateTime today = DateTime.Now;
				_selectedDay = new DateTime(today.Year, today.Month, today.Day, 0, 0, 0, 0);

				LstTrip.DataContext = _tripDetailList;
				_tripListHelper = new TripDetailViewListHelper(this, LstTrip, _tripDetailList, PgbTripLoading, TxtNoTrip);
				_tripListHelper.OnBeginQueryNewTripData += _tripListingHelper_OnBeginQueryNewTripData;
				_tripListHelper.OnUpdateTripViewInProgress += _tripListingHelper_OnUpdateTripViewInProgress;

				GridViewColumn gvc;
				gvc = (GridViewColumn)this.FindName("GvcDepartTimeCol");
				if (gvc != null)
					_lstTripViewDefault.OrgTimeColWidth = gvc.Width;

				gvc = (GridViewColumn)this.FindName("GvcArrivalTimeCol");
				if (gvc != null)
					_lstTripViewDefault.OrgArrivalTimeColWidth = gvc.Width;

				gvc = (GridViewColumn)this.FindName("GvcFareCol");
				if (gvc != null)
					_lstTripViewDefault.OrgFareColWidth = gvc.Width;

				gvc = (GridViewColumn)this.FindName("GvcSeatCol");
				if (gvc != null)
					_lstTripViewDefault.OrgSeatColWidth = gvc.Width;

				gvc = (GridViewColumn)this.FindName("GvcTripDetailCol");
				if (gvc != null)
					_lstTripViewDefault.OrgTripDetailColWidth = gvc.Width;

				gvc = (GridViewColumn)this.FindName("GvcPickingCol");
				if (gvc != null)
					_lstTripViewDefault.OrgPickingColWidth = gvc.Width;
			}
			catch (Exception ex)
			{
				App.ShowDebugMsg($@"{ex.Message}; (EXIT10000471); pgTrip.pgTrip");
				App.Log.LogError(_logChannel, "-", new Exception("(EXIT10000471)", ex), "EX01", classNMethodName: "pgTrip.Page_Unloaded");
				App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000471)");
			}
		}

		private void Page_Loaded(object sender, RoutedEventArgs e)
		{
			try
			{
				System.Windows.Forms.Application.DoEvents();

				_tripSelected = false;

				TxtScreenShield1.Visibility = Visibility.Visible;
				TxtScreenShield2.Visibility = Visibility.Collapsed;
				TxtScreenShield2.Text = "";
				TxtErrorCode.Visibility = TxtScreenShield2.Visibility;
				TxtErrorCode.Text = "";

				GrdScreenShield.Visibility = Visibility.Collapsed;

				App.MainScreenControl.MiniNavigator.AttachToFrame(frmNav, _language);
				if (_tripMode == TripMode.Depart)
				{
					App.MainScreenControl.MiniNavigator.IsPreviousVisible = Visibility.Visible;
				}
				else
				{
					App.MainScreenControl.MiniNavigator.IsPreviousVisible = Visibility.Hidden;
				}

				this.Resources.MergedDictionaries.Clear();

				if (_language == LanguageCode.Malay)
					this.Resources.MergedDictionaries.Add(_langMal);
				else
					this.Resources.MergedDictionaries.Add(_langEng);

				LstTrip.SelectedIndex = -1;
				LstTrip.SelectedItem = null;

				_tripListHelper.PageLoaded();
				InitShowTrips();
			}
			catch (Exception ex)
			{
				App.ShowDebugMsg($@"{ex.Message}; (EXIT10000472); pgTrip.Page_Loaded");
				App.Log.LogError(_logChannel, "-", new Exception("(EXIT10000472)", ex), "EX01", classNMethodName: "pgTrip.Page_Loaded");
				App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000472)");
			}
		}

		private void Page_Unloaded(object sender, RoutedEventArgs e)
		{
			try
			{
				_tripListHelper.PageUnLoaded();
				_tripListHelper.ClearTripLoading();
				App.ShowDebugMsg("pgTrip Page_Unloaded");
			}
			catch (Exception ex)
			{
				App.ShowDebugMsg($@"{ex.Message}; (EXIT10000473); pgTrip.Page_Unloaded");
				App.Log.LogError(_logChannel, "-", new Exception("(EXIT10000473)", ex), "EX01", classNMethodName: "pgTrip.Page_Unloaded");
				App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000473)");
			}
		}

		private void _tripListingHelper_OnBeginQueryNewTripData(object sender, EventArgs e)
		{
			try
			{
				BdTripViewerAllShield.Visibility = Visibility.Visible;

				if (_language == LanguageCode.Malay)
					TxtShieldAllCoverMsg.Text = _langMal["LOADING_IN_PROGRESS_Label"]?.ToString();
				else
					TxtShieldAllCoverMsg.Text = _langEng["LOADING_IN_PROGRESS_Label"]?.ToString();

				TxtShieldCoverMsg.Text = "---- ..";
				BdTripViewerShield.Visibility = Visibility.Collapsed;
			}
			catch (Exception ex)
			{
				App.ShowDebugMsg($@"{ex.Message}; (EXIT10000474); pgTrip._tripListingHelper_OnBeginQueryNewTripData");
				App.Log.LogError(_logChannel, "-", new Exception("(EXIT10000474)", ex), "EX01", classNMethodName: "pgTrip._tripListingHelper_OnBeginQueryNewTripData");
				App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000474)");
			}
		}

		private void _tripListingHelper_OnUpdateTripViewInProgress(object sender, EventArgs e)
		{
			try
			{
				App.TimeoutManager.ResetTimeout();

				BdTripViewerAllShield.Visibility = Visibility.Collapsed;

				TxtShieldCoverMsg.Text = "----- ..";
				BdTripViewerShield.Visibility = Visibility.Collapsed;
			}
			catch (Exception ex)
			{
				App.ShowDebugMsg($@"{ex.Message}; (EXIT10000475); pgTrip._tripListingHelper_OnUpdateTripViewInProgress");
				App.Log.LogError(_logChannel, "-", new Exception("(EXIT10000475)", ex), "EX01", classNMethodName: "pgTrip._tripListingHelper_OnUpdateTripViewInProgress");
				App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000475)");
			}
		}

		public DateTime SelectedDay => _selectedDay;
		public string SelectedTripId { get; private set; } = null;
		public TripMode TripMode => _tripMode;

		private DayCellSelector DaySelectionHandle
		{
			get
			{
				if (_daySelection is null)
				{
					try
					{
						_daySelection = new DayCellSelector(this, cvDaysFrame, ScvDayCalendarContainer, LstTrip);
						_daySelection.OnDateSelected += _daySelection_OnDateSelected;
						_daySelection.OnBeginDayCellMoveAnimate += _daySelection_OnBeginDayCellMoveAnimate;
						_daySelection.OnBeginDayCellTouchMove += _daySelection_OnBeginDayCellTouchMove;
					}
					catch (Exception ex)
					{
						App.ShowDebugMsg($@"{ex.Message}; (EXIT10000476); pgTrip.DaySelectionHandle");
						App.Log.LogError(_logChannel, "-", new Exception("(EXIT10000476)", ex), "EX01", classNMethodName: "pgTrip.DaySelectionHandle");
						App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000476)");
					}
				}
				return _daySelection;
			}
		}

		private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			try
			{
				ResizeTripList();
			}
			catch (Exception ex)
			{
				App.ShowDebugMsg($@"{ex.Message}; (EXIT10000477); pgTrip.Page_SizeChanged");
				App.Log.LogError(_logChannel, "-", new Exception("(EXIT10000477)", ex), "EX01", classNMethodName: "pgTrip.Page_SizeChanged");
				App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000477)");
			}
		}

		private void _daySelection_OnDateSelected(object sender, DateSelectedEventArgs e)
		{
			try
			{
				_selectedDay = e.SelectedDate;

				if (_tripMode == TripMode.Return)
					App.MainScreenControl.UpdateReturnDate(_selectedDay);
				else
					App.MainScreenControl.UpdateDepartDate(_selectedDay);

				LoadTripData();
			}
			catch (Exception ex)
			{
				App.ShowDebugMsg($@"{ex.Message}; (EXIT10000478); pgTrip._daySelection_OnDateSelected");
				App.Log.LogError(_logChannel, "-", new Exception("(EXIT10000478)", ex), "EX01", classNMethodName: "pgTrip._daySelection_OnDateSelected");
				App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000478)");
			}
		}

		private void _daySelection_OnBeginDayCellTouchMove(object sender, EventArgs e)
		{
			try
			{
				BdTripViewerAllShield.Visibility = Visibility.Collapsed;

				_tripListHelper.ClearTripLoading();

				if (_language == LanguageCode.Malay)
				{
					if (TripMode == TripMode.Depart)
						TxtShieldCoverMsg.Text = _langMal["SELECT_DEPART_DATE_Label"]?.ToString();
					else if (TripMode == TripMode.Return)
						TxtShieldCoverMsg.Text = _langMal["SELECT_RETURN_DATE_Label"]?.ToString();
					else
						TxtShieldCoverMsg.Text = _langMal["SELECT_DATE_Label"]?.ToString();
				}
				else
				{
					if (TripMode == TripMode.Depart)
						TxtShieldCoverMsg.Text = _langEng["SELECT_DEPART_DATE_Label"]?.ToString();
					else if (TripMode == TripMode.Return)
						TxtShieldCoverMsg.Text = _langEng["SELECT_RETURN_DATE_Label"]?.ToString();
					else
						TxtShieldCoverMsg.Text = _langEng["SELECT_DATE_Label"]?.ToString();
				}
				BdTripViewerShield.Visibility = Visibility.Visible;

			}
			catch (Exception ex)
			{
				App.ShowDebugMsg($@"{ex.Message}; (EXIT10000479); pgTrip._daySelection_OnBeginDayCellTouchMove");
				App.Log.LogError(_logChannel, "-", new Exception("(EXIT10000479)", ex), "EX01", classNMethodName: "pgTrip._daySelection_OnBeginDayCellTouchMove");
				App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000479)");
			}
		}

		private void _daySelection_OnBeginDayCellMoveAnimate(object sender, EventArgs e)
		{
			BdTripViewerAllShield.Visibility = Visibility.Visible;

			TxtShieldAllCoverMsg.Text = "Loading in progress ..";
			if (_language == LanguageCode.Malay)
				TxtShieldAllCoverMsg.Text = _langMal["LOADING_IN_PROGRESS_Label"]?.ToString();
			else
				TxtShieldAllCoverMsg.Text = _langEng["LOADING_IN_PROGRESS_Label"]?.ToString();

			TxtShieldCoverMsg.Text = "----- ..";
			BdTripViewerShield.Visibility = Visibility.Collapsed;
		}

		public void InitData(UserSession session, TravelMode travelMode)
		{
			try
			{
				// At thie moment (2020-03-15) Only implement Depart Travel
				_tripMode = (travelMode == TravelMode.ReturnOnly) ? TripMode.Return : TripMode.Depart;

				_language = session.Language;
				
				_noOfPax = session.NumberOfPassenger;

				if (_tripMode == TripMode.Return)
                {
					_passengerDepartDateTime = session.DepartPassengerDepartDateTime ;
					_fromStationCode = session.DestinationStationCode;
					_toStationCode = session.OriginStationCode;
					_departureStationDesc = session.DestinationStationName;
					_destinationeStationDesc = session.OriginStationName;

					if (_language == LanguageCode.Malay)
						TxtTrainNo.Text = string.Format(_langMal["TRAIN_NO_Label"].ToString(), 2);
					else
						TxtTrainNo.Text = string.Format(_langEng["TRAIN_NO_Label"].ToString(), 2);

					//TxtService.Text = session.ReturnVehicleService ?? "*";
					TxtService.Text = session.SelectedVehicleService ?? "*";

					TxtDepartureStationDesc.Text = (string.IsNullOrWhiteSpace(session.DestinationStationName) == false) ? session.DestinationStationName : "--";
					TxtDestinationStationDesc.Text = (string.IsNullOrWhiteSpace(session.OriginStationName) == false) ? session.OriginStationName : "--";
				}
				else
                {
					_passengerDepartDateTime = null; 

					_fromStationCode = session.OriginStationCode;
					_toStationCode = session.DestinationStationCode;
					_departureStationDesc = session.OriginStationName;
					_destinationeStationDesc = session.DestinationStationName;

					if (_language == LanguageCode.Malay)
						TxtTrainNo.Text = string.Format(_langMal["TRAIN_NO_Label"].ToString(), 1);
					else
						TxtTrainNo.Text = string.Format(_langEng["TRAIN_NO_Label"].ToString(), 1);

					//TxtService.Text = session.DepartVehicleService ?? "*";
					TxtService.Text = session.SelectedVehicleService ?? "*";

					TxtDepartureStationDesc.Text = (string.IsNullOrWhiteSpace(session.OriginStationName) == false) ? session.OriginStationName : "--";
					TxtDestinationStationDesc.Text = (string.IsNullOrWhiteSpace(session.DestinationStationName) == false) ? session.DestinationStationName : "--";
				}

				DayCellSelector.InitCommonProperties(_passengerDepartDateTime);

				TxtSubOriginDest.Text = $@"{_departureStationDesc}  >  {_destinationeStationDesc}";

				DayCellInfo.Language = _language;

				if (_language == LanguageCode.Malay)
				{
					if (_tripMode == TripMode.Return)
					{
						TxtTripMode.Text = _langMal["RETURN_TRIP_MODE_Label"]?.ToString();
						TxtTripModeDesc.Text = _langMal["RETURN_TRIP_MODE_DESC_Label"]?.ToString();
					}
					else
					{
						TxtTripMode.Text = _langMal["DEPART_TRIP_MODE_Label"]?.ToString();
						TxtTripModeDesc.Text = _langMal["DEPART_TRIP_MODE_DESC_Label"]?.ToString();
					}
				}
				else
				{
					if (_tripMode == TripMode.Return)
					{
						TxtTripMode.Text = _langEng["RETURN_TRIP_MODE_Label"]?.ToString();
						TxtTripModeDesc.Text = _langEng["RETURN_TRIP_MODE_DESC_Label"]?.ToString();
					}
					else
					{
						TxtTripMode.Text = _langEng["DEPART_TRIP_MODE_Label"]?.ToString();
						TxtTripModeDesc.Text = _langEng["DEPART_TRIP_MODE_DESC_Label"]?.ToString();
					}
				}

				_tripListHelper.InitTripData(_fromStationCode, _toStationCode, _language, session.SelectedVehicleService, _noOfPax, _tripMode);

				if ((session.DepartPassengerDepartDateTime.HasValue) && (_tripMode == TripMode.Depart))
					_selectedDay = new DateTime(session.DepartPassengerDepartDateTime.Value.Year, session.DepartPassengerDepartDateTime.Value.Month, session.DepartPassengerDepartDateTime.Value.Day, 0, 0, 0, 0);

				else if ((session.ReturnPassengerDepartDateTime.HasValue) && (_tripMode == TripMode.Return))
				{
					_selectedDay = new DateTime(session.ReturnPassengerDepartDateTime.Value.Year, session.ReturnPassengerDepartDateTime.Value.Month, session.ReturnPassengerDepartDateTime.Value.Day, 0, 0, 0, 0);
				}
				else
				{
					DateTime currDate = DateTime.Now;
					_selectedDay = new DateTime(currDate.Year, currDate.Month, currDate.Day, 0, 0, 0, 0);
				}

				if (_passengerDepartDateTime.HasValue)
				{
					DateTime passgDepDate = _passengerDepartDateTime.Value;
					int int_PassgDepDate = Convert.ToInt32($@"{passgDepDate.Year:0000}{passgDepDate.Month:00}{passgDepDate.Day:00}");
					int int_SelectedDate = Convert.ToInt32($@"{_selectedDay.Year:0000}{_selectedDay.Month:00}{_selectedDay.Day:00}");

					if (int_SelectedDate < int_PassgDepDate)
						_selectedDay = new DateTime(_passengerDepartDateTime.Value.Year, _passengerDepartDateTime.Value.Month, _passengerDepartDateTime.Value.Day, 0, 0, 0, 0);
				}
			}
			catch (Exception ex)
			{
				App.ShowDebugMsg($@"{ex.Message}; (EXIT10000481); pgTrip.InitData");
				App.Log.LogError(_logChannel, "-", new Exception("(EXIT10000481)", ex), "EX01", classNMethodName: "pgTrip.InitData");
				App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000481)");
			}
		}

		public void UpdateDepartTripList(UIDepartTripListAck uiDTrip, UserSession session)
		{
			_tripListHelper.UpdateTripList(uiDTrip, session);
		}

		public void UpdateReturnTripList(UIReturnTripListAck uiRTrip, UserSession session)
		{
			_tripListHelper.UpdateTripList(uiRTrip, session);
		}

		private void InitShowTrips()
		{
			//TxtShieldCoverMsg.Text = "Loading in progress ..";
			SelectedTripId = null;

			if (_tripMode == TripMode.Return)
            {

            }

			DaySelectionHandle.StartSelection(_selectedDay);
			ResizeTripList();

			if (_language == LanguageCode.Malay)
			{
				TxtShieldCoverMsg.Text = _langMal["LOADING_IN_PROGRESS_Label"]?.ToString();

				if (_tripMode == TripMode.Return)
					TxtTripMode.Text = _langMal["RETURN_TRIP_MODE_Label"]?.ToString();
				else
					TxtTripMode.Text = _langMal["DEPART_TRIP_MODE_Label"]?.ToString();
			}
			else
			{
				TxtShieldCoverMsg.Text = _langEng["LOADING_IN_PROGRESS_Label"]?.ToString();

				if (_tripMode == TripMode.Return)
					TxtTripMode.Text = _langEng["RETURN_TRIP_MODE_Label"]?.ToString();
				else
					TxtTripMode.Text = _langEng["DEPART_TRIP_MODE_Label"]?.ToString();
			}
			

			LoadTripData();
		}

		private void ResizeTripList()
		{
			if (_lstTripViewDefault.OrgTimeColWidth > 0)
			{
				GridViewColumn gvc = (GridViewColumn)this.FindName("GvcDepartTimeCol");
				if (gvc != null)
					gvc.Width = _lstTripViewDefault.OrgTimeColWidth;
			}

			if (_lstTripViewDefault.OrgArrivalTimeColWidth > 0)
			{
				GridViewColumn gvc = (GridViewColumn)this.FindName("GvcArrivalTimeCol");
				if (gvc != null)
					gvc.Width = _lstTripViewDefault.OrgTimeColWidth;
			}

			if (_lstTripViewDefault.OrgFareColWidth > 0)
			{
				GridViewColumn gvc = (GridViewColumn)this.FindName("GvcFareCol");
				if (gvc != null)
					gvc.Width = _lstTripViewDefault.OrgFareColWidth;
			}
			if (_lstTripViewDefault.OrgSeatColWidth > 0)
			{
				GridViewColumn gvc = (GridViewColumn)this.FindName("GvcSeatCol");
				if (gvc != null)
					gvc.Width = _lstTripViewDefault.OrgSeatColWidth;
			}
			if ((_lstTripViewDefault.OrgTripDetailColWidth > 0) && (this.ActualWidth > 0))
			{
				double newWidth = (_lstTripViewDefault.OrgTripDetailColWidth / _designWidth) * this.ActualWidth;

				if (this.ActualWidth == _designWidth)
					newWidth = _lstTripViewDefault.OrgTripDetailColWidth;

				else if ((this.ActualWidth - _designWidth) > 0)
					newWidth = _lstTripViewDefault.OrgTripDetailColWidth + (this.ActualWidth - _designWidth);

				else
					newWidth = _lstTripViewDefault.OrgTripDetailColWidth + (_designWidth - this.ActualWidth);

				GridViewColumn gvc = (GridViewColumn)this.FindName("GvcTripDetailCol");
				if (gvc != null)
					gvc.Width = newWidth;
			}
			if (_lstTripViewDefault.OrgPickingColWidth > 0)
			{
				GridViewColumn gvc = (GridViewColumn)this.FindName("GvcPickingCol");
				if (gvc != null)
					gvc.Width = _lstTripViewDefault.OrgPickingColWidth;
			}
		}

		private int _debugInx = 0;
		private bool _tripSelected = false;

		private void SelectTrip(TripDetailViewRow tripRow)
		{
			if (tripRow != null)
			{
				SelectedTripId = tripRow.TripId;

				App.ShowDebugMsg($@"Selected Trip ID : {SelectedTripId}");
				_tripListHelper.ClearTripLoading();
				_tripSelected = true;

				if (_tripMode == TripMode.Return)
                {
					SubmitReturnTrip(
						tripRow.TripId,
						tripRow.DepartDateTime,
						tripRow.ArrivalDateTime,
						tripRow.DepartTimeStr,
						tripRow.ArrivalDayOffset,
						tripRow.ArrivalTimeStr,
						tripRow.VehicleService,
						tripRow.VehicleNo,
						tripRow.ServiceCategory,
						tripRow.Currency,
						tripRow.Price
					);
				}
				else
                {
					SubmitDepartTrip(
						tripRow.TripId,
						tripRow.DepartDateTime,
						tripRow.ArrivalDateTime,
						tripRow.DepartTimeStr,
						tripRow.ArrivalDayOffset,
						tripRow.ArrivalTimeStr,
						tripRow.VehicleService,
						tripRow.VehicleNo,
						tripRow.ServiceCategory,
						tripRow.Currency,
						tripRow.Price
					);
				}
			}
		}

		private void SubmitDepartTrip(string tripId,
			DateTime passengerDepartDateTime,
			DateTime passengerArrivalDateTime,
			string passengerDepartTimeStr,
			int passengerArrivalDayOffset,
			string passengerArrivalTimeStr,
			string vehicleService,
			string vehicleNo,
			string serviceCategory,
			string currency,
			decimal price)
		{

			ShieldPage();
			System.Windows.Forms.Application.DoEvents();

			Thread submitWorker = new Thread(new ThreadStart(new Action(() => {
				try
				{
					App.NetClientSvc.SalesService.SubmitDepartTrip(
						tripId,
						passengerDepartDateTime,
						passengerArrivalDateTime,
						passengerDepartTimeStr,
						passengerArrivalDayOffset,
						passengerArrivalTimeStr,
						vehicleService,
						vehicleNo,
						serviceCategory,
						currency,
						price,
						out bool isServerResponded);

					if (isServerResponded == false)
						App.MainScreenControl.Alert(detailMsg: "Local Server not responding (EXIT10000482)");
				}
				catch (Exception ex)
				{
					App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000483)");
					App.Log.LogError(_logChannel, "", new Exception("(EXIT10000483)", ex), "EX01", "pgTrip.SubmitDepartTrip");
				}
			})));
			submitWorker.IsBackground = true;
			submitWorker.Start();
		}

		private void SubmitReturnTrip(string tripId,
			DateTime passengerDepartDateTime,
			DateTime passengerArrivalDateTime,
			string passengerDepartTimeStr,
			int passengerArrivalDayOffset,
			string passengerArrivalTimeStr,
			string vehicleService,
			string vehicleNo,
			string serviceCategory,
			string currency,
			decimal price)
		{

			ShieldPage();
			System.Windows.Forms.Application.DoEvents();

			Thread submitWorker = new Thread(new ThreadStart(new Action(() => {
				try
				{
					App.NetClientSvc.SalesService.SubmitReturnTrip(
						tripId,
						passengerDepartDateTime,
						passengerArrivalDateTime,
						passengerDepartTimeStr,
						passengerArrivalDayOffset,
						passengerArrivalTimeStr,
						vehicleService,
						vehicleNo,
						serviceCategory,
						currency,
						price,
						out bool isServerResponded);

					if (isServerResponded == false)
						App.MainScreenControl.Alert(detailMsg: "Local Server not responding (EXIT1000096)");
				}
				catch (Exception ex)
				{
					App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000497)");
					App.Log.LogError(_logChannel, "", new Exception("(EXIT10000497)", ex), "EX01", "pgTrip.SubmitReturnTrip");
				}
			})));
			submitWorker.IsBackground = true;
			submitWorker.Start();
		}

		private void LstTrip_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			try
			{
				if (_tripSelected)
					return;

				if (LstTrip.SelectedItem is TripDetailViewRow tripRow)
                {
					if (tripRow.IsPickSeatVisible == Visibility.Visible)
                    {
						if (string.IsNullOrWhiteSpace(tripRow.TripId) == false)
						{
							App.ShowDebugMsg($@"SelectedItem : {LstTrip.SelectedItem?.GetType().ToString()}; pgTrip.LstTrip_SelectionChanged");
							SelectTrip(tripRow);
						}
					}
				}
			}
			catch (Exception ex)
			{
				App.ShowDebugMsg($@"{ex.Message}; (EXIT10000484); pgTrip.LstTrip_SelectionChanged");
				App.Log.LogError(_logChannel, "-", new Exception("(EXIT10000484)", ex), "EX01", classNMethodName: "pgTrip.LstTrip_SelectionChanged");
				//App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000484)");
			}
		}

		public void ShieldPage()
		{
			TxtScreenShield1.Visibility = Visibility.Visible;
			TxtScreenShield2.Visibility = Visibility.Collapsed;
			TxtErrorCode.Visibility = TxtScreenShield2.Visibility;

			GrdScreenShield.Visibility = Visibility.Visible;
		}

		public void UpdateShieldErrorMessage(string message)
		{
			TxtScreenShield1.Visibility = Visibility.Collapsed;
			TxtScreenShield2.Visibility = Visibility.Visible;
			TxtErrorCode.Visibility = TxtScreenShield2.Visibility;

			message = message ?? "";

			string errorCode = "";

			if (message.Contains("(EXIT21638)"))
				errorCode = "(EXIT21638)";

			else if (message.Contains("(EXIT21639)"))
				errorCode = "(EXIT21639)";

			if (_language == LanguageCode.Malay)
				TxtScreenShield2.Text = _langMal["SEAT_QUERY_ERROR_Label"]?.ToString();
			else
				TxtScreenShield2.Text = _langEng["SEAT_QUERY_ERROR_Label"]?.ToString();

			TxtErrorCode.Text = errorCode;

			_tripSelected = false;
			SelectedTripId = null;

			LstTrip.SelectedIndex = -1;
			LstTrip.SelectedItem = null;

		}

		public void ResetPageAfterError()
		{
			TxtScreenShield1.Visibility = Visibility.Visible;
			TxtScreenShield2.Visibility = Visibility.Collapsed;
			TxtScreenShield2.Text = "";
			TxtErrorCode.Visibility = TxtScreenShield2.Visibility;
			TxtErrorCode.Text = "";

			GrdScreenShield.Visibility = Visibility.Collapsed;
		}

		private string GetTripId(string tripIdCode)
		{
			if ((tripIdCode != null) && (tripIdCode.IndexOf(TripDetailViewListHelper.TripIdPrefix) == 0))
			{
				string retId = tripIdCode.Substring(TripDetailViewListHelper.TripIdPrefix.Length).Trim();

				if (retId.Length > 0)
					return retId;
				else
					return null;
			}
			else
				return null;
		}

		private void LoadTripData()
		{
			_tripListHelper.UpdateList(_selectedDay);
		}

		private void LstTrip_ScrollChanged(object sender, ScrollChangedEventArgs e)
		{
			try
			{
				App.TimeoutManager.ResetTimeout();
			}
			catch (Exception ex)
			{
				App.Log.LogError(_logChannel, "-", ex, "EX01", classNMethodName: "pgStation.LstTrip_ScrollChanged");
			}
		}

		
	}
}
