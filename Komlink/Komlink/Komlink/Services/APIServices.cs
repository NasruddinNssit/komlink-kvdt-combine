using Komlink.Interface;
using Komlink.Models;
using Komlink.Views.Error;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Komlink.Services
{
    public class APIServices : IAPIServices
    {
        private readonly APIURLServices aPIURLServices;
        private readonly APISignatureServices aPISignatureServices;

        public APIServices(APIURLServices aPIURLServices, APISignatureServices aPISignatureServices)
        {
            this.aPIURLServices = aPIURLServices;
            this.aPISignatureServices = aPISignatureServices;
        }

        public async Task<ResultModel> CompleteTopUp(KomlinkCardCheckoutTopupRequestModel model)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var request = new HttpRequestMessage(HttpMethod.Post, aPIURLServices.CheckOutTopUp);
                    request.Headers.Add("RequestSignature", aPISignatureServices.GetSignature());

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

                        var data = JsonConvert.DeserializeObject<ResultModel>(result);

                        if (data != null)
                        {
                            return data;
                        }
                    }

                    throw new Exception("Failed to get API response.");
                }
            }catch(Exception ex)
            {
                throw;
            }
          
        }


        public async Task UpdateWriteStatus(KomlinkCardCancelTopupRequestModel model)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var request = new HttpRequestMessage(HttpMethod.Post, aPIURLServices.UpdateWriteStatus);

                    request.Headers.Add("RequestSignature", aPISignatureServices.GetSignature());


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

                        var data = JsonConvert.DeserializeObject<ResultModel>(result);



                    }

                    throw new Exception("Failed to get API response.");
                }
            }
            catch (Exception ex)
            {

            }
        }
        public async Task CancelTopUp(KomlinkCardCancelTopupRequestModel model)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var request = new HttpRequestMessage(HttpMethod.Post, aPIURLServices.CancelTopUp);

                    request.Headers.Add("RequestSignature", aPISignatureServices.GetSignature());


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

                        var data = JsonConvert.DeserializeObject<ResultModel>(result);

                        

                    }

                    throw new Exception("Failed to get API response.");
                }
            }
            catch(Exception ex)
            {

            }
        }

        public async Task<KomlinkCardAddTopupResultModel> AddTopUp (KomlinkCardAddTopUpRequestModel model)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var request = new HttpRequestMessage(HttpMethod.Post, aPIURLServices.AddTopUp);

                    request.Headers.Add("RequestSignature", aPISignatureServices.GetSignature());


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

                        var data = JsonConvert.DeserializeObject<ResultModel>(result);

                        if(data.Status == true)
                        {
                            string json = JsonConvert.SerializeObject(data.Data, Formatting.Indented);
                            var komlinkCardAddTopUpResultModel = JsonConvert.DeserializeObject<KomlinkCardAddTopupResultModel>(json);

                            if (komlinkCardAddTopUpResultModel != null)
                            {
                                return komlinkCardAddTopUpResultModel;
                            }
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
        public async Task<List<KomlinkTransactionDetail>> GetKomlinkCardTransaction(KomlinkCardTransactionRequesModel model)
        {
            try
            {
                var resList = new List<KomlinkTransactionDetail>();
                using(var client = new HttpClient())
                {
                    var request = new HttpRequestMessage(HttpMethod.Post, aPIURLServices.GetKomlinkCardTransaction);
                    request.Headers.Add("RequestSignature", aPISignatureServices.GetSignature());
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

                        var resultData = JsonConvert.DeserializeObject<ResultModel>(result);

                        //if (resultData.Data != null)
                        //{
                        //    return resultData;
                        //}

                        foreach(var item in resultData.Data)
                        {
                            var res = new KomlinkTransactionDetail();


                            res.TransactionDateTime = item.TransactionDateTime;
                            res.TransactionType = item.TransactionType;
                            res.TicketType = item.TicketType;
                            res.TotalAmount = item.TotalAmount;
                            res.Station = item.Station;
                            
                            resList.Add(res);
                        }

                        return resList;
                    }

                    throw new Exception("Failed to get API response.");

                }
            }catch(Exception ex)
            {
                throw;
            }
        }
        public async Task<KomlinkCard> SearchKomlinkCard(JObject model)
        {
            try
            {
                if (model == null)
                {
                    throw new ArgumentNullException(nameof(model), "Model is null.");
                }

                using (HttpClient client = new HttpClient())
                {
                    var request = new HttpRequestMessage(HttpMethod.Post, aPIURLServices.SearchKomlinkCard);
                    request.Headers.Add("RequestSignature", aPISignatureServices.GetSignature());
                    string jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(model);
                    HttpContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                    request.Content = content;

                    var response =  client.SendAsync(request).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();
                        KomlinkCard komlinkCard = Newtonsoft.Json.JsonConvert.DeserializeObject<KomlinkCard>(responseContent);
                        return komlinkCard;
                    }

                    throw new Exception("Failed to get a successful API response. Status code: " + response.StatusCode);
                }
            }
            catch (ArgumentNullException ex)
            {
                ErrorWindow errorWindow = new ErrorWindow(ex.Message);
                errorWindow.Show();
            }
            catch (HttpRequestException ex)
            {
                ErrorWindow errorWindow = new ErrorWindow("Failed to send HTTP request: " + ex.Message);
                errorWindow.Show();
            }
            catch (TaskCanceledException ex)
            {
                ErrorWindow errorWindow = new ErrorWindow("The request was canceled: " + ex.Message);
                errorWindow.Show();
            }
            catch (Exception ex)
            {
                ErrorWindow errorWindow = new ErrorWindow("An error occurred: " + ex.Message);
                errorWindow.Show();
            }

            return null;
        }

        public async Task<KomlinkCard> GetKomlinkCardDetail()
        {
            try
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Post, aPIURLServices.GetKomlinkCard);
                request.Headers.Add("RequestSignature", aPISignatureServices.GetSignature());

                var response = client.SendAsync(request).Result;


                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();

                    if (string.IsNullOrEmpty(result))
                    {
                        throw new Exception("Failed to deserialize response.");
                    }

                    KomlinkCard deserializeResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<KomlinkCard>(result);
                    if (deserializeResponse != null)
                    {
                        return deserializeResponse;
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

        public async Task<KomlinkTransaction> GetKomlinkTransactionDetail()
        {
            try
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Post, aPIURLServices.GetKomlinkTransaction);
                request.Headers.Add("RequestSignature", aPISignatureServices.GetSignature());

                var response = client.SendAsync(request).Result;

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(result))
                    {
                        throw new Exception("Failed to deserialize response.");
                    }

                    KomlinkTransaction deserializedResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<KomlinkTransaction>(result);
                    if (deserializedResponse != null)
                    {
                        return deserializedResponse;
                    }
                }
                throw new Exception("Failed to get API response");
            }
            catch (Exception ex)
            {
                ErrorWindow errorWindow = new ErrorWindow(ex.Message);
                errorWindow.Show();
                return null;
            }
        }

        public async Task<AFCServiceByCounter> GetAFCServiceByCounter(string counterId)
        {
            try
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Post, aPIURLServices.GetAFCServiceByCounter + "counterId=" + counterId);
                request.Headers.Add("RequestSignature", aPISignatureServices.GetSignature());

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
