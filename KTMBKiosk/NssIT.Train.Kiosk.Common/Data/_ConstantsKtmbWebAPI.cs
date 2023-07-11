using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Train.Kiosk.Common.Data
{
    public enum ServiceStatus
    {
        None = -1,
        Success = 0,
        DataNotFound = 1,
        DuplicateKey = 2,
        OtherErrors = 3,
        ReferenceConstraint = 4,
        SystemError = 5,
        NotTodayTransaction = 6,
        NotLogin = 7,
        BookingExpired = 8,
        InvalidBankSignature = 9,
        BankRequeryFail = 10,
        ResetPassword = 11,
        ForeignKeyConstraint = 12,
        UniqueConstraint = 13,
        NotImage = 14,
        ImageSizeBelowLimit = 15,
        InvalidSpecialRegistration = 16,
        InvalidStatus = 17
    }
}
