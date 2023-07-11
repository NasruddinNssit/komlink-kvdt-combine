using NssIT.Kiosk.Log.DB;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static NssIT.Kiosk.Client.ViewPage.KeyboardEventArgs;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI;
using NssIT.Kiosk.AppDecorator.UI;
using NssIT.Kiosk.AppDecorator.Common;
using System.Threading;
using System.Windows.Media;
using System.Threading.Tasks;
using NssIT.Kiosk.Client.Base;
using NssIT.Train.Kiosk.Common.Data;
using NssIT.Train.Kiosk.Common.Data.Response;
using System.Linq;
using System.Windows.Documents;
using System.Collections.Generic;


namespace NssIT.Kiosk.Client.ViewPage.StationTerminal
{
    /// <summary>
    /// Interaction logic for pgStation.xaml
    /// </summary>
    public partial class pgStation : Page, IStation, IKioskViewPage
	{
		private string _logChannel = "ViewPage";

		private bool _selectedFlag = false;
		private DbLog Log { get; set; }
		private StateViewListHelper _stateViewListHelper = null;
		private StationViewListHelper _stationViewListHelper = null;

		private string _selectedState = null;

		private pgRouteMap _routeMapPage = null;
		private LanguageCode _language = LanguageCode.English;
		private ResourceDictionary _langMal = null;
		private ResourceDictionary _langEng = null;

		private StationSelectionMode _stationMode = StationSelectionMode.OriginStation;

		public pgStation()
		{
			InitializeComponent();

			_routeMapPage = new pgRouteMap();
			frmRouteMap.NavigationService.Navigate(_routeMapPage);
            _routeMapPage.OnExit += _routeMapPage_OnExit;

			_langMal = CommonFunc.GetXamlResource(@"ViewPage\StationTerminal\rosStationMalay.xaml");
			_langEng = CommonFunc.GetXamlResource(@"ViewPage\StationTerminal\rosStationEng.xaml");

			Log = DbLog.GetDbLog();

			_stateViewListHelper = new StateViewListHelper(this, LstStateView);

			KbKeys.OnKeyPressed += KbKeys_OnKeyPressed;
			_stateViewListHelper.OnStateChanged += _stateViewListHelper_OnStateChanged;

			_stationViewListHelper = new StationViewListHelper(this, LstStationView);
		}

        

        private StationDetailsModel[] _stationList = null;
		private StateModel[] _stateList = null;
		public void InitStationData(UIDestinationListAck uiDest)
		{
			_stationMode = StationSelectionMode.DestinationStation;
			StationResult dest = (StationResult)uiDest.MessageData;

			try
			{
				if (dest is null)
					throw new Exception("Fail to show Station List; (EXIT10000201)");

				else if ((dest.Data is null) || (dest.Data.Length == 0))
					throw new Exception("Fail to show Station List; (EXIT10000202)");

				else
				{
					_language = (uiDest.Session != null ? uiDest.Session.Language : LanguageCode.English);
					_stateViewListHelper.InitView(_language);

					App.ShowDebugMsg($@"pgStation ==> StationResult => Data : {dest.Data.Length};");

					StateModel[] stteList = (from p in dest.Data.ToList()
											  group p.State by p.State into g
											  select new StateModel() { State = g.Key }).ToArray();

					_stateList = stteList;
					_stationList = dest.Data;
				}
			}
			catch (Exception ex)
			{
				App.ShowDebugMsg(ex.Message);
				Log.LogText(_logChannel, "-", uiDest, "EX01", "pgStation.InitStationData", AppDecorator.Log.MessageType.Fatal, extraMsg: "MsgObj: UIDestinationList");
				Log.LogFatal(_logChannel, "-", ex, "EX02", "pgStation.InitStationData");
				App.MainScreenControl.Alert(detailMsg: ex.Message);
			}
		}

		public void InitStationData(UIOriginListAck uiOriginList)
		{
			_stationMode = StationSelectionMode.OriginStation;
			StationResult orig = (StationResult)uiOriginList.MessageData;

			try
			{
				if (orig is null)
					throw new Exception("Fail to show Station List; (EXIT10000203)");

				else if ((orig.Data is null) || (orig.Data.Length == 0))
					throw new Exception("Fail to show Station List; (EXIT10000204)");

				else
				{
					_language = (uiOriginList.Session != null ? uiOriginList.Session.Language : LanguageCode.English);
					_stateViewListHelper.InitView(_language);

					App.ShowDebugMsg($@"pgStation ==> StationResult => data : {orig.Data.Length};");

					StateModel[] stteList = (from p in orig.Data.ToList()
								  group p.State by p.State into g
								  select new StateModel() { State = g.Key }).ToArray();

					_stateList = stteList;
					_stationList = orig.Data;
				}

			}
			catch (Exception ex)
			{
				App.ShowDebugMsg(ex.Message);
				Log.LogText(_logChannel, "-", uiOriginList, "EX01", "pgStation.InitStationData", AppDecorator.Log.MessageType.Fatal, extraMsg: "MsgObj: UIOriginListAck");
				Log.LogFatal(_logChannel, "-", ex, "EX02", "pgStation.InitStationData");
				App.MainScreenControl.Alert(detailMsg: ex.Message);
			}
		}

		private void Page_Loaded(object sender, RoutedEventArgs e)
		{
			try
			{
				GrdScreenShield.Visibility = Visibility.Collapsed;

				this.Resources.MergedDictionaries.Clear();

				if (_language == LanguageCode.Malay)
					this.Resources.MergedDictionaries.Add(_langMal);
				else
					this.Resources.MergedDictionaries.Add(_langEng);

				TxtStationFilter.Text = "";
				TxtStationFilterWatermark.Visibility = Visibility.Visible;
				GrdStationFilter.Visibility = Visibility.Collapsed;

				App.MainScreenControl.MiniNavigator.AttachToFrame(frmNav, _language);

				if (_stationMode == StationSelectionMode.OriginStation)
				{
					App.MainScreenControl.MiniNavigator.IsPreviousVisible = Visibility.Hidden;
					
					if (_language == LanguageCode.Malay)
						TxtPageHeader.Text = _langMal["ORIGIN_INFO_Label"].ToString();
					else
						TxtPageHeader.Text = _langEng["ORIGIN_INFO_Label"].ToString();
				}
				else
				{
					App.MainScreenControl.MiniNavigator.IsPreviousVisible = Visibility.Visible;
					
					if (_language == LanguageCode.Malay)
						TxtPageHeader.Text = _langMal["DESTINATION_INFO_Label"].ToString();
					else
						TxtPageHeader.Text = _langEng["DESTINATION_INFO_Label"].ToString();
				}

				_selectedState = null;
				_selectedFlag = false;

				_stateViewListHelper.CreateStateList(_stateList);
				//DEBUG-Testing _stateViewListHelper.CreateStateList(_stateViewListHelper.Debug_SampleStates());
				
				_stationViewListHelper.CreateStationList(_stationList, _stateList);
				//DEBUG-Testing _stationViewListHelper.CreateStationList(_stationViewListHelper.Debug_SampleStations(), _stateViewListHelper.Debug_SampleStates());

				System.Windows.Forms.Application.DoEvents();
				_stateViewListHelper.SelectAllStates();

				LstStationView.Focus();
			}
			catch (Exception ex)
			{
				Log.LogError(_logChannel, "-", ex, "EX01", classNMethodName: "pgStation.Page_Loaded");
				App.MainScreenControl.Alert(detailMsg: $@"Error: {ex.Message}; (EXIT10000205)");
			}
		}

		private void _stateViewListHelper_OnStateChanged(object sender, StateChangedEventArgs e)
		{
			try
			{
				
				App.ShowDebugMsg($@"Selected State : {e.State}");

				if (string.IsNullOrWhiteSpace(e.State) == false)
				{
					if ((e.State.Equals(_stateViewListHelper.AllStateCode)))
						_selectedState = null;
					else
						_selectedState = e.State;
				}
				else
					_selectedState = null;

				_stationViewListHelper.Filter(TxtStationFilter.Text, _selectedState);

				//App.NetClientSvc.SalesService.ResetTimeout();
				App.TimeoutManager.ResetTimeout();
			}
			catch (Exception ex)
			{
				Log.LogError(_logChannel, "-", ex, "EX01", classNMethodName: "pgStation._stateViewListHelper_OnStateChanged");
			}
		}

		private void BdState_MouseDown(object sender, MouseButtonEventArgs e) => _stateViewListHelper.StateMouseDownHandle(sender);

		private void Station_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			try
			{
				if (_selectedFlag == false)
				{
					_selectedFlag = true;

					StationViewRow row = (StationViewRow)((Border)sender).DataContext;

					if ((row.TrainService is null) || (row.TrainService.Count == 0))
						throw new Exception($@"Unable to get valid Train Service for station {row.StationDesc}; (EXIT10000211) ;Station Mode : {Enum.GetName(typeof(StationSelectionMode), _stationMode)}");

					Submit(row.Station, row.StationDesc, row.TrainService);
				}
			}
			catch (Exception ex)
			{
				App.Log.LogError(_logChannel, "-", new Exception("(EXIT10000208)", ex), "EX01", classNMethodName: "pgLanguage.Station_MouseDown");
				App.MainScreenControl.Alert(detailMsg: $@"{ex.Message ?? "Error when select a station"}; (EXIT10000208)");
			}
		}

		private void Submit(string stationCode, string stationName, IList<string> trainService)
		{
			ShieldPage();
			System.Windows.Forms.Application.DoEvents();

			if (_stationMode == StationSelectionMode.OriginStation)
			{
				Thread submitWorker = new Thread(new ThreadStart(new Action(() => {
					try
					{
						App.NetClientSvc.SalesService.SubmitOriginStation(stationCode, stationName, trainService, out bool isServerResponded);

						if (isServerResponded == false)
							App.MainScreenControl.Alert(detailMsg: "Local Server not responding (EXIT10000206)");
					}
					catch (Exception ex)
					{
						App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000207)");
						App.Log.LogError(_logChannel, "", new Exception("(EXIT10000207)", ex), "EX01", "pgStation.Submit(Origin)");
					}
				})));
				submitWorker.IsBackground = true;
				submitWorker.Start();
			}
			else
			{
				Thread submitWorker = new Thread(new ThreadStart(new Action(() => {
					try
					{
						App.NetClientSvc.SalesService.SubmitDestination(stationCode, stationName, trainService, out bool isServerResponded);

						if (isServerResponded == false)
							App.MainScreenControl.Alert(detailMsg: "Local Server not responding (EXIT10000209)");
					}
					catch (Exception ex)
					{
						App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000210)");
						App.Log.LogError(_logChannel, "", new Exception("(EXIT10000210)", ex), "EX01", "pgStation.Submit(Destination)");
					}
				})));
				submitWorker.IsBackground = true;
				submitWorker.Start();
			}
		}

		private void Button_RouteMap(object sender, MouseButtonEventArgs e)
		{
			try
			{
				ShowRouteMap();
			}
			catch (Exception ex)
			{
				App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000210)");
				App.Log.LogError(_logChannel, "", new Exception("(EXIT10000210)", ex), "EX01", "pgStation.Button_RouteMap");
			}
		}

		private void _routeMapPage_OnExit(object sender, EventArgs e)
		{
			try
			{
				HideRouteMap();
			}
			catch (Exception ex)
			{
				App.MainScreenControl.Alert(detailMsg: $@"{ex.Message}; (EXIT10000212)");
				App.Log.LogError(_logChannel, "", new Exception("(EXIT10000212)", ex), "EX01", "pgStation._routeMapPage_OnExit");
			}
		}

		public void ShieldPage()
		{
			frmRouteMap.Visibility = Visibility.Collapsed;
			StkInProgress.Visibility = Visibility.Visible;
			GrdScreenShield.Visibility = Visibility.Visible;
		}

		public void ShowRouteMap()
		{
			App.TimeoutManager.ResetTimeout();
			_routeMapPage.SetLanguage(_language);
			StkInProgress.Visibility = Visibility.Collapsed;
			frmRouteMap.Visibility = Visibility.Visible;
			GrdScreenShield.Visibility = Visibility.Visible;
		}

		public void HideRouteMap()
		{
			App.TimeoutManager.ResetTimeout();
			GrdScreenShield.Visibility = Visibility.Collapsed;
		}

		private void KbKeys_OnKeyPressed(object sender, KeyboardEventArgs e)
		{
			//bool needToResetTimeout = false;
			try
			{
				_stateViewListHelper.SelectAllStates();

				if (e.KyCat == KeyCat.NormalChar)
				{
					TxtStationFilter.Text += e.KeyString;
				}
				else
				{
					if (e.KyCat == KeyCat.Backspace)
					{
						if (TxtStationFilter.Text.Length > 0)
						{
							TxtStationFilter.Text = TxtStationFilter.Text.Substring(0, TxtStationFilter.Text.Length - 1);
						}
					}
					else if (e.KyCat == KeyCat.Enter)
					{

					}
					else if (e.KyCat == KeyCat.Space)
					{
						TxtStationFilter.Text += " ";
					}
				}

				if (string.IsNullOrWhiteSpace(TxtStationFilter.Text))
				{
					Visibility watermarkHistoryVisibility = TxtStationFilterWatermark.Visibility;

					TxtStationFilterWatermark.Visibility = Visibility.Visible;
					GrdStationFilter.Visibility = Visibility.Collapsed;

					//if (watermarkHistoryVisibility == Visibility.Collapsed)
					//	needToResetTimeout = true;
				}
				else
				{
					TxtStationFilterWatermark.Visibility = Visibility.Collapsed;
					GrdStationFilter.Visibility = Visibility.Visible;
					TxtStationFilter.CaretIndex = TxtStationFilter.Text.Length;
					TxtStationFilter.Focus();
					//needToResetTimeout = true;
				}

				System.Windows.Forms.Application.DoEvents();
				_stationViewListHelper.Filter(TxtStationFilter.Text, _selectedState);
				
				//if (needToResetTimeout)
				//{
				//App.NetClientSvc.SalesService.ResetTimeout();
				App.TimeoutManager.ResetTimeout();
				//}
			}
			catch (Exception ex)
			{
				Log.LogError(_logChannel, "-", ex, "EX01", classNMethodName: "pgStation.KbKeys_OnKeyPressed");
			}
            finally
            {
				System.Windows.Forms.Application.DoEvents();
			}
		}

		private void TxtStationFilter_LostFocus(object sender, RoutedEventArgs e)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(TxtStationFilter.Text))
				{
					TxtStationFilterWatermark.Visibility = Visibility.Visible;
					GrdStationFilter.Visibility = Visibility.Collapsed;
				}
			}
			catch (Exception ex)
			{
				Log.LogError(_logChannel, "-", ex, "EX01", classNMethodName: "pgStation.TxtStationFilter_LostFocus");
			}
		}

		private void TxtStationFilterWatermark_GotFocus(object sender, RoutedEventArgs e)
		{
			try
			{
				TxtStationFilterWatermark.Visibility = Visibility.Collapsed;
				GrdStationFilter.Visibility = Visibility.Visible;
				TxtStationFilter.Focus();
			}
			catch (Exception ex)
			{
				App.Log.LogError(_logChannel, "", ex, "EX01", "pgStation.TxtStationFilterWatermark_GotFocus");
			}
		}

		private void BtnRefreshState_Click(object sender, RoutedEventArgs e)
		{
			_stateViewListHelper.DebugTest_CreateStateList();
		}

		private void Button_ClearStationFilter(object sender, MouseButtonEventArgs e)
		{
			try
			{
				TxtStationFilter.Text = "";

				TxtStationFilterWatermark.Visibility = Visibility.Visible;
				GrdStationFilter.Visibility = Visibility.Collapsed;

				_stationViewListHelper.Filter(TxtStationFilter.Text, _selectedState);

				//App.NetClientSvc.SalesService.ResetTimeout();
				App.TimeoutManager.ResetTimeout();
			}
			catch (Exception ex)
			{
				Log.LogError(_logChannel, "-", ex, "EX01", classNMethodName: "pgStation.Button_ClearStationFilter");
			}
		}

		private void LstStationView_ScrollChanged(object sender, ScrollChangedEventArgs e)
		{
			try
			{
				App.TimeoutManager.ResetTimeout();
			}
			catch (Exception ex)
			{
				Log.LogError(_logChannel, "-", ex, "EX01", classNMethodName: "pgStation.LstStationView_ScrollChanged");
			}
		}

		private void LstStateView_ScrollChanged(object sender, ScrollChangedEventArgs e)
		{
			try
			{
				App.TimeoutManager.ResetTimeout();
			}
			catch (Exception ex)
			{
				Log.LogError(_logChannel, "-", ex, "EX01", classNMethodName: "pgStation.LstStateView_ScrollChanged");
			}
		}

		private void LstStateView_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			try
			{
				App.TimeoutManager.ResetTimeout();

				string debugStr = (sender is null) ? "-" : $@"{sender.GetType().ToString()}";
				App.ShowDebugMsg($@"pgStation.LstStateView_SelectionChanged; sender: {debugStr}");
			}
			catch (Exception ex)
			{
				App.ShowDebugMsg($@"pgStation.LstStateView_SelectionChanged Error; {ex.ToString()}");
				Log.LogError(_logChannel, "-", ex, "EX01", classNMethodName: "pgStation.LstStateView_SelectionChanged");
			}
		}

        private void BtnTest1_Click(object sender, RoutedEventArgs e)
        {
			//try
			//{
			//	App.ShowDebugMsg($@"pgStation; Start - BtnTest1_Click");
			//	_stateViewListHelper.SelectAllStates();
			//	App.ShowDebugMsg($@"pgStation; End - BtnTest1_Click");
			//}
			//catch (Exception ex)
			//{
			//	App.ShowDebugMsg($@"pgStation.BtnTest1_Click Error; {ex.ToString()}");
			//	Log.LogError(_logChannel, "-", ex, "EX01", classNMethodName: "pgStation.LstStateView_SelectionChanged");
			//}
		}
    }
}