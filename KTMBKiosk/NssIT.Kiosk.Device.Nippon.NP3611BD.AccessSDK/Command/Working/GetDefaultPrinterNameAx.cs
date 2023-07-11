using NssIT.Kiosk.Device.Nippon.NP3611BD.OrgAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Device.Nippon.NP3611BD.AccessSDK.Command.Working
{
    [Serializable]
    public class GetDefaultPrinterNameAx<P, A> : INP3611Command<P, A>
        where P: NullAccessParameter
        where A: GetDefaultPrinterNameEcho
    {
        public string ModelTag => "NssIT.Kiosk.Device.Nippon.NP3611BD.AccessSDK.Command.Working.DetectDefaultPrinterNameAccess";
        public Guid CommandId { get; } = Guid.NewGuid();
        public CommandStatus ExecStatus { get; } = new CommandStatus();
        public P Parameter { get; private set; } = null;
        public A SuccessEcho { get; private set; } = null;

        public GetDefaultPrinterNameAx()
        {  }

        public void StartAccessSDKCommand(NP3611BDPrinter printer)
        {
            ExecStatus.StartExecution();

            Exception err = null;
            try
            {
                string prtName = NP3611BDPrinter.AutoDetectDefaultPrinterName();

                if (string.IsNullOrWhiteSpace(prtName) == false)
                {
                    // ..success Exection
                    SuccessEcho = (A)(new GetDefaultPrinterNameEcho(prtName));
                    ExecStatus.EndExecution(isCommandEndSuccessFul: true, error: null);
                }
                else
                {
                    err = new Exception("Default Printer Name not found");
                }
            }
            catch (Exception ex)
            {
                err = ex;
            }
            finally
            {
                if (ExecStatus.IsCommandEnd == false)
                {
                    if (err is null)
                    {
                        new Exception("Unknown Error when detect default printer name.");
                    }

                    ExecStatus.EndExecution(isCommandEndSuccessFul: false, error: new Exception("Unknown Error when detect default printer name."));
                }
            }
        }
    }
}
