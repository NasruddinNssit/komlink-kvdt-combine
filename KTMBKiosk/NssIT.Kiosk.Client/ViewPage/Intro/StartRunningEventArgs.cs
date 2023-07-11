using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace NssIT.Kiosk.Client.ViewPage.Intro
{
    public class StartRunningEventArgs
    {
        public TransportGroup VehicleGroup { get; private set; } = TransportGroup.EtsIntercity;
        public Button CallerButton { get; private set; } = null;

        public StartRunningEventArgs(TransportGroup vehicleGroup, Button callerButton)
        {
            VehicleGroup = vehicleGroup;
            CallerButton = callerButton;
        }
    }
}