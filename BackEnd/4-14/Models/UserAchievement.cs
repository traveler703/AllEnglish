namespace AllEnBackend.Models
{
    public class UserAchievement
    {
        public string UserId { get; set; } = string.Empty;     // 用户ID
        public int AchievementId { get; set; }                 // 成就ID
        public int HasGained { get; set; } = 0;                // 是否取得成就（0=未取得，1=已取得）
        public DateTime? GainDate { get; set; }                // 取得成就的日期

        // 导航属性
        public User User { get; set; } = null!;
        public Achievement Achievement { get; set; } = null!;
    }
} 