using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AllEnBackend.Models
{
    public class UserBestRanking
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string UserId { get; set; }

        public int BestRankScore { get; set; } = 99999;

        public int BestRankActivity { get; set; } = 99999;

        public int BestScore { get; set; } = 0;

        public int BestActivityScore { get; set; } = 0;

        public DateTime AchievedAt { get; set; } = DateTime.UtcNow;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
