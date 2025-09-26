using AllEnBackend.Dtos;

namespace AllEnBackend.Services
{
    // 用户学习记录服务接口
    public interface IUserLearningRecordService
    {
        // 根据用户ID获取用户学习记录
        Task<UserLearningRecordDto?> GetUserLearningRecordAsync(string userId);
        
        // 更新用户学习记录
        Task<bool> UpdateUserLearningRecordAsync(UpdateUserLearningRecordDto updateDto);
    }
}
