using System;
using System.ComponentModel.DataAnnotations;

namespace AllEnBackend.Models
{
    public class LeaderboardReward
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [MaxLength(20)]
        public string RankType { get; set; }

        [Required]
        [MaxLength(20)]
        public string TimeRange { get; set; }

        [Required]
        public int RankStart { get; set; }

        [Required]
        public int RankEnd { get; set;}

        [Required]
        public int CoinReward { get; set; } = 0;

        [Required]
        public int PointReward { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedTime { get; set; } = DateTime.Now;
    }
}
