using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI;
using NssIT.Kiosk.AppDecorator.DomainLibs.PaymentGateway.UIx;
using NssIT.Kiosk.Client.NetClient;
using NssIT.Train.Kiosk.Common.Data;
using NssIT.Train.Kiosk.Common.Data.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NssIT.Kiosk.Client
{
    public class AppSalesSvcEventsHandler
    {
        private string _logChannel = "AppSys";

        private NetClientService _netClientSvc = null;

        public AppSalesSvcEventsHandler(NetClientService netClientSvc)
        {
            _netClientSvc = netClientSvc;

            _netClientSvc.SalesService.OnDataReceived += SalesService_OnDataReceived;
            _netClientSvc.BTnGService.OnDataReceived += BTnGService_OnDataReceived;
        }

        private void BTnGService_OnDataReceived(object sender, AppDecorator.Common.AppService.Network.DataReceivedEventArgs e)
        {
            if (e.ReceivedData?.MsgObject?.GetMsgData() is IUIxBTnGPaymentOngoingGroupAck)
            {
                App.MainScreenControl?.BTnGShowPaymentInfo(e.ReceivedData.MsgObject);
            }
        }

        private void SalesService_OnDataReceived(object sender, AppDecorator.Common.AppService.Network.DataReceivedEventArgs e)
        {
            if (App.IsClientReady == false)
                return;

            string relatedStage = "";
            try
            {
                if (e?.ReceivedData?.MsgObject is UISalesClientMaintenanceAck uiMtn)
                {
                    relatedStage = "*Client Maintenance*";
                    App.ShowDebugMsg($@"Found Client Maintenance Ack Instruction");
                    App.MainScreenControl?.ShowMaintenance();
                }

                else if (e?.ReceivedData?.MsgObject is UISalesCheckOutstandingCardSettlementAck outCrdSettAck)
                {
                    relatedStage = "*Check Outstanding Card Settlement*";
                    App.ShowDebugMsg($@"Found Outstanding Card Settlement answer");
                    App.MainScreenControl?.AcknowledgeOutstandingCardSettlement(outCrdSettAck);
                }

                else if (e?.ReceivedData?.MsgObject is UISalesCardSettlementStatusAck CrdSettSttAck)
                {
                    relatedStage = "*Check Outstanding Card Settlement*";
                    App.ShowDebugMsg($@"Found Card Settlement DB Transaction Status answer");
                    App.MainScreenControl?.AcknowledgeCardSettlementDBTransStatus(CrdSettSttAck);
                }

                else if (e?.ReceivedData?.MsgObject is UILanguageSelectionAck uiLang)
                {
                    relatedStage = "*Language Selection*";
                    App.ShowDebugMsg($@"Found showing UI Language Instruction");
                    App.MainScreenControl?.ChooseLanguage(uiLang);
                }

                else if (e?.ReceivedData?.MsgObject is UICountDownExpiredAck uiCntExp)
                {
                    relatedStage = "*Count Down Expired*";
                    App.LatestUserSession = new AppDecorator.Common.AppService.Sales.UserSession() { Expired = true };
                    App.ShowDebugMsg($@"Working Count Down Expired");
                    App.Log.LogText(_logChannel, "*", "CountDownExpiredAck", "A05", "AppSalesSvcEventsHandler.SalesService_OnDataReceived");
                    App.MainScreenControl?.ShowWelcome();
                }
                //XXXXX
                else if (e?.ReceivedData?.MsgObject is UIOriginListAck uiOrig)
                {
                    relatedStage = "*Origin List*";
                    if (uiOrig.MessageData is StationResult origList)
                    {
                        if (origList.Code.Equals("0"))
                        {
                            App.LatestUserSession = uiOrig.Session;
                            App.ShowDebugMsg($@"StationResult has received at SalesService");
                            App.MainScreenControl?.ChooseOriginStation(uiOrig, uiOrig.Session);
                        }
                        else
                        {
                            App.ShowDebugMsg($@"Unabled to read Origin station list; (EXIT500032); WebCode: {origList.Code}; ErrMsg: {origList.MessageString()}");
                            throw new Exception($@"Unabled to read Origin station list; (EXIT500032); WebCode: {origList.Code}; ErrMsg: {origList.MessageString()}");
                        }
                    }
                    else if (string.IsNullOrWhiteSpace(uiOrig.ErrorMessage) == false)
                    {
                        App.ShowDebugMsg($@"Error Reading Origin Data; (EXIT500033); {uiOrig.ErrorMessage}");
                        throw new Exception($@"Error Reading Origin Data; (EXIT500033); {uiOrig.ErrorMessage}");
                    }
                    else
                    {
                        App.ShowDebugMsg($@"Unable to read origin data (EXIT500034)");
                        throw new Exception($@"Unable to read origin data (EXIT500034)");
                    }
                }
                //XXXXX
                else if (e?.ReceivedData?.MsgObject is UIDestinationListAck uiDest)
                {
                    relatedStage = "*Destination List*";
                    if (uiDest.MessageData is StationResult destList)
                    {
                        if (destList.Code.Equals("0"))
                        {
                            App.LatestUserSession = uiDest.Session;
                            App.ShowDebugMsg($@"StationResult has received at SalesService");
                            App.MainScreenControl?.ChooseDestinationStation(uiDest, uiDest.Session);
                        }
                        else
                        {
                            App.ShowDebugMsg($@"Unabled to read Destination station list; (EXIT500023); WebCode: {destList.Code}; ErrMsg: {destList.MessageString()}");
                            throw new Exception($@"Unabled to read Destination station list; (EXIT500023); WebCode: {destList.Code}; ErrMsg: {destList.MessageString()}");
                        }
                    }
                    else if (string.IsNullOrWhiteSpace(uiDest.ErrorMessage) == false)
                    {
                        App.ShowDebugMsg($@"Error Reading Destination Data; (EXIT500024); {uiDest.ErrorMessage}");
                        throw new Exception($@"Error Reading Destination Data; (EXIT500024); {uiDest.ErrorMessage}");
                    }
                    else
                    {
                        App.ShowDebugMsg($@"Unable to read destination data (EXIT500021)");
                        throw new Exception($@"Unable to read destination data (EXIT500021)");
                    }
                }
                else if (e?.ReceivedData?.MsgObject is UITravelDatesEnteringAck uiTravelDates)
                {
                    relatedStage = "*Travel Dates Enter*";
                    App.LatestUserSession = uiTravelDates.Session;
                    App.ShowDebugMsg($@"Show Travel Dates Entering");
                    App.MainScreenControl?.ChooseTravelDates(uiTravelDates, uiTravelDates.Session);
                }

                else if (e?.ReceivedData?.MsgObject is UIDepartTripInitAck uiTripInit)
                {
                    relatedStage = "*Depart Trip Init*";
                    App.LatestUserSession = uiTripInit.Session;
                    App.ShowDebugMsg($@"Show Depart Trip Selection Page");
                    App.MainScreenControl?.ShowInitDepartTrip(uiTripInit, uiTripInit.Session);
                }

                else if (e?.ReceivedData?.MsgObject is UIDepartTripListAck uiDTrip)
                {
                    relatedStage = "*Depart Trip List*";
                    TripResult ts = null;

                    if (uiDTrip.MessageData is null)
                    {
                        throw new Exception("Unabled to read Depart Trip list; (EXIT500022)");
                    }
                    else
                    {
                        ts = (TripResult)uiDTrip.MessageData;

                        // Code = 4 mean no record found
                        if (ts.Code.Equals("0"))
                        {
                            App.LatestUserSession = uiDTrip.Session;
                        }
                        //else if (ts.code == 4)
                        //{
                        //    App.LatestUserSession = uiDTrip.Session;
                        //    ts.details = new trip_detail[0];
                        //    ts.companyLogoURL = "";
                        //}
                        //else if (ts.code != 0)
                        else
                        {
                            throw new Exception($@"Unabled to read Depart Trip data; (EXIT500024); WebCode: {ts.Code}; ErrMsg: {ts.MessageString()}");
                        }
                    }

                    App.ShowDebugMsg($@"Show Depart Trip Selection Page");
                    App.MainScreenControl?.UpdateDepartTripList(uiDTrip, uiDTrip.Session);
                }

                //else if (e?.ReceivedData?.MsgObject is UIDepartTripSubmissionErrorAck uiTripSubErr)
                //{
                //    App.ShowDebugMsg($@"Error When submit Trip");
                //    App.MainScreenControl?.UpdateDepartTripSubmitError(uiTripSubErr, uiTripSubErr.Session);
                //}

                else if (e?.ReceivedData?.MsgObject is UIDepartSeatListAck uiDSeat)
                {
                    relatedStage = "*Depart Seat List*";
                    TrainSeatResult sStt = null;

                    if (uiDSeat.MessageData is null)
                    {
                        throw new Exception("Unabled to read Depart Seat list; (EXIT500025)");
                    }
                    else
                    {
                        sStt = (TrainSeatResult)uiDSeat.MessageData;

                        if (sStt.Code.Equals("0"))
                        {
                            App.LatestUserSession = uiDSeat.Session;

                            if (sStt.Data is null)
                                sStt.Data = new TrainCoachSeatModel();

                            if (sStt.Data.CoachModels is null)
                                sStt.Data.CoachModels = new CoachModel[0];

                            if ((sStt.Data.CoachModels.Length > 0) && (sStt.Data.CoachModels[0] is null))
                                sStt.Data.CoachModels[0] = new CoachModel();

                            if ((sStt.Data.CoachModels.Length > 0) && (sStt.Data.CoachModels[0].SeatLayoutModels is null))
                                sStt.Data.CoachModels[0].SeatLayoutModels = new SeatLayoutModel[0];
                        }
                        else 
                        {
                            throw new Exception($@"Unabled to read Depart Seat data; (EXIT500026); WebCode: {sStt.Code}; ErrMsg: {sStt.MessageString()}");
                        }
                    }

                    App.ShowDebugMsg($@"Show Depart Seat Selection Page");
                    App.MainScreenControl?.ChooseDepartSeat(uiDSeat, uiDSeat.Session);
                }

                else if (e?.ReceivedData?.MsgObject is UIDepartPickupNDropAck uiDepPnD)
                {
                    relatedStage = "*Depart Pickup & Drop*";
                    PickupNDropList pnd = null;

                    if (uiDepPnD.MessageData is null)
                    {
                        throw new Exception("Unabled to read Pickup and Drop Off options; (EXIT500027)");
                    }
                    else
                    {
                        pnd = (PickupNDropList)uiDepPnD.MessageData;

                        if ((pnd.PickDetailList is null) || (pnd.PickDetailList.Length == 0))
                        {
                            throw new Exception($@"Unabled to read Pickup options; (EXIT500028);");
                        }
                        else if ((pnd.DropDetailList is null) || (pnd.DropDetailList.Length == 0))
                        {
                            throw new Exception($@"Unabled to read Drop Off options; (EXIT500029);");
                        }
                    }

                    App.ShowDebugMsg($@"Show Pickup and Drop Off Selection Page");
                    App.MainScreenControl?.ChoosePickupNDrop(uiDepPnD);
                }

                else if (e?.ReceivedData?.MsgObject is UIDepartSeatConfirmFailAck uiDpStCfmFail)
                {
                    relatedStage = "*Depart Seat Confirm Fail*";
                    string errMsg = uiDpStCfmFail.ErrorMessage;
                    errMsg = $@"{errMsg}; (EXIT500030); Please try again";

                    App.ShowDebugMsg($@"Fail Depart Seat Confirm; {errMsg}");
                    App.MainScreenControl?.Alert("TIDAK MENGESAHKAN", "UNABLE CONFIRM", detailMsg: $@"{errMsg}");
                }

                else if (e?.ReceivedData?.MsgObject is UIReturnTripInitAck uiRTripInit)
                {
                    relatedStage = "*Return Trip Init*";
                    App.LatestUserSession = uiRTripInit.Session;
                    App.ShowDebugMsg($@"Show Return Trip Selection Page");
                    App.MainScreenControl?.ShowInitReturnTrip(uiRTripInit, uiRTripInit.Session);
                }

                else if (e?.ReceivedData?.MsgObject is UIReturnTripListAck uiRTrip)
                {
                    relatedStage = "*Return Trip List*";
                    TripResult ts = null;

                    if (uiRTrip.MessageData is null)
                    {
                        throw new Exception("Unabled to read Return Trip list; (EXIT500032)");
                    }
                    else
                    {
                        ts = (TripResult)uiRTrip.MessageData;

                        if (ts.Code.Equals("0"))
                        {
                            App.LatestUserSession = uiRTrip.Session;
                        }
                        else
                        {
                            throw new Exception($@"Unabled to read Return Trip data; (EXIT500033); WebCode: {ts.Code}; ErrMsg: {ts.MessageString()}");
                        }
                    }

                    App.ShowDebugMsg($@"Show Return Trip Selection Page");
                    App.MainScreenControl?.UpdateReturnTripList(uiRTrip, uiRTrip.Session);
                }

                //xxxxx

                //else if (e?.ReceivedData?.MsgObject is UIReturnTripSubmissionErrorAck uiRTripSubErr)
                //{
                //    App.ShowDebugMsg($@"Error When submit Trip");
                //    App.MainScreenControl?.UpdateReturnTripSubmitError(uiRTripSubErr, uiRTripSubErr.Session);
                //}

                else if (e?.ReceivedData?.MsgObject is UIReturnSeatListAck uiRSeat)
                {
                    relatedStage = "*Return Seat List*";
                    TrainSeatResult sStt = null;

                    if (uiRSeat.MessageData is null)
                    {
                        throw new Exception("Unabled to read Return Seat list; (EXIT500036)");
                    }
                    else
                    {
                        sStt = (TrainSeatResult)uiRSeat.MessageData;

                        if (sStt.Code.Equals("0"))
                        {
                            App.LatestUserSession = uiRSeat.Session;

                            if (sStt.Data is null)
                                sStt.Data = new TrainCoachSeatModel();

                            if (sStt.Data.CoachModels is null)
                                sStt.Data.CoachModels = new CoachModel[0];

                            if ((sStt.Data.CoachModels.Length > 0) && (sStt.Data.CoachModels[0] is null))
                                sStt.Data.CoachModels[0] = new CoachModel() { CoachId = Guid.NewGuid().ToString("D"), CoachLabel = "-", SeatColumn = 0, SeatRow = 0 };

                            if ((sStt.Data.CoachModels.Length > 0) && (sStt.Data.CoachModels[0].SeatLayoutModels is null))
                                sStt.Data.CoachModels[0].SeatLayoutModels = new SeatLayoutModel[0];
                        }
                        else
                        {
                            throw new Exception($@"Unabled to read Return Seat data; (EXIT500037); WebCode: {sStt.Code}; ErrMsg: {sStt.MessageString()}");
                        }
                    }

                    App.ShowDebugMsg($@"Show Depart Seat Selection Page");
                    App.MainScreenControl?.ChooseReturnSeat(uiRSeat, uiRSeat.Session);
                }

                //else if (e?.ReceivedData?.MsgObject is UIDepartPickupNDropAck uiDepPnD)
                //{
                //    relatedStage = "*Depart Pickup & Drop*";
                //    PickupNDropList pnd = null;

                //    if (uiDepPnD.MessageData is null)
                //    {
                //        throw new Exception("Unabled to read Pickup and Drop Off options; (EXIT500027)");
                //    }
                //    else
                //    {
                //        pnd = (PickupNDropList)uiDepPnD.MessageData;

                //        if ((pnd.PickDetailList is null) || (pnd.PickDetailList.Length == 0))
                //        {
                //            throw new Exception($@"Unabled to read Pickup options; (EXIT500028);");
                //        }
                //        else if ((pnd.DropDetailList is null) || (pnd.DropDetailList.Length == 0))
                //        {
                //            throw new Exception($@"Unabled to read Drop Off options; (EXIT500029);");
                //        }
                //    }

                //    App.ShowDebugMsg($@"Show Pickup and Drop Off Selection Page");
                //    App.MainScreenControl?.ChoosePickupNDrop(uiDepPnD);
                //}

                else if (e?.ReceivedData?.MsgObject is UIReturnSeatConfirmFailAck uiRtStCfmFail)
                {
                    relatedStage = "*Return Seat Confirm Fail*";
                    string errMsg = uiRtStCfmFail.ErrorMessage;
                    errMsg = $@"{errMsg}; (EXIT500039); Please try again";

                    App.ShowDebugMsg($@"Fail Return Seat Confirm; {errMsg}");
                    App.MainScreenControl?.Alert("TIDAK MENGESAHKAN", "UNABLE CONFIRM", detailMsg: $@"{errMsg}");
                }

                else if (e?.ReceivedData?.MsgObject is UICustPromoCodeVerifyAck uiCustPromoCode)
                {
                    relatedStage = "*Promo Code Validation*";
                    PromoCodeVerifyResult sStt = null;

                    if (uiCustPromoCode.MessageData is null)
                    {
                        throw new Exception("Unabled to read data when validate Promo Code; (EXIT500051)");
                    }
                    else
                    {
                        sStt = (PromoCodeVerifyResult)uiCustPromoCode.MessageData;

                        if (sStt.Code.Equals("0"))
                        {
                            App.LatestUserSession = uiCustPromoCode.Session;
                        }
                        //else
                        //{
                        //    throw new Exception($@"Unabled to read data when validate Promo Code; (EXIT5000xx); WebCode: {sStt?.Code}; ErrMsg: {sStt?.MessageString()}");
                        //}
                    }

                    App.ShowDebugMsg($@"Found Validate Promo Code Result .. ");
                    App.MainScreenControl?.UpdatePromoCodeVerificationResult(uiCustPromoCode);
                }

                else if (e?.ReceivedData?.MsgObject is UICustInfoPNRTicketTypeAck uiPNRTicketType)
                {
                    relatedStage = "*Promo Code Validation*";
                    PassengerPNRTicketTypeResult sStt = null;

                    if (uiPNRTicketType.MessageData is null)
                    {
                        throw new Exception("Unabled to read data when validate Promo Code; (EXIT500035)");
                    }
                    else
                    {
                        //sStt = (PassengerPNRTicketTypeResult)uiPNRTicketType.MessageData;

                        //if (sStt.Code.Equals("0"))
                        //{
                        //    App.LatestUserSession = uiPNRTicketType.Session;
                        //}

                        //else
                        //{
                        //    throw new Exception($@"Unabled to read data when validate Promo Code; (EXIT5000xx); WebCode: {sStt?.Code}; ErrMsg: {sStt?.MessageString()}");
                        //}
                    }

                    //App.ShowDebugMsg($@"Found Validate Promo Code Result .. ");
                    App.MainScreenControl?.UpdatePNRTicketTicketResult(uiPNRTicketType);
                }

                else if (e?.ReceivedData?.MsgObject is UIETSCheckoutSaleFailAck uiETSChkFail)
                {
                    relatedStage = "*Fail ETS/Intercity Check out Ticket Sale *";
                    string errMsg = uiETSChkFail.ErrorMessage;
                    errMsg = $@"{errMsg}; (EXIT500052); Please try again";

                    App.ShowDebugMsg($@"{relatedStage}; {errMsg}");
                    App.MainScreenControl?.Alert("GAGAL CHECK OUT", "CHECK OUT FAIL", detailMsg: $@"{errMsg}");
                }

                else if (e?.ReceivedData?.MsgObject is UIKomuterTicketTypePackageAck uiKomTickPack)
                {
                    relatedStage = "*Komuter Ticket Type Packages*";
                    KomuterTicketTypePackageResult packRes = null;

                    if (uiKomTickPack.MessageData is null)
                    {
                        throw new Exception("Unabled to read Komuter Ticket Type Packages; (EXIT500040)");
                    }
                    else
                    {
                        packRes = (KomuterTicketTypePackageResult)uiKomTickPack.MessageData;

                        if (packRes.Code.Equals("0"))
                        {
                            App.LatestUserSession = uiKomTickPack.Session;

                            if (packRes.Data.KomuterPackages.Length == 0)
                                throw new Exception("Komuter Package not found from web server; (EXIT500041)");

                            bool validPaxOption = false;

                            foreach (KomuterPackageModel pack in packRes.Data.KomuterPackages)
                                if (pack.TicketTypes?.Length > 0)
                                {
                                    validPaxOption = true;
                                    break;
                                }

                            
                            if (validPaxOption == false)
                                throw new Exception("Pax(Passenger) selection option not set in web server; (EXIT500042)");
                        }
                        else
                        {
                            throw new Exception($@"Unabled to Komuter Ticket Type; (EXIT500043); WebCode: {packRes.Code}; ErrMsg: {packRes.MessageString()}");
                        }
                    }

                    App.ShowDebugMsg($@"Update Komuter Ticket Type Package .. ");
                    App.MainScreenControl?.UpdateKomuterTicketTypePackage(uiKomTickPack);
                }

                else if (e?.ReceivedData?.MsgObject is UIKomuterTicketBookingAck uiKomBkAck)
                {
                    relatedStage = "*Komuter Ticket Booking*";
                    KomuterBookingResult ts = null;

                    if (uiKomBkAck.MessageData is null)
                    {
                        throw new Exception("Unabled to read Komuter Ticket Booking status; (EXIT500045)");
                    }
                    else
                    {
                        ts = (KomuterBookingResult)uiKomBkAck.MessageData;

                        if (ts.Code.Equals("0"))
                        {
                            App.LatestUserSession = uiKomBkAck.Session;
                        }
                        else
                        {
                            throw new Exception($@"Unabled to read Komuter Ticket Booking status; (EXIT500046); WebCode: {ts.Code}; ErrMsg: {ts.MessageString()}");
                        }
                    }

                    App.ShowDebugMsg($@"Show Komuter Ticket Booking status");
                    App.MainScreenControl?.UpdateKomuterTicketBooking(uiKomBkAck);
                }

                else if (e?.ReceivedData?.MsgObject is UIKomuterBookingCheckoutAck uiKomChkoutAck)
                {
                    relatedStage = "*Checkout Komuter Ticket Booking*";
                    KomuterBookingCheckoutResult ts = null;

                    if (uiKomChkoutAck.MessageData is null)
                    {
                        throw new Exception("Unabled to read Komuter Ticket Booking Checkout status; (EXIT500047)");
                    }
                    else
                    {
                        ts = (KomuterBookingCheckoutResult)uiKomChkoutAck.MessageData;

                        if (ts.Code.Equals("0"))
                        {
                            App.LatestUserSession = uiKomChkoutAck.Session;
                            DateTime expiredTime = DateTime.Now.AddSeconds(ts.Data.BookingRemainingInSecond);
                            App.BookingTimeoutMan.UpdateBookingTimeout(expiredTime);
                        }
                        else
                        {
                            throw new Exception($@"Unabled to read Komuter Ticket Booking Checkout status; (EXIT500048); WebCode: {ts.Code}; ErrMsg: {ts.MessageString()}");
                        }
                    }

                    App.ShowDebugMsg($@"Show Komuter Ticket Booking Checkout status");
                    App.MainScreenControl?.UpdateKomuterBookingCheckoutResult(uiKomChkoutAck);
                }

                else if (e?.ReceivedData?.MsgObject is UIKomuterCompletePaymentAck uiKomPaymAck)
                {
                    relatedStage = "*Komuter Booking Payment*";
                    CompleteKomuterPaymentResult ts = null;

                    if (uiKomPaymAck.MessageData is null)
                    {
                        throw new Exception("Unabled to read Komuter Ticket Booking Payment status; (EXIT500049)");
                    }
                    else
                    {
                        ts = (CompleteKomuterPaymentResult)uiKomPaymAck.MessageData;

                        if (ts.Code.Equals("0"))
                        {
                            App.LatestUserSession = uiKomPaymAck.Session;
                        }
                        else
                        {
                            throw new Exception($@"Unabled to read Komuter Ticket Booking Payment status; (EXIT500050); WebCode: {ts.Code}; ErrMsg: {ts.MessageString()}");
                        }
                    }

                    App.ShowDebugMsg($@"Show Komuter Ticket Booking Payment status");
                    App.MainScreenControl?.UpdateKomuterTicketPaymentStatus(uiKomPaymAck);
                }

                //xxxxx

                else if (e?.ReceivedData?.MsgObject is UICustInfoAck uiCust)
                {
                    relatedStage = "*Cust. Info*";
                    GetTicketTypeResult ttr = (GetTicketTypeResult)uiCust.MessageData;

                    if ((ttr is null) || (ttr.Code.Equals("0") == false) || (ttr.Data is null) || (ttr.Data.Length == 0))
                        throw new Exception("No valid Ticket Type found;");

                    App.ShowDebugMsg($@"Show Passenger Info Entry Page");
                    App.MainScreenControl?.EnterPassengerInfo(uiCust);
                }

                else if (e?.ReceivedData?.MsgObject is UICustInfoUpdateFailAck uiFailUpdCustInfo)
                {
                    if (
                        (uiFailUpdCustInfo.IsRequestAmendPassengerInfo == true) 
                        && (uiFailUpdCustInfo.MessageData is PassengerSubmissionResult res)
                        && (res.Data?.UpdatePassengerResult?.PassengerDetailErrorModels?.Length > 0)
                        )
                    {
                        relatedStage = "*Amend Passenger Info*";
                        
                        App.ShowDebugMsg($@"Show Passenger Info Entry Page - Amend Passenger Info");
                        App.MainScreenControl?.RequestAmentPassengerInfo(uiFailUpdCustInfo);
                    }
                    else
                    {
                        relatedStage = "*Cust Info Update Fail*";
                        string errMsg = (string.IsNullOrWhiteSpace(uiFailUpdCustInfo.ErrorMessage) == false) ? uiFailUpdCustInfo.ErrorMessage.Trim() : "Unable to update your information at the moment";
                        errMsg = $@"{errMsg}; (EXIT500031); Please try again";

                        App.ShowDebugMsg($@"Fail Update Customer Info; {errMsg}");
                        App.MainScreenControl?.Alert("TIDAK DIKEMAS KINI", "UNABLE UPDATE", detailMsg: $@"{errMsg}");
                    }
                }

                else if (e?.ReceivedData?.MsgObject is UIETSInsuranceListAck uiETSInsrLst)
                {
                    relatedStage = "*ETS Insurance*";
                    App.ShowDebugMsg($@"Show ETS Insurance Page");
                    App.MainScreenControl?.ChooseInsurance(uiETSInsrLst);
                }

                else if (e?.ReceivedData?.MsgObject is UISalesStartSellingAck uiSalesStart)
                {
                    relatedStage = "*Make Sales Payment*";
                    App.ShowDebugMsg($@"Received UIMakeSalesPaymentAck");
                    App.MainScreenControl?.StartSelling(uiSalesStart);
                }

                else if (e?.ReceivedData?.MsgObject is UISalesPaymentProceedAck uiSalesPayment)
                {
                    relatedStage = "*Make Sales Payment*";
                    App.ShowDebugMsg($@"Received UIMakeSalesPaymentAck");
                    App.MainScreenControl?.MakeTicketPayment(uiSalesPayment);

                    if (uiSalesPayment.Session.BookingExpiredDateTime.HasValue == true)
                        App.BookingTimeoutMan?.UpdateBookingTimeout(uiSalesPayment.Session.BookingExpiredDateTime.Value);
                }

                else if (e?.ReceivedData?.MsgObject is UICompleteTransactionResult uiCompltResult)
                {
                    relatedStage = "*Complete Transaction Result*";
                    App.ShowDebugMsg($@"Received UICompleteTransactionResult");
                    App.MainScreenControl?.UpdateTransactionCompleteStatus(uiCompltResult);
                }

                else if (e?.ReceivedData?.MsgObject is UISalesTimeoutWarningAck uiTimeoutWarn)
                {
                    relatedStage = "*Timeout Warning Acknowledge*";
                    App.ShowDebugMsg($@"Received UISalesTimeoutWarningAck");

                    if (uiTimeoutWarn.ModeOfTimeout == TimeoutMode.UIResponseTimeoutWarning)
                        App.MainScreenControl?.AcquireUserTimeoutResponse(uiTimeoutWarn, requestResultState: false, out bool? isSuccess);
                    else
                    {
                        App.BookingTimeoutMan?.UpdateBookingTimeout(DateTime.Now);
                    }
                }

                else if (e?.ReceivedData?.MsgObject is UISalesBookingTimeoutExtensionResult uiBookingTimeoutExt)
                {
                    relatedStage = "*Booking Timeout Extension*";
                    App.ShowDebugMsg($@"Received UISalesBookingTimeoutExtensionResult");

                    if ((uiBookingTimeoutExt.SessionId.Equals(Guid.Empty) == false)
                        && (App.LatestUserSession != null)
                        && (uiBookingTimeoutExt.SessionId.Equals(App.LatestUserSession.SessionId) == true)
                        && (uiBookingTimeoutExt.MessageData is ExtendBookingSessionResult extRes)
                        )
                    {
                        DateTime expireTime = DateTime.Now.AddSeconds(extRes.Data.BookingRemainingInSecond);
                        App.BookingTimeoutMan.UpdateBookingTimeout(expireTime);
                    }
                }

                //
            }
            catch (Exception ex)
            {
                App.Log?.LogText(_logChannel, "-", e, "EX01", "AppSalesSvcEventsHandler.SalesService_OnDataReceived", NssIT.Kiosk.AppDecorator.Log.MessageType.Error,
                    extraMsg: "Error : " + ex.ToString() + "; \r\n\r\n MsgObj: DataReceivedEventArgs",
                    netProcessId: e?.ReceivedData?.NetProcessId);

                App.MainScreenControl?.Alert(detailMsg: $@"{ex.Message}; (EXIT500199); Related : {relatedStage}");
            }
        }
    }
}
