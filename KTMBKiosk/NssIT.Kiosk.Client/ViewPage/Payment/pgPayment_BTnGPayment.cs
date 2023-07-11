using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using NssIT.Kiosk.Client.Base;
using NssIT.Kiosk.Tools.ThreadMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace NssIT.Kiosk.Client.ViewPage.Payment
{
    /// <summary>
    /// ClassCode:EXIT80.08
    /// </summary>
    public class pgPayment_BTnGPayment
    {
        private string _logChannel = "Payment";

        private pgBTnGPayment _bTnGPaymentPage = null;
        private Frame _frmPayInfo = null;
        private Frame _frmGoPay = null;
        private ResourceDictionary _langRec = null;
        private LanguageCode _language = LanguageCode.English;
        private Border _bdPay = null;
        private pgPrintTicket2 _printTicketPage = null;
        private pgPayment _paymentPage = null;

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

        private ResetCurrentPaymentTypeDelg _resetCurrentPaymentTypeDelgHandle = null;

        public pgPayment_BTnGPayment(pgPayment paymentPage, Frame frmGoPay, Border bdPay, Frame frmPayInfo, pgPrintTicket2 printTicketPage,
            ResetCurrentPaymentTypeDelg resetCurrentPaymentTypeDelgHandle)
        {
            _frmGoPay = frmGoPay;
            _bdPay = bdPay;
            _frmPayInfo = frmPayInfo;
            _paymentPage = paymentPage;
            _printTicketPage = printTicketPage;

            _bTnGPaymentPage = pgBTnGPayment.GetBTnGPage();
            _resetCurrentPaymentTypeDelgHandle = resetCurrentPaymentTypeDelgHandle;
        }

        private RunThreadMan _printingThreadWorker = null;
        private RunThreadMan _endPaymentThreadWorker = null;
        private void _bTnGPaymentPage_OnEndPayment(object sender, EndOfPaymentEventArgs e)
        {
            try
            {
                LastBTnGSaleTransNo = e.BTnGSaleTransactionNo;

                App.Log.LogText(_logChannel, _bookingNo, e,
                    "A01", "pgPayment_BTnGPayment._bTnGPaymentPage_OnEndPayment", extraMsg: "MsgObj: EndOfPaymentEventArgs");

                if ((_endPaymentThreadWorker != null) && (_endPaymentThreadWorker.IsEnd == false))
                {
                    _endPaymentThreadWorker.AbortRequest(out _, 2000);
                }
                _endPaymentThreadWorker = new RunThreadMan(new ThreadStart(OnEndPaymentThreadWorking), "pgPayment_BTnGPayment._bTnGPaymentPage_OnEndPayment::ETS", 120, _logChannel);
            }
            catch (Exception ex)
            {
                App.Log.LogError(_logChannel, "-", ex, "EX02", "pgPayment._cashPaymentPage_OnEndPayment");
            }
            return;
            //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            /// <summary>
            /// FuncCode:EXIT80.088A
            /// </summary>
            void OnEndPaymentThreadWorking()
            {
                try
                {
                    App.ShowDebugMsg("_bTnGPaymentPage_OnEndPayment.. go to ticket printing");

                    if (e.ResultState == AppDecorator.Common.PaymentResult.Success)
                    {
                        App.BookingTimeoutMan.ResetCounter();

                        // Complete Transaction Then Print Ticket
                        _paymentPage.Dispatcher.Invoke(new Action(() =>
                        {
                            _frmPayInfo.Content = null;
                            _frmPayInfo.NavigationService.RemoveBackEntry();
                            System.Windows.Forms.Application.DoEvents();

                            _printTicketPage.InitSuccessPaymentCompleted(_bookingNo, _language);

                            _bTnGPaymentPage.ClearEvents();
                            //FrmGoPay.NavigationService.Navigate(_printTicketPage);
                            _frmGoPay.Content = null;
                            _frmGoPay.NavigationService.RemoveBackEntry();
                            _frmGoPay.NavigationService.Content = null;
                            _resetCurrentPaymentTypeDelgHandle();

                            _bdPay.Visibility = Visibility.Collapsed;

                            _frmPayInfo.NavigationService.Navigate(_printTicketPage);

                            //App.MainScreenControl.ExecMenu.HideMiniNavigator();
                            System.Windows.Forms.Application.DoEvents();
                        }));

                        // CYA-TEST.. Fail tesing .. Unremark below line when deploy.
                        App.NetClientSvc.SalesService.SubmitSalesPayment(_seatBookingId, _amount, _currency, e.BTnGSaleTransactionNo, e.PaymentMethod, out bool isServerResponded);

                        // CYA-TEST .. bool isServerResponded = false;
                        //----------------------------------------------------

                        if (isServerResponded == false)
                        {
                            _printTicketPage.UpdateCompleteTransactionState(isTransactionSuccess: false, language: _language);

                            string probMsg = "Local Server not responding (EXIT80.088A.X01)";
                            probMsg = $@"{probMsg}; Transaction No.:{_bookingNo}";

                            App.Log.LogError(_logChannel, _bookingNo, new Exception(probMsg), "EX01", "pgPayment_BTnGPayment.OnEndPaymentThreadWorking");

                            _printingThreadWorker = new RunThreadMan(new ThreadStart(PrintErrorThreadWorking), "pgPayment_BTnGPayment.OnEndPaymentThreadWorking(Fail Ticket Printing)", 60, _logChannel, isLogReq: true);
                        }
                    }
                    else if ((e.ResultState == AppDecorator.Common.PaymentResult.Cancel) || (e.ResultState == AppDecorator.Common.PaymentResult.Fail))
                    {
                        App.TimeoutManager.ResetTimeout();

                        _paymentPage.Dispatcher.Invoke(new Action(() =>
                        {
                            _bTnGPaymentPage.ClearEvents();
                            _frmGoPay.Content = null;
                            _frmGoPay.NavigationService.RemoveBackEntry();
                            _frmGoPay.NavigationService.Content = null;
                            _bdPay.Visibility = Visibility.Collapsed;
                            _resetCurrentPaymentTypeDelgHandle();

                            System.Windows.Forms.Application.DoEvents();
                        }));
                    }
                    else
                    {
                        App.BookingTimeoutMan.ResetCounter();

                        // Below used to handle result like ..
                        //------------------------------------------
                        // AppDecorator.Common.PaymentResult.Fail
                        // AppDecorator.Common.PaymentResult.Timeout
                        // AppDecorator.Common.PaymentResult.Unknown

                        App.NetClientSvc.SalesService.RequestSeatRelease(_bookingNo);
                        //if (isServerResponded == false)
                        //    App.MainScreenControl.Alert(detailMsg: "Local Server not responding (EXIT10000913)");

                        //if (_isPauseOnPrinting == false)
                        App.MainScreenControl.ShowWelcome();
                    }
                }
                catch (ThreadAbortException) { }
                catch (Exception ex)
                {
                    App.ShowDebugMsg($@"{ex.Message}; EX02;pgPayment.OnEndPaymentThreadWorking");
                    App.Log.LogError(_logChannel, "-", ex, "EX02", "pgPayment.OnEndPaymentThreadWorking");
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
                    _paymentPage.PrintTicketError2(_bookingNo);
                    App.ShowDebugMsg("Print Sales Receipt on Fail Completed Transaction ..; pgPayment.OnEndPaymentThreadWorking");

                    // App.MainScreenControl.Alert(detailMsg: "Local Server not responding (EXIT10000912)");
                }
                catch (ThreadAbortException)
                {
                    //PDFTools.KillAdobe("AcroRd32");
                }
                catch (Exception ex)
                {
                    App.ShowDebugMsg($@"{ex.Message}; EX02;pgPayment.PrintErrorThreadWorking");
                    App.Log.LogError(_logChannel, "-", ex, "EX03", "pgPayment.PrintErrorThreadWorking");
                }
            }
        }

        public void PageUnloaded()
        {
            _bTnGPaymentPage.ClearEvents();
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

                _frmGoPay.Content = null;
                _frmGoPay.NavigationService.RemoveBackEntry();
                _frmGoPay.NavigationService.Content = null;
                System.Windows.Forms.Application.DoEvents();

                App.CurrentTransStage = TicketTransactionStage.ETS;
                _frmGoPay.NavigationService.Navigate(_bTnGPaymentPage);
                _bdPay.Visibility = Visibility.Visible;

                System.Windows.Forms.Application.DoEvents();
            }
            catch (Exception ex)
            {
                App.Log.LogError(_logChannel, "-", ex, "EX01", "pgPayment.StartBTnGPayment");
            }
        }

        public void BTnGShowPaymentInfo(IKioskMsg kioskMsg)
        {
            _bTnGPaymentPage.BTnGShowPaymentInfo(kioskMsg);
        }
    }
}
