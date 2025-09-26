using AllEnBackend.Dtos;
using AllEnBackend.Repository;


namespace AllEnBackend.Services
{
    public class AchievementService : IAchievementService
    {
        private readonly IAchievementRepository _achievementRepository;

        public AchievementService(IAchievementRepository achievementRepository)
        {
            _achievementRepository = achievementRepository;
        }

        // 根据成就的ID查询成就的信息
        public async Task<AchievementDto?> GetAchievementByIdAsync(int achievementId)
        {
            return await _achievementRepository.GetAchievementByIdAsync(achievementId);
        }

        // 根据用户ID查询该用户所有已取得的成就
        public async Task<UserGainedAchievementsDto> GetUserGainedAchievementsAsync(string userId)
        {
            return await _achievementRepository.GetUserGainedAchievementsAsync(userId);
        }

        // 根据用户ID和成就ID查询该用户是否已取得该成就
        public async Task<UserAchievementStatusDto> GetUserAchievementStatusAsync(string userId, int achievementId)
        {
            return await _achievementRepository.GetUserAchievementStatusAsync(userId, achievementId);
        }

        // 根据用户ID查询所有成就的取得情况
        public async Task<List<UserAchievementDto>> GetAllUserAchievementsAsync(string userId)
        {
            return await _achievementRepository.GetAllUserAchievementsAsync(userId);
        }
    }
} 