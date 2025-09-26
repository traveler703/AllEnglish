using AllEnBackend.Models;
using AllEnBackend.Dtos;

namespace AllEnBackend.Repository
{
    public interface IAchievementRepository
    {
        // 根据成就ID查询成就信息
        Task<AchievementDto?> GetAchievementByIdAsync(int achievementId);
        
        // 根据用户ID查询该用户所有已取得的成就
        Task<UserGainedAchievementsDto> GetUserGainedAchievementsAsync(string userId);
        
        // 根据用户ID和成就ID查询该用户是否已取得该成就
        Task<UserAchievementStatusDto> GetUserAchievementStatusAsync(string userId, int achievementId);
        
        // 根据用户ID查询所有成就的取得情况
        Task<List<UserAchievementDto>> GetAllUserAchievementsAsync(string userId);
    }
} 