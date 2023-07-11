using Newtonsoft.Json;
using NssIT.Kiosk.Client.Kvdt.Interfaces;
using NssIT.Kiosk.Client.Kvdt.Models;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

namespace NssIT.Kiosk.Client.Kvdt.Services
{
    public class APIServices : IAPIServices
    {
        private readonly APIURLServices _apiURLServices;
        private readonly APISignatureServices _signatureServices;

        public APIServices(APIURLServices apiURLServices, APISignatureServices signatureServices)
        {
            _apiURLServices = apiURLServices;
            _signatureServices = signatureServices;
        }

        public async Task<AFCServiceByCounter> GetFCServiceByCounter(string counterId)
        {
            try
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Post, _apiURLServices.GetAFCServiceByCounter + "counterId=" + counterId);

                request.Headers.Add("RequestSignature", _signatureServices.GetSignature());

                var response = client.SendAsync(request).Result;

                if (!response.IsSuccessStatusCode)
                    throw new Exception("Failed to get API response. Status: " + response.StatusCode);

                var result = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrEmpty(result))
                {
                    throw new Exception("Failed to deserialize response.");
                }

                if (result.Contains("No data can be fetched from"))
                {
                    return null;
                }

                var deserializedResponse = JsonConvert.DeserializeObject<AFCServiceByCounter>(result);
                return deserializedResponse;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }
    }
}
