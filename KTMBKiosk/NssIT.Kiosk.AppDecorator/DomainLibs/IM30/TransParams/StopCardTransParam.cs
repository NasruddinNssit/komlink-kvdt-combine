using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.IM30.TransParams
{
    public class StopCardTransParam : I2ndCardCommandParam
    {
        public TransactionTypeEn TransactionType => TransactionTypeEn.StopTrans;
        public string PosTransId { get; private set; } = "StopTrans_" + Guid.NewGuid().ToString();
        
        public StopCardTransParam()
        {
            PosTransId = $@"StopCardTrans_{DateTime.Now:dd_HHmmss_fff}";
        }
    }
}