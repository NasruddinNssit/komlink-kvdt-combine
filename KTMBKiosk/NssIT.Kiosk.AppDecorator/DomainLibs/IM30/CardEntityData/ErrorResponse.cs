using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.IM30.CardEntityData
{
    public class ErrorResponse : ICardResponse
    {
        public bool IsDataFound { get; private set; } = false;
        public Exception DataError { get; private set; } = null;

        public ErrorResponse(Exception error)
        {
            IsDataFound = false;
            DataError = error;
        }
    }
}
