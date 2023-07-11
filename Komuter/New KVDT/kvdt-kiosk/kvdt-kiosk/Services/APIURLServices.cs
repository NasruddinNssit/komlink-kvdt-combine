namespace kvdt_kiosk.Services
{
    public class APIURLServices
    {
        public const string APIURLLocal = "https://localhost:44305/api/AFC/";

        public const string APIURL = "https://ktmb-dev-api.azurewebsites.net/api/AFC/";

        public string GetAFCStations => APIURL + "GetAFCStations?";
        public string GetAFCServices => APIURL + "GetAFCServices";

        public string GetAFCPackage => APIURL + "GetAFCPackage";

        public string GetAFCTicketType => APIURL + "GetAFCTicketType?";

        public string GetAFCAddOn => APIURL + "GetAFCAddOnAndPriceByAFCService?";

        public string GetAFCServiceByCounter => APIURL + "GetAFCServiceByCounter?";

        public string GetAFCTicketTypeAddOn => APIURL + "GetAFCTicketTypeAddOn?";

        public string MyKadURL = "http://localhost:1234/para=2";

        public string RequestAFCBooking => APIURL + "UpdateAFCBooking";

        public string RequestAFCCheckOut => APIURL + "CheckoutAFCBooking";

        public string RequestPaymentBooking => APIURL + "UpdateAFCBookingPayment";
    }
}
