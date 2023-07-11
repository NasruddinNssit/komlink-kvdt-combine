using kvdt_kiosk.Interfaces;
using kvdt_kiosk.Models;
using kvdt_kiosk.Views.Error;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace kvdt_kiosk.Services
{
    public class APIServices : IAPIServices
    {
        private readonly APIURLServices _apiURLServices;
        private readonly APISignatureServices _signatureServices;
        private readonly ErrorScreen _errorScreen = new ErrorScreen();
        private Window _parentWindow = Application.Current.MainWindow;

        public APIServices(APIURLServices apiURLServices, APISignatureServices signatureServices)
        {
            _apiURLServices = apiURLServices;
            _signatureServices = signatureServices;
        }


        public async Task<AFCPaymentResultModel> RequestAFCPayment(AFCPaymentModel model)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var request = new HttpRequestMessage(HttpMethod.Post, _apiURLServices.RequestPaymentBooking);
                    request.Headers.Add("RequestSignature", _signatureServices.GetSignature());

                    var jsonModel = JsonConvert.SerializeObject(model);

                    request.Content = new StringContent(jsonModel, Encoding.UTF8, "application/json");
                    var response = await client.SendAsync(request);
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();

                        if (string.IsNullOrEmpty(result))
                        {
                            _errorScreen.lblErrorMessage.Text = "Failed to deserialize response.";

                            _parentWindow.Content = _errorScreen;
                        }

                        var AfcBookingPaymentResult = JsonConvert.DeserializeObject<AFCPaymentResultModel>(result);

                        if (AfcBookingPaymentResult != null)
                        {
                            return AfcBookingPaymentResult;
                        }
                    }

                    throw new Exception("Failed to get API response.");
                }
            }
            catch (Exception ex)
            {
                _errorScreen.lblErrorMessage.Text = ex.Message;
                _parentWindow.Content = _errorScreen;
                throw;
            }
        }
        public async Task<CheckoutBookingResultModel> RequestAFCCheckoutBooking(AFCCheckOutModel model)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var request = new HttpRequestMessage(HttpMethod.Post, _apiURLServices.RequestAFCCheckOut);
                    request.Headers.Add("RequestSignature", _signatureServices.GetSignature());

                    var jsonModel = JsonConvert.SerializeObject(model);

                    request.Content = new StringContent(jsonModel, Encoding.UTF8, "application/json");
                    var response = await client.SendAsync(request);
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();

                        if (string.IsNullOrEmpty(result))
                        {
                            throw new Exception("Failed to deserialize response.");
                        }

                        var checkoutBookingResultModel = JsonConvert.DeserializeObject<CheckoutBookingResultModel>(result);

                        if (checkoutBookingResultModel != null)
                        {
                            return checkoutBookingResultModel;
                        }
                    }

                    throw new Exception("Failed to get API response.");
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<UpdateAFCBookingResultModel> RequestAFCBooking(AFCBookingModel model)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var request = new HttpRequestMessage(HttpMethod.Post, _apiURLServices.RequestAFCBooking);

                    request.Headers.Add("RequestSignature", _signatureServices.GetSignature());

                    var jsonModel = JsonConvert.SerializeObject(model);

                    request.Content = new StringContent(jsonModel, Encoding.UTF8, "application/json");

                    var response = await client.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();

                        if (string.IsNullOrEmpty(result))
                        {
                            throw new Exception("Failed to deserialize response.");
                        }

                        var updateAFCBookingResultModel = JsonConvert.DeserializeObject<UpdateAFCBookingResultModel>(result);

                        if (updateAFCBookingResultModel != null)
                        {
                            return updateAFCBookingResultModel;
                        }
                    }

                    throw new Exception("Failed to get API response.");
                }
            }
            catch (Exception ex)
            {
                // Handle the exception or log the error message
                // Example: Console.WriteLine("An error occurred: " + ex.Message);
                throw; // Re-throw the exception to propagate it to the caller
            }
        }

        public Task<AFCAddOn> GetAFCAddOn(string AFCServiceHeaders_Id)
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
                        return Task.FromResult(deserializedResponse);
                    }
                }
                throw new Exception("Failed to get API response.");
            }
            catch (Exception ex)
            {
                //show error message box with exception message

                return Task.FromResult<AFCAddOn>(null);
            }
        }

        public Task<AFCPackage> GetAFCPackage()
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
                        return Task.FromResult(deserializedResponse);
                    }
                }

                throw new Exception("Failed to get API response.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return Task.FromResult<AFCPackage>(null);
            }
        }

        public Task<AFCSeatInfo> GetAFCSeatInfo(string AFCServiceHeaders_Id, string From_MStations_Id, string To_MStations_Id, string PackageJourney)
        {
            try
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Post, _apiURLServices.GetAFCTicketTypeAddOn + "AFCServiceHeaders_Id=" + AFCServiceHeaders_Id + "&From_MStations_Id=" + From_MStations_Id + "&To_MStations_Id=" + To_MStations_Id + "&PackageJourney=" + PackageJourney);
                request.Headers.Add("RequestSignature", _signatureServices.GetSignature());

                var response = client.SendAsync(request).Result;

                if (!response.IsSuccessStatusCode) throw new Exception("Failed to get API response.");
                var result = response.Content.ReadAsStringAsync().Result;
                if (string.IsNullOrEmpty(result))
                {
                    throw new Exception("Failed to deserialize response.");
                }
                var deserializedResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<AFCSeatInfo>(result);
                if (deserializedResponse != null)
                {
                    return Task.FromResult(deserializedResponse);
                }
                throw new Exception("Failed to get API response.");

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return Task.FromResult<AFCSeatInfo>(null);
            }
        }

        public Task<AFCService> GetAFCServices()
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
                        return Task.FromResult(deserializedResponse);
                    }
                }
                throw new Exception("Failed to get API response.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return Task.FromResult<AFCService>(null);
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
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

                if (!response.IsSuccessStatusCode)
                    throw new Exception("Failed to get API response. Status: " + response.StatusCode);
                var result = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrEmpty(result))
                {
                    throw new Exception("Failed to deserialize response.");
                }

                var deserializedResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<AFCTicketType>(result);

                if (deserializedResponse != null)
                {
                    return deserializedResponse;
                }
                throw new Exception("Failed to get API response. Status: " + response.StatusCode);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public async Task<string> GetMyKadInfo()
        {
            try
            {
                var client = new HttpClient();
                var response = await client.GetAsync(_apiURLServices.MyKadURL);

                if (!response.IsSuccessStatusCode)
                    throw new Exception("Failed to get API response. Status: " + response.StatusCode);

                var result = await response.Content.ReadAsStringAsync();

                //get first digit of result
                var firstDigit = result.Substring(0, 1);

                if (string.IsNullOrEmpty(result))
                {
                    result = "Error: MyKad Reader Or Device Is NOT Detected.";
                }

                else if (result.Contains("99") && result.Contains("Error") &&
                    result.Contains("The specified reader name is not recognized"))
                {
                    result = "Error: MyKad Reader Or Device Is NOT Detected.";
                }

                else if (result.Contains("99") && result.Contains("Error") && result.Contains("The requested protocols are incompatible with the protocol currently in use with the smart card"))
                {
                    result = "Error: MyKad Reader Is Not Detected";
                }

                else if (result.Contains("99") && result.Contains("Error") &&
                         result.Contains("Operation is not valid due to the current state of the object"))
                {
                    result = "Error: Please insert MyKad into the reader.";
                }

                else if (result.Contains("99") && result.Contains("Error") &&
                        result.Contains("Fail to read JPN data"))
                {
                    result = "Error: Data Cannot Be Read From This MyKad.";
                }

                else if (firstDigit == "0")
                {
                    var values = result.Split(new[] { ',' });

                    var icNumber = values[2];
                    var passenggerName = values[3];

                    var responseResult = icNumber + "-" + passenggerName.Replace("     ", "");

                    return responseResult;
                }

                return result;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public async Task<AFCServiceByCounter> GetAFCServiceByCounter(string counterId)
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
