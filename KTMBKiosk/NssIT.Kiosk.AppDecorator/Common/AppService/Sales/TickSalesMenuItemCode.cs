using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common.AppService.Sales
{
	public enum TickSalesMenuItemCode
	{
		Non = 0,
		FromStation = 1,
		ToStation = 2,

		/// <summary>
		/// Include Depart Date and Return Date
		/// </summary>
		TravelDates = 12,

		/// <summary>
		/// Obsolete
		/// </summary>
		DepartDate = 3,

		DepartTrip = 4,
		DepartSeat = 5,

		/// <summary>
		/// Obsolete
		/// </summary>
		ReturnDate = 6,

		ReturnTrip = 7,
		ReturnSeat = 8,

		Passenger = 9,
		
		Insurance = 12,

		Payment = 10,

		AfterPayment = 11,

		/// <summary>
		/// Refer to KTM Komuter
		/// </summary>
		StartSelling = 20
	}
}