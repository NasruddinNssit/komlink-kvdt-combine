using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.IM30.CardEntityData.CreditDebit
{
    public class CreditDebitCardInfoResp :  ICardResponse
    {
        public string CardNumber { get; private set; }
        public string CardToken { get; private set; }

        /// <summary>
        /// This value is always masked with **** in actual transaction. Validation to this value is not necessary.
        /// </summary>
        public DateTime CardExpireDate { get; private set; } = DateTime.MaxValue;

        public bool IsDataFound { get; private set; } = false;
        public Exception DataError { get; private set; } = null;

        public CreditDebitCardInfoResp(IM30DataModel orgData)
        {
            try
            {
                if (orgData is null)
                    DataError = new Exception("-Invalid data when translate to Credit/Debit Card Info~");

                if ((DataError is null) && ((TransactionCodeDef.IsEqualTrans(orgData.TransactionCode, TransactionCodeDef.StartTransaction) == false)))
                {
                    DataError = new Exception("-Transaction Code not found when translate Credit/Debit Info~");
                }

                if ((DataError is null) && ((ResponseCodeDef.IsEqualResponse(orgData.ResponseCode, ResponseCodeDef.Approved) == false)))
                {
                    DataError = new Exception("-Fail to read Reader Credit/Debit Info~");
                }

                if ((DataError is null) && ((from fe in orgData.FieldElementCollection
                                             where FieldTypeDef.IsEqualType(fe.FieldTypeCode, FieldTypeDef.CardToken)
                                             select fe).FirstOrDefault() == null))
                {
                    DataError = new Exception("-Unrecognized Credit/Debit Card~Card Token not found");
                }

                if (DataError is null)
                {
                    IsDataFound = true;

                    string errMsg = "";

                    /////-----------------------------------------------------------------------------------------------------------
                    {
                        IM30FieldElementModel cd30 = (from fe in orgData.FieldElementCollection
                                                      where FieldTypeDef.IsEqualType(fe.FieldTypeCode, FieldTypeDef.PrimaryAccountNumber)
                                                      select fe).FirstOrDefault();

                        if ((cd30 is null) || (string.IsNullOrWhiteSpace(cd30.Data)))
                            errMsg += "Credit/Debit Card Number not found;";
                        else
                            CardNumber = cd30.Data.Trim();
                    }
                    /////-----------------------------------------------------------------------------------------------------------
                    {
                        IM30FieldElementModel cdC2 = (from fe in orgData.FieldElementCollection
                                                      where FieldTypeDef.IsEqualType(fe.FieldTypeCode, FieldTypeDef.CardToken)
                                                      select fe).FirstOrDefault();

                        if ((cdC2 is null) || (string.IsNullOrWhiteSpace(cdC2.Data)))
                            errMsg += "Credit/Debit Card Token not found;";
                        else
                            CardToken = cdC2.Data.Trim();
                    }
                    /////-----------------------------------------------------------------------------------------------------------
                    if (string.IsNullOrWhiteSpace(errMsg) == false)
                        DataError = new Exception(errMsg);
                }
            }
            catch (Exception ex)
            {
                DataError = ex;
            }
        }
    }
}