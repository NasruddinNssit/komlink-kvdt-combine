namespace NssIT.Kiosk.Client.Kvdt.Services
{
    public class APIURLServices
    {
        public const string APIURLLocal = "https://localhost:44305/api/AFC/";

        public const string APIURL = "https://ktmb-dev-api.azurewebsites.net/api/AFC/";
        public string GetAFCServiceByCounter => APIURL + "GetAFCServiceByCounter?";
        public string MyKadURL = "http://localhost:1234/para=2";

    }
}
