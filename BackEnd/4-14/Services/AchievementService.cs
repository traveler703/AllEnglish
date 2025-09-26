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

        // ���ݳɾ͵�ID��ѯ�ɾ͵���Ϣ
        public async Task<AchievementDto?> GetAchievementByIdAsync(int achievementId)
        {
            return await _achievementRepository.GetAchievementByIdAsync(achievementId);
        }

        // �����û�ID��ѯ���û�������ȡ�õĳɾ�
        public async Task<UserGainedAchievementsDto> GetUserGainedAchievementsAsync(string userId)
        {
            return await _achievementRepository.GetUserGainedAchievementsAsync(userId);
        }

        // �����û�ID�ͳɾ�ID��ѯ���û��Ƿ���ȡ�øóɾ�
        public async Task<UserAchievementStatusDto> GetUserAchievementStatusAsync(string userId, int achievementId)
        {
            return await _achievementRepository.GetUserAchievementStatusAsync(userId, achievementId);
        }

        // �����û�ID��ѯ���гɾ͵�ȡ�����
        public async Task<List<UserAchievementDto>> GetAllUserAchievementsAsync(string userId)
        {
            return await _achievementRepository.GetAllUserAchievementsAsync(userId);
        }
    }
} 