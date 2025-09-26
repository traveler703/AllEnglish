using AllEnBackend.Dtos;

namespace AllEnBackend.Services
{
    // 用户每日单词学习服务接口
    public interface IUserDailyWordsService
    {
        // 根据用户ID和日期获取用户每日学习的单词数量
        Task<UserDailyWordsResponseDto> GetUserDailyWordsAsync(string userId, DateTime studyDate);
    }
}
