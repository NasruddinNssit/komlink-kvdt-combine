using kvdt_kiosk.Interfaces;
using kvdt_kiosk.Models;
using kvdt_kiosk.Views.Error;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace kvdt_kiosk.Services
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

        public async Task<AFCAddOn> GetAFCAddOn(string AFCServiceHeaders_Id)
        {
            try
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Post, _apiURLServices.GetAFCAddOn + "AFCServiceHeaders_Id=" + AFCServiceHeaders_Id);
                request.Headers.Add("RequestSignature", _signatureServices.GetSignature());

                var response = client.SendAsync(request).Result;

                if (response.IsSuccessStatusCode)
                {
                    var result = response.Content.ReadAsStringAsync().Result;
                    if (string.IsNullOrEmpty(result))
                    {
                        throw new Exception("Failed to deserialize response.");
                    }
                    AFCAddOn deserializedResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<AFCAddOn>(result);
                    if (deserializedResponse != null)
                    {
                        return deserializedResponse;
                    }
                }
                throw new Exception("Failed to get API response.");
            }
            catch (Exception ex)
            {
                ErrorWindow errorWindow = new ErrorWindow(ex.Message);
                errorWindow.Show();
                return null;
            }
        }

        public async Task<AFCPackage> GetAFCPackage()
        {
            try
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Post, _apiURLServices.GetAFCPackage);
                request.Headers.Add("RequestSignature", _signatureServices.GetSignature());

                var response = client.SendAsync(request).Result;

                if (response.IsSuccessStatusCode)
                {
                    var result = response.Content.ReadAsStringAsync().Result;
                    if (string.IsNullOrEmpty(result))
                    {
                        throw new Exception("Failed to deserialize response.");
                    }
                    AFCPackage deserializedResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<AFCPackage>(result);
                    if (deserializedResponse != null)
                    {
                        return deserializedResponse;
                    }
                }

                throw new Exception("Failed to get API response.");
            }
            catch (Exception ex)
            {
                ErrorWindow errorWindow = new ErrorWindow(ex.Message);
                errorWindow.Show();
                return null;
            }
        }

        public async Task<AFCService> GetAFCServices()
        {
            try
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Post, _apiURLServices.GetAFCServices);
                request.Headers.Add("RequestSignature", _signatureServices.GetSignature());

                var response = client.SendAsync(request).Result;

                if (response.IsSuccessStatusCode)
                {
                    var result = response.Content.ReadAsStringAsync().Result;
                    if (string.IsNullOrEmpty(result))
                    {
                        throw new Exception("Failed to deserialize response.");
                    }
                    AFCService deserializedResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<AFCService>(result);
                    if (deserializedResponse != null)
                    {
                        return deserializedResponse;
                    }
                }
                throw new Exception("Failed to get API response.");
            }
            catch (Exception ex)
            {
                ErrorWindow errorWindow = new ErrorWindow(ex.Message);
                errorWindow.Show();
                return null;
            }
        }

        public async Task<AFCStation> GetAFCStations(string AFCServiceHeaders_Id)
        {
            try
            {
                var client = new HttpClient();

                var request = new HttpRequestMessage(HttpMethod.Post, _apiURLServices.GetAFCStations + "AFCServiceHeaders_Id=" + AFCServiceHeaders_Id);

                request.Headers.Add("RequestSignature", _signatureServices.GetSignature());

                var response = client.SendAsync(request).Result;


                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();

                    if (string.IsNullOrEmpty(result))
                    {
                        throw new Exception("Failed to deserialize response.");
                    }


                    AFCStation deserializedResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<AFCStation>(result);

                    if (deserializedResponse != null)
                    {
                        return deserializedResponse;

                    }
                }

                throw new Exception("Failed to get API response. Status: " + response.StatusCode);
            }
            catch (Exception ex)
            {
                ErrorWindow errorWindow = new ErrorWindow(ex.Message);
                errorWindow.Show();
                return null;
            }

        }

        public async Task<AFCTicketType> GetAFCTicketType(string purchaseChannel)
        {
            try
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Post, _apiURLServices.GetAFCTicketType + "purchaseChannel=" + purchaseChannel);

                request.Headers.Add("RequestSignature", _signatureServices.GetSignature());

                var response = client.SendAsync(request).Result;

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();

                    if (string.IsNullOrEmpty(result))
                    {
                        throw new Exception("Failed to deserialize response.");
                    }

                    AFCTicketType deserializedResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<AFCTicketType>(result);

                    if (deserializedResponse != null)
                    {
                        return deserializedResponse;
                    }
                }
                throw new Exception("Failed to get API response. Status: " + response.StatusCode);
            }
            catch (Exception ex)
            {
                ErrorWindow errorWindow = new ErrorWindow(ex.Message);
                errorWindow.Show();
                return null;
            }
        }

        public async Task<string> GetMyKadInfo()
        {
            try
            {
                var responseResult = "";
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Get, _apiURLServices.MyKadURL);

                var response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(result) || result.Contains("Error"))
                    {
                        responseResult = "Error";
                    }


                    string[] values = result.Split(new[] { ',' });

                    string icNumber = values[2];
                    string passenggerName = values[3];

                    // responseResult = passenggerName.Replace("     ", "");

                    responseResult = icNumber + "-" + passenggerName.Replace("     ", "");

                    return responseResult;
                }

                throw new Exception("Failed to get API response.");
            }
            catch (Exception ex)
            {
                ErrorWindow errorWindow = new ErrorWindow(ex.Message);
                errorWindow.Show();
                return null;
            }
        }

    }
}
