using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.IM30.TransParams
{
    public interface I2ndCardCommandParam
    {
        TransactionTypeEn TransactionType { get; }
        string PosTransId { get; }
    }
}
