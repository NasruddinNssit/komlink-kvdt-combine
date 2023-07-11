using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NssIT.Kiosk.AppDecorator;
using NssIT.Train.Kiosk.Common.Constants;

namespace NssIT.Train.Kiosk.Common.Data
{
    [Serializable]
    public class TrainCoachSeatModel
    {
        public string Id { get; set; }
        public string TrainNo { get; set; }
        public string TrainLabel { get; set; }
        public string ServiceCategory { get; set; }
        public string DepartureDateFormat { get; set; }
        public string DepartureTimeFormat { get; set; }
        public int ArrivalDayOffset { get; set; }
        public string ArrivalTimeFormat { get; set; }
        public int SeatAvailable { get; set; }
        public string Origin { get; set; }
        public string OriginDescription { get; set; }
        public int OriginSequenceNo { get; set; }
        public string Destination { get; set; }
        public string DestinationDescription { get; set; }
        public string Currency { get; set; }
        public string SeatSelectedIconURL { get; set; }
        public string SeatSoldIconURL { get; set; }
        public string SeatMaleIconURL { get; set; }
        public string SeatFemaleIconURL { get; set; }
        public string SeatReservedIconURL { get; set; }
        public string SeatBlockedIconURL { get; set; }

        /// <summary>
        /// Return value refer to NssIT.Train.Kiosk.Common.Constants.YesNo 
        /// </summary>
        public string TVMDisplayGender { get; set; } = YesNo.No;
        public string Error { get; set; } = YesNo.No;

        /// <summary>
        /// 0 for no error. 1 for has error.
        /// </summary>
        public int ErrorCode { get => (((Error ?? YesNo.No).Equals(YesNo.Yes)) ? 1 : 0); }
        public string ErrorMessage { get; set; } = null;
        public CoachModel[] CoachModels { get; set; }
    }
}
