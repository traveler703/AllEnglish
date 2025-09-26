using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AllEnBackend.Models
{
    [Table("LISTENING_PAPER")]
    public class ListeningPaper
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Required]
        [Column("PAPER_LEVEL")]
        public string Level { get; set; }

        [Required]
        [Column("PAPER_YEAR")]
        public int Year { get; set; }

        [Required]
        [Column("PAPER_SESSION")]
        public string Session { get; set; }

        [Column("PAPER_AUDIO_URL")]
        public string AudioUrl { get; set; }

        public List<ListeningSection> Sections { get; set; } = new();
    }
}
