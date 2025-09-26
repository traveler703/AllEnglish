namespace AllEnBackend.Dtos
{
    // 成就信息DTO
    public class AchievementDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int CoinCount { get; set; }
        public int ArticleCount { get; set; }
        public int WordCount { get; set; }
        public int OralTime { get; set; }
        public int ListeningTime { get; set; }
        public int ArticlePerday { get; set; }
        public int WordPerday { get; set; }
        public int OralPerday { get; set; }
        public int ListeningPerday { get; set; }
    }

    // 用户成就状态DTO
    public class UserAchievementDto
    {
        public int AchievementId { get; set; }
        public int HasGained { get; set; }
        public DateTime? GainDate { get; set; }
    }

    // 用户已取得成就列表DTO
    public class UserGainedAchievementsDto
    {
        public List<int> AchievementIds { get; set; } = new List<int>();
    }

    // 用户成就取得状态DTO
    public class UserAchievementStatusDto
    {
        public int HasGained { get; set; }
    }
} 