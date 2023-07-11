using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Instruction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService.Command
{
	public enum AccessDBCommandCode
	{
		UnKnown = UISalesInst.Unknown,

		SalesBookingTimeoutExtensionRequest = UISalesInst.SalesBookingTimeoutExtensionRequest,

		OriginListRequest = UISalesInst.OriginListRequest,
		DestinationListRequest = UISalesInst.DestinationListRequest,
		WebServerLogonRequest = UISalesInst.WebServerLogonRequest,
		DepartTripListRequest = UISalesInst.DepartTripListRequest,
		DepartSeatListRequest = UISalesInst.DepartSeatListRequest,
		DepartSeatConfirmRequest = UISalesInst.DepartSeatConfirmRequest,

		ReturnTripListRequest = UISalesInst.ReturnTripListRequest,
		ReturnSeatListRequest = UISalesInst.ReturnSeatListRequest,
		ReturnSeatConfirmRequest = UISalesInst.ReturnSeatConfirmRequest,

		CustInfoPrerequisiteRequest = UISalesInst.CustInfoPrerequisiteRequest,
		DepartCustInfoUpdateELSEReleaseSeatRequest = UISalesInst.CustInfoUpdateELSEReleaseSeatRequest,
		ETSCheckoutSaleRequest = UISalesInst.ETSCheckoutSaleRequest,
		CompleteTransactionElseReleaseSeatRequest = UISalesInst.CompleteTransactionElseReleaseSeatRequest,
		CustPromoCodeVerifyRequest = UISalesInst.CustPromoCodeVerifyRequest,
		CustInfoPNRTicketTypeRequest = UISalesInst.CustInfoPNRTicketTypeRequest,

		ETSInsuranceListRequest = UISalesInst.ETSInsuranceListRequest,
		ETSInsuranceSubmissionRequest = UISalesInst.ETSInsuranceSubmissionRequest,

		ETSIntercityTicketRequest  = UISalesInst.ETSIntercityTicketRequest,

		TicketReleaseRequest = UISalesInst.SeatReleaseRequest, 

		KomuterTicketTypePackageRequest = UISalesInst.KomuterTicketTypePackageRequest,
		KomuterTicketBookingRequest = UISalesInst.KomuterTicketBookingRequest,
		KomuterBookingCheckoutRequest = UISalesInst.KomuterBookingCheckoutRequest,
		KomuterCompletePaymentRequest = UISalesInst.KomuterCompletePaymentSubmission,
		KomuterBookingReleaseRequest = UISalesInst.KomuterBookingReleaseRequest,

		CounterConfigurationRequest = UISalesInst.CounterConfigurationRequest,

		CheckOutstandingCardSettlementRequest = UISalesInst.CheckOutstandingCardSettlementRequest,
		CardSettlementSubmission = UISalesInst.CardSettlementSubmission
	}
}
