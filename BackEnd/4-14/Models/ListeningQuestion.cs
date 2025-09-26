using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AllEnBackend.Models
{
    [Table("LISTENING_QUESTION")]
    public class ListeningQuestion
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Column("LISTENING_SECTION_ID")]
        public int ListeningSectionId { get; set; }

        [Column("QUESTION_ORDER")]
        public int Order { get; set; }

        [Required]
        [Column("STEM")]
        public string Stem { get; set; }

        [Required]
        [Column("CORRECT_OPTION")]
        public string CorrectOption { get; set; }

        public List<ListeningOption> Options { get; set; } = new();
    }
}
