using NssIT.Kiosk.AppDecorator.Common.AppService.Sales;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService.Command.Sales
{
	public class CompleteTransactionElseReleaseSeatCommand : IAccessDBCommand, IDisposable
	{
		public AccessDBCommandCode CommandCode => AccessDBCommandCode.CompleteTransactionElseReleaseSeatRequest;
		public string ProcessId { get; private set; }
		public Guid? NetProcessId { get; private set; }
		public bool HasCommand { get => (CommandCode != AccessDBCommandCode.UnKnown); }
		public bool ProcessDone { get; set; } = false;

		//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
		public string TransactionNo { get; private set; }
		public string TradeCurrency { get; set; }
		public decimal TotalAmount { get; private set; }
		public PaymentType TypeOfPayment { get; set; } = PaymentType.Unknown;

		/// <summary>
		/// Refer to a Bank reference number
		/// </summary>
		public string BankReferenceNo { get; private set; }

		public CreditCardResponse CreditCardAnswer { get; private set; }

		//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
		//B2B Cash Maching
		public int Cassette1NoteCount { get; private set; }
		public int Cassette2NoteCount { get; private set; }
		public int Cassette3NoteCount { get; private set; }
		public int RefundCoinAmount { get; private set; }
		//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
		// For 'Boost / Touch n Go' Payment transaction
		public string BTnGSaleTransactionNo { get; private set; }
		public string PaymentMethod { get; private set; }
		//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX


		/// <summary>
		/// Credit Card Payment ctor.
		/// </summary>
		/// <param name="processId"></param>
		/// <param name="netProcessId"></param>
		/// <param name="transactionNo"></param>
		/// <param name="tradeCurrency"></param>
		/// <param name="typeOfPayment"></param>
		/// <param name="totalAmount"></param>
		/// <param name="bankReferenceNo"></param>
		/// <param name="creditCardAnswer"></param>
		public CompleteTransactionElseReleaseSeatCommand(string processId, Guid? netProcessId, string transactionNo, string tradeCurrency,
			decimal totalAmount, string bankReferenceNo, CreditCardResponse creditCardAnswer)
		{
			ProcessId = processId;
			NetProcessId = netProcessId;

			TotalAmount = totalAmount;
			TransactionNo = transactionNo;
			TypeOfPayment = PaymentType.CreditCard;
			TradeCurrency = tradeCurrency;
			BankReferenceNo = bankReferenceNo;
			CreditCardAnswer = creditCardAnswer;
			PaymentMethod = "CreditCard";
		}

		/// <summary>
		/// Payment Gateway (eWallet) ctor.
		/// </summary>
		/// <param name="processId"></param>
		/// <param name="netProcessId"></param>
		/// <param name="transactionNo"></param>
		/// <param name="tradeCurrency"></param>
		/// <param name="totalAmount"></param>
		/// <param name="bTnGSaleTransactionNo"></param>
		public CompleteTransactionElseReleaseSeatCommand(string processId, Guid? netProcessId, string transactionNo, string tradeCurrency,
			decimal totalAmount, string bTnGSaleTransactionNo, string paymentMethod)
		{
			ProcessId = processId;
			NetProcessId = netProcessId;

			TotalAmount = totalAmount;
			TransactionNo = transactionNo;
			TypeOfPayment = PaymentType.PaymentGateway;
			TradeCurrency = tradeCurrency;
			BTnGSaleTransactionNo = bTnGSaleTransactionNo;
			PaymentMethod = paymentMethod;
		}

		public void Dispose()
		{
			NetProcessId = null;
		}
	}
}