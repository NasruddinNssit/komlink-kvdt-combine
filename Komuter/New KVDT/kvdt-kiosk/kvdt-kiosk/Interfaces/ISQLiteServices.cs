using System.Collections.Generic;
using System.Threading.Tasks;

namespace kvdt_kiosk.Interfaces
{
    public interface ISQLiteServices
    {
        Task<List<T>> Read<T>() where T : new();
        Task<int> Insert<T>(T model);
        Task<int> Update<T>(T model);
        Task<int> Delete<T>(T model);
        Task<int> DeleteAll<T>() where T : new();
        Task<List<T>> GetAll<T>()
            where T : new();
        Task<T> Get<T>(int id)
            where T : new();
        Task<T> Get<T>(string id)
            where T : new();
    }
}
