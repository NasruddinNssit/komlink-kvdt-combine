using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kvdt_kiosk.Reports
{
    public class ReportDataQuery
    {
        private const string LogChannel = "ViewPage";

        public static DsMelakaCentralErrorTicketMessage GetTicketErrorDataSet(string transactionNo, string terminalVerticalLogoPath)
        {
            CultureInfo provider = CultureInfo.InvariantCulture;
            try
            {
                DsMelakaCentralErrorTicketMessage ds = new DsMelakaCentralErrorTicketMessage();
                DsMelakaCentralErrorTicketMessage.ErrorInfoDataTable dt = (DsMelakaCentralErrorTicketMessage.ErrorInfoDataTable)ds.Tables["ErrorInfo"];
                dt.AcceptChanges();
                ds.AcceptChanges();

                DsMelakaCentralErrorTicketMessage.ErrorInfoRow rw = dt.NewErrorInfoRow();
                dt.Rows.Add(rw);

                rw.TerminalLogoPath = terminalVerticalLogoPath;
                rw.TransactionNo = transactionNo;
                rw.TimeStr = DateTime.Now.ToString("dd.MM.yyyy hh:mm:ss tt");
                rw.ErrorMsg = $@"Error encountered When Ticket Printing;";

                rw.AcceptChanges();
                dt.AcceptChanges();
                ds.AcceptChanges();
                return ds;
            }
            catch (Exception ex)
            {
               Log.Error(LogChannel, "-", ex, "EX01", "ReportDataQuery.GetTicketErrorDataSet");
                return new DsMelakaCentralErrorTicketMessage();
            }
        }
    }
}
