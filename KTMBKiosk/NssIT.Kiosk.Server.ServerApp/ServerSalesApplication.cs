using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Command;
using NssIT.Kiosk.AppDecorator.Common.AppService.Command.Sales;
using NssIT.Kiosk.AppDecorator.Common.AppService.Events;
using NssIT.Kiosk.AppDecorator.Common.AppService.Instruction;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI;
using NssIT.Kiosk.AppDecorator.Config.ConfigConstant;
using NssIT.Kiosk.AppDecorator.DomainLibs.Sales.UIx;
using NssIT.Kiosk.AppDecorator.DomainLibs.Sqlite.DB.Access.Echo;
using NssIT.Kiosk.AppDecorator.UI;
using NssIT.Kiosk.Log.DB;
using NssIT.Kiosk.Network.SignalRClient.API.Base;
using NssIT.Kiosk.Server.AccessDB;
using NssIT.Kiosk.Server.ServerApp.CustomApp;
using NssIT.Kiosk.Server.ServerApp.JobApp;
using NssIT.Kiosk.Tools.ThreadMonitor;
using NssIT.Train.Kiosk.Common.Common;
using NssIT.Train.Kiosk.Common.Constants;
using NssIT.Train.Kiosk.Common.Data.Response;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Server.ServerApp
{
    /// <summary>
    /// ClassCode:EXIT62.01
    /// </summary>
    public class ServerSalesApplication : IUIApplicationJob, IDisposable
    {
        private int _timeDelayMultiplicationIndex = 1; /* Default is 1, and should not less then 1*/

        /// <summary>
        /// This index used to multiply Timeout for purpose of DEMO; Default value should be 1.
        /// </summary>
        private int TimeDelayMultiplicationIndex
        {
            get
            {
                if (_timeDelayMultiplicationIndex >= 1)
                    return _timeDelayMultiplicationIndex;

                return 1;
            }
        }

        private int _standardCountDownIntervalSec = 30;
        private int _custInfoEntryIntervalSec = 60;

        //CYA-DEMO
        //private int _standardCountDownIntervalSec = 30 * 10;
        //private int _custInfoEntryIntervalSec = 60 * 3;
        //-------------------------------------------------------------

        private int _maxProcessPeriodSec = 180;

        private const string LogChannel = "ServerApplication";

        private bool _disposed = false;
        private bool _clientAppUnderMaintenance = false;

        private UserSession _session = new UserSession();
        private ServerAccess _svrAccess = null;
        private AppCountDown _appCountDown = null;
        private IServerAppPlan _svrAppPlan = null;

        //private List<Guid> _sessionNetProcessIDList = new List<Guid>();

        private AppModule _appModule = AppModule.UIKioskSales;
        private SessionSupervisor _SessionSuper = new SessionSupervisor();
        private KioskStatesJobMan _kioskStatesJobMan = new KioskStatesJobMan();

        public event EventHandler<UIMessageEventArgs> OnShowResultMessage;

        /// <summary>
        /// FuncCode:EXIT62.0101
        /// </summary>
        public ServerSalesApplication()
        {
            _svrAccess = ServerAccess.GetAccessServer();
            _appCountDown = new AppCountDown(this, _standardCountDownIntervalSec);
            _svrAppPlan = new KTMBAppPlan();
            _svrAccess.OnSendMessage += _b2bAccess_OnSendMessage;
            
        }

        private DbLog _log = null;
        private DbLog Log => (_log ?? (_log = DbLog.GetDbLog()));

        private string _currProcessId = "-";
        public string CurrentProcessId
        {
            get => _currProcessId;
            private set => _currProcessId = string.IsNullOrEmpty(value) ? "-" : value.Trim();
        }

        /// <summary>
        /// FuncCode:EXIT62.0102
        /// </summary>
        private void _b2bAccess_OnSendMessage(object sender, SendMessageEventArgs e)
        {
            if (e.Module != AppDecorator.Common.AppService.AppModule.UIKioskSales)
                return;

            if (_disposed)
            {
                Log.LogText(LogChannel, _currProcessId, $@"Application server has shutdowned. Net Process Id: {e?.NetProcessId};",
                    $@"A01", classNMethodName: "ServerSalesApplication._b2bAccess_OnSendMessage");
                return;
            }

            Log.LogText(LogChannel, _currProcessId, e, $@"A02", classNMethodName: "ServerSalesApplication._b2bAccess_OnSendMessage",
                extraMsg: $@"Start - _b2bAccess_OnCompleted; Net Process Id:{e?.NetProcessId}; MsgObj: SendMessageEventArgs");

            // e.EventMessageObj must has valid for returning a result.
            if (e.KioskDataPack != null)
            {
                if (e.KioskDataPack is UIWebServerLogonStatusAck uiLogonStt)
                {
                    SendInternalCommand(uiLogonStt.ProcessId, uiLogonStt.RefNetProcessId, uiLogonStt);
                }
                
                
                else if (e.KioskDataPack is UICounterConfigurationResult uiCounterConfig)
                {
                    SendInternalCommand(uiCounterConfig.ProcessId, uiCounterConfig.RefNetProcessId, uiCounterConfig);
                }

                else if (e.KioskDataPack is UISalesCheckOutstandingCardSettlementAck uiOutSett)
                {
                    SendInternalCommand(uiOutSett.ProcessId, uiOutSett.RefNetProcessId, uiOutSett);
                }

                else if (e.KioskDataPack is UISalesCardSettlementStatusAck uiSettStat)
                {
                    SendInternalCommand(uiSettStat.ProcessId, uiSettStat.RefNetProcessId, uiSettStat);
                }

                else if (e.KioskDataPack is UIOriginListAck uiOriginList)
                {
                    SendInternalCommand(uiOriginList.ProcessId, uiOriginList.RefNetProcessId, uiOriginList);
                }

                else if (e.KioskDataPack is UIDestinationListAck uiDestList)
                {
                    SendInternalCommand(uiDestList.ProcessId, uiDestList.RefNetProcessId, uiDestList);
                }

                else if (e.KioskDataPack is UIDepartTripListAck uiTripList)
                {
                    SendInternalCommand(uiTripList.ProcessId, uiTripList.RefNetProcessId, uiTripList);
                }
                
                else if (e.KioskDataPack is UIDepartSeatListAck uiSeatList)
                {
                    SendInternalCommand(uiSeatList.ProcessId, uiSeatList.RefNetProcessId, uiSeatList);
                }
                
                else if (e.KioskDataPack is UIDepartSeatConfirmResult uiResult)
                {
                    SendInternalCommand(uiResult.ProcessId, uiResult.RefNetProcessId, uiResult);
                }
                else if (e.KioskDataPack is UIReturnTripListAck uiRTripList)
                {
                    SendInternalCommand(uiRTripList.ProcessId, uiRTripList.RefNetProcessId, uiRTripList);
                }
                else if (e.KioskDataPack is UIReturnSeatListAck uiRSeatList)
                {
                    SendInternalCommand(uiRSeatList.ProcessId, uiRSeatList.RefNetProcessId, uiRSeatList);
                }
                else if (e.KioskDataPack is UIReturnSeatConfirmResult uiRResult)
                {
                    SendInternalCommand(uiRResult.ProcessId, uiRResult.RefNetProcessId, uiRResult);
                }
                else if (e.KioskDataPack is UICustInfoPrerequisiteAck uiCustInfoPreq)
                {
                    SendInternalCommand(uiCustInfoPreq.ProcessId, uiCustInfoPreq.RefNetProcessId, uiCustInfoPreq);
                }
                else if (e.KioskDataPack is UICustPromoCodeVerifyAck uiCustPromoCode)
                {
                    SendInternalCommand(uiCustPromoCode.ProcessId, uiCustPromoCode.RefNetProcessId, uiCustPromoCode);
                }
                else if (e.KioskDataPack is UIDepartCustInfoUpdateResult uiCIUpdResult)
                {
                    SendInternalCommand(uiCIUpdResult.ProcessId, uiCIUpdResult.RefNetProcessId, uiCIUpdResult);
                }
                else if (e.KioskDataPack is UIETSCheckoutSaleResult uiETSCheckoutResult)
                {
                    SendInternalCommand(uiETSCheckoutResult.ProcessId, uiETSCheckoutResult.RefNetProcessId, uiETSCheckoutResult);
                }

                else if (e.KioskDataPack is UIETSInsuranceListAck uiETSInsrLstResult)
                {
                    SendInternalCommand(uiETSInsrLstResult.ProcessId, uiETSInsrLstResult.RefNetProcessId, uiETSInsrLstResult);
                }

                else if (e.KioskDataPack is UISalesETSInsuranceSubmissionResult uiETSInsrSubmitResult)
                {
                    SendInternalCommand(uiETSInsrSubmitResult.ProcessId, uiETSInsrSubmitResult.RefNetProcessId, uiETSInsrSubmitResult);
                }

                else if (e.KioskDataPack is UICompleteTransactionResult uiCompltResult)
                {
                    SendInternalCommand(uiCompltResult.ProcessId, uiCompltResult.RefNetProcessId, uiCompltResult);
                }
                else if (e.KioskDataPack is UISeatReleaseResult uiSeatRel)
                {
                    // By Pass -- SendInternalCommand(uiSeatRel.ProcessId, uiSeatRel.RefNetProcessId, uiSeatRel);
                }
                else if (e.KioskDataPack is UIKomuterTicketTypePackageAck uiKomuterPackage)
                {
                    SendInternalCommand(uiKomuterPackage.ProcessId, uiKomuterPackage.RefNetProcessId, uiKomuterPackage);
                }
                else if (e.KioskDataPack is UIKomuterTicketBookingAck uiKomuterTickBook)
                {
                    SendInternalCommand(uiKomuterTickBook.ProcessId, uiKomuterTickBook.RefNetProcessId, uiKomuterTickBook);
                }
                else if (e.KioskDataPack is UIKomuterBookingCheckoutAck uiKomuterBookChk)
                {
                    SendInternalCommand(uiKomuterBookChk.ProcessId, uiKomuterBookChk.RefNetProcessId, uiKomuterBookChk);
                }
                else if (e.KioskDataPack is UIKomuterCompletePaymentAck uiKomuterPay)
                {
                    SendInternalCommand(uiKomuterPay.ProcessId, uiKomuterPay.RefNetProcessId, uiKomuterPay);
                }
                else if (e.KioskDataPack is UISalesBookingTimeoutExtensionResult uiBkExtRes)
                {
                    SendInternalCommand(uiBkExtRes.ProcessId, uiBkExtRes.RefNetProcessId, uiBkExtRes);
                }
                else if (e.KioskDataPack is UICustInfoPNRTicketTypeAck uiPnrTicketType)
                {
                    SendInternalCommand(uiPnrTicketType.ProcessId, uiPnrTicketType.RefNetProcessId, uiPnrTicketType);
                }
                else
                {
                    //CYA-PENDING-CODE -- Need to avoid running this area. -- Pending to remove
                    MessageType msgTyp = string.IsNullOrWhiteSpace(e.KioskDataPack.ErrorMessage) ? MessageType.NormalType : MessageType.ErrorType;
                    RaiseOnShowResultMessage(e.NetProcessId, e.KioskDataPack, msgTyp);
                }
            }
            else
                RaiseOnShowResultMessage(e.NetProcessId, null, MessageType.ErrorType, e.Message);

            Log.LogText(LogChannel, _currProcessId, $@"End - _b2bAccess_OnSendMessage; Net Process Id:{e?.NetProcessId}", "A10", classNMethodName: "ServerSalesApplication._b2bAccess_OnSendMessage");
        }

        private SemaphoreSlim _asyncSendLock = new SemaphoreSlim(1);

        /// <summary>
        /// FuncCode:EXIT62.0103
        /// </summary>
        public async Task<bool> SendInternalCommand(string processId, Guid? netProcessId, IKioskMsg svcMsg)
        {
            bool lockSuccess = false;
            try
            {
                lockSuccess = await _asyncSendLock.WaitAsync(5 * 60 * 1000);

                if (lockSuccess == false)
                    return false;

                Log.LogText(LogChannel, "-", svcMsg, "A01", "ServerSalesApplication.SendInternalCommand", netProcessId: netProcessId, 
                    extraMsg: $@"Start - SendInternalCommand; MsgObj: {svcMsg.GetType().ToString()}");

                // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- 
                // Setup Session & Count Down
                {
                    if (svcMsg.Instruction == (CommInstruction)UISalesInst.ClientMaintenanceRequest)
                    {
                        _appCountDown.Abort();
                        _SessionSuper.CleanNetProcessId();
                        _SessionSuper.AddNetProcessId(netProcessId.Value);
                    }

                    else if ((svcMsg.Instruction == (CommInstruction)UISalesInst.CheckOutstandingCardSettlementRequest)
                        || (svcMsg.Instruction == (CommInstruction)UISalesInst.CardSettlementSubmission)
                        || (svcMsg.Instruction == (CommInstruction)UISalesInst.CheckOutstandingCardSettlementAck)
                        || (svcMsg.Instruction == (CommInstruction)UISalesInst.CardSettlementStatusAck))
                    {
                        /*By Pass*/
                        string tt1 = "Test-Here-X01"; 
                    }

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.ClientMaintenanceFinishedSubmission)
                    {
                        _clientAppUnderMaintenance = false;
                    }

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.SalesEndSessionRequest)
                    {
                        //if (netProcessId.HasValue == false)
                        //    throw new Exception("Fail to start CountDown; NetProcessID Not found; (EXIT213xx)");

                        //_clientAppUnderMaintenance = false;

                        if ((_session != null) && (_session.PaymentState != AppDecorator.Common.PaymentResult.Success))
                            ReleaseSeat(svcMsg.ProcessId, svcMsg.RefNetProcessId, _session);

                        _session.SessionReset();

                        _appCountDown.EndSession();
                        _SessionSuper.CleanNetProcessId();
                        return true;
                    }

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.CountDownStartRequest)
                    {
                        if (netProcessId.HasValue == false)
                            throw new Exception("Fail to start CountDown; NetProcessID Not found; (EXIT21321)");

                        _clientAppUnderMaintenance = false;

                        _session.NewSession(netProcessId.Value);
                        
                        _appCountDown.SetNewCountDown(_maxProcessPeriodSec, _session.SessionId);
                        _SessionSuper.CleanNetProcessId();
                        _SessionSuper.AddNetProcessId(netProcessId.Value);
                    }

                    else if ((svcMsg is IGnReq) &&
                            (svcMsg is UIReq<UIxGetMachineLastRebootTimeRequest>)
                        )
                    {
                        try
                        {
                            InternalCommandParameters parm = new InternalCommandParameters(processId, netProcessId, svcMsg);
                            RunThreadMan tMan = new RunThreadMan(new ParameterizedThreadStart(GetMachineLastRebootTimeThreadWorking), 
                                parm, "ServerSalesApplication.GetMachineLastRebootTimeThreadWorking", 
                                20, LogChannel, ThreadPriority.AboveNormal);
                        }
                        catch (Exception ex)
                        {
                            string msg = ex.Message;
                        }
                    }

                    else if (
                            (svcMsg.Instruction == (CommInstruction)UISalesInst.ServerApplicationStatusRequest)
                            || (svcMsg.Instruction == (CommInstruction)UISalesInst.WebServerLogonRequest)
                            || (svcMsg.Instruction == (CommInstruction)UISalesInst.WebServerLogonStatusAck)
                            )
                    {
                        _clientAppUnderMaintenance = false;
                        /*By Pass*/
                    }
                    else if (svcMsg is UITimeoutChangeRequest)
                    {
                        /*By Pass*/
                    }
                    else if (svcMsg is UIRestartMachineRequest)
                    {
                        try
                        {
                            Thread tWorker = new Thread(new ThreadStart(ShutdownThreadWorking));
                            tWorker.IsBackground = true;
                            tWorker.Priority = ThreadPriority.Highest;
                            tWorker.Start();
                        }
                        catch (Exception ex)
                        {
                            string msg = ex.Message;
                        }
                    }
                    else if ((_session.SessionId.Equals(Guid.Empty) == false))
                    {
                        if ((_session.Expired == false))
                        {
                            _clientAppUnderMaintenance = false;

                            if ((svcMsg is UICustPromoCodeVerifyRequest) || (svcMsg is UICustPromoCodeVerifyAck))
                            {
                                _SessionSuper.AddNetProcessId(netProcessId.Value);
                                // ..no timeout update needed.
                            }

                            else if (_appCountDown.UpdateCountDown(_maxProcessPeriodSec, _session.SessionId, isMandatoryExtensionChange: false, $@"First Init SendInternalCommand; {svcMsg.GetType()}", out bool isMatchedSession1) == false)
                            {
                                if (isMatchedSession1 == false)
                                    throw new Exception($@"Unable to update Count Down; (EXIT21330); Current App Session ID {_session.SessionId.ToString("D")} is not matched with count down.");
                                else
                                    throw new Exception($@"Unable to update Count Down; (EXIT21331); Session ID {_session.SessionId.ToString("D")} already expired.");
                            }

                            else
                            {
                                _SessionSuper.AddNetProcessId(netProcessId.Value);
                            }
                        }
                        else
                            throw new Exception($@"Unable to update Count Down; (EXIT21332); Session ID {_session.SessionId.ToString("D")} already expired.");
                    }
                }
                // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- 

                if (svcMsg.Instruction == (CommInstruction)UISalesInst.ClientMaintenanceRequest)
                    SendMaintenanceAck(processId, netProcessId);

                else if (svcMsg.Instruction == (CommInstruction)UISalesInst.ClientMaintenanceFinishedSubmission)
                { /*By Pass*/  }

                // ----- Card Settlement ----- 
                else if (svcMsg.Instruction == (CommInstruction)UISalesInst.CheckOutstandingCardSettlementRequest)
                {
                    CheckOutstandingCardSettlement(processId, netProcessId);
                }
                else if (svcMsg.Instruction == (CommInstruction)UISalesInst.CardSettlementSubmission)
                {
                    SubmitCardSettlement(processId, netProcessId, svcMsg);
                }
                else if ((svcMsg.Instruction == (CommInstruction)UISalesInst.CheckOutstandingCardSettlementAck)
                    || (svcMsg.Instruction == (CommInstruction)UISalesInst.CardSettlementStatusAck)
                    )
                {
                    RedirectDataToClient(processId, netProcessId, svcMsg, _session);
                }
                // ----- ----- ----- ----- ----- 

                else if (svcMsg.Instruction == (CommInstruction)UISalesInst.ServerApplicationStatusRequest)
                    GetServerStatus(processId, netProcessId);

                else if (svcMsg.Instruction == (CommInstruction)UISalesInst.RestartMachine)
                { /**/ }

                // -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- 

                else if (svcMsg.Instruction == (CommInstruction)UISalesInst.WebServerLogonRequest)
                    ReLogon(processId, netProcessId);

                else if (svcMsg.Instruction == (CommInstruction)UISalesInst.WebServerLogonStatusAck)
                    RedirectDataToClient(processId, netProcessId, svcMsg, _session);

                else
                {
                    // Below is User Session Processing // Note : For non-user data process, do not put on the below. 
                    //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

                    if (_session.SessionId.Equals(Guid.Empty))
                        throw new Exception("Empty (EXIT21322).");

                    else if (_session.Expired)
                        throw new Exception("Session Expired (EXIT21323).");

                    //if (svcMsg is UIDetailEditRequest uiEdit)
                    //{
                    //    _session = _svrAppPlan.SetEditingSession(_session, uiEdit.EditItemCode);
                    //}
                    //else 
                    if (svcMsg is UIPageNavigateRequest uiPgNav)
                    {
                        _session = _svrAppPlan.SetUIPageNavigateSession(_session, uiPgNav);
                    }
                    else
                        _session = _svrAppPlan.UpdateUserSession(_session, svcMsg);

                    Log.LogText(LogChannel, _session.SessionId.ToString("D"), _session, "SESSION_1", "ServerSalesApplication.SendInternalCommand", 
                        extraMsg: $@"Inst: {svcMsg.Instruction}; Inst.Desc.:{svcMsg.InstructionDesc} ;MsgObj: UserSession");

                    //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

                    UISalesInst inst = _svrAppPlan.NextInstruction(processId, netProcessId, svcMsg, _session, out bool releaseSeatRequestOnEdit);

                    if (releaseSeatRequestOnEdit)
                    {
                        ReleaseSeatOnEdit(processId, netProcessId, _session);
                    }

                    if (inst == UISalesInst.LanguageSelectionAck)
                    {
                        _session.CurrentEditMenuItemCode = null;
                        SelectLanguageSendAck(processId, netProcessId, svcMsg);
                    }

                    else if (inst == UISalesInst.TimeoutChangeRequest)
                    {
                        ChangeTimeout(_session, (UITimeoutChangeRequest)svcMsg);
                    }

                    else if (inst == UISalesInst.CounterConfigurationRequest)
                    {
                        _session.CurrentEditMenuItemCode = TickSalesMenuItemCode.FromStation;
                        GetCounterConfiguration(processId, netProcessId);
                    }

                    else if (inst == UISalesInst.OriginListRequest)
                    {
                        _session.CurrentEditMenuItemCode = TickSalesMenuItemCode.FromStation;
                        GetOriginStationList(processId, netProcessId);
                    }

                    else if (inst == UISalesInst.DestinationListRequest)
                    {
                        _session.CurrentEditMenuItemCode = TickSalesMenuItemCode.ToStation;
                        GetDestinationList(processId, netProcessId, _session.OriginStationCode);
                    }

                    else if (inst == UISalesInst.TravelDatesEnteringAck)
                    {
                        _session.CurrentEditMenuItemCode = TickSalesMenuItemCode.TravelDates;
                        SelectTravelDatesSendAck(processId, netProcessId);
                    }

                    else if (inst == UISalesInst.DepartTripListInitAck)
                    {
                        _session.CurrentEditMenuItemCode = TickSalesMenuItemCode.DepartTrip;
                        InitDepartTripSendAck(processId, netProcessId, _session.DepartPassengerDepartDateTime.Value);
                    }

                    else if (inst == UISalesInst.DepartTripListRequest)
                    {
                        _session.CurrentEditMenuItemCode = TickSalesMenuItemCode.DepartTrip;
                        if (svcMsg is UIDepartTripListRequest uiDTripRq)
                        {
                            GetDepartTripList(processId, netProcessId, uiDTripRq.PassengerDepartDate, _session.OriginStationCode, _session.DestinationStationCode);
                        }
                    }
                    //else if (inst == UISalesInst.DepartTripSubmissionErrorAck)
                    //{
                    //    _session.CurrentEditMenuItemCode = TickSalesMenuItemCode.DepartOperator;
                    //    DepartTripSubmissionErrorAck(processId, netProcessId, svcMsg.ErrorMessage);
                    //}
                    else if (inst == UISalesInst.DepartSeatListRequest)
                    {
                        _session.CurrentEditMenuItemCode = TickSalesMenuItemCode.DepartSeat;
                        GetDepartSeatList(processId, netProcessId, _session.DepartTripId);
                    }

                    else if (inst == UISalesInst.DepartSeatConfirmRequest)
                    {
                        _session.CurrentEditMenuItemCode = TickSalesMenuItemCode.DepartSeat;
                        DepartConfirmSeat(processId, netProcessId, _session);
                    }

                    else if (inst == UISalesInst.DepartSeatConfirmFailAck)
                    {
                        _session.CurrentEditMenuItemCode = TickSalesMenuItemCode.DepartSeat;
                        DepartConfirmSeatFailSendAck(processId, netProcessId, _session, svcMsg);
                    }

                    //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                    //Return
                    else if (inst == UISalesInst.ReturnTripListInitAck)
                    {
                        _session.CurrentEditMenuItemCode = TickSalesMenuItemCode.ReturnTrip;
                        InitReturnTripSendAck(processId, netProcessId, _session.ReturnPassengerDepartDateTime.Value);
                    }

                    else if (inst == UISalesInst.ReturnTripListRequest)
                    {
                        _session.CurrentEditMenuItemCode = TickSalesMenuItemCode.ReturnTrip;
                        if (svcMsg is UIReturnTripListRequest uiRTripRq)
                        {
                            GetReturnTripList(processId, netProcessId, uiRTripRq.PassengerDepartDate, _session.DestinationStationCode, _session.OriginStationCode);
                        }
                    }

                    //else if (inst == UISalesInst.ReturnTripSubmissionErrorAck)
                    //{
                    //    _session.CurrentEditMenuItemCode = TickSalesMenuItemCode.ReturnTrip;
                    //    ReturnTripSubmissionErrorAck(processId, netProcessId, svcMsg.ErrorMessage);
                    //}

                    else if (inst == UISalesInst.ReturnSeatListRequest)
                    {
                        _session.CurrentEditMenuItemCode = TickSalesMenuItemCode.ReturnSeat;
                        GetReturnSeatList(processId, netProcessId, _session.ReturnTripId);
                    }

                    else if (inst == UISalesInst.ReturnSeatConfirmRequest)
                    {
                        _session.CurrentEditMenuItemCode = TickSalesMenuItemCode.ReturnSeat;
                        ReturnConfirmSeat(processId, netProcessId, _session);
                    }

                    else if (inst == UISalesInst.ReturnSeatConfirmFailAck)
                    {
                        _session.CurrentEditMenuItemCode = TickSalesMenuItemCode.ReturnSeat;
                        ReturnConfirmSeatFailSendAck(processId, netProcessId, _session, svcMsg);
                    }
                    
                    //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                    //CustInfoPrerequisiteRequest
                    else if (inst == UISalesInst.CustInfoPrerequisiteRequest)
                    {
                        _session.CurrentEditMenuItemCode = TickSalesMenuItemCode.Passenger;
                        GetCustInfoPrerequisite(processId, netProcessId, _session);
                    }

                    else if (inst == UISalesInst.CustInfoPNRTicketTypeRequest)
                    {
                        _session.CurrentEditMenuItemCode = TickSalesMenuItemCode.Passenger;
                        GetCustInfoPNRTicketTypeRequest(processId, netProcessId, _session, svcMsg);
                    }

                    else if (inst == UISalesInst.CustInfoAck)
                    {
                        _session.CurrentEditMenuItemCode = TickSalesMenuItemCode.Passenger;
                        PassengerInfoEntrySendAck(processId, netProcessId, _session, (UICustInfoPrerequisiteAck)svcMsg);
                    }

                    else if (inst == UISalesInst.CustPromoCodeVerifyRequest)
                    {
                        _session.CurrentEditMenuItemCode = TickSalesMenuItemCode.Passenger;
                        VerifyCustPromoCode(processId, netProcessId, _session, svcMsg);
                    }

                    else if (inst == UISalesInst.CustInfoUpdateELSEReleaseSeatRequest)
                    {
                        _session.CurrentEditMenuItemCode = TickSalesMenuItemCode.Passenger;
                        UpdateCustInfoUpdateV2(processId, netProcessId, _session);
                    }

                    else if (inst == UISalesInst.CustInfoUpdateFailAck)
                    {
                        _session.CurrentEditMenuItemCode = TickSalesMenuItemCode.Passenger;
                        CustInfoUpdateFailSendAck(processId, netProcessId, _session, svcMsg);
                    }

                    else if (inst == UISalesInst.ETSInsuranceListRequest)
                    {
                        _session.CurrentEditMenuItemCode = TickSalesMenuItemCode.Insurance;
                        GetETSInsuranceList(processId, netProcessId, _session);
                    }

                    else if (inst == UISalesInst.ETSInsuranceSubmissionRequest)
                    {
                        _session.CurrentEditMenuItemCode = TickSalesMenuItemCode.Insurance;
                        SubmitETSInsurance(processId, netProcessId, _session, svcMsg);
                    }

                    else if (inst == UISalesInst.ETSCheckoutSaleRequest)
                    {
                        _session.CurrentEditMenuItemCode = TickSalesMenuItemCode.Payment;
                        CheckoutSale(processId, netProcessId, _session);
                    }

                    else if (inst == UISalesInst.ETSCheckoutSaleFailAck)
                    {
                        _session.CurrentEditMenuItemCode = TickSalesMenuItemCode.Payment;
                        ReleaseSeat(processId, netProcessId, _session);
                        FailCheckoutSaleAck(processId, netProcessId, _session);
                    }

                    else if (inst == UISalesInst.SalesPaymentProceedAck)
                    {
                        _session.CurrentEditMenuItemCode = TickSalesMenuItemCode.Payment;
                        MakeSalesPaymentSendAck(processId, netProcessId, _session);
                    }

                    else if (inst == UISalesInst.CompleteTransactionElseReleaseSeatRequest)
                    {
                        _session.CurrentEditMenuItemCode = TickSalesMenuItemCode.AfterPayment;
                        CompleteTransactionRequest(processId, netProcessId, _session, svcMsg);
                    }

                    else if (inst == UISalesInst.SeatReleaseRequest)
                    {
                        ReleaseSeat(processId, netProcessId, _session);
                    }

                    //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                    // KTM - Komuter
                    else if (inst == UISalesInst.StartSellingAck)
                    {
                        _session.CurrentEditMenuItemCode = TickSalesMenuItemCode.StartSelling;
                        StartSellingAck(processId, netProcessId, _session);
                    }

                    else if (inst == UISalesInst.KomuterResetUserSessionRequest)
                    {
                        _session.CurrentEditMenuItemCode = TickSalesMenuItemCode.StartSelling;

                        _appCountDown.SetNewCountDown(_maxProcessPeriodSec, _session.SessionId);
                        _SessionSuper.CleanNetProcessId();
                        //_SessionSuper.AddNetProcessId(netProcessId.Value);
                        /* Process End Here */
                    }

                    else if (inst == UISalesInst.KomuterTicketTypePackageRequest)
                    {
                        _session.CurrentEditMenuItemCode = TickSalesMenuItemCode.StartSelling;
                        GetKomuterTicketTypePackage(processId, netProcessId, _session, svcMsg);
                    }

                    else if (inst == UISalesInst.KomuterTicketBookingRequest)
                    {
                        _session.CurrentEditMenuItemCode = TickSalesMenuItemCode.Payment;
                        GetKomuterTicketBooking(processId, netProcessId, _session, svcMsg);
                    }

                    else if (inst == UISalesInst.KomuterBookingCheckoutRequest)
                    {
                        _session.CurrentEditMenuItemCode = TickSalesMenuItemCode.Payment;
                        CheckoutKomuterTicketBooking(processId, netProcessId, _session, svcMsg);
                    }

                    else if (inst == UISalesInst.KomuterCompletePaymentSubmission)
                    {
                        _session.CurrentEditMenuItemCode = TickSalesMenuItemCode.Payment;
                        SubmitKomuterBookingPayment(processId, netProcessId, _session, svcMsg);
                    }

                    //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

                    else if (inst == UISalesInst.RedirectDataToClient)
                    {
                        if (svcMsg.Instruction == (CommInstruction)UISalesInst.OriginListAck)
                        {
                            _session.CurrentEditMenuItemCode = TickSalesMenuItemCode.FromStation;
                        }
                        else if (svcMsg.Instruction == (CommInstruction)UISalesInst.DestinationListAck)
                        {
                            _session.CurrentEditMenuItemCode = TickSalesMenuItemCode.ToStation;
                        }
                        else if (svcMsg.Instruction == (CommInstruction)UISalesInst.DepartTripListAck)
                        {
                            _session.CurrentEditMenuItemCode = TickSalesMenuItemCode.DepartTrip;
                        }
                        else if (svcMsg.Instruction == (CommInstruction)UISalesInst.DepartSeatListAck)
                        {
                            _session.CurrentEditMenuItemCode = TickSalesMenuItemCode.DepartSeat;
                        }
                        else if (svcMsg.Instruction == (CommInstruction)UISalesInst.ReturnTripListAck)
                        {
                            _session.CurrentEditMenuItemCode = TickSalesMenuItemCode.ReturnTrip;
                        }
                        else if (svcMsg.Instruction == (CommInstruction)UISalesInst.ReturnSeatListAck)
                        {
                            _session.CurrentEditMenuItemCode = TickSalesMenuItemCode.ReturnSeat;
                        }
                        else if (svcMsg.Instruction == (CommInstruction)UISalesInst.CustPromoCodeVerifyAck)
                        {
                            _session.CurrentEditMenuItemCode = TickSalesMenuItemCode.Passenger;
                        }
                        else if (svcMsg.Instruction == (CommInstruction)UISalesInst.CustInfoPNRTicketTypeAck)
                        {
                            _session.CurrentEditMenuItemCode = TickSalesMenuItemCode.Passenger;
                        }
                        else if (svcMsg.Instruction == (CommInstruction)UISalesInst.ETSInsuranceListAck)
                        {
                            _session.CurrentEditMenuItemCode = TickSalesMenuItemCode.Insurance;
                        }
                        else if (svcMsg.Instruction == (CommInstruction)UISalesInst.CompleteTransactionResult)
                        {
                            _session.CurrentEditMenuItemCode = TickSalesMenuItemCode.AfterPayment;
                        }
                        else if (svcMsg.Instruction == (CommInstruction)UISalesInst.KomuterTicketTypePackageAck)
                        {
                            _session.CurrentEditMenuItemCode = TickSalesMenuItemCode.DepartTrip;
                        }
                        else if (svcMsg.Instruction == (CommInstruction)UISalesInst.KomuterTicketBookingAck)
                        {
                            _session.CurrentEditMenuItemCode = TickSalesMenuItemCode.DepartTrip;
                        }
                        else if (svcMsg.Instruction == (CommInstruction)UISalesInst.KomuterBookingCheckoutAck)
                        {
                            _session.CurrentEditMenuItemCode = TickSalesMenuItemCode.Payment;
                        }
                        else if (svcMsg.Instruction == (CommInstruction)UISalesInst.KomuterCompletePaymentAck)
                        {
                            _session.CurrentEditMenuItemCode = TickSalesMenuItemCode.AfterPayment;

                            _appCountDown.UpdateBookingTimeout(_session.SessionId, bookingId: "*", null, isUpdateTimeoutSuccess: false);
                        }

                        RedirectDataToClient(processId, netProcessId, svcMsg, _session);
                    }

                    else if (inst == UISalesInst.CountDownPausedRequest)
                    {
                        PauseCountDown(processId, netProcessId, _session);
                    }

                    else if (inst == UISalesInst.CountDownExpiredAck)
                    {
                        TimeoutSession(processId, netProcessId, _session);
                    }

                    else if (inst == UISalesInst.SalesBookingTimeoutExtensionRequest)
                    {
                        ExtendingBookingTimeout(processId, netProcessId, _session, svcMsg);
                    }

                    else if (inst == UISalesInst.SalesBookingTimeoutExtensionResult)
                    {
                        UpdateBookingTimeoutExtension(processId, netProcessId, _session, svcMsg);
                    }

                    else
                    {
                        Log.LogText(LogChannel, processId,
                            svcMsg,
                            $@"EXXX01",
                            classNMethodName: "ServerSalesApplication.SendInternalCommand",
                            messageType: AppDecorator.Log.MessageType.Error,
                            extraMsg: $@"Unregconized Instruction Code; Net Process Id: {netProcessId}; Next Sales Instruction: {Enum.GetName(typeof(UISalesInst), inst)}; MsgObj: IUISvcMsg");

                        bool ret1 = _appCountDown.UpdateCountDown(_maxProcessPeriodSec, _session.SessionId, isMandatoryExtensionChange: false, $@"OutOfExpectation; {svcMsg.GetType().ToString()}",out bool isMatchedSession2);
                    }
                }

                Log.LogText(LogChannel, _currProcessId, $@"Done - SendInternalCommand; Net Process Id:{netProcessId}", $@"A03", classNMethodName: "ServerSalesApplication.SendInternalCommand");
            }
            catch (Exception ex)
            {
                Log.LogText(LogChannel, processId, svcMsg, "EX01", "ServerSalesApplication.SendInternalCommand", AppDecorator.Log.MessageType.Error, extraMsg: "MsgObj: IKioskMsg", netProcessId: netProcessId);
                Log.LogError(LogChannel, processId, ex, "E02", "ServerSalesApplication.SendInternalCommand", netProcessId: netProcessId);
            }
            finally
            {
                if ((lockSuccess == true) && (_asyncSendLock.CurrentCount == 0))
                    _asyncSendLock.Release();

                Log.LogText(LogChannel, "-", $@"End - SendInternalCommand", "A20", "ServerSalesApplication.SendInternalCommand", netProcessId: netProcessId);
            }

            return true;

            //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
            //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

            void GetMachineLastRebootTimeThreadWorking(object tParam)
            {
                if (tParam is InternalCommandParameters parm)
                {
                    MessageType msgTyp = MessageType.ErrorType;
                    UIAck<UIxGnAppAck<KioskLastRebootTimeEcho>> resultAck = null;
                    UIxGnAppAck<KioskLastRebootTimeEcho> uIx = null;

                    try
                    {
                        KioskLastRebootTimeEcho res = _kioskStatesJobMan.ReadKioskLastRebootTime();
                        uIx = new UIxGnAppAck<KioskLastRebootTimeEcho>(parm.NetProcessId, parm.ProcessId, res);
                        msgTyp = MessageType.NormalType;
                    }
                    catch (Exception ex)
                    {
                        uIx = new UIxGnAppAck<KioskLastRebootTimeEcho>(parm.NetProcessId, parm.ProcessId, ex);
                        Log.LogError(LogChannel, processId, new Exception($@"Error found. Net Process Id: {netProcessId}", ex), "EX05", "ServerSalesApplication.GetMachineLastRebootTimeThreadWorking");
                    }

                    resultAck = new UIAck<UIxGnAppAck<KioskLastRebootTimeEcho>>(parm.NetProcessId, parm.ProcessId, _appModule, DateTime.Now, uIx);

                    RaiseOnShowResultMessage(parm.NetProcessId, resultAck, msgTyp);
                }
            }

            void ShutdownThreadWorking()
            {
                try
                {
                    _kioskStatesJobMan.UpdateKioskLastRebootTime(DateTime.Now, out _);

                    var cmd = new System.Diagnostics.ProcessStartInfo("shutdown.exe", "-r -f -t 0");
                    cmd.CreateNoWindow = true;
                    cmd.UseShellExecute = false;
                    cmd.ErrorDialog = false;
                    System.Diagnostics.Process.Start(cmd);
                }
                catch (Exception ex)
                {
                    Log.LogError(LogChannel, processId, new Exception($@"Error found. Net Process Id: {netProcessId}", ex), "EX05", "ServerSalesApplication.ShutdownThreadWorking");
                }
            }

            void RedirectDataToClient(string procId, Guid? netProcId, IKioskMsg kioskMsg, UserSession userSession)
            {
                if ((kioskMsg is UICompleteTransactionResult payRes) 
                    && (payRes.ProcessState == ProcessResult.Fail) 
                    && (string.IsNullOrWhiteSpace(userSession?.DepartPendingReleaseTransNo) == false)
                    )
                {
                    ReleaseSeat(procId, netProcId, userSession);
                }

                if ((kioskMsg is UIKomuterBookingCheckoutAck UiKomBkChkout) && (UiKomBkChkout.MessageData is KomuterBookingCheckoutResult komBkChkout))
                {
                    if ((komBkChkout.Code.Equals(WebAPIAgent.ApiCodeOK)) && (komBkChkout.Data.Error?.Equals(YesNo.No) == true))
                    {
                        DateTime expireTime = DateTime.Now.AddSeconds(komBkChkout.Data.BookingRemainingInSecond);
                        _appCountDown.UpdateBookingTimeout(_session.SessionId, _session.SeatBookingId, expireTime, isUpdateTimeoutSuccess: true);
                    }
                    else
                    {
                        _appCountDown.UpdateBookingTimeout(_session.SessionId, _session.SeatBookingId, null, isUpdateTimeoutSuccess: false);
                    }
                }

                MessageType msgTyp = string.IsNullOrWhiteSpace(kioskMsg.ErrorMessage) ? MessageType.NormalType : MessageType.ErrorType;
                RaiseOnShowResultMessage(netProcId, kioskMsg, msgTyp);
            }

            void SendMaintenanceAck(string procId, Guid? netProcId)
            {
                try
                {
                    if (_disposed)
                        throw new Exception("System is shutting down (EXIT21306);");

                    UISalesClientMaintenanceAck mtnAck = new UISalesClientMaintenanceAck(netProcId, procId, DateTime.Now);
                    RaiseOnShowResultMessage(netProcId, mtnAck, MessageType.NormalType);
                }
                catch (Exception ex)
                {
                    Log.LogError(LogChannel, processId, new Exception($@"Error found. Net Process Id: {netProcessId}", ex), "E01", "ServerSalesApplication.SendMaintenanceAck");
                }
            }

            /// <summary>
            /// FuncCode:EXIT62.018A
            /// </summary>
            void GetServerStatus(string procId, Guid? netProcId)
            {
                UIServerApplicationStatusAck stt = null;
                try
                {
                    AppDecorator.Config.Setting setting = NssIT.Kiosk.AppDecorator.Config.Setting.GetSetting();
                    WebAPISiteCode webApiCode = setting.WebAPICode;
                    _svrAccess.QueryServerStatus(out bool isServerDisposed, out bool isServerShutdown, out UICounterConfigurationResult machineConfig);

                    if ((isServerShutdown == false) && (isServerDisposed == false))
                    {
                        if (machineConfig?.IsResultSuccess == true) 
                        {
                            string bTngWebApiVerStr = PaymentGuard.SectionVersion;
                            WebAPISiteCode bTngWebApiCode = WebAPISiteCode.Unknown;

                            // Check Parameter Web API Code
                            if (webApiCode == WebAPISiteCode.Unknown)
                            {
                                stt = new UIServerApplicationStatusAck(netProcId, procId, DateTime.Now, setting.ApplicationVersion, isServerDisposed, isServerShutdown,
                                machineConfig.MessageData, setting.KioskId, setting.WebApiURL, webApiCode, isResultSuccess: false)
                                { ErrorMessage = "Local Server is not ready; Unknown Web API Code; (EXIT62.018A.X01)" };
                            }

                            // Read BTnG Setting
                            if ((stt is null) 
                                && (string.IsNullOrWhiteSpace(bTngWebApiVerStr) == false) 
                                && (Enum.TryParse<WebAPISiteCode>(bTngWebApiVerStr, out bTngWebApiCode) == false))
                            {
                                stt = new UIServerApplicationStatusAck(netProcId, procId, DateTime.Now, setting.ApplicationVersion, isServerDisposed, isServerShutdown,
                                machineConfig.MessageData, setting.KioskId, setting.WebApiURL, webApiCode, isResultSuccess: false)
                                { ErrorMessage = "Local Server is not ready; Fail reading for Payment Gateway Web API Section; (EXIT62.018A.X02)" };
                            }

                            // Compare BTnG Web API and Parameter Web API
                            if ((stt is null)
                                && (bTngWebApiCode != WebAPISiteCode.Unknown)
                                && (bTngWebApiCode != webApiCode) 
                                && (webApiCode != WebAPISiteCode.Local_Host)
                                )
                            {
                                stt = new UIServerApplicationStatusAck(netProcId, procId, DateTime.Now, setting.ApplicationVersion, isServerDisposed, isServerShutdown,
                                machineConfig.MessageData, setting.KioskId, setting.WebApiURL, webApiCode, isResultSuccess: false)
                                { ErrorMessage = "Local Server is not ready; Payment Gateway Web API Section mismatched; (EXIT62.018A.X03)" };
                            }

                            if (stt is null)
                            {
                                stt = new UIServerApplicationStatusAck(netProcId, procId, DateTime.Now, setting.ApplicationVersion, isServerDisposed, isServerShutdown,
                                            machineConfig.MessageData, machineConfig.MachineId, setting.WebApiURL, webApiCode, isResultSuccess: true);

                                if ((_svrAppPlan is KTMBAppPlan ktmbPlan) && (machineConfig.MessageData is CounterConfigCompiledResult contRes))
                                    ktmbPlan.UpdateMachineConfig(contRes);
                            }
                        }
                        else if (machineConfig is null)
                        {
                            stt = new UIServerApplicationStatusAck(netProcId, procId, DateTime.Now, setting.ApplicationVersion, isServerDisposed, isServerShutdown, 
                                null, setting.KioskId, setting.WebApiURL, webApiCode, isResultSuccess: false)
                                { ErrorMessage = "Local Server is not ready; (EXIT21302)" };
                        }
                        else 
                        {
                            string errMsg = string.IsNullOrWhiteSpace(machineConfig?.ErrorMessage) ? "Local Server having unknown error; (EXIT62.018A.X04)" : machineConfig.ErrorMessage;

                            stt = new UIServerApplicationStatusAck(netProcId, procId, DateTime.Now, setting.ApplicationVersion, isServerDisposed, isServerShutdown, 
                                machineConfig?.MessageData, setting.KioskId, setting.WebApiURL, webApiCode, isResultSuccess: false)
                                { ErrorMessage = errMsg };
                        }
                    }
                    else
                    {
                        string errMsg = string.IsNullOrWhiteSpace(machineConfig?.ErrorMessage) ? "Local Server having unknown error; (EXIT62.018A.X05)" : machineConfig.ErrorMessage;
                        errMsg = $@"Local Server have been shutdowned; {errMsg}";
                        stt = new UIServerApplicationStatusAck(netProcId, procId, DateTime.Now, setting.ApplicationVersion, isServerDisposed, isServerShutdown,
                                null, setting.KioskId, setting.WebApiURL, webApiCode, isResultSuccess: false)
                                { ErrorMessage = errMsg };
                    }

                    RaiseOnShowResultMessage(netProcId, stt, MessageType.NormalType);
                }
                catch (Exception ex)
                {
                    string err = $@"{ex.Message}; (EXIT62.018A.EX01)";
                    stt = new UIServerApplicationStatusAck(netProcId, procId, DateTime.Now, "XXERRXX", false, false,
                            null, "", "-", WebAPISiteCode.Unknown, isResultSuccess: false)
                            { ErrorMessage = err };

                    Log.LogError(LogChannel, processId, new Exception(err, ex), "E01", "ServerSalesApplication.GetServerStatus");
                    RaiseOnShowResultMessage(netProcId, stt, MessageType.ErrorType, err);
                }
            }

            //void DepartTripSubmissionErrorAck(string procId, Guid? netProcId, string errorMessage)
            //{
            //    try
            //    {
            //        IKioskMsg msg = new UIDepartTripSubmissionErrorAck(netProcId, procId, DateTime.Now, errorMessage);
            //        RaiseOnShowResultMessage(netProcId, msg, MessageType.ErrorType);
            //    }
            //    catch (Exception ex)
            //    {
            //        Log.LogError(LogChannel, processId, ex, "E01", "ServerSalesApplication.DepartTripSubmissionErrorAck");
            //    }
            //}

            void ReLogon(string procId, Guid? netProcId)
            {
                try
                {
                    if (_disposed)
                        throw new Exception("System is shutting down (EXIT21313);");

                    LogonCommand command = new LogonCommand(procId, netProcId);

                    Log.LogText(LogChannel, procId, $@"Start - ReLogon; Net Process Id:{netProcId}", $@"A02", classNMethodName: "ServerSalesApplication.ReLogon");

                    bool addCommandResult = _svrAccess.AddCommand(new AccessCommandPack(command), out string errorMsg);

                    if (addCommandResult == false)
                    {
                        if (string.IsNullOrWhiteSpace(errorMsg) == false)
                            throw new Exception(errorMsg);
                        else
                            throw new Exception("Unknown error (EXIT21314).");
                    }
                }
                catch (Exception ex)
                {
                    RaiseOnShowResultMessage(netProcId, null, MessageType.ErrorType, "Server error; Error when sending internal service command (Logon again). " + ex.Message);
                }
            }

            void CheckOutstandingCardSettlement(string procId, Guid? netProcId)
            {
                try
                {
                    if (_disposed)
                        throw new Exception("System is shutting down (EXIT21336);");

                    CheckOutstandingCardSettlementCommand command = new CheckOutstandingCardSettlementCommand(procId, netProcId);

                    Log.LogText(LogChannel, procId, $@"Start - CheckOutstandingCardSettlement; Net Process Id:{netProcId}", $@"A02", classNMethodName: "ServerSalesApplication.CheckOutstandingCardSettlement");

                    bool addCommandResult = _svrAccess.AddCommand(new AccessCommandPack(command), out string errorMsg);

                    if (addCommandResult == false)
                    {
                        if (string.IsNullOrWhiteSpace(errorMsg) == false)
                            throw new Exception(errorMsg);
                        else
                            throw new Exception("Unknown error (EXIT21337).");
                    }
                }
                catch (Exception ex)
                {
                    RaiseOnShowResultMessage(netProcId, null, MessageType.ErrorType, "Server error; Error when sending internal service command (Check Outstanding Card Settlement); " + ex.Message);
                }
            }

            void SubmitCardSettlement(string procId, Guid? netProcId, IKioskMsg kioskMsg)
            {
                try
                {
                    if (_disposed)
                        throw new Exception("System is shutting down (EXIT21401);");

                    if (kioskMsg is UISalesCardSettlementSubmission settSubm)
                    {
                        CardSettlementCommand command = new CardSettlementCommand(procId, netProcId
                            , settSubm.HostNo
                            , settSubm.BatchNumber
                            , settSubm.BatchCount
                            , settSubm.BatchCurrencyAmount
                            , settSubm.StatusCode
                            , settSubm.MachineId
                            , settSubm.ErrorMessage);

                        Log.LogText(LogChannel, procId, $@"Start - SubmitCardSettlement; Net Process Id:{netProcId}", $@"A02", classNMethodName: "ServerSalesApplication.SubmitCardSettlement");

                        bool addCommandResult = _svrAccess.AddCommand(new AccessCommandPack(command), out string errorMsg);

                        if (addCommandResult == false)
                        {
                            if (string.IsNullOrWhiteSpace(errorMsg) == false)
                                throw new Exception(errorMsg);
                            else
                                throw new Exception("Unknown error (EXIT21402).");
                        }
                    }
                    else
                        throw new Exception($@"Unexpected Kiosk Message Instant({kioskMsg.GetType().ToString()}); (EXIT21403).");
                }
                catch (Exception ex)
                {
                    RaiseOnShowResultMessage(netProcId, null, MessageType.ErrorType, "Server error; Error when sending internal service command (Submit Card Settlement Submission); " + ex.Message);
                }
            }

            void SelectLanguageSendAck(string procId, Guid? netProcId, IKioskMsg kioskMsg)
            {
                try
                {
                    if (_disposed)
                        throw new Exception("System is shutting down (EXIT21315);");

                    if ((kioskMsg is UICounterConfigurationResult config) && (((CounterConfigCompiledResult)config.MessageData)).Code.Equals(WebAPIAgent.ApiCodeOK) == false)
                    {
                        CounterConfigCompiledResult res = (CounterConfigCompiledResult)config.MessageData;
                        string errorMsg = res.MessageString() ?? "";

                        if (string.IsNullOrWhiteSpace(errorMsg))
                            errorMsg = config.ErrorMessage ?? "";

                        if (string.IsNullOrWhiteSpace(errorMsg))
                            errorMsg = "Error when reading counter setting; (EXIT21390)";

                        UILanguageSelectionAck lang = new UILanguageSelectionAck(netProcId, procId, DateTime.Now) { ErrorMessage = errorMsg };
                        RaiseOnShowResultMessage(netProcId, lang, MessageType.NormalType);
                    }
                    else
                    {
                        UILanguageSelectionAck lang = new UILanguageSelectionAck(netProcId, procId, DateTime.Now);
                        RaiseOnShowResultMessage(netProcId, lang, MessageType.NormalType);
                    }
                }
                catch (Exception ex)
                {
                    Log.LogError(LogChannel, processId, new Exception($@"Error found. Net Process Id: {netProcessId}", ex), "E01", "ServerSalesApplication.SelectLanguage");
                }
            }

            void GetCounterConfiguration(string procId, Guid? netProcId)
            {
                try
                {
                    if (_disposed)
                        throw new Exception("System is shutting down (EXIT21396);");

                    CounterConfigurationRequestCommand command = new CounterConfigurationRequestCommand(procId, netProcId);

                    Log.LogText(LogChannel, procId, $@"Start - GetCounterConfiguration; Net Process Id:{netProcId}", $@"A02", classNMethodName: "ServerSalesApplication.GetCounterConfiguration");

                    bool addCommandResult = _svrAccess.AddCommand(new AccessCommandPack(command), out string errorMsg);

                    if (addCommandResult == false)
                    {
                        if (string.IsNullOrWhiteSpace(errorMsg) == false)
                            throw new Exception(errorMsg);
                        else
                            throw new Exception("Unknown error (EXIT21397).");
                    }
                }
                catch (Exception ex)
                {
                    RaiseOnShowResultMessage(netProcId, null, MessageType.ErrorType, "Server error; Error when sending internal service command (Get Counter Configuration); " + ex.Message);
                }
            }

            void GetOriginStationList(string procId, Guid? netProcId)
            {
                try
                {
                    if (_disposed)
                        throw new Exception("System is shutting down (EXIT21333);");

                    OriginListRequestCommand command = new OriginListRequestCommand(procId, netProcId);

                    Log.LogText(LogChannel, procId, $@"Start - GetOriginStationList; Net Process Id:{netProcId}", $@"A02", classNMethodName: "ServerSalesApplication.GetOriginStationList");

                    bool addCommandResult = _svrAccess.AddCommand(new AccessCommandPack(command), out string errorMsg);

                    if (addCommandResult == false)
                    {
                        if (string.IsNullOrWhiteSpace(errorMsg) == false)
                            throw new Exception(errorMsg);
                        else
                            throw new Exception("Unknown error (EXIT21334).");
                    }
                }
                catch (Exception ex)
                {
                    RaiseOnShowResultMessage(netProcId, null, MessageType.ErrorType, "Server error; Error when sending internal service command (Get Origin Station List); " + ex.Message);
                }
            }

            void GetDestinationList(string procId, Guid? netProcId, string originStationId)
            {
                try
                {
                    if (_disposed)
                        throw new Exception("System is shutting down (EXIT21311);");

                    DestinationListRequestCommand command = new DestinationListRequestCommand(procId, netProcId, originStationId);

                    Log.LogText(LogChannel, procId, $@"Start - GetDestinationList; Net Process Id:{netProcId}", $@"A02", classNMethodName: "ServerSalesApplication.GetDestinationList");

                    bool addCommandResult = _svrAccess.AddCommand(new AccessCommandPack(command), out string errorMsg);

                    if (addCommandResult == false)
                    {
                        if (string.IsNullOrWhiteSpace(errorMsg) == false)
                            throw new Exception(errorMsg);
                        else
                            throw new Exception("Unknown error (EXIT21312).");
                    }
                }
                catch (Exception ex)
                {
                    RaiseOnShowResultMessage(netProcId, null, MessageType.ErrorType, "Server error; Error when sending internal service command (Get Destination List). " + ex.Message);
                }
            }

            void InitDepartTripSendAck(string procId, Guid? netProcId, DateTime tripDate)
            {
                try
                {
                    if (_disposed)
                        throw new Exception("System is shutting down (EXIT21345);");

                    UIDepartTripInitAck tripInit = new UIDepartTripInitAck(netProcId, procId, tripDate, AppDecorator.Common.TravelMode.DepartOnly);
                    RaiseOnShowResultMessage(netProcId, tripInit, MessageType.NormalType);
                }
                catch (Exception ex)
                {
                    Log.LogError(LogChannel, processId, new Exception($@"Error found. Net Process Id: {netProcessId}", ex), "E01", "ServerSalesApplication.InitDepartTrip");
                }
            }

            void GetDepartTripList(string procId, Guid? netProcId, DateTime tripDate, string fromStationCode, string toStationCode)
            {
                try
                {
                    if (_disposed)
                        throw new Exception("System is shutting down (EXIT21341);");
                    
                    DepartTripListCommand command = new DepartTripListCommand(procId, netProcId, tripDate, fromStationCode, toStationCode);

                    Log.LogText(LogChannel, procId, $@"Start - GetDepartTripList; Net Process Id:{netProcId}", $@"A02", classNMethodName: "ServerSalesApplication.GetDepartTripList");

                    bool addCommandResult = _svrAccess.AddCommand(new AccessCommandPack(command), out string errorMsg);

                    if (addCommandResult == false)
                    {
                        if (string.IsNullOrWhiteSpace(errorMsg) == false)
                            throw new Exception(errorMsg);
                        else
                            throw new Exception("Unknown error (EXIT21342).");
                    }
                }
                catch (Exception ex)
                {
                    RaiseOnShowResultMessage(netProcId, null, MessageType.ErrorType, "Server error; Error when sending internal service command (Get Depart Trip List). " + ex.Message);
                }
            }

            void GetDepartSeatList(string procId, Guid? netProcId, string tripId)
            {
                try
                {
                    if (_disposed)
                        throw new Exception("System is shutting down (EXIT21346);");

                    DepartSeatListCommand command = new DepartSeatListCommand(procId, netProcId, tripId);

                    Log.LogText(LogChannel, procId, $@"Start - GetDepartSeatList; Net Process Id:{netProcId}", $@"A02", classNMethodName: "ServerSalesApplication.GetDepartSeatList");

                    bool addCommandResult = _svrAccess.AddCommand(new AccessCommandPack(command), out string errorMsg);

                    if (addCommandResult == false)
                    {
                        if (string.IsNullOrWhiteSpace(errorMsg) == false)
                            throw new Exception(errorMsg);
                        else
                            throw new Exception("Unknown error (EXIT21347).");
                    }
                }
                catch (Exception ex)
                {
                    RaiseOnShowResultMessage(netProcId, null, MessageType.ErrorType, "Server error; Error when sending internal service command (Get Depart Seat List). " + ex.Message);
                }
            }

            void SelectTravelDatesSendAck(string procId, Guid? netProcId)
            {
                try
                {
                    if (_disposed)
                        throw new Exception("System is shutting down (EXIT21310);");

                    UITravelDatesEnteringAck tvDates = new UITravelDatesEnteringAck(netProcId, procId, DateTime.Now);
                    RaiseOnShowResultMessage(netProcId, tvDates, MessageType.NormalType);
                }
                catch (Exception ex)
                {
                    Log.LogError(LogChannel, processId, new Exception($@"Error found. Net Process Id: {netProcessId}", ex), "E01", "ServerSalesApplication.SelectTravelDates");
                }
            }

            void DepartConfirmSeat(string procId, Guid? netProcId, UserSession session)
            {
                try
                {
                    if (_disposed)
                        throw new Exception("System is shutting down (EXIT21385);");

                    DepartSeatConfirmCommand command = new DepartSeatConfirmCommand(procId, netProcId, "", session.DepartTrainSeatModelId, session.DepartPassengerSeatDetailList);

                    Log.LogText(LogChannel, procId, $@"Start - DepartConfirmSeat; Net Process Id:{netProcId}", $@"A02", classNMethodName: "ServerSalesApplication.DepartConfirmSeat");

                    bool addCommandResult = _svrAccess.AddCommand(new AccessCommandPack(command), out string errorMsg);

                    if (addCommandResult == false)
                    {
                        if (string.IsNullOrWhiteSpace(errorMsg) == false)
                            throw new Exception(errorMsg);
                        else
                            throw new Exception("Unknown error (EXIT21386).");
                    }
                }
                catch (Exception ex)
                {
                    RaiseOnShowResultMessage(netProcId, null, MessageType.ErrorType, "Server error; Error when sending internal service command (Confirm Depart Seat). " + ex.Message);
                }
            }

            void DepartConfirmSeatFailSendAck(string procId, Guid? netProcId, UserSession session, IKioskMsg kioskMsg)
            {
                try
                {
                    if (_disposed)
                        throw new Exception("System is shutting down (EXIT21316);");

                    string errMsg = null;
                    if (kioskMsg is UIDepartSeatConfirmResult kMsg)
                    {
                        errMsg = $@"{kMsg.ErrorMessage}";
                        if (kMsg.MessageData is BookingResult br)
                        {
                            errMsg += $@"; {br.MessageString()}";
                        }
                    }

                    UIDepartSeatConfirmFailAck uiFail = new UIDepartSeatConfirmFailAck(netProcId, procId, DateTime.Now) { ErrorMessage = errMsg };
                    RaiseOnShowResultMessage(netProcId, uiFail, MessageType.NormalType);
                }
                catch (Exception ex)
                {
                    Log.LogError(LogChannel, processId, new Exception($@"Error found. Net Process Id: {netProcessId}", ex), "E01", "ServerSalesApplication.DepartConfirmSeatFailSendAck");
                }
            }

            void InitReturnTripSendAck(string procId, Guid? netProcId, DateTime tripDate)
            {
                try
                {
                    if (_disposed)
                        throw new Exception("System is shutting down (EXIT21377);");

                    UIReturnTripInitAck tripInit = new UIReturnTripInitAck(netProcId, procId, tripDate, AppDecorator.Common.TravelMode.ReturnOnly);
                    RaiseOnShowResultMessage(netProcId, tripInit, MessageType.NormalType);
                }
                catch (Exception ex)
                {
                    Log.LogError(LogChannel, processId, new Exception($@"Error found. Net Process Id: {netProcessId}", ex), "E01", "ServerSalesApplication.InitDepartTrip");
                }
            }

            void GetReturnTripList(string procId, Guid? netProcId, DateTime tripDate, string fromStationCode, string toStationCode)
            {
                try
                {
                    if (_disposed)
                        throw new Exception("System is shutting down (EXIT21379);");

                    ReturnTripListCommand command = new ReturnTripListCommand(procId, netProcId, tripDate, fromStationCode, toStationCode);

                    Log.LogText(LogChannel, procId, $@"Start - GetReturnTripList; Net Process Id:{netProcId}", $@"A02", classNMethodName: "ServerSalesApplication.GetReturnTripList");

                    bool addCommandResult = _svrAccess.AddCommand(new AccessCommandPack(command), out string errorMsg);

                    if (addCommandResult == false)
                    {
                        if (string.IsNullOrWhiteSpace(errorMsg) == false)
                            throw new Exception($@"{errorMsg}; (EXIT21380)");
                        else
                            throw new Exception("Unknown error (EXIT21381)");
                    }
                }
                catch (Exception ex)
                {
                    RaiseOnShowResultMessage(netProcId, null, MessageType.ErrorType, "Server error; Error when sending internal service command (Get Return Trip List). " + ex.Message);
                }
            }

            void GetReturnSeatList(string procId, Guid? netProcId, string tripId)
            {
                try
                {
                    if (_disposed)
                        throw new Exception("System is shutting down (EXIT21369);");

                    ReturnSeatListCommand command = new ReturnSeatListCommand(procId, netProcId, tripId);

                    Log.LogText(LogChannel, procId, $@"Start - GetReturnSeatList; Net Process Id:{netProcId}", $@"A02", classNMethodName: "ServerSalesApplication.GetReturnSeatList");

                    bool addCommandResult = _svrAccess.AddCommand(new AccessCommandPack(command), out string errorMsg);

                    if (addCommandResult == false)
                    {
                        if (string.IsNullOrWhiteSpace(errorMsg) == false)
                            throw new Exception($@"(EXIT21370);{errorMsg}");
                        else
                            throw new Exception("Unknown error (EXIT21371).");
                    }
                }
                catch (Exception ex)
                {
                    RaiseOnShowResultMessage(netProcId, null, MessageType.ErrorType, "Server error; Error when sending internal service command (Get Return Seat List). " + ex.Message);
                }
            }

            void ReturnConfirmSeat(string procId, Guid? netProcId, UserSession session)
            {
                try
                {
                    if (_disposed)
                        throw new Exception("System is shutting down (EXIT21373);");

                    ReturnSeatConfirmCommand command = new ReturnSeatConfirmCommand(procId, netProcId, session.SeatBookingId, session.ReturnTrainSeatModelId, session.ReturnPassengerSeatDetailList);

                    Log.LogText(LogChannel, procId, $@"Start - GetReturnSeatList; Net Process Id:{netProcId}", $@"A02", classNMethodName: "ServerSalesApplication.GetReturnSeatList");

                    bool addCommandResult = _svrAccess.AddCommand(new AccessCommandPack(command), out string errorMsg);

                    if (addCommandResult == false)
                    {
                        if (string.IsNullOrWhiteSpace(errorMsg) == false)
                            throw new Exception($@"(EXIT21374) {errorMsg}");
                        else
                            throw new Exception("Unknown error (EXIT21375).");
                    }
                }
                catch (Exception ex)
                {
                    RaiseOnShowResultMessage(netProcId, null, MessageType.ErrorType, "Server error; Error when sending internal service command (Confirm Return Seat). " + ex.Message);
                }
            }

            void ReturnConfirmSeatFailSendAck(string procId, Guid? netProcId, UserSession session, IKioskMsg kioskMsg)
            {
                try
                {
                    if (_disposed)
                        throw new Exception("System is shutting down (EXIT21383);");

                    string errMsg = null;
                    if (kioskMsg is UIReturnSeatConfirmResult kMsg)
                    {
                        errMsg = $@"{kMsg.ErrorMessage}";
                        if (kMsg.MessageData is BookingResult br)
                        {
                            errMsg += $@"; {br.MessageString()}";
                        }
                    }

                    UIReturnSeatConfirmFailAck uiFail = new UIReturnSeatConfirmFailAck(netProcId, procId, DateTime.Now) { ErrorMessage = errMsg };
                    RaiseOnShowResultMessage(netProcId, uiFail, MessageType.NormalType);
                }
                catch (Exception ex)
                {
                    Log.LogError(LogChannel, processId, new Exception($@"Error found. Net Process Id: {netProcessId}", ex), "E01", "ServerSalesApplication.ReturnConfirmSeatFailSendAck");
                }
            }

            //GetKomuterTicketTypePackage(processId, netProcessId, _session);
            void GetKomuterTicketTypePackage(string procId, Guid? netProcId, UserSession session, IKioskMsg kioskMsg)
            {
                try
                {
                    if (_disposed)
                        throw new Exception("System is shutting down (EXIT21319);");

                    UIKomuterTicketTypePackageRequest packReq = (UIKomuterTicketTypePackageRequest)kioskMsg;
                    KomuterTicketTypePackageCommand command = new KomuterTicketTypePackageCommand(procId, netProcId, packReq.OriginStationId, packReq.DestinationStationId);

                    Log.LogText(LogChannel, procId, $@"Start - GetKomuterTicketTypePackage; Net Process Id:{netProcId}", $@"A02", classNMethodName: "ServerSalesApplication.GetKomuterTicketTypePackage");

                    bool addCommandResult = _svrAccess.AddCommand(new AccessCommandPack(command), out string errorMsg);

                    if (addCommandResult == false)
                    {
                        if (string.IsNullOrWhiteSpace(errorMsg) == false)
                            throw new Exception(errorMsg);
                        else
                            throw new Exception("Unknown error (EXIT21324).");
                    }
                }
                catch (Exception ex)
                {
                    RaiseOnShowResultMessage(netProcId, null, MessageType.ErrorType, "Server error; Error when sending internal service command (Get Depart Trip List). " + ex.Message);
                }
            }

            void VerifyCustPromoCode(string procId, Guid? netProcId, UserSession session, IKioskMsg kioskMsg)
            {
                try
                {
                    if (_disposed)
                        throw new Exception("System is shutting down (EXIT21343);");

                    UICustPromoCodeVerifyRequest promoVf = (UICustPromoCodeVerifyRequest)kioskMsg;
                    CustPromoCodeVerifyCommand command = new CustPromoCodeVerifyCommand(procId, netProcId, promoVf.TrainSeatModelId, promoVf.SeatLayoutModelId, promoVf.TicketTypesId, promoVf.PassengerIC, promoVf.PromoCode);

                    Log.LogText(LogChannel, procId, $@"Start - VerifyCustPromoCode; Net Process Id:{netProcId}", $@"A02", classNMethodName: "ServerSalesApplication.VerifyCustPromoCode");

                    bool addCommandResult = _svrAccess.AddCommand(new AccessCommandPack(command), out string errorMsg);

                    if (addCommandResult == false)
                    {
                        if (string.IsNullOrWhiteSpace(errorMsg) == false)
                            throw new Exception(errorMsg);
                        else
                            throw new Exception("Unknown error (EXIT21344).");
                    }
                }
                catch (Exception ex)
                {
                    RaiseOnShowResultMessage(netProcId, null, MessageType.ErrorType, "Server error; Error when sending internal service command (Verify Cust Promo Code). " + ex.Message);
                }
            }

            void GetCustInfoPrerequisite(string procId, Guid? netProcId, UserSession session)
            {
                try
                {
                    if (_disposed)
                        throw new Exception("System is shutting down (EXIT21387);");

                    CustInfoPrerequisiteRequestCommand command = new CustInfoPrerequisiteRequestCommand(procId, netProcId, "");

                    Log.LogText(LogChannel, procId, $@"Start - GetCustInfoPrerequisite; Net Process Id:{netProcId}", $@"A02", classNMethodName: "ServerSalesApplication.GetCustInfoPrerequisite");

                    bool addCommandResult = _svrAccess.AddCommand(new AccessCommandPack(command), out string errorMsg);

                    if (addCommandResult == false)
                    {
                        if (string.IsNullOrWhiteSpace(errorMsg) == false)
                            throw new Exception($@"{errorMsg}; (EXIT21388)");
                        else
                            throw new Exception("Unknown error (EXIT21389)");
                    }
                }
                catch (Exception ex)
                {
                    RaiseOnShowResultMessage(netProcId, null, MessageType.ErrorType, "Server error; Error when sending internal service command (Get Custumer Info Prerequisite). " + ex.Message);
                }
            }

            void GetCustInfoPNRTicketTypeRequest(string procId, Guid? netProcId, UserSession session, IKioskMsg kioskMsg)
            {
                try
                {
                    if (_disposed)
                        throw new Exception("System is shutting down (EXIT21406);");

                    if (kioskMsg is UICustInfoPNRTicketTypeRequest uiReq)
                    {
                        CustInfoPNRTicketTypeRequestCommand command = new CustInfoPNRTicketTypeRequestCommand(procId, netProcId, 
                            uiReq.BookingId, uiReq.PassenggerIdentityNo, uiReq.TripScheduleSeatLayoutDetails_Ids);

                        Log.LogText(LogChannel, procId, $@"Start - GetCustInfoPrerequisite; Net Process Id:{netProcId}", $@"A02", classNMethodName: "ServerSalesApplication.GetCustInfoPrerequisite");

                        bool addCommandResult = _svrAccess.AddCommand(new AccessCommandPack(command), out string errorMsg);

                        if (addCommandResult == false)
                        {
                            if (string.IsNullOrWhiteSpace(errorMsg) == false)
                                throw new Exception($@"{errorMsg}; (EXIT21407)");
                            else
                                throw new Exception("Unknown error (EXIT21408)");
                        }
                    }
                }
                catch (Exception ex)
                {
                    RaiseOnShowResultMessage(netProcId, null, MessageType.ErrorType, "Server error; Error when sending internal service command (Get Custumer PNR Ticket Type Info). " + ex.Message);
                }
            }

            void PassengerInfoEntrySendAck(string procId, Guid? netProcId, UserSession session, UICustInfoPrerequisiteAck kioskMsg)
            {
                try
                {
                    if (_disposed)
                        throw new Exception("System is shutting down (EXIT21317);");

                    UICustInfoAck uiCust = new UICustInfoAck(netProcId, procId, DateTime.Now, kioskMsg.MessageData);
                    RaiseOnShowResultMessage(netProcId, uiCust, MessageType.NormalType);
                }
                catch (Exception ex)
                {
                    Log.LogError(LogChannel, processId, new Exception($@"Error found. Net Process Id: {netProcessId}", ex), "E01", "ServerSalesApplication.PassengerInfoEntry");
                }
            }

            void UpdateCustInfoUpdateV2(string procId, Guid? netProcId, UserSession session)
            {
                try
                {
                    if (_disposed)
                        throw new Exception("System is shutting down (EXIT21351);");

                    CustInfoUpdateCommand command = 
                        new CustInfoUpdateCommand
                        (procId, netProcId, 
                        session.DepartPassengerSeatDetailList, session.ReturnPassengerSeatDetailList, 
                        session.SeatBookingId);

                    Log.LogText(LogChannel, procId, $@"Start - UpdateCustInfoUpdateV2; Net Process Id:{netProcId}", $@"A02", classNMethodName: "ServerSalesApplication.UpdateCustInfoUpdateV2");

                    bool addCommandResult = _svrAccess.AddCommand(new AccessCommandPack(command), out string errorMsg);

                    if (addCommandResult == false)
                    {
                        if (string.IsNullOrWhiteSpace(errorMsg) == false)
                            throw new Exception(errorMsg);
                        else
                            throw new Exception("Unknown error (EXIT21352).");
                    }
                }
                catch (Exception ex)
                {
                    RaiseOnShowResultMessage(netProcId, null, MessageType.ErrorType, "Server error; Error when sending internal service command (Get Depart Seat List). " + ex.Message);
                }
            }

            void CustInfoUpdateFailSendAck(string procId, Guid? netProcId, UserSession session, IKioskMsg kioskMsg)
            {
                try
                {
                    if (_disposed)
                        throw new Exception("System is shutting down (EXIT21356);");

                    if ((session.IsRequestAmendPassengerInfo) && (kioskMsg is UIDepartCustInfoUpdateResult res))
                    {
                        UICustInfoUpdateFailAck uiFail = new UICustInfoUpdateFailAck(netProcId, procId, DateTime.Now, res.MessageData);
                        RaiseOnShowResultMessage(netProcId, uiFail, MessageType.NormalType);
                    }
                    else
                    {
                        string errorMsg = "";
                        if (string.IsNullOrWhiteSpace(session.PassengerInfoUpdateFailMessage) == false)
                        {
                            errorMsg = session.PassengerInfoUpdateFailMessage;
                        }
                        else
                            errorMsg = "Unabled to update passenger info";

                        UICustInfoUpdateFailAck uiFail = new UICustInfoUpdateFailAck(netProcId, procId, DateTime.Now, errorMsg);
                        RaiseOnShowResultMessage(netProcId, uiFail, MessageType.NormalType);
                        ReleaseSeat(procId, netProcId, session);
                    }                    
                }
                catch (Exception ex)
                {
                    Log.LogError(LogChannel, processId, new Exception($@"Error found. Net Process Id: {netProcessId}", ex), "E01", "ServerSalesApplication.CustInfoUpdateFailSendAck");
                }
            }

            void GetETSInsuranceList(string procId, Guid ? netProcId, UserSession session)
            {
                try
                {
                    if (_disposed)
                        throw new Exception("System is shutting down (EXIT21501);");

                    GetETSInsuranceCommand command =
                        new GetETSInsuranceCommand(procId, netProcId, session.SeatBookingId);

                    Log.LogText(LogChannel, procId, $@"Start - Get ETS Insurance Access Command; Net Process Id:{netProcId}", $@"A02", classNMethodName: "ServerSalesApplication.GetETSInsuranceList");

                    bool addCommandResult = _svrAccess.AddCommand(new AccessCommandPack(command), out string errorMsg);

                    if (addCommandResult == false)
                    {
                        if (string.IsNullOrWhiteSpace(errorMsg) == false)
                            throw new Exception(errorMsg);
                        else
                            throw new Exception("Unknown error (EXIT21501).");
                    }
                }
                catch (Exception ex)
                {
                    string errMsg = "Server error; Error when sending internal service command (Get ETS Insurance Access Command); (EXIT21501); " + ex.Message;
                    Log.LogError(LogChannel, processId, new Exception(errMsg, ex), "E01", "ServerSalesApplication.GetETSInsuranceList", netProcessId : netProcId);
                }
            }

            void SubmitETSInsurance(string procId, Guid? netProcId, UserSession session, IKioskMsg kioskMsg)
            {
                try
                {
                    if (_disposed)
                        throw new Exception("System is shutting down (EXIT21886);");

                    if (kioskMsg is UIETSInsuranceSubmissionRequest req)
                    {
                        ETSInsuranceSubmissionCommand command =
                        new ETSInsuranceSubmissionCommand(procId, netProcId, req.TransactionNo, req.InsuranceHeadersId);

                        Log.LogText(LogChannel, procId, $@"Start - Submit ETS Insurance with Access Command; Net Process Id:{netProcId}", $@"A02", classNMethodName: "ServerSalesApplication.SubmitETSInsurance");

                        bool addCommandResult = _svrAccess.AddCommand(new AccessCommandPack(command), out string errorMsg);

                        if (addCommandResult == false)
                        {
                            if (string.IsNullOrWhiteSpace(errorMsg) == false)
                                throw new Exception(errorMsg);
                            else
                                throw new Exception("Unknown error (EXIT21887)");
                        }
                    }
                }
                catch (Exception ex)
                {
                    string errMsg = "Server error; Error when sending internal service command (Get ETS Insurance Access Command); (EXIT21888); " + ex.Message;
                    Log.LogError(LogChannel, processId, new Exception(errMsg, ex), "E01", "ServerSalesApplication.GetETSInsuranceList", netProcessId: netProcId);
                }
            }

            void CheckoutSale(string procId, Guid? netProcId, UserSession session)
            {
                try
                {
                    if (_disposed)
                        throw new Exception("System is shutting down (EXIT21376);");

                    int totalSeatCount = 0;

                    if (session.DepartPassengerSeatDetailList?.Length > 0)
                        totalSeatCount += session.DepartPassengerSeatDetailList.Length;

                    if (session.ReturnPassengerSeatDetailList?.Length > 0)
                        totalSeatCount += session.ReturnPassengerSeatDetailList.Length;

                    ETSCheckoutSaleCommand command =
                        new ETSCheckoutSaleCommand(procId, netProcId, session.SeatBookingId, totalSeatCount);

                    Log.LogText(LogChannel, procId, $@"Start - Check out ETS/Intercity sale; Net Process Id:{netProcId}", $@"A02", classNMethodName: "ServerSalesApplication.CheckoutSale");

                    bool addCommandResult = _svrAccess.AddCommand(new AccessCommandPack(command), out string errorMsg);

                    if (addCommandResult == false)
                    {
                        if (string.IsNullOrWhiteSpace(errorMsg) == false)
                            throw new Exception(errorMsg);
                        else
                            throw new Exception("Unknown error (EXIT21378).");
                    }
                }
                catch (Exception ex)
                {
                    string errMsg = "Server error; Error when sending internal service command (Check out ETS/Intercity Sale); (EXIT21882); " + ex.Message;
                    UIETSCheckoutSaleFailAck errUi = new UIETSCheckoutSaleFailAck(netProcId, procId, DateTime.Now, errMsg);
                    RaiseOnShowResultMessage(netProcId, errUi, MessageType.ErrorType, errMsg);
                }
            }

            void FailCheckoutSaleAck(string procId, Guid? netProcId, UserSession session)
            {
                try
                {
                    if (_disposed)
                        throw new Exception("System is shutting down (EXIT21885);");

                    if (string.IsNullOrWhiteSpace(session.ETSIntercityCheckoutFailMessage) == false)
                    {
                        UIETSCheckoutSaleFailAck errUi = new UIETSCheckoutSaleFailAck(netProcId, procId, DateTime.Now, session.ETSIntercityCheckoutFailMessage);
                        RaiseOnShowResultMessage(netProcId, errUi, MessageType.ErrorType, session.ETSIntercityCheckoutFailMessage);
                    }
                    else
                    {
                        UIETSCheckoutSaleFailAck errUi = new UIETSCheckoutSaleFailAck(netProcId, procId, DateTime.Now, "Unable to check out sale properly; (EXIT21883)");
                        RaiseOnShowResultMessage(netProcId, errUi, MessageType.ErrorType, session.ETSIntercityCheckoutFailMessage);
                    }
                }
                catch (Exception ex)
                {
                    string errMsg = "Unable to check out sale properly; (EXIT21884)" + ex.Message;
                    UIETSCheckoutSaleFailAck errUi = new UIETSCheckoutSaleFailAck(netProcId, procId, DateTime.Now, errMsg);
                    RaiseOnShowResultMessage(netProcId, errUi, MessageType.ErrorType, errMsg);
                }
            }

            void StartSellingAck(string procId, Guid? netProcId, UserSession session)
            {
                try
                {
                    if (_disposed)
                        throw new Exception("System is shutting down (EXIT21391);");

                    UISalesStartSellingAck uiStart = new UISalesStartSellingAck(netProcId, procId, DateTime.Now);
                    RaiseOnShowResultMessage(netProcId, uiStart, MessageType.NormalType);
                }
                catch (Exception ex)
                {
                    Log.LogError(LogChannel, processId, new Exception($@"Error found. Net Process Id: {netProcessId}", ex), "E01", "ServerSalesApplication.StartSellingAck");
                }
            }

            void MakeSalesPaymentSendAck(string procId, Guid? netProcId, UserSession session)
            {
                try
                {
                    if (_disposed)
                        throw new Exception("System is shutting down (EXIT21356);");

                    //SalesPaymentData salesData = new SalesPaymentData() { TransactionNo = session.DepartSeatConfirmTransNo, Amount = session.DepartTotalAmount };

                    _appCountDown.UpdateBookingTimeout(_session.SessionId, _session.SeatBookingId, _session.BookingExpiredDateTime, isUpdateTimeoutSuccess: true);

                    UISalesPaymentProceedAck uiPay = new UISalesPaymentProceedAck(netProcId, procId, DateTime.Now, _session.BookingExpiredDateTime.Value);
                    RaiseOnShowResultMessage(netProcId, uiPay, MessageType.NormalType);

                    //--------------------------------------------
                }
                catch (Exception ex)
                {
                    Log.LogError(LogChannel, processId, new Exception($@"Error found. Net Process Id: {netProcessId}", ex), "E01", "ServerSalesApplication.MakeSalesPaymentSendAck");
                }
            }

            void ReleaseSeatOnEdit(string procId, Guid? netProcId, UserSession session)
            {
                try
                {
                    if (_disposed)
                        throw new Exception("System is shutting down (EXIT21357);");

                    if (string.IsNullOrWhiteSpace(session.DepartPendingReleaseTransNo))
                        throw new Exception("No Transaction Number found when release seat (EXIT21358);");

                    TicketReleaseCommand command = new TicketReleaseCommand(procId, netProcId, session.DepartPendingReleaseTransNo);

                    Log.LogText(LogChannel, procId, $@"Start - ReleaseSeat; Net Process Id:{netProcId}", $@"A02", classNMethodName: "ServerSalesApplication.ReleaseSeat");

                    bool addCommandResult = _svrAccess.AddCommand(new AccessCommandPack(command), out string errorMsg);

                    session.SeatBookingId = null;
                    session.DepartPendingReleaseTransNo = null;
                    if (addCommandResult == false)
                    {
                        if (string.IsNullOrWhiteSpace(errorMsg) == false)
                            throw new Exception(errorMsg);
                        else
                            throw new Exception("Unknown error (EXIT21359).");
                    }
                }
                catch (Exception ex)
                {
                    //RaiseOnShowResultMessage(netProcId, null, MessageType.ErrorType, "Server error; Error when sending internal service command (Release Seat). " + ex.Message);
                    Log.LogError(LogChannel, processId, new Exception($@"Error found. Net Process Id: {netProcessId}", ex), "E01", "ServerSalesApplication.ReleaseSeat");
                }
            }

            void CompleteTransactionRequest(string procId, Guid? netProcId, UserSession session, IKioskMsg kioskMsg)
            {
                try
                {
                    if (_disposed)
                        throw new Exception("System is shutting down (EXIT21360);");

                    _appCountDown.UpdateBookingTimeout(_session.SessionId, "*", null, isUpdateTimeoutSuccess: false);

                    UISalesPaymentSubmission paySubm = (UISalesPaymentSubmission)kioskMsg;
                    CompleteTransactionElseReleaseSeatCommand command = null;

                    if (paySubm.TypeOfPayment == PaymentType.PaymentGateway)
                    {
                        command = new CompleteTransactionElseReleaseSeatCommand(procId, netProcId,
                                paySubm.SeatBookingId, paySubm.TradeCurrency, session.GrossTotal, paySubm.BTnGSaleTransactionNo, paySubm.PaymentMethod);
                    }

                    // Default PaymentType is CreditCard
                    else
                    {
                        command = new CompleteTransactionElseReleaseSeatCommand(procId, netProcId,
                                paySubm.SeatBookingId, paySubm.TradeCurrency, session.GrossTotal, paySubm.BankReferenceNo, paySubm.CreditCardAnswer);
                    }

                    Log.LogText(LogChannel, procId, $@"Start - CompleteTransactionRequest; Net Process Id:{netProcId}", $@"A02", classNMethodName: "ServerSalesApplication.CompleteTransactionRequest");

                    bool addCommandResult = _svrAccess.AddCommand(new AccessCommandPack(command), out string errorMsg);

                    if (addCommandResult == false)
                    {
                        if (string.IsNullOrWhiteSpace(errorMsg) == false)
                            throw new Exception(errorMsg);
                        else
                            throw new Exception("Unknown error (EXIT21361).");
                    }
                }
                catch (Exception ex)
                {
                    RaiseOnShowResultMessage(netProcId, null, MessageType.ErrorType, "Server error; Error when sending internal service command (Completing Transaction Request). " + ex.Message);
                }
            }

            void ChangeTimeout(UserSession session, UITimeoutChangeRequest changeReq)
            {
                try
                {
                    if (_disposed)
                        throw new Exception("System is shutting down (EXIT21372);");

                    if (changeReq.ChangeMode == TimeoutChangeMode.ResetNormalTimeout)
                    {
                        _appCountDown.ResetTimeoutWarning();
                        _appCountDown.ResetTimeout(session.SessionId, out bool isMatchedSession);
                    }
                    else if (changeReq.ChangeMode == TimeoutChangeMode.MandatoryExtension)
                        _appCountDown.ClientChangeTimeout(changeReq.ChangeMode, changeReq.ExtensionTimeSec, session.SessionId, "MandatoryExtension", out bool isMatchedSession);

                    else if (changeReq.ChangeMode == TimeoutChangeMode.RemoveMandatoryTimeout)
                        _appCountDown.ClientChangeTimeout(changeReq.ChangeMode, 0, session.SessionId, "RemoveMandatoryTimeout", out bool isMatchedSession);

                    string tt1 = "";
                }
                catch (Exception ex)
                {
                    Log.LogError(LogChannel, processId, new Exception($@"Error found. Net Process Id: {netProcessId}", ex), "E01", "ServerSalesApplication.ChangeTimeout");
                }
            }

            void ReleaseSeat(string procId, Guid? netProcId, UserSession session)
            {
                try
                {
                    if (_disposed)
                        throw new Exception("System is shutting down (EXIT21362);");

                    if (string.IsNullOrWhiteSpace(session.DepartPendingReleaseTransNo) && string.IsNullOrWhiteSpace(session.SeatBookingId))
                        throw new Exception("No Transaction Number found when release seat (EXIT21363);");

                    if (session.PaymentState == AppDecorator.Common.PaymentResult.Success)
                        throw new Exception("Payment Success. Release Seat not allowed (EXIT21392);");

                    string bookingId = null;
                    
                    if (string.IsNullOrWhiteSpace(session.DepartPendingReleaseTransNo) == false)
                        bookingId = session.DepartPendingReleaseTransNo;

                    else if (string.IsNullOrWhiteSpace(session.SeatBookingId) == false)
                        bookingId = session.SeatBookingId;
                    
                    if (string.IsNullOrWhiteSpace(bookingId) == false)
                    {
                        TicketReleaseCommand command = new TicketReleaseCommand(procId, netProcId, bookingId);

                        Log.LogText(LogChannel, procId, $@"Start - ReleaseSeat; Net Process Id:{netProcId}", $@"A02", classNMethodName: "ServerSalesApplication.ReleaseSeat");

                        bool addCommandResult = _svrAccess.AddCommand(new AccessCommandPack(command), out string errorMsg);

                        session.SeatBookingId = null;
                        session.DepartPendingReleaseTransNo = null;
                        if (addCommandResult == false)
                        {
                            if (string.IsNullOrWhiteSpace(errorMsg) == false)
                                throw new Exception(errorMsg);
                            else
                                throw new Exception("Unknown error (EXIT21364).");
                        }
                    }
                }
                catch (Exception ex)
                {
                    //RaiseOnShowResultMessage(netProcId, null, MessageType.ErrorType, "Server error; Error when sending internal service command (Release Seat). " + ex.Message);
                    Log.LogError(LogChannel, processId, new Exception($@"Error found. Net Process Id: {netProcessId}", ex), "E01", "ServerSalesApplication.ReleaseSeat");
                }
            }

            void GetKomuterTicketBooking(string procId, Guid? netProcId, UserSession session, IKioskMsg kioskMsg)
            {
                if (kioskMsg is UIKomuterTicketBookingRequest komBkReq)
                {
                    try
                    {
                        if (_disposed)
                            throw new Exception("System is shutting down (EXIT22311);");

                        KomuterTicketBookingRequestCommand command = new KomuterTicketBookingRequestCommand(procId, netProcId, 
                            komBkReq.OriginStationId, 
                            komBkReq.OriginStationName, 
                            komBkReq.DestinationStationId, 
                            komBkReq.DestinationStationName, 
                            komBkReq.KomuterPackageId, 
                            komBkReq.TicketItemList);

                        Log.LogText(LogChannel, procId, command, $@"A02", classNMethodName: "ServerSalesApplication.GetKomuterTicketBooking", 
                            netProcessId: netProcId, extraMsg: $@"Start - GetKomuterTicketBooking; MsgObj: KomuterTicketBookingRequestCommand");

                        bool addCommandResult = _svrAccess.AddCommand(new AccessCommandPack(command), out string errorMsg);

                        if (addCommandResult == false)
                        {
                            if (string.IsNullOrWhiteSpace(errorMsg) == false)
                                throw new Exception(errorMsg);
                            else
                                throw new Exception("Unknown error (EXIT22301).");
                        }

                        Log.LogText(LogChannel, procId, $@"End - GetKomuterTicketBooking;", $@"A10", classNMethodName: "ServerSalesApplication.GetKomuterTicketBooking", netProcessId: netProcId);

                    }
                    catch (Exception ex)
                    {
                        Log.LogError(LogChannel, processId, new Exception($@"Error found. Net Process Id: {netProcessId}", ex), "E01", "ServerSalesApplication.GetKomuterTicketBooking");
                        RaiseOnShowResultMessage(netProcId, null, MessageType.ErrorType, "Server error; Error when sending internal service command (Getting Komuter Ticket Booking). " + ex.Message);
                    }
                }
            }

            void CheckoutKomuterTicketBooking(string procId, Guid? netProcId, UserSession session, IKioskMsg kioskMsg)
            {
                if (kioskMsg is UIKomuterBookingCheckoutRequest komBkChkOutReq)
                {
                    try
                    {
                        if (_disposed)
                            throw new Exception("System is shutting down (EXIT22303);");

                        KomuterBookingCheckoutCommand command = new KomuterBookingCheckoutCommand(procId, netProcId, 
                            komBkChkOutReq.BookingId, 
                            komBkChkOutReq.TotalAmount, 
                            komBkChkOutReq.FinancePaymentMethod
                        );

                        Log.LogText(LogChannel, procId, command, $@"A02", classNMethodName: "ServerSalesApplication.CheckoutKomuterTicketBooking",
                            netProcessId: netProcId, extraMsg: $@"Start - CheckoutKomuterTicketBooking; MsgObj: KomuterBookingCheckoutCommand");

                        bool addCommandResult = _svrAccess.AddCommand(new AccessCommandPack(command), out string errorMsg);

                        if (addCommandResult == false)
                        {
                            if (string.IsNullOrWhiteSpace(errorMsg) == false)
                                throw new Exception(errorMsg);
                            else
                                throw new Exception("Unknown error (EXIT22305).");
                        }

                        Log.LogText(LogChannel, procId, $@"End - CheckoutKomuterTicketBooking;", $@"A10", classNMethodName: "ServerSalesApplication.CheckoutKomuterTicketBooking", netProcessId: netProcId);

                    }
                    catch (Exception ex)
                    {
                        Log.LogError(LogChannel, processId, new Exception($@"Error found. Net Process Id: {netProcessId}", ex), "E01", "ServerSalesApplication.CheckoutKomuterTicketBooking");
                        RaiseOnShowResultMessage(netProcId, null, MessageType.ErrorType, "Server error; Error when sending internal service command (Checkout Komuter Ticket Booking). " + ex.Message);
                    }
                }
            }

            void SubmitKomuterBookingPayment(string procId, Guid? netProcId, UserSession session, IKioskMsg kioskMsg)
            {
                if (kioskMsg is UIKomuterCompletePaymentSubmission komPaySubm)
                {
                    try
                    {
                        if (_disposed)
                            throw new Exception("System is shutting down (EXIT22307);");

                        KomuterCompletePaymentCommand command = null;
                        if (komPaySubm.TypeOfPayment == PaymentType.PaymentGateway)
                        {
                            command = new KomuterCompletePaymentCommand(procId, netProcId,
                                komPaySubm.BookingId,
                                komPaySubm.CurrencyId,
                                komPaySubm.Amount,
                                komPaySubm.FinancePaymentMethod, 
                                komPaySubm.BTnGSaleTransactionNo
                            );
                        }
                        else
                        {
                            command = new KomuterCompletePaymentCommand(procId, netProcId,
                                komPaySubm.BookingId,
                                komPaySubm.CurrencyId,
                                komPaySubm.Amount,
                                komPaySubm.FinancePaymentMethod,
                                komPaySubm.CreditCardAnswer
                            );
                        }
                        
                        Log.LogText(LogChannel, procId, command, $@"A02", classNMethodName: "ServerSalesApplication.UIKomuterCompletePaymentSubmission",
                            netProcessId: netProcId, extraMsg: $@"Start - UIKomuterCompletePaymentSubmission; MsgObj: KomuterCompletePaymentCommand");

                        bool addCommandResult = _svrAccess.AddCommand(new AccessCommandPack(command), out string errorMsg);

                        if (addCommandResult == false)
                        {
                            if (string.IsNullOrWhiteSpace(errorMsg) == false)
                                throw new Exception(errorMsg);
                            else
                                throw new Exception("Unknown error (EXIT22309).");
                        }

                        Log.LogText(LogChannel, procId, $@"End - UIKomuterCompletePaymentSubmission;", $@"A10", classNMethodName: "ServerSalesApplication.UIKomuterCompletePaymentSubmission", netProcessId: netProcId);

                    }
                    catch (Exception ex)
                    {
                        Log.LogError(LogChannel, processId, new Exception($@"Error found. Net Process Id: {netProcessId}", ex), "E01", "ServerSalesApplication.UIKomuterCompletePaymentSubmission");
                        RaiseOnShowResultMessage(netProcId, null, MessageType.ErrorType, "Server error; Error when sending internal service command (Submit Komuter Ticket Payment). " + ex.Message);
                    }
                }
            }

            void ExtendingBookingTimeout(string procId, Guid? netProcId, UserSession session, IKioskMsg kioskMsg)
            {
                try
                {
                    if (kioskMsg is UISalesBookingTimeoutExtensionRequest uiExtReq)
                    {
                        if (_disposed)
                            throw new Exception("System is shutting down (EXIT22302);");

                        ExtendBookingTimeoutCommand command = new ExtendBookingTimeoutCommand(procId, netProcId, uiExtReq.BookingId);

                        Log.LogText(LogChannel, procId, command, $@"A02", classNMethodName: "ServerSalesApplication.ExtendingBookingTimeout",
                            netProcessId: netProcId, extraMsg: $@"Start - ExtendingBookingTimeout; MsgObj: ExtendBookingTimeoutCommand");

                        bool addCommandResult = _svrAccess.AddCommand(new AccessCommandPack(command), out string errorMsg);

                        if (addCommandResult == false)
                        {
                            if (string.IsNullOrWhiteSpace(errorMsg) == false)
                                throw new Exception(errorMsg);
                            else
                                throw new Exception("Unknown error (EXIT22304).");
                        }

                        Log.LogText(LogChannel, procId, $@"End - ExtendingBookingTimeout;", $@"A10", classNMethodName: "ServerSalesApplication.ExtendingBookingTimeout", netProcessId: netProcId);
                    }
                }
                catch (Exception ex)
                {
                    Log.LogError(LogChannel, processId, new Exception($@"Error found. Net Process Id: {netProcessId}", ex), "E01", "ServerSalesApplication.ExtendingBookingTimeout");
                }
            }

            void UpdateBookingTimeoutExtension(string procId, Guid? netProcId, UserSession session, IKioskMsg kioskMsg)
            {
                try
                {
                    if (kioskMsg is UISalesBookingTimeoutExtensionResult uiExtRes)
                    {
                        if (uiExtRes.MessageData is ExtendBookingSessionResult extRes)
                        {
                            if (extRes.Code.Equals(WebAPIAgent.ApiCodeOK) && (extRes.Data.Error?.Equals(YesNo.No) == true))
                            {
                                DateTime expireTime = DateTime.Now.AddSeconds(extRes.Data.BookingRemainingInSecond);

                                _appCountDown.UpdateBookingTimeout(_session.SessionId, _session.SeatBookingId, expireTime, isUpdateTimeoutSuccess: true);
                                uiExtRes.UpdateSessionId(_session.SessionId);

                                RaiseOnShowResultMessage(_session.SessionId, uiExtRes, MessageType.NormalType);
                            }
                            else
                            {
                                _appCountDown.UpdateBookingTimeout(_session.SessionId, _session.SeatBookingId, bookingTimeout: null, isUpdateTimeoutSuccess: false);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.LogError(LogChannel, processId, new Exception($@"Error found. Net Process Id: {netProcessId}", ex), "E01", "ServerSalesApplication.ChangeTimeout");
                }
            }

            void PauseCountDown(string procId, Guid? netProcId, UserSession session)
            {
                bool retRes = _appCountDown.Pause(session.SessionId);
            }

            void TimeoutSession(string procId, Guid? netProcId, UserSession session)
            {
                RaiseOnShowResultMessage(_session.SessionId, new UICountDownExpiredAck(_session.SessionId, "-", DateTime.Now));
            }
        }

        private Guid? _raiseOnShowResultMessage_currentWorkingToken = null;
        private ConcurrentQueue<Guid> _workQ = new ConcurrentQueue<Guid>();
        private SemaphoreSlim _showResultMessageLock = new SemaphoreSlim(1);
        private SemaphoreSlim _showResultMessageLock2 = new SemaphoreSlim(1);
        /// <summary>
        /// FuncCode:EXIT62.0104
        /// </summary>
        /// <param name="netProcessId"></param>
        /// <param name="kioskMsg">Must has a valid object in order to return a result to UI.</param>
        /// <param name="msgType"></param>
        /// <param name="message">Normally this is 2nd error message. 1st error message is in returnObj.ErrorMessage</param>
        public void RaiseOnShowResultMessage(Guid? netProcessId, IKioskMsg kioskMsg, MessageType? msgType = null, string message = null)
        {
            bool isSaleEventDispatched = false;
            bool updateCountDownUponDispatched = false;
            Guid workToken = Guid.NewGuid();

            try
            {
                _showResultMessageLock.WaitAsync().Wait();

                // Maintenence - Card Settlement - Reboot
                if ((kioskMsg.Instruction == (CommInstruction)UISalesInst.CardSettlementStatusAck)
                    || (kioskMsg.Instruction == (CommInstruction)UISalesInst.CheckOutstandingCardSettlementAck)
                    || (kioskMsg is UIServerApplicationStatusAck)
                    || (kioskMsg is UIAck<UIxGnAppAck<KioskLastRebootTimeEcho>>)
                    )
                {
                    MessageType msgTy2 = (msgType.HasValue == false) ? MessageType.NormalType : msgType.Value;
                    //Card Settlement
                    if ((string.IsNullOrWhiteSpace(kioskMsg.ErrorMessage)) && (msgTy2 == MessageType.NormalType || msgTy2 == MessageType.UnknownType))
                        OnShowResultMessage?.Invoke(null, new UIMessageEventArgs(netProcessId) { Message = message, KioskMsg = kioskMsg, MsgType = MessageType.NormalType });
                    else
                        OnShowResultMessage?.Invoke(null, new UIMessageEventArgs(netProcessId) { Message = message, KioskMsg = kioskMsg, MsgType = MessageType.ErrorType });

                    return;
                }
                else if (kioskMsg.Instruction == (CommInstruction)UISalesInst.SalesTimeoutWarningAck)
                {
                    OnShowResultMessage?.Invoke(null, new UIMessageEventArgs(netProcessId) { Message = message, KioskMsg = kioskMsg, MsgType = MessageType.NormalType });
                    return;
                }
                else if (kioskMsg.Instruction == (CommInstruction)UISalesInst.SalesBookingTimeoutExtensionResult)
                {
                    OnShowResultMessage?.Invoke(null, new UIMessageEventArgs(netProcessId) { Message = message, KioskMsg = kioskMsg, MsgType = MessageType.NormalType });
                    return;
                }
                //
                //---------------------------------------------------------------------------------------

                if (_clientAppUnderMaintenance)
                {
                    return;
                }

                if (kioskMsg != null)
                {
                    bool proceed = false;

                    MessageType msgTy = (msgType.HasValue == false) ? MessageType.NormalType : msgType.Value;

                    if ((kioskMsg is UIServerApplicationStatusAck) 
                        || (kioskMsg is UISalesClientMaintenanceAck) 
                        || (kioskMsg is UIWebServerLogonStatusAck) 
                        || (kioskMsg is UICountDownExpiredAck)
                        )
                    {
                        proceed = true;

                        //----------------------------------------------------------
                        // Clear previous NetProcessID if already expired
                        if (kioskMsg is UICountDownExpiredAck)
                        {
                            ReleaseSeatOnTimeout();
                            _session.Expired = true;
                            _SessionSuper.CleanNetProcessId();
                        }

                        if (kioskMsg is UISalesClientMaintenanceAck)
                            _clientAppUnderMaintenance = true;
                    }
                    else if (_session.SessionId.Equals(Guid.Empty))
                    {
                        proceed = true;
                    }
                    else if (_session.Expired == false)
                    {
                        if (_SessionSuper.FindNetProcessId(netProcessId.Value))
                        {
                            proceed = true;
                            updateCountDownUponDispatched = true;
                        }
                        else
                        {
                            // Note : This mean netProcessId is refer to previous session and already expired.
                            proceed = false;
                        }
                    }
                    //--------------------------------------------------------------
                    if (proceed)
                    {
                        //----------------------------------------------------------
                        // Update Session info into kioskMsg
                        if (kioskMsg is IUserSession sess)
                            sess.UpdateSession(_session);
                        //----------------------------------------------------------

                        if ((string.IsNullOrWhiteSpace(kioskMsg.ErrorMessage)) && (msgTy == MessageType.NormalType || msgTy == MessageType.UnknownType))
                            OnShowResultMessage?.Invoke(null, new UIMessageEventArgs(netProcessId) { Message = message, KioskMsg = kioskMsg, MsgType = MessageType.NormalType });
                        else
                            OnShowResultMessage?.Invoke(null, new UIMessageEventArgs(netProcessId) { Message = message, KioskMsg = kioskMsg, MsgType = MessageType.ErrorType });

                        isSaleEventDispatched = true;
                        _workQ.Enqueue(workToken);
                    }
                }
                else
                {
                    message = (string.IsNullOrWhiteSpace(message) == true) ? "Result not available. (EXIT21318)" : message;
                    OnShowResultMessage?.Invoke(null, new UIMessageEventArgs(netProcessId) { Message = message, KioskMsg = null, MsgType = MessageType.ErrorType });
                }
            }
            catch (Exception ex)
            {
                if (kioskMsg != null)
                    Log.LogError(LogChannel, kioskMsg.ProcessId, new Exception($@"Unhandle event error OnShowCustomerMessage. Net Process Id: {netProcessId}", ex), "E01", "ServerSalesApplication.ShowCustomerMessage");
                else
                    Log.LogError(LogChannel, "-", new Exception($@"Unhandle event error OnShowCustomerMessage. Net Process Id: {netProcessId}", ex), "E02", "ServerSalesApplication.ShowCustomerMessage");
            }
            finally
            {
                if (_showResultMessageLock.CurrentCount == 0)
                    _showResultMessageLock.Release();

                //Note : Re-set Count Down; Below statement put here to avoid death-lock between _showResultMessageLock and _appCountDown's threadLock;
                if (isSaleEventDispatched) 
                {
                    bool workDone = false;

                    while (workDone == false)
                    {
                        try
                        {
                            _showResultMessageLock2.WaitAsync().Wait();

                            if (_raiseOnShowResultMessage_currentWorkingToken.HasValue == false)
                            {
                                if (_workQ.TryDequeue(out Guid nextToken))
                                    _raiseOnShowResultMessage_currentWorkingToken = nextToken;
                            }

                            if ((_raiseOnShowResultMessage_currentWorkingToken.HasValue == false) || (_raiseOnShowResultMessage_currentWorkingToken.Value.Equals(workToken)))
                            {
                                if (updateCountDownUponDispatched)
                                {
                                    int timeOutDelay = GetTimeOutDelaySec(kioskMsg, out string timeOutRefTag, out bool byPassUpdateTimeOut);

                                    if ((byPassUpdateTimeOut == false) && (_appCountDown.ChangeDefaultTimeoutSetting((timeOutDelay + 3), timeOutRefTag).ResetTimeout(_session.SessionId, out bool isMatchedSession2) == false))
                                    {
                                        if (isMatchedSession2 == false)
                                            throw new Exception($@"Unable to update Count Down; (EXIT21325); Current App Session ID {_session.SessionId.ToString("D")} is not matched with count down.");
                                        else
                                            throw new Exception($@"Unable to update Count Down; (EXIT21327); Session ID {_session.SessionId.ToString("D")} already expired.");
                                    }
                                }

                                workDone = true;
                                _raiseOnShowResultMessage_currentWorkingToken = null;

                            }
                        }
                        catch (Exception ex)
                        {
                            workDone = true;
                            _raiseOnShowResultMessage_currentWorkingToken = null;

                            Log.LogText(LogChannel, "-", kioskMsg, "EX11", "ServerSalesApplication.ShowCustomerMessage", AppDecorator.Log.MessageType.Error, extraMsg: "MsgObj: IKioskMsg", netProcessId: netProcessId);
                            Log.LogError(LogChannel, "-", ex, "EX12", "ServerSalesApplication.ShowCustomerMessage", netProcessId: netProcessId);
                        }
                        finally
                        {
                            if (_showResultMessageLock2.CurrentCount == 0)
                                _showResultMessageLock2.Release();

                            if (workDone == false)
                                Task.Delay(GetJumpNextDelay()).Wait();
                        }
                    }
                }
            }

            int GetJumpNextDelay()
            {
                string aVal = Guid.NewGuid().ToString("D").Substring(0,1);
                int intDelay = Convert.ToInt32(aVal, 16) * 3;
                return intDelay;
            }

            void ReleaseSeatOnTimeout()
            {
                string transactionNo = null;
                if (string.IsNullOrWhiteSpace(_session.SeatBookingId) == false)
                {
                    transactionNo = _session.SeatBookingId;
                }
                else if (string.IsNullOrWhiteSpace(_session.DepartPendingReleaseTransNo) == false)
                {
                    transactionNo = _session.DepartPendingReleaseTransNo;
                }
                else
                    return;

                Thread tWorker = new Thread(new ThreadStart(new Action(() => {
                    Guid dummyGuid = Guid.NewGuid();
                    try
                    {
                        if (_disposed)
                            throw new Exception("System is shutting down (EXIT21365);");

                        TicketReleaseCommand command = new TicketReleaseCommand("-", dummyGuid, transactionNo);
                        Log.LogText(LogChannel, "-", $@"Start - ReleaseSeatOnTimeout; Net Process Id:{dummyGuid}", $@"A02", classNMethodName: "ServerSalesApplication.ReleaseSeatOnTimeout");
                        bool addCommandResult = _svrAccess.AddCommand(new AccessCommandPack(command), out string errorMsg);

                        if (addCommandResult == false)
                        {
                            if (string.IsNullOrWhiteSpace(errorMsg) == false)
                                throw new Exception(errorMsg);
                            else
                                throw new Exception("Unknown error (EXIT21366).");
                        }
                    }
                    catch (Exception ex)
                    {
                        //RaiseOnShowResultMessage(netProcId, null, MessageType.ErrorType, "Server error; Error when sending internal service command (Release Seat). " + ex.Message);
                        Log.LogError(LogChannel, "-", new Exception($@"Error found. Net Process Id: {dummyGuid}", ex), "E01", "ServerSalesApplication.ReleaseSeatOnTimeout");
                    }
                })));
                tWorker.IsBackground = true;
                tWorker.Priority = ThreadPriority.AboveNormal;
                tWorker.Start();
            }

            ///<summary>
            ///Return deley time that can be reused for reseting timeout for many time.
            ///</summary>
            int GetTimeOutDelaySec(IKioskMsg kioskMsgX, out string refDelayTag, out bool byPassUpdateTimeOutX)
            {
                byPassUpdateTimeOutX = false;
                refDelayTag = "Default";

                int timeoutDelaySec = _standardCountDownIntervalSec * TimeDelayMultiplicationIndex;

                if (kioskMsgX.Instruction == (CommInstruction)UISalesInst.LanguageSelectionAck)
                {
                    refDelayTag = Enum.GetName(typeof(CommInstruction), kioskMsgX.Instruction);
                    timeoutDelaySec = _standardCountDownIntervalSec * TimeDelayMultiplicationIndex;
                }
                else if (kioskMsgX.Instruction == (CommInstruction)UISalesInst.OriginListAck)
                {
                    refDelayTag = Enum.GetName(typeof(CommInstruction), kioskMsgX.Instruction);
                    timeoutDelaySec = _standardCountDownIntervalSec * TimeDelayMultiplicationIndex;
                }
                else if (kioskMsgX.Instruction == (CommInstruction)UISalesInst.DestinationListAck)
                {
                    refDelayTag = Enum.GetName(typeof(CommInstruction), kioskMsgX.Instruction);
                    timeoutDelaySec = _standardCountDownIntervalSec * TimeDelayMultiplicationIndex;
                }
                else if (kioskMsgX.Instruction == (CommInstruction)UISalesInst.TravelDatesEnteringAck)
                {
                    refDelayTag = Enum.GetName(typeof(CommInstruction), kioskMsgX.Instruction);
                    timeoutDelaySec = _standardCountDownIntervalSec * TimeDelayMultiplicationIndex;
                }
                else if (kioskMsgX.Instruction == (CommInstruction)UISalesInst.DepartTripListInitAck)
                {
                    refDelayTag = Enum.GetName(typeof(CommInstruction), kioskMsgX.Instruction);
                    timeoutDelaySec = _standardCountDownIntervalSec * 2 * TimeDelayMultiplicationIndex;
                }
                else if (kioskMsgX.Instruction == (CommInstruction)UISalesInst.DepartTripListAck)
                {
                    refDelayTag = Enum.GetName(typeof(CommInstruction), kioskMsgX.Instruction);
                    timeoutDelaySec = _standardCountDownIntervalSec * 2 * TimeDelayMultiplicationIndex;
                }
                else if (kioskMsgX.Instruction == (CommInstruction)UISalesInst.DepartSeatListAck)
                {
                    refDelayTag = Enum.GetName(typeof(CommInstruction), kioskMsgX.Instruction);
                    timeoutDelaySec = _standardCountDownIntervalSec * 3 * TimeDelayMultiplicationIndex;
                }
                else if (kioskMsgX.Instruction == (CommInstruction)UISalesInst.DepartSeatConfirmFailAck)
                {
                    refDelayTag = Enum.GetName(typeof(CommInstruction), kioskMsgX.Instruction);
                    timeoutDelaySec = _standardCountDownIntervalSec * 3 * TimeDelayMultiplicationIndex;
                }
                //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                //Return
                else if (kioskMsgX.Instruction == (CommInstruction)UISalesInst.ReturnTripListInitAck)
                {
                    refDelayTag = Enum.GetName(typeof(CommInstruction), kioskMsgX.Instruction);
                    timeoutDelaySec = _standardCountDownIntervalSec * 2 * TimeDelayMultiplicationIndex;
                }
                else if (kioskMsgX.Instruction == (CommInstruction)UISalesInst.ReturnTripListAck)
                {
                    refDelayTag = Enum.GetName(typeof(CommInstruction), kioskMsgX.Instruction);
                    timeoutDelaySec = _standardCountDownIntervalSec * 2 * TimeDelayMultiplicationIndex;
                }
                else if (kioskMsgX.Instruction == (CommInstruction)UISalesInst.ReturnSeatConfirmFailAck)
                {
                    refDelayTag = Enum.GetName(typeof(CommInstruction), kioskMsgX.Instruction);
                    timeoutDelaySec = _standardCountDownIntervalSec * 3 * TimeDelayMultiplicationIndex;
                }
                else if (kioskMsgX.Instruction == (CommInstruction)UISalesInst.ReturnSeatListAck)
                {
                    refDelayTag = Enum.GetName(typeof(CommInstruction), kioskMsgX.Instruction);
                    timeoutDelaySec = _standardCountDownIntervalSec * 3 * TimeDelayMultiplicationIndex;
                }
                //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                //CustInfoPrerequisiteRequest
                else if (kioskMsgX.Instruction == (CommInstruction)UISalesInst.CustInfoAck)
                {
                    refDelayTag = Enum.GetName(typeof(CommInstruction), kioskMsgX.Instruction);
                    timeoutDelaySec = (_custInfoEntryIntervalSec * 5 * TimeDelayMultiplicationIndex) + 3;
                }
                else if (kioskMsgX.Instruction == (CommInstruction)UISalesInst.CustPromoCodeVerifyAck)
                {
                    byPassUpdateTimeOutX = true;
                    refDelayTag = Enum.GetName(typeof(CommInstruction), kioskMsgX.Instruction);

                    // ..note .. below timeoutDelaySec statement has no effect.
                    timeoutDelaySec = (_custInfoEntryIntervalSec * 5 * TimeDelayMultiplicationIndex) + 3;
                    //--------------------------------------------------
                }
                else if (kioskMsgX.Instruction == (CommInstruction)UISalesInst.CustInfoPNRTicketTypeAck)
                {
                    byPassUpdateTimeOutX = true;
                    refDelayTag = Enum.GetName(typeof(CommInstruction), kioskMsgX.Instruction);

                    // ..note .. below timeoutDelaySec statement has no effect.
                    timeoutDelaySec = (_custInfoEntryIntervalSec * 5 * TimeDelayMultiplicationIndex) + 3;
                    //--------------------------------------------------
                }
                else if (kioskMsgX.Instruction == (CommInstruction)UISalesInst.CustInfoUpdateFailAck)
                {
                    // Request to Amend PassengerInfo
                    if (_session.IsRequestAmendPassengerInfo == true)
                    {
                        refDelayTag = Enum.GetName(typeof(CommInstruction), kioskMsgX.Instruction);
                        timeoutDelaySec = (_custInfoEntryIntervalSec * 3 * TimeDelayMultiplicationIndex) + 3;
                    }

                    // Fail PassengerInfo Submission
                    else
                    {
                        refDelayTag = Enum.GetName(typeof(CommInstruction), kioskMsgX.Instruction);
                        timeoutDelaySec = _standardCountDownIntervalSec * TimeDelayMultiplicationIndex;
                    }
                }
                //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                else if (kioskMsgX.Instruction == (CommInstruction)UISalesInst.ETSInsuranceListAck)
                {
                    refDelayTag = Enum.GetName(typeof(CommInstruction), kioskMsgX.Instruction);
                    timeoutDelaySec = _standardCountDownIntervalSec * TimeDelayMultiplicationIndex;
                }
                //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                //Payment
                else if (kioskMsgX.Instruction == (CommInstruction)UISalesInst.SalesPaymentProceedAck)
                {
                    refDelayTag = Enum.GetName(typeof(CommInstruction), kioskMsgX.Instruction);
                    timeoutDelaySec = (60 * 3) + 45;
                }
                // Printing
                else if (kioskMsgX.Instruction == (CommInstruction)UISalesInst.CompleteTransactionResult)
                {
                    refDelayTag = Enum.GetName(typeof(CommInstruction), kioskMsgX.Instruction);
                    timeoutDelaySec = _standardCountDownIntervalSec * 6 * TimeDelayMultiplicationIndex;
                }
                //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                // KTM - Komuter
                else if (kioskMsgX.Instruction == (CommInstruction)UISalesInst.StartSellingAck)
                {
                    refDelayTag = Enum.GetName(typeof(CommInstruction), kioskMsgX.Instruction);
                    timeoutDelaySec = _standardCountDownIntervalSec * 2 * TimeDelayMultiplicationIndex;
                }
                else if (kioskMsgX.Instruction == (CommInstruction)UISalesInst.KomuterTicketTypePackageAck)
                {
                    refDelayTag = Enum.GetName(typeof(CommInstruction), kioskMsgX.Instruction);
                    timeoutDelaySec = _standardCountDownIntervalSec * 3 * TimeDelayMultiplicationIndex;
                }
                else if (kioskMsgX.Instruction == (CommInstruction)UISalesInst.KomuterTicketBookingAck)
                {
                    refDelayTag = Enum.GetName(typeof(CommInstruction), kioskMsgX.Instruction);
                    timeoutDelaySec = _standardCountDownIntervalSec * 5 * TimeDelayMultiplicationIndex;
                }
                //Payment
                else if (kioskMsgX.Instruction == (CommInstruction)UISalesInst.KomuterBookingCheckoutAck)
                {
                    refDelayTag = Enum.GetName(typeof(CommInstruction), kioskMsgX.Instruction);
                    timeoutDelaySec = (60 * 3) + 30;
                }
                // Printing
                else if (kioskMsgX.Instruction == (CommInstruction)UISalesInst.KomuterCompletePaymentAck)
                {
                    refDelayTag = Enum.GetName(typeof(CommInstruction), kioskMsgX.Instruction);
                    timeoutDelaySec = _standardCountDownIntervalSec * 6 * TimeDelayMultiplicationIndex;
                }
                //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                return timeoutDelaySec;
            }
        }

        public bool ShutDown()
        {
            _disposed = true;

            if (OnShowResultMessage != null)
            {
                Delegate[] delgList = OnShowResultMessage.GetInvocationList();
                foreach (EventHandler<UIMessageEventArgs> delg in delgList)
                {
                    OnShowResultMessage -= delg;
                }
            }

            return true;
        }

        public void Dispose()
        {
            _disposed = true;

            if (OnShowResultMessage != null)
            {
                Delegate[] delgList = OnShowResultMessage.GetInvocationList();
                foreach (EventHandler<UIMessageEventArgs> delg in delgList)
                {
                    OnShowResultMessage -= delg;
                }
            }

            _log = null;
            _svrAccess = null;
        }

    }

    class InternalCommandParameters : IDisposable 
    {
        public string ProcessId { get; private set; } 
        public Guid? NetProcessId { get; private set; }
        public IKioskMsg SvcMsg { get; private set; }

        public InternalCommandParameters(string processId, Guid? netProcessId, IKioskMsg svcMsg)
        {
            ProcessId = processId;
            NetProcessId = netProcessId;
            SvcMsg = svcMsg;
        }

        public void Dispose()
        {
            SvcMsg = null;
        }
    }
    
}
