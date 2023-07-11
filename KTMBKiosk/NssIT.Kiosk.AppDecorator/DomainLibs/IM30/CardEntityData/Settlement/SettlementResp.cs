using NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI;
using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.Base;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.IM30.CardEntityData.Settlement
{
    public class SettlementResp : ICardResponse, IDisposable 
    {
        public SettlementStatusEn SettlementResult { get; private set; } = SettlementStatusEn.Fail;
        public string ResponseText { get; private set; } = "";
        public string MerchantNameAddr { get; private set; } = "";
        public DateTime? TransactionDateTime { get; private set; } = null;
        public int TotalSettleBatch { get; private set; } = 0;
        public bool IsDataFound { get; private set; } = false;
        public Exception DataError { get; private set; } = null;
        public List<SettlementBatch> SettlementList { get; private set; } = new List<SettlementBatch>();

        public SettlementResp(Exception error)
        {
            SettlementResult = SettlementStatusEn.Fail;

            if (error != null)
                DataError = error;
            else
                DataError = new Exception("~Error; Unknown Settlement Response~");
        }

        public SettlementResp(IM30DataModel orgData)
        {
            if (orgData is null)
                DataError = new Exception("-Invalid data when translate to Settlement Info~");

            if (DataError is null)
            {
                IM30FieldElementModel cd02 = (from fe in orgData.FieldElementCollection
                                              where FieldTypeDef.IsEqualType(fe.FieldTypeCode, FieldTypeDef.SettleResponseText)
                                              select fe).FirstOrDefault();

                if ((cd02 is null) || (string.IsNullOrWhiteSpace(cd02.Data)))
                {
                    //Not Importance .. errMsg += "Invalid Response Text;";
                }
                else
                    ResponseText = cd02.Data.Trim();
            }

            if ((DataError is null) && ((TransactionCodeDef.IsEqualTrans(orgData.TransactionCode, TransactionCodeDef.Settlement) == false)))
            {
                DataError = new Exception("-Transaction Code not found when translate Settlement Info~");
            }

            if ((DataError is null) && ((ResponseCodeDef.IsEqualResponse(orgData.ResponseCode, ResponseCodeDef.TransactionNotAvailable))))
            {
                SettlementResult = SettlementStatusEn.Empty;
                DataError = new Exception($@"-No Sale Transaction Found~({orgData.ResponseCode})");
            }

            if ((DataError is null) && ((ResponseCodeDef.IsEqualResponse(orgData.ResponseCode, ResponseCodeDef.Approved) == false)))
            {
                DataError = new Exception("-Fail to Settlement (Reader) Info~");
            }

            if (DataError is null)
            {
                IsDataFound = true;
                string errMsg = "";
                ///////-----------------------------------------------------------------------------------------------------------
                //{
                //    IM30FieldElementModel cd02 = (from fe in orgData.FieldElementCollection
                //                                  where FieldTypeDef.IsEqualType(fe.FieldTypeCode, FieldTypeDef.SettleResponseText)
                //                                  select fe).FirstOrDefault();

                //    if ((cd02 is null) || (string.IsNullOrWhiteSpace(cd02.Data)))
                //        errMsg += "Approval code not found;";
                //    else
                //    {
                //        ResponseText = cd02.Data.Trim();
                //    }
                //}
                /////-----------------------------------------------------------------------------------------------------------
                {
                    IM30FieldElementModel cdD0 = (from fe in orgData.FieldElementCollection
                                                  where FieldTypeDef.IsEqualType(fe.FieldTypeCode, FieldTypeDef.MerchantNameAndAddress)
                                                  select fe).FirstOrDefault();

                    if ((cdD0 is null) || (string.IsNullOrWhiteSpace(cdD0.Data)))
                        errMsg += "Approval code not found;";
                    else
                    {
                        MerchantNameAddr = cdD0.Data.Trim();
                    }
                }
                /////-----------------------------------------------------------------------------------------------------------
                {
                    IM30FieldElementModel cdD0 = (from fe in orgData.FieldElementCollection
                                                  where FieldTypeDef.IsEqualType(fe.FieldTypeCode, FieldTypeDef.MerchantNameAndAddress)
                                                  select fe).FirstOrDefault();

                    if ((cdD0 is null) || (string.IsNullOrWhiteSpace(cdD0.Data)))
                        errMsg += "Approval code not found;";
                    else
                    {
                        MerchantNameAddr = cdD0.Data.Trim();
                    }
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
                    IM30FieldElementModel cds1 = (from fe in orgData.FieldElementCollection
                                                  where FieldTypeDef.IsEqualType(fe.FieldTypeCode, FieldTypeDef.SettleTotalNoOfBatch)
                                                  select fe).FirstOrDefault();

                    if ((cds1 is null) || (string.IsNullOrWhiteSpace(cds1.Data)))
                        errMsg += "Total Settle Batch not found;";

                    else if (int.TryParse(cds1.Data.Trim(), out int totBch))
                    {
                        TotalSettleBatch = totBch;
                    }
                    else
                        errMsg += $@"-Invalid Total Settle Batch~({cds1.Data})";
                }
                /////-----------------------------------------------------------------------------------------------------------
                {
                    bool isFoundSettlementBatch = false;
                    bool isHavingBatchFailSttlement = false;
                    int settleBatchSeqNo = 0;
                    IM30FieldElementModel[] fieldArr = orgData.FieldElementCollection.OrderBy(r => r.SequenceNo).ToArray();
                    if (fieldArr?.Length > 0)
                    {
                        (int BatchSeqNo, BatchSettlementStatusEn BatchSettlementStatus, string SettlementBatchName, string SettlementResponseCode,
                            string SettlementResponseText, string TID, string MerchantNo, string BatchNo, DateTime SettlementDateTime, int TotalSalesCount,
                            decimal TotalSalesAmount, int TotalRefundCount, decimal TotalRefundAmount) tmpBatch 
                            = (BatchSeqNo: -200, BatchSettlementStatus: BatchSettlementStatusEn.Fail, SettlementBatchName: "", SettlementResponseCode: "",
                                SettlementResponseText: "", TID: "", MerchantNo: "", BatchNo: "", SettlementDateTime: DateTime.Now, TotalSalesCount: 0,
                                TotalSalesAmount: 0.0M, TotalRefundCount: 0, TotalRefundAmount: 0.0M);
                        
                        SettlementBatch currBatch = null;
                        foreach (IM30FieldElementModel fe1 in fieldArr)
                        {
                            if (FieldTypeDef.IsEqualType(fe1.FieldTypeCode, FieldTypeDef.SettleBatchName))
                            {
                                isFoundSettlementBatch = true;
                                if (tmpBatch.BatchSeqNo == -1)
                                {
                                    settleBatchSeqNo++;
                                    tmpBatch.BatchSeqNo = settleBatchSeqNo;
                                    SettlementList.Add(new SettlementBatch(tmpBatch.BatchSeqNo, tmpBatch.BatchSettlementStatus, tmpBatch.SettlementBatchName, tmpBatch.SettlementResponseCode,
                                        tmpBatch.SettlementResponseText, tmpBatch.TID, tmpBatch.MerchantNo, tmpBatch.BatchNo, tmpBatch.SettlementDateTime, tmpBatch.TotalSalesCount,
                                        tmpBatch.TotalSalesAmount, tmpBatch.TotalRefundCount, tmpBatch.TotalRefundAmount));
                                }
                                tmpBatch = (BatchSeqNo: -1, BatchSettlementStatus: BatchSettlementStatusEn.Fail, SettlementBatchName: "", SettlementResponseCode: "",
                                            SettlementResponseText: "", TID: "", MerchantNo: "", BatchNo: "", SettlementDateTime: DateTime.Now, TotalSalesCount: 0,
                                            TotalSalesAmount: 0.0M, TotalRefundCount: 0, TotalRefundAmount: 0.0M);

                                if (string.IsNullOrWhiteSpace(fe1.Data) == false)
                                    tmpBatch.SettlementBatchName = fe1.Data.Trim();
                            }
                            else if (FieldTypeDef.IsEqualType(fe1.FieldTypeCode, FieldTypeDef.SettleResponseCode))
                            {
                                if (ResponseCodeDef.IsEqualResponse(fe1.Data?.ToString().Trim(), ResponseCodeDef.Approved))
                                {
                                    tmpBatch.BatchSettlementStatus = BatchSettlementStatusEn.Success;
                                }
                                else
                                {
                                    isHavingBatchFailSttlement = true;
                                    tmpBatch.BatchSettlementStatus = BatchSettlementStatusEn.Fail;
                                }
                            }
                            else if (FieldTypeDef.IsEqualType(fe1.FieldTypeCode, FieldTypeDef.SettleResponseText))
                            {
                                if (string.IsNullOrWhiteSpace(fe1.Data) == false)
                                    tmpBatch.SettlementResponseText = fe1.Data.Trim();
                            }
                            else if (FieldTypeDef.IsEqualType(fe1.FieldTypeCode, FieldTypeDef.TID))
                            {
                                if (string.IsNullOrWhiteSpace(fe1.Data) == false)
                                    tmpBatch.TID = fe1.Data.Trim();
                            }
                            else if (FieldTypeDef.IsEqualType(fe1.FieldTypeCode, FieldTypeDef.MerchantNo))
                            {
                                if (string.IsNullOrWhiteSpace(fe1.Data) == false)
                                    tmpBatch.MerchantNo = fe1.Data.Trim();
                            }
                            else if (FieldTypeDef.IsEqualType(fe1.FieldTypeCode, FieldTypeDef.BatchNo))
                            {
                                if (string.IsNullOrWhiteSpace(fe1.Data) == false)
                                    tmpBatch.BatchNo = fe1.Data.Trim();
                            }
                            else if (FieldTypeDef.IsEqualType(fe1.FieldTypeCode, FieldTypeDef.SettleDateTime))
                            {
                                if (string.IsNullOrWhiteSpace(fe1.Data) == false)
                                {
                                    ///// note : "20" + yyMMddHHmmss
                                    string aDateTime = "20" + fe1.Data.Trim();
                                    string format = "yyyyMMddHHmmss";
                                    try
                                    {
                                        tmpBatch.SettlementDateTime = DateTime.ParseExact(aDateTime, format, CultureInfo.InvariantCulture);
                                    }
                                    catch (Exception ex)
                                    {
                                        tmpBatch.SettlementDateTime = DateTime.Now;
                                    }
                                }
                                    
                            }
                            else if (FieldTypeDef.IsEqualType(fe1.FieldTypeCode, FieldTypeDef.SettleTotalSalesCount))
                            {
                                if (int.TryParse(fe1.Data.Trim(), out int totSalCnt))
                                {
                                    tmpBatch.TotalSalesCount = totSalCnt;
                                }
                            }
                            else if (FieldTypeDef.IsEqualType(fe1.FieldTypeCode, FieldTypeDef.SettleTotalSalesAmount))
                            {
                                if (long.TryParse(fe1.Data.Trim(), out long totSalAmt))
                                {
                                    tmpBatch.TotalSalesAmount = ((decimal)totSalAmt) / 100M;
                                }
                            }
                            else if (FieldTypeDef.IsEqualType(fe1.FieldTypeCode, FieldTypeDef.SettleTotalRefundCount))
                            {
                                if (int.TryParse(fe1.Data.Trim(), out int totRefdCnt))
                                {
                                    tmpBatch.TotalRefundCount = totRefdCnt;
                                }
                            }
                            else if (FieldTypeDef.IsEqualType(fe1.FieldTypeCode, FieldTypeDef.SettleTotalRefundAmount))
                            {
                                if (long.TryParse(fe1.Data.Trim(), out long totRefdAmt))
                                {
                                    tmpBatch.TotalRefundAmount = ((decimal)totRefdAmt) / 100M;
                                }
                            }
                        }

                        if (tmpBatch.BatchSeqNo == -1)
                        {
                            settleBatchSeqNo++;
                            tmpBatch.BatchSeqNo = settleBatchSeqNo;
                            SettlementList.Add(new SettlementBatch(tmpBatch.BatchSeqNo, tmpBatch.BatchSettlementStatus, tmpBatch.SettlementBatchName, tmpBatch.SettlementResponseCode,
                                tmpBatch.SettlementResponseText, tmpBatch.TID, tmpBatch.MerchantNo, tmpBatch.BatchNo, tmpBatch.SettlementDateTime, tmpBatch.TotalSalesCount,
                                tmpBatch.TotalSalesAmount, tmpBatch.TotalRefundCount, tmpBatch.TotalRefundAmount));
                        }
                    } 
                    
                    if ((isFoundSettlementBatch)
                        &&
                        (SettlementList.Count > 0)
                        &&
                        (TotalSettleBatch == SettlementList.Count)
                        &&
                        (isHavingBatchFailSttlement == false)
                    )
                    {
                        SettlementResult = SettlementStatusEn.Success;
                    }
                    else
                    {
                        if (TotalSettleBatch == 0)
                        {
                            if ((isFoundSettlementBatch) && (SettlementList.Count > 0) && (isHavingBatchFailSttlement == false))
                            {
                                SettlementResult = SettlementStatusEn.Success;
                            }
                            else if ((isFoundSettlementBatch) && (SettlementList.Count > 0) && (isHavingBatchFailSttlement))
                            {
                                SettlementResult = SettlementStatusEn.PartiallyDone;
                            }
                            else
                            {
                                SettlementResult = SettlementStatusEn.Fail;
                            }
                        }
                        else /*if (TotalSettleBatch > 0)*/
                        {
                            if ((isFoundSettlementBatch) && (SettlementList.Count > 0) && (isHavingBatchFailSttlement == false))
                            {
                                SettlementResult = SettlementStatusEn.PartiallyDone;
                            }
                            else if ((isFoundSettlementBatch) && (SettlementList.Count > 0) && (isHavingBatchFailSttlement))
                            {
                                SettlementResult = SettlementStatusEn.PartiallyDone;
                            }
                            else
                            {
                                SettlementResult = SettlementStatusEn.Fail;
                            }
                            
                            if (TotalSettleBatch > SettlementList.Count)
                            {
                                if (DataError is null)
                                {
                                    DataError = new Exception($@"-Header Total Batch Count is Not Tally with actual Settlement detail~Header Show:{TotalSettleBatch}; Detail Show: {SettlementList.Count} ~");
                                }
                            }
                        }
                    }
                }
                /////-----------------------------------------------------------------------------------------------------------

            }
        }

        public void Dispose()
        {
            SettlementList?.Clear();
            SettlementList = null;
        }
    }

    public class SettlementBatch
    {
        public int BatchSeqNo { get; private set; } = -1;
        public BatchSettlementStatusEn BatchSettlementStatus  { get; private set; } = BatchSettlementStatusEn.Fail;
        public string SettlementBatchName { get; private set; } = "";
        public string SettlementResponseCode { get; private set; } = "";
        public string SettlementResponseText { get; private set; } = "";
        public string TID { get; private set; } = "";
        public string MerchantNo { get; private set; } = "";
        public string BatchNo { get; private set; } = "";
        public DateTime SettlementDateTime { get; private set; } = DateTime.Now;
        public int TotalSalesCount { get; private set; } = 0;
        public decimal TotalSalesAmount { get; private set; } = 0.0M;
        public int TotalRefundCount { get; private set; } = 0;
        public decimal TotalRefundAmount { get; private set; } = 0.0M;

        public SettlementBatch(int batchSeqNo, BatchSettlementStatusEn batchSettlementStatus, string settlementBatchName, string settlementResponseCode,
                            string settlementResponseText, string tid, string merchantNo, string batchNo, DateTime settlementDateTime, int totalSalesCount,
                            decimal totalSalesAmount, int totalRefundCount, decimal totalRefundAmount)
        {
            BatchSeqNo = batchSeqNo;
            BatchSettlementStatus = batchSettlementStatus;
            SettlementBatchName = settlementBatchName;
            SettlementResponseCode = settlementResponseCode;
            SettlementResponseText = settlementResponseText;
            TID = tid;
            MerchantNo = merchantNo;
            BatchNo = batchNo;
            SettlementDateTime = settlementDateTime;
            TotalSalesCount = totalSalesCount;
            TotalSalesAmount = totalSalesAmount;
            TotalRefundCount = totalRefundCount;
            TotalRefundAmount = totalRefundAmount;
        }
    }
}
