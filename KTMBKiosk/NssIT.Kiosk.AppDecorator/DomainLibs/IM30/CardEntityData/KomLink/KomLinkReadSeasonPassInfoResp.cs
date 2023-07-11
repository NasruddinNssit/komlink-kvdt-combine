using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.AppDecorator.DomainLibs.IM30.CardEntityData.KomLink
{
    public class KomLinkReadSeasonPassInfoResp : ICardResponse, IDisposable 
    {
        public bool IsSP1Available { get; private set; } = false;
        public bool IsSP2Available { get; private set; } = false;
        public bool IsSP3Available { get; private set; } = false;
        public bool IsSP4Available { get; private set; } = false;
        public bool IsSP5Available { get; private set; } = false;
        public bool IsSP6Available { get; private set; } = false;
        public bool IsSP7Available { get; private set; } = false;
        public bool IsSP8Available { get; private set; } = false;

        public KomLinkSeasonPass[] SeasonPassList { get; private set; } = new KomLinkSeasonPass[] { };

        public KomLinkReadSeasonPassInfoResp(
            bool isSP1Available, bool isSP2Available,
            bool isSP3Available, bool isSP4Available,
            bool isSP5Available, bool isSP6Available,
            bool isSP7Available, bool isSP8Available,
            KomLinkSeasonPass[] seasonPassList)
        {
            IsSP1Available = isSP1Available;
            IsSP2Available = isSP2Available;
            IsSP3Available = isSP3Available;
            IsSP4Available = isSP4Available;
            IsSP5Available = isSP5Available;
            IsSP6Available = isSP6Available;
            IsSP7Available = isSP7Available;
            IsSP8Available = isSP8Available;

            if (seasonPassList != null)
                SeasonPassList = seasonPassList;
        }

        public void Dispose()
        {
            SeasonPassList = null;
        }
    }
}
