using kvdt_kiosk.Models;
using System.Threading.Tasks;

namespace kvdt_kiosk.Interfaces
{
    public interface IAPIServices
    {
        Task<AFCStation> GetAFCStations(string AFCServiceHeaders_Id);

        Task<AFCAddOn> GetAFCAddOn(string AFCServiceHeaders_Id);

        Task<AFCSeatInfo> GetAFCSeatInfo(string AFCServiceHeaders_Id, string From_MStations_Id, string To_MStations_Id, string PackageJourney);

        Task<AFCService> GetAFCServices();

        Task<AFCServiceByCounter> GetAFCServiceByCounter(string counterId);

        Task<AFCPackage> GetAFCPackage();

        Task<AFCTicketType> GetAFCTicketType(string purchaseChannel);

        Task<string> GetMyKadInfo();

        Task<UpdateAFCBookingResultModel> RequestAFCBooking(AFCBookingModel aFCBookingModel);

        Task<CheckoutBookingResultModel> RequestAFCCheckoutBooking(AFCCheckOutModel aFCCheckoutBookingModel);

        Task<AFCPaymentResultModel> RequestAFCPayment(AFCPaymentModel aFCPaymentModel);

    }
}
