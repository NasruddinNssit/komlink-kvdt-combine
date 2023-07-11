using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.Log.Marking
{
    public class MarkLogSection 
    {
        /// <summary>
        /// Will Save to Process Id Column
        /// </summary>
        public Guid HeaderId { get; private set; } = Guid.NewGuid();
        //public object WorkLock { get; private set; } = new object();
        /// <summary>
        /// Will Save to ClsMetName Column
        /// </summary>
        public string MarkTitle { get; private set; } = "#*#";
        public MarkingLogType MarkLogType { get; private set; } = MarkingLogType.TimeIntervalSec;
        public DateTime? LogRequestedTime { get; set; } = null;
        public List<MarkLog> MsgList { get; private set; } = new List<MarkLog>();

        public MarkLogSection(Guid headerId, string title, MarkingLogType markLogType)
        {
            HeaderId = headerId;
            MarkTitle = title;
            MarkLogType = markLogType;
        }
    }
}
