using NssIT.Kiosk.AppDecorator.DomainLibs.KioskStatus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Network.StatusMonitorApp.JobApp
{
    public interface IStatusCheckingTask
    {
        /// <summary>
        /// Return process result to log as KioskStatusData. Null indicate not necessary.
        /// </summary>
        /// <param name="statusData"></param>
        /// <returns></returns>
        KioskStatusData CheckInNewStatus(KioskStatusData newStatusData);
        KioskStatusData GetDefaultStatus(string remark, IStatusRemark RemarkObj);
        KioskCheckingCode ThisCheckingCode { get; }
        void ReportSentStatus(KioskStatusData[] sentStatusList);
    }
}
