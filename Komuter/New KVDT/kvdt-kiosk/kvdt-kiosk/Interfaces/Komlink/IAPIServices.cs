using kvdt_kiosk.Models.Komlink;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kvdt_kiosk.Interfaces.Komlink
{
    public interface IAPIServices
    {
        Task<KomlinkCard> GetKomlinkCardDetail();
        Task<KomlinkTransaction> GetKomlinkTransactionDetail();

        Task<KomlinkCard> SearchKomlinkCard(JObject komlinkCard);

        Task<List<KomlinkTransactionDetail>> GetKomlinkCardTransaction(KomlinkCardTransactionRequesModel reques);

        Task<KomlinkCardAddTopupResultModel> AddTopUp(KomlinkCardAddTopUpRequestModel requestModel);

        Task CancelTopUp(KomlinkCardCancelTopupRequestModel requestModel);

        Task UpdateWriteStatus(KomlinkCardCancelTopupRequestModel requestModel);

        Task<ResultModel> CompleteTopUp(KomlinkCardCheckoutTopupRequestModel requestModel);
    }
}
