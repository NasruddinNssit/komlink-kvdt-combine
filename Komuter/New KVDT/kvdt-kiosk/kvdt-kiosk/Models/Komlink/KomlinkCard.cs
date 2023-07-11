using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kvdt_kiosk.Models.Komlink
{
    public class KomlinkCard
    {
        public bool Status { get; set; }
        public IList<string> Messages { get; set; }
        public object Code { get; set; }
        public KomlinkCardDetailModel Data { get; set; }
    }

    public class KomlinkCardDetailModel
    {
        public string CardData { get; set; }
        public string CardStatus { get; set; }
        public string CardNo { get; set; }
        public string CardSerialNo { get; set; }
        public string Name { get; set; }
        public string PhoneNo { get; set; }
        public string Email { get; set; }
        public decimal? CardBalance { get; set; }
        public string IsBlacklisted { get; set; }
        public string Gender { get; set; }
        public DateTime CardTypeExpireDate { get; set; }
        public bool IsMalaysian { get; set; }
        public DateTime? DateOfBirth { get; set; } 
        public string PNRNo { get; set; }
        public string IdType { get; set; }
        public string ICPassportNo { get; set; }
        public string TicketType { get; set; }
        public string IsKomlinkCardRead { get; set; }

        public IList<SeasonPass> SeasonPasses { get; set; } = new List<SeasonPass>();
    }

    public class SeasonPass
    {
        public string SeasonPassId { get; set; }
        public string SeasonPassDescription { get; set; }
        //public int ValidityPeriod { get; set; }
        //public string IsFixedValidity { get; set; }
        //public string AFCServiceId { get; set; }
        //public string AFCServiceDescription { get; set; }
        //public string IsSpecifiedOnD { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        //public string OriginId { get; set; }
        //public string OriginDescription { get; set; }
        //public string DestinationId { get; set; }
        //public string DestinationDescription { get; set; }
        //public string RegistrationCode { get; set; }
        //public decimal PassPrice { get; set; }
        //public string KomlinkCardNo { get; set; }
        //public int SeasonPassSlotNo { get; set; }
        //public DateTime SeasonPassLastTravelDateTime { get; set; }
        //public int SeasonPassDailyTravelTripCount { get; set; }
        //public string IssuerSAMId { get; set; }
    }


}
