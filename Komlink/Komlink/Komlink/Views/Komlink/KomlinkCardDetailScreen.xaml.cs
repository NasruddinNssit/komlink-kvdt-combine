using Komlink.Models;
using Komlink.Services;
using Komlink.Views.Komlink.Payment.PayWave;
using Newtonsoft.Json;
using NssIT.Train.Kiosk.Common.Helper;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Komlink.Views.Komlink
{
    /// <summary>
    /// Interaction logic for KomlinkCardDetailScreen.xaml
    /// </summary>
    public partial class KomlinkCardDetailScreen : UserControl
    {

        CardDetail cardDetail;
        PaymentAmountOption paymentAmountOption;
        BackButton backButton;

        PaymentOption paymentOption;

        KeyPad keypad;

        ExitButton exitButton;

        TransactionDate transactionDate = new TransactionDate();

        TableTransaction tableTransaction;

        Calendar calendar;

        DateTime _startDate;
        DateTime _endDate;
       

        ConfirmButton confirmButton;

        CardPayWave cardPayWave = null;
        public KomlinkCardDetailScreen()
        {
            InitializeComponent();
            LoadLanguage();
            cardDetail = new CardDetail();
            KomlinkDetail.Children.Add(cardDetail);

            exitButton = new ExitButton();
            BackOrExitButton.Children.Add(exitButton);

            cardPayWave = CardPayWave.GetCreditCardPayWavePage();
            
        }

        private void LoadLanguage()
        {
            try
            {
                if(App.Language == "ms")
                {
                    KomlinkCardText.Text = "Komlink Kad";
                    TopUpBtnText.Content = "Tambah Nilai";
                    ViewTransactionText.Content = "Papar Transaksi";
                }
            }catch(Exception ex)
            {
                Log.Logger.Error(ex, UserSession.SessionId + " Error in KomlinkCardDetailScreen.xaml.cs");

            }
        }

        private void Button_TopUp_Click(object sender, RoutedEventArgs e)
        {
            ConfirmButton.Children.Clear();
            KomlinkToUpSection.Children.Clear();
            paymentAmountOption = new PaymentAmountOption();
            KomlinkToUpSection.Children.Add(paymentAmountOption);

            paymentAmountOption.TextBoxClicked += TextBox_Clicked;

            PaymentOptionOrKeypad.Children.Clear();

            paymentOption = new PaymentOption();
            paymentOption.ButtonPayWaveClicked += ButtonPayWaveClicked;

            PaymentOptionOrKeypad.Children.Add(paymentOption);

            BackOrExitButton.Children.Clear();

            backButton = new BackButton();

            backButton.BackButtonClicked += BackButton_Clicked;

            BackOrExitButton.Children.Add(backButton);
        }

        private void BackButton_Clicked(object sender, EventArgs e)
        {
            ConfirmButton.Children.Clear();
            KomlinkToUpSection.Children?.Clear();

            PaymentOptionOrKeypad.Children?.Clear();

            BackOrExitButton.Children.Clear();

            BackOrExitButton.Children.Add(exitButton);
        }

        private void TextBox_Clicked(object sender, EventArgs e)
        {
            PaymentOptionOrKeypad.Children.Clear();

            keypad = new KeyPad();
            keypad.KeyPadClicked += KeyPad_Clicked;
            keypad.KeyPadDeletedClicked += KeyPadDelete_Clicked;
            keypad.KeyPadEnterClicked += KeyPadEnter_Clicked;

            keypad.keyPadCancelClicked += KeyPadCancel_Clicked;
            PaymentOptionOrKeypad.Children.Add(keypad);

            BackOrExitButton.Children?.Clear();

            paymentAmountOption.SetText(0);

            paymentAmountOption.SetAllButtonUnSelected();
        }

        private void KeyPad_Clicked(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            if (button != null)
            {
                string amountText = button.Content.ToString();

                int amount = Convert.ToInt32(amountText);

                paymentAmountOption.SetText(amount);
            }

            UpdateEnterButton();
        }

        private void KeyPadEnter_Clicked(object sender, EventArgs e)
        {
            ConfirmButton.Children.Clear();
            PaymentOptionOrKeypad.Children.Clear();
            PaymentOptionOrKeypad.Children.Add(paymentOption);
            BackOrExitButton?.Children?.Clear();
            BackOrExitButton?.Children.Add(backButton);
        }

        private void KeyPadCancel_Clicked(object sender, EventArgs e)
        {


            ConfirmButton.Children.Clear();
            PaymentOptionOrKeypad.Children.Clear();
            PaymentOptionOrKeypad.Children.Add(paymentOption);
            BackOrExitButton?.Children?.Clear();
            BackOrExitButton?.Children.Add(backButton);


            if (! paymentAmountOption.GetText().Equals(""))
            {
                paymentAmountOption.SetText(0);
            }
           
        }

        private void KeyPadDelete_Clicked(object sender, EventArgs e)
        {
            string text = paymentAmountOption.GetText();

            if (string.IsNullOrEmpty(text))
            {
                return; // Exit the method if the text is empty or null
            }

            if (text.Length == 1)
            {
                paymentAmountOption.SetTextAfterDelete(0); // Set a default value when the text has only one character
                keypad.DisableEnterButton();
                return;
            }

            int amount;
            if (int.TryParse(text.Remove(text.Length - 1), out amount))
            {
                paymentAmountOption.SetTextAfterDelete(amount);
            }
            else
            {
                // Handle the case when the text cannot be parsed to an integer
                // You can display an error message or perform alternative actions
            }
        }


        private void UpdateEnterButton()
        {
            keypad.UpdateEnterButton();
        }

        private void Button_Transaction_Clicked(object sender, RoutedEventArgs e)
        {
            ConfirmButton.Children.Clear();
            KomlinkToUpSection.Children?.Clear();
            PaymentOptionOrKeypad.Children?.Clear();
            BackOrExitButton?.Children?.Clear();
          

            transactionDate.DateRangeClicked += DateRangeClicked;
            transactionDate.Past1WeekClicked += BtnPast1WeekClicked;
            transactionDate.Past30DayClicked += BtnPast30DayClicked;
            transactionDate.TodayClicked += BtnTodayClicked;

            KomlinkToUpSection?.Children?.Add(transactionDate);

            tableTransaction = new TableTransaction();


            //List<KomlinkTransactionDetail> komlinkTransactions = getTransactionData().Result;

            //tableTransaction.AddData(komlinkTransactions);
            PaymentOptionOrKeypad.Children?.Add(tableTransaction);

            exitButton = new ExitButton();
            BackOrExitButton?.Children?.Add(exitButton);
        }

        private void TransactionDate_Past30DayClicked(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void DateRangeClicked(object sender, EventArgs e)
        {
            PaymentOptionOrKeypad.Children.Clear();
            calendar = new Calendar();
            calendar.SelectedStartDateChanged += setStartDateChange;
            calendar.SelectedEndDateChanged += setEndDateChange;
            PaymentOptionOrKeypad?.Children?.Add(calendar);

            ConfirmButton.Children.Clear();
            confirmButton = new ConfirmButton();
            confirmButton.ButtonConfirmClicked += ButtonConfirmClicked;
            ConfirmButton?.Children?.Add(confirmButton);
        }

        private async void ButtonConfirmClicked(object sender, EventArgs e)
        {
            PaymentOptionOrKeypad.Children.Clear();
            ConfirmButton.Children.Clear();
            PaymentOptionOrKeypad.Children?.Add(tableTransaction);


            List<KomlinkTransactionDetail> komlinkTransactions = await getTransactionData(_startDate, _endDate);

            List<KomlinkTransactionDetailItem> komlinkTransactionDetailItems = new List<KomlinkTransactionDetailItem>();


            foreach (KomlinkTransactionDetail item in komlinkTransactions)
            {
                KomlinkTransactionDetailItem kTdI = new KomlinkTransactionDetailItem();
              
                
               
                switch(item.TransactionType.ToString())
                {
                    case "1":
                        if(App.Language == "ms")
                        {
                            kTdI.TransactionType = "Top Up";
                        }
                        else
                        {
                            kTdI.TransactionType = "Top Up";
                        }
                        break;
                    case "2":
                        if (App.Language == "ms")
                        {
                            kTdI.TransactionType = "Pengambilan";
                        }
                        else
                        {
                            kTdI.TransactionType = "Refund";
                        }
                        break;
                    case "3":
                        if(App.Language == "ms")
                        {
                            kTdI.TransactionType = "Pengunaan";
                        }
                        else
                        {
                            kTdI.TransactionType = "Usage";
                        }
                        break;
                    case "4":
                        break;
                    case "5":
                        break;
                        
                    case "6":
                        break;
                    default:
                        break;

                      
                }

                kTdI.Date = item.TransactionDateTime.Date.ToString("dd/MM/yyyy");
                kTdI.Time = item.TransactionDateTime.TimeOfDay.ToString(@"hh\:mm");
                kTdI.Station = item.Station;
                kTdI.TicketType = item.TicketType;
                kTdI.TotalAmount = item.TotalAmount.ToString("C2").Replace("$", "RM");

                komlinkTransactionDetailItems.Add(kTdI);
            }

            tableTransaction.AddData(komlinkTransactionDetailItems);


            //var listData = JsonConvert.SerializeObject(komlinkTransactions.Data);

        }

        private void setStartDateChange(object sender, EventArgs e)
        {

           string date = sender.ToString();
           DateTime startDate  = DateTime.Parse(date);
           string startDateString = startDate.ToString("dd MMM yyyy");
           transactionDate.setStartDateText(startDateString);

            _startDate = startDate;
        }

        private void setEndDateChange(object sender, EventArgs e)
        {
            string date = sender.ToString();
            DateTime endDate = DateTime.Parse(date);
            string endDateString = endDate.ToString("dd MMM yyyy");
            transactionDate.setEndDateText(endDateString);

            _endDate = endDate.AddDays(1).AddTicks(-1);
        }




        private async void BtnPast30DayClicked(object sender, EventArgs e)
        {



            ConfirmButton.Children.Clear();
            PaymentOptionOrKeypad.Children.Clear();
            DateTime endDate = DateTime.Now;
            DateTime startDate = endDate.AddDays(-30);

            transactionDate.setStartDateText(startDate.ToString("dd MMM yyyy"));
            transactionDate.setEndDateText(endDate.ToString("dd MMM yyyy"));

            tableTransaction = new TableTransaction();


            _startDate = startDate;
            _endDate = endDate.AddDays(1).AddTicks(-1);

            List<KomlinkTransactionDetail> komlinkTransactions = await getTransactionData(_startDate, _endDate);



            List<KomlinkTransactionDetailItem> komlinkTransactionDetailItems = new List<KomlinkTransactionDetailItem>();
            foreach (KomlinkTransactionDetail item in komlinkTransactions)
            {
                KomlinkTransactionDetailItem kTdI = new KomlinkTransactionDetailItem();



                switch (item.TransactionType.ToString())
                {
                    case "1":
                        if (App.Language == "ms")
                        {
                            kTdI.TransactionType = "Top Up";
                        }
                        else
                        {
                            kTdI.TransactionType = "Top Up";
                        }
                        break;
                    case "2":
                        if (App.Language == "ms")
                        {
                            kTdI.TransactionType = "Pengambilan";
                        }
                        else
                        {
                            kTdI.TransactionType = "Refund";
                        }
                        break;
                    case "3":
                        if (App.Language == "ms")
                        {
                            kTdI.TransactionType = "Pengunaan";
                        }
                        else
                        {
                            kTdI.TransactionType = "Usage";
                        }
                        break;
                    case "4":
                        break;
                    case "5":
                        break;

                    case "6":
                        break;
                    default:
                        break;


                }

                kTdI.Date = item.TransactionDateTime.Date.ToString("dd/MM/yyyy");
                kTdI.Time = item.TransactionDateTime.TimeOfDay.ToString(@"hh\:mm");
                kTdI.Station = item.Station;
                kTdI.TicketType = item.TicketType;
                kTdI.TotalAmount = item.TotalAmount.ToString("C2").Replace("$", "RM"); 

                komlinkTransactionDetailItems.Add(kTdI);
            }
            tableTransaction.AddData(komlinkTransactionDetailItems);
            PaymentOptionOrKeypad.Children?.Add(tableTransaction);
        }

        private async  void BtnPast1WeekClicked(object sender, EventArgs e)
        {
            ConfirmButton.Children.Clear();
            PaymentOptionOrKeypad.Children.Clear();

            DateTime startDate = DateTime.Now;
            DateTime endDate = startDate.AddDays(-7);

            transactionDate.setStartDateText(startDate.ToString("dd MMM yyyy"));
            transactionDate.setEndDateText(endDate.ToString("dd MMM yyyy"));

            tableTransaction = new TableTransaction();

            _startDate = startDate;
            _endDate = endDate.AddDays(1).AddTicks(-1);

            List<KomlinkTransactionDetail> komlinkTransactions = await getTransactionData(_startDate, _endDate);



            List<KomlinkTransactionDetailItem> komlinkTransactionDetailItems = new List<KomlinkTransactionDetailItem>();
            foreach (KomlinkTransactionDetail item in komlinkTransactions)
            {
                KomlinkTransactionDetailItem kTdI = new KomlinkTransactionDetailItem();



                switch (item.TransactionType.ToString())
                {
                    case "1":
                        if (App.Language == "ms")
                        {
                            kTdI.TransactionType = "Top Up";
                        }
                        else
                        {
                            kTdI.TransactionType = "Top Up";
                        }
                        break;
                    case "2":
                        if (App.Language == "ms")
                        {
                            kTdI.TransactionType = "Pengambilan";
                        }
                        else
                        {
                            kTdI.TransactionType = "Refund";
                        }
                        break;
                    case "3":
                        if (App.Language == "ms")
                        {
                            kTdI.TransactionType = "Pengunaan";
                        }
                        else
                        {
                            kTdI.TransactionType = "Usage";
                        }
                        break;
                    case "4":
                        break;
                    case "5":
                        break;

                    case "6":
                        break;
                    default:
                        break;


                }

                kTdI.Date = item.TransactionDateTime.Date.ToString("dd/MM/yyyy");
                kTdI.Time = item.TransactionDateTime.TimeOfDay.ToString(@"hh\:mm");
                kTdI.Station = item.Station;
                kTdI.TicketType = item.TicketType;
                kTdI.TotalAmount = item.TotalAmount.ToString("C2").Replace("$", "RM"); ;

                komlinkTransactionDetailItems.Add(kTdI);
            }
            tableTransaction.AddData(komlinkTransactionDetailItems);
            PaymentOptionOrKeypad.Children?.Add(tableTransaction);
        }

        private async void BtnTodayClicked(object sender, EventArgs e)
        {
            ConfirmButton.Children.Clear();
            PaymentOptionOrKeypad.Children.Clear();
            DateTime startDate = DateTime.UtcNow.Date;
            DateTime endDate = startDate.AddDays(1).AddTicks(-1);

            transactionDate.setStartDateText(startDate.ToString("dd MMM yyyy"));
            transactionDate.setEndDateText(startDate.ToString("dd MMM yyyy"));


            tableTransaction = new TableTransaction();

            _startDate = startDate;
            _endDate = endDate.AddDays(1).AddTicks(-1);

            List<KomlinkTransactionDetail> komlinkTransactions = await getTransactionData(_startDate, _endDate);



            List<KomlinkTransactionDetailItem> komlinkTransactionDetailItems = new List<KomlinkTransactionDetailItem>();
            foreach(KomlinkTransactionDetail item in komlinkTransactions)
            {
                KomlinkTransactionDetailItem kTdI = new KomlinkTransactionDetailItem();



                switch (item.TransactionType.ToString())
                {
                    case "1":
                        if (App.Language == "ms")
                        {
                            kTdI.TransactionType = "Top Up";
                        }
                        else
                        {
                            kTdI.TransactionType = "Top Up";
                        }
                        break;
                    case "2":
                        if (App.Language == "ms")
                        {
                            kTdI.TransactionType = "Pengambilan";
                        }
                        else
                        {
                            kTdI.TransactionType = "Refund";
                        }
                        break;
                    case "3":
                        if (App.Language == "ms")
                        {
                            kTdI.TransactionType = "Pengunaan";
                        }
                        else
                        {
                            kTdI.TransactionType = "Usage";
                        }
                        break;
                    case "4":
                        break;
                    case "5":
                        break;

                    case "6":
                        break;
                    default:
                        break;


                }

                kTdI.Date = item.TransactionDateTime.Date.ToString("dd/MM/yyyy");
                kTdI.Time = item.TransactionDateTime.TimeOfDay.ToString(@"hh\:mm");
                kTdI.Station = item.Station;
                kTdI.TicketType = item.TicketType;
                kTdI.TotalAmount = item.TotalAmount.ToString("C2").Replace("$", "RM");

                komlinkTransactionDetailItems.Add(kTdI);
            }

            

            tableTransaction.AddData(komlinkTransactionDetailItems);

           
            PaymentOptionOrKeypad.Children?.Add(tableTransaction);
        }


        private async Task<List<KomlinkTransactionDetail>> getTransactionData(DateTime startDate, DateTime endDate)
        {

            //KomlinkCardDetailModel data = ScreenNavigation.KomlinkCardDetails;

            //APIServices aPIServices = new APIServices(new APIURLServices(), new APISignatureServices());
            //var result = aPIServices.GetKomlinkTransactionDetail().Result;



            //return result?.Data;

            List<KomlinkTransactionDetail> resultModel = null;

            try
            {

                string komlinkCard = UserSession.CurrentUserSession.CardData;

                //string info = "{\"CSN\":\"04097902DB\",\"KVDTCardNo\":227182759798068397,\"IsCardCancelled\":false,\"IsCardBlacklisted\":false,\"IsSP1Available\":true,\"IsSP2Available\":true,\"IsSP3Available\":false,\"IsSP4Available\":false,\"IsSP5Available\":false,\"IsSP6Available\":false,\"IsSP7Available\":false,\"IsSP8Available\":false,\"ChkInGateNo\":\"\",\"ChkInDatetime\":\"2001-01-01T00:00:00\",\"ChkInStationNo\":\"\",\"ChkOutGateNo\":\"\",\"ChkOutDatetime\":\"2001-01-01T00:00:00\",\"ChkOutStationNo\":\"\",\"MainPurse\":21639268,\"MainTransNo\":65000,\"IssuerSAMId\":\"0CA51022F7CC92DD\",\"Gender\":\"M\",\"CardIssuedDate\":\"2043-06-05T00:00:00\",\"CardExpireDate\":\"2001-01-01T00:00:00\",\"PNR\":\"PNR-E7B07F86\",\"CardType\":\"oku\",\"CardTypeExpireDate\":\"2023-07-28T00:00:00\",\"IsMalaysian\":false,\"DOB\":\"2001-06-22T00:00:00\",\"LRCKey\":182,\"IDType\":\"PassportNo\",\"IDNo\":\"980521595019345\",\"PassengerName\":\"LEONG WAI LIEMmm\",\"BLKStartDatetime\":\"2099-12-31T00:00:00\",\"RefillSAMId\":\"0CA51022F7CC92DD\",\"RefillDatetime\":\"2023-06-05T13:47:26\",\"LastTransDatetime\":\"2023-06-08T09:07:20\",\"BackupPurse\":21639268,\"BackupTransNo\":65000,\"BLKSAMId\":\"0000000000000000\",\"BLKDatetime\":\"2001-01-01T00:00:00\",\"KomLinkSAMId\":\"0CA51022F7CC92DD\",\"MerchantNameAddr\":null,\"TransactionDateTime\":null,\"KomLinkSeasonPassDatas\":[{\"SPNo\":1,\"SPSaleDate\":\"2023-06-07T00:00:00\",\"SPMaxTravelAmtPDayPTrip\":0.1,\"SPIssuerSAMId\":\"0CA51022F7CC92DD\",\"SPStartDate\":\"2023-06-14T00:00:00\",\"SPEndDate\":\"2023-06-20T00:00:00\",\"SPSaleDocNo\":\"KCT230608353297 \",\"SPServiceCode\":\"KV \",\"SPPackageCode\":\"KVAD\",\"SPType\":\"KVWP\",\"SPMaxTripCountPDay\":10,\"SPIsAvoidChecking\":false,\"SPIsAvoidTripDurationCheck\":false,\"SPOriginStationNo\":\"50600 \",\"SPDestinationStationNo\":\"54500 \",\"SPLastTravelDate\":\"2001-01-01T00:00:00\",\"SPDailyTravelTripCount\":0},{\"SPNo\":2,\"SPSaleDate\":\"2023-06-03T00:00:00\",\"SPMaxTravelAmtPDayPTrip\":1,\"SPIssuerSAMId\":\"0CA51022F7CC92DD\",\"SPStartDate\":\"2023-06-03T00:00:00\",\"SPEndDate\":\"2023-09-13T00:00:00\",\"SPSaleDocNo\":\"SP-DOC-111156789\",\"SPServiceCode\":\"KVDT\",\"SPPackageCode\":\"PK01\",\"SPType\":\"WEEK\",\"SPMaxTripCountPDay\":8,\"SPIsAvoidChecking\":false,\"SPIsAvoidTripDurationCheck\":false,\"SPOriginStationNo\":\"19000 \",\"SPDestinationStationNo\":\"19700 \",\"SPLastTravelDate\":\"2023-06-06T00:00:00\",\"SPDailyTravelTripCount\":1}]}";


                KomlinkCardTransactionRequesModel komlinkCardTransactionRequesModel = new KomlinkCardTransactionRequesModel();
                komlinkCardTransactionRequesModel.DateFrom = startDate;
                komlinkCardTransactionRequesModel.DateTo = endDate;

               

                komlinkCardTransactionRequesModel.KomlinkCardDetails = komlinkCard;

                APIServices aPIServices = new APIServices(new APIURLServices(), new APISignatureServices());

                List<KomlinkTransactionDetail> result = await aPIServices.GetKomlinkCardTransaction(komlinkCardTransactionRequesModel);



                resultModel = result;

            }
            catch (Exception ex)
            {

            }

            return resultModel;

        }


        private void ButtonPayWaveClicked(object sender, EventArgs e)
        {
            UserSession.TotalTicketPrice = decimal.Parse(paymentAmountOption.GetText());

            cardPayWave.ClearEvents();
            cardPayWave.InitPaymentData(UserSession.CurrencyCode, 5, UserSession.SessionId, App.PayWaveCOMPORT, null);


            cardPayWave.OnEndPayment += CardPayWave_OnEndPayment;
            FrmSubFrame.Content = null;
            FrmSubFrame.NavigationService.RemoveBackEntry();
            FrmSubFrame.NavigationService.Content = null;
            System.Windows.Forms.Application.DoEvents();

            FrmSubFrame.NavigationService.Navigate(cardPayWave);
            BdSubFrame.Visibility = Visibility.Visible;
            System.Windows.Forms.Application.DoEvents();
        }

        private void CardPayWave_OnEndPayment(object sender, EndOfPaymentEventArgs e)
        {
            throw new NotImplementedException();
        }

    }
}
