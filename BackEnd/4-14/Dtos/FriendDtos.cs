using AllEnBackend.Models;
using System.ComponentModel.DataAnnotations;

namespace AllEnBackend.Dtos
{
    public class AddFriendDto
    {
        [Required(ErrorMessage = "好友用户ID不能为空")]
        [StringLength(50, ErrorMessage = "用户ID长度不能超过50")]
        public string FriendsUserId { get; set; }
    }

    public class FriendResponseDto
    {
    public long FriendsId { get; set; }
    public string UserId { get; set; }
    public string FriendsUserId { get; set; }
    public string FriendUserName { get; set; }
    public string FriendAvatarUrl { get; set; }
    public int Status { get; set; } // 改为int
    public string StatusText => FriendStatus.GetStatusText(Status);
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdateAt { get; set; }
    }

    public class FriendStatusUpdateDto
    {
        [Required(ErrorMessage = "好友关系ID不能为空")]
        public string friendsId { get; set; }

        [Required(ErrorMessage = "状态不能为空")]
        [Range(0, 3, ErrorMessage = "状态值必须在0-3之间")]
        public int Status { get; set; } // 改为int

        public bool IsValidStatus()
        {
            return FriendStatus.IsValidStatus(Status);
        }
    }

    // 根据昵称返回找到的用户
    public class FriendSearchResponseDto
    { 
        public string FriendsId { get; set; }

        public string FriendName { get; set; }

        public string FriendAvatarUrl { get; set; }
    }
}
