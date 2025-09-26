// UserAdventureDto.cs
using System.ComponentModel.DataAnnotations;

namespace AllEnBackend.Dtos
{
    // 用户冒险记录DTO
    public class UserAdventureDto
    {
        public string UserId { get; set; }
        public long AdventureId { get; set; }
        public string Status { get; set; } 
    }

    // 创建/更新用户冒险记录DTO
    public class CreateUserAdventureDto
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        public long AdventureId { get; set; }

        [Required]
        [RegularExpression("^(locked|unlocked)$", ErrorMessage = "状态只能是locked或unlocked")]
        public string Status { get; set; }
    }

    // 用户冒险状态DTO
    public class UserAdventureStatusDto
    {
        public string Status { get; set; } // "locked" 或 "unlocked"
        public bool IsUnlocked => Status == "unlocked";
        public DateTime? UnlockTime { get; set; }
    }
}