using NssIT.Kiosk.AppDecorator.Common.AppService.Instruction;
using NssIT.Kiosk.AppDecorator.Common.AppService.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService.Sales.UI
{
	[Serializable]
	public class UISalesPaymentSubmission : IKioskMsg, INetCommandDirective
	{
		public Guid BaseNetProcessId { get; private set; }
		public Guid? RefNetProcessId { get; private set; } = null;
		public string ProcessId { get; private set; }
		public DateTime TimeStamp { get; private set; } = DateTime.MinValue;
		public AppModule Module { get; } = AppModule.UIKioskSales;
		public string ModuleDesc { get => Enum.GetName(typeof(AppModule), Module); }
		public dynamic GetMsgData() => NotApplicable.Object;
		public CommInstruction Instruction { get; } = (CommInstruction)UISalesInst.SalesPaymentSubmission;
		public string InstructionDesc { get => Enum.GetName(typeof(CommInstruction), Instruction); }
		public string ErrorMessage { get; set; } = null;
		public CommunicationDirection CommuCommandDirection { get; } = CommunicationDirection.SendOneResponseOne;

		//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
		public string SeatBookingId { get; private set; } = null;
		public decimal TotalAmount { get; private set; } = 0M;
		public string TradeCurrency { get; private set; }
		public PaymentType TypeOfPayment { get; private set; } = PaymentType.Unknown;
		//public PaymentResult PaymentState { get; private set; } = PaymentResult.Unknown;
		//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
		// Credit Card
		public string BankReferenceNo { get; private set; }
		public CreditCardResponse CreditCardAnswer { get; private set; }

		//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
		// B2B Cash Status
		public int Cassette1NoteCount { get; private set; }
		public int Cassette2NoteCount { get; private set; }
		public int Cassette3NoteCount { get; private set; }
		public int RefundCoinAmount { get; private set; }

		//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
		// For 'Boost / Touch n Go' Payment transaction
		
		//Unique sale number. 
		public string BTnGSaleTransactionNo { get; private set; }
		/// <summary>
		/// Refer to KTMBCTS table PaymentGatewayMappings.PaymentMethod; Or NssIT.Train.Kiosk.Common.Constants.FinancePaymentMethod.
		/// </summary>
		public string PaymentMethod { get; private set; }		
		//==================================================================

		/// <summary>
		/// Submission for Credit Card
		/// </summary>
		/// <param name="processId"></param>
		/// <param name="timeStamp"></param>
		/// <param name="seatBookingId"></param>
		/// <param name="totalAmount"></param>
		/// <param name="tradeCurrency"></param>
		/// <param name="typeOfPayment"></param>
		/// <param name="bankReferenceNo"></param>
		/// <param name="creditCardAnswer"></param>
		public UISalesPaymentSubmission(string processId, DateTime timeStamp, string seatBookingId, decimal totalAmount, 
			string tradeCurrency, string bankReferenceNo, CreditCardResponse creditCardAnswer)
		{
			BaseNetProcessId = Guid.NewGuid();
			RefNetProcessId = BaseNetProcessId;
			ProcessId = processId;
			TimeStamp = timeStamp;
			SeatBookingId = seatBookingId;
			TotalAmount = totalAmount;
			TypeOfPayment = PaymentType.CreditCard;
			TradeCurrency = tradeCurrency;
			BankReferenceNo = bankReferenceNo;
			CreditCardAnswer = creditCardAnswer;
			PaymentMethod = "CreditCard";
		}

		/// <summary>
		/// Submission for Payment Gateway (eWallet)
		/// </summary>
		/// <param name="processId"></param>
		/// <param name="timeStamp"></param>
		/// <param name="seatBookingId"></param>
		/// <param name="totalAmount"></param>
		/// <param name="tradeCurrency"></param>
		public UISalesPaymentSubmission(string processId, DateTime timeStamp, string seatBookingId, decimal totalAmount,
			string tradeCurrency, string bTnGSaleTransactionNo, string paymentMethod)
		{
			BaseNetProcessId = Guid.NewGuid();
			RefNetProcessId = BaseNetProcessId;
			ProcessId = processId;
			TimeStamp = timeStamp;
			SeatBookingId = seatBookingId;
			TotalAmount = totalAmount;
			TypeOfPayment = PaymentType.PaymentGateway;
			TradeCurrency = tradeCurrency;
			BTnGSaleTransactionNo = bTnGSaleTransactionNo;
			PaymentMethod = paymentMethod;
		}

		public void Dispose()
		{ }
	}
}
