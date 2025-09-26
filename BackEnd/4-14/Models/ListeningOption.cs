using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AllEnBackend.Models
{
    [Table("LISTENING_OPTION")]
    public class ListeningOption
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Column("LISTENING_QUESTION_ID")]
        public int ListeningQuestionId { get; set; }

        [Required]
        [Column("OPTION_LABEL")]
        public string Label { get; set; }

        [Required]
        [Column("CONTENT")]
        public string Content { get; set; }
    }
}