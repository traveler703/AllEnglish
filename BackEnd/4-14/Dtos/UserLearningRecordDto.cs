namespace AllEnBackend.Dtos
{
    // 用户学习记录响应DTO
    public class UserLearningRecordDto
    {
        public string UserId { get; set; } = string.Empty;    // 用户ID
        public int ArticleCount { get; set; }    // 已阅读的文章的总量
        public int WordCount { get; set; }    // 已学习的单词的总量
        public int OralTime { get; set; }    // 已训练的口语的总时长（分钟）
        public int ListeningTime { get; set; }    // 已训练的听力的总时长（分钟）
        public int ArticlePerDay { get; set; }    // 单日已阅读的文章的数量
        public int WordPerDay { get; set; }    // 单日已学习的单词的数量
        public int OralPerDay { get; set; }    // 单日已训练的口语的时长（分钟）
        public int ListeningPerDay { get; set; }    // 单日已训练的听力的时长（分钟）
    }

    // 获取用户学习记录请求DTO
    public class GetUserLearningRecordRequestDto
    {
        public string UserId { get; set; } = string.Empty;    // 用户ID
    }

    // 更新用户学习记录请求DTO
    public class UpdateUserLearningRecordDto
    {
        public string UserId { get; set; } = string.Empty;    // 用户ID
        public int ArticleCount { get; set; }    // 已阅读的文章的总量
        public int WordCount { get; set; }    // 已学习的单词的总量
        public int OralTime { get; set; }    // 已训练的口语的总时长（分钟）
        public int ListeningTime { get; set; }    // 已训练的听力的总时长（分钟）
        public int ArticlePerDay { get; set; }    // 单日已阅读的文章的数量
        public int WordPerDay { get; set; }    // 单日已学习的单词的数量
        public int OralPerDay { get; set; }    // 单日已训练的口语的时长（分钟）
        public int ListeningPerDay { get; set; }    // 单日已训练的听力的时长（分钟）
    }
}
