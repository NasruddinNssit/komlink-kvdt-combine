using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.Base.Time
{
    public class BookingTimeoutCounter : IDisposable 
    {
        private const string LogChannel = "ViewPage";

        private SemaphoreSlim _asyncLock = new SemaphoreSlim(1);

        private DateTime? _leadingBookingTimeout = null;
        private bool _hasTriggerNotification = false;
        private Thread _bookingTimeoutEstimatorThreadWorker = null;

        public BookingTimeoutCounter()
        {
            ResetCounter();

            _bookingTimeoutEstimatorThreadWorker = new Thread(new ThreadStart(BookingTimeoutEstimatorThreadWorking));
            _bookingTimeoutEstimatorThreadWorker.IsBackground = true;
            _bookingTimeoutEstimatorThreadWorker.Start();
        }

        private bool _disposed = false;
        public void Dispose()
        {
            _disposed = true;
        }

        public void ResetCounter()
        {
            try
            {
                _asyncLock.WaitAsync().Wait();

                _leadingBookingTimeout = null;
                _hasTriggerNotification = false;

                ///// App.ShowDebugMsg($@"BookingTimeoutCounter.ResetCounter; ## * ##");
            }
            catch (Exception ex)
            {
                App.Log.LogError(LogChannel, "*", ex, "EX01", "BookingTimeoutCounter.ResetCounter");
            }
            finally
            {
                if (_asyncLock.CurrentCount == 0)
                    _asyncLock.Release();
            }            
        }

        public void UpdateBookingTimeout(DateTime bookingTimeout)
        {
            try
            {
                _asyncLock.WaitAsync().Wait();

                int timeoutLeadingPeriodSec = 180;
                _leadingBookingTimeout = bookingTimeout.AddSeconds(timeoutLeadingPeriodSec * -1);

                App.ShowDebugMsg($@"BookingTimeoutCounter.UpdateBookingTimeout(); bookingTimeout : {bookingTimeout.ToString("HH:mm:ss")}; _leadingBookingTimeout : {_leadingBookingTimeout.Value.ToString("HH:mm:ss")}");
            }
            catch (Exception ex)
            {
                App.Log.LogError(LogChannel, "*", ex, "EX01", "BookingTimeoutCounter.UpdateEstimatedBookingTimeout");
            }
            finally
            {
                if (_asyncLock.CurrentCount == 0)
                    _asyncLock.Release();
            }
        }

        private void BookingTimeoutEstimatorThreadWorking()
        {
            while (!_disposed)
            {
                Thread.Sleep(1000);

                if ((_disposed == false) && (_hasTriggerNotification == false) && (_leadingBookingTimeout.HasValue == true))
                {
                    bool triggerUINotification = false;

                    try
                    {
                        _asyncLock.WaitAsync().Wait();

                        if (_leadingBookingTimeout.HasValue == true)
                        {
                            if (_leadingBookingTimeout.Value.Subtract(DateTime.Now).TotalSeconds <= 0)
                            {
                                triggerUINotification = true;
                                _hasTriggerNotification = true;

                                App.ShowDebugMsg($@"BookingTimeoutCounter.BookingTimeoutEstimatorThreadWorking(); Start Trigger");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        App.Log.LogError(LogChannel, "*", ex, "EX01", "BookingTimeoutCounter.BookingTimeoutEstimatorThreadWorking");
                    }
                    finally
                    {
                        if (_asyncLock.CurrentCount == 0)
                            _asyncLock.Release();

                        if (triggerUINotification)
                            ShowBookingTimeoutNotification();
                    }
                }
            }

            void ShowBookingTimeoutNotification()
            {
                bool? isSuccess = null;

                try
                {
                    if (App.MainScreenControl != null)
                    {
                        App.MainScreenControl.AcquireUserTimeoutResponse(
                            new AppDecorator.Common.AppService.Sales.UI.UISalesTimeoutWarningAck(Guid.NewGuid(), "*", DateTime.Now, AppDecorator.Common.AppService.Sales.TimeoutMode.BookingTimeoutAck),
                            requestResultState: true, out isSuccess);

                        if ((isSuccess.HasValue == true) && (isSuccess.Value == true))
                        {
                            /*- By Pass -*/
                            App.ShowDebugMsg($@"BookingTimeoutCounter.ShowBookingTimeoutNotification(); Success");
                        }
                        else
                        {
                            _hasTriggerNotification = false;

                            App.ShowDebugMsg($@"BookingTimeoutCounter.ShowBookingTimeoutNotification(); ===== FAIL =====");
                        }
                    }                    
                }
                catch (Exception ex)
                {
                    App.Log.LogError(LogChannel, "*", ex, "EX01", "BookingTimeoutCounter.ShowBookingTimeoutNotification");
                }
            }
        }
    }
}
