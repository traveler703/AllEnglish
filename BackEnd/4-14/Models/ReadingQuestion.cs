using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AllEnBackend.Models
{
    public class ReadingQuestion
    {
        [Key]
        public int Id { get; set; }

        public int ArticleId { get; set; }

        [StringLength(20)]
        public string Seqo { get; set; } // 题号

        public int? Kind { get; set; }

        public string Stem { get; set; } // 题干

        public string Options { get; set; }

        public string AnswerKey { get; set; }

        public int? Score { get; set; } // 本题分值

        public DateTime? CreatedAt { get; set; }

        [ForeignKey("ArticleId")]
        public Article Article { get; set; }
    }
}
