using AllEnBackend.Models;

namespace AllEnBackend.Repository
{
    // 用户每日单词学习仓储接口
    public interface IUserDailyWordsRepository
    {
        // 根据用户ID和日期获取用户每日单词学习记录
        Task<UserDailyWords?> GetByUserIdAndDateAsync(string userId, DateTime studyDate);
        
        // 创建或更新用户每日单词学习记录
        Task<bool> CreateOrUpdateAsync(UserDailyWords record);
    }
}
