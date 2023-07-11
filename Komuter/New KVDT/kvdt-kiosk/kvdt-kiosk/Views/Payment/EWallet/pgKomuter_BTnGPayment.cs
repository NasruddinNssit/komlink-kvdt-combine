
using kvdt_kiosk.Models;
using kvdt_kiosk.Views.Payment.PayWave;
using kvdt_kiosk.Views.Printing;
using kvdt_kiosk.Views.PurchaseTicket;
using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.Log.DB;
using NssIT.Kiosk.Tools.ThreadMonitor;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace kvdt_kiosk.Views.Payment.EWallet
{
    public class pgKomuter_BTnGPayment
    {
        private string _logChannel = "Payment";
        private pgBTnGPayment _bTnGPaymentPage = null;
        private Frame _frmSubFrame = null;
        private ResourceDictionary _langRec = null;
        private Border _bdSubFrame = null;
        private Frame _frmPint = null;
        private PrintingTemplateScreen _printingTemplateScreen = null;
        private PurchaseTicketScreen _purchaseTicketScreen = null;
 
        private LanguageCode _language = LanguageCode.English;

        private string _currency = null;
        private decimal _amount = 0.0M;
        private string _bookingNo = null;
        private string _seatBookingId = null;
        private string _paymentGateway = null;
        private string _paymentMethod = null;
        private string _firstName = null;
        private string _lastName = null;
        private string _contactNo = null;
        private string _paymentGatewayLogoUrl = null;

        public string LastBTnGSaleTransNo { get; private set; } = null;


        public pgKomuter_BTnGPayment(PurchaseTicketScreen purchaseTicketScreen,Frame frmSubFrame, Border bdSubFrame, Frame frmPrinting, PrintingTemplateScreen printingTemplateScreen) 
        { 
            _frmSubFrame = frmSubFrame;
            _bdSubFrame = bdSubFrame;
            _frmPint = frmPrinting; 
            _printingTemplateScreen = printingTemplateScreen;
            _purchaseTicketScreen = purchaseTicketScreen;

            _bTnGPaymentPage = pgBTnGPayment.GetBTnGPage();
        }

        private RunThreadMan _printingThreadWorker = null;
        private RunThreadMan _endPaymentThreadWorker = null;



        public void PageUnloaded()
        {
            _bTnGPaymentPage.ClearEvents();
        }

        public void BTnGShowPaymentInfo(IKioskMsg kioskMsg)
        {
            _bTnGPaymentPage.BTnGShowPaymentInfo(kioskMsg);
        }


        public void StartBTnGPayment(string currency, decimal amount, string refNo, string paymentGateway, string firstName, string lastName, string contactNo,
           string seatBookingId, string paymentGatewayLogoUrl, string paymentMethod,
           ResourceDictionary languageResource, LanguageCode language)
        {
            try
            {
                _langRec = languageResource;
                _language = language;

                LastBTnGSaleTransNo = null;
                _currency = currency;
                _amount = amount;
                _bookingNo = refNo;
                _paymentGateway = paymentGateway;
                _paymentMethod = paymentMethod;
                _firstName = firstName;
                _lastName = lastName;
                _contactNo = contactNo;
                _seatBookingId = seatBookingId;
                _paymentGatewayLogoUrl = paymentGatewayLogoUrl;

                _bTnGPaymentPage.ClearEvents();
                _bTnGPaymentPage.OnEndPayment += _bTnGPaymentPage_OnEndPayment;

                _bTnGPaymentPage.InitPaymentData(
                    currency, amount, _bookingNo, paymentGateway,
                    firstName, lastName, contactNo, paymentGatewayLogoUrl, _paymentMethod,
                    languageResource);

                _frmSubFrame.Content = null;
                _frmSubFrame.NavigationService.RemoveBackEntry();
                _frmSubFrame.NavigationService.Content = null;
                System.Windows.Forms.Application.DoEvents();

                //App.CurrentTransStage = TicketTransactionStage.Komuter;
                _frmSubFrame.NavigationService.Navigate(_bTnGPaymentPage);
                _bdSubFrame.Visibility = Visibility.Visible;

                System.Windows.Forms.Application.DoEvents();
            }
            catch (Exception ex)
            {
                //App.Log.LogError(_logChannel, "-", ex, "EX01", "pgKomuter_BTnGPayment.StartBTnGPayment");
            }
        }

        private void _bTnGPaymentPage_OnEndPayment(object sender, EndOfPaymentEventArgs e)
        {
            try
            {
                LastBTnGSaleTransNo = e.BTnGSaleTransactionNo;

                if ((_endPaymentThreadWorker != null) && (_endPaymentThreadWorker.IsEnd == false))
                {
                    _endPaymentThreadWorker.AbortRequest(out _, 2000);
                }
                _endPaymentThreadWorker = new RunThreadMan(new ThreadStart(OnEndPaymentThreadWorking), "pgPayment_BTnGPayment._bTnGPaymentPage_OnEndPayment::Komuter", 120, _logChannel);
            }
            catch (Exception ex)
            {
                Log.Error(_logChannel, "-", ex, "EX02", "pgKomuter_BTnGPayment._cashPaymentPage_OnEndPayment");
            }
            return;
            //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

            /// <summary>
            /// FuncCode:EXIT80.069A
            /// </summary>
            void OnEndPaymentThreadWorking()
            {
                try
                {
                    App.ShowDebugMsg("_bTnGPaymentPage_OnEndPayment.. go to ticket printing");

                    if (e.ResultState == NssIT.Kiosk.AppDecorator.Common.PaymentResult.Success)
                    {
                        _purchaseTicketScreen.Dispatcher.Invoke(new Action(() =>
                        {
                            _printingTemplateScreen.UpdatePaymentComplete(UserSession.UpdateAFCBookingResultModel.BookingNo);
                            _bTnGPaymentPage.ClearEvents();
                            _frmSubFrame.Content = null;
                            _frmSubFrame.NavigationService.RemoveBackEntry();
                            _frmSubFrame.NavigationService.Content = null;
                            _bdSubFrame.Visibility = Visibility.Collapsed;

                            System.Windows.Forms.Application.DoEvents();

                            _frmPint.Content = null;
                            _frmPint.NavigationService.RemoveBackEntry();
                            _frmPint.NavigationService.Navigate(_printingTemplateScreen);
                            _frmPint.Visibility = Visibility.Visible;
                            System.Windows.Forms.Application.DoEvents();

                        }));

                        _purchaseTicketScreen.RequestPayment();
                    }
                    else if ((e.ResultState == NssIT.Kiosk.AppDecorator.Common.PaymentResult.Cancel) || (e.ResultState == NssIT.Kiosk.AppDecorator.Common.PaymentResult.Fail))
                    {
                        _purchaseTicketScreen.Dispatcher.Invoke(new Action(() =>
                        {
                            _bTnGPaymentPage.ClearEvents();
                            _frmSubFrame.Content = null;
                            _frmSubFrame.NavigationService.RemoveBackEntry();
                            _frmSubFrame.NavigationService.Content = null;
                            _bdSubFrame.Visibility = Visibility.Collapsed;
                            
                        }));
                    }
                    else
                    {
                        //App.komlinkCardDetailScreen.RequestCancelTopUp();
                       //App.NetClientSvc.BTnGService
                    }
                }
                catch (ThreadAbortException) { }
                catch (Exception ex)
                {
                    //App.ShowDebugMsg($@"{ex.Message}; EX02; pgKomuter_BTnGPayment.OnEndPaymentThreadWorking");
                    //App.Log.LogError(_logChannel, "-", ex, "EX02", "pgKomuter_BTnGPayment.OnEndPaymentThreadWorking");
                }
                finally
                {
                    _endPaymentThreadWorker = null;
                }
            }

            void PrintErrorThreadWorking()
            {
                try
                {
                //    _paymentPage.PrintTicketError2(_bookingNo);
                //    App.ShowDebugMsg("Print Sales Receipt on Fail Completed Transaction ..; pgKomuter_BTnGPayment.OnEndPaymentThreadWorking");
                }
                catch (ThreadAbortException)
                {
                }
                catch (Exception ex)
                {
                    //App.ShowDebugMsg($@"{ex.Message}; EX02; pgKomuter_BTnGPayment.PrintErrorThreadWorking");
                    //App.Log.LogError(_logChannel, "-", ex, "EX03", "pgKomuter_BTnGPayment.PrintErrorThreadWorking");
                }
            }
        }
    }
}
