using NssIT.Kiosk.AppDecorator.DomainLibs.IM30.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.IM30.TransParams
{
    public class TnGEntryCheckinParam : I2ndCardCommandParam
    {
        public TransactionTypeEn TransactionType => TransactionTypeEn.TouchNGo_2ndComm_CheckIn;
        public string PosTransId { get; private set; }
        public decimal PenaltyAmount { get; private set; }

        /// <summary>
        /// Optional field for handheld only; For TnG & KomLink card
        /// </summary>
        public string EntryStationCode { get; private set; } = null;
        /// <summary>
        /// DateTime in yyMMddHHmmss
        /// </summary>
        public DateTime EntryDateTime { get; private set; }

        /// <summary>
        /// </summary>
        /// <param name="posTransId"></param>
        /// <param name="penaltyAmount"></param>
        /// <param name="entryDateTime"></param>
        /// <param name="entryStationCode">Default is NULL; For handheld this is needed</param>
        public TnGEntryCheckinParam(string posTransId, decimal penaltyAmount, DateTime entryDateTime, 
            string entryStationCode = null)
        {
            if (string.IsNullOrWhiteSpace(posTransId))
                throw new Exception("-TransactionId not valid; Invalid Touch n Go Checkin Parameter~");

            else if (penaltyAmount < 0)
                throw new Exception("-Penalty Amount not valid; Invalid Touch n Go Checkin Parameter~");

            PosTransId = posTransId;
            PenaltyAmount = penaltyAmount;
            EntryDateTime = entryDateTime;

            if (string.IsNullOrWhiteSpace(entryStationCode))
                EntryStationCode = null;
            else
                EntryStationCode = entryStationCode.Trim();
        }
    }
}