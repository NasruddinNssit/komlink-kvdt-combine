using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI;
using NssIT.Kiosk.AppDecorator.UI;
using NssIT.Kiosk.Client.ViewPage.Alert;
using NssIT.Kiosk.Client.ViewPage.Info;
using NssIT.Kiosk.Client.ViewPage.Menu;
using NssIT.Kiosk.Client.ViewPage.Navigator;
using NssIT.Kiosk.Device.PAX.IM30.IM30PayApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace NssIT.Kiosk.Client
{
	public interface IMainScreenControl
	{
		/// <summary>
		/// Execution Menu
		/// </summary>
		INav MiniNavigator { get; set; }

		//IInfo UserInfo { get; set; }

		Dispatcher MainFormDispatcher { get;  }

		void InitiateMaintenance(PayWaveSettlementScheduler cardSettScheduler);
		void AcknowledgeOutstandingCardSettlement(UISalesCheckOutstandingCardSettlementAck outstandingCardSettlement);
		void AcknowledgeCardSettlementDBTransStatus(UISalesCardSettlementStatusAck cardSettSttAck);

		void ShowMaintenance();
		void ShowWelcome();
		void ChooseLanguage(UILanguageSelectionAck lang);
		void ChooseOriginStation(UIOriginListAck uiOrig, UserSession session);
		void ChooseDestinationStation(UIDestinationListAck uiDest, UserSession session);
		void ChooseTravelDates(UITravelDatesEnteringAck uiTravelDate, UserSession session);

		void ShowInitDepartTrip(UIDepartTripInitAck tripInit, UserSession session);
		void UpdateDepartTripList(UIDepartTripListAck uiTravelDate, UserSession session);
		void UpdateDepartTripSubmitError(UIDepartTripSubmissionErrorAck uiTripSubErr, UserSession session);
		void ChooseDepartSeat(UIDepartSeatListAck uiDepartSeatList, UserSession session);

		void ShowInitReturnTrip(UIReturnTripInitAck tripInit, UserSession session);
		void UpdateReturnTripList(UIReturnTripListAck uiReturnTripList, UserSession session);
		void ChooseReturnSeat(UIReturnSeatListAck uiReturnSeatList, UserSession session);

		void ChoosePickupNDrop(UIDepartPickupNDropAck uiIDepartPickupNDrop);
		void EnterPassengerInfo(UICustInfoAck uiCustInfo);
		void ChooseInsurance(UIETSInsuranceListAck uiEstInsrLst);

		/// <summary>
		/// Refer to KTM Komuter method
		/// </summary>
		/// <param name="uiStartSales"></param>
		void StartSelling(UISalesStartSellingAck uiStartSales);

		void MakeTicketPayment(UISalesPaymentProceedAck uiSalesPayment);
		void UpdateTransactionCompleteStatus(UICompleteTransactionResult uiCompltResult);
		void UpdateDepartDate(DateTime newDepartDate);
		void UpdateReturnDate(DateTime newReturnDate);
		void UpdatePromoCodeVerificationResult(UICustPromoCodeVerifyAck codeVerificationAnswer);
		void UpdatePNRTicketTicketResult(UICustInfoPNRTicketTypeAck custPNRTicketTypeResult);
		void RequestAmentPassengerInfo(UICustInfoUpdateFailAck uiFailUpdateCustInfo);
		void BTnGShowPaymentInfo(IKioskMsg kioskMsg);

		void Alert(string malayShortMsg = "TIDAK BERFUNGSI", string engShortMsg = "OUT OF ORDER", string detailMsg = "");

		//void StartSelling(LanguageCode langCode);
		void ToTopMostScreenLayer();
		void ToNormalScreenLayer();

		void UpdateKomuterTicketTypePackage(UIKomuterTicketTypePackageAck uiTickPack);
		void UpdateKomuterTicketBooking(UIKomuterTicketBookingAck uiTickBook);
		void UpdateKomuterBookingCheckoutResult(UIKomuterBookingCheckoutAck bookingCheckoutAck);
		void UpdateKomuterTicketPaymentStatus(UIKomuterCompletePaymentAck paymentStatusAck);

		void AcquireUserTimeoutResponse(UISalesTimeoutWarningAck uiTimeoutWarn, bool requestResultState, out bool? isSuccess);
		void UnLoadTimeoutNoticePage();
	}
}
