using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using System.Threading;
using System.Runtime.InteropServices;

namespace NssIT.Kiosk.Device.PosiFlex.MotionSensor1.OrgAPI
{

    public class PosiFlexMotionSensorDLLWrapper
    {
        public event EventHandler<SensorMotionEventArgs> OnSensorMotion;
        public event EventHandler<NotificationEventArgs> OnNotifySensorMotion;

        public event EventHandler OnPresentCustomer;
        public event EventHandler OnAbsentCustomer;

        [DllImport("PosiFlexMotionSensorDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int setThresholdValue(string comPortName, int level);
        [DllImport("PosiFlexMotionSensorDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int getDeviceInfo(string comPortName, byte[] buffer);
        [DllImport("PosiFlexMotionSensorDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int getSensorValue(string comPortName, byte[] buffer);
        [DllImport("PosiFlexMotionSensorDLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int startAutoAdjustment(string comPortName);

        public (DateTime? StartPresentTime, DateTime? LastPresentTime) _presentPeriod = (StartPresentTime: null, LastPresentTime: null);
        public (DateTime? StartAbsentTime, DateTime? LastAbsentTime) _absentPeriod = (StartAbsentTime: null, LastAbsentTime: null);

        private String _comPortName; // Com Port
        private Int32 adjustment_time;
        private Int32 _level = 1;

        private Thread _motionDetectorThreadWorking = null;
        private bool _isMotionDetectorStart = false;

        public PosiFlexMotionSensorDLLWrapper(string COMPort, Int32 level)
        {
            _comPortName = COMPort;
            Level = level;
        }

        public Int32 Level
        {
            get => _level;
            set { 
                if ((value >= 1) && (value <= 5))
                {
                    _level = value;
                }
            }
        }

        public void StartMotionDetector()
        {
            int minPersonNearByRange = 18;
            int validPersonPresentMinPeriodSec = 1;
            int validPersonAbsentMinPeriodSec = 3;
            bool isCustomerPresent = false;

            // Abort Existing Thread
            if (_motionDetectorThreadWorking != null)
            {
                if (((_motionDetectorThreadWorking?.ThreadState & ThreadState.Aborted) == ThreadState.Aborted) ||
                    ((_motionDetectorThreadWorking?.ThreadState & ThreadState.Stopped) == ThreadState.Stopped) ||
                    ((_motionDetectorThreadWorking?.ThreadState & ThreadState.Unstarted) == ThreadState.Unstarted))
                { /*By Pass*/ }
                else
                {
                    try
                    {
                        _motionDetectorThreadWorking?.Abort();
                    }
                    catch { }

                    Thread.Sleep(1500);
                }
            }

            NotifyMessage($@"StartMotionDetector - Prepare to start");
            _motionDetectorThreadWorking = new Thread(new ThreadStart(MotionDetectorThreadWorking));
            _motionDetectorThreadWorking.SetApartmentState(ApartmentState.STA);
            _motionDetectorThreadWorking.IsBackground = true;
            _motionDetectorThreadWorking.Start();

            NotifyMessage($@"StartMotionDetector - started");

            Thread.Sleep(100);


            return;

            void MotionDetectorThreadWorking()
            {
                try
                {
                    _isMotionDetectorStart = true;

                    NotifyMessage($@"MotionDetectorThreadWorking - Starting setThresholdValue(..)");

                    setThresholdValue(this._comPortName, Level + 1);
                    Thread.Sleep(100);

                    NotifyMessage($@"MotionDetectorThreadWorking - End setThresholdValue(..)");

                    NotifyMessage($@"_isMotionDetectorStart : {_isMotionDetectorStart}");

                    if ((_motionDetectorThreadWorking != null))
                        NotifyMessage($@"_motionDetectorThreadWorking != null");
                    else
                        NotifyMessage($@"_motionDetectorThreadWorking is null");

                    if ((_isMotionDetectorStart) && (_motionDetectorThreadWorking != null))
                        NotifyMessage($@"Valid to loop");
                    else
                        NotifyMessage($@"NOT Valid to loop");

                    _presentPeriod.StartPresentTime = null;
                    _presentPeriod.LastPresentTime = null;
                    _absentPeriod.StartAbsentTime = null;
                    _absentPeriod.LastAbsentTime = null;

                    while ((_isMotionDetectorStart) && (_motionDetectorThreadWorking != null))
                    {
                        try
                        {
                            NotifyMessage($@"updateMotionSensorValue - start");

                            updateMotionSensorValue();

                            NotifyMessage($@"updateMotionSensorValue - end");
                        }
                        catch (Exception ex)
                        {
                            NotifyMessage($@"updateMotionSensorValue - Error (EX01): {ex.ToString()}");
                        }

                        if (_isMotionDetectorStart)
                            Thread.Sleep(350);
                    }
                }
                catch (Exception ex)
                {
                    NotifyMessage($@"MotionDetectorThreadWorking - Error (EX02): {ex.ToString()}");
                }

                // Quit
                if (_isMotionDetectorStart == false)
                    _motionDetectorThreadWorking = null;

                NotifyMessage($@"MotionDetectorThreadWorking - Quit");
            }

            void updateMotionSensorValue()
            {
                Byte[] cmdData = new Byte[16];
                Boolean dataStart = false;
                Int32 dataStartLocation = 0;
                Int32 sensorValue = 0;

                NotifyMessage($@"getSensorValue - Start; ");

                getSensorValue(this._comPortName, cmdData);

                NotifyMessage($@"getSensorValue - End; cmdData length  : -{cmdData?.Length}- ");

                for (int i = 0; i < 6; i++)
                {
                    if (cmdData?.Length >= (i + 1))
                    {
                        //NotifyMessage($@"updateMotionSensorValue (A1); i:{i}; cmdData[i] : {cmdData[i]}");
                    }

                    if (dataStart)
                    {
                        //NotifyMessage($@"updateMotionSensorValue (A2);");

                        if (cmdData[i] == 0x0D && cmdData[i + 1] == 0x0A)
                        {
                            //NotifyMessage($@"updateMotionSensorValue (A1); ");
                            break;
                        }
                    }
                    if (cmdData[i] == 0xB0)
                    {
                        //NotifyMessage($@"updateMotionSensorValue (A2); i:{i}; cmdData[i] == 0xB0 - Start");
                        dataStart = true;
                        dataStartLocation = i;
                        //NotifyMessage($@"updateMotionSensorValue (A3); i:{i}; cmdData[i] == 0xB0 - End");
                    }
                }

                sensorValue = ((Int32.Parse(cmdData[dataStartLocation + 3].ToString()) * 256) + Int32.Parse(cmdData[dataStartLocation + 2].ToString()));

                bool isTriggerAttention = false;
                bool isSomeOneNearBy = false;
                if (sensorValue >= minPersonNearByRange)
                    isSomeOneNearBy = true;

                if (isSomeOneNearBy)
                {
                    if (_presentPeriod.StartPresentTime.HasValue == false)
                        _presentPeriod.StartPresentTime = DateTime.Now;
                    else
                    {
                        _presentPeriod.LastPresentTime = DateTime.Now;

                        if ((isCustomerPresent == false) 
                            && (_presentPeriod.LastPresentTime.Value.Subtract(_presentPeriod.StartPresentTime.Value).TotalSeconds >= validPersonPresentMinPeriodSec))
                        {
                            isTriggerAttention = true;
                        }
                    }
                    _absentPeriod.StartAbsentTime = null;
                    _absentPeriod.LastAbsentTime = null;
                }
                else
                {
                    if (_absentPeriod.StartAbsentTime.HasValue == false)
                        _absentPeriod.StartAbsentTime = DateTime.Now;
                    else
                    {
                        _absentPeriod.LastAbsentTime = DateTime.Now;

                        if ((isCustomerPresent == true)
                            && (_absentPeriod.LastAbsentTime.Value.Subtract(_absentPeriod.StartAbsentTime.Value).TotalSeconds >= validPersonAbsentMinPeriodSec))
                        {
                            isTriggerAttention = true;
                        }
                    }
                    _presentPeriod.StartPresentTime = null;
                    _presentPeriod.LastPresentTime = null;
                }

                try
                {
                    OnSensorMotion?.Invoke(null, new SensorMotionEventArgs(sensorValue, isSomeOneNearBy));

                    if (isTriggerAttention)
                    {
                        if (_presentPeriod.LastPresentTime.HasValue)
                        {
                            try
                            {
                                isCustomerPresent = true;
                                OnPresentCustomer?.Invoke(null, new EventArgs());
                            }
                            catch (Exception ex)
                            { }
                        }
                        else if (_absentPeriod.LastAbsentTime.HasValue)
                        {
                            try
                            {
                                isCustomerPresent = false;
                                OnAbsentCustomer?.Invoke(null, new EventArgs());
                            }
                            catch (Exception ex)
                            { }
                        }
                    }
                }
                catch(Exception ex)
                {
                    NotifyMessage($@"updateMotionSensorValue - Error (EX101) : {ex.ToString()}");
                }

                ////// Consider some person/body nearby
                ////if (cmdData[dataStartLocation + 1] == 0x43)
                ////{
                ////    //NotifyMessage("updateMotionSensorValue -> cmdData[dataStartLocation + 1] == 0x43 ");
                ////}

                ////// Consider No person/body nearby
                ////else if (cmdData[dataStartLocation + 1] == 0x41)
                ////{
                ////    //NotifyMessage("updateMotionSensorValue -> cmdData[dataStartLocation + 1] == 0x41 ");
                ////}
                //////NotifyMessage($@"END - updateMotionSensorValue ; sensorValue : {sensorValue}");
            }
        }

        public bool IsMotionDetectorStarted
        {
            get
            {
                if (_motionDetectorThreadWorking != null)
                {
                    if ((_motionDetectorThreadWorking?.ThreadState == ThreadState.Running) ||
                        ((_motionDetectorThreadWorking?.ThreadState & ThreadState.Background) == ThreadState.Background))
                    { 
                        return true; 
                    }
                }
                return false;
            }
        }

        public void StopMotionDetector()
        {
            _isMotionDetectorStart = false;
            Thread.Sleep(700);

            // Abort Existing Thread
            if (_motionDetectorThreadWorking != null)
            {
                if (((_motionDetectorThreadWorking?.ThreadState & ThreadState.Aborted) == ThreadState.Aborted) ||
                    ((_motionDetectorThreadWorking?.ThreadState & ThreadState.Stopped) == ThreadState.Stopped) ||
                    ((_motionDetectorThreadWorking?.ThreadState & ThreadState.Unstarted) == ThreadState.Unstarted))
                { /*By Pass*/ }
                else
                {
                    try
                    {
                        _motionDetectorThreadWorking.Abort();
                    }
                    catch { }

                    Thread.Sleep(1000);
                }
            }
        }

        private void NotifyMessage(string message)
        {
            try
            {
                OnNotifySensorMotion?.Invoke(null, new NotificationEventArgs(message));
            }
            catch (Exception ex)
            { }
        }

    }
}
