using Komlink.Reports;
using Komlink.Reports.KTMB;
using Microsoft.Reporting.WinForms;
using NssIT.Kiosk.Client.Reports;
using NssIT.Kiosk.Client.Reports.KTMB;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.Base
{
    public class RDLCLibraryStarter
    {
        public static bool LibraryIsReady { get; private set; } = true;

        private RDLCLibraryStarter()
        { }

        private string CreditCardReceiptReportSourceName
        {
            get
            {
                return "RPTCreditCardReceipt";
            }
        }

        public static void InitLibrary()
        {
            RDLCLibraryStarter starter = new RDLCLibraryStarter();
            Thread threadWorker = new Thread(starter.InitRDLCThreadWorking);
            threadWorker.IsBackground = true;
            LibraryIsReady = false;
            threadWorker.Start();
        }

        private void InitRDLCThreadWorking()
        {
            try
            {
                DSCreditCardReceipt.DSCreditCardReceiptDataTable dt = new DSCreditCardReceipt.DSCreditCardReceiptDataTable();
                DSCreditCardReceipt.DSCreditCardReceiptRow rw = dt.NewDSCreditCardReceiptRow();
                rw.AID = "X0001";
                rw.Approval = "X0001";
                rw.BatchNo = "X0001";
                rw.CardHolder_Name = "X0001";
                rw.CardNo = "X0001";
                rw.CardType = "X0001";
                rw.ExpDate = "X0001";
                rw.HostNo = "X0001";
                rw.Merchant_Id = "X0001";
                rw.RRN = "X0001";
                rw.Status = "X0001";
                rw.TC = "X0001";
                rw.Terminal_Id = "X0001";
                rw.TransactionType = "SALE";
                rw.TransactionDate = "X0001";
                rw.TransactionTrace = "X0001";
                rw.AmountString = "X0001";
                rw.MachineId = "X0001";
                rw.RefNumber = "X0001";
                dt.Rows.Add(rw);
                dt.AcceptChanges();

                //LocalReport receiptRep = RdlcImageRendering.CreateLocalReport($@"{App.ExecutionFolderPath}\Reports\KTMB\{CreditCardReceiptReportSourceName}.rdlc",
                //        new ReportDataSource[] { new ReportDataSource("DataSet1", (DataTable)dt) });
                //ReportImageSize receiptSize = new ReportImageSize(3.2M, 8.2M, 0, 0, 0, 0, ReportImageSizeUnitMeasurement.Inch);
                //Stream[] streamReceiptList = RdlcImageRendering.Export(receiptRep, receiptSize);

                ////////Thread.Sleep(1000 * 30);

                string tt1 = "Done";
            }
            catch (Exception ex)
            {
                string tt1 = ex.Message;
            }
            finally
            {
                LibraryIsReady = true;
            }
        }
    }
}
