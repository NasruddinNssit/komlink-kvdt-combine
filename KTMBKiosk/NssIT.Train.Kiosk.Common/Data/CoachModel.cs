using NssIT.Kiosk.AppDecorator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Train.Kiosk.Common.Data
{
    [Serializable]
    public class CoachModel
    {
        public Guid Id { get; set; }
        public string CoachId { get; set; }
        public string CoachLabel { get; set; }
        public int SeatAvailable { get; set; }
        public virtual int SeatColumn { get; set; }
        public virtual int SeatRow { get; set; }

        private SeatLayoutModel[] _seatLayoutModels = new SeatLayoutModel[0];
        public SeatLayoutModel[] SeatLayoutModels 
        { 
            get => _seatLayoutModels;
            set
            {
                _seatLayoutModels = value;
                if (value != null)
                    SeatLayoutModelsLength = value.Length;
            }
        } 

        public bool IsSeatLayoutModelsToString { get; set; } = false;
        public int SeatLayoutModelsLength { get; set; } = 0;
        public CoachModel()
        {
            IsSeatLayoutModelsToString = (CommonAct.RandNoAct.Next() % 500) == 0;
        }

        public bool ShouldSerializeSeatLayoutModels()
        {
            return IsSeatLayoutModelsToString;
        }
    }
}
