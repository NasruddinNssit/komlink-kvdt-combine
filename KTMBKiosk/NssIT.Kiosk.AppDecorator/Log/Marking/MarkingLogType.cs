using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Log.Marking
{
    public enum MarkingLogType
    {
        /// <summary>
        /// Log only when interval timeout
        /// </summary>
        TimeIntervalSec = 0,
        /// <summary>
        /// Log only when hit a minimum capacity
        /// </summary>
        MinCapacity = 1,
        /// <summary>
        /// Log only when interval timeout with a minimum capacity
        /// </summary>
        TimeIntervalSec_With_MinCapacity = 2,
        /// <summary>
        /// Log only when receive intruction with minimum interval timeout and minimum capacity
        /// </summary>
        TriggerLog_With_MinTimeIntervalSec_N_MinCapacity = 3
    }
}