using NssIT.Kiosk.AppDecorator;
using NssIT.Train.Kiosk.Common.Common.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Train.Kiosk.Common.Data.Response
{
    [Serializable]
    public class StationResult : iWebApiResult, IDisposable
    {
        public bool Status { get; set; }

        private StationDetailsModel[] _data = null;
        public StationDetailsModel[] Data 
        { 
            get => _data;
            set
            {
                _data = value;
                if (value != null)
                    DataLength = value.Length;
                else
                    DataLength = 0;
            }
        }

        public IList<string> Messages { get; set; }
        public string Code { get; set; }

        public string MessageString()
        {
            string msgStr = null;
            foreach (string msg in Messages)
                msgStr += $@"{msg}; ";
            return msgStr;
        }

        public void Dispose()
        {
            Data = null;
            Messages = null;
        }

        public bool IsDataToString { get; set; } = false;
        public int DataLength { get; set; } = 0;
        public StationResult()
        {
            IsDataToString = (CommonAct.RandNoAct.Next() % 50) == 0;
        }

        public bool ShouldSerializeData()
        {
            return IsDataToString;
        }
    }
}
