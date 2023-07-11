using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI;
using NssIT.Kiosk.Log.DB;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Server.ServerApp
{
    public class AppCountDown : IDisposable 
    {
        private const string LogChannel = "ServerApplication";

        private int _defaultCountDownPeriodSec = 30;
        private int _lastCountDownPeriodSec = 30;
        private int _tolerantPeriodSec = 5;
        private int _expiredWarningDelaySec = 45;

        private (Guid SessionId, bool Expired, DateTime ExpireTime, bool Pause) _session = (SessionId: Guid.Empty, Expired: true, ExpireTime: DateTime.MinValue, Pause: false);

        private ServerSalesApplication _jobApp = null;
        private object _threadLock = new object();

        private DbLog _log = DbLog.GetDbLog();

        // Booking Timeout --------------------------------------------
        // Note : ValidRemainingTimeForExtensionSec = valid remaining period to extend booking timeout refer to last Timeout value.
        private const int _validRemainingTimeForExtensionSec = 300;
        private const int _leadingBookingTimeoutSec = 30;
        (string BookingId, DateTime? Timeout, DateTime? LeadingTimeout, DateTime? NextTimeoutExtensionTime, bool TimeoutExtensionRequested, bool IsTimeoutNotified) _bookingTimeout
            = (BookingId: null, Timeout: null, LeadingTimeout: null, NextTimeoutExtensionTime: null, TimeoutExtensionRequested: false, IsTimeoutNotified: false);

        //-------------------------------------------------------------

        private bool? _timeoutWarningAck = null;
        private bool _expiredActionOnHold = false;
        private DateTime? _lastMandatoryTimeout = null;

        public AppCountDown(ServerSalesApplication jobApp, int defaultCountDownPeriodSec)
        {
            _jobApp = jobApp;
            _defaultCountDownPeriodSec = defaultCountDownPeriodSec;
            _lastCountDownPeriodSec = _defaultCountDownPeriodSec;

            Thread threadWorker = new Thread(new ThreadStart(CountingThreadWorking));
            threadWorker.IsBackground = true;
            threadWorker.Start();
        }

        public void Abort()
        {
            lock (_threadLock)
            {
                _session.SessionId = Guid.Empty;
                _session.Expired = true;
                _lastMandatoryTimeout = null;
                _expiredActionOnHold = false;
                _session.ExpireTime = DateTime.MinValue;
                _timeoutRefTag = "*Abort*";
                _lastCountDownPeriodSec = _defaultCountDownPeriodSec;

                _bookingTimeout.BookingId = null;
                _bookingTimeout.Timeout = null;
                _bookingTimeout.NextTimeoutExtensionTime = null;
                _bookingTimeout.TimeoutExtensionRequested = false;
                _bookingTimeout.LeadingTimeout = null;
                _bookingTimeout.IsTimeoutNotified = false;
            }
        }

        //public bool Pause()
        //{
        //    bool isSuccess = false;

        //    lock (_threadLock)
        //    {
        //        if (_session.Expired == false)
        //        {
        //            _session.Pause = true;
        //            isSuccess = true;
        //        }
        //    }

        //    return isSuccess;
        //}

        //public void UnPause()
        //{
        //    lock (_threadLock)
        //    {
        //        _session.Pause = false;
        //    }
        //}

        public bool IsSessionExpired(Guid sessionId, out bool isSessionFound, out bool isExpiredActionOnHold)
        {
            isExpiredActionOnHold = false;
            isSessionFound = false;
            bool isExpired = true;

            lock (_threadLock)
            {
                if (sessionId.Equals(Guid.Empty))
                    isExpired = true;

                else if (_session.SessionId.Equals(sessionId) == false)
                    isExpired = false;

                else if (_session.SessionId.Equals(sessionId))
                {
                    if (_expiredActionOnHold)
                    {
                        isExpiredActionOnHold = _expiredActionOnHold;
                        isExpired = false;
                    }
                    else
                    {
                        isExpired = _session.Expired;
                    }
                    
                    isSessionFound = true;
                }
            }
            return isExpired;
        }

        public void ResetTimeoutWarning()
        {
            _timeoutWarningAck = null;
        }

        /// <summary>
        /// Return true if success
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public bool Pause(Guid sessionId)
        {
            bool isSuccess = true;

            lock (_threadLock)
            {
                if (sessionId.Equals(Guid.Empty))
                    isSuccess = false;

                else if (_session.SessionId.Equals(sessionId) == false)
                    isSuccess = false;

                else if (_session.SessionId.Equals(sessionId))
                {
                    isSuccess = true;
                    _expiredActionOnHold = true;
                }
            }
            return isSuccess;
        }

        public void UnPause()
        {
            lock (_threadLock)
            {
                _expiredActionOnHold = false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="countDownPeriodSec"></param>
        /// <param name="sessionNetProcessId">Refer to NetProcess Id of UIStartCountDownRequest</param>
        /// <returns></returns>
        public void SetNewCountDown(int countDownPeriodSec, Guid sessionNetProcessId)
        {
            lock (_threadLock)
            {
                _expiredActionOnHold = false;
                _session.SessionId = sessionNetProcessId;
                _session.Expired = false;
                _timeoutWarningAck = null;
                _session.ExpireTime = DateTime.Now.AddSeconds(countDownPeriodSec);
                _timeoutRefTag = "*NewInit*";
                _lastCountDownPeriodSec = _defaultCountDownPeriodSec;
                _lastMandatoryTimeout = null;

                _bookingTimeout.BookingId = null;
                _bookingTimeout.Timeout = null;
                _bookingTimeout.NextTimeoutExtensionTime = null;
                _bookingTimeout.TimeoutExtensionRequested = false;
                _bookingTimeout.LeadingTimeout = null;
                _bookingTimeout.IsTimeoutNotified = false;
            }
        }

        public void EndSession()
        {
            lock (_threadLock)
            {
                _expiredActionOnHold = false;
                _session.SessionId = Guid.Empty;
                _session.Expired = true;
                _timeoutWarningAck = null;
                _session.ExpireTime = DateTime.MinValue;
                _timeoutRefTag = "*EndSession*";
                _lastCountDownPeriodSec = _defaultCountDownPeriodSec;
                _lastMandatoryTimeout = null;

                _bookingTimeout.BookingId = null;
                _bookingTimeout.Timeout = null;
                _bookingTimeout.NextTimeoutExtensionTime = null;
                _bookingTimeout.TimeoutExtensionRequested = false;
                _bookingTimeout.LeadingTimeout = null;
                _bookingTimeout.IsTimeoutNotified = false;
            }
        }

        //public void ChangeTimeoutX(TimeoutChangeMode changeMode, int countDownPeriodSec, Guid sessionNetProcessId)
        //{
        //    bool ret = false;
        //    if (changeMode == TimeoutChangeMode.ResetNormalTimeout)
        //    {
        //        ret = UpdateCountDown(countDownPeriodSec, sessionNetProcessId, out bool isMatchedSession);
        //    }
        //}

        /// <summary>
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="bookingTimeout">Valid bookingTimeout value. For invalid bookingTimeout, leave this value as null</param>
        /// <param name="isUpdateTimeoutSuccess"></param>
        public void UpdateBookingTimeout(Guid sessionId, string bookingId, DateTime? bookingTimeout, bool isUpdateTimeoutSuccess)
        {
            lock (_threadLock)
            {
                if (_session.SessionId.Equals(sessionId))
                {
                    if ((isUpdateTimeoutSuccess) && (bookingTimeout.HasValue) && (string.IsNullOrWhiteSpace(bookingId) == false))
                    {
                        _bookingTimeout.Timeout = bookingTimeout;
                        _bookingTimeout.BookingId = bookingId;
                        _bookingTimeout.LeadingTimeout = bookingTimeout.Value.AddSeconds(_leadingBookingTimeoutSec * -1);

                        DateTime tmpNextBookingTimeoutExtensionTime = _bookingTimeout.Timeout.Value.AddSeconds(_validRemainingTimeForExtensionSec * -1);

                        if (tmpNextBookingTimeoutExtensionTime.Subtract(DateTime.Now).TotalSeconds > (_validRemainingTimeForExtensionSec + 5))
                        {
                            // Update the next time for extending Booking Timeout
                            _bookingTimeout.NextTimeoutExtensionTime = tmpNextBookingTimeoutExtensionTime;
                        }
                        else
                        {
                            // Disable Booking Timeout Extension
                            _bookingTimeout.NextTimeoutExtensionTime = null;
                        }
                    }
                    else if (isUpdateTimeoutSuccess == false)
                    {
                        // Disable Booking Timeout Extension
                        _bookingTimeout.NextTimeoutExtensionTime = null;
                    }

                    _bookingTimeout.TimeoutExtensionRequested = false;
                }
            }
        }
        
        private string _timeoutRefTag = "*****";
        public AppCountDown ChangeDefaultTimeoutSetting(int countDownPeriodSec, string timeoutRefTag)
        {
            if (countDownPeriodSec >= 30)
                _lastCountDownPeriodSec = countDownPeriodSec;

            _timeoutRefTag = timeoutRefTag ?? "#####";

            return this;
        }

        public bool ResetTimeout(Guid sessionNetProcessId, out bool isMatchedSession)
        {
            bool result = ClientChangeTimeout(TimeoutChangeMode.ResetNormalTimeout, _lastCountDownPeriodSec, sessionNetProcessId, _timeoutRefTag, out isMatchedSession);
            return result;
        }

        /// <summary>
        /// Normally refer to the need of client UI.
        /// </summary>
        /// <param name="changeMode"></param>
        /// <param name="countDownPeriodSec"></param>
        /// <param name="sessionNetProcessId"></param>
        /// <param name="changeRefTag"></param>
        public bool ClientChangeTimeout(TimeoutChangeMode changeMode, int countDownPeriodSec, Guid sessionNetProcessId, string changeRefTag, out bool isMatchedSession)
        {
            bool ret = false;
            isMatchedSession = true;

            if (changeMode == TimeoutChangeMode.ResetNormalTimeout)
            {
                lock (_threadLock)
                {
                    //DateTime expectedExpireTime = DateTime.Now.AddSeconds(countDownPeriodSec);

                    //if (_session.ExpireTime.Ticks < expectedExpireTime.Ticks)
                    //{
                    ret = UpdateCountDown(countDownPeriodSec, sessionNetProcessId, isMandatoryExtensionChange: false, changeRefTag, out isMatchedSession);
                    //}
                }
            }

            else if (changeMode == TimeoutChangeMode.MandatoryExtension)
            {
                lock (_threadLock)
                {
                    int totalExtensionSec = Convert.ToInt32( _session.ExpireTime.Subtract(DateTime.Now).TotalSeconds);

                    totalExtensionSec = (totalExtensionSec > 0) ? totalExtensionSec : 0;
                    totalExtensionSec = totalExtensionSec + countDownPeriodSec;

                    ret = UpdateCountDown(totalExtensionSec, sessionNetProcessId, isMandatoryExtensionChange: true, changeRefTag, out isMatchedSession);
                }
            }

            else if (changeMode == TimeoutChangeMode.RemoveMandatoryTimeout)
            {
                _lastMandatoryTimeout = null;

                if (_session.SessionId.Equals(sessionNetProcessId) == false)
                {
                    isMatchedSession = false;
                }
            }

            return ret;
        }

        /// <summary>
        /// Return false when session expired or session not matched
        /// </summary>
        /// <param name="countDownPeriodSec"></param>
        /// <param name="sessionNetProcessId">Refer to NetProcess Id of UIStartCountDownRequest</param>
        /// <returns></returns>
        public bool UpdateCountDown(int countDownPeriodSec, Guid sessionNetProcessId, bool isMandatoryExtensionChange, string changeRefTag, out bool isMatchedSession)
        {
            isMatchedSession = false;
            bool isSuccess = false;
            changeRefTag = changeRefTag ?? "*";
            string logStr = $@"changeRefTag: {changeRefTag}; countDownPeriodSec: {countDownPeriodSec}; sessionNetProcessId: {sessionNetProcessId}; isMandatoryExtensionChange: {isMandatoryExtensionChange}; ";

            lock (_threadLock)
            {
                if (_session.SessionId.Equals(sessionNetProcessId) == false)
                {
                    isMatchedSession = false;
                    isSuccess = false;
                }
                else if (_session.SessionId.Equals(sessionNetProcessId))
                {
                    isMatchedSession = true;
                    if (_session.Expired == false)
                    {
                        DateTime newExpireTime = DateTime.Now.AddSeconds(countDownPeriodSec);

                        if (isMandatoryExtensionChange)
                        {
                            _lastMandatoryTimeout = newExpireTime;
                            _session.ExpireTime = newExpireTime;

                            _log.LogText(LogChannel, "-", logStr, "B05", "AppCountDown.UpdateCountDown");
                        }
                        else
                        {
                            if (_lastMandatoryTimeout.HasValue)
                            {
                                if (newExpireTime.Ticks > _lastMandatoryTimeout.Value.Ticks)
                                {
                                    _session.ExpireTime = newExpireTime;

                                    _log.LogText(LogChannel, "-", logStr, "B06", "AppCountDown.UpdateCountDown");
                                }
                            }
                            else
                            {
                                _session.ExpireTime = newExpireTime;

                                _log.LogText(LogChannel, "-", logStr, "B07", "AppCountDown.UpdateCountDown");
                            }
                        }
                        isSuccess = true;
                    }
                    else
                        isSuccess = false;
                }
            }

            return isSuccess;
        }

        private void CountingThreadWorking()
        {
            _log.LogText(LogChannel, "", "Start of Count Down Thread", "A01", "AppCountDown.CountingThreadWorking");

            //Booking Timeout --------
            int trimTimeoutSec = 8;
            //------------------------

            while (_disposed == false)
            {
                Thread.Sleep(1000);
                if ((_session.SessionId.Equals(Guid.Empty) == false) && (_session.Expired != true))
                {
                    try
                    {
                        lock (_threadLock)
                        {
                            if ((_session.SessionId.Equals(Guid.Empty) == false) && (_session.Expired != true))
                            {

                                if (_expiredActionOnHold == true)
                                {
                                    // By Pass
                                    string tt1 = "_expiredActionOnHold";
                                }
                                else if ((_session.ExpireTime.Subtract(DateTime.Now).TotalMilliseconds <= 0) && (_session.Pause == false ))
                                {
                                    if (_timeoutWarningAck.HasValue && (_timeoutWarningAck.Value == true))
                                    {
                                        _session.Expired = true;
                                        _lastMandatoryTimeout = null;

                                        _jobApp.RaiseOnShowResultMessage(_session.SessionId, new UICountDownExpiredAck(_session.SessionId, "-", DateTime.Now));
                                    }
                                    else
                                    {
                                        _session.ExpireTime = DateTime.Now.AddSeconds(_expiredWarningDelaySec);
                                        _timeoutWarningAck = true;
                                        _jobApp.RaiseOnShowResultMessage(_session.SessionId, new UISalesTimeoutWarningAck(_session.SessionId, "-", DateTime.Now));
                                    }
                                }
                                else if (IsNofifyBookingTimeout())
                                {
                                    _session.ExpireTime = DateTime.Now.AddSeconds(60);
                                    _bookingTimeout.IsTimeoutNotified = true;

                                    _jobApp.RaiseOnShowResultMessage(_session.SessionId, new UISalesTimeoutWarningAck(_session.SessionId, "-", DateTime.Now, TimeoutMode.BookingTimeoutAck));
                                }
                                else if (IsBookingTimeout())
                                {
                                    _session.Expired = true;
                                    _lastMandatoryTimeout = null;

                                    _jobApp.RaiseOnShowResultMessage(_session.SessionId, new UICountDownExpiredAck(_session.SessionId, "-", DateTime.Now));
                                }
                                else if (IsBookingTimeoutNeedExtension())
                                {
                                    ExtendBookingTimeout(_bookingTimeout.BookingId);
                                }

                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _log.LogError(LogChannel, "-", ex, "EX01", "AppCountDown.CountingThreadWorking");
                    }
                }
            }

            _log.LogText(LogChannel, "-", "End of Count Down Thread", "A10", "AppCountDown.CountingThreadWorking");

            //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            
            bool IsBookingTimeout()
            {
                if (_bookingTimeout.Timeout.HasValue == false)
                    return false;

                DateTime bkTimeout = _bookingTimeout.Timeout.Value.Subtract(new TimeSpan(0, 0, trimTimeoutSec));

                if (bkTimeout.Subtract(DateTime.Now).TotalSeconds <= 0)
                    return true;

                return false;
            }

            ///<summary>
            ///Check for the valid times to Nofify UI for Booking has came to Timeout.
            ///</summary>
            bool IsNofifyBookingTimeout()
            {
                if ((_bookingTimeout.IsTimeoutNotified == false) && (_bookingTimeout.LeadingTimeout.HasValue))
                {
                    if ((_bookingTimeout.LeadingTimeout.Value.Subtract(DateTime.Now).TotalSeconds <= 0))
                        return true;
                    else
                        return false;
                }
                return false;
            }

            // Check BookingTimeout for the need of extension
            bool IsBookingTimeoutNeedExtension()
            {
                // .. if do not have booking timeout
                if ((_bookingTimeout.Timeout.HasValue) && (string.IsNullOrWhiteSpace(_bookingTimeout.BookingId) == false))
                {
                    // .. if booking Timeout Extension has already request and still wait for response.
                    if (_bookingTimeout.TimeoutExtensionRequested)
                        return false;

                    if (_bookingTimeout.NextTimeoutExtensionTime?.Subtract(DateTime.Now).TotalSeconds <= 0)
                        return true;
                }

                return false;
            }

            void ExtendBookingTimeout(string bookingId)
            {
                if (string.IsNullOrWhiteSpace(bookingId))
                    return;

                _log.LogText(LogChannel, "-", $@"Start Request- ExtendBookingTimeout; bookingId : {bookingId}; Time: {DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")}", "A01", "AppCountDown.ExtendBookingTimeout");

                IKioskMsg msg = new UISalesBookingTimeoutExtensionRequest("**", DateTime.Now, bookingId);

                _jobApp.SendInternalCommand("**", msg.RefNetProcessId, msg);

                _bookingTimeout.TimeoutExtensionRequested = true;

                _log.LogText(LogChannel, "-", $@"End Request- ExtendBookingTimeout; bookingId : {bookingId}; Time: {DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")}", "A10", "AppCountDown.ExtendBookingTimeout");
            }
        }

        private bool _disposed = false;
        public void Dispose()
        {
            if (_disposed == false)
            {

                _disposed = true;
            }
        }


    }
}
