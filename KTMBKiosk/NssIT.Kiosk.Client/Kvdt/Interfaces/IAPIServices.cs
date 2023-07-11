using NssIT.Kiosk.Client.Kvdt.Models;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.Kvdt.Interfaces
{
    public interface IAPIServices
    {
        Task<AFCServiceByCounter> GetFCServiceByCounter(string counterId);
    }
}
