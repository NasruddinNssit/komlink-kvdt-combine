using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.IM30.CardEntityData.Sys
{
    public class GetDeviceInfoResp : ICardResponse
    {
        public string DeviceSerialNo { get; private set; } = null;
        public int TnGSamIdStationCode { get; private set; } = -1;
        public string TnGSamIdOperatorID { get; private set; } = null;
        public int TnGSamIdGateNo { get; private set; } = -1;

        public bool IsDataFound { get; private set; } = false;
        public Exception DataError { get; private set; } = null;
        
        public GetDeviceInfoResp(IM30DataModel orgData)
        {
            try
            {
                if (orgData is null)
                    DataError = new Exception("-Invalid data when translate to Device Info~");

                if ((DataError is null) && ((TransactionCodeDef.IsEqualTrans(orgData.TransactionCode, TransactionCodeDef.GetDeviceInfo) == false)))
                {
                    DataError = new Exception("-Transaction Code not matched when translate Device Info~");
                }

                if ((DataError is null) && ((ResponseCodeDef.IsEqualResponse(orgData.ResponseCode, ResponseCodeDef.Approved) == false)))
                {
                    DataError = new Exception("-Fail to read Reader Device Info~");
                }

                if (DataError is null)
                {
                    IsDataFound = true;

                    string errMsg = "";

                    /////-----------------------------------------------------------------------------------------------------------
                    {
                        IM30FieldElementModel daFe = (from fe in orgData.FieldElementCollection
                                                      where FieldTypeDef.IsEqualType(fe.FieldTypeCode, FieldTypeDef.DeviceSerialNo)
                                                      select fe).FirstOrDefault();

                        if ((daFe is null) || (string.IsNullOrWhiteSpace(daFe.Data)))
                            errMsg += "DeviceSerialNo not found;";
                        else
                            DeviceSerialNo = daFe.Data.Trim();
                    }
                    /////-----------------------------------------------------------------------------------------------------------
                    {
                        IM30FieldElementModel daHa = (from fe in orgData.FieldElementCollection
                                                      where FieldTypeDef.IsEqualType(fe.FieldTypeCode, FieldTypeDef.TnGSamIdStationCode)
                                                      select fe).FirstOrDefault();

                        if (daHa is null)
                            errMsg += "TnGSamIdStationCode not found;";
                        else
                        {
                            if (int.TryParse(daHa.Data.Trim(), out int intHa) == true)
                                TnGSamIdStationCode = intHa;
                            //else
                            //    errMsg += $@"TnGSamIdStationCode having invalid data ({daHa.Data.Trim()});";
                        }
                    }
                    /////-----------------------------------------------------------------------------------------------------------
                    {
                        IM30FieldElementModel daHb = (from fe in orgData.FieldElementCollection
                                                      where FieldTypeDef.IsEqualType(fe.FieldTypeCode, FieldTypeDef.TnGSamIdOperatorID)
                                                      select fe).FirstOrDefault();

                        if (daHb is null)
                            errMsg += "Unable to read TnGSamIdOperatorID data;";

                        //else if (string.IsNullOrWhiteSpace(daHb.Data))
                        //    errMsg += "TnGSamIdOperatorID not found;";

                        else
                        {
                            TnGSamIdOperatorID = daHb.Data.Trim();
                        }
                    }
                    /////-----------------------------------------------------------------------------------------------------------
                    {
                        IM30FieldElementModel daHb = (from fe in orgData.FieldElementCollection
                                                      where FieldTypeDef.IsEqualType(fe.FieldTypeCode, FieldTypeDef.TnGSamIdGateNo)
                                                      select fe).FirstOrDefault();

                        if (daHb is null)
                            errMsg += "TnGSamIdStationCode not found;";
                        else
                        {
                            if (int.TryParse(daHb.Data.Trim(), out int intHb) == true)
                                TnGSamIdGateNo = intHb;
                            //else
                            //    errMsg += $@"TnGSamIdGateNo having invalid data ({daHb.Data.Trim()});";
                        }
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
