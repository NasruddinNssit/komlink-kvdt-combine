using NssIT.Kiosk.Device.Nippon.NP3611BD.OrgAPI;
using NssIT.Kiosk.Log.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Device.Nippon.NP3611BD.AccessSDK.Command.Working
{
    [Serializable]
    public class ReadPrinterStatusAx<P, A> : INP3611Command<P, A> , IDisposable 
        where P : NullAccessParameter
        where A : ReadPrinterStatusEcho
    {
        private const string _logChannel = "PRINTER_NP3611BD_Access";
        private DbLog _log = null;

        public string ModelTag => "NssIT.Kiosk.Device.Nippon.NP3611BD.AccessSDK.Command.Working.ReadPrinterStatusAccess";
        public Guid CommandId { get; } = Guid.NewGuid();
        public CommandStatus ExecStatus { get; } = new CommandStatus();
        public P Parameter => null;
        public A SuccessEcho { get; private set; }

        public void Dispose()
        {
            SuccessEcho = null;
        }

        public DbLog Log
        {
            get
            {
                return _log ?? (_log = DbLog.GetDbLog());
            }
        }

        public void StartAccessSDKCommand(NP3611BDPrinter printer)
        {
            ExecStatus.StartExecution();

            Exception err = null;
            try
            {
                printer.GetPrinterStatus(out bool isPrinterError, out bool isPrinterWarning, out string statusDescription, out string locStateDesc);

                SuccessEcho = (A)(new ReadPrinterStatusEcho(isPrinterError, isPrinterWarning, statusDescription, locStateDesc));
                ExecStatus.EndExecution(isCommandEndSuccessFul: true, error: null);
            }
            catch (Exception ex)
            {
                err = new Exception("Error when read printer status", ex);
                Log?.LogError(_logChannel, "*", ex, "EX01", "ReadPrinterStatusAx.StartAccessSDKCommand");
            }
            finally
            {
                if (ExecStatus.IsCommandEnd == false)
                {
                    if (err is null)
                    {
                        new Exception("Unknown error when read printer status");
                    }

                    ExecStatus.EndExecution(isCommandEndSuccessFul: false, err);
                }
            }
        }
    }
}
