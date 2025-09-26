namespace AllEnBackend.Models
{
    public class Advertisement
    {
        public string Id { get; set; } = string.Empty; 
        public string MediaUrl { get; set; } = string.Empty;         // 预览图资源地址
        public string TargetId { get; set; } = string.Empty;        // 点击跳转地址
        public string Context { get; set; } = string.Empty;          // 广告文字部分
        public int Status { get; set; } = 1;                         // 状态：0=停用 1=启用
        public int ClickCount { get; set; } = 0;                    // 累计点击次数
        public DateTime CreateTime { get; set; } = DateTime.Now;    // 创建时间
    }
}