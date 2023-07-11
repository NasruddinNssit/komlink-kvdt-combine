using Newtonsoft.Json;
using NssIT.Kiosk.AppDecorator.Log;
using NssIT.Kiosk.Log.DB;
using NssIT.Train.Kiosk.Common.Common.WebApi;
using NssIT.Train.Kiosk.Common.Data.PostRequestParam;
using NssIT.Train.Kiosk.Common.Data.Response;
using NssIT.Train.Kiosk.Common.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TrainSeatRnD
{
    public class WebApiConn
    {
        public const string ApiCodeOK = "0";

        private string _baseAPIUrl = @"https://localhost:44305/api/";
        private string _apiUrl = @"TrainService/getTrip";

        public dynamic GetTestData(IPostRequestParam parameters)
        {
            HttpResponseMessage response;
            TimeSpan apiTimeOutSec = new TimeSpan(0, 0, 45);

            dynamic oReturn = null;
            try
            {
                var httpClientHandler = new HttpClientHandler();
                using (var client = new HttpClient(httpClientHandler))
                {
                    System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls | SecurityProtocolType.Ssl3;

                    client.Timeout = apiTimeOutSec;
                    client.BaseAddress = new Uri(_baseAPIUrl);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    // Authentication
                    client.DefaultRequestHeaders.Add("RequestSignature", SecurityHelper.getSignature());
                    // --------------------

                    //Station oStation = new Station();

                    response = null;
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, _apiUrl);
                    string StrRequest = null;
                    if (parameters != null)
                    {
                        StrRequest = JsonConvert.SerializeObject(parameters);
                        request.Content = new System.Net.Http.StringContent(StrRequest, Encoding.UTF8, "application/json");
                    }

                    //request.Content = new System.Net.Http.StringContent(StrRequest, Encoding.UTF8, "application/json");
                    //request.Content = new System.Net.Http.StringContent("", Encoding.UTF8, "application/json");
                    response = client.SendAsync(request).ConfigureAwait(false).GetAwaiter().GetResult();

                    if (response.IsSuccessStatusCode)
                    {
                        dynamic oRet = null;
                        string result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                        //oRet = (TRet)Activator.CreateInstance(typeof(TRet));
                        oRet = JsonConvert.DeserializeObject<TrainSeatResult>(result);
                        if ((oRet is iWebApiResult apiRes) && (apiRes.Status == true) && (apiRes.Code != null) && (apiRes.Code.Trim().Equals(ApiCodeOK) == true))
                        {
                            /* Success - By Pass */
                            oReturn = oRet;
                        }
                        else if ((oRet is iWebApiResult apiRes2))
                        {
                            string errMsg1 = $@"Unable to read from web; Status : {apiRes2.Status}; Code : {apiRes2.Code}; (EXIT700000103)";
                            if (apiRes2.Messages?.Count > 0)
                                apiRes2.Messages.Add(errMsg1);
                            else
                                apiRes2.Messages = new List<string>(new string[] { errMsg1 });

                            oReturn = new WebApiException(apiRes2.Messages, apiRes2.Code);
                        }
                        else
                        {
                            oReturn = new WebApiException("Unrecognized data; (EXIT700000104)");
                        }
                    }
                    else
                    {
                        oReturn = new WebApiException($@"Web no response ; R.Code : {response?.StatusCode}; (EXIT700000105)");
                    }
                }
                return oReturn;
            }
            catch (ThreadAbortException) { }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                //endThreadWork = true;
            }

            return new WebApiException("Unrecognized error; (EXIT700000105)");
        }
    }
}
