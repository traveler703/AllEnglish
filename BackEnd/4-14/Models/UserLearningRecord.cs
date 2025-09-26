namespace AllEnBackend.Models
{
    public class UserLearningRecord
    {
        public string UserId { get; set; } = string.Empty;
        public int ArticleCount { get; set; } = 0;
        public int WordCount { get; set; } = 0;
        public int OralTime { get; set; } = 0;
        public int ListeningTime { get; set; } = 0;
        public int ArticlePerDay { get; set; } = 0;
        public int WordPerDay { get; set; } = 0;
        public int OralPerDay { get; set; } = 0;
        public int ListeningPerDay { get; set; } = 0;

        // 导航属性，引用User表
        public User User { get; set; } = null!;
    }
}
