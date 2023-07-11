using NssIT.Kiosk.Device.Nippon.NP3611BD.OrgAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Device.Nippon.NP3611BD.AccessSDK.Command.Working
{
    [Serializable]
    public class AutoCreatePrinterAx<P, A> : INP3611Command<P, A>
        where P: AutoCreatePrinterParameter
        where A: AutoCreatePrinterEcho
    {
        public string ModelTag { get; } = "NssIT.Kiosk.Device.Nippon.NP3611BD.AccessSDK.Command.Working.AutoCreatePrinterAccess";
        public Guid CommandId { get; } = Guid.NewGuid();
        public CommandStatus ExecStatus { get; } = new CommandStatus();
        public P Parameter { get; } 
        public A SuccessEcho { get; private set; } 

        public AutoCreatePrinterAx(bool pCheckPrinterPaperLowState)
        {
            Parameter = (P)(new AutoCreatePrinterParameter(pCheckPrinterPaperLowState));
        }

        public void StartAccessSDKCommand(NP3611BDPrinter printer)
        {
            ExecStatus.StartExecution();

            Exception err = null; 
            try
            {
                string prtName = NP3611BDPrinter.AutoDetectDefaultPrinterName();

                if (string.IsNullOrWhiteSpace(prtName) == false)
                {
                    NP3611BDPrinter prtApi = NP3611BDPrinter.GetPrintQueue(prtName, Parameter.CheckPrinterPaperLowState);

                    if (prtApi is null)
                    {
                        err = new Exception("Printer not found but printer name detected.");
                    }
                    else
                    {
                        // .. success Exection
                        SuccessEcho = (A)(new AutoCreatePrinterEcho(prtName, prtApi));
                        ExecStatus.EndExecution(isCommandEndSuccessFul: true, error: null);
                    }
                }
                else
                {
                    err = new Exception("No default printer name has not found");
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
                        new Exception("Unknown error when auto detect printer");
                    }

                    ExecStatus.EndExecution(isCommandEndSuccessFul: false, err);
                }
            }
        }
    }
}
