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
	public class UIKomuterCompletePaymentSubmission : IKioskMsg, INetCommandDirective
	{
		public Guid BaseNetProcessId { get; private set; }
		public Guid? RefNetProcessId { get; private set; } = null;
		public string ProcessId { get; private set; }
		public DateTime TimeStamp { get; private set; } = DateTime.MinValue;

		public AppModule Module { get; } = AppModule.UIKioskSales;
		public string ModuleDesc { get => Enum.GetName(typeof(AppModule), Module); }
		public dynamic GetMsgData() => NotApplicable.Object;
		public CommInstruction Instruction { get; } = (CommInstruction)UISalesInst.KomuterCompletePaymentSubmission;
		public string InstructionDesc { get => Enum.GetName(typeof(CommInstruction), Instruction); }

		public string ErrorMessage { get; set; } = null;

		public CommunicationDirection CommuCommandDirection { get; } = CommunicationDirection.SendOneResponseOne;

		public string BookingId { get; set; }
		public string CurrencyId { get; set; }
		public decimal Amount { get; set; }
		public string FinancePaymentMethod { get; set; }
		public PaymentType TypeOfPayment { get; private set; } = PaymentType.Unknown;

		public CreditCardResponse CreditCardAnswer { get; private set; }
		//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
		// For 'Boost / Touch n Go' Payment transaction
		public string BTnGSaleTransactionNo { get; private set; }
		//==================================================================

		public UIKomuterCompletePaymentSubmission(string processId, DateTime timeStamp,
			string bookingId,
			string currencyId,
			decimal amount,
			string financePaymentMethod, CreditCardResponse creditCardAnswer)
		{
			BaseNetProcessId = Guid.NewGuid();
			RefNetProcessId = BaseNetProcessId;
			ProcessId = processId;
			TimeStamp = timeStamp;

			BookingId = bookingId;
			CurrencyId = currencyId;
			Amount = amount;
			FinancePaymentMethod = financePaymentMethod;
			CreditCardAnswer = creditCardAnswer;

			TypeOfPayment = PaymentType.CreditCard;
		}

		/// <summary>
		/// Payment submission base om eWallet
		/// </summary>
		/// <param name="processId"></param>
		/// <param name="timeStamp"></param>
		/// <param name="bookingId"></param>
		/// <param name="currencyId"></param>
		/// <param name="amount"></param>
		public UIKomuterCompletePaymentSubmission(string processId, DateTime timeStamp,
			string bookingId, string currencyId, decimal amount, string financePaymentMethod, string bTnGSaleTransactionNo)
		{
			BaseNetProcessId = Guid.NewGuid();
			RefNetProcessId = BaseNetProcessId;
			ProcessId = processId;
			TimeStamp = timeStamp;

			BookingId = bookingId;
			CurrencyId = currencyId;
			Amount = amount;
			TypeOfPayment = PaymentType.PaymentGateway;
			FinancePaymentMethod = financePaymentMethod;
			BTnGSaleTransactionNo = bTnGSaleTransactionNo;
		}

		public void Dispose()
		{

		}
	}
}
