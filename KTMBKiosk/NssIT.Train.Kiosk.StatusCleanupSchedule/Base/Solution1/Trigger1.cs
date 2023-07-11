﻿using Newtonsoft.Json;
using NssIT.Kiosk.Log.DB;
using NssIT.Train.Kiosk.Common.Data.Response;
using NssIT.Train.Kiosk.Common.Data.Response.BTnG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Train.Kiosk.StatusCleanupSchedule.Base.Solution1
{
    public static class Trigger1
    {
        private const string LogChannel = "Trigger1-KioskStatus-Cleanup";

        public static async Task<CommonResult> Run(LibShowMessageWindow.MessageWindow msgWin = null)
        {
            try
            {
                ExecSchedule1 execSchd = new ExecSchedule1(msgWin, NssIT.Kiosk.AppDecorator.Config.Setting.GetSetting().WebApiURL, SysInit.StatusLifeTimeDay);
                CommonResult res = await execSchd.ExecSchedule();

                DbLog.GetDbLog()?.LogText(LogChannel, "*", res, "B01", "Schedule1Win.CleanUp_Click01");

                return res;
            }
            catch (Exception ex)
            {
                DbLog.GetDbLog().LogText(LogChannel, "*", ex, "EX01", "Schedule1Win.CleanUp_Click01");
                throw ex;
            }
        }
    }
}
