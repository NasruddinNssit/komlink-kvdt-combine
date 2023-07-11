using NssIT.Kiosk.Device.Nippon.NP3611BD.OrgAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Device.Nippon.NP3611BD.AccessSDK.Command.Working
{
    [Serializable]
    public class ManualCreatePrinterEcho : INP3611CommandSuccessAnswer
    {
        public string ModelTag => "NssIT.Kiosk.Device.Nippon.NP3611BD.AccessSDK.Command.Working.ManualCreatePrinterAccessAnswer";

        public bool ExistingPrinterHasAlreadyExist { get; private set; } = false;
        public NP3611BDPrinter PrinterApi { get; private set; }

        /// <summary>
        /// </summary>
        /// <param name="isExistingPrinterHasAlreadyExist"></param>
        /// <param name="printerApi">Cannot be null</param>
        public ManualCreatePrinterEcho(bool isExistingPrinterHasAlreadyExist, NP3611BDPrinter printerApi)
        {
            if (printerApi is null)
                throw new Exception("ManualCreatePrinterAccessAnswer found invalid parameter with null printer api");

            ExistingPrinterHasAlreadyExist = isExistingPrinterHasAlreadyExist;
            PrinterApi = printerApi;
        }

        public void DetachPrinterApi()
        {
            PrinterApi = null;
        }
    }
}
