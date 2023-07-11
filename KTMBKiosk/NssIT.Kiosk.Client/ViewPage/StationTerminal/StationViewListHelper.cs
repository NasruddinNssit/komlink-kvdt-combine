﻿using NssIT.Train.Kiosk.Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NssIT.Kiosk.Client.ViewPage.StationTerminal
{
	public class StationViewListHelper
	{
		private Page _pgStationSelection = null;
		private ListView _lstStationViewer = null;
		private ScrollViewer _lstStationScrollViewer = null;
		private StationViewList _stationViewList = null;

		private StationViewRow[] _orgStationViewList = null;

		public StationViewListHelper(Page pgStationSelection, ListView lstStationViewer)
		{
			_pgStationSelection = pgStationSelection;
			_lstStationViewer = lstStationViewer;

			_stationViewList = new StationViewList();
			_lstStationViewer.DataContext = _stationViewList;

			
		}

		private ScrollViewer GetScrollViewer()
        {
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(_lstStationViewer); i++)
			{
				Decorator childVisual = VisualTreeHelper.GetChild(_lstStationViewer, i) as Decorator;

				if (childVisual.Child is ScrollViewer sv)
                {
					return sv;
				}
			}
			return null;
		}

		public void Filter(string stationFilter, string state)
		{
			using (var d = _pgStationSelection.Dispatcher.DisableProcessing())
			{
				_lstStationViewer.DataContext = null;

				List<StationViewRow> filteredList = null;

				if (string.IsNullOrWhiteSpace(stationFilter) && string.IsNullOrWhiteSpace(state))
				{
					filteredList = _orgStationViewList.ToList();
				}
				else if ((!string.IsNullOrWhiteSpace(stationFilter)) && (!string.IsNullOrWhiteSpace(state)))
				{
					filteredList = (from row in _orgStationViewList
									where (
										(row.StationDesc.ToUpper().Contains(stationFilter) && (row.State.Equals(state)))
										||
										((row.IsIndicateState == true) && (row.State.Equals(state)))
										)
									select row).ToList();
				}
				else
				{
					if (!string.IsNullOrWhiteSpace(stationFilter))
					{
						filteredList = (from row in _orgStationViewList
										where (row.StationDesc.ToUpper().Contains(stationFilter) || (row.IsIndicateState == true))
										select row).ToList();
					}
					else
					{
						filteredList = (from row in _orgStationViewList
										where (row.State.Equals(state))
										select row).ToList();
					}
				}

				StationViewRow[] filteredSortedList = TrimNSortStationList(filteredList);

				_stationViewList.Clear();
				foreach (StationViewRow row in filteredSortedList)
				{
					_stationViewList.Add(row);
				}
				_lstStationViewer.DataContext = _stationViewList;
			}

			//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
			StationViewRow[] TrimNSortStationList(List<StationViewRow> stationList)
			{
				StationViewRow[] retArr = new StationViewRow[0];

				if ((stationList is null) || (stationList.Count == 0))
					return retArr;

				var stateList = (from st in stationList
								 group st by st.State into g
								 select new { StateCode = g.Key }).ToArray();

				List<StationViewRow> trimStationList = new List<StationViewRow>();

				foreach (var stateX in stateList)
				{
					string stateCode = stateX.StateCode;

					StationViewRow[] stList = (from stationS in stationList
												where (stationS.State.Equals(stateCode))
												select stationS).ToArray();

					if (stList.Length > 1)
					{
						foreach(StationViewRow sr in stList)
						{
							trimStationList.Add(new StationViewRow() { State = sr.State, StateDesc = sr.StateDesc, Station = sr.Station, StationDesc = sr.StationDesc, IsIndicateState = sr.IsIndicateState, TrainService = sr.TrainService });
						}
					}
				}

				retArr = (from stt in trimStationList
						  orderby stt.StateDesc, stt.StationDesc
						  select stt).ToArray();

				return retArr;
			}
		}

		//public void Filter(string stationFilter, string state)
		//{
		//	StationViewRow[] filteredRows = null;

		//	if (string.IsNullOrWhiteSpace(stationFilter) && string.IsNullOrWhiteSpace(state))
		//	{
		//		filteredRows = (from row in _orgStationViewList
		//						orderby row.State, row.Station
		//						select row).ToArray();
		//	}
		//	else if ((!string.IsNullOrWhiteSpace(stationFilter)) && (!string.IsNullOrWhiteSpace(state)))
		//	{
		//		filteredRows = (from row in _orgStationViewList
		//						where (
		//							(row.StationDesc.ToUpper().Contains(stationFilter) && (row.State.Equals(state)))
		//							||
		//							((row.IsIndicateState == true) && (row.State.Equals(state)))
		//							)
		//						orderby row.State, row.Station
		//						select row).ToArray();
		//	}
		//	else
		//	{
		//		if (!string.IsNullOrWhiteSpace(stationFilter))
		//		{
		//			filteredRows = (from row in _orgStationViewList
		//							where (row.StationDesc.ToUpper().Contains(stationFilter) || (row.IsIndicateState == true))
		//							orderby row.State, row.Station
		//							select row).ToArray();
		//		}
		//		else
		//		{
		//			filteredRows = (from row in _orgStationViewList
		//							where (row.State.Equals(state))
		//							orderby row.State, row.Station
		//							select row).ToArray();
		//		}
		//	}

		//	_stationViewList.Clear();

		//	foreach (StationViewRow row in filteredRows)
		//	{
		//		_stationViewList.Add(row);
		//	}
		//}

		public void CreateStationList(StationDetailsModel[] stationList, StateModel[] stateMasterList)
		{
			string sPACE = " ";
			_stationViewList.Clear();

			if (_lstStationScrollViewer is null)
			{
				System.Windows.Forms.Application.DoEvents();
				_lstStationScrollViewer = GetScrollViewer();
			}

			if (stationList != null)
			{
				var stateList = (from st in stationList
								 group st by st.State into g
								 select new { StateCode = g.Key }).ToArray();


				List<StateModel> validStateList = new List<StateModel>();

				foreach (var state in stateList)
				{
					// Note : For KTM State Code is same as State Name
					string stateCode = state.StateCode;
					if (stateMasterList?.Length > 0)
					{
						var stList = (from stateMas in stateMasterList
									  where stateMas.State.Equals(stateCode)
									  select stateMas).ToArray();

						if (stList.Length > 0)
						{
							validStateList.Add(new StateModel() { State = stateCode });
						}
						//else
						//	throw new Exception("Unable to read state info correctly.");
					}
					else
						validStateList.Add(new StateModel() { State = stateCode });
				}

				StateModel[] sortedStateList = (from stt in validStateList
												   orderby stt.State
												   select stt).ToArray();

				foreach (var state in sortedStateList)
				{
					string stateCode = state.State;
					string stateDesc = state.State;

					_stationViewList.Add(new StationViewRow() { State = stateCode, Station = "", StationDesc = $@"{sPACE}{stateDesc}", IsIndicateState = true, StateDesc = stateDesc }); ;

					StationDetailsModel[] selectedStationList = (from st in stationList
																where st.State.Equals(stateCode)
																select st).ToArray();

					foreach (StationDetailsModel station in selectedStationList)
					{
						_stationViewList.Add(new StationViewRow() { State = stateCode, Station = station.Id, StationDesc = station.Description?.Trim(), StateDesc = stateDesc, TrainService = station.TrainService });
					}

				}
			}

			_orgStationViewList = (from r in _stationViewList.ListItems
								   orderby r.StateDesc, r.StationDesc
								   select r).ToArray();

			if (_lstStationScrollViewer is ScrollViewer)
            {
				_lstStationScrollViewer.ScrollToVerticalOffset(0d);
				System.Windows.Forms.Application.DoEvents();
			}
		}

		public void DebugTest_CreateStationList()
		{
			_stationViewList.Clear();

			_stationViewList.Add(new StationViewRow() { State = "PERLIS", Station = "", StationDesc = "PERLIS", IsIndicateState = true });
			_stationViewList.Add(new StationViewRow() { State = "PERLIS", Station = "Air Tawar", StationDesc = "Air Tawar" });
			_stationViewList.Add(new StationViewRow() { State = "PERLIS", Station = "Alor Gajah", StationDesc = "Alor Gajah" });

			_stationViewList.Add(new StationViewRow() { State = "KEDAH", Station = "", StationDesc = "KEDAH", IsIndicateState = true });
			_stationViewList.Add(new StationViewRow() { State = "KEDAH", Station = "Bachok", StationDesc = "Bachok" });
			_stationViewList.Add(new StationViewRow() { State = "KEDAH", Station = "Bagan Datuk", StationDesc = "Bagan Datuk" });
			_stationViewList.Add(new StationViewRow() { State = "KEDAH", Station = "Bagan Serai", StationDesc = "Bagan Serai" });
			_stationViewList.Add(new StationViewRow() { State = "KEDAH", Station = "Bahau", StationDesc = "Bahau" });

			_stationViewList.Add(new StationViewRow() { State = "PERAK", Station = "", StationDesc = "PERAK", IsIndicateState = true });
			_stationViewList.Add(new StationViewRow() { State = "PERAK", Station = "AMANJAYA", StationDesc = "AMANJAYA" });
			_stationViewList.Add(new StationViewRow() { State = "PERAK", Station = "AIR TAWAR", StationDesc = "AIR TAWAR" });
			_stationViewList.Add(new StationViewRow() { State = "PERAK", Station = "BIDOR", StationDesc = "BIDOR" });
			_stationViewList.Add(new StationViewRow() { State = "PERAK", Station = "BATU GAJAH", StationDesc = "BATU GAJAH" });
			_stationViewList.Add(new StationViewRow() { State = "PERAK", Station = "BAGAN DATUK", StationDesc = "BAGAN DATUK" });
			_stationViewList.Add(new StationViewRow() { State = "PERAK", Station = "BAGAN SERAI", StationDesc = "BAGAN SERAI" });
			_stationViewList.Add(new StationViewRow() { State = "PERAK", Station = "DEPOH KBES", StationDesc = "DEPOH KBES" });
			_stationViewList.Add(new StationViewRow() { State = "PERAK", Station = "PARIT BUNTAR", StationDesc = "PARIT BUNTAR" });

			_stationViewList.Add(new StationViewRow() { State = "KUALA LUMPUR", Station = "", StationDesc = "KUALA LUMPUR", IsIndicateState = true });
			_stationViewList.Add(new StationViewRow() { State = "KUALA LUMPUR", Station = "ONE UTAMA", StationDesc = "ONE UTAMA" });
			_stationViewList.Add(new StationViewRow() { State = "KUALA LUMPUR", Station = "BUKIT BINTANG", StationDesc = "BUKIT BINTANG" });
			_stationViewList.Add(new StationViewRow() { State = "KUALA LUMPUR", Station = "PLAZA DAMAS", StationDesc = "PLAZA DAMAS" });
			_stationViewList.Add(new StationViewRow() { State = "KUALA LUMPUR", Station = "PUDURAYA", StationDesc = "PUDURAYA" });
			_stationViewList.Add(new StationViewRow() { State = "KUALA LUMPUR", Station = "KL SENTRAL", StationDesc = "KL SENTRAL" });
			_stationViewList.Add(new StationViewRow() { State = "KUALA LUMPUR", Station = "GREENWOOD", StationDesc = "GREENWOOD" });
			_stationViewList.Add(new StationViewRow() { State = "KUALA LUMPUR", Station = "E CURVE", StationDesc = "E CURVE" });

			_orgStationViewList = (from r in _stationViewList.ListItems
								   select r).ToArray();

		}

		//public destination_detail[] Debug_SampleStations()
		//{
		//	List<destination_detail> list = new List<destination_detail>();

		//	list.Add(new destination_detail() { State = "PERLIS", code = "Air Tawar", desc = "Air Tawar" });
		//	list.Add(new destination_detail() { State = "PERLIS", code = "Alor Gajah", desc = "Alor Gajah" });

		//	list.Add(new destination_detail() { State = "KEDAH", code = "Bachok", desc = "Bachok" });
		//	list.Add(new destination_detail() { State = "KEDAH", code = "Bagan Datuk", desc = "Bagan Datuk" });
		//	list.Add(new destination_detail() { State = "KEDAH", code = "Bagan Serai", desc = "Bagan Serai" });
		//	list.Add(new destination_detail() { State = "KEDAH", code = "Bahau", desc = "Bahau" });

		//	list.Add(new destination_detail() { State = "PERAK", code = "AMANJAYA", desc = "AMANJAYA" });
		//	list.Add(new destination_detail() { State = "PERAK", code = "AIR TAWAR", desc = "AIR TAWAR" });
		//	list.Add(new destination_detail() { State = "PERAK", code = "BIDOR", desc = "BIDOR" });
		//	list.Add(new destination_detail() { State = "PERAK", code = "BATU GAJAH", desc = "BATU GAJAH" });
		//	list.Add(new destination_detail() { State = "PERAK", code = "BAGAN DATUK", desc = "BAGAN DATUK" });
		//	list.Add(new destination_detail() { State = "PERAK", code = "BAGAN SERAI", desc = "BAGAN SERAI" });
		//	list.Add(new destination_detail() { State = "PERAK", code = "DEPOH KBES", desc = "DEPOH KBES" });
		//	list.Add(new destination_detail() { State = "PERAK", code = "PARIT BUNTAR", desc = "PARIT BUNTAR" });

		//	list.Add(new destination_detail() { State = "KUALA LUMPUR", code = "ONE UTAMA", desc = "ONE UTAMA" });
		//	list.Add(new destination_detail() { State = "KUALA LUMPUR", code = "BUKIT BINTANG", desc = "BUKIT BINTANG" });
		//	list.Add(new destination_detail() { State = "KUALA LUMPUR", code = "PLAZA DAMAS", desc = "PLAZA DAMAS" });
		//	list.Add(new destination_detail() { State = "KUALA LUMPUR", code = "PUDURAYA", desc = "PUDURAYA" });
		//	list.Add(new destination_detail() { State = "KUALA LUMPUR", code = "KL SENTRAL", desc = "KL SENTRAL" });
		//	list.Add(new destination_detail() { State = "KUALA LUMPUR", code = "GREENWOOD", desc = "GREENWOOD" });
		//	list.Add(new destination_detail() { State = "KUALA LUMPUR", code = "E CURVE", desc = "E CURVE" });

		//	return list.ToArray();
		//}
	}
}
