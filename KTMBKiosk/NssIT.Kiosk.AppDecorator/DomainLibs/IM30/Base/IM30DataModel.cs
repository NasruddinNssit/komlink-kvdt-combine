using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.IM30.Base
{
    public class IM30DataModel : IDisposable 
    {
        ///// xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        ///// Message Data 
        ///// ------------
        public string TransportHeaderType { get; private set; }
        public string TransportDestination { get; private set; }
        public string TransportSource { get; private set; }

        public string FormatVersion { get; private set; }
        public string RequestResponseIndicator { get; private set; }
        public string RequestResponseIndicatorDesc { get; private set; }
        public string TransactionCode { get; private set; }
        public string TransactionDesc { get; private set; }
        public string ResponseCode { get; private set; }
        public string ResponseDesc { get; private set; }
        public string MoreIndicator { get; private set; }
        public string MoreIndicatorDesc { get; private set; }
        public byte FieldSeparator { get; private set; } = 0x1C; 
        public List<IM30FieldElementModel> FieldElementCollection { get; private set; }
        ///// xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        public byte ExpectedSTX { get; private set; } = 0x02;
        public byte ExpectedETX { get; private set; } = 0x03;
        public byte LRC { get; private set; } = 0;
        public string HexLRC => $@"0x{LRC:X2}";
        /// <summary>
        /// Message Data Length in Binary Coded Decimel (BCD); LLLL
        /// </summary>
        public string BCDMsgDataLengthLLLL { get; private set; } = "";
        public string HexDataString { get; private set; } = null;
        public Exception Error { get; private set; } = null;
        ///// xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

        private IM30DataModel()
        {

        }

        public IM30DataModel(string transportHeaderType, string transportDestination, string transportSource,
            string formatVersion, string requestResponseIndicator, string transactionCode, string responseCode, string moreIndicator,
            byte fieldSeparator = 0x1C)
        {
            ///// ----------------------------------------------------------------------------
            ///// Message Data 
            ///// ------------
            TransportHeaderType = transportHeaderType;
            TransportDestination = transportDestination;
            TransportSource = transportSource;

            FormatVersion = formatVersion;
            RequestResponseIndicator = requestResponseIndicator;
            TransactionCode = transactionCode;
            ResponseCode = responseCode;
            MoreIndicator = moreIndicator;
            FieldSeparator = fieldSeparator;

            RequestResponseIndicatorDesc = RequestResponseIndicatorDef.GetIndicatorDesc(requestResponseIndicator);

            TransactionDesc = TransactionCodeDef.GetCodeDesc(transactionCode);

            ResponseDesc = ResponseCodeDef.GetResponseDesc(responseCode, out string extMsg);
            if (string.IsNullOrWhiteSpace(extMsg) == false)
                ResponseDesc += $@"-{extMsg}";

            MoreIndicatorDesc = MoreIndicatorDef.GetIndicatorDesc(moreIndicator);

            FieldElementCollection = new List<IM30FieldElementModel>();
            ///// ----------------------------------------------------------------------------
        }

        /// <summary>
        /// This constuctor suitable be used when data error found and need to store incomming data.
        /// </summary>
        /// <param name="data"></param>
        public IM30DataModel(byte[] data, Exception error)
        {
            if (data != null)
                HexDataString = BitConverter.ToString(data).Replace("-", "");

            Error = error;
        }

        public void AddFieldElement(int sequenceNo, string fieldTypeCode, string data)
        {
            FieldElementCollection.Add(new IM30FieldElementModel(sequenceNo, fieldTypeCode, data, FieldSeparator));
        }

        public void AddFieldElement(int sequenceNo, string fieldTypeCode, DateTime dateTime)
        {
            string data = "";

            IM30FieldElementStructure strct = FieldTypeDef.GetTypeProperties(fieldTypeCode);

            if (strct is null)
                throw new Exception($@"-Card Reader field type not found~Field Type Code: {fieldTypeCode}");

            if (strct.FieldAttribute == FieldTypeAttributeEn.N_D)
            {
                data = dateTime.ToString("yyMMdd");
            }
            else if (strct.FieldAttribute == FieldTypeAttributeEn.N_DT)
            {
                data = dateTime.ToString("yyMMddHHmmss");
            }
            else if (strct.FieldAttribute == FieldTypeAttributeEn.N_M)
            {
                data = dateTime.ToString("yyMM");
            }
            else if (strct.FieldAttribute == FieldTypeAttributeEn.N_DS)
            {
                data = dateTime.ToString("yyMMdd");
            }
            else if (strct.FieldAttribute == FieldTypeAttributeEn.N_TS)
            {
                data = dateTime.ToString("HHmmss");
            }
            else 
            {
                throw new Exception("-Field type not found for related datetime format for Card Reader~");
            }

            FieldElementCollection.Add(new IM30FieldElementModel(sequenceNo, fieldTypeCode, data, FieldSeparator));
        }

        /// <summary>
        /// </summary>
        /// <param name="sequenceNo"></param>
        /// <param name="fieldTypeCode"></param>
        /// <param name="moneyRMAmount">In Ringgit or Dollar unit. 100 cents per Ringgit/Dollar</param>
        public void AddFieldElement(int sequenceNo, string fieldTypeCode, decimal moneyRMAmount)
        {
            int centsAmount = Convert.ToInt32(Math.Floor(moneyRMAmount * 100.0M));

            FieldElementCollection.Add(new IM30FieldElementModel(sequenceNo, fieldTypeCode, centsAmount.ToString().Trim(), FieldSeparator));
        }

        /// <summary>
        /// </summary>
        /// <param name="stx"></param>
        /// <param name="etx"></param>
        /// <param name="lrc"></param>
        /// <param name="bcdLLLL">Message Data Length in Binary Coded Decimel (BCD); LLLL</param>
        public void UpdateMessageDataFrame(byte stx, byte etx, byte lrc, string bcdLLLL, byte[] data = null)
        {
            ExpectedSTX = stx;
            ExpectedETX = etx;
            LRC = lrc;
            BCDMsgDataLengthLLLL = bcdLLLL;

            if (data != null)
                HexDataString = BitConverter.ToString(data).Replace("-", "");
        }

        public IM30DataModel Duplicate()
        {
            //List<IM30FieldElementModel> newFEleColl = new List<IM30FieldElementModel>();
            IM30DataModel retMod = new IM30DataModel()
            {
                BCDMsgDataLengthLLLL = this.BCDMsgDataLengthLLLL, 
                Error = this.Error, 
                ExpectedETX = this.ExpectedETX, 
                ExpectedSTX = this.ExpectedSTX, 
                FieldSeparator = this.FieldSeparator, 
                FormatVersion = this.FormatVersion, 
                HexDataString = this.HexDataString, 
                LRC = this.LRC, 
                MoreIndicator = this.MoreIndicator, 
                MoreIndicatorDesc = this.MoreIndicatorDesc, 
                RequestResponseIndicator = this.RequestResponseIndicator, 
                RequestResponseIndicatorDesc = this.RequestResponseIndicatorDesc, 
                ResponseCode = this.ResponseCode, 
                ResponseDesc = this.ResponseDesc, 
                TransactionCode = this.TransactionCode, 
                TransactionDesc = this.TransactionDesc, 
                TransportDestination = this.TransportDestination, 
                TransportHeaderType = this.TransportHeaderType, 
                TransportSource = this.TransportSource
            };
            retMod.FieldElementCollection  = (from ele in this.FieldElementCollection
                                                select ele.Duplicate()).ToList();

            return retMod;
        }

        public void Dispose()
        {
            BCDMsgDataLengthLLLL = null;

            TransportHeaderType = null;
            TransportDestination = null;
            TransportSource = null;

            FormatVersion = null;
            RequestResponseIndicator = null;
            TransactionCode = null;
            ResponseCode = null;
            MoreIndicator = null;

            HexDataString = null;
            Error = null;

            FieldElementCollection.Clear();
            FieldElementCollection = null;
        }
    }
}
