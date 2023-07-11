using NssIT.Kiosk.AppDecorator.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService.Sales
{
    [Serializable]
    public class UserSession : IDisposable 
    {
        //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
        /// <summary>
        /// Normally refer to NetProcessID of UICountDownStartRequest
        /// </summary>
        public Guid SessionId { get; set; }
        public bool Expired { get; set; }

        //XXXXXXXXXXXXXXXX
        // Setting
        public int ETS_Intercity_MaxPaxAllowed { get; set; } = 5;
        public int Komuter_MaxPaxAllowed { get; set; } = 5;

        //XXXXXXXXXXXXXXXX

        public LanguageCode Language { get; set; }
        public TravelMode TravelMode { get; set; }
        public TransportGroup VehicleGroup { get; set; }

        public string OriginStationCode { get; set; }
        public string OriginStationName { get; set; }
        public IList<string> OriginAvailableTrainService { get; set; } = null;

        public int NumberOfPassenger { get; set; } = 1;

        public string DestinationStationCode { get; set; }
        public string DestinationStationName { get; set; }
        public IList<string> DestinationAvailableTrainService { get; set; } = null;

        /// <summary>
        /// Like Train with ETS or INTERCITY
        /// </summary>
        public string SelectedVehicleService { get; set; } = null;
        public string SeatBookingId { get; set; } = null;
        public bool IsRequestAmendPassengerInfo { get; set; } = false;
        public ProcessResult PassengerInfoUpdateStatus { get; set; } = ProcessResult.Fail;
        public string PassengerInfoUpdateFailMessage { get; set; }

        public ProcessResult ETSIntercityCheckoutStatus { get; set; } = ProcessResult.Fail;
        public string ETSIntercityCheckoutFailMessage { get; set; }

        public PaymentResult PaymentState { get; set; } = PaymentResult.Unknown;
        public TickSalesMenuItemCode? CurrentEditMenuItemCode { get; set; } = null;

        /// <summary>
        /// State result refer to a response after the payment transaction has sent to web service. 
        /// </summary>
        public ProcessResult PaymentTransactionWebCompletedState { get; private set; } = ProcessResult.Fail;
        public string PaymentTypeDesc { get; set; }

        // XXXXX Depart Trip XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
        public string DepartPendingReleaseTransNo { get; set; }
        public string DepartTripId { get; set; }
        /// <summary>
        /// Customer Departure Date; The Date refer to customer checkin into a bus.
        /// </summary>
        public DateTime? DepartPassengerDepartDateTime { get; private set; }
        public DateTime? DepartPassengerArrivalDate { get; set; } /* KTM */
        public string DepartPassengerDepartTimeStr { get; set; }
        public int DepartPassengerArrivalDayOffset { get; set; } = 0;
        public string DepartPassengerArrivalTimeStr { get; set; } /* KTM */
        /// <summary>
        /// KTM : ETS, INTERCITY or Komuter
        /// </summary>
        public string DepartVehicleService { get; set; }
        /// <summary>
        /// KTM : Train Number
        /// </summary>
        public string DepartVehicleNo { get; set; } /* KTM : Train Number */
        /// <summary>
        /// Gold / Platinum
        /// </summary>
        public string DepartServiceCategory { get; set; }
        public string DepartCurrency { get; set; }
        public decimal DepartPrice { get; set; }
        // XXXXX -- End Depart Trip -- XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
        // XXXXX -- Depart Seat ------ XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
        
        public string DepartTrainSeatModelId { get; set; } = null;
        public decimal DepartTotalAmount { get;  set; }
        public string DepartSeatConfirmCode { get; set; }
        public CustSeatDetail[] DepartPassengerSeatDetailList { get; set; }
        // XXXXX -- End Depart Seat -- XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
        // XXXXX -- Return Trip ------ XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
        /// <summary>
        /// Customer Return Date; The Date refer to customer checkin into a bus.
        /// </summary>
        public string ReturnTripId { get; set; }
        public DateTime? ReturnPassengerDepartDateTime { get; private set; }
        public DateTime? ReturnPassengerArrivalDate { get; set; } /* KTM */
        public string ReturnPassengerDepartTimeStr { get; set; }
        public int ReturnPassengerArrivalDayOffset { get; set; } = 0;
        public string ReturnPassengerArrivalTimeStr { get; set; } /* KTM */
        /// <summary>
        /// KTM : ETS, INTERCITY or Komuter
        /// </summary>
        public string ReturnVehicleService { get; set; }
        /// <summary>
        /// KTM : Train Number
        /// </summary>
        public string ReturnVehicleNo { get; set; } /* KTM : Train Number */
        /// <summary>
        /// Gold / Platinum
        /// </summary>
        public string ReturnServiceCategory { get; set; }
        public string ReturnCurrency { get; set; }
        public decimal ReturnPrice { get; set; }
        // XXXXX -- End Return Trip -- XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
        // XXXXX -- Return Seat ------ XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
        public string ReturnTrainSeatModelId { get; set; } = null;
        public decimal ReturnTotalAmount { get; set; }
        public string ReturnSeatConfirmCode { get; set; }
        public CustSeatDetail[] ReturnPassengerSeatDetailList { get; set; }
        //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        public bool IsAllowedETSIntercityInsurance { get; set; } = false;
        public bool IsETSIntercityInsuranceValid { get; set; } = false;
        public string ETSInsuranceHeadersId { get; set; } = null;
        public bool IsETSInsuranceSuccessSubmission { get; set; } = false;
        //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        // XXXXX -- End Depart Seat -- XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
        //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        //Temporary Solution for capturing cash status
        public PaymentType TypeOfPayment { get; set; } = PaymentType.Unknown;

        // Cash Status
        public int Cassette1NoteCount { get; set; }
        public int Cassette2NoteCount { get; set; }
        public int Cassette3NoteCount { get; set; }
        public int RefundCoinAmount { get; set; }
        //-----------------------------------------------------------

        /// <summary>
        /// This Number is the Ticket Booking number given to customer. And is created after sales checkout.
        /// </summary>
        public string KtmbSalesTransactionNo { get; set; }
        public string TradeCurrency { get; set; }
        public decimal GrossTotal { get; set; }
        public string GrossTotalStr { get; set; }
        public string FinancePaymentMethod { get; set; }

        //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        // Komuter
        // -------------------
        // SeatBookingId
        // SalesTransactionNo
        // OriginStationCode 
        // OriginStationName 
        // DestinationStationCode 
        // DestinationStationName 
        // FinancePaymentMethod
        // TradeCurrency
        // GrossTotal
        // PaymentState
        public string TicketPackageId { get; set; }
        public TicketItem[] TicketItemList { get; set; }
        public decimal BookingAmount { get; set; }
        public string BookingCurrency { get; set; }
        public DateTime? BookingExpiredDateTime { get; set; }

        //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
        public UserSession() { SessionReset(); }

        public void NewSession(Guid sessionId)
        {
            SessionReset();

            SessionId = sessionId;
            Expired = false;
        }

        public void SessionReset()
        {
            SessionId = Guid.Empty;
            Expired = true;
            Language = LanguageCode.English;

            ETS_Intercity_MaxPaxAllowed = 5;
            Komuter_MaxPaxAllowed = 5;

            TravelMode = TravelMode.DepartOnly;
            VehicleGroup = TransportGroup.EtsIntercity;
            CurrentEditMenuItemCode = null;
            NumberOfPassenger = 1;

            OriginStationCode = null;
            OriginStationName = null;
            OriginAvailableTrainService = null;
            DestinationStationCode = null;
            DestinationStationName = null;
            DestinationAvailableTrainService = null;
            SelectedVehicleService = null;
            PaymentState = PaymentResult.Unknown;
            PaymentTransactionWebCompletedState = ProcessResult.Fail;
            PaymentTypeDesc = null;
            IsRequestAmendPassengerInfo = false;
            PassengerInfoUpdateStatus = ProcessResult.Fail;
            PassengerInfoUpdateFailMessage = null;
            ETSIntercityCheckoutStatus = ProcessResult.Fail;
            ETSIntercityCheckoutFailMessage = null;

            TypeOfPayment = PaymentType.Unknown;
            Cassette1NoteCount = -1;
            Cassette2NoteCount = -1;
            Cassette3NoteCount = -1;

            DepartPassengerDepartDateTime = null;
            DepartPassengerArrivalDate = null;
            DepartPassengerArrivalDayOffset = 0;
            DepartTripId = null;
            DepartPrice = 0M;

            DepartPassengerSeatDetailList = null;
            DepartTotalAmount = 0M;
            DepartVehicleService = null;
            DepartVehicleNo = null;
            DepartServiceCategory = null;
            DepartPassengerDepartTimeStr = null;
            DepartPassengerArrivalTimeStr = null;
            DepartTrainSeatModelId = null;
            SeatBookingId = null;

            DepartCurrency = null;
            DepartSeatConfirmCode = null;
            DepartPendingReleaseTransNo = null;

            ReturnPassengerDepartDateTime = null;
            ReturnPassengerArrivalDate = null;
            ReturnPassengerDepartTimeStr = null;
            ReturnPassengerArrivalDayOffset = 0;
            ReturnPassengerArrivalTimeStr = null;

            ReturnVehicleService = null;
            ReturnVehicleNo = null;
            ReturnServiceCategory = null;
            ReturnCurrency = null;
            ReturnPrice = 0.0M;
            ReturnTrainSeatModelId = null;

            ReturnTripId = null;
            ReturnPassengerSeatDetailList = null;
            ReturnTotalAmount = 0M;
            ReturnSeatConfirmCode = null;

            IsAllowedETSIntercityInsurance = false;
            IsETSIntercityInsuranceValid = false;
            ETSInsuranceHeadersId = null;
            IsETSInsuranceSuccessSubmission = false;

            TicketPackageId = null;
            TicketItemList = null;
            BookingCurrency = null;
            BookingAmount = 0.0M;
            BookingExpiredDateTime = null;

            KtmbSalesTransactionNo = null;
            TradeCurrency = null;
            GrossTotal = 0.0M;
            GrossTotalStr = null;
            FinancePaymentMethod = null;
        }

        public void SetTravelDate(TravelMode travelMode, DateTime? passengerDepartDate, DateTime? passengerReturnDate)
        {
            TravelMode = travelMode;
            DepartPassengerDepartDateTime = passengerDepartDate;

            if (TravelMode == TravelMode.DepartOrReturn)
                ReturnPassengerDepartDateTime = passengerReturnDate;
        }

        public void Dispose()
        {
            DepartPassengerSeatDetailList = null;
            ReturnPassengerSeatDetailList = null;
        }
    }
}
