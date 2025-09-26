using System;

namespace AllEnBackend.Models
{
    public class UserCoin
    {
        public string Id { get; set; } // 主键
        public string UserId { get; set; } // 外键（用户ID）
        public int Coin { get; set; } = 1000;//虚拟币（创建用户时默认值为1000）
        public DateTime? LastSignDate { get; set; }
        public DateTime? FirstSignDate { get; set;}
    }
}
