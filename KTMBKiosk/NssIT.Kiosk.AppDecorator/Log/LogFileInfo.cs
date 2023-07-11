using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Log
{
	public delegate void ShowMessageLogDelg(string accessResult);

	public class LogFileInfo : IDisposable
	{
		public DateTime Time;
		/// <summary>
		///		Message String
		/// </summary>
		public string MsgXStr;
		/// <summary>
		///		Message Object
		/// </summary>
		public object MsgXObj = null;
		/// <summary>
		///		Message String For Admin Support
		/// </summary>
		public string AdminMsg;

		/// <summary>
		///		Channel / File Name
		/// </summary>
		public string Channel;

		/// <summary>
		///		Location Tag
		/// </summary>
		public string LoctXTag;
		/// <summary>
		///		Message Type refer to MesageType
		/// </summary>
		public MessageType MsgXType = MessageType.Info;
		/// <summary>
		///		Process ID ; Used to indicated a bunch of working.
		/// </summary>
		public string ProXId;

		/// <summary>
		///		Net Process ID ; Used to identify a network (like TCP) data transaction.
		/// </summary>
		public Guid? NetProcessId = null;

		/// <summary>
		///		Class & Method Tag
		/// </summary>
		public string ClsMetXName;

		public void Dispose()
		{
			MsgXStr = null;
			MsgXObj = null;
			AdminMsg = null;
			Channel = null;
			ProXId = null;
			NetProcessId = null;
			ClsMetXName = null;
			LoctXTag = null;
		}

		public void SetDefaultIfEmpty()
		{
			MsgXStr = (string.IsNullOrWhiteSpace(MsgXStr)) ? null : MsgXStr.Trim();
			AdminMsg = (string.IsNullOrWhiteSpace(AdminMsg)) ? null : AdminMsg.Trim();
			ProXId = (string.IsNullOrWhiteSpace(ProXId)) ? null : ProXId.Trim();
			ClsMetXName = (string.IsNullOrWhiteSpace(ClsMetXName)) ? null : ClsMetXName.Trim();
			LoctXTag = (string.IsNullOrWhiteSpace(LoctXTag)) ? null : LoctXTag.Trim();
			Channel = (string.IsNullOrWhiteSpace(Channel)) ? null : Channel.Trim();
		}

		public string ToJSonString()
		{
			SetDefaultIfEmpty();

			return JsonConvert.SerializeObject(this);
		}

		public string ToClassicalKioskMsg()
		{
			string retStr = "";
			SetDefaultIfEmpty();

			retStr = $@"{Time.ToString("HH:mm:ss")} - (C) {LoctXTag ?? ""} : {MsgXStr ?? ""} {(ProXId ?? ""),50}";

			return retStr;
		}

		public string GetMsgXObjJSonStr()
		{
			string rStr = "";

			try
			{
				rStr = (MsgXObj != null) ? JsonConvert.SerializeObject(MsgXObj) : null;
			}
			catch (Exception ex)
			{
				string typeStr = "";
				try
				{
					typeStr = MsgXObj?.GetType()?.FullName;
				}
				catch (Exception ex2)
				{
					typeStr = $@"Unable to read the data type name (X); {ex2?.Message}";
				}
				rStr = $@"Error..; Suspect loop-back data structure; {ex?.Message}; Type: {typeStr}; (86B39819-DF3C-45C4-A7F6-5A6103477650)";
			}
			return rStr;
		}
	}

	public enum MessageType
	{
		[Description("Information")]
		Info = 0,
		[Description("Warning")]
		Warning = 1,
		[Description("Error")]
		Error = 2,
		[Description("Fatal")]
		Fatal = 3,
		[Description("Marking")]
		Marking = 5,
		[Description("Debug")]
		Debug = 100
	}

	public enum LogMode
	{
		// High Level Message / End User Message; 
		[Description("Classical_Kiosk")]
		ClassicKiosk = 0,

		// All In JSon Format 
		[Description("JSon")]
		AllInJSon = 1
	}
}
