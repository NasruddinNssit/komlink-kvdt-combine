using NssIT.Train.Kiosk.Common.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Train.Kiosk.Common.Data
{
    [Serializable]
    public class ValidatePromoCodeModel
    {
        public string Error { get; set; } = YesNo.No;
        public string ErrorMessage { get; set; } = "";
    }
}
