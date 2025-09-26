using AllEnBackend.Dtos;

namespace AllEnBackend.Services
{
    public interface IAchievementService
    {
        // ���ݳɾ͵�ID��ѯ�ɾ͵���Ϣ
        Task<AchievementDto?> GetAchievementByIdAsync(int achievementId);
        
        // �����û�ID��ѯ���û�������ȡ�õĳɾ�
        Task<UserGainedAchievementsDto> GetUserGainedAchievementsAsync(string userId);
        
        // �����û�ID�ͳɾ�ID��ѯ���û��Ƿ���ȡ�øóɾ�
        Task<UserAchievementStatusDto> GetUserAchievementStatusAsync(string userId, int achievementId);
        
        // �����û�ID��ѯ���гɾ͵�ȡ�����
        Task<List<UserAchievementDto>> GetAllUserAchievementsAsync(string userId);
    }
} 