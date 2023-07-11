using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.IM30.TransParams
{
    public class TnGExitCheckoutParam : I2ndCardCommandParam
    {
        public TransactionTypeEn TransactionType => TransactionTypeEn.TouchNGo_2ndComm_Checkout;
        public string PosTransId { get; private set; }
        public decimal PenaltyAmount { get; private set; }
        public decimal FareAmount { get; private set; }

        /// <summary>
        /// Optional field for handheld only; For TnG & KomLink card
        /// </summary>
        public string ExitStationCode { get; private set; } = null;
        /// <summary>
        /// DateTime in yyMMddHHmmss
        /// </summary>
        public DateTime ExitDateTime { get; private set; }

        /// <summary>
        /// </summary>
        /// <param name="posTransId"></param>
        /// <param name="penaltyAmount"></param>
        /// <param name="exitDateTime"></param>
        /// <param name="exitStationCode">Default is NULL; For handheld this is needed</param>
        public TnGExitCheckoutParam(string posTransId, decimal fareAmount, decimal penaltyAmount, DateTime exitDateTime,
            string exitStationCode = null)
        {
            if (string.IsNullOrWhiteSpace(posTransId))
                throw new Exception("-TransactionId not valid; Invalid Touch n Go Checkin Parameter~");

            else if (penaltyAmount < 0)
                throw new Exception("-Penalty Amount not valid; Invalid Touch n Go Checkin Parameter~");

            PosTransId = posTransId;
            PenaltyAmount = penaltyAmount;
            ExitDateTime = exitDateTime;
            FareAmount = fareAmount;

            if (string.IsNullOrWhiteSpace(exitStationCode))
                ExitStationCode = null;
            else
                ExitStationCode = exitStationCode.Trim();
        }
    }
}