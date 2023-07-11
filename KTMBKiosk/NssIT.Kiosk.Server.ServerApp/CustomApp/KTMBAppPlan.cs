using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Command.Sales;
using NssIT.Kiosk.AppDecorator.Common.AppService.Events;
using NssIT.Kiosk.AppDecorator.Common.AppService.Instruction;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI;
using NssIT.Kiosk.AppDecorator.UI;
using NssIT.Kiosk.Log.DB;
using NssIT.Kiosk.Server.AccessDB;
using NssIT.Train.Kiosk.Common.Common;
using NssIT.Train.Kiosk.Common.Constants;
using NssIT.Train.Kiosk.Common.Data;
using NssIT.Train.Kiosk.Common.Data.Response;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Server.ServerApp.CustomApp
{
    /// <summary>
    /// ClassCode:EXIT62.03
    /// </summary>
    public class KTMBAppPlan : IServerAppPlan
    {
        private CultureInfo _provider = CultureInfo.InvariantCulture;
        private CounterConfigCompiledResult _lastCounterConfiguration = null;

        public void UpdateMachineConfig(CounterConfigCompiledResult machineConfig)
        {
            if (machineConfig != null)
            {
                _lastCounterConfiguration = machineConfig;
            }
        }

        /// <summary>
        /// FuncCode:EXIT62.0301
        /// </summary>
        public UISalesInst NextInstruction(string procId, Guid? netProcId, IKioskMsg svcMsg, UserSession session, out bool releaseSeatRequestOnEdit)
        {
            releaseSeatRequestOnEdit = false;
            if (svcMsg != null)
            {
                if (svcMsg is UIPageNavigateRequest uiPgNav)
                {
                    // Only Ets-Intercity having UIPageNavigateRequest
                    if (string.IsNullOrWhiteSpace(session.DepartPendingReleaseTransNo) == false)
                        releaseSeatRequestOnEdit = true;

                    if (uiPgNav.NavigateDirection == PageNavigateDirection.Previous)
                    {
                        if (session.CurrentEditMenuItemCode.HasValue == false)
                        {
                            return UISalesInst.CountDownExpiredAck;
                        }

                        // Note : Passenger cannot back to ReturnSeat
                        //else if (session.CurrentEditMenuItemCode == TickSalesMenuItemCode.Passenger)
                        //{
                        //    return UISalesInst.ReturnSeatListRequest;
                        //}

                        else if (session.CurrentEditMenuItemCode == TickSalesMenuItemCode.ReturnSeat)
                        {
                            return UISalesInst.ReturnTripListInitAck;
                        }

                        // Note : ReturnTrip cannot back to DepartSeat
                        //else if (session.CurrentEditMenuItemCode == TickSalesMenuItemCode.ReturnTrip)
                        //{
                        //    return UISalesInst.DepartSeatListRequest;
                        //}

                        else if (session.CurrentEditMenuItemCode == TickSalesMenuItemCode.DepartSeat)
                        {
                            return UISalesInst.DepartTripListInitAck;
                        }
                        else if (session.CurrentEditMenuItemCode == TickSalesMenuItemCode.DepartTrip)
                        {
                            return UISalesInst.TravelDatesEnteringAck;
                        }

                        else if (session.CurrentEditMenuItemCode == TickSalesMenuItemCode.TravelDates)
                        {
                            return UISalesInst.DestinationListRequest;
                        }

                        else if (session.CurrentEditMenuItemCode == TickSalesMenuItemCode.ToStation)
                        {
                            return UISalesInst.OriginListRequest;
                        }
                        else
                            return UISalesInst.CountDownExpiredAck;
                    }
                    else if (uiPgNav.NavigateDirection == PageNavigateDirection.Exit)
                    {  }

                    return UISalesInst.CountDownExpiredAck;
                }
                else if (session.VehicleGroup == TransportGroup.EtsIntercity)
                {
                    if (svcMsg.Instruction == (CommInstruction)UISalesInst.CountDownStartRequest)
                        return UISalesInst.LanguageSelectionAck;

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.CounterConfigurationResult)
                        return UISalesInst.LanguageSelectionAck;

                    //xxxxxxxxxxxxxxxxxx

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.LanguageSubmission)
                        return UISalesInst.OriginListRequest;

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.OriginListAck)
                        return UISalesInst.RedirectDataToClient;

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.OriginSubmission)
                        return UISalesInst.DestinationListRequest;

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.DestinationListAck)
                        return UISalesInst.RedirectDataToClient;

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.DestinationSubmission)
                        return UISalesInst.TravelDatesEnteringAck;

                    //xxxxxxxxxxxxxxxxxxx

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.TravelDatesSubmission)
                        return UISalesInst.DepartTripListInitAck;

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.DepartTripListRequest)
                        return UISalesInst.DepartTripListRequest;

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.DepartTripListAck)
                        return UISalesInst.RedirectDataToClient;

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.DepartTripSubmission)
                        return UISalesInst.DepartSeatListRequest;

                    //else if (svcMsg.Instruction == (CommInstruction)UISalesInst.DepartSeatListErrorResult)
                    //    return UISalesInst.DepartTripSubmissionErrorAck;

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.DepartSeatListAck)
                        return UISalesInst.RedirectDataToClient;

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.DepartSeatSubmission)
                    {
                        return UISalesInst.DepartSeatConfirmRequest;
                    }

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.DepartSeatConfirmResult)
                    {
                        if (session.DepartSeatConfirmCode?.Equals("0") == true)
                        {
                            if (session.TravelMode == AppDecorator.Common.TravelMode.DepartOrReturn)
                            {
                                return UISalesInst.ReturnTripListInitAck;
                            }
                            else
                            {
                                // Prepare for Customer Info Entry
                                return UISalesInst.CustInfoPrerequisiteRequest;
                            }
                        }
                        else
                            return UISalesInst.DepartSeatConfirmFailAck;
                    }

                    //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                    //Return
                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.ReturnTripListRequest)
                        return UISalesInst.ReturnTripListRequest;

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.ReturnTripListAck)
                        return UISalesInst.RedirectDataToClient;

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.ReturnTripSubmission)
                        return UISalesInst.ReturnSeatListRequest;

                    //else if (svcMsg.Instruction == (CommInstruction)UISalesInst.ReturnSeatListErrorResult)
                    //    return UISalesInst.ReturnTripSubmissionErrorAck;

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.ReturnSeatListAck)
                        return UISalesInst.RedirectDataToClient;

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.ReturnSeatSubmission)
                    {
                        return UISalesInst.ReturnSeatConfirmRequest;
                    }

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.ReturnSeatConfirmResult)
                    {
                        if (session.ReturnSeatConfirmCode?.Equals("0") == true)
                        {
                            // Prepare for Customer Info Entry
                            return UISalesInst.CustInfoPrerequisiteRequest;
                        }
                        else
                            return UISalesInst.ReturnSeatConfirmFailAck;
                    }
                    //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.CustInfoPrerequisiteAck)
                        return UISalesInst.CustInfoAck;

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.CustPromoCodeVerifyRequest)
                        return UISalesInst.CustPromoCodeVerifyRequest;

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.CustPromoCodeVerifyAck)
                    {
                        return UISalesInst.RedirectDataToClient;
                    }

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.CustInfoPNRTicketTypeRequest)
                    {
                        return UISalesInst.CustInfoPNRTicketTypeRequest;
                    }

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.CustInfoPNRTicketTypeAck)
                    {
                        return UISalesInst.RedirectDataToClient;
                    }

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.CustInfoSubmission)
                        return UISalesInst.CustInfoUpdateELSEReleaseSeatRequest;

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.CustInfoUpdateResult)
                    {
                        if (session.PassengerInfoUpdateStatus == ProcessResult.Success)
                        {
                            if (session.IsAllowedETSIntercityInsurance)
                            {
                                return UISalesInst.ETSInsuranceListRequest;
                            }
                            else
                            {
                                return UISalesInst.ETSCheckoutSaleRequest;
                            }
                        }
                        else
                        {
                            return UISalesInst.CustInfoUpdateFailAck;
                        }
                    }

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.ETSInsuranceListAck)
                    {
                        if (session.IsETSIntercityInsuranceValid == false)
                        {
                            return UISalesInst.ETSCheckoutSaleRequest;
                        }
                        else
                        {
                            session.CurrentEditMenuItemCode = TickSalesMenuItemCode.Insurance;
                            return UISalesInst.RedirectDataToClient;
                        }
                    }

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.ETSInsuranceSubmissionRequest)
                    {
                        if (string.IsNullOrWhiteSpace(session.ETSInsuranceHeadersId) == false)
                            return UISalesInst.ETSInsuranceSubmissionRequest;
                        else
                            return UISalesInst.ETSCheckoutSaleRequest;
                    }

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.ETSInsuranceSubmissionResult)
                    {
                        return UISalesInst.ETSCheckoutSaleRequest;
                    }

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.ETSCheckoutSaleResult)
                    {
                        if (session.ETSIntercityCheckoutStatus == ProcessResult.Success)
                            return UISalesInst.SalesPaymentProceedAck;
                        else
                            return UISalesInst.ETSCheckoutSaleFailAck;
                    }

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.ETSIntercityTicketRequest)
                        return UISalesInst.ETSIntercityTicketRequest;

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.SalesPaymentSubmission)
                    {
                        return UISalesInst.CompleteTransactionElseReleaseSeatRequest;
                    }

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.SeatReleaseRequest)
                    {
                        return UISalesInst.SeatReleaseRequest;
                    }

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.CompleteTransactionResult)
                    {
                        return UISalesInst.RedirectDataToClient;
                    }

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.CountDownPausedRequest)
                    {
                        return UISalesInst.CountDownPausedRequest;
                    }

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.TimeoutChangeRequest)
                    {
                        return UISalesInst.TimeoutChangeRequest;
                    }

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.SalesBookingTimeoutExtensionRequest)
                    {
                        return UISalesInst.SalesBookingTimeoutExtensionRequest;
                    }

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.SalesBookingTimeoutExtensionResult)
                    {
                        return UISalesInst.SalesBookingTimeoutExtensionResult;
                    }
                        
                }
                else if (session.VehicleGroup == TransportGroup.Komuter)
                {
                    if (svcMsg.Instruction == (CommInstruction)UISalesInst.CountDownStartRequest)
                        return UISalesInst.LanguageSelectionAck;

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.CounterConfigurationResult)
                        return UISalesInst.LanguageSelectionAck;

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.LanguageSubmission)
                        return UISalesInst.StartSellingAck;

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.KomuterResetUserSessionRequest)
                        return UISalesInst.KomuterResetUserSessionRequest;

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.KomuterTicketTypePackageRequest)
                        return UISalesInst.KomuterTicketTypePackageRequest;

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.KomuterTicketTypePackageAck)
                        return UISalesInst.RedirectDataToClient;

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.KomuterTicketBookingRequest)
                        return UISalesInst.KomuterTicketBookingRequest;

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.KomuterTicketBookingAck)
                        return UISalesInst.RedirectDataToClient;

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.KomuterBookingCheckoutRequest)
                        return UISalesInst.KomuterBookingCheckoutRequest;

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.KomuterBookingCheckoutAck)
                        return UISalesInst.RedirectDataToClient;

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.KomuterCompletePaymentSubmission)
                        return UISalesInst.KomuterCompletePaymentSubmission;

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.KomuterCompletePaymentAck)
                        return UISalesInst.RedirectDataToClient;

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.SalesBookingTimeoutExtensionRequest)
                    {
                        return UISalesInst.SalesBookingTimeoutExtensionRequest;
                    }

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.SalesBookingTimeoutExtensionResult)
                        return UISalesInst.SalesBookingTimeoutExtensionResult;

                    else if (svcMsg.Instruction == (CommInstruction)UISalesInst.TimeoutChangeRequest)
                    {
                        return UISalesInst.TimeoutChangeRequest;
                    }
                }
            }
            return UISalesInst.Unknown;

            //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
        }

        /// <summary>
        /// FuncCode:EXIT62.0302
        /// </summary>
        public UserSession UpdateUserSession(UserSession userSession, IKioskMsg svcMsg)
        {
            //UICountDownStartRequest
            if (svcMsg is UICountDownStartRequest uiNewReq)
            {
                userSession.VehicleGroup = uiNewReq.VehicleCategory;
            }

            if (svcMsg is UILanguageSubmission uiLang)
            {
                if (_lastCounterConfiguration != null)
                {
                    userSession.ETS_Intercity_MaxPaxAllowed = _lastCounterConfiguration.Data.SystemConfiguration.TVMOtherTicket;
                    userSession.Komuter_MaxPaxAllowed = _lastCounterConfiguration.Data.SystemConfiguration.TVMKomTicket;
                }

                userSession.CurrentEditMenuItemCode = null;
                //userSession.Language = AppDecorator.Common.LanguageCode.English;
                userSession.Language = uiLang.Language;
                userSession.TravelMode = AppDecorator.Common.TravelMode.DepartOrReturn;
            }

            if (userSession.VehicleGroup == TransportGroup.EtsIntercity)
            {
                if (svcMsg is UIOriginSubmission uiOrigSubm)
                {
                    userSession.OriginStationCode = uiOrigSubm.OriginCode;
                    userSession.OriginStationName = uiOrigSubm.OriginName;
                    userSession.OriginAvailableTrainService = uiOrigSubm.TrainService;
                }

                else if (svcMsg is UIDestinationSubmission uiDesc)
                {
                    userSession.DestinationStationCode = uiDesc.DestinationCode;
                    userSession.DestinationStationName = uiDesc.DestinationName;
                    userSession.DestinationAvailableTrainService = uiDesc.TrainService;

                    if (userSession.OriginAvailableTrainService?.Count == 1)
                        userSession.SelectedVehicleService = userSession.OriginAvailableTrainService[0].ToString().Trim();

                    else if (userSession.DestinationAvailableTrainService?.Count == 1)
                        userSession.SelectedVehicleService = userSession.DestinationAvailableTrainService[0].ToString().Trim();

                    else
                        userSession.SelectedVehicleService = "#";
                }

                else if (svcMsg is UITravelDateSubmission uiTvDate)
                {
                    userSession.NumberOfPassenger = uiTvDate.NumberOfPassenger;

                    AppDecorator.Common.TravelMode tm = AppDecorator.Common.TravelMode.DepartOnly;
                    if (uiTvDate.ReturnDate.HasValue)
                        tm = AppDecorator.Common.TravelMode.DepartOrReturn;

                    userSession.SetTravelDate(tm, uiTvDate.DepartDate, uiTvDate.ReturnDate);
                }

                else if (svcMsg is UIDepartTripListRequest uiTrpReq)
                {
                    userSession.SetTravelDate(userSession.TravelMode, uiTrpReq.PassengerDepartDate, userSession.ReturnPassengerDepartDateTime);
                }

                else if (svcMsg is UIDepartTripSubmission uiDTripSubm)
                {
                    userSession.SetTravelDate(userSession.TravelMode, uiDTripSubm.PassengerDepartDateTime, userSession.ReturnPassengerDepartDateTime);
                    userSession.DepartTripId = uiDTripSubm.TripId;
                    userSession.DepartPassengerArrivalDate = uiDTripSubm.PassengerArrivalDateTime;
                    userSession.DepartPassengerDepartTimeStr = uiDTripSubm.PassengerDepartTimeStr;
                    userSession.DepartPassengerArrivalDayOffset = uiDTripSubm.PassengerArrivalDayOffset;
                    userSession.DepartPassengerArrivalTimeStr = uiDTripSubm.PassengerArrivalTimeStr;
                    userSession.DepartVehicleService = uiDTripSubm.VehicleService;
                    userSession.DepartVehicleNo = uiDTripSubm.VehicleNo;
                    userSession.DepartServiceCategory = uiDTripSubm.ServiceCategory;
                    userSession.DepartCurrency = uiDTripSubm.Currency;
                    userSession.DepartPrice = uiDTripSubm.Price;
                }

                //else if (svcMsg is UIDepartSeatListErrorResult uiSeatError)
                //{
                //    userSession = SetEditingSession(userSession, TickSalesMenuItemCode.DepartOperator);
                //}

                else if (svcMsg is UIDepartSeatSubmission uiDSeatSubm)
                {
                    userSession.DepartPassengerSeatDetailList = uiDSeatSubm.PassengerSeatDetailList;
                    userSession.DepartTrainSeatModelId = uiDSeatSubm.TrainSeatModelId;
                    userSession.DepartTotalAmount = 0.0M;
                    foreach (var seat in uiDSeatSubm.PassengerSeatDetailList)
                    {
                        userSession.DepartTotalAmount += (seat.Price + seat.Surcharge);
                    }
                }

                else if (svcMsg is UIDepartSeatConfirmResult uiDepSeatConfResult)
                {
                    userSession.CurrentEditMenuItemCode = TickSalesMenuItemCode.DepartSeat;

                    if (uiDepSeatConfResult.MessageData != null)
                    {
                        BookingResult bR = (BookingResult)uiDepSeatConfResult.MessageData;

                        if ((bR.Status == true) && (bR.Code.Equals(WebAPIAgent.ApiCodeOK)))
                        {
                            if (bR.Data is BookingModel bm)
                            {
                                if ((string.IsNullOrWhiteSpace(bm.Booking_Id) == false) && (bm.ErrorCode == 0))
                                {
                                    userSession.DepartSeatConfirmCode = "0";
                                    userSession.DepartPendingReleaseTransNo = null;
                                }
                                userSession.SeatBookingId = bm.Booking_Id;
                            }
                        }
                    }
                }

                else if (svcMsg is UIReturnTripListRequest uiRTrpReq)
                {
                    userSession.SetTravelDate(userSession.TravelMode, userSession.DepartPassengerDepartDateTime, uiRTrpReq.PassengerDepartDate);
                }

                //XXXXX

                else if (svcMsg is UIReturnTripSubmission uiRTripSubm)
                {
                    userSession.SetTravelDate(userSession.TravelMode, userSession.DepartPassengerDepartDateTime, uiRTripSubm.PassengerDepartDateTime);
                    userSession.ReturnTripId = uiRTripSubm.TripId;
                    userSession.ReturnPassengerArrivalDate = uiRTripSubm.PassengerArrivalDateTime;
                    userSession.ReturnPassengerDepartTimeStr = uiRTripSubm.PassengerDepartTimeStr;
                    userSession.ReturnPassengerArrivalDayOffset = uiRTripSubm.PassengerArrivalDayOffset;
                    userSession.ReturnPassengerArrivalTimeStr = uiRTripSubm.PassengerArrivalTimeStr;
                    userSession.ReturnVehicleService = uiRTripSubm.VehicleService;
                    userSession.ReturnVehicleNo = uiRTripSubm.VehicleNo;
                    userSession.ReturnServiceCategory = uiRTripSubm.ServiceCategory;
                    userSession.ReturnCurrency = uiRTripSubm.Currency;
                    userSession.ReturnPrice = uiRTripSubm.Price;
                }

                ////else if (svcMsg is UIDepartSeatListErrorResult uiSeatError)
                ////{
                ////    userSession = SetEditingSession(userSession, TickSalesMenuItemCode.DepartOperator);
                ////}

                else if (svcMsg is UIReturnSeatSubmission uiRSeatSubm)
                {
                    userSession.ReturnPassengerSeatDetailList = uiRSeatSubm.PassengerSeatDetailList;
                    userSession.ReturnTrainSeatModelId = uiRSeatSubm.TrainSeatModelId;
                    userSession.ReturnTotalAmount = 0.0M;
                    foreach (var seat in uiRSeatSubm.PassengerSeatDetailList)
                    {
                        userSession.ReturnTotalAmount += (seat.Price + seat.Surcharge);
                    }
                }

                else if (svcMsg is UIReturnSeatConfirmResult uiRetSeatConfResult)
                {
                    userSession.CurrentEditMenuItemCode = TickSalesMenuItemCode.ReturnSeat;

                    if (uiRetSeatConfResult.MessageData != null)
                    {
                        BookingResult bR = (BookingResult)uiRetSeatConfResult.MessageData;

                        if ((bR.Status == true) && (bR.Code.Equals(WebAPIAgent.ApiCodeOK)))
                        {
                            if (bR.Data is BookingModel bm)
                            {
                                if ((string.IsNullOrWhiteSpace(bm.Booking_Id) == false) && (bm.ErrorCode == 0))
                                {
                                    userSession.DepartPendingReleaseTransNo = null;
                                    userSession.ReturnSeatConfirmCode = "0";
                                    //userSession.ReturnBookingId = bm.Booking_Id;
                                }
                            }
                        }
                    }
                }

                //XXXXX

                else if (svcMsg is UICustInfoSubmission uiCustSubm)
                {
                    if (uiCustSubm.PassengerSeatDetailList?.Length > 0)
                    {
                        for (int inx = 0; inx < uiCustSubm.PassengerSeatDetailList.Length; inx++)
                        {
                            userSession.DepartPassengerSeatDetailList[inx].Contact = uiCustSubm.PassengerSeatDetailList[inx].Contact;
                            userSession.DepartPassengerSeatDetailList[inx].CustIC = uiCustSubm.PassengerSeatDetailList[inx].CustIC;
                            userSession.DepartPassengerSeatDetailList[inx].CustName = uiCustSubm.PassengerSeatDetailList[inx].CustName;
                            userSession.DepartPassengerSeatDetailList[inx].Gender = uiCustSubm.PassengerSeatDetailList[inx].Gender;
                            userSession.DepartPassengerSeatDetailList[inx].TicketType = uiCustSubm.PassengerSeatDetailList[inx].TicketType;
                            userSession.DepartPassengerSeatDetailList[inx].DepartPromoCode = uiCustSubm.PassengerSeatDetailList[inx].DepartPromoCode;
                            userSession.DepartPassengerSeatDetailList[inx].PNR = uiCustSubm.PassengerSeatDetailList[inx].PNR;

                            if (userSession.ReturnPassengerSeatDetailList?.Length > 0)
                            {
                                userSession.ReturnPassengerSeatDetailList[inx].Contact = uiCustSubm.PassengerSeatDetailList[inx].Contact;
                                userSession.ReturnPassengerSeatDetailList[inx].CustIC = uiCustSubm.PassengerSeatDetailList[inx].CustIC;
                                userSession.ReturnPassengerSeatDetailList[inx].CustName = uiCustSubm.PassengerSeatDetailList[inx].CustName;
                                userSession.ReturnPassengerSeatDetailList[inx].Gender = uiCustSubm.PassengerSeatDetailList[inx].Gender;
                                userSession.ReturnPassengerSeatDetailList[inx].TicketType = uiCustSubm.PassengerSeatDetailList[inx].TicketType;
                                userSession.ReturnPassengerSeatDetailList[inx].ReturnPromoCode = uiCustSubm.PassengerSeatDetailList[inx].ReturnPromoCode;
                                userSession.ReturnPassengerSeatDetailList[inx].PNR = uiCustSubm.PassengerSeatDetailList[inx].PNR;
                            }
                        }
                    }
                }

                else if (svcMsg is UIDepartCustInfoUpdateResult uiCustUpdRes)
                {
                    userSession.IsRequestAmendPassengerInfo = false;
                    userSession.PassengerInfoUpdateStatus = ProcessResult.Fail;
                    userSession.PassengerInfoUpdateFailMessage = "Unknown error when submit passenger info; (EXIT62.0302.XS10)"; ;
                    userSession.IsAllowedETSIntercityInsurance = false;

                    if (uiCustUpdRes.MessageData is PassengerSubmissionResultV3 updStt)
                    {
                        if (updStt.Code.Equals(WebAPIAgent.ApiCodeOK))
                        {
                            if (uiCustUpdRes.IsRequestAmendPassengerInfo == true)
                            {
                                userSession.PassengerInfoUpdateFailMessage = "Request to amend Promo Code";
                                userSession.IsRequestAmendPassengerInfo = uiCustUpdRes.IsRequestAmendPassengerInfo;
                                /////userSession.BookingExpiredDateTime = null;
                                /////userSession.SeatBookingId = null;
                            }
                            else
                            {
                                userSession.PassengerInfoUpdateStatus = ProcessResult.Success;
                                userSession.IsAllowedETSIntercityInsurance = (updStt.Data.UpdatePassengerResult.IsAllowInsurnace == YesNo.Yes);

                                CustSeatDetail custSeatDetail = null;
                                PassengerResultDetailModel pssgResultDetailModel = null;
                                decimal departTotalAmount = 0M;
                                decimal returnTotalAmount = 0M;

                                List<PassengerResultDetailModel> pssgDetList = new List<PassengerResultDetailModel>(updStt.Data.GetPassengerResult.PassengerDetailModels);

                                for (int inx = 0; inx < userSession.DepartPassengerSeatDetailList.Length; inx++)
                                {
                                    custSeatDetail = userSession.DepartPassengerSeatDetailList[inx];

                                    pssgResultDetailModel = null;
                                    pssgResultDetailModel = pssgDetList.Find(p => ((p.SeatLayoutModel_Id == custSeatDetail.SeatLayoutModel_Id) && (p.SeatLayoutModel_Id.Equals(Guid.Empty) == false)));

                                    if (pssgResultDetailModel != null)
                                    {
                                        custSeatDetail.Currency = pssgResultDetailModel.Currency;
                                        custSeatDetail.TicketPriceFormat = pssgResultDetailModel.TicketPriceFormat;
                                        custSeatDetail.OriginalTicketPrice = pssgResultDetailModel.BeforePromoDiscountTicketPrice;
                                        custSeatDetail.PromoDiscountAmount = pssgResultDetailModel.PromoDiscountAmount;
                                        custSeatDetail.TicketPrice = pssgResultDetailModel.TicketPrice;

                                        custSeatDetail.InsuranceAmount = 0M;
                                        custSeatDetail.NetTicketPrice = pssgResultDetailModel.TicketPrice;

                                        userSession.DepartPassengerSeatDetailList[inx] = custSeatDetail;
                                    }
                                }

                                departTotalAmount = (from det in userSession.DepartPassengerSeatDetailList
                                                     select det.NetTicketPrice).Sum();

                                if (userSession.ReturnPassengerSeatDetailList?.Length > 0)
                                {
                                    for (int inx = 0; inx < userSession.ReturnPassengerSeatDetailList.Length; inx++)
                                    {
                                        custSeatDetail = userSession.ReturnPassengerSeatDetailList[inx];

                                        pssgResultDetailModel = null;
                                        pssgResultDetailModel = pssgDetList.Find(p => ((p.SeatLayoutModel_Id == custSeatDetail.SeatLayoutModel_Id) && (p.SeatLayoutModel_Id.Equals(Guid.Empty) == false)));

                                        if (pssgResultDetailModel != null)
                                        {
                                            custSeatDetail.Currency = pssgResultDetailModel.Currency;
                                            custSeatDetail.TicketPriceFormat = pssgResultDetailModel.TicketPriceFormat;
                                            custSeatDetail.OriginalTicketPrice = pssgResultDetailModel.BeforePromoDiscountTicketPrice;
                                            custSeatDetail.TicketPrice = pssgResultDetailModel.TicketPrice;
                                            custSeatDetail.PromoDiscountAmount = pssgResultDetailModel.PromoDiscountAmount;

                                            custSeatDetail.InsuranceAmount = 0M;
                                            custSeatDetail.NetTicketPrice = pssgResultDetailModel.TicketPrice;

                                            userSession.ReturnPassengerSeatDetailList[inx] = custSeatDetail;
                                        }
                                    }

                                    returnTotalAmount = (from det in userSession.ReturnPassengerSeatDetailList
                                                            select det.NetTicketPrice).Sum();
                                    
                                }

                                userSession.DepartTotalAmount = departTotalAmount;
                                userSession.ReturnTotalAmount = returnTotalAmount;
                            }
                        }
                        else
                        {
                            if (string.IsNullOrWhiteSpace(uiCustUpdRes.ErrorMessage) == false)
                                userSession.PassengerInfoUpdateFailMessage = uiCustUpdRes.ErrorMessage;
                            else
                                userSession.PassengerInfoUpdateFailMessage = "Unable to submit passenger info; (EXIT62.0302.XS01)";

                            userSession.BookingExpiredDateTime = null;
                            userSession.DepartPendingReleaseTransNo = userSession.SeatBookingId;
                            userSession.SeatBookingId = null;
                        }
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(uiCustUpdRes.ErrorMessage) == false)
                            userSession.PassengerInfoUpdateFailMessage = uiCustUpdRes.ErrorMessage;
                        else
                            userSession.PassengerInfoUpdateFailMessage = "Unable to submit passenger info; (EXIT62.0302.XS03)";

                        userSession.DepartPendingReleaseTransNo = userSession.SeatBookingId;
                        userSession.SeatBookingId = null;
                    }
                }

                else if (svcMsg is UIETSInsuranceListAck uiETSInsrLstRes)
                {
                    if (string.IsNullOrWhiteSpace(uiETSInsrLstRes.ErrorMessage))
                    {
                        userSession.IsETSIntercityInsuranceValid = true;
                    }
                    else
                    { 
                        userSession.IsETSIntercityInsuranceValid = false;
                    }
                }

                else if (svcMsg is UIETSInsuranceSubmissionRequest uiETSInsrSubmitReq)
                {
                    userSession.IsETSInsuranceSuccessSubmission = false;
                    if (uiETSInsrSubmitReq.IsAgreeToBuyInsurance)
                    {
                        userSession.ETSInsuranceHeadersId = uiETSInsrSubmitReq.InsuranceHeadersId;
                    }
                    else
                        userSession.ETSInsuranceHeadersId = null;

                }

                else if (svcMsg is UISalesETSInsuranceSubmissionResult uiETSInsrSubmitRes)
                {
                    if ((uiETSInsrSubmitRes.MessageData is UpdateBookingInsuranceResult updInstRes)
                        && (updInstRes.Status == true) && (updInstRes.Code?.Equals("0") == true) && (updInstRes.Data?.Error?.Equals(YesNo.No) == true))
                    {
                        userSession.IsETSInsuranceSuccessSubmission = true;
                    }
                    else
                    {
                        userSession.IsETSInsuranceSuccessSubmission = false;
                    }
                }

                else if (svcMsg is UIETSCheckoutSaleResult uiETSCheckOutRes)
                {
                    bool isValidResult = true;
                    userSession.ETSIntercityCheckoutFailMessage = "Unknown error when check out sale; (EXIT62.0302.XT10)";
                    userSession.ETSIntercityCheckoutStatus = ProcessResult.Fail;

                    if (uiETSCheckOutRes.MessageData is ETSCheckoutSaleResult updStt)
                    {
                        if (updStt.Code.Equals(WebAPIAgent.ApiCodeOK))
                        {
                            CustSeatDetail custSeatDetail = null;
                            PassengerResultDetailModel pssgResultDetailModel = null;

                            decimal departTotalAmount = 0M;
                            decimal returnTotalAmount = 0M;

                            List<PassengerResultDetailModel> pssgDetList = new List<PassengerResultDetailModel>(updStt.Data.GetPassengerResult.PassengerDetailModels);
                            CheckoutBookingModel chkoutBkRes = updStt.Data.CheckoutBookingResult;

                            if (string.IsNullOrWhiteSpace(updStt.Data.GetPassengerResult.TotalAmountFormat))
                            {
                                isValidResult = false;
                                userSession.ETSIntercityCheckoutFailMessage = "Invalid total amount (I)";
                            }
                            else if (updStt.Data.GetPassengerResult.TotalAmount <= 0)
                            {
                                isValidResult = false;
                                userSession.ETSIntercityCheckoutFailMessage = $@"Invalid total amount (II); Amount : {updStt.Data.GetPassengerResult.TotalAmount}";
                            }
                            else if (string.IsNullOrWhiteSpace(updStt.Data.GetPassengerResult.Currency))
                            {
                                isValidResult = false;
                                userSession.ETSIntercityCheckoutFailMessage = "Invalid currency (I)";
                            }
                            else
                            {
                                for (int inx = 0; inx < userSession.DepartPassengerSeatDetailList.Length; inx++)
                                {
                                    custSeatDetail = userSession.DepartPassengerSeatDetailList[inx];

                                    pssgResultDetailModel = null;
                                    pssgResultDetailModel = pssgDetList.Find(p => ((p.SeatLayoutModel_Id == custSeatDetail.SeatLayoutModel_Id) && (p.SeatLayoutModel_Id.Equals(Guid.Empty) == false)));

                                    if (pssgResultDetailModel is null)
                                    {
                                        userSession.DepartPendingReleaseTransNo = userSession.SeatBookingId;
                                        userSession.SeatBookingId = null;
                                        userSession.ETSIntercityCheckoutFailMessage = "Depart Seat info mismatched";
                                        isValidResult = false;
                                        break;
                                    }

                                    custSeatDetail.Currency = pssgResultDetailModel.Currency;
                                    custSeatDetail.TicketPriceFormat = pssgResultDetailModel.TicketPriceFormat;
                                    custSeatDetail.OriginalTicketPrice = pssgResultDetailModel.BeforePromoDiscountTicketPrice;
                                    custSeatDetail.PromoDiscountAmount = pssgResultDetailModel.PromoDiscountAmount;
                                    custSeatDetail.TicketPrice = pssgResultDetailModel.TicketPrice;

                                    if (pssgResultDetailModel.IsInsurance?.Equals(YesNo.Yes) == true)
                                    {
                                        custSeatDetail.InsuranceAmount = pssgResultDetailModel.InsuranceAmount;
                                        custSeatDetail.NetTicketPrice = pssgResultDetailModel.TicketPrice + pssgResultDetailModel.InsuranceAmount;
                                    }
                                    else
                                    {
                                        custSeatDetail.InsuranceAmount = 0M;
                                        custSeatDetail.NetTicketPrice = pssgResultDetailModel.TicketPrice;
                                    }

                                    userSession.DepartPassengerSeatDetailList[inx] = custSeatDetail;
                                }

                                if (isValidResult == true)
                                {
                                    departTotalAmount = (from det in userSession.DepartPassengerSeatDetailList
                                                            select det.NetTicketPrice).Sum();
                                }
                            }

                            if ((isValidResult) && (userSession.ReturnPassengerSeatDetailList?.Length > 0))
                            {
                                for (int inx = 0; inx < userSession.ReturnPassengerSeatDetailList.Length; inx++)
                                {
                                    custSeatDetail = userSession.ReturnPassengerSeatDetailList[inx];

                                    pssgResultDetailModel = null;
                                    pssgResultDetailModel = pssgDetList.Find(p => ((p.SeatLayoutModel_Id == custSeatDetail.SeatLayoutModel_Id) && (p.SeatLayoutModel_Id.Equals(Guid.Empty) == false)));

                                    if (pssgResultDetailModel is null)
                                    {
                                        userSession.ETSIntercityCheckoutFailMessage = "Return Seat info mismatched";
                                        userSession.DepartPendingReleaseTransNo = userSession.SeatBookingId;
                                        userSession.SeatBookingId = null;
                                        isValidResult = false;
                                        break;
                                    }

                                    custSeatDetail.Currency = pssgResultDetailModel.Currency;
                                    custSeatDetail.TicketPriceFormat = pssgResultDetailModel.TicketPriceFormat;
                                    custSeatDetail.OriginalTicketPrice = pssgResultDetailModel.BeforePromoDiscountTicketPrice;
                                    custSeatDetail.TicketPrice = pssgResultDetailModel.TicketPrice;
                                    custSeatDetail.PromoDiscountAmount = pssgResultDetailModel.PromoDiscountAmount;

                                    if (pssgResultDetailModel.IsInsurance?.Equals(YesNo.Yes) == true)
                                    {
                                        custSeatDetail.InsuranceAmount = pssgResultDetailModel.InsuranceAmount;
                                        custSeatDetail.NetTicketPrice = pssgResultDetailModel.TicketPrice + pssgResultDetailModel.InsuranceAmount;
                                    }
                                    else
                                    {
                                        custSeatDetail.InsuranceAmount = 0M;
                                        custSeatDetail.NetTicketPrice = pssgResultDetailModel.TicketPrice;
                                    }

                                    userSession.ReturnPassengerSeatDetailList[inx] = custSeatDetail;
                                }

                                if (isValidResult == true)
                                {
                                    returnTotalAmount = (from det in userSession.ReturnPassengerSeatDetailList
                                                            select det.NetTicketPrice).Sum();
                                }
                            }

                            if (isValidResult)
                            {
                                userSession.BookingExpiredDateTime = DateTime.Now.AddSeconds(chkoutBkRes.BookingRemainingInSecond);
                                userSession.KtmbSalesTransactionNo = updStt.Data.CheckoutBookingResult.BookingNo;
                                userSession.TradeCurrency = updStt.Data.CheckoutBookingResult.MCurrencies_Id ?? "*RM";
                                userSession.GrossTotal = updStt.Data.CheckoutBookingResult.PayableAmount;
                                userSession.GrossTotalStr = $@"{userSession.TradeCurrency} {userSession.GrossTotal:#,###.00}";
                                userSession.ETSIntercityCheckoutStatus = ProcessResult.Success;
                                userSession.DepartTotalAmount = departTotalAmount;
                                userSession.ReturnTotalAmount = returnTotalAmount;

                                //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                            }
                            else
                            {
                                userSession.BookingExpiredDateTime = null;
                                userSession.DepartPendingReleaseTransNo = userSession.SeatBookingId;
                                userSession.SeatBookingId = null;
                            }
                        }
                        else
                        {
                            if (string.IsNullOrWhiteSpace(uiETSCheckOutRes.ErrorMessage) == false)
                                userSession.ETSIntercityCheckoutFailMessage = $@"{uiETSCheckOutRes.ErrorMessage}; (EXIT62.0302.XT01)";
                            else
                                userSession.ETSIntercityCheckoutFailMessage = "Unable to check out sale; (EXIT62.0302.XT02)";

                            userSession.BookingExpiredDateTime = null;
                            userSession.DepartPendingReleaseTransNo = userSession.SeatBookingId;
                            userSession.SeatBookingId = null;
                        }
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(uiETSCheckOutRes.ErrorMessage) == false)
                            userSession.ETSIntercityCheckoutFailMessage = $@"{uiETSCheckOutRes.ErrorMessage}; (EXIT62.0302.XT04)";
                        else
                            userSession.ETSIntercityCheckoutFailMessage = "Unable to check out sale; (EXIT62.0302.XT05)";

                        userSession.BookingExpiredDateTime = null;
                        userSession.DepartPendingReleaseTransNo = userSession.SeatBookingId;
                        userSession.SeatBookingId = null;
                    }
                }

                else if (svcMsg is UISalesPaymentSubmission uiPaymSubm)
                {
                    userSession.PaymentState = AppDecorator.Common.PaymentResult.Success;
                    userSession.PaymentTypeDesc = Enum.GetName(typeof(PaymentType), uiPaymSubm.TypeOfPayment);
                    userSession.TypeOfPayment = uiPaymSubm.TypeOfPayment;

                    userSession.Cassette1NoteCount = uiPaymSubm.Cassette1NoteCount;
                    userSession.Cassette2NoteCount = uiPaymSubm.Cassette2NoteCount;
                    userSession.Cassette3NoteCount = uiPaymSubm.Cassette3NoteCount;
                    userSession.RefundCoinAmount = uiPaymSubm.RefundCoinAmount;
                }

                else if (svcMsg is UICompleteTransactionResult uiPaymRes)
                {
                    if (uiPaymRes.ProcessState == ProcessResult.Fail)
                    {
                        userSession.DepartPendingReleaseTransNo = userSession.SeatBookingId;
                        userSession.SeatBookingId = null;
                    }
                }

                else if (svcMsg is UISeatReleaseRequest uiStRelReq)
                {
                    userSession.DepartPendingReleaseTransNo = uiStRelReq.TransactionNo;

                    userSession.DepartSeatConfirmCode = null;
                    userSession.ReturnSeatConfirmCode = null;
                    //userSession.DepartSeatConfirmTransNo = null;
                    //userSession.DepartSeatConfirmMessage = null;
                }

                else if (svcMsg is UISalesBookingTimeoutExtensionResult uiSalExtRes)
                {
                    if (uiSalExtRes.MessageData is ExtendBookingSessionResult res)
                    {
                        if (res.Code.Equals(WebAPIAgent.ApiCodeOK) && (res.Data.Error?.Equals(YesNo.No) == true))
                        {
                            //userSession.BookingExpiredDateTime = res.Data.BookingExpiredDateTime;
                            userSession.BookingExpiredDateTime = DateTime.Now.AddSeconds(res.Data.BookingRemainingInSecond);
                        }
                    }
                }
            }
            else if (userSession.VehicleGroup == TransportGroup.Komuter)
            {
                if (svcMsg is UILanguageSubmission uiLang2)
                {
                    if (_lastCounterConfiguration != null)
                    {
                        userSession.OriginStationCode = _lastCounterConfiguration.Data.CounterInfo.StationId;
                        userSession.OriginStationName = _lastCounterConfiguration.Data.CounterInfo.StationDescription;
                    }
                }
                else if (svcMsg is UIKomuterResetUserSessionRequest uiKommReset)
                {
                    LanguageCode langCode = userSession.Language;
                    Guid existingSessionId = userSession.SessionId;

                    userSession.SessionReset();

                    userSession.Expired = false;
                    userSession.SessionId = existingSessionId;
                    userSession.VehicleGroup = TransportGroup.Komuter;
                    userSession.CurrentEditMenuItemCode = null;
                    userSession.Language = langCode;
                    userSession.TravelMode = AppDecorator.Common.TravelMode.DepartOrReturn;

                    if (_lastCounterConfiguration != null)
                    {
                        userSession.ETS_Intercity_MaxPaxAllowed = _lastCounterConfiguration.Data.SystemConfiguration.TVMOtherTicket;
                        userSession.Komuter_MaxPaxAllowed = _lastCounterConfiguration.Data.SystemConfiguration.TVMKomTicket;
                        userSession.OriginStationCode = _lastCounterConfiguration.Data.CounterInfo.StationId;
                        userSession.OriginStationName = _lastCounterConfiguration.Data.CounterInfo.StationDescription;
                    }
                }
                else if (svcMsg is UIKomuterTicketTypePackageRequest uiTickPack)
                {
                    userSession.DestinationStationCode = uiTickPack.DestinationStationId;
                    userSession.DestinationStationName = uiTickPack.DestinationStationName;
                }

                else if (svcMsg is UIKomuterTicketBookingRequest uiTickBookReq)
                {
                    userSession.TicketPackageId = uiTickBookReq.KomuterPackageId;
                    userSession.TicketItemList = uiTickBookReq.TicketItemList;
                }

                else if (svcMsg is UIKomuterTicketBookingAck uiTickBkAck)
                {
                    if ((uiTickBkAck.MessageData is KomuterBookingResult bookRes)
                        && (bookRes.Code.Equals(WebAPIAgent.ApiCodeOK))
                        )
                    {
                        userSession.SeatBookingId = bookRes.Data.Booking_Id;
                        userSession.KtmbSalesTransactionNo = bookRes.Data.BookingNo;
                        userSession.BookingAmount = bookRes.Data.TotalAmount;
                        userSession.BookingCurrency = bookRes.Data.MCurrencies_Id;
                        //userSession.BookingExpiredDateTime = bookRes.Data.BookingExpiredDateTime;
                        //userSession.BookingExpiredDateTime = DateTime.Now.AddSeconds(bookRes.Data.BookingRemainingInSecond);
                    }
                    else
                    {
                        userSession.SeatBookingId = null;
                        userSession.KtmbSalesTransactionNo = null;
                        userSession.BookingAmount = 0;
                        userSession.BookingCurrency = null;
                        userSession.BookingExpiredDateTime = null;
                    }
                }

                else if (svcMsg is UIKomuterBookingCheckoutRequest uiBkChkoutReq)
                {
                    userSession.FinancePaymentMethod = uiBkChkoutReq.FinancePaymentMethod;
                }

                else if (svcMsg is UIKomuterBookingCheckoutAck uiBkChkoutAck)
                {
                    if ((uiBkChkoutAck.MessageData is KomuterBookingCheckoutResult chkOutRes)
                        && (chkOutRes.Code.Equals(WebAPIAgent.ApiCodeOK))
                        )
                    {
                        userSession.GrossTotal = chkOutRes.Data.PayableAmount;
                        userSession.TradeCurrency = chkOutRes.Data.MCurrencies_Id;
                        //userSession.BookingExpiredDateTime = chkOutRes.Data.BookingExpiredDateTime;
                        userSession.BookingExpiredDateTime = DateTime.Now.AddSeconds(chkOutRes.Data.BookingRemainingInSecond);
                    }
                    else
                    {
                        userSession.GrossTotal = 0.0M;
                        userSession.TradeCurrency = null;
                    }
                }

                else if (svcMsg is UIKomuterCompletePaymentSubmission uiKomPayAck)
                {
                    userSession.PaymentState = AppDecorator.Common.PaymentResult.Success; 
                }

                else if (svcMsg is UISalesBookingTimeoutExtensionResult uiSalExtRes)
                {
                    if (uiSalExtRes.MessageData is ExtendBookingSessionResult res)
                    {
                        if (res.Code.Equals(WebAPIAgent.ApiCodeOK) && (res.Data.Error?.Equals(YesNo.No) == true))
                        {
                            //userSession.BookingExpiredDateTime = res.Data.BookingExpiredDateTime;
                            userSession.BookingExpiredDateTime = DateTime.Now.AddSeconds(res.Data.BookingRemainingInSecond);
                        }
                    }
                }
            }

            return userSession;
        }

        /// <summary>
        /// FuncCode:EXIT62.0303
        /// </summary>
        public UserSession SetUIPageNavigateSession(UserSession userSession, UIPageNavigateRequest pageNav)
        {
            if (pageNav.NavigateDirection == PageNavigateDirection.Previous)
            {
                if (userSession.CurrentEditMenuItemCode.HasValue == false)
                {  }

                // Note : Passenger cannot back to ReturnSeat
                //else if (userSession.CurrentEditMenuItemCode == TickSalesMenuItemCode.Passenger)
                //{
                //    userSession = SetEditingSession(userSession, TickSalesMenuItemCode.ReturnSeat);
                //}

                else if (userSession.CurrentEditMenuItemCode == TickSalesMenuItemCode.ReturnSeat)
                {
                    userSession = SetEditingSession(userSession, TickSalesMenuItemCode.ReturnTrip);
                }

                // Note : ReturnTrip cannot back to DepartSeat
                //else if (userSession.CurrentEditMenuItemCode == TickSalesMenuItemCode.ReturnTrip)
                //{
                //    userSession = SetEditingSession(userSession, TickSalesMenuItemCode.DepartSeat);
                //}

                else if (userSession.CurrentEditMenuItemCode ==  TickSalesMenuItemCode.DepartSeat)
                {
                    userSession = SetEditingSession(userSession, TickSalesMenuItemCode.DepartTrip); 
                }
                else if (userSession.CurrentEditMenuItemCode == TickSalesMenuItemCode.DepartTrip)
                {
                    userSession = SetEditingSession(userSession, TickSalesMenuItemCode.TravelDates);
                }
                else if (userSession.CurrentEditMenuItemCode == TickSalesMenuItemCode.TravelDates)
                {
                    userSession = SetEditingSession(userSession, TickSalesMenuItemCode.ToStation);
                }
                else if (userSession.CurrentEditMenuItemCode == TickSalesMenuItemCode.ToStation)
                {
                    userSession = SetEditingSession(userSession, TickSalesMenuItemCode.FromStation);
                }
            }
            else if (pageNav.NavigateDirection == PageNavigateDirection.Exit)
            {
                // Below used to release seat if seat has already booked.
                userSession = SetEditingSession(userSession, TickSalesMenuItemCode.FromStation);
            }

            return userSession;
        }

        /// <summary>
        /// FuncCode:EXIT62.0304
        /// </summary>
        public UserSession SetEditingSession(UserSession userSession, TickSalesMenuItemCode detailItemCode)
        {
            // If user at Passenger Names Page, he is only allowed to Exit
            // If User at ReturnSeat, he is allowed back to ReturnTrip. But ReturnTrip NOT allowed back to DepartSeat
            //     : ReturnTrip anly allowed Exit.
            // If User at DepartSeat, he is allowed back to DepartTrip. 


            //if (detailItemCode == TickSalesMenuItemCode.ReturnSeat)
            //{
            //    userSession = ResetReturnSeat(userSession);
            //}
            if (detailItemCode == TickSalesMenuItemCode.ReturnTrip)
            {
                userSession = ResetReturnSeat(userSession);
                userSession = ResetReturnTrip(userSession);
            }
            //if (detailItemCode == TickSalesMenuItemCode.DepartSeat)
            //{
            //    userSession = ResetReturnSeat(userSession);
            //    userSession = ResetReturnOperator(userSession);
            //    userSession = ResetDepartSeat(userSession);
            //}
            if (detailItemCode == TickSalesMenuItemCode.DepartTrip)
            {
                userSession = ResetReturnSeat(userSession);
                userSession = ResetReturnTrip(userSession);
                userSession = ResetDepartSeat(userSession);
                userSession = ResetDepartTrip(userSession);
            } 
            else if (detailItemCode == TickSalesMenuItemCode.TravelDates)
            {
                userSession = ResetReturnSeat(userSession);
                userSession = ResetReturnTrip(userSession);
                userSession = ResetDepartSeat(userSession);
                userSession = ResetDepartTrip(userSession);
                userSession = ResetTravelDates(userSession);
            }
            else if (detailItemCode == TickSalesMenuItemCode.ToStation)
            {
                userSession = ResetReturnSeat(userSession);
                userSession = ResetReturnTrip(userSession);
                userSession = ResetDepartSeat(userSession);
                userSession = ResetDepartTrip(userSession);
                userSession = ResetTravelDates(userSession);
                userSession = ResetDestStation(userSession);
            }
            else if (detailItemCode == TickSalesMenuItemCode.FromStation)
            {
                userSession = ResetReturnSeat(userSession);
                userSession = ResetReturnTrip(userSession);
                userSession = ResetDepartSeat(userSession);
                userSession = ResetDepartTrip(userSession);
                userSession = ResetTravelDates(userSession);
                userSession = ResetDestStation(userSession);
                userSession = ResetOriginStation(userSession);
            }

            return userSession;

            UserSession ResetOriginStation(UserSession session)
            {
                session.OriginStationCode = null;
                session.OriginStationName = null;
                session.OriginAvailableTrainService = null;
                return session;
            }

            UserSession ResetDestStation(UserSession session)
            {
                session.DestinationStationCode = null;
                session.DestinationStationName = null;
                session.DestinationAvailableTrainService = null;
                return session;
            }

            UserSession ResetTravelDates(UserSession session)
            {
                session.NumberOfPassenger = 1;
                userSession.TravelMode = AppDecorator.Common.TravelMode.DepartOrReturn;
                session.SetTravelDate(session.TravelMode, null, null);
                return session;
            }

            UserSession ResetDepartTrip(UserSession session)
            {
                //userSession.SetTravelDate(userSession.TravelMode, null, null);
                session.DepartTripId = null;
                session.DepartPassengerArrivalDate = null;
                session.DepartPassengerDepartTimeStr = null;
                session.DepartPassengerArrivalDayOffset = 0;
                session.DepartPassengerArrivalTimeStr = null;
                session.DepartVehicleService = null;
                session.DepartVehicleNo = null;
                session.DepartServiceCategory = null;
                session.DepartCurrency = null;
                session.DepartPrice = 0.00M;

                return session;
            }

            UserSession ResetDepartSeat(UserSession session)
            {
                session.DepartPassengerSeatDetailList = null;

                if ((string.IsNullOrWhiteSpace(session.SeatBookingId) == false) &&
                    (session.DepartSeatConfirmCode?.Trim().Equals("0") == true))
                    session.DepartPendingReleaseTransNo = session.SeatBookingId;
                else
                    session.DepartPendingReleaseTransNo = null;

                session.SeatBookingId = null;
                session.DepartSeatConfirmCode = null;
                session.ReturnSeatConfirmCode = null;

                session.DepartPassengerSeatDetailList = null;
                session.PassengerInfoUpdateStatus = ProcessResult.Fail;
                session.IsAllowedETSIntercityInsurance = false;
                session.ETSIntercityCheckoutFailMessage = null;
                session.ETSIntercityCheckoutStatus = ProcessResult.Fail;

                return session;
            }

            UserSession ResetReturnTrip(UserSession session)
            {
                //userSession.SetTravelDate(userSession.TravelMode, null, null);
                session.ReturnTripId = null;
                session.ReturnPassengerArrivalDate = null;
                session.ReturnPassengerDepartTimeStr = null;
                session.ReturnPassengerArrivalDayOffset = 0;
                session.ReturnPassengerArrivalTimeStr = null;
                session.ReturnVehicleService = null;
                session.ReturnVehicleNo = null;
                session.ReturnServiceCategory = null;
                session.ReturnCurrency = null;
                session.ReturnPrice = 0.00M;

                return session;
            }

            UserSession ResetReturnSeat(UserSession session)
            {
                session.ReturnPassengerSeatDetailList = null;
                session.ReturnSeatConfirmCode = null;
                session.PassengerInfoUpdateStatus = ProcessResult.Fail;
                session.IsAllowedETSIntercityInsurance = false;
                session.ETSIntercityCheckoutFailMessage = null;
                session.ETSIntercityCheckoutStatus = ProcessResult.Fail;

                return session;
            }
        }
    }
}
