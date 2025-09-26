using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AllEnBackend.Models
{
    [Table("LISTENING_SECTION")]
    public class ListeningSection
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Column("LISTENING_PAPER_ID")]
        public int ListeningPaperId { get; set; }

        [Column("SECTION_ORDER")]
        public int Order { get; set; }

        public List<ListeningQuestion> Questions { get; set; } = new();
    }
}