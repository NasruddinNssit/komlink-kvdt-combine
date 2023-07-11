using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService.Instruction
{
	public enum UISalesInst
	{
		Unknown = CommInstruction.Blank,

		RestartMachine = CommInstruction.RestartMachine,

		CountDownStartRequest = CommInstruction.UISalesCountDownStartRequest,
		CountDownExpiredAck = CommInstruction.UISalesCountDownExpiredAck,

		LanguageSelectionAck = CommInstruction.UISalesLanguageSelectionAck,
		LanguageSubmission = CommInstruction.UISalesLanguageSubmission,

		ClientMaintenanceRequest = CommInstruction.UISalesClientMaintenanceRequest,
		ClientMaintenanceAck = CommInstruction.UISalesClientMaintenanceAck,
		ClientMaintenanceFinishedSubmission = CommInstruction.UISalesClientMaintenanceFinishedSubmission,

		StartNewSalesRequest = CommInstruction.UISalesStartNewSalesRequest,

		ServerApplicationStatusRequest = CommInstruction.UISalesServerApplicationStatusRequest,
		ServerApplicationStatusAck = CommInstruction.UISalesServerApplicationStatusAck,

		OriginListRequest = CommInstruction.UISalesOriginListRequest,
		OriginListAck = CommInstruction.UISalesOriginListAck,
		OriginSubmission = CommInstruction.UISalesOriginSubmission,

		DestinationListRequest = CommInstruction.UISalesDestinationListRequest,
		DestinationListAck = CommInstruction.UISalesDestinationListAck,
		DestinationSubmission = CommInstruction.UISalesDestinationSubmission,

		TravelDatesEnteringAck = CommInstruction.UISalesTravelDatesEnteringAck,
		TravelDatesSubmission = CommInstruction.UISalesTravelDatesSubmission,

		WebServerLogonRequest = CommInstruction.UISalesWebServerLogonRequest,
		WebServerLogonStatusAck = CommInstruction.UISalesWebServerLogonStatusAck,

		DetailEditRequest = CommInstruction.UISalesDetailEditRequest,

		DepartTripListRequest = CommInstruction.UISalesDepartTripListRequest,
		DepartTripListAck = CommInstruction.UISalesDepartTripListAck,
		DepartTripSubmission = CommInstruction.UISalesDepartTripSubmission,
		DepartTripListInitAck = CommInstruction.UISalesDepartTripListInitAck,
		DepartTripSubmissionErrorAck = CommInstruction.UISalesDepartTripSubmissionErrorAck,

		DepartSeatListRequest = CommInstruction.UISalesDepartSeatListRequest,
		DepartSeatListAck = CommInstruction.UISalesDepartSeatListAck,
		DepartSeatSubmission = CommInstruction.UISalesDepartSeatSubmission,
		DepartSeatConfirmRequest = CommInstruction.UISalesDepartSeatConfirmRequest,
		DepartSeatConfirmResult = CommInstruction.UISalesDepartSeatConfirmResult,
		DepartSeatConfirmFailAck = CommInstruction.UISalesDepartSeatConfirmFailAck,
		DepartSeatListErrorResult = CommInstruction.UISalesDepartSeatListErrorResult,

		DepartPickupNDropAck = CommInstruction.UISalesDepartPickupNDropAck,
		DepartPickupNDropSubmission = CommInstruction.UISalesDepartPickupNDropSubmission,

		ReturnTripListRequest = CommInstruction.UISalesReturnTripListRequest,
		ReturnTripListAck = CommInstruction.UISalesReturnTripListAck,
		ReturnTripSubmission = CommInstruction.UISalesReturnTripSubmission,
		ReturnTripListInitAck = CommInstruction.UISalesReturnTripListInitAck,
		ReturnTripSubmissionErrorAck = CommInstruction.UISalesReturnTripSubmissionErrorAck,

		ReturnSeatListRequest = CommInstruction.UISalesReturnSeatListRequest,
		ReturnSeatListAck = CommInstruction.UISalesReturnSeatListAck,
		ReturnSeatSubmission = CommInstruction.UISalesReturnSeatSubmission,
		ReturnSeatConfirmRequest = CommInstruction.UISalesReturnSeatConfirmRequest,
		ReturnSeatConfirmResult = CommInstruction.UISalesReturnSeatConfirmResult,
		ReturnSeatConfirmFailAck = CommInstruction.UISalesReturnSeatConfirmFailAck,
		ReturnSeatListErrorResult = CommInstruction.UISalesReturnSeatListErrorResult,

		CustInfoAck = CommInstruction.UISalesCustInfoAck,
		CustInfoSubmission = CommInstruction.UISalesCustInfoSubmission,
		CustInfoUpdateResult = CommInstruction.UISalesCustInfoUpdateResult,
		CustInfoUpdateFailAck = CommInstruction.UISalesCustInfoUpdateFailAck,
		CustPromoCodeVerifyRequest = CommInstruction.UISalesCustPromoCodeVerifyRequest,
		CustPromoCodeVerifyAck = CommInstruction.UISalesCustPromoCodeVerifyAck,

		/// /// <summary>
		/// Update Customer Info. Release Seat when failed to update
		/// </summary>
		CustInfoUpdateELSEReleaseSeatRequest = CommInstruction.UISalesCustInfoUpdateELSEReleaseSeatRequest,
		CustInfoPrerequisiteRequest = CommInstruction.UISalesCustInfoPrerequisiteRequest,
		CustInfoPrerequisiteAck = CommInstruction.UISalesCustInfoPrerequisiteAck,
		CustInfoPNRTicketTypeRequest = CommInstruction.UISalesCustInfoPNRTicketTypeRequest,
		CustInfoPNRTicketTypeAck = CommInstruction.UISalesCustInfoPNRTicketTypeAck,

		ETSInsuranceListRequest = CommInstruction.UISalesETSInsuranceListRequest,
		ETSInsuranceListAck = CommInstruction.UISalesETSInsuranceListAck,
		ETSInsuranceSubmissionRequest = CommInstruction.UISalesETSInsuranceSubmissionRequest,
		ETSInsuranceSubmissionResult = CommInstruction.UISalesETSInsuranceSubmissionResult,

		ETSCheckoutSaleRequest = CommInstruction.UISalesETSCheckoutSaleRequest,
		ETSCheckoutSaleResult = CommInstruction.UISalesETSCheckoutSaleResult,
		ETSCheckoutSaleFailAck = CommInstruction.UISalesETSCheckoutSaleFailAck,

		SalesPaymentProceedAck = CommInstruction.UISalesPaymentProceedAck,
		SalesPaymentSubmission = CommInstruction.UISalesPaymentSubmission,

		SalesTimeoutWarningAck = CommInstruction.UISalesTimeoutWarningAck,
		SalesEndSessionRequest = CommInstruction.UISalesEndSessionRequest,
		SalesBookingTimeoutExtensionRequest = CommInstruction.UISalesBookingTimeoutExtensionRequest,
		SalesBookingTimeoutExtensionResult = CommInstruction.UISalesBookingTimeoutExtensionResult,

		SeatReleaseRequest = CommInstruction.UISalesSeatReleaseRequest,
		SeatReleaseResult = CommInstruction.UISalesSeatReleaseResult,

		CompleteTransactionElseReleaseSeatRequest = CommInstruction.UISalesCompleteTransactionElseReleaseSeatRequest,
		CompleteTransactionResult = CommInstruction.UISalesCompleteTransactionResult,

		RedirectDataToClient = CommInstruction.UISalesRedirectDataToClient,

		CountDownPausedRequest = CommInstruction.UISalesCountDownPausedRequest,
		//CountDownUnPaused = CommInstruction.UISalesCountDownUnPaused
		PageNavigateRequest = CommInstruction.UISalesPageNavigateRequest,
		TimeoutChangeRequest = CommInstruction.UISalesTimeoutChangeRequest,

		StartSellingAck = CommInstruction.UISalesStartSellingAck,

		KomuterTicketTypePackageRequest = CommInstruction.UIKomuterTicketTypePackageRequest,
		KomuterTicketTypePackageAck = CommInstruction.UIKomuterTicketTypePackageAck,

		KomuterTicketBookingRequest = CommInstruction.UIKomuterTicketBookingRequest,
		KomuterTicketBookingAck = CommInstruction.UIKomuterTicketBookingAck,
		KomuterBookingCheckoutRequest = CommInstruction.UIKomuterBookingCheckoutRequest,
		KomuterBookingCheckoutAck = CommInstruction.UIKomuterBookingCheckoutAck,
		KomuterCompletePaymentSubmission = CommInstruction.UIKomuterCompletePaymentSubmission,
		KomuterCompletePaymentAck = CommInstruction.UIKomuterCompletePaymentAck,
		KomuterBookingReleaseRequest = CommInstruction.UIKomuterBookingReleaseRequest,
		KomuterBookingReleaseResult = CommInstruction.UIKomuterBookingReleaseResult,
		KomuterResetUserSessionRequest = CommInstruction.UIKomuterResetUserSessionRequest,

		ETSIntercityTicketRequest = CommInstruction.UIETSIntercityTicketRequest,
		ETSIntercityTicketAck = CommInstruction.UIETSIntercityTicketAck,

		CounterConfigurationRequest = CommInstruction.UISalesCounterConfigurationRequest,
		CounterConfigurationResult = CommInstruction.UISalesCounterConfigurationResult,

		CheckOutstandingCardSettlementRequest = CommInstruction.UISalesCheckOutstandingCardSettlementRequest,
		CheckOutstandingCardSettlementAck = CommInstruction.UISalesCheckOutstandingCardSettlementAck,
		CardSettlementSubmission = CommInstruction.UISalesCardSettlementSubmission,
		CardSettlementStatusAck = CommInstruction.UISalesCardSettlementStatusAck
	}
}