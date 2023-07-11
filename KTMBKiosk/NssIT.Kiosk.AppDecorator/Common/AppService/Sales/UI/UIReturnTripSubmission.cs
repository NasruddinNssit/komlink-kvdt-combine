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
	public class UIReturnTripSubmission : IKioskMsg, INetCommandDirective
	{
		public Guid BaseNetProcessId { get; private set; }
		public Guid? RefNetProcessId { get; private set; } = null;
		public string ProcessId { get; private set; }
		public DateTime TimeStamp { get; private set; } = DateTime.MinValue;
		public AppModule Module { get; } = AppModule.UIKioskSales;
		public string ModuleDesc { get => Enum.GetName(typeof(AppModule), Module); }
		public dynamic GetMsgData() => NotApplicable.Object;
		public CommInstruction Instruction { get; } = (CommInstruction)UISalesInst.ReturnTripSubmission;
		public string InstructionDesc { get => Enum.GetName(typeof(CommInstruction), Instruction); }
		public string ErrorMessage { get; set; } = null;
		public CommunicationDirection CommuCommandDirection { get; } = CommunicationDirection.SendOneResponseOne;

		//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
		public string TripId { get; set; } = null;
		public DateTime PassengerDepartDateTime { get; set; } = DateTime.MinValue;
		public DateTime PassengerArrivalDateTime { get; set; } = DateTime.MinValue;
		public string PassengerDepartTimeStr { get; set; } = null;
		public int PassengerArrivalDayOffset { get; set; } = 0;
		public string PassengerArrivalTimeStr { get; set; } = null;
		/// <summary>
		/// Like : ETS / INTERCITY
		/// </summary>
		public string VehicleService { get; set; } = null;
		public string VehicleNo { get; set; } = null;
		public string ServiceCategory { get; set; } = null;
		public string Currency { get; set; } = null;
		public decimal Price { get; set; } = 0.00M;
		//XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

		public UIReturnTripSubmission(string processId, DateTime timeStamp,
			string tripId,
			DateTime passengerDepartDateTime,
			DateTime passengerArrivalDateTime,
			string passengerDepartTimeStr,
			int passengerArrivalDayOffset,
			string passengerArrivalTimeStr,
			string vehicleService,
			string vehicleNo,
			string serviceCategory,
			string currency,
			decimal price)
		{
			BaseNetProcessId = Guid.NewGuid();
			RefNetProcessId = BaseNetProcessId;
			ProcessId = processId;
			TimeStamp = timeStamp;

			TripId = tripId;
			PassengerDepartDateTime = passengerDepartDateTime;
			PassengerArrivalDateTime = passengerArrivalDateTime;
			PassengerDepartTimeStr = passengerDepartTimeStr;
			PassengerArrivalDayOffset = passengerArrivalDayOffset;
			PassengerArrivalTimeStr = passengerArrivalTimeStr;
			VehicleService = vehicleService;
			VehicleNo = vehicleNo;
			ServiceCategory = serviceCategory;
			Currency = currency;
			Price = price;
		}

		public void Dispose() { }
	}
}

