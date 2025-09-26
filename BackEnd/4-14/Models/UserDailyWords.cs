using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AllEnBackend.Models
{
    [Table("USER_DAILY_WORDS")]
    public class UserDailyWords
    {
        [Column("USER_ID")]
        [StringLength(50)]
        public string UserId { get; set; } = string.Empty;

        [Column("STUDY_DATE")]
        public DateTime StudyDate { get; set; }

        [Column("WORD_COUNT")]
        public int WordCount { get; set; } = 0;

        // 导航属性，引用User表
        public User User { get; set; } = null!;
    }
}
