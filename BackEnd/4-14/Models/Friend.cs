using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AllEnBackend.Models
{
    public class Friend
    {
        [Key]
        [Column("FRIENDS_ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int FriendId { get; set; }

        [Required]
        [Column("USER_ID")]
        [StringLength(50)]
        public string UserId { get; set; }

        [Required]
        [Column("FRIENDS_USER_ID")]
        [StringLength(50)]
        public string FriendUserId { get; set; }

        [Required]
        [Column("STATUS")]
        public int Status { get; set; }

        [Column("CREATED_AT")]
        public DateTime? CreatedAt { get; set; }

        [Column("UPDATE_AT")]
        public DateTime? UpdateAt { get; set; }

        // 导航属性
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("FriendUserId")]
        public virtual User FriendUser { get; set; }
    }

    // 好友状态常量类
    public static class FriendStatus
    {
        public const int Pending = 0;    // 待处理
        public const int Accepted = 1;   // 已接受  
        public const int Rejected = 2;   // 已拒绝
        public const int Blocked = 3;    // 已屏蔽

        public static string GetStatusText(int status)
        {
            return status switch
            {
                Pending => "待处理",
                Accepted => "已接受",
                Rejected => "已拒绝",
                Blocked => "已屏蔽",
                _ => "未知状态"
            };
        }

        public static bool IsValidStatus(int status)
        {
            return status >= 0 && status <= 3;
        }
    }
}
