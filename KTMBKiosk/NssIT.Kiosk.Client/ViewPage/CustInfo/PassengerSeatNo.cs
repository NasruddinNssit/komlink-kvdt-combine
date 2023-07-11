﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Client.ViewPage.CustInfo
{
    public class PassengerSeatNo
    {
        public Guid SeatId { get; private set; }
        public string SeatDesn { get; private set; }
        public string SeatType { get; private set; }

        public PassengerSeatNo(string seatDesn, Guid seatId, string seatType)
        {
            SeatDesn = seatDesn;
            SeatId = seatId;
            SeatType = seatType;
        }
    }
}
