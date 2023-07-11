using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.ViewPage.CustInfo
{
    public class PassengerSeat
    {
        public string TrainSeatModelId { get; set; } = null;
        public string SeatNo { get; set; } = null;
        public Guid SeatLayoutModelId { get; set; } = Guid.Empty;
        public string TicketTypesId { get; set; } = string.Empty;
        public TravelDirection TravelDirect { get; set; } = TravelDirection.Depart;
        public string OriginStationCode { get; set; } = null;
        public string OriginStationName { get; set; } = null;
        public string DestinationStationCode { get; set; } = null;
        public string DestinationStationName { get; set; } = null;

        public static PassengerSeat[] DuplicateList(PassengerSeat[] passengerSeatList)
        {
            if ((passengerSeatList is null) || (passengerSeatList.Length > 0))
                return new PassengerSeat[0];

            return (from seat in passengerSeatList select (new PassengerSeat() 
            { 
                DestinationStationCode = seat.DestinationStationCode, 
                DestinationStationName = seat.DestinationStationName, 
                SeatLayoutModelId = seat.SeatLayoutModelId, 
                OriginStationCode = seat.OriginStationCode, 
                OriginStationName = seat.OriginStationName, 
                SeatNo = seat.SeatNo, 
                TicketTypesId = seat.TicketTypesId, 
                TrainSeatModelId = seat.TrainSeatModelId, 
                TravelDirect = seat.TravelDirect
            })).ToArray();
        }

        public static PassengerSeat Duplicate(PassengerSeat passengerSeat)
        {
            if (passengerSeat is null)
                return null;

            return (new PassengerSeat()
                    {
                        DestinationStationCode = passengerSeat.DestinationStationCode,
                        DestinationStationName = passengerSeat.DestinationStationName,
                        SeatLayoutModelId = passengerSeat.SeatLayoutModelId,
                        OriginStationCode = passengerSeat.OriginStationCode,
                        OriginStationName = passengerSeat.OriginStationName,
                        SeatNo = passengerSeat.SeatNo,
                        TicketTypesId = passengerSeat.TicketTypesId,
                        TrainSeatModelId = passengerSeat.TrainSeatModelId,
                        TravelDirect = passengerSeat.TravelDirect
                    });
        }
    }
}
