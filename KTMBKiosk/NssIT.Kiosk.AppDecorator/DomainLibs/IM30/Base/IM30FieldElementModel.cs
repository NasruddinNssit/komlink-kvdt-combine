using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.IM30.Base
{
    public class IM30FieldElementModel
    {
        public int SequenceNo { get; private set; }
        public string FieldTypeCode { get; private set; }
        public string FieldTypeDesc { get; private set; } = "";
        public int FieldLength { get; private set; } = 0;
        public string Data { get; private set; } = "";
        public byte FieldSeparator { get; private set; } = 0x1C;

        private IM30FieldElementModel()
        {

        }

        /// <summary>
        /// This constructor used to create a new Request Field Element. But not suitable for Response Field Element
        /// </summary>
        /// <param name="sequenceNo"></param>
        /// <param name="fieldTypeCode"></param>
        /// <param name="data"></param>
        /// <param name="fieldSeparator"></param>
        public IM30FieldElementModel(int sequenceNo, string fieldTypeCode, string data, byte fieldSeparator)
        {
            IM30FieldElementStructure fEleStrct = FieldTypeDef.GetTypeProperties(fieldTypeCode);

            if (fEleStrct is null)
                throw new Exception($@"Card Reader's Field Type({fieldTypeCode}) not found.");

            if (string.IsNullOrWhiteSpace(data))
                Data = "";
            else
                Data = data.Trim();

            int maxDataLength = fEleStrct.FieldLength;

            SequenceNo = sequenceNo;
            FieldTypeCode = fieldTypeCode;
            FieldTypeDesc = fEleStrct.FieldDescription;

            if (fEleStrct.IsDefinableFieldLength)
            {
                if (Data.Length > maxDataLength)
                    throw new Exception($@"-Card Reader's Data out of range. Current Data Length~: {data.Trim().Length} ; Max.Data Length Allowed: {maxDataLength}");

                FieldLength = Data.Length;
            }
            else
            {
                FieldLength = fEleStrct.FieldLength;
            }

            FieldSeparator = fieldSeparator;
            
            Data = data.Trim();
            if (Data.Length > FieldLength)
            {
                Data = "";
                throw new Exception($@"Card Reader's Data out of range. Current Data Length : {data.Trim().Length} ; Max.Data Length : {FieldLength}");
            }
            else
            {
                if ((fEleStrct.FieldAttribute == FieldTypeAttributeEn.ANS) || (fEleStrct.FieldAttribute == FieldTypeAttributeEn.AN) || (fEleStrct.FieldAttribute == FieldTypeAttributeEn.Z))
                    Data = Data.PadRight(FieldLength, ' ');

                else if (fEleStrct.FieldAttribute == FieldTypeAttributeEn.N_M)
                {
                    /////.....for credit/debit card, this field may mask with "*"
                    if (Data.IndexOf(' ') >= 0)
                    {
                        throw new Exception($@"-Card Reader's Data not allowed to have SPACE in related Datetime format~. Data: '{data}' ; Field Type Attribute : {fEleStrct.FieldAttribute}");
                    }
                }

                else if ((fEleStrct.FieldAttribute == FieldTypeAttributeEn.N_D)
                    || (fEleStrct.FieldAttribute == FieldTypeAttributeEn.N_DS)
                    || (fEleStrct.FieldAttribute == FieldTypeAttributeEn.N_DT)
                    || (fEleStrct.FieldAttribute == FieldTypeAttributeEn.N_M)
                    || (fEleStrct.FieldAttribute == FieldTypeAttributeEn.N_TS))
                {
                    if (Data.IndexOf(' ') >= 0)
                    {
                        throw new Exception($@"-Card Reader's Data not allowed to have SPACE in related Datetime format~. Data: '{data}' ; Field Type Attribute : {fEleStrct.FieldAttribute}");
                    }
                    if (long.TryParse(Data, out _) == false)
                    {
                        throw new Exception($@"-Invalid Datetime format for Card Reader Data~. Data: '{data}' ; Field Type Attribute : {fEleStrct.FieldAttribute}");
                    }
                }

                else if ((fEleStrct.FieldAttribute == FieldTypeAttributeEn.M) || (fEleStrct.FieldAttribute == FieldTypeAttributeEn.N))
                    Data = Data.PadLeft(FieldLength, '0');

                else if ((fEleStrct.FieldAttribute == FieldTypeAttributeEn.T) || (fEleStrct.FieldAttribute == FieldTypeAttributeEn.B))
                    throw new Exception($@"Card Reader Error; Field Attribute ({fEleStrct.FieldAttribute}) for the field type ({FieldTypeCode}) is not supported at the moment");
            }
        }

        ///// <summary>
        ///// This constructor used to create a Response Field Element. But not suitable for new Request Field Element
        ///// </summary>
        ///// <param name="sequenceNo"></param>
        ///// <param name="fieldTypeCode"></param>
        ///// <param name="data"></param>
        ///// <param name="fieldSeparator"></param>
        ///// <param name="fieldLength"></param>
        //public IM30FieldElementModel(int sequenceNo, string fieldTypeCode, string data, byte fieldSeparator, int fieldLength)
        //{
        //    IM30FieldElementStructure fEleStrct = FieldTypeDef.GetTypeProperties(fieldTypeCode);
        //    SequenceNo = sequenceNo;
        //    FieldTypeCode = fieldTypeCode;
        //    FieldSeparator = fieldSeparator;
        //    FieldLength = fieldLength;
        //    Data = data;
        //    if (string.IsNullOrWhiteSpace(data) == false)
        //        ActualDataLength = data.Length;
        //    if (fEleStrct != null)
        //        FieldTypeDesc = fEleStrct.FieldDescription;
        //}

        public byte[] GetRenderedData()
        {
            ///// ( 2 <for FieldTypeCode> + 2 <for FieldLength> + FieldLength + 1 <for FieldSeparator>)
            int dataLength = (2 + 2 + FieldLength + 1);
            byte[] retRenderedData = new byte[dataLength];

            byte[] dFieldType = System.Text.Encoding.ASCII.GetBytes(FieldTypeCode);

            string fieldLengthHexStr = FieldLength.ToString().Trim().PadLeft(4, '0');
            byte[] dDataLengthLLLL = IM30Tools.Get2BytesDataFromBCD(fieldLengthHexStr);

            byte[] dData = System.Text.Encoding.ASCII.GetBytes(Data);

            System.Buffer.BlockCopy(dFieldType, 0, retRenderedData, 0, dFieldType.Length);
            System.Buffer.BlockCopy(dDataLengthLLLL, 0, retRenderedData, dFieldType.Length, dDataLengthLLLL.Length);
            System.Buffer.BlockCopy(dData, 0, retRenderedData, dFieldType.Length + dDataLengthLLLL.Length, dData.Length);
            retRenderedData[retRenderedData.Length - 1] = FieldSeparator;

            return retRenderedData;
        }

        public IM30FieldElementModel Duplicate()
        {
            return new IM30FieldElementModel()
            {
                Data = this.Data, 
                FieldLength = this.FieldLength, 
                FieldSeparator = this.FieldSeparator, 
                FieldTypeCode = this.FieldTypeCode, 
                FieldTypeDesc = this.FieldTypeDesc, 
                SequenceNo = this.SequenceNo
            };
        }
    }

    public class IM30FieldElementStructure
    {
        public string FieldType { get; private set; }
        public string FieldDescription { get; private set; }
        public FieldTypeAttributeEn FieldAttribute { get; private set; }
        public int FieldLength { get; private set; } = 0;
        public bool IsDefinableFieldLength { get; private set; } = false;

        public IM30FieldElementStructure(string fieldType, string fieldDescription, FieldTypeAttributeEn fieldAttribute, int fieldLength, bool isDefinableFieldLength = false)
        {
            FieldType = fieldType;
            FieldDescription = fieldDescription;
            FieldAttribute = fieldAttribute;
            FieldLength = fieldLength;
            IsDefinableFieldLength = isDefinableFieldLength;
        }
    }
}
