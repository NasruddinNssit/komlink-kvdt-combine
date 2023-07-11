using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Device.PAX.IM30.OrgAPI.Base
{
    public class IM30RequestResponseDataWorks
    {
        private const byte _stx = 0x02;
        private const byte _etx = 0x03;

        /// <summary>
        /// Hex Request/Response String to Message (Log) String
        /// </summary>
        /// <param name="hexString"></param>
        /// <param name="isLRCValid"></param>
        /// <returns></returns>
        public static string Testing_HexReqRespStrToMsgStr(string hexString, out bool isLRCValid)
        {
            isLRCValid = false;

            string retCommandStr = "\r\n";
            string hexData = hexString;
            string currHexStr = "";

            byte[] commandBytes = new byte[hexString.Length / 2];

            int commByteInx = 0;
            for (int inx = 0; inx < hexString.Length; inx += 2)
            {
                currHexStr = hexString.Substring(inx, 2);
                commandBytes[commByteInx] = Convert.ToByte(currHexStr, 16);
                commByteInx++;
            }

            /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

            int dataLenToRead = 0;
            int currDataInx = 0;
            byte[] data4Byte = new byte[4];
            byte[] data2Byte = new byte[2];
            byte[] data1Byte = new byte[1];
            byte[] fieldData = null;
            byte fieldSeparator = 0;
            int fieldCount = 0;
            string tmpStr = "";
            int fieldDataLength = 0;
            string fieldDataHexLenLLLL = "";
            byte calMsgDataLRC = 0;
            int estimatedNextBlockStartInx = -1;

            // Get START TRANSMISSION
            data1Byte[0] = commandBytes[currDataInx];
            currDataInx++;
            retCommandStr += $@"Expected <STX> : 0x{data1Byte[0]:X2}{"\r\n"}";

            // Get Total Message Data Length
            dataLenToRead = 2;
            data2Byte = new byte[dataLenToRead];
            Array.Copy(commandBytes, currDataInx, data2Byte, 0, dataLenToRead);
            currDataInx += dataLenToRead;
            string msgDataHexLenLLLL = BitConverter.ToString(data2Byte).Replace("-", "");
            int totalMsgDataLength = int.Parse(msgDataHexLenLLLL);
            int totalDataRead = 0;

            retCommandStr += $@"Total Message Data Length : {totalMsgDataLength}{"\r\n\r\n"}";

            //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            //// CYA-TEST - Get Message Data
            byte[] msgData = new byte[totalMsgDataLength];
            Array.Copy(commandBytes, currDataInx, msgData, 0, totalMsgDataLength);
            retCommandStr += $@"Message Data (HEX) : {BitConverter.ToString(msgData)}{"\r\n"}";
            //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            //// Get LLLL + Message Data + ETX
            calMsgDataLRC = IM30Tools.GetLRC(commandBytes);// GetLRC(msgDataX);
            isLRCValid = calMsgDataLRC == commandBytes[commandBytes.Length - 1];

            if (isLRCValid == true)
            {
                retCommandStr += $@"Calculated (I)  Message Data LRC : 0x{calMsgDataLRC:X2}{"\r\n\r\n"}";

                calMsgDataLRC = IM30Tools.GetLRC(commandBytes);
                retCommandStr += $@"Calculated (II) Message Data LRC : 0x{calMsgDataLRC:X2}{"\r\n\r\n\r\n"}";
            }
            else
                retCommandStr += $@"Error ! Incorrect LRC ; Calculated Message Data LRC : 0x{calMsgDataLRC:X2}; Given LRC : {commandBytes[commandBytes.Length - 1]:X2}{"\r\n\r\n"}";
            //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

            retCommandStr += $@"Transport Header {"\r\n"}===================={"\r\n"}";

            // Get Transport Header Type - Transport Header
            dataLenToRead = 2;
            data2Byte = new byte[dataLenToRead];
            Array.Copy(commandBytes, currDataInx, data2Byte, 0, dataLenToRead);
            currDataInx += dataLenToRead;
            totalDataRead += dataLenToRead;
            tmpStr = Encoding.ASCII.GetString(data2Byte, 0, dataLenToRead);
            retCommandStr += $@"Transport Header Type : '{tmpStr}'{"\r\n"}";

            // Get Transport Destination - Transport Header
            dataLenToRead = 4;
            data4Byte = new byte[dataLenToRead];
            Array.Copy(commandBytes, currDataInx, data4Byte, 0, dataLenToRead);
            currDataInx += dataLenToRead;
            totalDataRead += dataLenToRead;
            tmpStr = Encoding.ASCII.GetString(data4Byte, 0, dataLenToRead);
            retCommandStr += $@"Transport Destination : '{tmpStr}'{"\r\n"}";

            // Get Transport Source - Transport Header
            dataLenToRead = 4;
            data4Byte = new byte[dataLenToRead];
            Array.Copy(commandBytes, currDataInx, data4Byte, 0, dataLenToRead);
            currDataInx += dataLenToRead;
            totalDataRead += dataLenToRead;
            tmpStr = Encoding.ASCII.GetString(data4Byte, 0, dataLenToRead);
            retCommandStr += $@"Transport Source : '{tmpStr}'{"\r\n\r\n"}";

            retCommandStr += $@"Presentation Header {"\r\n"}======================={"\r\n"}";

            // Get Format Version - Presentation Header
            dataLenToRead = 1;
            data1Byte[0] = commandBytes[currDataInx];
            currDataInx += dataLenToRead;
            totalDataRead += dataLenToRead;
            tmpStr = Encoding.ASCII.GetString(data1Byte);
            retCommandStr += $@"Format Version : '{tmpStr}'{"\r\n"}";

            // Get Request-Response Indicator - Presentation Header
            dataLenToRead = 1;
            data1Byte[0] = commandBytes[currDataInx];
            currDataInx += dataLenToRead;
            totalDataRead += dataLenToRead;
            tmpStr = Encoding.ASCII.GetString(data1Byte);
            retCommandStr += $@"Request-Response Indicator : '{tmpStr}'{"\r\n"}";

            // Get Transaction Code - Presentation Header
            dataLenToRead = 2;
            data2Byte = new byte[dataLenToRead];
            Array.Copy(commandBytes, currDataInx, data2Byte, 0, dataLenToRead);
            currDataInx += dataLenToRead;
            totalDataRead += dataLenToRead;
            tmpStr = Encoding.ASCII.GetString(data2Byte, 0, dataLenToRead);
            string transDesc = TransactionCodeDef.GetCodeDesc(tmpStr);
            retCommandStr += $@"Transaction Code : '{tmpStr}'{"\t"}({transDesc}){"\r\n"}";

            // Get Response Code - Presentation Header
            dataLenToRead = 2;
            data2Byte = new byte[dataLenToRead];
            Array.Copy(commandBytes, currDataInx, data2Byte, 0, dataLenToRead);
            currDataInx += dataLenToRead;
            totalDataRead += dataLenToRead;
            tmpStr = Encoding.ASCII.GetString(data2Byte, 0, dataLenToRead);
            retCommandStr += $@"Response Code : '{tmpStr}'{"\r\n"}";

            // Get More Indicator - Presentation Header
            dataLenToRead = 1;
            data1Byte[0] = commandBytes[currDataInx];
            currDataInx += dataLenToRead;
            totalDataRead += dataLenToRead;
            tmpStr = Encoding.ASCII.GetString(data1Byte);
            retCommandStr += $@"More Indicator : '{tmpStr}'{"\r\n"}";

            // Get Field Separator - Presentation Header
            dataLenToRead = 1;
            data1Byte[0] = commandBytes[currDataInx];
            currDataInx += dataLenToRead;
            totalDataRead += dataLenToRead;
            tmpStr = Encoding.ASCII.GetString(data1Byte);
            retCommandStr += $@"Field Separator : 0x{BitConverter.ToString(data1Byte)}{"\r\n\r\n"}";

            fieldSeparator = data1Byte[0];

            //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            

            byte[] outFieldType2B = null;
            string outFieldTypeStr = null;
            byte[] outFieldDataLenLLLL2B = null;
            int outFieldDataLen = 0;
            byte[] outFieldData = null;
            string outFieldDataStr = null;
            byte outFieldSeparator = 0;
            int outNextDataInx = 0;
            int outTotalLenOfDataRead = 0;
            string outMsgTexts = null;
            string errorMsg = "";

            //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

            while (currDataInx < (totalMsgDataLength + 3))
            {
                fieldCount++;

                outFieldType2B = null;
                outFieldTypeStr = null;
                outFieldDataLenLLLL2B = null;
                outFieldDataLen = 0;
                outFieldData = null;
                outFieldDataStr = null;
                outFieldSeparator = 0;
                outNextDataInx = 0;
                outTotalLenOfDataRead = 0;
                outMsgTexts = null;
                errorMsg = "";

                estimatedNextBlockStartInx = GetNextFieldElementStartIndex(commandBytes, currDataInx, fieldSeparator);
                GetElementFieldData(commandBytes, currDataInx, out outFieldType2B, out outFieldTypeStr, out outFieldDataLenLLLL2B, out outFieldDataLen,
                    out outFieldData, out outFieldDataStr,
                    out outFieldSeparator, out outNextDataInx, out outTotalLenOfDataRead, out outMsgTexts);

                if (estimatedNextBlockStartInx == outNextDataInx)
                    currDataInx = outNextDataInx;

                else
                {
                    errorMsg += $@"Error ! Separator Mis-allocation; estimatedNextBlockStartInx : {estimatedNextBlockStartInx}; outNextDataInx: {outNextDataInx}{"\r\n"}";

                    if (estimatedNextBlockStartInx >= 0)
                        currDataInx = estimatedNextBlockStartInx;

                    else
                        currDataInx = outNextDataInx;
                }

                totalDataRead += outTotalLenOfDataRead;
                retCommandStr += $@"Data Field Element : {fieldCount}{"\r\n"}";
                retCommandStr += $@"========================================{"\r\n"}";
                retCommandStr += outMsgTexts + errorMsg + "\r\n";
            }


            //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            if (totalMsgDataLength == totalDataRead)
            {
                // CYA-TEST .. retCommandStr += $@"Total Data Read : {totalDataRead}{"\r\n\r\n"}";
            }
            else
            {
                retCommandStr += $@"Error ! Total Data Read is not same as Total Message Data Length ; Total Data Read : {totalDataRead}; Total Message Data Length : {totalMsgDataLength}{"\r\n\r\n"}";
            }
            //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

            // Get END OF TRANSMISSION
            data1Byte[0] = commandBytes[currDataInx];
            retCommandStr += $@"Expected <ETX> : 0x{data1Byte[0]:X2}{"\r\n"}";
            currDataInx++;

            // Get LRC
            if (currDataInx == (commandBytes.Length - 1))
            {
                data1Byte[0] = commandBytes[currDataInx];
                retCommandStr += $@"LRC : 0x{data1Byte[0]:X2}{"\r\n"}";
            }
            else
            {
                data1Byte[0] = commandBytes[commandBytes.Length - 1];
                retCommandStr += $@"Error ! Next Data Index is not LRC; Next Data Index : {currDataInx}; Last Data Index : {commandBytes.Length - 1} ; LRC : 0x{data1Byte[0]:X2}{"\r\n"}";
            }

            return retCommandStr;
        }

        public static bool Testing_HexReqRespStrToIM30Data(string hexString, out IM30DataModel im30Data, out Exception error)
        {
            im30Data = null;
            error = null;

            string retCommandStr = "\r\n";
            string hexData = hexString;
            string currHexStr = "";

            byte[] commandBytes = new byte[hexString.Length / 2];

            int commByteInx = 0;
            for (int inx = 0; inx < hexString.Length; inx += 2)
            {
                currHexStr = hexString.Substring(inx, 2);
                commandBytes[commByteInx] = Convert.ToByte(currHexStr, 16);
                commByteInx++;
            }

            bool retVal = ConvertToIM30DataModel(commandBytes, out IM30DataModel im30DataX, out Exception errorX);
            im30Data = im30DataX;
            error = errorX;

            return retVal;
        }

        public static bool ConvertToIM30DataModel(byte[] reqRespData, out IM30DataModel im30Data, out Exception error)
        {
            im30Data = null;
            error = null;
            try
            {
                im30Data = ConvertToIM30DataModelX(reqRespData, out Exception errorX);

                if (errorX != null)
                    error = errorX;
            }
            catch (Exception ex)
            {
                error = new Exception($@"'-Card Reader Error; Error occur when reading raw data in application-' {ex.Message}");
            }

            if (error != null)
            {
                im30Data = new IM30DataModel(reqRespData, error);
                return false;
            }
            else if (im30Data is null)
            {
                error = new Exception($@"'-Card Reader Error; Unknown error occur when reading raw data in application-'");
                return false;
            }
            
            return true;
            
            /////xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

            IM30DataModel ConvertToIM30DataModelX(byte[] reqRespDataX, out Exception errorX)
            {
                errorX = null;

                if ((reqRespDataX is null) || (reqRespDataX.Length < 5))
                    throw new Exception("'-Invalid Card Reader Data Length when receiving-'");

                string errorMsg = "";

                int dataLenToRead = 0;
                int currDataInx = 0;
                byte[] data4Byte = new byte[4];
                byte[] data2Byte = new byte[2];
                byte[] data1Byte = new byte[1];
                byte[] fieldData = null;
                int fieldCount = 0;
                string tmpStr = "";
                int fieldDataLength = 0;
                string fieldDataHexLenLLLL = "";
                byte calMsgDataLRC = 0;
                int estimatedNextBlockStartInx = -1;
                bool isLRCValid = false;

                // Get START TRANSMISSION
                data1Byte[0] = reqRespDataX[currDataInx];
                currDataInx++;
                byte theSTX = data1Byte[0];

                // Get Total Message Data Length (LLLL)
                dataLenToRead = 2;
                data2Byte = new byte[dataLenToRead];
                Array.Copy(reqRespDataX, currDataInx, data2Byte, 0, dataLenToRead);
                currDataInx += dataLenToRead;
                string msgDataHexLenLLLL = BitConverter.ToString(data2Byte).Replace("-", "");
                int totalMsgDataLength = int.Parse(msgDataHexLenLLLL);
                int totalDataRead = 0;

                //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                ///// Validate Data Length
                ///// .. (totalMsgDataLength + 1 (for STX) + 2 (for LLLL) + 1 (for ETX) + 1 (LRC))
                if ((totalMsgDataLength + 1 + 2 + 1 + 1) != reqRespDataX.Length)
                    throw new Exception($@"'-Card Reader Error; Invalid Data Length-' Requested Data Length : {totalMsgDataLength + 1 + 2 + 1 + 1}; Actual Data Length : {reqRespDataX.Length}");
                //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                //// Validate LRC Byte
                calMsgDataLRC = IM30Tools.GetLRC(reqRespDataX);
                isLRCValid = calMsgDataLRC == reqRespDataX[reqRespDataX.Length - 1];

                if (isLRCValid == false)
                    errorMsg += $@"'-Card Reader Error; Invalid LRC code-' Calculated Code : 0x{calMsgDataLRC:X2}; Given Code : 0x{reqRespDataX[reqRespDataX.Length - 1]:X2} {"\r\n"}";
                //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                // Transport Header
                // ====================

                // Get Transport Header Type - Transport Header
                dataLenToRead = 2;
                data2Byte = new byte[dataLenToRead];
                Array.Copy(reqRespDataX, currDataInx, data2Byte, 0, dataLenToRead);
                currDataInx += dataLenToRead;
                totalDataRead += dataLenToRead;
                tmpStr = Encoding.ASCII.GetString(data2Byte, 0, dataLenToRead);
                string transportHeaderType = tmpStr;

                // Get Transport Destination - Transport Header
                dataLenToRead = 4;
                data4Byte = new byte[dataLenToRead];
                Array.Copy(reqRespDataX, currDataInx, data4Byte, 0, dataLenToRead);
                currDataInx += dataLenToRead;
                totalDataRead += dataLenToRead;
                tmpStr = Encoding.ASCII.GetString(data4Byte, 0, dataLenToRead);
                string transportDestination = tmpStr;

                // Get Transport Source - Transport Header
                dataLenToRead = 4;
                data4Byte = new byte[dataLenToRead];
                Array.Copy(reqRespDataX, currDataInx, data4Byte, 0, dataLenToRead);
                currDataInx += dataLenToRead;
                totalDataRead += dataLenToRead;
                tmpStr = Encoding.ASCII.GetString(data4Byte, 0, dataLenToRead);
                string transportSource = tmpStr;

                // Presentation Header
                // =======================

                // Get Format Version - Presentation Header
                dataLenToRead = 1;
                data1Byte[0] = reqRespDataX[currDataInx];
                currDataInx += dataLenToRead;
                totalDataRead += dataLenToRead;
                tmpStr = Encoding.ASCII.GetString(data1Byte);
                string formatVersion = tmpStr;

                // Get Request-Response Indicator - Presentation Header
                dataLenToRead = 1;
                data1Byte[0] = reqRespDataX[currDataInx];
                currDataInx += dataLenToRead;
                totalDataRead += dataLenToRead;
                tmpStr = Encoding.ASCII.GetString(data1Byte);
                string requestResponseIndicator = tmpStr;

                // Get Transaction Code - Presentation Header
                dataLenToRead = 2;
                data2Byte = new byte[dataLenToRead];
                Array.Copy(reqRespDataX, currDataInx, data2Byte, 0, dataLenToRead);
                currDataInx += dataLenToRead;
                totalDataRead += dataLenToRead;
                tmpStr = Encoding.ASCII.GetString(data2Byte, 0, dataLenToRead);
                string transactionCode = tmpStr;

                // Get Response Code - Presentation Header
                dataLenToRead = 2;
                data2Byte = new byte[dataLenToRead];
                Array.Copy(reqRespDataX, currDataInx, data2Byte, 0, dataLenToRead);
                currDataInx += dataLenToRead;
                totalDataRead += dataLenToRead;
                tmpStr = Encoding.ASCII.GetString(data2Byte, 0, dataLenToRead);
                string responseCode = tmpStr;

                // Get More Indicator - Presentation Header
                dataLenToRead = 1;
                data1Byte[0] = reqRespDataX[currDataInx];
                currDataInx += dataLenToRead;
                totalDataRead += dataLenToRead;
                tmpStr = Encoding.ASCII.GetString(data1Byte);
                string moreIndicator = tmpStr;

                // Get Field Separator - Presentation Header
                dataLenToRead = 1;
                data1Byte[0] = reqRespDataX[currDataInx];
                currDataInx += dataLenToRead;
                totalDataRead += dataLenToRead;
                tmpStr = Encoding.ASCII.GetString(data1Byte);
                byte fieldSeparator = data1Byte[0];

                //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

                IM30DataModel retIM30Data = new IM30DataModel(
                    transportHeaderType, transportDestination, transportSource,
                    formatVersion, requestResponseIndicator, transactionCode, responseCode, moreIndicator,
                    fieldSeparator);

                //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

                byte[] outFieldType2B = null;
                string outFieldTypeStr = null;
                byte[] outFieldDataLenLLLL2B = null;
                int outFieldDataLen = 0;
                byte[] outFieldData = null;
                string outFieldDataStr = null;
                byte outFieldSeparator = 0;
                int outNextDataInx = 0;
                int outTotalLenOfDataRead = 0;
                string outMsgTexts = null;

                while (currDataInx < (totalMsgDataLength + 3))
                {
                    fieldCount++;

                    outFieldType2B = null;
                    outFieldTypeStr = null;
                    outFieldDataLenLLLL2B = null;
                    outFieldDataLen = 0;
                    outFieldData = null;
                    outFieldDataStr = null;
                    outFieldSeparator = 0;
                    outNextDataInx = 0;
                    outTotalLenOfDataRead = 0;
                    outMsgTexts = null;

                    estimatedNextBlockStartInx = GetNextFieldElementStartIndex(reqRespDataX, currDataInx, fieldSeparator);
                    GetElementFieldData(reqRespDataX, currDataInx, out outFieldType2B, out outFieldTypeStr, out outFieldDataLenLLLL2B, out outFieldDataLen,
                        out outFieldData, out outFieldDataStr,
                        out outFieldSeparator, out outNextDataInx, out outTotalLenOfDataRead, out outMsgTexts);

                    if (estimatedNextBlockStartInx == outNextDataInx)
                        currDataInx = outNextDataInx;

                    else
                    {
                        if (estimatedNextBlockStartInx >= 0)
                        {
                            errorMsg += $@"Error (A)! Separator Mis-allocation; Estimated Next FieldElement Block Start Inx : {estimatedNextBlockStartInx}; Processing Next FieldElement Data Inx: {outNextDataInx}{"\r\n"}";
                            currDataInx = estimatedNextBlockStartInx;
                        }
                        else
                        {
                            errorMsg += $@"Error (B)! Separator Mis-allocation; Estimated Next FieldElement Block Start Inx : {estimatedNextBlockStartInx}; Processing Next FieldElement Data Inx: {outNextDataInx}{"\r\n"}";
                            currDataInx = outNextDataInx;
                        }
                    }

                    totalDataRead += outTotalLenOfDataRead;

                    retIM30Data.AddFieldElement(fieldCount, outFieldTypeStr, outFieldDataStr);
                }

                //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
                if (totalMsgDataLength == totalDataRead)
                {
                    // .. By Pass ..
                }
                else
                {
                    errorMsg += $@"Error ! Total Data Read is not same as Total Message Data Length ; Total Data Read : {totalDataRead}; Total Message Data Length : {totalMsgDataLength}{"\r\n"}";
                }
                //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

                // Get END OF TRANSMISSION
                data1Byte[0] = reqRespDataX[currDataInx];
                byte theETX = data1Byte[0];
                currDataInx++;

                byte lrc = 0;
                // Get LRC
                if (currDataInx == (reqRespDataX.Length - 1))
                {
                    data1Byte[0] = reqRespDataX[currDataInx];
                    lrc = data1Byte[0];
                }
                else
                {
                    data1Byte[0] = reqRespDataX[reqRespDataX.Length - 1];
                    lrc = data1Byte[0];
                    errorMsg += $@"Error ! Processing Index not reached LRC; Processing Data Index : {currDataInx}; LRC Data Index : {reqRespDataX.Length - 1} ; LRC : 0x{data1Byte[0]:X2}{"\r\n"}";
                }

                retIM30Data.UpdateMessageDataFrame(theSTX, theETX, lrc, msgDataHexLenLLLL, reqRespDataX);

                ///// CYA-DEBUG ----------
                ////if ((_debugTestCnt <= 1) && (transactionCode?.Equals("C1") == true) && (responseCode?.Equals("00") == true) && (string.IsNullOrWhiteSpace(errorMsg)))
                ////{
                ////    errorMsg = "Test error to simulate NAK 7.";
                ////    _debugTestCnt++;
                ////}
                /////---------------------

                if (string.IsNullOrWhiteSpace(errorMsg) == false)
                    errorX = new Exception(errorMsg);

                return retIM30Data;
            }
        }

        private static int _debugTestCnt = 0;
        private static bool _debugTestEnabled = true;
        public static void DebugReset()
        {
            _debugTestCnt = 0;
        }
        
        public static void SetDebugTesting(bool debugTestEnabled)
        {
            _debugTestEnabled = debugTestEnabled;
        }

        private static int GetNextFieldElementStartIndex(byte[] fullReqRespDataX, int currDataInxX, byte fieldSeparatorX)
        {
            for (int inx = currDataInxX; inx < fullReqRespDataX.Length; inx++)
            {
                if (fullReqRespDataX[inx] == fieldSeparatorX)
                    return inx + 1;
            }
            return -1;
        }


        /// <summary>
        /// </summary>
        /// <param name="fullReqRespData">Funll Request or Response Data</param>
        /// <param name="currDataInx"></param>
        /// <param name="outFieldType2B"></param>
        /// <param name="outFieldDataLenLLLL2B"></param>
        /// <param name="outFieldDataLen"></param>
        /// <param name="outFieldData"></param>
        /// <param name="outFieldSeparator"></param>
        /// <param name="outNextDataInx"></param>
        /// <param name="outTotalLenOfDataRead"></param>
        private static void GetElementFieldData(
            byte[] fullReqRespData, int currDataInx,
            out byte[] outFieldType2B, out string outFieldTypeStr,
            out byte[] outFieldDataLenLLLL2B, out int outFieldDataLen, 
            out byte[] outFieldData, out string outFieldDataStr,
            out byte outFieldSeparator,
            out int outNextDataInx, out int outTotalLenOfDataRead,
            out string outMsgTexts)
        {
            outFieldType2B = null;
            outFieldDataLenLLLL2B = null;
            outFieldDataLen = 0;
            outFieldData = null;
            outFieldDataStr = null;
            outFieldSeparator = 0;
            outNextDataInx = currDataInx;
            outTotalLenOfDataRead = 0;
            outMsgTexts = "";

            // Get Field Type
            int dataLenToRead = 2;
            outFieldType2B = new byte[dataLenToRead];
            Array.Copy(fullReqRespData, outNextDataInx, outFieldType2B, 0, dataLenToRead);
            outNextDataInx += dataLenToRead;
            outTotalLenOfDataRead += dataLenToRead;
            outFieldTypeStr = Encoding.ASCII.GetString(outFieldType2B, 0, dataLenToRead);
            IM30FieldElementStructure feSt = FieldTypeDef.GetTypeProperties(outFieldTypeStr);
            if (feSt != null)
                outMsgTexts += $@"Field Type : '{outFieldTypeStr}'{"\t"}({feSt.FieldDescription}){"\r\n"}";
            else
                outMsgTexts += $@"Field Type : '{outFieldTypeStr}'{"\r\n"}";

            // Get Field Data Length (LLLL)
            dataLenToRead = 2;
            outFieldDataLenLLLL2B = new byte[dataLenToRead];
            Array.Copy(fullReqRespData, outNextDataInx, outFieldDataLenLLLL2B, 0, dataLenToRead);
            outNextDataInx += dataLenToRead;
            outTotalLenOfDataRead += dataLenToRead;
            string fieldDataHexLenLLLLStr = BitConverter.ToString(outFieldDataLenLLLL2B).Replace("-", "");
            outFieldDataLen = int.Parse(fieldDataHexLenLLLLStr);
            outMsgTexts += $@"Field Data Length (LLLL) : {outFieldDataLen}{"\r\n"}";

            // Get Field Data
            dataLenToRead = outFieldDataLen;
            outFieldData = new byte[dataLenToRead];
            Array.Copy(fullReqRespData, outNextDataInx, outFieldData, 0, dataLenToRead);
            outNextDataInx += dataLenToRead;
            outTotalLenOfDataRead += dataLenToRead;
            outMsgTexts += $@"Field Data (HEX) : {BitConverter.ToString(outFieldData)}{"\r\n"}";
            outFieldDataStr = System.Text.Encoding.ASCII.GetString(outFieldData);
            outMsgTexts += $@"Field Data (Text) : '{outFieldDataStr}'{"\r\n"}";

            // Get Field Separator
            dataLenToRead = 1;
            outFieldSeparator = fullReqRespData[outNextDataInx];
            outNextDataInx += dataLenToRead;
            outTotalLenOfDataRead += dataLenToRead;
            outMsgTexts += $@"0x{outFieldSeparator:X2}{"\r\n"}";
        }

        public static IM30DataModel CreateNewMessageData(
            string requestResponseIndicator, string transactionCode, string responseCode, string moreIndicator,
            string transportHeaderType = "60", string transportDestination = "0000", string transportSource = "0000",
            string formatVersion = "1")
        {
            return new IM30DataModel(transportHeaderType, transportDestination, transportSource, formatVersion, requestResponseIndicator, transactionCode, responseCode, moreIndicator);
        }

        private static Dictionary<int, byte[]> _fieldElementsCollection = new Dictionary<int, byte[]>();
        public static byte[] RenderData(IM30DataModel msgData)
        {
            _fieldElementsCollection.Clear();

            byte[] retRenderData = null;

            //------------------------------------------------------
            //.. Data - Starting ..
            byte[] stxData = new byte[1] {_stx};
            byte[] msgDataLengthLLLLData = new byte[2];

            //.. Data - 10 bytes for TRANSPORT HEADER
            byte[] transportHeaderTypeData = System.Text.Encoding.ASCII.GetBytes(msgData.TransportHeaderType);
            byte[] transportDestinationData = System.Text.Encoding.ASCII.GetBytes(msgData.TransportDestination);
            byte[] transportSourceData = System.Text.Encoding.ASCII.GetBytes(msgData.TransportSource);

            //.. Data - 8 bytes for PRESENTATION HEADER
            byte[] formatVersionData = System.Text.Encoding.ASCII.GetBytes(msgData.FormatVersion);
            byte[] requestResponseIndicatorData = System.Text.Encoding.ASCII.GetBytes(msgData.RequestResponseIndicator);
            byte[] transactionCodeData = System.Text.Encoding.ASCII.GetBytes(msgData.TransactionCode);
            byte[] responseCodeData = System.Text.Encoding.ASCII.GetBytes(msgData.ResponseCode);
            byte[] moreIndicatorData = System.Text.Encoding.ASCII.GetBytes(msgData.MoreIndicator);
            byte[] fieldSeparatorData = new byte[1] {msgData.FieldSeparator};

            //.. Data - Field Elements
            int fieldElementCount = 0;
            int lastFieldElementCount = 0;
            int totalFieldElementDataSize = 0;
            IM30FieldElementModel[] fieldElementArr = msgData.FieldElementCollection.OrderBy(x => x.SequenceNo).ToArray();
            byte[] fieldEleData = null;
            foreach (IM30FieldElementModel fieldElement in fieldElementArr)
            {
                fieldElementCount++;
                fieldEleData = fieldElement.GetRenderedData();
                totalFieldElementDataSize += fieldEleData.Length;
                _fieldElementsCollection.Add(fieldElementCount, fieldEleData);
            }
            lastFieldElementCount = fieldElementCount;

            // .. Data - Ending ..
            byte[] etxData = new byte[1] { _etx };
            byte[] lrcData = new byte[1];

            //------------------------------------------------------
            // Put all data into retRenderData
            retRenderData = new byte[1 + 2 + 10 + 8 + totalFieldElementDataSize + 1 + 1];

            int startCopyIndex = 0;
            System.Buffer.BlockCopy(stxData, 0, retRenderData, startCopyIndex, stxData.Length);
            startCopyIndex += stxData.Length;
            System.Buffer.BlockCopy(msgDataLengthLLLLData, 0, retRenderData, startCopyIndex, msgDataLengthLLLLData.Length);
            startCopyIndex += msgDataLengthLLLLData.Length;

            System.Buffer.BlockCopy(transportHeaderTypeData, 0, retRenderData, startCopyIndex, transportHeaderTypeData.Length);
            startCopyIndex += transportHeaderTypeData.Length;
            System.Buffer.BlockCopy(transportDestinationData, 0, retRenderData, startCopyIndex, transportDestinationData.Length);
            startCopyIndex += transportDestinationData.Length;
            System.Buffer.BlockCopy(transportSourceData, 0, retRenderData, startCopyIndex, transportSourceData.Length);
            startCopyIndex += transportSourceData.Length;

            System.Buffer.BlockCopy(formatVersionData, 0, retRenderData, startCopyIndex, formatVersionData.Length);
            startCopyIndex += formatVersionData.Length;
            System.Buffer.BlockCopy(requestResponseIndicatorData, 0, retRenderData, startCopyIndex, requestResponseIndicatorData.Length);
            startCopyIndex += requestResponseIndicatorData.Length;
            System.Buffer.BlockCopy(transactionCodeData, 0, retRenderData, startCopyIndex, transactionCodeData.Length);
            startCopyIndex += transactionCodeData.Length;
            System.Buffer.BlockCopy(responseCodeData, 0, retRenderData, startCopyIndex, responseCodeData.Length);
            startCopyIndex += responseCodeData.Length;
            System.Buffer.BlockCopy(moreIndicatorData, 0, retRenderData, startCopyIndex, moreIndicatorData.Length);
            startCopyIndex += moreIndicatorData.Length;
            System.Buffer.BlockCopy(fieldSeparatorData, 0, retRenderData, startCopyIndex, fieldSeparatorData.Length);
            startCopyIndex += fieldSeparatorData.Length;

            for(int fieldCount = 1; fieldCount <= lastFieldElementCount; fieldCount++)
            {
                fieldEleData = null;
                if (_fieldElementsCollection.TryGetValue(fieldCount, out fieldEleData) == false)
                {
                    throw new Exception($@"'-Card Reader Error; Fail to allocate buffer data when filling Render Data container in Command App.-' (Error Count : {fieldCount})");
                }

                System.Buffer.BlockCopy(fieldEleData, 0, retRenderData, startCopyIndex, fieldEleData.Length);
                startCopyIndex += fieldEleData.Length;
            }

            System.Buffer.BlockCopy(etxData, 0, retRenderData, startCopyIndex, etxData.Length);
            startCopyIndex += etxData.Length;
            System.Buffer.BlockCopy(lrcData, 0, retRenderData, startCopyIndex, lrcData.Length);
            startCopyIndex += lrcData.Length;


            //------------------------------------------------------
            // Eva. Message Data Length LLLL 
            int msgDataLength = 10 + 8 + totalFieldElementDataSize;
            string msgDataLengthHexStr = msgDataLength.ToString().Trim().PadLeft(4, '0');
            byte[] tmpMsgDataLengthLLLLData = IM30Tools.Get2BytesDataFromBCD(msgDataLengthHexStr);

            retRenderData[1] = tmpMsgDataLengthLLLLData[0];
            retRenderData[2] = tmpMsgDataLengthLLLLData[1];
            //------------------------------------------------------
            // Eva. LRC
            byte aLRC = IM30Tools.GetLRC(retRenderData);

            //CYA-DEBUG .. Testing
            ////if ((msgData.TransactionCode?.ToString().Equals("C1") == true) && (_debugTestEnabled))
            ////{
            ////    retRenderData[retRenderData.Length - 1] = 0;
            ////}
            ////else
            //---------------------------------
                retRenderData[retRenderData.Length - 1] = aLRC;
            //------------------------------------------------------
            msgData.UpdateMessageDataFrame(stxData[0], etxData[0], aLRC, msgDataLengthHexStr, retRenderData);
            _fieldElementsCollection.Clear();

            return retRenderData;
        }
    }
}