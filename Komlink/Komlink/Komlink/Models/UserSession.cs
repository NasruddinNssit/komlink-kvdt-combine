using NssIT.Train.Kiosk.Common.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Komlink.Models
{
    public static class UserSession
    {
        public static string SessionId { get; set; }

        public static string FromStationId { get; set; }

        public static string FromStationName { get; set; }

        public static string CurrencyCode { get; set; } = "MYR";

        public static decimal TotalTicketPrice { get; set; } = 0;

        public static string PaymentGateWaySalesTransactionNo { get; set; }

        public static string PurchaseStationId { get; set; }

        public static string MCurrenciesId = "MYR";

        public static string MCounters_Id { get; set; }

        public static string KioskId { get;set; }

        public static Action<KomlinkCardDetailModel> CreateSession;
        
        public static KomlinkCardDetailModel CurrentUserSession { get; set; }

        public static KomlinkCardAddTopupResultModel requestAddTopUpModel { get; set; }

        public static string PaymentMethod { get; set; }

        public static void CreateSessionAction(KomlinkCardDetailModel model)
        {
            CurrentUserSession = model;
            CreateSession?.Invoke(model);
        }

        public static void ClearSession()
        {
            CurrentUserSession = null;
            TotalTicketPrice = 0;
            requestAddTopUpModel = null;
            
        }

    
    }
}
