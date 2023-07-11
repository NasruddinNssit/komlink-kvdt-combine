﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Train.Kiosk.Common.Data
{
    [Serializable]
    public class BTnGSuccessPaidModel
    {
        ///<summary>PaymentGateway Header Related</summary>
        public string SalesTransactionNo { get; set; }
        ///<summary>Refer to KTMB.Library.Constants.Constants.PaymentGatewayHeaderStatus; PaymentGateway Header Related</summary>
        public string HeaderStatus { get; set; }

        ///<summary>Refer to KTMB.Library.Constants.Constants.PaymentGatewayDetailStatus; PaymentGateway Detail Related</summary>
        public string DetailStatus { get; set; }
        ///<summary>PaymentGateway Detail Related</summary>
        public string LineTag { get; set; }
        ///<summary>PaymentGateway Detail Related</summary>
        public string Remark { get; set; }
        ///<summary>PaymentGateway Detail Related</summary>
        public string TransObjectType { get; set; }
        ///<summary>PaymentGateway Detail Related</summary>
        public string TransObject { get; set; }
        ///<summary>PaymentGateway Detail Related</summary>
        public string User { get; set; }
    }
}
