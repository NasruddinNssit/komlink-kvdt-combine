using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Device.PAX.IM20.OrgAPI.Base
{
	public class PayECSData
	{
		public string ProcessId { get; set; }
		public string Command { get; set; }
		public string Host { get; set; }
		public string Amount { get; set; }
		/// <summary>
		///		For Sale, this is an Additional Data
		/// </summary>
		public string QrId { get; set; }
		public string QrNo { get; set; }
		public string DocNumbers { get; set; }
		public bool ProcessDone { get; set; } = false;

		public int FirstWaitingTimeSec { get; set; } = 60;
		public int MaxWaitingSec { get; set; } = 120;

		public bool HasCommand { get { return ((Command ?? "").Trim().Length > 0); } }

		public AccountType AccType { get; set; } = AccountType.CreditCard;

		public void Reset()
		{
			ProcessId = null;
			Command = null;
			Host = null;
			Amount = null;
			QrId = null;
			QrNo = null;
		}

		public PayECSData Duplicate()
        {
			return new PayECSData()
			{
				ProcessId = this.ProcessId,
				AccType = this.AccType,
				Amount = this.Amount,
				Command = this.Command,
				DocNumbers = this.DocNumbers,
				FirstWaitingTimeSec = this.FirstWaitingTimeSec,
				Host = this.Host,
				MaxWaitingSec = this.MaxWaitingSec,
				ProcessDone = this.ProcessDone,
				QrId = this.QrId,
				QrNo = this.QrNo
			};
        }
	}

	public enum AccountType
	{
		CreditCard = 0,
		/// <summary>
		/// (Not in Use)
		/// </summary>
		Saving = 1,
		/// <summary>
		/// (Not in Use)
		/// </summary>
		Current = 2,
		/// <summary>
		/// For built in scanner
		/// </summary>
		EWallets = 3,
		/// <summary>
		/// Payment type will be selected in terminal (IM20 only)
		/// </summary>
		TerminalSelection = 4
	}
}
