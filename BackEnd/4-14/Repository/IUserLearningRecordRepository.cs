using AllEnBackend.Models;

namespace AllEnBackend.Repository
{
    // 用户学习记录仓储接口
    public interface IUserLearningRecordRepository
    {
        // 根据用户ID获取用户学习记录
        Task<UserLearningRecord?> GetByUserIdAsync(string userId);
        
        // 更新用户学习记录
        Task<bool> UpdateAsync(UserLearningRecord record);
    }
}
