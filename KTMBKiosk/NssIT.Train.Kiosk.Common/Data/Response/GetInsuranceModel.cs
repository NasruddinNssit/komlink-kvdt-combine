using NssIT.Train.Kiosk.Common.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Train.Kiosk.Common.Data.Response
{
    [Serializable]
    public class GetInsuranceModel : IDisposable
    {
        public string Booking_Id { get; set; }
        public string MCurrencies_Id { get; set; }
        public string Error { get; set; } = YesNo.No;
        public string ErrorMessage { get; set; } = "";

        public InsuranceModel[] InsuranceModels { get; set; } = new InsuranceModel[0];

        public void Dispose()
        {
            InsuranceModels = null;
        }
    }
}
