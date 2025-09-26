using System.ComponentModel.DataAnnotations;

namespace AllEnBackend.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PostId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // 导航属性
        public virtual Post Post { get; set; }
        public virtual User User { get; set; }
    }
}
