using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AllEnBackend.Models
{
    public class Article
    {
        [Column("ARTICLE_ID")]
        public int ArticleId { get; set; }
        [Column("COURSE_ID")]
        public int CourseId { get; set; }
        [Column("TITLE")]
        public string Title { get; set; } = string.Empty;
        [Column("CONTENT")]
        public string Content { get; set; } = string.Empty;
        [Column("DIFFICULTY")]
        public int Difficulty { get; set; }
        [Column("COVER_IMAGE")]
        public string? CoverImage { get; set; }
        [Column("CATEGORY")]
        public string? Category { get; set; }
        [Column("READING_TIME")]
        public int? ReadingTime { get; set; }
        [Column("WORD_COUNT")]
        public int? WordCount { get; set; }
        [Column("TAGS")]
        [Required(AllowEmptyStrings = true)]
        public List<string>? Tags { get; set; } = new();
        [Column("CREATED_AT")]
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        [Column("UPDATED_AT")]
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;

    }
}