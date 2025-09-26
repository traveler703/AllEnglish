namespace AllEnBackend.Models
{
    public class VIP
    {
        public string UserId { get; set; } = string.Empty;             // 用户ID
        public DateTime StartTime { get; set; } = DateTime.Now;        // VIP 开始时间
        public DateTime ExpirationTime { get; set; }                   // VIP 过期时间
        public int IsActive { get; set; } = 0;                         // 是否激活：0=未激活，1=已激活
    }
}
