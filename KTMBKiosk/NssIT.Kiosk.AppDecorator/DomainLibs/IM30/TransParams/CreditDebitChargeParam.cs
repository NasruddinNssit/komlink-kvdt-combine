using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.IM30.TransParams
{
    //... send ICardTransParameters into IM30CardSale for any transaction .....
    public class CreditDebitChargeParam : I2ndCardCommandParam
    {
        public TransactionTypeEn TransactionType => TransactionTypeEn.CreditCard_2ndComm;
        public string PosTransId { get; private set; }
        public decimal Price { get; private set; }

        public CreditDebitChargeParam(string posTransId, decimal price)
        {
            if (string.IsNullOrWhiteSpace(posTransId))
                throw new Exception("-TransactionId not valid; Invalid Credit_Debit_Charge_Parameter~");

            else if (price <= 0)
                throw new Exception("-Price not valid; Invalid Credit_Debit_Charge_Parameter~");

            PosTransId = posTransId;
            Price = price;
        }
    }
}