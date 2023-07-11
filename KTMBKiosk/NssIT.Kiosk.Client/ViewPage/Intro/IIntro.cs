using NssIT.Kiosk.AppDecorator.DomainLibs.Common.CreditDebitCharge;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.ViewPage.Intro
{
    public interface IIntro
    {
        void InitPage();
        void MaintenanceScheduler_OnRequestSettlement(object sender, EventArgs e);
        void MaintenanceScheduler_OnSettlementDone(object sender, SettlementDoneEventArgs e);
    }
}