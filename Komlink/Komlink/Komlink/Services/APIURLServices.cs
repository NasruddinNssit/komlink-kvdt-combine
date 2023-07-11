using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Komlink.Services
{
    public class APIURLServices
    {
        public const string APIURLLocal = "https://localhost:44305/api/komlink/";

        public const string APIURL = "https://ktmb-dev-api.azurewebsites.net/api/komlink/";

        public const string APIURLAFC = "https://ktmb-dev-api.azurewebsites.net/api/AFC/";


        public string GetKomlinkCard => APIURL + "GetCard";

        public string SearchKomlinkCard = APIURL + "SearchKomlinkCard";

        public string GetKomlinkTransaction => APIURL + "GetTransaction";

        public string StartIM30Reader = "http://127.0.0.1:1234/Para=15&IMPR=1,2&FTVM=1";
        public string GetDataFromIM30Reader = "http://127.0.0.1:1234/Para=17";

        public string GetKomlinkCardTransaction = APIURL + "GetKomlinkCardTransaction";

        public string AddTopUp = APIURL + "AddTopup";

        public string CancelTopUp = APIURL + "CancelTopUp";
        public string CheckOutTopUp = APIURL + "CheckoutTopup";

        public string UpdateWriteStatus = APIURL + "UpdateTopup";

        public string MakePaymentUsingIM30 = APIURLLocal + "MakePaymentWithIM30";

        public string GetAFCServiceByCounter => APIURLAFC + "GetAFCServiceByCounter?";
    }
}
