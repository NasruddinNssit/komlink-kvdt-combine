using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace NssIT.Kiosk.Client.ViewPage.Komuter
{
    public class RouteSelector
    {
        private string _logChannel = "ViewPage";

        /// <summary>
        /// Return true if reset successfully.
        /// </summary>
        /// <returns></returns>
        public delegate bool ResetRouteSelection();

        private pgMap _mapPage = null;

        //private Brush _selectedBackgroundColor = new SolidColorBrush(Color.FromArgb(0xFF, 0x7A, 0x79, 0x99));
        public event EventHandler<StationSelectedEventArgs> OnStationSelectChanged;

        private List<RouteElement> _routingList1 = new List<RouteElement>();
        private List<RouteElement> _routingList2 = new List<RouteElement>();
        private List<RouteElement> _routingList3 = new List<RouteElement>();

        private RouteElement _first01SelectedStation = null;
        private RouteElement _second02SelectedStation = null;
        private List<RouteElement> _selectedRoutingList = null;

        private Brush _normalBorderBrushColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xA3, 0xA3, 0xA3));
        private Brush _selectedBackgroundColor = new SolidColorBrush(Color.FromArgb(0xFF, 0xFB, 0xD0, 0x12));
        private Brush _unselectedBackgroundColor = new SolidColorBrush(Color.FromArgb(0x00, 0xFF, 0xFF, 0xFF));
        private System.Windows.Thickness _normalBorderTickness = new System.Windows.Thickness(2, 2, 2, 2);
        private System.Windows.Thickness _selectedBorderTickness = new System.Windows.Thickness(0, 0, 0, 0);

        /// <summary>
        /// Return true if reset successfully.
        /// </summary>
        private ResetRouteSelection _resetRouteSelectionDelgHandle = null;

        private DropShadowEffect _stationSelectedEffect = new DropShadowEffect()
        {
            ShadowDepth = 8,
            BlurRadius = 10,
            Direction = 270,
            Color = Color.FromArgb(255, 136, 136, 136)
        };

        public RouteSelector(pgMap mapPage, ResetRouteSelection resetRouteSelectionDelgHandle)
        {
            _mapPage = mapPage;
            _resetRouteSelectionDelgHandle = resetRouteSelectionDelgHandle;
            CreateRoutingList();
        }

        private void Station_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if ((sender is Border bd) && (bd.Tag is string stationTag) && (string.IsNullOrWhiteSpace(stationTag) == false))
                {
                    if ((_first01SelectedStation != null) && (_first01SelectedStation.StationControlTagId.Equals(stationTag)))
                    {
                        // Selected the same station
                        return;
                    }
                    else if (_second02SelectedStation != null)
                    {
                        if ((_resetRouteSelectionDelgHandle != null) && (_resetRouteSelectionDelgHandle() == true))
                        {
                            System.Windows.Forms.Application.DoEvents();
                        }
                        else
                            return;
                    }
                    RouteElement destinationSttElem = _routingList1.Find(s => ((s.StationControlTagId != null) && (s.StationControlTagId.Equals(stationTag))));

                    if (destinationSttElem == null)
                        destinationSttElem = _routingList2.Find(s => ((s.StationControlTagId != null) && (s.StationControlTagId.Equals(stationTag))));

                    if (destinationSttElem == null)
                        destinationSttElem = _routingList3.Find(s => ((s.StationControlTagId != null) && (s.StationControlTagId.Equals(stationTag))));

                    if (destinationSttElem != null)
                    {
                        _second02SelectedStation = destinationSttElem;
                        HighLightingStationSelection(bd);
                        ShowSelectedTripPath();
                        RaiseOnStationSelectChanged();
                    }
                    System.Windows.Forms.Application.DoEvents();
                }
            }
            catch(Exception ex)
            {
                App.Log.LogError(_logChannel, "", new Exception("(EXIT10001102)", ex), "EX01", "RouteSelector.Station_MouseLeftButtonUp");
            }

            void RaiseOnStationSelectChanged()
            {
                try
                {
                    if (OnStationSelectChanged != null)
                    {
                        OnStationSelectChanged.Invoke(null, new StationSelectedEventArgs(_second02SelectedStation.StationId, _second02SelectedStation.StationName));
                    }
                }
                catch (Exception ex)
                {
                    App.Log.LogError(_logChannel, "", new Exception("Unhandle error exception; (EXIT10001101)", ex), "EX01", "RouteSelector.RaiseOnStationSelectChanged");
                }
                
            }
        }

        public void LoadSelector(string originStationId)
        {
            ResetSelector();

            RouteElement oriSttElem = _routingList1.Find(e => ((e.StationId != null) && ((e.StationId.Equals(originStationId)))));

            if (oriSttElem == null)
                oriSttElem = _routingList2.Find(e => ((e.StationId != null) && (e.StationId.Equals(originStationId))));

            if (oriSttElem == null)
                oriSttElem = _routingList3.Find(e => ((e.StationId != null) && (e.StationId.Equals(originStationId))));

            if (oriSttElem != null)
            {
                _first01SelectedStation = oriSttElem;
                HighLightingStationSelection(oriSttElem.StationBorderCtrl);
            }
            System.Windows.Forms.Application.DoEvents();
        }

        private bool ShowSelectedTripPath()
        {
            if ((_second02SelectedStation != null) && (_first01SelectedStation != null))
            {
                _selectedRoutingList = null;

                if ((_routingList1.Find(s => ((s.StationId != null) && (s.StationId.Equals(_first01SelectedStation.StationId)))) is RouteElement) 
                    && (_routingList1.Find(s => ((s.StationId != null) && (s.StationId.Equals(_second02SelectedStation.StationId)))) is RouteElement))
                {
                    _selectedRoutingList = _routingList1;
                    HighLightingTripPath(_selectedRoutingList, _first01SelectedStation, _second02SelectedStation);
                    return true;
                }
                else if ((_routingList2.Find(s => ((s.StationId != null) && (s.StationId.Equals(_first01SelectedStation.StationId)))) is RouteElement)
                    && (_routingList2.Find(s => ((s.StationId != null) && (s.StationId.Equals(_second02SelectedStation.StationId)))) is RouteElement))
                {
                    _selectedRoutingList = _routingList2;
                    HighLightingTripPath(_selectedRoutingList, _first01SelectedStation, _second02SelectedStation);
                    return true;
                }
                else if ((_routingList3.Find(s => ((s.StationId != null) && (s.StationId.Equals(_first01SelectedStation.StationId)))) is RouteElement)
                    && (_routingList3.Find(s => ((s.StationId != null) && (s.StationId.Equals(_second02SelectedStation.StationId)))) is RouteElement))
                {
                    _selectedRoutingList = _routingList3;
                    HighLightingTripPath(_selectedRoutingList, _first01SelectedStation, _second02SelectedStation);
                    return true;
                }
            }
            return false;

            //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            void HighLightingTripPath(List<RouteElement> selectedRoutingList, RouteElement originElem, RouteElement DestElem)
            {
                RouteElement[] elememtPathList = null;

                RouteElement originElemCurr = selectedRoutingList.Find(s => ((s.StationControlTagId != null) && s.StationControlTagId.Equals(originElem.StationControlTagId)));
                RouteElement destElemCurr = selectedRoutingList.Find(s => ((s.StationControlTagId != null) && s.StationControlTagId.Equals(DestElem.StationControlTagId)));

                if (originElemCurr.RouteIndex < destElemCurr.RouteIndex)
                {
                    elememtPathList = (from elem in selectedRoutingList
                                       where ((elem.RouteIndex >= originElemCurr.RouteIndex) && (elem.RouteIndex <= destElemCurr.RouteIndex) && (elem.ElementType == KomuterRouteElementType.HighLightingLine))
                                       select elem).ToArray();
                }
                else
                {
                    elememtPathList = (from elem in selectedRoutingList
                                       where ((elem.RouteIndex >= destElemCurr.RouteIndex) && (elem.RouteIndex <= originElemCurr.RouteIndex) && (elem.ElementType == KomuterRouteElementType.HighLightingLine))
                                       select elem).ToArray();
                }

                if (elememtPathList?.Length > 0)
                    foreach (RouteElement elem in elememtPathList)
                        ShowSelectionLine(elem.HighLightingLine);
            }
        }

        private void HighLightingStationSelection(Border bdStation)
        {
            if (bdStation != null)
            {
                bdStation.Effect = _stationSelectedEffect;
                bdStation.BorderThickness = _selectedBorderTickness;
                bdStation.BorderBrush = null;
                bdStation.Background = _selectedBackgroundColor;
            }
        }

        private void UnselectStation(RouteElement stationElement)
        {
            if (stationElement != null)
            {
                stationElement.StationBorderCtrl.Effect = null;
                stationElement.StationBorderCtrl.BorderThickness = _normalBorderTickness;
                stationElement.StationBorderCtrl.BorderBrush = _normalBorderBrushColor;
                stationElement.StationBorderCtrl.Background = _unselectedBackgroundColor;
            }
        }

        private void ShowSelectionLine(Border bdLine)
        {
            bdLine.Visibility = System.Windows.Visibility.Visible;
        }

        private void HideSelectionLine(Border bdLine)
        {
            bdLine.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void ResetSelector()
        {
            _first01SelectedStation = null;
            _second02SelectedStation = null;

            foreach (RouteElement rEle1 in _routingList1)
                if (rEle1.ElementType == KomuterRouteElementType.Station)
                {
                    rEle1.IsSelected = false;
                    UnselectStation(rEle1);
                }
                else if (rEle1.ElementType == KomuterRouteElementType.HighLightingLine)
                {
                    HideSelectionLine(rEle1.HighLightingLine);
                }

            foreach (RouteElement rEle2 in _routingList2)
                if (rEle2.ElementType == KomuterRouteElementType.Station)
                {
                    rEle2.IsSelected = false;
                    UnselectStation(rEle2);
                }
                else if (rEle2.ElementType == KomuterRouteElementType.HighLightingLine)
                {
                    HideSelectionLine(rEle2.HighLightingLine);
                }

            foreach (RouteElement rEle3 in _routingList3)
                if (rEle3.ElementType == KomuterRouteElementType.Station)
                {
                    rEle3.IsSelected = false;
                    UnselectStation(rEle3);
                }
                else if (rEle3.ElementType == KomuterRouteElementType.HighLightingLine)
                {
                    HideSelectionLine(rEle3.HighLightingLine);
                }
        }

        private void CreateRoutingList()
        {
            int rIndex = -1;
            _routingList1.Add(new RouteElement(++rIndex, _mapPage.BdPadangBesar, @"Padang Besar", "47300"));
            _routingList1.Add(new RouteElement(++rIndex, _mapPage.BdPadangBesarHiLi1));
            _routingList1.Add(new RouteElement(++rIndex, _mapPage.BdBukitKetriHiLi1));
            _routingList1.Add(new RouteElement(++rIndex, _mapPage.BdBukitKetri, @"Bukit Ketri", "46300"));
            _routingList1.Add(new RouteElement(++rIndex, _mapPage.BdBukitKetriHiLi2));
            _routingList1.Add(new RouteElement(++rIndex, _mapPage.BdArauHiLi1));
            _routingList1.Add(new RouteElement(++rIndex, _mapPage.BdArau, @"Arau", "45800"));
            _routingList1.Add(new RouteElement(++rIndex, _mapPage.BdArauHiLi2));
            _routingList1.Add(new RouteElement(++rIndex, _mapPage.BdKodiangHiLi1));
            _routingList1.Add(new RouteElement(++rIndex, _mapPage.BdKodiang, @"Kodiang", "45600"));
            _routingList1.Add(new RouteElement(++rIndex, _mapPage.BdKodiangHiLi2));
            _routingList1.Add(new RouteElement(++rIndex, _mapPage.BdAnakBukitHiLi1));
            _routingList1.Add(new RouteElement(++rIndex, _mapPage.BdAnakBukit, @"Anak Bukit", "44400"));
            _routingList1.Add(new RouteElement(++rIndex, _mapPage.BdAnakBukitHiLi2));
            _routingList1.Add(new RouteElement(++rIndex, _mapPage.BdAlorSetarHiLi1));
            _routingList1.Add(new RouteElement(++rIndex, _mapPage.BdAlorSetar, @"Alor Setar", "44000"));
            _routingList1.Add(new RouteElement(++rIndex, _mapPage.BdAlorSetarHiLi2));
            _routingList1.Add(new RouteElement(++rIndex, _mapPage.BdKobahHiLi1));
            _routingList1.Add(new RouteElement(++rIndex, _mapPage.BdKobah, @"Kobah", "43100"));
            _routingList1.Add(new RouteElement(++rIndex, _mapPage.BdKobahHiLi2));
            _routingList1.Add(new RouteElement(++rIndex, _mapPage.BdGurunHiLi1));
            _routingList1.Add(new RouteElement(++rIndex, _mapPage.BdGurun, @"Gurun", "42400"));
            _routingList1.Add(new RouteElement(++rIndex, _mapPage.BdGurunHiLi2));
            _routingList1.Add(new RouteElement(++rIndex, _mapPage.BdSungaiPetaniHiLi1));
            _routingList1.Add(new RouteElement(++rIndex, _mapPage.BdSungaiPetani, @"Sungai Petani", "41400"));
            _routingList1.Add(new RouteElement(++rIndex, _mapPage.BdSungaiPetaniHiLi2));
            _routingList1.Add(new RouteElement(++rIndex, _mapPage.BdTasekGelugorHiLi1));
            _routingList1.Add(new RouteElement(++rIndex, _mapPage.BdTasekGelugor, @"Tasek Gelugor", "40500"));
            _routingList1.Add(new RouteElement(++rIndex, _mapPage.BdTasekGelugorHiLi2));
            _routingList1.Add(new RouteElement(++rIndex, _mapPage.BdBukitMertajamHiLi1));
            _routingList1.Add(new RouteElement(++rIndex, _mapPage.BdBukitMertajam, @"Bukit Mertajam", "600"));
            _routingList1.Add(new RouteElement(++rIndex, _mapPage.BdBukitMertajamHiLi2));
            _routingList1.Add(new RouteElement(++rIndex, _mapPage.BdSimpangAmpatHiLi1));
            _routingList1.Add(new RouteElement(++rIndex, _mapPage.BdSimpangAmpat, @"Simpang Ampat", "1000"));
            _routingList1.Add(new RouteElement(++rIndex, _mapPage.BdSimpangAmpatHiLi2));
            _routingList1.Add(new RouteElement(++rIndex, _mapPage.BdNibongTebalHiLi1));
            _routingList1.Add(new RouteElement(++rIndex, _mapPage.BdNibongTebal, @"Nibong Tebal", "1700"));
            _routingList1.Add(new RouteElement(++rIndex, _mapPage.BdNibongTebalHiLi2));
            _routingList1.Add(new RouteElement(++rIndex, _mapPage.BdParitBuntarHiLi1));
            _routingList1.Add(new RouteElement(++rIndex, _mapPage.BdParitBuntar, @"Parit Buntar", "1900"));
            _routingList1.Add(new RouteElement(++rIndex, _mapPage.BdParitBuntarHiLi2));
            _routingList1.Add(new RouteElement(++rIndex, _mapPage.BdBaganSeraiHiLi1));
            _routingList1.Add(new RouteElement(++rIndex, _mapPage.BdBaganSerai, @"Bagan Serai", "2600"));
            _routingList1.Add(new RouteElement(++rIndex, _mapPage.BdBaganSeraiHiLi2));
            _routingList1.Add(new RouteElement(++rIndex, _mapPage.BdKamuntingHiLi1));
            _routingList1.Add(new RouteElement(++rIndex, _mapPage.BdKamunting, @"Kamunting", "4500"));
            _routingList1.Add(new RouteElement(++rIndex, _mapPage.BdKamuntingHiLi2));
            _routingList1.Add(new RouteElement(++rIndex, _mapPage.BdTaipingHiLi1));
            _routingList1.Add(new RouteElement(++rIndex, _mapPage.BdTaiping, @"Taiping", "4700"));
            _routingList1.Add(new RouteElement(++rIndex, _mapPage.BdTaipingHiLi2));
            _routingList1.Add(new RouteElement(++rIndex, _mapPage.BdPadangRengasHiLi1));
            _routingList1.Add(new RouteElement(++rIndex, _mapPage.BdPadangRengas, @"Padang Rengas", "5700"));

            rIndex = -1;
            _routingList2.Add(new RouteElement(++rIndex, _mapPage.BdPadangBesar, @"Padang Besar", "47300"));
            _routingList2.Add(new RouteElement(++rIndex, _mapPage.BdPadangBesarHiLi1));
            _routingList2.Add(new RouteElement(++rIndex, _mapPage.BdBukitKetriHiLi1));
            _routingList2.Add(new RouteElement(++rIndex, _mapPage.BdBukitKetri, @"Bukit Ketri", "46300"));
            _routingList2.Add(new RouteElement(++rIndex, _mapPage.BdBukitKetriHiLi2));
            _routingList2.Add(new RouteElement(++rIndex, _mapPage.BdArauHiLi1));
            _routingList2.Add(new RouteElement(++rIndex, _mapPage.BdArau, @"Arau", "45800"));
            _routingList2.Add(new RouteElement(++rIndex, _mapPage.BdArauHiLi2));
            _routingList2.Add(new RouteElement(++rIndex, _mapPage.BdKodiangHiLi1));
            _routingList2.Add(new RouteElement(++rIndex, _mapPage.BdKodiang, @"Kodiang", "45600"));
            _routingList2.Add(new RouteElement(++rIndex, _mapPage.BdKodiangHiLi2));
            _routingList2.Add(new RouteElement(++rIndex, _mapPage.BdAnakBukitHiLi1));
            _routingList2.Add(new RouteElement(++rIndex, _mapPage.BdAnakBukit, @"Anak Bukit", "44400"));
            _routingList2.Add(new RouteElement(++rIndex, _mapPage.BdAnakBukitHiLi2));
            _routingList2.Add(new RouteElement(++rIndex, _mapPage.BdAlorSetarHiLi1));
            _routingList2.Add(new RouteElement(++rIndex, _mapPage.BdAlorSetar, @"Alor Setar", "44000"));
            _routingList2.Add(new RouteElement(++rIndex, _mapPage.BdAlorSetarHiLi2));
            _routingList2.Add(new RouteElement(++rIndex, _mapPage.BdKobahHiLi1));
            _routingList2.Add(new RouteElement(++rIndex, _mapPage.BdKobah, @"Kobah", "43100"));
            _routingList2.Add(new RouteElement(++rIndex, _mapPage.BdKobahHiLi2));
            _routingList2.Add(new RouteElement(++rIndex, _mapPage.BdGurunHiLi1));
            _routingList2.Add(new RouteElement(++rIndex, _mapPage.BdGurun, @"Gurun", "42400"));
            _routingList2.Add(new RouteElement(++rIndex, _mapPage.BdGurunHiLi2));
            _routingList2.Add(new RouteElement(++rIndex, _mapPage.BdSungaiPetaniHiLi1));
            _routingList2.Add(new RouteElement(++rIndex, _mapPage.BdSungaiPetani, @"Sungai Petani", "41400"));
            _routingList2.Add(new RouteElement(++rIndex, _mapPage.BdSungaiPetaniHiLi2));
            _routingList2.Add(new RouteElement(++rIndex, _mapPage.BdTasekGelugorHiLi1));
            _routingList2.Add(new RouteElement(++rIndex, _mapPage.BdTasekGelugor, @"Tasek Gelugor", "40500"));
            _routingList2.Add(new RouteElement(++rIndex, _mapPage.BdTasekGelugorHiLi2));
            _routingList2.Add(new RouteElement(++rIndex, _mapPage.BdBukitMertajamHiLi1));
            _routingList2.Add(new RouteElement(++rIndex, _mapPage.BdBukitMertajam, @"Bukit Mertajam", "600"));
            _routingList2.Add(new RouteElement(++rIndex, _mapPage.BdBukitMertajamHiLi3));
            _routingList2.Add(new RouteElement(++rIndex, _mapPage.BdBukitMertajamHiLi4));
            _routingList2.Add(new RouteElement(++rIndex, _mapPage.BdBukitTengahHiLi2));
            _routingList2.Add(new RouteElement(++rIndex, _mapPage.BdBukitTengah, @"Bukit Tengah", "400"));
            _routingList2.Add(new RouteElement(++rIndex, _mapPage.BdBukitTengahHiLi1));
            _routingList2.Add(new RouteElement(++rIndex, _mapPage.BdButterworthHiLi1));
            _routingList2.Add(new RouteElement(++rIndex, _mapPage.BdButterworth, @"Butterworth", "100"));

            rIndex = -1;
            _routingList3.Add(new RouteElement(++rIndex, _mapPage.BdButterworth, @"Butterworth", "100"));
            _routingList3.Add(new RouteElement(++rIndex, _mapPage.BdButterworthHiLi1));
            _routingList3.Add(new RouteElement(++rIndex, _mapPage.BdBukitTengahHiLi1));
            _routingList3.Add(new RouteElement(++rIndex, _mapPage.BdBukitTengah, @"Bukit Tengah", "400"));
            _routingList3.Add(new RouteElement(++rIndex, _mapPage.BdBukitTengahHiLi2));
            _routingList3.Add(new RouteElement(++rIndex, _mapPage.BdBukitMertajamHiLi4));
            _routingList3.Add(new RouteElement(++rIndex, _mapPage.BdBukitMertajamHiLi3));
            _routingList3.Add(new RouteElement(++rIndex, _mapPage.BdBukitMertajam, @"Bukit Mertajam", "600"));
            _routingList3.Add(new RouteElement(++rIndex, _mapPage.BdBukitMertajamHiLi2));
            _routingList3.Add(new RouteElement(++rIndex, _mapPage.BdSimpangAmpatHiLi1));
            _routingList3.Add(new RouteElement(++rIndex, _mapPage.BdSimpangAmpat, @"Simpang Ampat", "1000"));
            _routingList3.Add(new RouteElement(++rIndex, _mapPage.BdSimpangAmpatHiLi2));
            _routingList3.Add(new RouteElement(++rIndex, _mapPage.BdNibongTebalHiLi1));
            _routingList3.Add(new RouteElement(++rIndex, _mapPage.BdNibongTebal, @"Nibong Tebal", "1700"));
            _routingList3.Add(new RouteElement(++rIndex, _mapPage.BdNibongTebalHiLi2));
            _routingList3.Add(new RouteElement(++rIndex, _mapPage.BdParitBuntarHiLi1));
            _routingList3.Add(new RouteElement(++rIndex, _mapPage.BdParitBuntar, @"Parit Buntar", "1900"));
            _routingList3.Add(new RouteElement(++rIndex, _mapPage.BdParitBuntarHiLi2));
            _routingList3.Add(new RouteElement(++rIndex, _mapPage.BdBaganSeraiHiLi1));
            _routingList3.Add(new RouteElement(++rIndex, _mapPage.BdBaganSerai, @"Bagan Serai", "2600"));
            _routingList3.Add(new RouteElement(++rIndex, _mapPage.BdBaganSeraiHiLi2));
            _routingList3.Add(new RouteElement(++rIndex, _mapPage.BdKamuntingHiLi1));
            _routingList3.Add(new RouteElement(++rIndex, _mapPage.BdKamunting, @"Kamunting", "4500"));
            _routingList3.Add(new RouteElement(++rIndex, _mapPage.BdKamuntingHiLi2));
            _routingList3.Add(new RouteElement(++rIndex, _mapPage.BdTaipingHiLi1));
            _routingList3.Add(new RouteElement(++rIndex, _mapPage.BdTaiping, @"Taiping", "4700"));
            _routingList3.Add(new RouteElement(++rIndex, _mapPage.BdTaipingHiLi2));
            _routingList3.Add(new RouteElement(++rIndex, _mapPage.BdPadangRengasHiLi1));
            _routingList3.Add(new RouteElement(++rIndex, _mapPage.BdPadangRengas, @"Padang Rengas", "5700"));

            foreach (RouteElement rEle1 in _routingList1)
                if (rEle1.ElementType == KomuterRouteElementType.Station)
                    rEle1.StationBorderCtrl.MouseLeftButtonUp += Station_MouseLeftButtonUp;

            _mapPage.BdButterworth.MouseLeftButtonUp += Station_MouseLeftButtonUp;
            _mapPage.BdBukitTengah.MouseLeftButtonUp += Station_MouseLeftButtonUp;
        }
    }
}
