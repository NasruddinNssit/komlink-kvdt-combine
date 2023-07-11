using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.IM30.Base
{
    public class IM30Tools
    {
        /// <summary>
        /// Convert a maximum 4 digits number based on Binary Coded Decimal (BCD) to 2 data bytes array. And let the Most Significant Byte Transmitted first
        /// </summary>
        /// <param name="BCDNumber"></param>
        /// <returns></returns>
        public static byte[] Get2BytesDataFromBCD(string BCDNumber)
        {
            byte[] retData = null;
            int bcdNumberInt = 0;

            if (string.IsNullOrWhiteSpace(BCDNumber))
                throw new Exception("-Card Reader Error; Invalid BCD Number; The value cannot be null or empty-");

            if (int.TryParse(BCDNumber, out bcdNumberInt) == false)
            {
                throw new Exception($@"-Card Reader Error; Invalid BCD Number; The value is not a valid integer number-({BCDNumber})-");
            }
            else
            {
                if (bcdNumberInt > 9999)
                    throw new Exception($@"-Card Reader Error; Invalid BCD Number; The value should not more than 9999-({BCDNumber})-");
                else if (bcdNumberInt < 0)
                    throw new Exception($@"-Card Reader Error; Invalid BCD Number; The value should more than 0-({BCDNumber})-");
            }

            ///// Note : Most Significant Byte Transmitted first. Like 0x0012. 0x00 will be transmitted first then follow by 0x12.
            string hexStr = bcdNumberInt.ToString().Trim().PadLeft(4, '0');
            retData = new byte[2];
            int inx = 0;
            for (int position = 0 ; position < 4 ; position+=2)
            {
                retData[inx] = Convert.ToByte(hexStr.Substring(position, 2), 16);
                inx++;
            }
            
            return retData;
        }

        public static byte GetLRC(byte[] fullRequestResponseData)
        {
            byte retLRC = fullRequestResponseData[1];

            for (int inx = 2; inx <= (fullRequestResponseData.Length - 2); inx++)
                retLRC ^= fullRequestResponseData[inx];

            return retLRC;
        }

		public static string TranslateAsciiCode(byte code)
        {
			string retStr = $@"{code:X2}";

			switch (code)
			{
				case 0x00: return "<NUL>"; 
				case 0x01: return "<SOH>";
				case 0x02: return "<STX>";
				case 0x03: return "<ETX>";
				case 0x04: return "<EOT>";
				case 0x05: return "<ENQ>";
				case 0x06: return "<ACK>";
				case 0x07: return "<BEL>";
				case 0x08: return "<BS>";
				case 0x09: return "<HT>";
				case 0x0A: return "<LF>";
				case 0x0B: return "<VT>";
				case 0x0C: return "<FF>";
				case 0x0D: return "<CR>";
				case 0x0E: return "<SO>";
				case 0x0F: return "<SI>";
				case 0x10: return "<DLE>";
				case 0x11: return "<DC1>";
				case 0x12: return "<DC2>";
				case 0x13: return "<DC3>";
				case 0x14: return "<DC4>";
				case 0x15: return "<NAK>";
				case 0x16: return "<SYN>";
				case 0x17: return "<ETB>";
				case 0x18: return "<CAN>";
				case 0x19: return "<EM>";
				case 0x1A: return "<SUB>";
				case 0x1B: return "<ESC>";
				case 0x1C: return "<FS>";
				case 0x1D: return "<GS>";
				case 0x1E: return "<RS>";
				case 0x1F: return "<US>";
				case 0x7F: return "<DEL>";
				default:
					return $@"{code:X2}";
			}
		}

		public static string AsciiOctets2String(byte[] bytes)
		{
			try
			{
				StringBuilder sb = new StringBuilder(bytes.Length);
				foreach (char c in System.Text.Encoding.UTF8.GetString(bytes).ToCharArray())
				{
					switch (c)
					{
						case '\u0000': sb.Append("<NUL>"); break;
						case '\u0001': sb.Append("<SOH>"); break;
						case '\u0002': sb.Append("<STX>"); break;
						case '\u0003': sb.Append("<ETX>"); break;
						case '\u0004': sb.Append("<EOT>"); break;
						case '\u0005': sb.Append("<ENQ>"); break;
						case '\u0006': sb.Append("<ACK>"); break;
						case '\u0007': sb.Append("<BEL>"); break;
						case '\u0008': sb.Append("<BS>"); break;
						case '\u0009': sb.Append("<HT>"); break;
						case '\u000A': sb.Append("<LF>"); break;
						case '\u000B': sb.Append("<VT>"); break;
						case '\u000C': sb.Append("<FF>"); break;
						case '\u000D': sb.Append("<CR>"); break;
						case '\u000E': sb.Append("<SO>"); break;
						case '\u000F': sb.Append("<SI>"); break;
						case '\u0010': sb.Append("<DLE>"); break;
						case '\u0011': sb.Append("<DC1>"); break;
						case '\u0012': sb.Append("<DC2>"); break;
						case '\u0013': sb.Append("<DC3>"); break;
						case '\u0014': sb.Append("<DC4>"); break;
						case '\u0015': sb.Append("<NAK>"); break;
						case '\u0016': sb.Append("<SYN>"); break;
						case '\u0017': sb.Append("<ETB>"); break;
						case '\u0018': sb.Append("<CAN>"); break;
						case '\u0019': sb.Append("<EM>"); break;
						case '\u001A': sb.Append("<SUB>"); break;
						case '\u001B': sb.Append("<ESC>"); break;
						case '\u001C': sb.Append("<FS>"); break;
						case '\u001D': sb.Append("<GS>"); break;
						case '\u001E': sb.Append("<RS>"); break;
						case '\u001F': sb.Append("<US>"); break;
						case '\u007F': sb.Append("<DEL>"); break;
						default:
							if (c > '\u007F')
							{
								sb.AppendFormat(@"\u{0:X4}", (ushort)c); // in ASCII, any octet in the range 0x80-0xFF doesn't have a character glyph associated with it
							}
							else
							{
								sb.Append(c);
							}
							break;
					}
				}
				return sb.ToString();

			}
			catch (Exception e)
			{
				return "";
			}
		}
	}
}
