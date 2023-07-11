using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService.Command.Sales
{
	public class KomuterCompletePaymentCommand : IAccessDBCommand, IDisposable
	{
		public AccessDBCommandCode CommandCode => AccessDBCommandCode.KomuterCompletePaymentRequest;
		public string ProcessId { get; private set; }
		public Guid? NetProcessId { get; private set; }
		public bool HasCommand { get => (CommandCode != AccessDBCommandCode.UnKnown); }
		public bool ProcessDone { get; set; } = false;

		//xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

		public string BookingId { get; set; }
		public string CurrencyId { get; set; }
		public decimal Amount { get; set; }
		public string FinancePaymentMethod { get; set; }
		public PaymentType TypeOfPayment { get; set; } = PaymentType.Unknown;
		public CreditCardResponse CreditCardAnswer { get; private set; }

		//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
		// For 'Boost / Touch n Go' Payment transaction
		public string BTnGSaleTransactionNo { get; private set; }
		//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

		public KomuterCompletePaymentCommand(string processId, Guid? netProcessId,
			string bookingId,
			string currencyId,
			decimal amount,
			string financePaymentMethod, CreditCardResponse creditCardAnswer)
		{
			ProcessId = processId;
			NetProcessId = netProcessId;

			BookingId = bookingId;
			CurrencyId = currencyId;
			Amount = amount;
			FinancePaymentMethod = financePaymentMethod;
			CreditCardAnswer = creditCardAnswer;
			TypeOfPayment = PaymentType.CreditCard;
		}

		public KomuterCompletePaymentCommand(string processId, Guid? netProcessId,
			string bookingId,
			string currencyId,
			decimal amount,
			string financePaymentMethod, string bTnGSaleTransactionNo)
		{
			ProcessId = processId;
			NetProcessId = netProcessId;

			BookingId = bookingId;
			CurrencyId = currencyId;
			Amount = amount;
			FinancePaymentMethod = financePaymentMethod;
			TypeOfPayment = PaymentType.PaymentGateway;
			BTnGSaleTransactionNo = bTnGSaleTransactionNo;
		}

		public void Dispose()
		{
			NetProcessId = null;
		}
	}
}


