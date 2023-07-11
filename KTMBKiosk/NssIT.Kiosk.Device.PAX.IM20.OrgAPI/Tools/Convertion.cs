using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Device.PAX.IM20.OrgAPI.Tools
{
	public class Convertion
	{
		public static string ToECRCurrencyString(string currStr, int decimalPoint = 2)
		{
			decimalPoint = (decimalPoint < 0) ? 0 : decimalPoint;

			string currencyStr = (currStr ?? "").Trim();
			string retStr = "";
			decimal decVal = 0;

			if (decimal.TryParse(currencyStr, out decVal))
			{
				decVal = Math.Round(decVal, decimalPoint);
				decVal = decVal * (decimal)(Math.Pow(10D, (double)decimalPoint));
				decVal = Math.Floor(decVal);
				retStr = decVal.ToString();
			}
			else
			{
				throw new System.Exception($@"Invalid currency ({currencyStr}) convertion");
			}

			return retStr;
		}

		public static decimal ToECRCurrency(string currStr, int decimalPoint = 2)
		{
			string currencyStr = (currStr ?? "").Trim();
			decimal retVal = 0M;
			decimal decAmt = 0M;

			if (decimal.TryParse(currStr, out decAmt))
			{
				retVal = decAmt / ((decimal)(Math.Pow(10D, (double)decimalPoint)));
			}
			else
			{
				throw new System.Exception($@"Invalid ECR currency ({currencyStr}) convertion");
			}

			return retVal;
		}
	}
}