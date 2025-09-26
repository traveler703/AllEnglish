namespace AllEnBackend.Dtos
{
    // 单词详细信息数据传输对象
    public class WordDetailDto
    {
        // 单词名称
        public string WordName { get; set; } = string.Empty;

        // 英文释义列表
        public List<string> EnglishDefinitions { get; set; } = new();

        // 中文翻译列表
        public List<string> ChineseTranslations { get; set; } = new();

        // 音标列表
        public List<string> Phonetics { get; set; } = new();

        // 例句列表
        public List<string> Examples { get; set; } = new();
    }

    // 用户单词详细信息数据传输对象
    public class UserWordDetailDto
    {
        // 单词ID
        public int WordId { get; set; }

        // 单词名称
        public string WordName { get; set; } = string.Empty;

        // 是否已学习
        public bool HasLearned { get; set; }

        // 正确回答次数
        public int CorrectCount { get; set; }

        // 学习次数
        public int LearnCount { get; set; }

        // 是否已收藏
        public bool HasBookmarked { get; set; }
    }

    // 用户单词查询条件数据传输对象
    public class UserWordQueryDto
    {
        // 学习状态筛选 (-1表示不考虑此属性)
        public int HasLearned { get; set; } = -1;

        // 收藏状态筛选 (-1表示不考虑此属性)
        public int HasBookmarked { get; set; } = -1;

        // 最小正确次数筛选 (-1表示不考虑此属性)
        public int MinCorrectCount { get; set; } = -1;

        // 最大正确次数筛选 (-1表示不考虑此属性)
        public int MaxCorrectCount { get; set; } = -1;

        // 最小学习次数筛选 (-1表示不考虑此属性)
        public int MinLearnCount { get; set; } = -1;

        // 最大学习次数筛选 (-1表示不考虑此属性)
        public int MaxLearnCount { get; set; } = -1;

        // 考纲范围筛选 (格式如 "cet4,cet6" 或 "postgraduate"，"-1"表示不考虑)
        public string SyllabusScope { get; set; } = "-1";
    }

    // 用户单词列表数据传输对象
    public class UserWordDto
    {
        // 单词ID列表
        public List<int> WordIds { get; set; } = new();

        // 符合条件的单词总数
        public int TotalCount { get; set; }
    }
}