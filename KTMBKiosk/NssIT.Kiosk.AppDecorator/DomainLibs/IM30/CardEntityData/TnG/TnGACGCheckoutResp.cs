using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.Base;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.IM30.CardEntityData.TnG
{
    public class TnGACGCheckoutResp : ICardResponse
    {
        /// <summary>
        /// Importance
        /// </summary>
        public ResponseCodeEn ResponseResult { get; private set; } = ResponseCodeEn.Fail;

        public string ResponseText { get; private set; } = "";


        public decimal FareAmount { get; private set; } = 0.0M; /*#######################################################################*/
        public decimal PenaltyAmount { get; private set; } = 0.0M; 
        /// <summary>
        /// Not Importance; Merchant Name and Address
        /// </summary>
        public string MerchantNameAddr { get; private set; } = "";
        /// <summary>
        /// Not Importance
        /// </summary>
        public string CardIssuerName { get; private set; } = "";
        /// <summary>
        /// Not Importance; Same as Host No.
        /// </summary>
        public string CardIssuerID { get; private set; } = "";
        /// <summary>
        /// Importance
        /// </summary>
        public DateTime? TransactionDateTime { get; private set; } = DateTime.Now;

        public string TKEntryGateNo { get; private set; } = "";
        public DateTime TKEntryDateTime { get; private set; } = KomLinkCardInfo.MinDateTime;
        public string TKEntryStationCode { get; private set; } = "";
        public string TnGEntryOperatorID { get; private set; } = "";


        public string TKExitGateNo { get; private set; } = "";
        public DateTime TKExitDateTime { get; private set; } = KomLinkCardInfo.MinDateTime;
        public string TKExitStationCode { get; private set; } = "";
        public string TnGExitOperatorID { get; private set; } = "";

        public string TnGDebitTranType { get; private set; } = "";
        public decimal TnGAmountCharged { get; private set; } = 0;
        public decimal TnGAfterCardBalance { get; private set; } = 0;
        public uint TnGCSCTransactionNo { get; private set; } = 0;

        /// <summary>
        /// Last Transaction Operator ID
        /// </summary>
        public string TnGOperatorID { get; private set; } = "";

        public DateTime TKTransactionDateTime { get; private set; } = DateTime.Now;

        public decimal TKMainPurseAmount { get; private set; } = 0;
        public uint TKMainTransNo { get; private set; } = 0;
        public string PosTransId { get; private set; } = "";

        public bool IsDataFound { get; private set; } = false;
        public Exception DataError { get; private set; } = null;

        public TnGACGCheckoutResp(IM30DataModel orgData)
        {
            if (orgData is null)
                DataError = new Exception("-Invalid data when translate to TnG Card Response Info~");

            if (DataError is null)
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

            if ((DataError is null) && ((TransactionCodeDef.IsEqualTrans(orgData.TransactionCode, TransactionCodeDef.Exit) == false)))
            {
                if ((from fe in orgData.FieldElementCollection
                     where FieldTypeDef.IsEqualType(fe.FieldTypeCode, FieldTypeDef.TnGEntryOperatorID)
                     select fe).FirstOrDefault() == null)
                {
                    DataError = new Exception("-TnG card related info not found~");
                }
            }

            if ((DataError is null) && ((ResponseCodeDef.IsEqualResponse(orgData.ResponseCode, ResponseCodeDef.Approved) == false)))
            {
                ResponseResult = ResponseCodeEn.Fail;
                DataError = new Exception($@"-Fail TnG card transaction~{orgData.ResponseCode}-{orgData.ResponseDesc}");
            }

            if (DataError is null)
            {
                string errMsg = "";

                IsDataFound = true;
                ResponseResult = ResponseCodeEn.Success;
                //FareAmount

                /////-----------------------------------------------------------------------------------------------------------
                {
                    IM30FieldElementModel cd40 = (from fe in orgData.FieldElementCollection
                                                  where FieldTypeDef.IsEqualType(fe.FieldTypeCode, FieldTypeDef.TransAmount)
                                                  select fe).FirstOrDefault();

                    if ((cd40 is null) || (string.IsNullOrWhiteSpace(cd40.Data)))
                        errMsg += "-TnG Amount not found~;";

                    else if (long.TryParse(cd40.Data.Trim(), out long trnAmt))
                    {
                        FareAmount = ((decimal)trnAmt) / 100M;
                    }
                    else
                        errMsg += $@"-Invalid TnG Amount~({cd40.Data})";
                }
                /////-----------------------------------------------------------------------------------------------------------
                {
                    IM30FieldElementModel cdP1 = (from fe in orgData.FieldElementCollection
                                                  where FieldTypeDef.IsEqualType(fe.FieldTypeCode, FieldTypeDef.PenaltyAmount)
                                                  select fe).FirstOrDefault();

                    if ((cdP1 is null) || (string.IsNullOrWhiteSpace(cdP1.Data)))
                        errMsg += "-TnG Penalty Amount not found~;";

                    else if (long.TryParse(cdP1.Data.Trim(), out long trnAmt))
                    {
                        PenaltyAmount = ((decimal)trnAmt) / 100M;
                    }
                    else
                        errMsg += $@"-Invalid TnG Penalty Amount~({cdP1.Data})";
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
                    IM30FieldElementModel cd03 = (from fe in orgData.FieldElementCollection
                                                  where FieldTypeDef.IsEqualType(fe.FieldTypeCode, FieldTypeDef.TransactionDate)
                                                  select fe).FirstOrDefault();

                    IM30FieldElementModel cd04 = (from fe in orgData.FieldElementCollection
                                                  where FieldTypeDef.IsEqualType(fe.FieldTypeCode, FieldTypeDef.TransactionTime)
                                                  select fe).FirstOrDefault();

                    if ((cd03 is null) || (string.IsNullOrWhiteSpace(cd03.Data)))
                    {
                        errMsg += "Invalid Reader Transaction Date (D);";
                    }
                    else if ((cd04 is null) || (string.IsNullOrWhiteSpace(cd04.Data)))
                    {
                        errMsg += "Invalid Reader Transaction Time (E);";
                    }
                    else
                    {
                        if (long.TryParse(cd03.Data.Trim(), out _) == false)
                        {
                            errMsg += $@"Invalid Reader Transaction Date (B);{cd03.Data.Trim()}";
                        }
                        else if (long.TryParse(cd04.Data.Trim(), out _) == false)
                        {
                            errMsg += $@"Invalid Reader Transaction Time (G);{cd04.Data.Trim()}";
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
                                TransactionDateTime = DateTime.Now;
                                errMsg += $@"Invalid Reader Transaction Date (T)-{cd03.Data.Trim()};";
                            }
                        }
                    }
                }
                /////-----------------------------------------------------------------------------------------------------------
                {
                    IM30FieldElementModel cdD4 = (from fe in orgData.FieldElementCollection
                                                  where FieldTypeDef.IsEqualType(fe.FieldTypeCode, FieldTypeDef.IssuerId)
                                                  select fe).FirstOrDefault();

                    if ((cdD4 is null) || (string.IsNullOrWhiteSpace(cdD4.Data)))
                    {
                        errMsg += "Invalid Card IssuerID;";
                    }
                    else
                        CardIssuerID = cdD4.Data.Trim();
                }
                /////-----------------------------------------------------------------------------------------------------------
                {
                    IM30FieldElementModel cdT6 = (from fe in orgData.FieldElementCollection
                                                  where FieldTypeDef.IsEqualType(fe.FieldTypeCode, FieldTypeDef.TKMainPurseAmount)
                                                  select fe).FirstOrDefault();

                    if ((cdT6 is null) || (string.IsNullOrWhiteSpace(cdT6.Data)))
                        errMsg += "-TnG Main Purse Amount not found~;";

                    else if (long.TryParse(cdT6.Data.Trim(), out long trnAmt))
                    {
                        TKMainPurseAmount = ((decimal)trnAmt) / 100M;
                    }
                    else
                        errMsg += $@"-Invalid TnG Main Purse Amount~({cdT6.Data})";
                }
                /////-----------------------------------------------------------------------------------------------------------
                {
                    IM30FieldElementModel cdT6 = (from fe in orgData.FieldElementCollection
                                                  where FieldTypeDef.IsEqualType(fe.FieldTypeCode, FieldTypeDef.TKMainTranNo)
                                                  select fe).FirstOrDefault();

                    if ((cdT6 is null) || (string.IsNullOrWhiteSpace(cdT6.Data)))
                        errMsg += "TnG Main Trans. No. not found;";

                    else if (uint.TryParse(cdT6.Data.Trim(), out uint trnNo))
                    {
                        TKMainTransNo = trnNo;
                    }
                    else
                        errMsg += $@"-Invalid TnG Main Trans. No.~({cdT6.Data});";
                }
                /////-----------------------------------------------------------------------------------------------------------
                {
                    IM30FieldElementModel cdH4 = (from fe in orgData.FieldElementCollection
                                                  where FieldTypeDef.IsEqualType(fe.FieldTypeCode, FieldTypeDef.TKEntryGateNo)
                                                  select fe).FirstOrDefault();

                    if ((cdH4 is null) || (string.IsNullOrWhiteSpace(cdH4.Data)))
                    {
                        //Ignored
                    }
                    else
                        TKEntryGateNo = cdH4.Data.Trim();
                }
                /////-----------------------------------------------------------------------------------------------------------
                {
                    IM30FieldElementModel cdH2 = (from fe in orgData.FieldElementCollection
                                                  where FieldTypeDef.IsEqualType(fe.FieldTypeCode, FieldTypeDef.TKEntryDateTime)
                                                  select fe).FirstOrDefault();

                    if ((cdH2 is null) || (string.IsNullOrWhiteSpace(cdH2.Data)))
                    {
                        //Ignored
                    }
                    else
                    {
                        if (long.TryParse(cdH2.Data.Trim(), out _) == false)
                        {
                            errMsg += $@"-Invalid TnG Entry Date Time (A)~{cdH2.Data.Trim()};";
                        }
                        else if (long.TryParse(cdH2.Data.Trim(), out long dt1) && (dt1 == 0))
                        {
                            TKEntryDateTime = KomLinkCardInfo.MinDateTime;
                        }
                        else
                        {
                            ///// note : "20" + yyMMddHHmmss
                            string aDateTime = "20" + cdH2.Data.Trim();
                            string format = "yyyyMMddHHmmss";

                            try
                            {
                                TKEntryDateTime = DateTime.ParseExact(aDateTime, format, CultureInfo.InvariantCulture);
                            }
                            catch (Exception ex)
                            {
                                TKEntryDateTime = KomLinkCardInfo.MinDateTime;
                                errMsg += $@"-Invalid TnG Entry Date Time (B)~{cdH2.Data.Trim()};";
                            }
                        }
                    }
                }
                /////-----------------------------------------------------------------------------------------------------------
                {
                    IM30FieldElementModel cdH1 = (from fe in orgData.FieldElementCollection
                                                  where FieldTypeDef.IsEqualType(fe.FieldTypeCode, FieldTypeDef.TKEntryStationCode)
                                                  select fe).FirstOrDefault();

                    if ((cdH1 is null) || (string.IsNullOrWhiteSpace(cdH1.Data)))
                    {
                        //Ignored
                    }
                    else
                        TKEntryStationCode = cdH1.Data.Trim();
                }
                /////-----------------------------------------------------------------------------------------------------------
                {
                    IM30FieldElementModel cdH3 = (from fe in orgData.FieldElementCollection
                                                  where FieldTypeDef.IsEqualType(fe.FieldTypeCode, FieldTypeDef.TnGEntryOperatorID)
                                                  select fe).FirstOrDefault();

                    if ((cdH3 is null) || (string.IsNullOrWhiteSpace(cdH3.Data)))
                    {
                        //Ignored
                    }
                    else
                        TnGEntryOperatorID = cdH3.Data.Trim();
                }
                /////-----------------------------------------------------------------------------------------------------------
                {
                    IM30FieldElementModel cdH8 = (from fe in orgData.FieldElementCollection
                                                  where FieldTypeDef.IsEqualType(fe.FieldTypeCode, FieldTypeDef.TKExitGateNo)
                                                  select fe).FirstOrDefault();

                    if ((cdH8 is null) || (string.IsNullOrWhiteSpace(cdH8.Data)))
                    {
                        //Ignored
                    }
                    else
                        TKExitGateNo = cdH8.Data.Trim();
                }
                /////-----------------------------------------------------------------------------------------------------------
                {
                    IM30FieldElementModel cdH6 = (from fe in orgData.FieldElementCollection
                                                  where FieldTypeDef.IsEqualType(fe.FieldTypeCode, FieldTypeDef.TKExitDateTime)
                                                  select fe).FirstOrDefault();

                    if ((cdH6 is null) || (string.IsNullOrWhiteSpace(cdH6.Data)))
                    {
                        //Not Importance .. errMsg += "Invalid Card IssuerId;";
                    }
                    else
                    {
                        if (long.TryParse(cdH6.Data.Trim(), out _) == false)
                        {
                            errMsg += $@"Invalid TnG Exit Datetime;{cdH6.Data.Trim()}";
                        }
                        else if (long.TryParse(cdH6.Data.Trim(), out long dt1) && (dt1 == 0))
                        {
                            TKExitDateTime = KomLinkCardInfo.MinDateTime;
                        }
                        else
                        {
                            ///// note : "20" + yyMMddHHmmss
                            string aDateTime = "20" + cdH6.Data.Trim();
                            string format = "yyyyMMddHHmmss";

                            try
                            {
                                TKExitDateTime = DateTime.ParseExact(aDateTime, format, CultureInfo.InvariantCulture);
                            }
                            catch (Exception ex)
                            {
                                TKExitDateTime = KomLinkCardInfo.MinDateTime;
                                errMsg += $@"Invalid TnG Exit Datetime-{cdH6.Data.Trim()};";
                            }
                        }
                    }
                }
                /////-----------------------------------------------------------------------------------------------------------
                {
                    IM30FieldElementModel cdH5 = (from fe in orgData.FieldElementCollection
                                                  where FieldTypeDef.IsEqualType(fe.FieldTypeCode, FieldTypeDef.TKExitStationCode)
                                                  select fe).FirstOrDefault();

                    if ((cdH5 is null) || (string.IsNullOrWhiteSpace(cdH5.Data)))
                    {
                        //By Pass
                    }
                    else
                        TKExitStationCode = cdH5.Data.Trim();
                }
                /////-----------------------------------------------------------------------------------------------------------
                {
                    IM30FieldElementModel cdH7 = (from fe in orgData.FieldElementCollection
                                                  where FieldTypeDef.IsEqualType(fe.FieldTypeCode, FieldTypeDef.TnGExitOperatorID)
                                                  select fe).FirstOrDefault();

                    if ((cdH7 is null) || (string.IsNullOrWhiteSpace(cdH7.Data)))
                    {
                        //By Pass
                    }
                    else
                        TnGExitOperatorID = cdH7.Data.Trim();
                }
                /////-----------------------------------------------------------------------------------------------------------
                {
                    IM30FieldElementModel cdTA = (from fe in orgData.FieldElementCollection
                                                  where FieldTypeDef.IsEqualType(fe.FieldTypeCode, FieldTypeDef.TnGDebitTranType)
                                                  select fe).FirstOrDefault();

                    if ((cdTA is null) || (string.IsNullOrWhiteSpace(cdTA.Data)))
                    {
                        //Not Importance .. errMsg += "Invalid Merchant Name And Address;";
                    }
                    else
                        TnGDebitTranType = cdTA.Data.Trim();
                }
                /////-----------------------------------------------------------------------------------------------------------
                {
                    IM30FieldElementModel cdTC = (from fe in orgData.FieldElementCollection
                                                  where FieldTypeDef.IsEqualType(fe.FieldTypeCode, FieldTypeDef.TnGAmountCharged)
                                                  select fe).FirstOrDefault();

                    if ((cdTC is null) || (string.IsNullOrWhiteSpace(cdTC.Data)))
                        errMsg += "-TnG Amount Charged not found~;";

                    else if (long.TryParse(cdTC.Data.Trim(), out long trnAmt))
                    {
                        TnGAmountCharged = ((decimal)trnAmt) / 100M;
                    }
                    else
                        errMsg += $@"-Invalid TnG Amount Charged~({cdTC.Data})";
                }
                /////-----------------------------------------------------------------------------------------------------------
                {
                    IM30FieldElementModel cdTD = (from fe in orgData.FieldElementCollection
                                                  where FieldTypeDef.IsEqualType(fe.FieldTypeCode, FieldTypeDef.TnGAfterCardBalance)
                                                  select fe).FirstOrDefault();

                    if ((cdTD is null) || (string.IsNullOrWhiteSpace(cdTD.Data)))
                        errMsg += "-TnG After Car dBalance not found~;";

                    else if (long.TryParse(cdTD.Data.Trim(), out long trnAmt))
                    {
                        TnGAfterCardBalance = ((decimal)trnAmt) / 100M;
                    }
                    else
                        errMsg += $@"-Invalid TnG After Card Balance~({cdTD.Data})";
                }
                /////-----------------------------------------------------------------------------------------------------------
                {
                    IM30FieldElementModel cdTB = (from fe in orgData.FieldElementCollection
                                                  where FieldTypeDef.IsEqualType(fe.FieldTypeCode, FieldTypeDef.TnGCSCTransactionNo)
                                                  select fe).FirstOrDefault();

                    if ((cdTB is null) || (string.IsNullOrWhiteSpace(cdTB.Data)))
                        errMsg += "TnG CSC Transaction No. not found;";

                    else if (uint.TryParse(cdTB.Data.Trim(), out uint trnNo))
                    {
                        TnGCSCTransactionNo = trnNo;
                    }
                    else
                        errMsg += $@"-Invalid CSC Transaction No.~({cdTB.Data});";
                }
                /////-----------------------------------------------------------------------------------------------------------
                {
                    IM30FieldElementModel cdTE = (from fe in orgData.FieldElementCollection
                                                  where FieldTypeDef.IsEqualType(fe.FieldTypeCode, FieldTypeDef.TnGOperatorID)
                                                  select fe).FirstOrDefault();

                    if ((cdTE is null) || (string.IsNullOrWhiteSpace(cdTE.Data)))
                    {
                        //Not Importance .. errMsg += "Invalid Merchant Name And Address;";
                    }
                    else
                        TnGOperatorID = cdTE.Data.Trim();
                }
                /////-----------------------------------------------------------------------------------------------------------
                {
                    IM30FieldElementModel cdTF = (from fe in orgData.FieldElementCollection
                                                  where FieldTypeDef.IsEqualType(fe.FieldTypeCode, FieldTypeDef.TKTransactionDateTime)
                                                  select fe).FirstOrDefault();

                    if ((cdTF is null) || (string.IsNullOrWhiteSpace(cdTF.Data)))
                    {
                        //Not Importance .. errMsg += "Invalid Card IssuerId;";
                    }
                    else
                    {
                        if (long.TryParse(cdTF.Data.Trim(), out _) == false)
                        {
                            errMsg += $@"Invalid TnG Exit Datetime;{cdTF.Data.Trim()}";
                        }
                        else if (long.TryParse(cdTF.Data.Trim(), out long dt1) && (dt1 == 0))
                        {
                            TKTransactionDateTime = KomLinkCardInfo.MinDateTime;
                        }
                        else
                        {
                            ///// note : "20" + yyMMddHHmmss
                            string aDateTime = "20" + cdTF.Data.Trim();
                            string format = "yyyyMMddHHmmss";

                            try
                            {
                                TKTransactionDateTime = DateTime.ParseExact(aDateTime, format, CultureInfo.InvariantCulture);
                            }
                            catch (Exception ex)
                            {
                                TKTransactionDateTime = KomLinkCardInfo.MinDateTime;
                                errMsg += $@"Invalid TnG Exit Datetime-{cdTF.Data.Trim()};";
                            }
                        }
                    }
                }
                /////-----------------------------------------------------------------------------------------------------------
                {
                    IM30FieldElementModel cd77 = (from fe in orgData.FieldElementCollection
                                                  where FieldTypeDef.IsEqualType(fe.FieldTypeCode, FieldTypeDef.PosTransId)
                                                  select fe).FirstOrDefault();

                    if ((cd77 is null) || (string.IsNullOrWhiteSpace(cd77.Data)))
                    {
                        //Not Importance .. errMsg += "Invalid Merchant Name And Address;";
                    }
                    else
                        PosTransId = cd77.Data.Trim();
                }
                /////-----------------------------------------------------------------------------------------------------------

                if (string.IsNullOrWhiteSpace(errMsg) == false)
                    DataError = new Exception(errMsg);
            }
        }

        public TnGACGCheckoutResp(Exception error)
        {
            IsDataFound = false;

            if (error is null)
                error = new Exception("-Unknown TnG Check-in Response~");

            DataError = error;
        }
    }
}
