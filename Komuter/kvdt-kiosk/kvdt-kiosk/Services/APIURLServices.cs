namespace kvdt_kiosk.Services
{
    public class APIURLServices
    {
        //public const string APIURL = "https://localhost:44305/api/AFC/";

        public const string APIURL = "https://ktmb-dev-api.azurewebsites.net/api/AFC/";

        public string GetAFCStations => APIURL + "GetAFCStations?";
        public string GetAFCServices => APIURL + "GetAFCServices";

        public string GetAFCPackage => APIURL + "GetAFCPackage";

        public string GetAFCTicketType => APIURL + "GetAFCTicketType?";

        public string GetAFCAddOn => APIURL + "GetAFCAddOnAndPriceByAFCService?";

        public string MyKadURL = "http://localhost:1234/para=2";
    }
}
