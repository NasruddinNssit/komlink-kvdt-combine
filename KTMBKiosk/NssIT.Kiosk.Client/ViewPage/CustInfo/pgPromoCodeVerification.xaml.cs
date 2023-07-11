using NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI;
using NssIT.Train.Kiosk.Common.Common;
using NssIT.Train.Kiosk.Common.Constants;
using NssIT.Train.Kiosk.Common.Data.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NssIT.Kiosk.Client.ViewPage.CustInfo
{
    /// <summary>
    /// Interaction logic for pgPromoCodeVerification.xaml
    /// </summary>
    public partial class pgPromoCodeVerification : Page
    {
        private const string LogChannel = "ViewPage";

        public event EventHandler<EndOfPromoCodeVerificationEventArgs> OnEndPromoCodeVerification;

        private ResourceDictionary _languageResource = null;
        public string _trainSeatModelId = "";
        public Guid _seatLayoutModelId = Guid.Empty;
        public string _ticketTypesId = "";
        public string _passengerIC = "";
        public string _promoCode = "";

        private UICustPromoCodeVerifyAck _codeVerificationAnswer = null;
        private Thread _codeVerificationThreadWorker = null;

        public pgPromoCodeVerification()
        {
            InitializeComponent();
        }

        private bool _pageLoaded = false;
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            this.Resources.MergedDictionaries.Clear();
            this.Resources.MergedDictionaries.Add(_languageResource);

            EndThreadWorker();
            _codeVerificationThreadWorker = new Thread(VerifyPromoCodeThreadWorking);
            _codeVerificationThreadWorker.IsBackground = true;
            _codeVerificationThreadWorker.Start();

            _pageLoaded = true;
        }

        private void EndThreadWorker()
        {
            if (_codeVerificationThreadWorker != null)
            {
                if (
                    ((_codeVerificationThreadWorker.ThreadState & ThreadState.Stopped) != ThreadState.Stopped)
                    && 
                    ((_codeVerificationThreadWorker.ThreadState & ThreadState.Aborted) != ThreadState.Aborted)
                    )
                {
                    try
                    {
                        _codeVerificationThreadWorker.Abort();
                        Thread.Sleep(500);
                    }
                    catch { }
                    _codeVerificationThreadWorker = null;
                }
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _pageLoaded = false;
        }

        public void InitPromoCode(string trainSeatModelId, Guid seatLayoutModelId, string ticketTypesId, string passengerIC, string promoCode, ResourceDictionary languageResource)
        {
            _languageResource = languageResource;
            _codeVerificationAnswer = null;
            _trainSeatModelId = trainSeatModelId;
            _seatLayoutModelId = seatLayoutModelId;
            _ticketTypesId = ticketTypesId;
            _promoCode = promoCode;
            _passengerIC = passengerIC;
        }

        public void SetPromoCodeVerificationResult(UICustPromoCodeVerifyAck codeVerificationAnswer)
        {
            if (_pageLoaded == true)
                _codeVerificationAnswer = codeVerificationAnswer;
        }

        private void VerifyPromoCodeThreadWorking()
        {
            try
            {
                App.NetClientSvc.SalesService.RequestPromoCodeVerification(_trainSeatModelId, _seatLayoutModelId, _ticketTypesId, _passengerIC, _promoCode,
                    out bool isServerResponded);

                if (isServerResponded == true)
                {
                    DateTime startTime = DateTime.Now;
                    DateTime endTime = startTime.AddSeconds(20);

                    while ((endTime.Subtract(DateTime.Now).TotalSeconds > 0) && (_codeVerificationAnswer == null))
                        Task.Delay(100).Wait();
                    
                    if (_codeVerificationAnswer == null)
                    {
                        RaiseOnEndPromoCodeVerification(errorMessage: "Local Kiosk Service not responding; (E2)");
                    }
                    else if (string.IsNullOrWhiteSpace(_codeVerificationAnswer.ErrorMessage) == false)
                    {
                        RaiseOnEndPromoCodeVerification(errorMessage: _codeVerificationAnswer.ErrorMessage);
                    }
                    else if (_codeVerificationAnswer.MessageData is PromoCodeVerifyResult pResult)
                    {
                        if (pResult.Code.Equals(WebAPIAgent.ApiCodeOK) && (pResult.Data?.Error?.Equals(YesNo.No) == true))
                        {
                            RaiseOnEndPromoCodeVerification(isSuccess: true);
                        }
                        else if (string.IsNullOrWhiteSpace(pResult.MessageString()) == false)
                        {
                            RaiseOnEndPromoCodeVerification(errorMessage: pResult.MessageString());
                        }
                        else
                        {
                            RaiseOnEndPromoCodeVerification(errorMessage: "Unknown Promo Code Verification UI Error");
                        }
                    }
                    else
                    {
                        RaiseOnEndPromoCodeVerification(errorMessage: "Unknown Promo Code Verification UI Error (2x)");
                    }
                }
                else
                {
                    RaiseOnEndPromoCodeVerification(errorMessage: "Local Kiosk Service not responding");
                }
            }
            catch (Exception ex)
            {
                App.Log.LogError(LogChannel, "", new Exception("(EXIT10000835)", ex), "EX01", "pgPromoCodeVerification.VerifyPromoCodeThreadWorking");
                RaiseOnEndPromoCodeVerification(errorMessage: $@"Promo Code Verification UI Error; {ex.Message}");
            }

            return;
        }

        private void RaiseOnEndPromoCodeVerification(bool isSuccess = false, string errorMessage = null)
        {
            if (_pageLoaded == false)
                return;

            try
            {
                if (OnEndPromoCodeVerification != null)
                {
                    if (string.IsNullOrWhiteSpace(errorMessage) == false)
                    {
                        OnEndPromoCodeVerification.Invoke(null, new EndOfPromoCodeVerificationEventArgs(errorMessage));
                    }
                    else if (isSuccess == true)
                    {
                        OnEndPromoCodeVerification.Invoke(null, new EndOfPromoCodeVerificationEventArgs());
                    }
                }
            }
            catch (Exception ex)
            {
                App.Log.LogError(LogChannel, "-", ex, "EX01", "pgPromoCodeVerification.RaiseOnEndPromoCodeVerification");
            }
        }

        public void ClearEventSubscription()
        {
            if (OnEndPromoCodeVerification != null)
            {
                Delegate[] delgList = OnEndPromoCodeVerification.GetInvocationList();
                foreach (EventHandler<EndOfPromoCodeVerificationEventArgs> delg in delgList)
                    OnEndPromoCodeVerification -= delg;
            }
        }
    }
}
