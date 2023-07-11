using NssIT.Kiosk.Device.Nippon.NP3611BD.OrgAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Device.Nippon.NP3611BD.AccessSDK.Command.Working
{
    [Serializable]
    public class ManualCreatePrinterAx<P, A> : INP3611Command<P, A>
        where P : ManualCreatePrinterParameter
        where A : ManualCreatePrinterEcho
    {
        public string ModelTag => "NssIT.Kiosk.Device.Nippon.NP3611BD.AccessSDK.Command.Working.ManualCreatePrinterAccess";
        public Guid CommandId { get; } = Guid.NewGuid();
        public CommandStatus ExecStatus { get; } = new CommandStatus();

        public P Parameter { get; private set; } = null;
        public A SuccessEcho { get; private set; } = null;

        public ManualCreatePrinterAx(string printerName, bool checkPrinterPaperLowState)
        {
            Parameter = (P)(new ManualCreatePrinterParameter(printerName, checkPrinterPaperLowState));
        }

        public void StartAccessSDKCommand(NP3611BDPrinter printer)
        {
            ExecStatus.StartExecution();

            Exception err = null;
            try
            {
                if (NP3611BDPrinter.IsInstanceAlreadyCreated)
                {
                    SuccessEcho = (A)(new ManualCreatePrinterEcho(isExistingPrinterHasAlreadyExist: true, NP3611BDPrinter.Instance));
                    ExecStatus.EndExecution(isCommandEndSuccessFul: true, error: null);
                }
                else
                {
                    NP3611BDPrinter prtApi = NP3611BDPrinter.GetPrintQueue(Parameter.PrinterName, Parameter.CheckPrinterPaperLowState);

                    if (prtApi is null)
                    {
                        // This error will never happen as the NP3611BDPrinter.GetPrintQueue already throw an exception.
                        err = new Exception("Printer not found refer to parameter; Unknown Error");
                    }
                    else
                    {
                        // .. success Exection
                        SuccessEcho = (A)(new ManualCreatePrinterEcho(isExistingPrinterHasAlreadyExist: false, prtApi));
                        ExecStatus.EndExecution(isCommandEndSuccessFul: true, error: null);
                    }
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
                        err = new Exception("Unknown error when create printer manually");
                    }

                    ExecStatus.EndExecution(isCommandEndSuccessFul: false, err);
                }
            }
        }
    }
}
