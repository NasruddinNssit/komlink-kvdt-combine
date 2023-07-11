using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Device.Nippon.NP3611BD.AccessSDK.Command
{
    public class CommandStatus : IDisposable 
    {
        public bool IsCommandNew { get; set; }
        public DateTime CommandCreatedTime { get; set; }
        /////---------------------------------------------
        public bool IsCommandInProgress { get; set; } = false;
        public bool IsCommandEnd { get; set; } = false;
        public bool IsCommandEndSuccessful { get; set; } = false;
        public bool IsErrorFound => (ErrorObject != null);
        public Exception ErrorObject { get; set; } = null;
        public DateTime? CommandStartExecuteTime { get; set; } = null;
        public DateTime? CommandFinishedTime { get; set; } = null;

        public CommandStatus()
        {
            IsCommandNew = true;
            CommandCreatedTime = DateTime.Now;
        }

        /// <summary>
        /// Only allowed to run for one time.
        /// </summary>
        public void StartExecution()
        {
            if (IsCommandInProgress)
                return;

            IsCommandNew = false;
            IsCommandInProgress = true;
            CommandStartExecuteTime = DateTime.Now;
        }

        /// <summary>
        /// Only allowed to run for one time.
        /// </summary>
        /// <param name="isCommandEndSuccessFul"></param>
        /// <param name="error"></param>
        public void EndExecution(bool isCommandEndSuccessFul, Exception error)
        {
            if (IsCommandEnd)
                return;

            IsCommandEnd = true;
            IsCommandInProgress = true;
            IsCommandNew = false;

            if (CommandStartExecuteTime.HasValue == false)
                CommandStartExecuteTime = DateTime.Now;

            ErrorObject = error;
            IsCommandEndSuccessful = isCommandEndSuccessFul;

            CommandFinishedTime = DateTime.Now;
        }

        public void Dispose()
        {
            ErrorObject = null;
            CommandStartExecuteTime = null;
            CommandFinishedTime = null;
        }
    }
}
