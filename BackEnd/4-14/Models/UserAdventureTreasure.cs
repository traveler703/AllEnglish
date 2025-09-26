using System;

namespace AllEnBackend.Models
{
    public class UserAdventureTreasure
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public long TreasureId { get; set; }

        // 导航属性
        public AdventureTreasure Treasure { get; set; }

        public DateTime OpenedAt { get; set; }
        public string RewardsReceived { get; set; }
    }
}