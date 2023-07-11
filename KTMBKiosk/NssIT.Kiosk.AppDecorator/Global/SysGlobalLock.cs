using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Global
{
    public static class SysGlobalLock
    {
        public static object SysLock = new object();
        public static object DBLock = new object();
        public static object IM30Lock = new object();

        /// <summary>
        /// Init at the biginning og application Need to be init the static varibles to stablized the value. Or to avoid overlapped object initiation.
        /// </summary>
        public static void Init()
        {
            object sysLock = SysLock;
            object dbLock = DBLock;
            object im30Lock = IM30Lock;
        }
    }
}
