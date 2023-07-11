using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.Base;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.IM30.CardEntityData.CreditDebit
{
    public class CreditDebitVoidTransactionResp : ICardResponse
    {
        /// <summary>
        /// Importance
        /// </summary>
        public ResponseCodeEn ResponseResult { get; private set; } = ResponseCodeEn.Fail;
        /// <summary>
        /// Importance
        /// </summary>
        public decimal Amount { get; private set; } = 0.00M;
        /// <summary>
        /// Importance
        /// </summary>
        public string ApprovalCode { get; private set; } = "";
        /// <summary>
        /// Importance
        /// </summary>
        public string InvoiceNo { get; private set; } = "";
        /// <summary>
        /// Importance
        /// </summary>
        public string POSTransactionID { get; private set; } = "";

        /// <summary>
        /// Not Importance; Merchant Name and Address
        /// </summary>
        public string MerchantNameAddr { get; private set; } = "";
        /// <summary>
        /// Importance
        /// </summary>
        public string MerchantNumber { get; private set; } = "";

        /// <summary>
        /// Importance; Terminal ID; Terminal Identification Number
        /// </summary>
        public string TID { get; private set; } = "";
        /// <summary>
        /// Not Importance
        /// </summary>
        public string CardIssuerName { get; private set; } = "";
        /// <summary>
        /// Importance
        /// </summary>
        public string CardNumber { get; private set; } = "";

        ///// <summary>
        ///// Importance
        ///// </summary>
        public string CardToken { get; private set; } = "";

        /// <summary>
        /// Not Importance; This value is always masked with **** in actual transaction. Validation to this value is not necessary.
        /// </summary>
        public DateTime? ExpiryDate { get; private set; } = DateTime.MaxValue;
        /// <summary>
        /// Importance
        /// </summary>
        public string BatchNumber { get; private set; } = "";
        /// <summary>
        /// Importance
        /// </summary>
        public DateTime? TransactionDateTime { get; private set; } = DateTime.Now;

        /// <summary>
        /// Importance; Retrieval Reference Number
        /// </summary>
        public string RRN { get; private set; } = "";

        /// <summary>
        /// Not Importance; Same as Host No.
        /// </summary>
        public string CardIssuerID { get; private set; } = "";
        /// <summary>
        /// Not Importance
        /// </summary>
        public string CardHolderName { get; private set; } = "";

        ///// <summary>
        ///// Not Importance; Application ID
        ///// </summary>
        //public string AID { get; private set; } = "";
        ///// <summary>
        ///// Not Importance
        ///// </summary>
        ////public string ApplicationName { get; private set; } = "";
        ///// <summary>
        ///// Importance
        ///// </summary>
        //public string Cryptogram { get; private set; } = "";

        ///// <summary>
        ///// Not Importance; Transaction verification result
        ///// </summary>
        //public string TVR { get; private set; } = "";

        ///// <summary>
        ///// Not Importance; Card Verification Method
        ///// </summary>
        //public string CVM { get; private set; } = "";

        /// <summary>
        /// Not Importance; Merchant Copy CVM Description
        /// </summary>
        public string CVMCopyDesc { get; private set; } = "";

        /// <summary>
        /// Not Importance; Customer Copy CVM Description
        /// </summary>
        public string CustomerCopyCVM { get; private set; } = "";

        /// <summary>
        /// Importance; Standard Unique Trace Number 
        /// </summary>
        public string STAN { get; private set; } = "";

        /// <summary>
        /// Not Importance
        /// </summary>
        public PaymentIndicatorEn PaymentIndicator { get; private set; } = PaymentIndicatorEn.Unknown;

        /// <summary>
        /// Not Importance
        /// </summary>
        public string ResponseText { get; private set; } = "";

        public bool IsDataFound { get; private set; } = false;
        public Exception DataError { get; private set; } = null;

        public CreditDebitVoidTransactionResp(IM30DataModel orgData, bool isDataGetFromLastTransaction = false)
        {
            if (orgData is null)
                DataError = new Exception("-Invalid data when translate to Credit/Debit Card Response Info~");

            if ((DataError is null) && (isDataGetFromLastTransaction == false) && ((TransactionCodeDef.IsEqualTrans(orgData.TransactionCode, TransactionCodeDef.Void) == false)))
            {
                //if ((from fe in orgData.FieldElementCollection
                //     where FieldTypeDef.IsEqualType(fe.FieldTypeCode, FieldTypeDef.CardToken)
                //     select fe).FirstOrDefault() == null)
                //{
                DataError = new Exception("-Void Credit/Debit Card Transaction code has not found~");
                //}
            }
            else if (DataError is null)
            {
                /////-----------------------------------------------------------------------------------------------------------
                {
                    IM30FieldElementModel cd02 = (from fe in orgData.FieldElementCollection
                                                  where FieldTypeDef.IsEqualType(fe.FieldTypeCode, FieldTypeDef.ResponseText)
                                                  select fe).FirstOrDefault();

                    if ((cd02 is null) || (string.IsNullOrWhiteSpace(cd02.Data)))
                    {
                        //Not Importance .. errMsg += "Invalid Response Text;";
                    }
                    else
                        ResponseText = cd02.Data.Trim();
                }
            }

            if ((DataError is null) && ((ResponseCodeDef.IsEqualResponse(orgData.ResponseCode, ResponseCodeDef.Approved) == false)))
            {
                ResponseResult = ResponseCodeEn.Fail;
                DataError = new Exception($@"-Fail Credit/Debit card transaction~{orgData.ResponseCode}-{orgData.ResponseDesc}");
            }

            if (DataError is null)
            {
                IsDataFound = true;

                ResponseResult = ResponseCodeEn.Success;

                string errMsg = "";

                /////-----------------------------------------------------------------------------------------------------------
                {
                    IM30FieldElementModel cd40 = (from fe in orgData.FieldElementCollection
                                                  where FieldTypeDef.IsEqualType(fe.FieldTypeCode, FieldTypeDef.TransAmount)
                                                  select fe).FirstOrDefault();

                    if ((cd40 is null) || (string.IsNullOrWhiteSpace(cd40.Data)))
                        errMsg += "Transaction Amount not found;";

                    else if (long.TryParse(cd40.Data.Trim(), out long trnAmt))
                    {
                        Amount = ((decimal)trnAmt) / 100M;
                    }
                    else
                        errMsg += $@"-Invalid Trans. Amount~({cd40.Data})";
                }
                /////-----------------------------------------------------------------------------------------------------------
                {
                    IM30FieldElementModel cd01 = (from fe in orgData.FieldElementCollection
                                                  where FieldTypeDef.IsEqualType(fe.FieldTypeCode, FieldTypeDef.ApprovalCode)
                                                  select fe).FirstOrDefault();

                    if ((cd01 is null) || (string.IsNullOrWhiteSpace(cd01.Data)))
                        errMsg += "Approval code not found;";
                    else
                    {
                        ApprovalCode = cd01.Data.Trim();
                    }
                }
                /////-----------------------------------------------------------------------------------------------------------
                {
                    IM30FieldElementModel cd65 = (from fe in orgData.FieldElementCollection
                                                  where FieldTypeDef.IsEqualType(fe.FieldTypeCode, FieldTypeDef.InvoiceNo)
                                                  select fe).FirstOrDefault();

                    if ((cd65 is null) || (string.IsNullOrWhiteSpace(cd65.Data)))
                        errMsg += "Invalid Invoice No.;";
                    else
                        InvoiceNo = cd65.Data.Trim();
                }
                /////-----------------------------------------------------------------------------------------------------------
                {
                    IM30FieldElementModel cd77 = (from fe in orgData.FieldElementCollection
                                                  where FieldTypeDef.IsEqualType(fe.FieldTypeCode, FieldTypeDef.PosTransId)
                                                  select fe).FirstOrDefault();

                    if ((cd77 is null) || (string.IsNullOrWhiteSpace(cd77.Data)))
                        errMsg += "Invalid POS Transaction ID.;";
                    else
                        POSTransactionID = cd77.Data.Trim();
                }
                /////-----------------------------------------------------------------------------------------------------------
                {
                    IM30FieldElementModel cdD0 = (from fe in orgData.FieldElementCollection
                                                  where FieldTypeDef.IsEqualType(fe.FieldTypeCode, FieldTypeDef.MerchantNameAndAddress)
                                                  select fe).FirstOrDefault();

                    if ((cdD0 is null) || (string.IsNullOrWhiteSpace(cdD0.Data)))
                    {
                        //Not Importance .. errMsg += "Invalid Merchant Name And Address;";
                    }
                    else
                        MerchantNameAddr = cdD0.Data.Trim();
                }
                /////-----------------------------------------------------------------------------------------------------------
                {
                    IM30FieldElementModel cdD1 = (from fe in orgData.FieldElementCollection
                                                  where FieldTypeDef.IsEqualType(fe.FieldTypeCode, FieldTypeDef.MerchantNo)
                                                  select fe).FirstOrDefault();

                    if ((cdD1 is null) || (string.IsNullOrWhiteSpace(cdD1.Data)))
                    {
                        errMsg += "Invalid Merchant No.;";
                    }
                    else
                        MerchantNumber = cdD1.Data.Trim();
                }
                /////-----------------------------------------------------------------------------------------------------------
                {
                    IM30FieldElementModel cd16 = (from fe in orgData.FieldElementCollection
                                                  where FieldTypeDef.IsEqualType(fe.FieldTypeCode, FieldTypeDef.TID)
                                                  select fe).FirstOrDefault();

                    if ((cd16 is null) || (string.IsNullOrWhiteSpace(cd16.Data)))
                    {
                        errMsg += "Invalid Terminal Identification Number (TID).;";
                    }
                    else
                        TID = cd16.Data.Trim();
                }
                /////-----------------------------------------------------------------------------------------------------------
                {
                    IM30FieldElementModel cdD2 = (from fe in orgData.FieldElementCollection
                                                  where FieldTypeDef.IsEqualType(fe.FieldTypeCode, FieldTypeDef.CardIssuerName)
                                                  select fe).FirstOrDefault();

                    if ((cdD2 is null) || (string.IsNullOrWhiteSpace(cdD2.Data)))
                    {
                        //Not Importance .. errMsg += "Invalid Card Issuer Name.;";
                    }
                    else
                        CardIssuerName = cdD2.Data.Trim();
                }
                /////-----------------------------------------------------------------------------------------------------------
                {
                    IM30FieldElementModel cd30 = (from fe in orgData.FieldElementCollection
                                                  where FieldTypeDef.IsEqualType(fe.FieldTypeCode, FieldTypeDef.PrimaryAccountNumber)
                                                  select fe).FirstOrDefault();

                    if ((cd30 is null) || (string.IsNullOrWhiteSpace(cd30.Data)))
                    {
                        errMsg += "Credit/Debit Card Number not found;";
                    }
                    else
                        CardNumber = cd30.Data.Trim();
                }
                /////-----------------------------------------------------------------------------------------------------------
                {
                    IM30FieldElementModel cdC2 = (from fe in orgData.FieldElementCollection
                                                  where FieldTypeDef.IsEqualType(fe.FieldTypeCode, FieldTypeDef.CardToken)
                                                  select fe).FirstOrDefault();

                    if ((cdC2 is null) || (string.IsNullOrWhiteSpace(cdC2.Data)))
                    {
                        errMsg += "Credit/Debit Card Token not found;";
                    }
                    else
                        CardToken = cdC2.Data.Trim();
                }
                /////-----------------------------------------------------------------------------------------------------------
                {
                    IM30FieldElementModel cd50 = (from fe in orgData.FieldElementCollection
                                                  where FieldTypeDef.IsEqualType(fe.FieldTypeCode, FieldTypeDef.BatchNo)
                                                  select fe).FirstOrDefault();

                    if ((cd50 is null) || (string.IsNullOrWhiteSpace(cd50.Data)))
                    {
                        errMsg += "Invalid Batch No.;";
                    }
                    else
                        BatchNumber = cd50.Data.Trim();
                }
                /////-----------------------------------------------------------------------------------------------------------
                {
                    IM30FieldElementModel cd03 = (from fe in orgData.FieldElementCollection
                                                  where FieldTypeDef.IsEqualType(fe.FieldTypeCode, FieldTypeDef.TransactionDate)
                                                  select fe).FirstOrDefault();

                    IM30FieldElementModel cd04 = (from fe in orgData.FieldElementCollection
                                                  where FieldTypeDef.IsEqualType(fe.FieldTypeCode, FieldTypeDef.TransactionTime)
                                                  select fe).FirstOrDefault();

                    if ((cd03 is null) || (string.IsNullOrWhiteSpace(cd03.Data)))
                    {
                        errMsg += "Invalid Transaction Date;";
                    }
                    else if ((cd04 is null) || (string.IsNullOrWhiteSpace(cd04.Data)))
                    {
                        errMsg += "Invalid Transaction Time;";
                    }
                    else
                    {
                        if (long.TryParse(cd03.Data.Trim(), out _) == false)
                        {
                            errMsg += $@"Invalid Transaction Date (B);{cd03.Data.Trim()}";
                        }
                        else if (long.TryParse(cd04.Data.Trim(), out _) == false)
                        {
                            errMsg += $@"Invalid Transaction Time (B);{cd04.Data.Trim()}";
                        }
                        else
                        {
                            ///// note : "20" + yyMMdd +  HHmmss
                            string aDateTime = "20" + cd03.Data.Trim() + cd04.Data.Trim();
                            string format = "yyyyMMddHHmmss";

                            try
                            {
                                TransactionDateTime = DateTime.ParseExact(aDateTime, format, CultureInfo.InvariantCulture);
                            }
                            catch (Exception ex)
                            {
                                TransactionDateTime = null;
                                errMsg += $@"Invalid Transaction Date-{cd03.Data.Trim()};";
                            }
                        }
                    }
                }
                /////-----------------------------------------------------------------------------------------------------------
                {
                    IM30FieldElementModel cdD3 = (from fe in orgData.FieldElementCollection
                                                  where FieldTypeDef.IsEqualType(fe.FieldTypeCode, FieldTypeDef.RRN)
                                                  select fe).FirstOrDefault();

                    if ((cdD3 is null) || (string.IsNullOrWhiteSpace(cdD3.Data)))
                    {
                        errMsg += "Invalid Retrieval Reference Number (RRN);";
                    }
                    else
                        RRN = cdD3.Data.Trim();
                }
                /////-----------------------------------------------------------------------------------------------------------
                {
                    IM30FieldElementModel cdD4 = (from fe in orgData.FieldElementCollection
                                                  where FieldTypeDef.IsEqualType(fe.FieldTypeCode, FieldTypeDef.IssuerId)
                                                  select fe).FirstOrDefault();

                    if ((cdD4 is null) || (string.IsNullOrWhiteSpace(cdD4.Data)))
                    {
                        //Not Importance .. errMsg += "Invalid Card IssuerId;";
                    }
                    else
                        CardIssuerID = cdD4.Data.Trim();
                }
                /////-----------------------------------------------------------------------------------------------------------
                {
                    IM30FieldElementModel cdD5 = (from fe in orgData.FieldElementCollection
                                                  where FieldTypeDef.IsEqualType(fe.FieldTypeCode, FieldTypeDef.CardHolderName)
                                                  select fe).FirstOrDefault();

                    if ((cdD5 is null) || (string.IsNullOrWhiteSpace(cdD5.Data)))
                    {
                        //Not Importance .. errMsg += "Invalid Card Holder Name;";
                    }
                    else
                        CardHolderName = cdD5.Data.Trim();
                }
                /////-----------------------------------------------------------------------------------------------------------
                {
                    IM30FieldElementModel cdE5 = (from fe in orgData.FieldElementCollection
                                                  where FieldTypeDef.IsEqualType(fe.FieldTypeCode, FieldTypeDef.MerchantCopyCVMDesc)
                                                  select fe).FirstOrDefault();

                    if ((cdE5 is null) || (string.IsNullOrWhiteSpace(cdE5.Data)))
                    {
                        //Not Importance .. errMsg += "Invalid Merchant Copy CVM Desc.;";
                    }
                    else
                        CVMCopyDesc = cdE5.Data.Trim();
                }
                /////-----------------------------------------------------------------------------------------------------------
                {
                    IM30FieldElementModel cdE6 = (from fe in orgData.FieldElementCollection
                                                  where FieldTypeDef.IsEqualType(fe.FieldTypeCode, FieldTypeDef.CustomerCopyCVMDesc)
                                                  select fe).FirstOrDefault();

                    if ((cdE6 is null) || (string.IsNullOrWhiteSpace(cdE6.Data)))
                    {
                        //Not Importance .. errMsg += "Invalid Customer Copy CVM Desc.;";
                    }
                    else
                        CustomerCopyCVM = cdE6.Data.Trim();
                }
                /////-----------------------------------------------------------------------------------------------------------
                {
                    IM30FieldElementModel cdE6 = (from fe in orgData.FieldElementCollection
                                                  where FieldTypeDef.IsEqualType(fe.FieldTypeCode, FieldTypeDef.STAN)
                                                  select fe).FirstOrDefault();

                    if ((cdE6 is null) || (string.IsNullOrWhiteSpace(cdE6.Data)))
                    {
                        errMsg += "Invalid Standard Unique Trace Number (STAN);";
                    }
                    else
                        STAN = cdE6.Data.Trim();
                }
                /////-----------------------------------------------------------------------------------------------------------
                {
                    IM30FieldElementModel cdF1 = (from fe in orgData.FieldElementCollection
                                                  where FieldTypeDef.IsEqualType(fe.FieldTypeCode, FieldTypeDef.PaymentIndicator)
                                                  select fe).FirstOrDefault();

                    if ((cdF1 is null) || (string.IsNullOrWhiteSpace(cdF1.Data)))
                    {
                        //Not Importance .. errMsg += "Invalid Payment Indicator;";
                    }
                    else
                    {
                        if (cdF1.Data.Trim().Equals("1", StringComparison.InvariantCultureIgnoreCase) == true)
                        {
                            PaymentIndicator = PaymentIndicatorEn.ContactCard;
                        }
                        else if (cdF1.Data.Trim().Equals("2", StringComparison.InvariantCultureIgnoreCase) == true)
                        {
                            PaymentIndicator = PaymentIndicatorEn.ContactlessCard;
                        }
                        else if (cdF1.Data.Trim().Equals("0", StringComparison.InvariantCultureIgnoreCase) == true)
                        {
                            PaymentIndicator = PaymentIndicatorEn.Unknown;
                        }
                        //Not Importance .. else
                        //Not Importance .. errMsg += $@"Invalid Payment Indicator ({cdF1.Data.Trim()});";
                    }
                }
                /////-----------------------------------------------------------------------------------------------------------
                {
                    if (string.IsNullOrWhiteSpace(ResponseText))
                    {
                        IM30FieldElementModel cd02 = (from fe in orgData.FieldElementCollection
                                                      where FieldTypeDef.IsEqualType(fe.FieldTypeCode, FieldTypeDef.ResponseText)
                                                      select fe).FirstOrDefault();

                        if ((cd02 is null) || (string.IsNullOrWhiteSpace(cd02.Data)))
                        {
                            //Not Importance .. errMsg += "Invalid Response Text;";
                        }
                        else
                            ResponseText = cd02.Data.Trim();
                    }
                }

                if (string.IsNullOrWhiteSpace(errMsg) == false)
                    DataError = new Exception(errMsg);
            }
        }

        public CreditDebitVoidTransactionResp(Exception error)
        {
            IsDataFound = false;

            if (error is null)
                error = new Exception("-Unknown Credit/Debit Charge Card Response~");

            DataError = error;
        }
    }
}