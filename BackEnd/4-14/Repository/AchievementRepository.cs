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

        // 根据成就ID查询成就信息
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

        // 根据用户ID查询该用户所有已取得的成就
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

        // 根据用户ID和成就ID查询该用户是否已取得该成就
        public async Task<UserAchievementStatusDto> GetUserAchievementStatusAsync(string userId, int achievementId)
        {
            var userAchievement = await _context.UserAchievements
                .Where(ua => ua.UserId == userId && ua.AchievementId == achievementId)
                .FirstOrDefaultAsync();

            // 如果用户没有该成就的记录，返回未取得状态
            return new UserAchievementStatusDto
            {
                HasGained = userAchievement?.HasGained ?? 0
            };
        }

        // 根据用户ID查询所有成就的取得情况
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