using AllEnBackend.Data;
using AllEnBackend.Models;
using AllEnBackend.Dtos;
using Microsoft.EntityFrameworkCore;

namespace AllEnBackend.Repository
{
    public class AchievementRepository : IAchievementRepository
    {
        private readonly AppDbContext _context;

        public AchievementRepository(AppDbContext context)
        {
            _context = context;
        }

        // ���ݳɾ�ID��ѯ�ɾ���Ϣ
        public async Task<AchievementDto?> GetAchievementByIdAsync(int achievementId)
        {
            var achievement = await _context.Achievements
                .Where(a => a.Id == achievementId)
                .FirstOrDefaultAsync();

            if (achievement == null)
            {
                return null;
            }

            return new AchievementDto
            {
                Id = achievement.Id,
                Title = achievement.Title ?? string.Empty,
                Description = achievement.Description ?? string.Empty,
                CoinCount = achievement.CoinCount,
                ArticleCount = achievement.ArticleCount,
                WordCount = achievement.WordCount,
                OralTime = achievement.OralTime,
                ListeningTime = achievement.ListeningTime,
                ArticlePerday = achievement.ArticlePerday,
                WordPerday = achievement.WordPerday,
                OralPerday = achievement.OralPerday,
                ListeningPerday = achievement.ListeningPerday
            };
        }

        // �����û�ID��ѯ���û�������ȡ�õĳɾ�
        public async Task<UserGainedAchievementsDto> GetUserGainedAchievementsAsync(string userId)
        {
            var achievementIds = await _context.UserAchievements
                .Where(ua => ua.UserId == userId && ua.HasGained == 1)
                .Select(ua => ua.AchievementId)
                .ToListAsync();

            return new UserGainedAchievementsDto
            {
                AchievementIds = achievementIds
            };
        }

        // �����û�ID�ͳɾ�ID��ѯ���û��Ƿ���ȡ�øóɾ�
        public async Task<UserAchievementStatusDto> GetUserAchievementStatusAsync(string userId, int achievementId)
        {
            var userAchievement = await _context.UserAchievements
                .Where(ua => ua.UserId == userId && ua.AchievementId == achievementId)
                .FirstOrDefaultAsync();

            // ����û�û�иóɾ͵ļ�¼������δȡ��״̬
            return new UserAchievementStatusDto
            {
                HasGained = userAchievement?.HasGained ?? 0
            };
        }

        // �����û�ID��ѯ���гɾ͵�ȡ�����
        public async Task<List<UserAchievementDto>> GetAllUserAchievementsAsync(string userId)
        {
            var userAchievements = await _context.UserAchievements
                .Where(ua => ua.UserId == userId)
                .ToListAsync();

            return userAchievements.Select(ua => new UserAchievementDto
            {
                AchievementId = ua.AchievementId,
                HasGained = ua.HasGained,
                GainDate = ua.GainDate
            }).ToList();
        }
    }
} 