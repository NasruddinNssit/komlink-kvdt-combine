using NssIT.Kiosk.AppDecorator.Log;
using NssIT.Kiosk.Device.PosiFlex.StatusIndicator1.OrgAPI;
using NssIT.Kiosk.Log.DB;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Device.PosiFlex.StatusIndicator1.AccessSDK
{
    public class StatusIndicator1Access : IDisposable
    {
		private const string LogChannel = "Status_Indicator_ACCESS";
		private DbLog _log = null;

		private Thread _indicatorThreadWorker = null;
		private TimeSpan _MaxWaitPeriod = new TimeSpan(0, 0, 1);
		private ConcurrentQueue<IndicatorExecParam> _lightParamList = new ConcurrentQueue<IndicatorExecParam>();
		
		private string _comPort = "COM1";
		private bool _disposed = false;

		private static SemaphoreSlim _manLock = new SemaphoreSlim(1);
		private static StatusIndicator1Access _indicatorAccess = null;

		private ShowMessageLogDelg _showMessageLogHandle = null;

		public DbLog Log
		{
			get
			{
				return _log ?? (_log = DbLog.GetDbLog());
			}
		}

		public static StatusIndicator1Access GetStatusIndicator(string comPort)
		{
			if (_indicatorAccess == null)
			{
				try
				{
					_manLock.WaitAsync().Wait();
					if (_indicatorAccess == null)
					{
						_indicatorAccess = new StatusIndicator1Access(comPort);
					}
					return _indicatorAccess;
				}
				finally
				{
					if (_manLock.CurrentCount == 0)
						_manLock.Release();
				}
			}
			else
				return _indicatorAccess;
		}

		public static void AssignShowMessageLogHandle(ShowMessageLogDelg showMessageLogHandle)
        {
			if (_indicatorAccess != null)
            {
				_indicatorAccess._showMessageLogHandle = showMessageLogHandle;
			}
		}

		private StatusIndicator1Access(string comPort)
		{
			_comPort = comPort;
			Init();
		}

		private void Init()
		{
			try
			{
				_indicatorThreadWorker = new Thread(AccessExecutionThreadWorking);
				_indicatorThreadWorker.SetApartmentState(ApartmentState.STA);
				_indicatorThreadWorker.IsBackground = true;
				_indicatorThreadWorker.Start();
			}
			catch (Exception ex)
			{
				string byPassMsg = ex.Message;
			}

			/////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

			void AccessExecutionThreadWorking()
            {
                try
                {
					using (StatusTowerIndicator statusTowerIndicator = new StatusTowerIndicator(_comPort))
					{
						while (!_disposed)
						{
							if (GetExecParam() is IndicatorExecParam param)
							{
								Guid exId = Guid.NewGuid();
								if (_disposed)
									break;
								try
								{
									statusTowerIndicator.ShowLight(param.ColorBytes);
									statusTowerIndicator.ShowLight(param.ColorBytes);
								}
								catch (Exception ex)
								{
									Log.LogError(LogChannel, exId.ToString(), ex, "EX01", "StatusIndicator1Access.AccessExecutionThreadWorking");
								}
							}
						}

						Log.LogText(LogChannel, "*", "Quit AccessExecutionThreadWorking", "A20", "StatusIndicator1Access.AccessExecutionThreadWorking");
						Thread.Sleep(300);
					}
				}
				catch (Exception ex)
				{
					Log.LogError(LogChannel, "*", ex, "EX10", "StatusIndicator1Access.AccessExecutionThreadWorking");
				}
			}
			
			IndicatorExecParam GetExecParam()
			{
				IndicatorExecParam retLogInfo = null;
				bool logFound = false;

				if (_disposed == false)
				{
					try
					{
						lock (_lightParamList)
						{
							if (_lightParamList.Count == 0)
							{
								Monitor.Wait(_lightParamList, _MaxWaitPeriod);
							}

							logFound = _lightParamList.TryDequeue(out retLogInfo);
						}
					}
					// Used to handle "_lightParamList" is null after disposed
					catch (Exception ex) { string byPassStr = ex.Message; }
				}

				if (logFound)
					return retLogInfo;
				else
					return null;
			}
		}

		public void Dispose()
        {
			if (!_disposed)
            {
				_disposed = true;

				lock (_lightParamList)
				{
					Log.LogText(LogChannel, "*", "Start - Dispose", "A01", "StatusIndicator1Access.Dispose");

					try
					{
						while (_lightParamList.Count > 0)
							_lightParamList.TryDequeue(out IndicatorExecParam x);

						Monitor.PulseAll(_lightParamList);
						Thread.Sleep(300);
					}
					catch { }

					Log.LogText(LogChannel, "*", "End - Dispose", "A10", "StatusIndicator1Access.Dispose");
				}
			}
			
		}

		public void ShowColor(IndicatorColor color, LightMode lightMode = LightMode.Normal)
        {
			Thread threadWorker = new Thread(new ThreadStart(new Action(() => {

				string colorDesc = "";
				byte[] colorBytes = null;

				switch (color)
                {
					case IndicatorColor.Red:
						colorBytes = StatusTowerIndicator.ColorBytes_Red;
						colorDesc = "Red";
						break;

					case IndicatorColor.Green:
						colorBytes = StatusTowerIndicator.ColorBytes_Green;
						colorDesc = "Green";
						break;

					case IndicatorColor.Blue:
						colorBytes = StatusTowerIndicator.ColorBytes_Blue;
						colorDesc = "Blue";
						break;

					case IndicatorColor.Aqua :
						colorBytes = StatusTowerIndicator.ColorBytes_Aqua;
						colorDesc = "Aqua";
						break;

					case IndicatorColor.Magenta:
						colorBytes = StatusTowerIndicator.ColorBytes_Magenta;
						colorDesc = "Magenta";
						break;

					case IndicatorColor.Yellow:
						colorBytes = StatusTowerIndicator.ColorBytes_Yellow;
						colorDesc = "Yellow";
						break;

					default:
						colorBytes = StatusTowerIndicator.ColorBytes_White;
						colorDesc = "White";
						break;
				}

				if (colorBytes?.Length > 0)
				{
					if (lightMode == LightMode.Blinking)
					{
						byte[] blinkingColorBytes = new byte[colorBytes.Length];
						Array.Copy(colorBytes, blinkingColorBytes, colorBytes.Length);
						blinkingColorBytes[10] = StatusTowerIndicator.ColorByte_Blink;
						colorBytes = blinkingColorBytes;
						colorDesc += "-Blinking";
					}

					lock (_lightParamList)
                    {
						if (!_disposed)
                        {
							_lightParamList.Enqueue(new IndicatorExecParam(colorBytes, colorDesc));
							Monitor.PulseAll(_lightParamList);
						}						
					}
				}

			})));
			threadWorker.IsBackground = true;
			threadWorker.Start();
		}

		public void SwitchOff()
        {
			Thread threadWorker = new Thread(new ThreadStart(new Action(() => 
			{
				byte[] colorBytes = StatusTowerIndicator.ColorBytes_Stop;
				lock (_lightParamList)
				{
					if (!_disposed)
					{
						_lightParamList.Enqueue(new IndicatorExecParam(colorBytes, "SwitchOff"));
						Monitor.PulseAll(_lightParamList);
					}
				}
			})));
			threadWorker.IsBackground = true;
			threadWorker.Start();
		}

		public enum IndicatorColor
        {
			White = 0,

			Red = 1,
			Green = 2,
			Blue = 3,

			Aqua = 4,
			Yellow = 5,
			Magenta = 6
		}

		public enum LightMode
        {
			Normal = 0,
			Blinking = 1
        }

		class IndicatorExecParam
        {
			public byte[] ColorBytes { get; private set; }
			public string ColorDescription { get; private set; }

			public IndicatorExecParam(byte[] colorBytes, string colorDescription)
            {
				ColorBytes = colorBytes;
				ColorDescription = (string.IsNullOrEmpty(colorDescription)) ? "*" : colorDescription.Trim();
			}
		}
    }
}
