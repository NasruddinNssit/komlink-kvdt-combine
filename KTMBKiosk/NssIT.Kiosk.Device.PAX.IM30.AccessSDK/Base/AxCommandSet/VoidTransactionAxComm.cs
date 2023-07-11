using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Device.PAX.IM30.AccessSDK.Base.AxCommandSet
{
    public class VoidTransactionAxComm : IIM30AxCommand
    {
        public string InvoiceNo { get; private set; }
        public string CardToken { get; private set; }
        public decimal VoidAmount { get; private set; }
        

        public VoidTransactionAxComm(string invoiceNo, decimal voidAmount, string cardToken)
        {
            InvoiceNo = invoiceNo;
            CardToken = cardToken;
            VoidAmount = voidAmount;
        }
    }
}
