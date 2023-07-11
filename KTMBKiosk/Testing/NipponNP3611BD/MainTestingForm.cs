using NssIT.Kiosk.Device.Nippon.NP3611BD.AccessSDK;
using NssIT.Kiosk.Device.Nippon.NP3611BD.AccessSDK.Command;
using NssIT.Kiosk.Device.Nippon.NP3611BD.AccessSDK.Command.Working;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NipponNP3611BD
{
    public partial class MainTestingForm : Form
    {
        private LibShowMessageWindow.MessageWindow _msg = new LibShowMessageWindow.MessageWindow();
        private NP3611BDPrinterAccess _printerAccess = null;
        private string _initLog = "";

        public MainTestingForm()
        {
            InitializeComponent();

            CollectAllPrinter(out _initLog);
        }

        private void MainTestingForm_Load(object sender, EventArgs e)
        {
            _msg.ShowMessage(_initLog);
        }

        public NP3611BDPrinterAccess PrinterAccess
        {
            get
            {
                if (_printerAccess != null)
                    return _printerAccess;

                //if (string.IsNullOrWhiteSpace(txtPrinterName.Text) == false)
                //{
                    bool toCheckPrinterPaperLowState = true;

                    if (chkNoPaperLow.Checked == true)
                    {
                        toCheckPrinterPaperLowState = false;
                    }

                    _printerAccess = NP3611BDPrinterAccess.GetPrinterAccess(txtPrinterName.Text, toCheckPrinterPaperLowState);

                    DateTime endTime = DateTime.Now.AddSeconds(30);
                    while(endTime.Ticks > DateTime.Now.Ticks)
                    {
                        if (_printerAccess.IsApiCreatedSuccessfully.HasValue == false)
                        {
                            Thread.Sleep(1000);
                        }
                        else
                            break;
                    }

                    if ((_printerAccess?.IsApiCreatedSuccessfully.HasValue == true) && _printerAccess.IsApiCreatedSuccessfully.Value == false)
                    {
                        Exception ex2 = _printerAccess.ApiError;

                        if (ex2 is null)
                            ex2 = new Exception("Unknown printer access creation error");

                        _printerAccess = null;

                        throw ex2;
                    }
                    else if (_printerAccess?.IsApiCreatedSuccessfully.HasValue == false)
                    {
                        Exception ex2 = new Exception("Timeout; Unable to create printer API on application initialization");

                        _printerAccess = null;

                        throw ex2;
                    }
                    else if (_printerAccess is null)
                    {
                        Exception ex2 = new Exception("Timeout; Unable to create printer access");

                        throw ex2;
                    }

                    return _printerAccess;
                //}
                //else
                //    throw new Exception("Printer Name not found/set.");
            }
        }

        private void CollectAllPrinter(out string logMsg)
        {
            logMsg = "\r\n";
            try
            {
                PrintServer psvr = new PrintServer();
                PrintQueueCollection pqColl = psvr.GetPrintQueues();

                cboPrinters.Items.Clear();

                foreach (PrintQueue pq3 in pqColl)
                {
                    if (pq3.QueueDriver?.Name is string drvStr)
                        logMsg += $@"Prt : {pq3.FullName} - Drv.Str : {drvStr}{"\r\n"}";
                    else
                        logMsg += $@"Prt : {pq3.FullName} - Drv.Str : NO DRV. Found{"\r\n"}";

                    if (string.IsNullOrWhiteSpace(pq3.FullName) == false)
                    {
                        cboPrinters.Items.Add(pq3.FullName.Trim());

                        PrinterSettings ps = new PrinterSettings();
                        ps.PrinterName = pq3.FullName;
                    }
                }

                cboPrinters.Items.Add("Unknown printer XXXXX");
            }
            catch (Exception ex)
            {
                _msg.ShowMessage(ex.ToString());
            }
        }

        private void btnChkPrinter_Click(object sender, EventArgs e)
        {
            try
            {
                //if (string.IsNullOrWhiteSpace(txtPrinterName.Text) == false)
                //{
                _msg.ShowMessage($@"Selected Printer Name : {txtPrinterName.Text}");

                ReadPrinterStatusAx<NullAccessParameter, ReadPrinterStatusEcho> comm 
                    = new ReadPrinterStatusAx<NullAccessParameter, ReadPrinterStatusEcho>();

                ReadPrinterStatusAx<NullAccessParameter, ReadPrinterStatusEcho> answer 
                    = (ReadPrinterStatusAx<NullAccessParameter, ReadPrinterStatusEcho>)PrinterAccess.ExecCommand(comm, waitDelaySec: 20);

                ReadPrinterStatusEcho ans = answer.SuccessEcho;

                _msg.ShowMessage($@"{"\r\n"}Is Printer Error : {ans.IsPrinterError} ; {"\r\n"}Is Printer Warning : {ans.IsPrinterWarning}; {"\r\n"}StatusDescription : {ans.StatusDescription}; {"\r\n"}LocalStateDesc : {ans.LocalStatusDescription}{"\r\n"}");
                //}
                //else
                //{
                //    _msg.ShowMessage($@"No selected Printer");
                //}

                if (string.IsNullOrWhiteSpace(txtPrinterName.Text) == true)
                {
                    txtPrinterName.Text = PrinterAccess.FinalizedPrinterName ?? "";
                }

            }
            catch (Exception ex)
            {
                _msg.ShowMessage(ex.ToString());
            }

            System.Windows.Forms.Application.DoEvents();
        }

        private string GetSelectedPrinterName()
        {
            if (cboPrinters.SelectedItem is string printerName)
                return printerName;
            else
                return null;
        }

        private void btnAutoDetectPrinter_Click(object sender, EventArgs e)
        {
            try
            {
                //string prtName = NP3611BDPrinter.AutoDetectDefaultPrinterName();

                //if (string.IsNullOrWhiteSpace(prtName) == false)
                //{
                //    txtPrinterName.Text = prtName;
                //}
                //else
                //{
                //    _msg.ShowMessage("Printer not found");
                //}
            }
            catch (Exception ex)
            {
                _msg.ShowMessage(ex.ToString());
            }
        }

        private void cboPrinters_SelectedIndexChanged(object sender, EventArgs e)
        {
            
            try
            {
                string prtName = GetSelectedPrinterName();

                txtPrinterName.Text = prtName ?? "";
            }
            catch (Exception ex)
            {
                _msg.ShowMessage(ex.ToString());
            }
        }

        private Thread _checkPrintingThread = null;
        private void StartCheckingPrintingStates(int checkingPeriodSec)
        {
            //DateTime endCheckingTime = DateTime.Now.AddSeconds(checkingPeriodSec);

            //bool? isPrintingState = null;

            //if (Printer == null)
            //    throw new Exception("Printer not found/set.");

            //lblPrintingTracking.Text = "0";
            //System.Windows.Forms.Application.DoEvents();

            //do
            //{
            //    bool resultState = Printer.IsPrinterPrinting;

            //    if ((isPrintingState.HasValue && isPrintingState.Value != resultState)
            //        ||
            //        (isPrintingState.HasValue == false)
            //        )
            //    {
            //        _msg.ShowMessage($@"Printing Status : {resultState}");
            //    }
            //    isPrintingState = resultState;

            //    lblPrintingTracking.Text = endCheckingTime.Subtract(DateTime.Now).TotalSeconds.ToString();
            //    System.Windows.Forms.Application.DoEvents();

            //    Thread.Sleep(5);
            //} while (endCheckingTime.Ticks > DateTime.Now.Ticks);

            //_msg.ShowMessage($@"Printing end Tracking");
        }

        private void btnPrintingTracking_Click(object sender, EventArgs e)
        {
            try
            {
                StartCheckingPrintingStates(30);
            }
            catch (Exception ex)
            {
                _msg.ShowMessage(ex.ToString());
            }
        }

        private void MainTestingForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                _printerAccess?.Dispose();
            }
            catch (Exception ex)
            {
                string em = ex.Message;
                string em2 = "-";
            }
        }
    }
}