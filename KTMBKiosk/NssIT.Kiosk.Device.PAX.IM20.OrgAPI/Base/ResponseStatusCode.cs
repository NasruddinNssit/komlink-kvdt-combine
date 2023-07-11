using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Device.PAX.IM20.OrgAPI.Base
{
	public class ResponseStatusCode
	{
		/// <summary>
		///		APPROVED - Transaction Successful
		/// </summary>
		public static string Success = "00";

		/// <summary>
		///		Amount not matched during Void and Adjust
		/// </summary>
		public static string AE = "AE";

		/// <summary>
		///		Batch Not found during settlement on Host
		/// </summary>
		public static string BU = "BU";

		/// <summary>
		///		Comms Error or Connection Timeout. Please retry
		/// </summary>
		public static string CE = "CE";

		/// <summary>
		///		Card Expired
		/// </summary>
		public static string EC = "EC";

		/// <summary>
		///		File error, empty
		/// </summary>
		public static string FE = "FE";

		/// <summary>
		///		Host number Error applicable where msg includes host number field
		/// </summary>
		public static string HE = "HE";

		/// <summary>
		///		Invalid Card Swiped / Card Not Supported
		/// </summary>
		public static string IC = "IC";

		/// <summary>
		///		Line Error ,No phone line
		/// </summary>
		public static string LE = "LE";

		/// <summary>
		///		Pin Entry Error
		/// </summary>
		public static string PE = "PE";

		/// <summary>
		///		Record error, Trace# not found during Void or Sale Comp
		/// </summary>
		public static string RE = "RE";

		/// <summary>
		///		Terminal Full
		/// </summary>
		public static string SE = "SE";

		/// <summary>
		///		Transaction Aborted
		/// </summary>
		public static string TA = "TA";

		/// <summary>
		///		Transaction already Voided
		/// </summary>
		public static string VB = "VB";

		/// <summary>
		///		Card Number Not Matched
		/// </summary>
		public static string WC = "WC";

		/// <summary>
		///		Zero Amount Settlement/No transaction
		/// </summary>
		public static string ZE = "ZE";

		/// <summary>
		///		REFER TO CARD ISSUER
		/// </summary>
		public static string Code01 = "01";

		/// <summary>
		///		REFER TO CARD ISSUER’S SPECIAL CONDITION
		/// </summary>
		public static string Code02 = "02";

		/// <summary>
		///		INVALID MERCHANT
		/// </summary>
		public static string Code03 = "03";

		/// <summary>
		///		DECLINED
		/// </summary>
		public static string Code05 = "05";

		/// <summary>
		///		INVALID TRANSACTION
		/// </summary>
		public static string Code12 = "12";

		/// <summary>
		///		INVALID AMOUNT
		/// </summary>
		public static string Code13 = "13";

		/// <summary>
		///		INVALID CARD NUMBER
		/// </summary>
		public static string Code14 = "14";

		/// <summary>
		///		RE-ENTER TRANSACTION
		/// </summary>
		public static string Code19 = "19";

		/// <summary>
		///		UNABLE TO LOCATE RECORD ON FILE
		/// </summary>
		public static string Code25 = "25";

		/// <summary>
		///		FORMAT ERROR
		/// </summary>
		public static string Code30 = "30";

		/// <summary>
		///		BANK NOT SUPPORTED BY SWITCH
		/// </summary>
		public static string Code31 = "31";

		/// <summary>
		///		LOST CARD
		/// </summary>
		public static string Code41 = "41";

		/// <summary>
		///		STOLEN CARD PICK UP
		/// </summary>
		public static string Code43 = "43";

		/// <summary>
		///		NOT SUFFICIENT FUNDS
		/// </summary>
		public static string Code51 = "51";

		/// <summary>
		///		EXPIRED CARD
		/// </summary>
		public static string Code54 = "54";

		/// <summary>
		///		INCORRECT PIN
		/// </summary>
		public static string Code55 = "55";

		/// <summary>
		///		TRANSACTION NOT PERMITTED IN TERMINAL
		/// </summary>
		public static string Code58 = "58";

		/// <summary>
		///		INVALID PRODUCT CODES
		/// </summary>
		public static string Code76 = "76";

		/// <summary>
		///		RECONCILE ERROR
		/// </summary>
		public static string Code77 = "77";

		/// <summary>
		///		TRACE# NOT FOUND
		/// </summary>
		public static string Code78 = "78";

		/// <summary>
		///		BATCH NUMBER NOT FOUND
		/// </summary>
		public static string Code80 = "80";

		/// <summary>
		///		BAD TERMINAL ID
		/// </summary>
		public static string Code89 = "89";

		/// <summary>
		///		ISSUER/SWITCH INOPERATIVE
		/// </summary>
		public static string Code91 = "91";

		/// <summary>
		///		DUPLICATE TRANSMISSION
		/// </summary>
		public static string Code94 = "94";

		/// <summary>
		///		BATCH UPLOAD
		/// </summary>
		public static string Code95 = "95";

		/// <summary>
		///		SYSTEM MALFUNCTION
		/// </summary>
		public static string Code96 = "96";

		/// <summary>
		///		EMV APPROVED
		/// </summary>
		public static string Y1 = "Y1";

		/// <summary>
		///		EMV APPROVED
		/// </summary>
		public static string Y3 = "Y3";

		/// <summary>
		///		EMV DECLINED
		/// </summary>
		public static string Z1 = "Z1";

		/// <summary>
		///		EMV DECLINED
		/// </summary>
		public static string Z3 = "Z3";

		public static string TranslateCode(string statusCode)
		{
			statusCode = (statusCode ?? "").ToUpper().Trim();
			string retDesc = null;

			if (statusCode.Equals("00"))
				retDesc = @"Transaction Successful";
			else if (statusCode.Equals("AE"))
				retDesc = @"Amount not matched during Void and Adjust";
			else if (statusCode.Equals("BU"))
				retDesc = @"Batch Not found during settlement on Host";
			else if (statusCode.Equals("CE"))
				retDesc = @"Comms Error or Connection Timeout. Please retry later";
			else if (statusCode.Equals("EC"))
				retDesc = @"Card Expired";
			else if (statusCode.Equals("FE"))
				retDesc = @"File error, empty";
			else if (statusCode.Equals("HE"))
				retDesc = @"Host number Error applicable where msg includes host number field";
			else if (statusCode.Equals("IC"))
				retDesc = @"Invalid Card Swiped / Card Not Supported";
			else if (statusCode.Equals("LE"))
				retDesc = "Line Error ,No phone line";
			else if (statusCode.Equals("PE"))
				retDesc = "Pin Entry Error";
			else if (statusCode.Equals("RE"))
				retDesc = "Record error ,Trace# not found during Void or Sale Comp.";
			else if (statusCode.Equals("SE"))
				retDesc = "Terminal Full";
			else if (statusCode.Equals("TA"))
				retDesc = "Transaction Aborted";
			else if (statusCode.Equals("VB"))
				retDesc = "Transaction already Voided";
			else if (statusCode.Equals("WC"))
				retDesc = "Card Number Not Matched";
			else if (statusCode.Equals("Y1"))
				retDesc = @"EMV APPROVED";
			else if (statusCode.Equals("Y3"))
				retDesc = @"EMV APPROVED";
			else if (statusCode.Equals("ZE"))
				retDesc = "Zero Amount Settlement/No transaction";
			else if (statusCode.Equals("Z1"))
				retDesc = @"EMV DECLINED";
			else if (statusCode.Equals("Z3"))
				retDesc = @"EMV DECLINED";
			else if (statusCode.Equals("01"))
				retDesc = @"REFER TO CARD ISSUER";
			else if (statusCode.Equals("02"))
				retDesc = @"REFER TO CARD ISSUER’S SPECIAL CONDITION";
			else if (statusCode.Equals("03"))
				retDesc = @"INVALID MERCHANT";
			else if (statusCode.Equals("05"))
				retDesc = @"DECLINED";
			else if (statusCode.Equals("12"))
				retDesc = @"INVALID TRANSACTION";
			else if (statusCode.Equals("13"))
				retDesc = @"INVALID AMOUNT";
			else if (statusCode.Equals("14"))
				retDesc = @"INVALID CARD NUMBER";
			else if (statusCode.Equals("19"))
				retDesc = @"RE-ENTER TRANSACTION";
			else if (statusCode.Equals("25"))
				retDesc = @"UNABLE TO LOCATE RECORD ON FILE";
			else if (statusCode.Equals("30"))
				retDesc = @"FORMAT ERROR";
			else if (statusCode.Equals("31"))
				retDesc = @"BANK NOT SUPPORTED BY SWITCH";
			else if (statusCode.Equals("41"))
				retDesc = @"LOST CARD";
			else if (statusCode.Equals("43"))
				retDesc = @"STOLEN CARD PICK UP";
			else if (statusCode.Equals("46"))
				retDesc = @"BANK REJECTED;OVER USED;NEED TO CONTACT BANK";
			else if (statusCode.Equals("51"))
				retDesc = @"NOT SUFFICIENT FUNDS";
			else if (statusCode.Equals("54"))
				retDesc = @"EXPIRED CARD";
			else if (statusCode.Equals("55"))
				retDesc = @"INCORRECT PIN";
			else if (statusCode.Equals("58"))
				retDesc = @"TRANSACTION NOT PERMITTED IN TERMINAL";
			else if (statusCode.Equals("61"))
				retDesc = @"Exceeds Withdrawal Amount Limits";
			else if (statusCode.Equals("76"))
				retDesc = @"CARD MACHINE-INVALID PRODUCT CODES";
			else if (statusCode.Equals("77"))
				retDesc = @"RECONCILE ERROR";
			else if (statusCode.Equals("78"))
				retDesc = @"CARD MACHINE-TRACE# NOT FOUND";
			else if (statusCode.Equals("80"))
				retDesc = @"CARD MACHINE-BATCH NUMBER NOT FOUND";
			else if (statusCode.Equals("89"))
				retDesc = @"BAD TERMINAL ID";
			else if (statusCode.Equals("91"))
				retDesc = @"CARD MACHINE-ISSUER/SWITCH INOPERATIVE";
			else if (statusCode.Equals("94"))
				retDesc = @"CARD MACHINE-DUPLICATE TRANSMISSION";
			else if (statusCode.Equals("95"))
				retDesc = @"CARD MACHINE-BATCH UPLOAD";
			else if (statusCode.Equals("96"))
				retDesc = @"CARD MACHINE-SYSTEM MALFUNCTION";
			else
				retDesc = $@"Unable to translate Status code ({statusCode})";

			return retDesc;
		}

	}

}
