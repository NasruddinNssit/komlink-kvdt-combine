using NssIT.Kiosk.AppDecorator.Log;
using NssIT.Kiosk.Device.PosiFlex.StatusIndicator1.AccessSDK;
using System;
using System.Collections.Generic;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NssIT.Kiosk.Device.PosiFlex.StatusIndicator1.AccessSDK.StatusIndicator1Access;

namespace NssIT.Kiosk.Client.Base.LocalDevices
{
    public class PosiFlexStatusIndicator : ITowerLight
    {
        private string _logChannel = "StatusIndicator";

        private string _comPort = "COM1";
        private StatusIndicator1Access _towerLight = null;

        public PosiFlexStatusIndicator(string comPort, ShowMessageLogDelg showMessageLogHandle)
        {
            if (string.IsNullOrWhiteSpace(comPort) == true)
                throw new Exception("Error Serial Port specification for PosiFlexStatusIndicator.");

            _comPort = comPort;

            _towerLight = StatusIndicator1Access.GetStatusIndicator(_comPort);
            StatusIndicator1Access.AssignShowMessageLogHandle(showMessageLogHandle);
        }

        public void Dispose()
        {
            if (_towerLight != null)
            {
                try
                {
                    _towerLight.Dispose();
                }
                catch { }

                _towerLight = null;
            }
        }

        public void ShowAvailableState() => ShowColorLight(StatusIndicator1Access.IndicatorColor.Green);

        public void ShowBusyState() => ShowColorLight(StatusIndicator1Access.IndicatorColor.Yellow);

        public void ShowErrorState() => ShowColorLight(StatusIndicator1Access.IndicatorColor.Red);

        public void ShowErrorStateWithBlinking() => ShowColorLight(StatusIndicator1Access.IndicatorColor.Red, LightMode.Blinking);

        public void SwitchOff()
        {
            if (_towerLight is null)
                return;

            try
            {
                _towerLight.SwitchOff();
            }
            catch (Exception ex)
            {
                App.Log?.LogError(_logChannel, "*", ex, "EX01", "PosiFlexStatusIndicator.SwitchOff");
            }
        }

        private void ShowColorLight(StatusIndicator1Access.IndicatorColor colorArrCode, LightMode lightMode = LightMode.Normal)
        {
            if (_towerLight is null)
                return;

            try
            {
                _towerLight.ShowColor(colorArrCode, lightMode);
            }
            catch (Exception ex)
            {
                string eMsg = $@"Error : {ex.Message}; Color : {Enum.GetName(typeof(StatusIndicator1Access.IndicatorColor), colorArrCode)}";
                App.Log?.LogError(_logChannel, "*", new Exception(eMsg, ex), "EX01", "PosiFlexStatusIndicator.ShowColorLight");
            }
        }
    }
}
