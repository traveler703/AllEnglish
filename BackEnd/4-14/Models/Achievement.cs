namespace AllEnBackend.Models
{
    public class Achievement
    {
        public int Id { get; set; }                           // 成就ID
        public string Title { get; set; } = string.Empty;     // 成就标题
        public string Description { get; set; } = string.Empty; // 成就描述
        public int CoinCount { get; set; } = 0;               // 所需金币
        public int ArticleCount { get; set; } = 0;            // 所需阅读文章总量
        public int WordCount { get; set; } = 0;               // 所需学习单词总量
        public int OralTime { get; set; } = 0;                // 所需口语训练总时长
        public int ListeningTime { get; set; } = 0;           // 所需听力训练总时长
        public int ArticlePerday { get; set; } = 0;           // 单日所需阅读文章数量
        public int WordPerday { get; set; } = 0;              // 单日所需学习单词数量
        public int OralPerday { get; set; } = 0;              // 单日所需口语训练时长
        public int ListeningPerday { get; set; } = 0;         // 单日所需听力训练时长
    }
} 