﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Device.Nippon.NP3611BD.AccessSDK.Command
{
    [Serializable]
    public class NullAccessParameter : INP3611CommandParameter
    {
        public string ModelTag => "NullAccessParameter";
    }
}
