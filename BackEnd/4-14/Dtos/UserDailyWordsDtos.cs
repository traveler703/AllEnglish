namespace AllEnBackend.Dtos
{
    // 获取用户每日单词学习数量请求DTO
    public class GetUserDailyWordsRequestDto
    {
        public string UserId { get; set; } = string.Empty;    // 用户ID
        public DateTime StudyDate { get; set; }    // 学习日期
    }

    // 用户每日单词学习数量响应DTO
    public class UserDailyWordsResponseDto
    {
        public string UserId { get; set; } = string.Empty;    // 用户ID
        public DateTime StudyDate { get; set; }    // 学习日期
        public int WordCount { get; set; }    // 学习的单词数量
    }
}
