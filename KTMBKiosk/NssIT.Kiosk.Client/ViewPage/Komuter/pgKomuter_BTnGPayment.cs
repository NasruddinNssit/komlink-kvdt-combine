using NssIT.Kiosk.AppDecorator.Common;
using NssIT.Kiosk.AppDecorator.Common.AppService;
using NssIT.Kiosk.Client.Base;
using NssIT.Kiosk.Client.ViewPage.Payment;
using NssIT.Kiosk.Log.DB;
using NssIT.Kiosk.Tools.ThreadMonitor;
using NssIT.Train.Kiosk.Common.Constants;
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
using static NssIT.Kiosk.Client.ViewPage.Komuter.pgSalesPanel;

namespace NssIT.Kiosk.Client.ViewPage.Komuter
{
    /// <summary>
    /// ClassCode:EXIT80.06
    /// </summary>
    public class pgKomuter_BTnGPayment
    {
        private string _logChannel = "Payment";

        private pgBTnGPayment _bTnGPaymentPage = null;
        private Frame _frmPrinting = null;
        private Frame _frmSubFrame = null;
        private ResourceDictionary _langRec = null;
        private LanguageCode _language = LanguageCode.English;
        private Border _bdSubFrame = null;
        private pgPrintTicket2 _printTicketPage = null;
        private pgKomuter _paymentPage = null;
        private Frame _frmMap = null;
        private Frame _frmSales = null;
        private pgMap _mapPage = null;

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
        private ActivatePaymentSelectionDelg _activatePaymentSelectionDelgHandle = null;

        //BdSubFrame//FrmSubFrame
        public pgKomuter_BTnGPayment(pgKomuter paymentPage, Frame frmSubFrame, Border bdSubFrame, Frame frmPrinting, pgPrintTicket2 printTicketPage, 
            Frame frmMap, Frame frmSales, pgMap mapPage, 
            ActivatePaymentSelectionDelg activatePaymentSelectionDelgHandle)
        {
            _frmSubFrame = frmSubFrame;
            _bdSubFrame = bdSubFrame;
            _frmPrinting = frmPrinting;
            _paymentPage = paymentPage;
            _printTicketPage = printTicketPage;
            _frmMap = frmMap;
            _frmSales = frmSales;
            _mapPage = mapPage;

            _activatePaymentSelectionDelgHandle = activatePaymentSelectionDelgHandle;
            _bTnGPaymentPage = pgBTnGPayment.GetBTnGPage();
        }

        private RunThreadMan _printingThreadWorker = null;
        private RunThreadMan _endPaymentThreadWorker = null;
        /// <summary>
        /// FuncCode:EXIT80.0605
        /// </summary>
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
                App.Log.LogError(_logChannel, "-", ex, "EX02", "pgKomuter_BTnGPayment._cashPaymentPage_OnEndPayment");
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

                    if (e.ResultState == AppDecorator.Common.PaymentResult.Success)
                    {
                        App.BookingTimeoutMan.ResetCounter();

                        // Complete Transaction Then Print Ticket
                        _paymentPage.Dispatcher.Invoke(new Action(() =>
                        {
                            _frmMap.Content = null;
                            _frmMap.NavigationService.RemoveBackEntry();

                            _frmSales.Content = null;
                            _frmSales.NavigationService.RemoveBackEntry();
                            System.Windows.Forms.Application.DoEvents();

                            _frmSubFrame.Content = null;
                            _frmSubFrame.NavigationService.RemoveBackEntry();
                            _frmSubFrame.NavigationService.Content = null;
                            System.Windows.Forms.Application.DoEvents();

                            _printTicketPage.InitSuccessPaymentCompleted(_bookingNo, _language);

                            _bTnGPaymentPage.ClearEvents();
                            //FrmGoPay.NavigationService.Navigate(_printTicketPage);
                            _frmSubFrame.Content = null;
                            _frmSubFrame.NavigationService.RemoveBackEntry();
                            _frmSubFrame.NavigationService.Content = null;
                            _bdSubFrame.Visibility = Visibility.Collapsed;

                            //_frmPrinting.Content = null;
                            //_frmPrinting.NavigationService.RemoveBackEntry();
                            //System.Windows.Forms.Application.DoEvents();

                            _frmPrinting.NavigationService.Navigate(_printTicketPage);
                            //App.MainScreenControl.ExecMenu.HideMiniNavigator();
                            System.Windows.Forms.Application.DoEvents();
                        }));

                        App.NetClientSvc.SalesService.SubmitKomuterBookingPayment(
                            _bookingNo, _seatBookingId, _currency, _amount, _paymentMethod, LastBTnGSaleTransNo, out bool isServerResponded);

                        if (isServerResponded == false)
                        {
                            _printTicketPage.UpdateCompleteTransactionState(isTransactionSuccess: false, _language);

                            string probMsg = "Local Server not responding (EXIT80.069A.X01)";
                            probMsg = $@"{probMsg}; Transaction No.:{_bookingNo}";

                            App.Log.LogError(_logChannel, _bookingNo, new Exception(probMsg), "EX01", "pgKomuter_BTnGPayment.OnEndPaymentThreadWorking");

                            _printingThreadWorker = new RunThreadMan(new ThreadStart(PrintErrorThreadWorking), "pgKomuter_BTnGPayment.OnEndPaymentThreadWorking", 60, _logChannel);
                        }

                        /////string tt1 = "..";
                    }
                    else if ((e.ResultState == AppDecorator.Common.PaymentResult.Cancel) || (e.ResultState == AppDecorator.Common.PaymentResult.Fail))
                    {
                        App.TimeoutManager.ResetTimeout();

                        _paymentPage.Dispatcher.Invoke(new Action(() =>
                        {
                            _bTnGPaymentPage.ClearEvents();
                            _frmSubFrame.Content = null;
                            _frmSubFrame.NavigationService.RemoveBackEntry();
                            _frmSubFrame.NavigationService.Content = null;

                            _mapPage.ActivateReset();
                            _bdSubFrame.Visibility = Visibility.Collapsed;
                            System.Windows.Forms.Application.DoEvents();
                            _activatePaymentSelectionDelgHandle();
                        }));
                    }
                    else
                    {
                        // Below used to handle result like ..
                        //------------------------------------------
                        // AppDecorator.Common.PaymentResult.Timeout
                        // AppDecorator.Common.PaymentResult.Unknown

                        //if (_isPauseOnPrinting == false)

                        App.Log.LogError(_logChannel, $@"{_bookingNo}", new WithDataException("Abnormal workflow (Timeout / Unknown)", new Exception("Abnormal workflow"), e), "X11", "pgKomuter_BTnGPayment.OnEndPaymentThreadWorking");
                        App.MainScreenControl.ShowWelcome();
                    }
                }
                catch (ThreadAbortException) { }
                catch (Exception ex)
                {
                    App.ShowDebugMsg($@"{ex.Message}; EX02; pgKomuter_BTnGPayment.OnEndPaymentThreadWorking");
                    App.Log.LogError(_logChannel, "-", ex, "EX02", "pgKomuter_BTnGPayment.OnEndPaymentThreadWorking");
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
                    App.ShowDebugMsg("Print Sales Receipt on Fail Completed Transaction ..; pgKomuter_BTnGPayment.OnEndPaymentThreadWorking");
                }
                catch (ThreadAbortException)
                {
                }
                catch (Exception ex)
                {
                    App.ShowDebugMsg($@"{ex.Message}; EX02; pgKomuter_BTnGPayment.PrintErrorThreadWorking");
                    App.Log.LogError(_logChannel, "-", ex, "EX03", "pgKomuter_BTnGPayment.PrintErrorThreadWorking");
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

                _frmSubFrame.Content = null;
                _frmSubFrame.NavigationService.RemoveBackEntry();
                _frmSubFrame.NavigationService.Content = null;
                System.Windows.Forms.Application.DoEvents();

                App.CurrentTransStage = TicketTransactionStage.Komuter;
                _frmSubFrame.NavigationService.Navigate(_bTnGPaymentPage);
                _bdSubFrame.Visibility = Visibility.Visible;

                System.Windows.Forms.Application.DoEvents();
            }
            catch (Exception ex)
            {
                App.Log.LogError(_logChannel, "-", ex, "EX01", "pgKomuter_BTnGPayment.StartBTnGPayment");
            }
        }

        public void BTnGShowPaymentInfo(IKioskMsg kioskMsg)
        {
            _bTnGPaymentPage.BTnGShowPaymentInfo(kioskMsg);
        }
    }
}
