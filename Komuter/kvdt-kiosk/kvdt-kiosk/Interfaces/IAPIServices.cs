using kvdt_kiosk.Models;
using System.Threading.Tasks;

namespace kvdt_kiosk.Interfaces
{
    public interface IAPIServices
    {
        Task<AFCStation> GetAFCStations(string AFCServiceHeaders_Id);

        Task<AFCAddOn> GetAFCAddOn(string AFCServiceHeaders_Id);

        Task<AFCService> GetAFCServices();

        Task<AFCPackage> GetAFCPackage();

        Task<AFCTicketType> GetAFCTicketType(string purchaseChannel);

        Task<string> GetMyKadInfo();
    }
}
