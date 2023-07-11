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

namespace NssIT.Kiosk.Client.ViewPage.Komuter
{
    public class RouteElement
    {
        public int RouteIndex { get; private set; }
        public Guid LocalElementId { get; private set; } = Guid.NewGuid();
        public Border StationBorderCtrl { get; private set; }
        public string StationName { get; private set; }
        /// <summary>
        /// Station Id Refer to database
        /// </summary>
        public string StationId { get; private set; }
        /// <summary>
        /// Tag Id thai is marked in Border control of Station
        /// </summary>
        public string StationControlTagId { get; private set; }
        public bool IsSelected { get; set; } = false;

        public KomuterRouteElementType ElementType { get; private set; }

        public Border HighLightingLine { get; private set; }

        /// <summary>
        /// Constructor for a Station Point
        /// </summary>
        /// <param name="routeIndex"></param>
        /// <param name="stationControl"></param>
        /// <param name="stationName"></param>
        /// <param name="stationId"></param>
        public RouteElement(int routeIndex, Border stationControl, string stationName, string stationId)
        {
            RouteIndex = routeIndex;
            StationBorderCtrl = stationControl;
            StationName = stationName;
            StationId = stationId;
            ElementType = KomuterRouteElementType.Station;

            if ((stationControl.Tag is string sTag) && (string.IsNullOrWhiteSpace(sTag) == false))
                StationControlTagId = sTag;
            else
                StationControlTagId = "*";
        }

        /// <summary>
        /// Constructor for HighLighting Line
        /// </summary>
        /// <param name="routeIndex"></param>
        /// <param name="highLightingLine"></param>
        public RouteElement(int routeIndex, Border highLightingLine)
        {
            RouteIndex = routeIndex;
            HighLightingLine = highLightingLine;
            ElementType = KomuterRouteElementType.HighLightingLine;
        }
    }
}
