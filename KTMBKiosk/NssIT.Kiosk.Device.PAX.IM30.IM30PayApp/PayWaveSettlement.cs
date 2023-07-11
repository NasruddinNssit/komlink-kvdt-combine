using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NssIT.Kiosk.Device.PAX.IM20.AccessSDK;
//////using NssIT.Kiosk.Device.PAX.IM20.OrgAPI.Base;
using NssIT.Kiosk.Log;
using NssIT.Kiosk.Log.DB;
using System.Collections.Concurrent;
using System.Net.Http;
//////using NssIT.Kiosk.Device.PAX.IM20.OrgAPI;
using NssIT.Kiosk.AppDecorator.DomainLibs.Common.CreditDebitCharge;
using NssIT.Kiosk.Device.PAX.IM30.AccessSDK;
using Newtonsoft.Json;
using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.Base;
using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.CardEntityData.Settlement;
using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.CardEntityData;
using System.Net.Http.Headers;

namespace NssIT.Kiosk.Device.PAX.IM30.IM30PayApp
{
    public class PayWaveSettlement : IDisposable
    {
        private const string LogChannel = "IM30_APP";

        public delegate string[] RequestOutstandingSettlementInfo(out bool isRequestSuccessful);
        /// <summary>
        /// Return true when update DB successful.
        /// </summary>
        /// <param name="processId"></param>
        /// <param name="responseInfo"></param>
        /// <returns></returns>
        public delegate bool UpdateSettlementInfo(string processId, ResponseInfo responseInfo);

        private object _dataLock = new object();
        private ConcurrentQueue<HostInfo> _hostList = new ConcurrentQueue<HostInfo>();

        private int _hostCount = 0;
        private bool _endWorking = false;
        private bool _isBusyFlag = false;

        //private string _receiptPath = @"C:\eTicketing_Log\ECR_Receipts\";
        private string _lastProcessHostNo = "01";
        private string _paywWaveCOM = "";

        private Thread _threadWorker;
        private CardTransResponseEventArgs _lastResponseInfo = null;

        public event EventHandler<TrxCallBackEventArgs> OnSettlementDoneCallback;

        public PayWaveSettlement(string paywWaveCOM)
        {
            _paywWaveCOM = (paywWaveCOM ?? "").Trim();

            if (_paywWaveCOM.Length == 0)
                throw new Exception("Invalid COM port specification to PayWave Settlement.");

            _threadWorker = new Thread(new ThreadStart(SettlementThreadWorking));
            _threadWorker.IsBackground = true;
            _threadWorker.Start();
        }

        private DbLog _schdLog = null;
        private DbLog Log
        {
            get => _schdLog ?? (_schdLog = DbLog.GetDbLog());
        }

        private string _lastProcessId = "--";
        private void SettlementThreadWorking()
        {
            CardTransResponseEventArgs sttRespInfo = null;
            HostInfo hsif = null;
            string lastHsno = null;
            string procId = "-";
            string lastProcessMessage = "";

            Log.LogText(LogChannel, procId, $@"Start settlement working", "START", "PayWaveSettlement.SettlementThreadWorking");

            bool? isLastSettleStartSuccessful = null;
            while (!_endWorking)
            {
                hsif = null;
                sttRespInfo = null;

                lastProcessMessage = "";
                _lastResponseInfo = null;
                procId = "-";
                _lastProcessId = procId;
                //----------------------------------------------------
                // Settlement
                try
                {
                    if (isLastSettleStartSuccessful == false)
                    {
                        hsif = new HostInfo() { Inx = 0, HostNo = "R1", RequestToSettlement = true };
                    }
                    else
                    {
                        hsif = GetNextSettlementHost();
                    }

                    if (hsif != null)
                    {

                        lastProcessMessage = $@"----- Start Settlement -----" + "\r\n";

                        procId = Guid.NewGuid().ToString();
                        _lastProcessId = procId;

                        lastHsno = hsif.HostNo;

                        bool isSettlementRun = false;
                        Exception errorJ = null;

                        try
                        {
                            sttRespInfo = DoSettlement(procId, out isSettlementRun, out errorJ);
                        }
                        catch (Exception ex)
                        {
                            isLastSettleStartSuccessful = false;
                            Log.LogError(LogChannel, procId, ex, "X10", "SettlementThreadWorking.SettlementThreadWorking");
                        }

                        if (isSettlementRun)
                        {
                            isLastSettleStartSuccessful = true;

                            if (sttRespInfo.ResponseInfo is SettlementResp sttCardInfo)
                            {
                                lastProcessMessage += $@"{DateTime.Now.ToString()} - Settlement Done with Status Code : {sttCardInfo.SettlementResult}; ErrMsg : {sttCardInfo.DataError?.Message}" + "\r\n";
                            }
                        }
                        else
                        {
                            isLastSettleStartSuccessful = false;
                            errorJ = errorJ ?? new Exception("Unknown error; Fail to start settlement");
                            Log.LogError(LogChannel, procId, errorJ, "X15", "SettlementThreadWorking.SettlementThreadWorking");
                        }
                    }
                    else
                    {
                        lastHsno = null;
                    }
                }
                catch (Exception ex)
                {
                    hsif.LastSettlementTime = DateTime.Now;
                    hsif.LastSettlementStatus = "99";
                    hsif.LastSettlementErrMsg = $@"Error occur when Settlement. ProcId:{procId}; Error : {ex.Message}";

                    lastProcessMessage += $@"{DateTime.Now.ToString()} - {hsif.LastSettlementErrMsg}" + "\r\n";
                    Log.LogError(LogChannel, procId, ex, "Error when doing settlement", "PayWaveSettlement.SettlementThreadWorking");
                }
                finally
                {
                    if (hsif != null)
                    {
                        lastProcessMessage += $@"{DateTime.Now.ToString()} - Ending Settlement.." + "\r\n";
                    }
                }
                //----------------------------------------------------
                if (hsif == null)
                {
                    lock (_hostList)
                    {
                        if (_hostList.Count == 0)
                            _isBusyFlag = false;
                    }
                }

                if (string.IsNullOrWhiteSpace(lastProcessMessage) == false)
                    Log.LogText(LogChannel, "-", lastProcessMessage, "END", "PayWaveSettlement.SettlementThreadWorking");

                // Below Sleep is a latency time for Machine to complete outstanding work.
                Thread.Sleep(5 * 1000);
            }
        }

        public void SettleHost(string host)
        {
            if ((host ?? "").Trim().Length == 0)
                return;

            lock (_hostList)
            {
                _isBusyFlag = true;
                _hostCount++;
                _hostList.Enqueue(new HostInfo() { Inx = _hostCount, HostNo = host.Trim(), RequestToSettlement = true });

                Monitor.PulseAll(_hostList);
            }
        }

        public bool IsSystemBusy
        {
            get
            {
                return _isBusyFlag;
            }
        }

        private TimeSpan _MaxWaitPeriod = new TimeSpan(0, 0, 1);
        private HostInfo GetNextSettlementHost()
        {
            HostInfo retHsif = null;

            lock (_hostList)
            {
                if (_hostList.Count == 0)
                {
                    Monitor.Wait(_hostList, _MaxWaitPeriod);
                }

                if (_hostList.TryDequeue(out retHsif))
                    return retHsif;
            }
            return retHsif;
        }

        private IM30AccessSDK _cardPayReader = null;
        private IM30AccessSDK CardPayReader
        {
            get
            {
                return _cardPayReader ?? (_cardPayReader = NewCardPayReader());

                IM30AccessSDK NewCardPayReader()
                {
                    IM30AccessSDK cardPayReader = new IM30AccessSDK(_paywWaveCOM, null);

                    cardPayReader.OnTransactionResponse += CardPayReader_OnTransactionResponse;
                    _cardPayReader = cardPayReader;

                    return cardPayReader;
                }
            }
        }

        private CardTransResponseEventArgs DoSettlement(string procId, out bool isSettlementRun, out Exception error)
        {
            _lastResponseInfo = null;
            isSettlementRun = false;
            error = null;

            IM30AccessSDK cardPayReader = CardPayReader;

            Log.LogText(LogChannel, procId, $@"Start running settlement", "Start", "PayWaveSettlement.DoSettlement");

            bool isSettSent = false;
            int reTry = 0;
            do
            {
                Exception errorB = null;
                try
                {
                    isSettSent = false;
                    isSettSent = cardPayReader.SettleCreditDebitSales(out errorB);
                }
                catch { }

                if (isSettSent == false)
                {
                    error = errorB;
                    Thread.Sleep(5000);
                }
                else
                {
                    error = null;
                    break;
                }

                reTry++;
            } while (reTry < 10);

            if (isSettSent == false)
            {
                isSettlementRun = false;
                if (error is null)
                    error = new Exception("Unknown error; Unable to run settlement");

                return null;
            }

            error = null;
            isSettlementRun = true;

            DateTime startTime = DateTime.Now;
            DateTime endTime = startTime.AddSeconds(IM30PortManagerAx.SettlementMaxWaitTimeSec + 30);

            while ((_lastResponseInfo == null) && (endTime.Subtract(startTime).TotalSeconds > 0))
            {
                // Wait until finish work.
                Thread.Sleep(1000);
            }

            if (_lastResponseInfo == null)
            {
                _lastResponseInfo = new CardTransResponseEventArgs(TransactionTypeEn.Settlement, TransEventCodeEn.Timeout, 
                    new SettlementResp(new Exception("Timeout. Unable to do settlement at the moment")), "Unable to do settlement at the moment");

                CardPayReader_OnTransactionResponse(null, _lastResponseInfo);
            }
            Log.LogText(LogChannel, procId, $@"End Settlement", "End", "PayWaveSettlement.DoSettlement");
            
            return _lastResponseInfo;
        }

        private void CardPayReader_OnTransactionResponse(object sender, AppDecorator.DomainLibs.IM30.CardEntityData.CardTransResponseEventArgs e)
        {
            _lastResponseInfo = e;
            Log.LogText(LogChannel, _lastProcessId, new WithDataObj("Start - Receive Settlement Result", e), "A01", "PayWaveSettlement.CardPayReader_OnTransactionResponse");
            
            if (e.TransType == TransactionTypeEn.Settlement)
            {
                SettlementReceivedResult(e);
            }
            
            return;
            /////==============================================================================================
            void SettlementReceivedResult(CardTransResponseEventArgs eArg)
            {
                List<TrxCallBackEventArgs> respInfoList = new List<TrxCallBackEventArgs>();
                if (eArg.ResponseInfo is SettlementResp sttCardInfo)
                {
                    ///// ===== ===== Settlement - Success - ===== =====
                    if (sttCardInfo.SettlementResult == SettlementStatusEn.Success)
                    {
                        bool isFailSettleFound = false;
                        foreach (SettlementBatch sBath in sttCardInfo.SettlementList)
                        {
                            if (sBath.BatchSettlementStatus == BatchSettlementStatusEn.Success)
                            {
                                ResponseInfo rInfo = new ResponseInfo()
                                {
                                    ResponseMsg = "SETTLEMENT",
                                    HostNo = "01",
                                    StatusCode = ResponseCodeDef.Approved,
                                    BatchNumber = (sBath.BatchNo ?? "").Trim(),
                                    BatchCount = sBath.TotalSalesCount.ToString(),
                                    BatchAmount = Convert.ToInt32(Math.Floor(((sBath.TotalSalesAmount - sBath.TotalRefundAmount) / 100M))).ToString(),
                                    BatchCurrencyAmount = (sBath.TotalSalesAmount - sBath.TotalRefundAmount)
                                };
                                respInfoList.Add(new TrxCallBackEventArgs()
                                {
                                    IsSuccess = true,
                                    ProcessId = Guid.NewGuid().ToString(),
                                    Result = rInfo
                                });
                            }
                            else
                                isFailSettleFound = true;
                        }

                        if (isFailSettleFound)
                        {
                            respInfoList.Add(new TrxCallBackEventArgs()
                            {
                                IsSuccess = false,
                                ProcessId = Guid.NewGuid().ToString(),
                                Result = new ResponseInfo() { HostNo = _lastProcessHostNo, StatusCode = "99", ErrorMsg = "-Fail Settlement; Unknown Error (D)~" }
                            });
                        }
                    }

                    ///// ===== ===== Settlement Partially Done - ===== =====
                    else if (sttCardInfo.SettlementResult == SettlementStatusEn.PartiallyDone)
                    {
                        respInfoList.Add(new TrxCallBackEventArgs()
                        {
                            IsSuccess = false,
                            ProcessId = Guid.NewGuid().ToString(),
                            Result = new ResponseInfo() { HostNo = _lastProcessHostNo, StatusCode = "99", ErrorMsg = "-Fail Settlement; Only Partial of Batches has settled~" }
                        });

                        foreach (SettlementBatch sBath in sttCardInfo.SettlementList)
                        {
                            if (sBath.BatchSettlementStatus == BatchSettlementStatusEn.Success)
                            {
                                ResponseInfo rInfo = new ResponseInfo()
                                {
                                    ResponseMsg = "SETTLEMENT",
                                    HostNo = "01",
                                    StatusCode = ResponseCodeDef.Approved,
                                    BatchNumber = (sBath.BatchNo ?? "").Trim(),
                                    BatchCount = sBath.TotalSalesCount.ToString(),
                                    BatchAmount = Convert.ToInt32(Math.Floor(((sBath.TotalSalesAmount - sBath.TotalRefundAmount) / 100M))).ToString(),
                                    BatchCurrencyAmount = (sBath.TotalSalesAmount - sBath.TotalRefundAmount)
                                };
                                respInfoList.Add(new TrxCallBackEventArgs()
                                {
                                    IsSuccess = true,
                                    ProcessId = Guid.NewGuid().ToString(),
                                    Result = rInfo
                                });
                            }
                        }
                    }

                    ///// ===== ===== Empty Settlement - ===== =====
                    else if (sttCardInfo.SettlementResult == SettlementStatusEn.Empty)
                    {
                        respInfoList.Add(new TrxCallBackEventArgs()
                        {
                            IsSuccess = false,
                            ProcessId = Guid.NewGuid().ToString(),
                            Result = new ResponseInfo() { HostNo = _lastProcessHostNo, StatusCode = ResponseCodeDef.TransactionNotAvailable, ErrorMsg = sttCardInfo.DataError?.Message }
                        });
                    }

                    ///// ===== ===== Fail Settlement - ===== =====
                    else /* if (sttCardInfo.SettlementResult == SettlementStatusEn.Fail) */
                    {
                        if (string.IsNullOrWhiteSpace(sttCardInfo.DataError?.Message))
                        {
                            respInfoList.Add(new TrxCallBackEventArgs()
                            {
                                IsSuccess = false,
                                ProcessId = Guid.NewGuid().ToString(),
                                Result = new ResponseInfo() { HostNo = _lastProcessHostNo, StatusCode = "99", ErrorMsg = "-Fail Settlement; Unknown Error (A)~" }
                            });
                        }
                        else
                        {
                            respInfoList.Add(new TrxCallBackEventArgs()
                            {
                                IsSuccess = false,
                                ProcessId = Guid.NewGuid().ToString(),
                                Result = new ResponseInfo() { HostNo = _lastProcessHostNo, StatusCode = "99", ErrorMsg = sttCardInfo.DataError.Message }
                            });
                        }
                    }
                }

                ///// ===== ===== Settlement - Fail - ===== =====
                else if (eArg.ResponseInfo is ErrorResponse errResp)
                {
                    if (string.IsNullOrWhiteSpace(errResp.DataError?.Message))
                    {
                        respInfoList.Add(new TrxCallBackEventArgs()
                        {
                            IsSuccess = false,
                            ProcessId = Guid.NewGuid().ToString(),
                            Result = new ResponseInfo() { HostNo = _lastProcessHostNo, StatusCode = "99", ErrorMsg = "-Fail Settlement; Unknown Error (B)~" }
                        });
                    }
                    else
                    {
                        respInfoList.Add(new TrxCallBackEventArgs()
                        {
                            IsSuccess = false,
                            ProcessId = Guid.NewGuid().ToString(),
                            Result = new ResponseInfo() { HostNo = _lastProcessHostNo, StatusCode = "99", ErrorMsg = errResp.DataError.Message.Trim() }
                        });
                    }
                }

                SendSettlementResult(respInfoList);
            }

            void SendSettlementResult(List<TrxCallBackEventArgs> respInfoList)
            {
                Log.LogText(LogChannel, "*", respInfoList, "A01", "PayWaveSettlement.SendSettlementResult");
                foreach (TrxCallBackEventArgs rInfo in respInfoList)
                {
                    if (OnSettlementDoneCallback != null)
                    {
                        TrxCallBackEventArgs ne = rInfo.Duplicate();
                        try
                        {
                            OnSettlementDoneCallback.Invoke(this, ne);
                            Thread.Sleep(1000);
                        }
                        catch (Exception ex)
                        {
                            Log.LogError(LogChannel, rInfo.ProcessId, ex, "EX01", "PayWaveSettlement.SendSettlementResult");
                        }
                        
                    }
                    Log.LogText(LogChannel, rInfo.ProcessId, "-End-", "A04", "PayWaveSettlement.SendSettlementResult");
                }
            }
        }


        public void Dispose()
        {
            _endWorking = true;

            if (_hostList != null)
            {
                lock (_hostList)
                {
                    HostInfo hsif = null;
                    while (_hostList.TryDequeue(out hsif)) { string tt = "just to release item in the list"; }
                    Monitor.PulseAll(_hostList);
                }
            }

            if (OnSettlementDoneCallback != null)
            {
                Delegate[] delgList = OnSettlementDoneCallback.GetInvocationList();
                foreach (EventHandler<TrxCallBackEventArgs> delg in delgList)
                    OnSettlementDoneCallback -= delg;
            }

            if (_cardPayReader != null)
            {
                _cardPayReader.OnTransactionResponse -= CardPayReader_OnTransactionResponse;

                //CYA-DEBUG if _cardPayReader success create transaction, then not need to stop;
                _cardPayReader.StopCardTransaction(out _);
                _cardPayReader.Dispose();
                _cardPayReader = null;
            }
        }

        private class HostInfo
        {
            public int Inx;
            public string HostNo = null;
            public bool RequestToSettlement;
            public DateTime LastSettlementTime;
            public string LastSettlementStatus = null;
            public string LastSettlementErrMsg = null;

            public DateTime LastWebTransTime;
            public bool LastWebTransIsSuccess = false;
            public string LastWebTransErrMsg = null;

            public DateTime NextSettlementSchedule;
        }
    }
}