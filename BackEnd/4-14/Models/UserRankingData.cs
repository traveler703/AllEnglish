using Oracle.ManagedDataAccess.Types;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Metadata;

namespace AllEnBackend.Models
{
    [Table("USER_RANKING_DATA")]
    public class UserRankingData
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string Username { get; set; }

        public int Score { get; set; } = 0;
        public int ActivityScore { get; set; } = 0;
        public int ReadingCount { get; set; } = 0;
        public int WordCount { get; set; } = 0;
        public int ListeningCount { get; set; } = 0;
        public int OralScore { get; set; } = 0;
        public int CurrentRankScore { get; set; } = 0;
        public int CurrentRankActivity { get; set; } = 0;
        public int LastRankScore { get; set; } = 0;
        public int LastRankActivity { get; set; } = 0;
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
