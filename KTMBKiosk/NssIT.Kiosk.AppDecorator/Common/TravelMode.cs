using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Common
{
    public enum TravelMode
    {
        DepartOnly = 0,
        ReturnOnly = 1,

        /// <summary>
        /// Depart is mandatory, return may optional
        /// </summary>
        DepartOrReturn = 2,

        WeeklyPass = 3

        // KTM ETS/Intercity use DepartOnly, ReturnOnly, DepartOrReturn
        // KTM KOMUTER use DepartOnly (One Way), DepartOrReturn, WeeklyPass

    }
}