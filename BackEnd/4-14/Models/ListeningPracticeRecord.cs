using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AllEnBackend.Models
{
    /// <summary>
    /// 用户每次完成一套听力真题的记录
    /// </summary>
    [Table("LISTENING_PRACTICE_RECORD")]
    public class ListeningPracticeRecord
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Required]
        [Column("USER_ID")]
        public string UserId { get; set; } = null!;

        [Required]
        [Column("LISTENING_PAPER_ID")]
        public int ListeningPaperId { get; set; }

        [Required]
        [Column("SCORE")]
        public int Score { get; set; }

        [Required]
        [Column("COMPLETED_AT")]
        public DateTime CompletedAt { get; set; }

    }
}