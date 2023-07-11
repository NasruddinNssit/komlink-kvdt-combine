using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Train.Kiosk.Common.Data
{
    [Serializable]
    public class BookingSeatDetailModel
    {
        public Guid SeatLayoutModel_Id { get; set; } = Guid.Empty;
    }
}
