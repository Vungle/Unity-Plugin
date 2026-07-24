using System.Collections.Generic;

namespace VungleAds
{
    public class VungleCSBData
    {
        public double BidFloor { get; private set; }
        public string AuctionId { get; private set; }
        public string CreativeId { get; private set; }
        public string AdUnitId { get; private set; }
        public bool IsVxWinner { get; private set; }
        public bool IsPriorityAccess { get; private set; }
        public Dictionary<string, string> Extras { get; private set; }

        public VungleCSBData(
            double bidFloor,
            string auctionId = null,
            string creativeId = null,
            string adUnitId = null,
            bool isVxWinner = false,
            bool isPriorityAccess = false)
        {
            BidFloor = bidFloor;
            AuctionId = auctionId;
            CreativeId = creativeId;
            AdUnitId = adUnitId;
            IsVxWinner = isVxWinner;
            IsPriorityAccess = isPriorityAccess;
            Extras = new Dictionary<string, string>();
        }

        public void SetExtras(Dictionary<string, string> extras)
        {
            if (extras != null)
            {
                Extras = new Dictionary<string, string>(extras);
            }
        }
    }
}
