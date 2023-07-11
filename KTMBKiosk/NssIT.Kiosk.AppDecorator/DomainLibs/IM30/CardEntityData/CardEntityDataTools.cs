using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.IM30.CardEntityData
{
    public class CardEntityDataTools
    {
        public static CardTypeEn CheckCardType(IM30DataModel orgData, out bool isSuccessData)
        {
            isSuccessData = false;

            if ((ResponseCodeDef.IsEqualResponse(orgData.ResponseCode, ResponseCodeDef.Approved) == true))
            {
                isSuccessData = true;
            }

            IM30FieldElementModel cdC2 = (from fe in orgData.FieldElementCollection
                                          where FieldTypeDef.IsEqualType(fe.FieldTypeCode, FieldTypeDef.CardToken)
                                          select fe).FirstOrDefault();

            if (cdC2 != null)
            {
                return CardTypeEn.CreditCard;
            }

            IM30FieldElementModel cdT4 = (from fe in orgData.FieldElementCollection
                                          where FieldTypeDef.IsEqualType(fe.FieldTypeCode, FieldTypeDef.TnGAccountCode)
                                          select fe).FirstOrDefault();

            if (cdT4 != null)
            {
                return CardTypeEn.TouchNGo;
            }

            IM30FieldElementModel cdH3 = (from fe in orgData.FieldElementCollection
                                          where FieldTypeDef.IsEqualType(fe.FieldTypeCode, FieldTypeDef.TnGEntryOperatorID)
                                          select fe).FirstOrDefault();

            if (cdH3 != null)
            {
                return CardTypeEn.TouchNGo;
            }

            return CardTypeEn.Unknown;
        }

        public static bool CheckIsSettlementData(IM30DataModel orgData, out bool isSuccessData)
        {
            isSuccessData = false;

            bool isValid = false;
            if ((TransactionCodeDef.IsEqualTrans(orgData.TransactionCode, TransactionCodeDef.Settlement) == true))
            {
                isValid = true;
            }

            if ((ResponseCodeDef.IsEqualResponse(orgData.ResponseCode, ResponseCodeDef.Approved) == true))
            {
                isSuccessData = true;
            }
            return isValid;
        }
    }
}
