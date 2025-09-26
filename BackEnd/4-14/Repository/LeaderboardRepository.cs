using AllEnBackend.Data;
using AllEnBackend.Dtos;
using AllEnBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace AllEnBackend.Repository
{
    public class LeaderboardRepository : ILeaderboardRepository
    {
        private readonly AppDbContext _context;

        public LeaderboardRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UserRankingData?> GetByUserIdAsync(string userId)
        {
            return await _context.UserRankingData
                .FirstOrDefaultAsync(x => x.UserId == userId);
        }

        public async Task<UserRankingData> CreateAsync(UserRankingData userRanking)
        {
            _context.UserRankingData.Add(userRanking);
            await _context.SaveChangesAsync();
            return userRanking;
        }

        public async Task<UserRankingData> UpdateAsync(UserRankingData userRanking)
        {
            userRanking.LastUpdated = DateTime.UtcNow;
            _context.UserRankingData.Update(userRanking);
            await _context.SaveChangesAsync();
            return userRanking;
        }

        public async Task<List<UserRankingData>> GetLeaderboardByScoreAsync(int page, int size)
        {
            return await _context.UserRankingData
                .OrderByDescending(x => x.Score)
                .ThenBy(x => x.Username)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();
        }

        public async Task<List<UserRankingData>> GetLeaderboardByActivityAsync(int page, int size)
        {
            return await _context.UserRankingData
                .OrderByDescending(x => x.ActivityScore)
                .ThenBy(x => x.Username)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();
        }

        public async Task<List<UserRankingData>> GetLeaderboardByUsernameAsync(int page, int size)
        {
            return await _context.UserRankingData
                .OrderBy(x => x.Username)
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();
        }

        public async Task<long> GetTotalUsersCountAsync()
        {
            return await _context.UserRankingData.CountAsync();
        }

        public async Task<List<UserRankingData>> GetAllOrderByScoreAsync()
        {
            return await _context.UserRankingData
                .OrderByDescending(x => x.Score)
                .ThenBy(x => x.Username)
                .ToListAsync();
        }

        public async Task<List<UserRankingData>> GetAllOrderByActivityAsync()
        {
            return await _context.UserRankingData
                .OrderByDescending(x => x.ActivityScore)
                .ThenBy(x => x.Username)
                .ToListAsync();
        }

        public async Task UpdateRankingsAsync(List<UserRankingData> rankings)
        {
            _context.UserRankingData.UpdateRange(rankings);
            await _context.SaveChangesAsync();
        }

        public async Task<List<UserRankingData>> GetNearbyUsersAsync(string userId, string type, int count = 5)
        {
            var user = await GetByUserIdAsync(userId);
            if (user == null) return new List<UserRankingData>();

            var currentRank = type.ToLower() == "score" ? user.CurrentRankScore : user.CurrentRankActivity;
            var startRank = Math.Max(1, currentRank - count / 2);
            var endRank = currentRank + count / 2;

            if (type.ToLower() == "score")
            {
                return await _context.UserRankingData
                    .Where(x => x.CurrentRankScore >= startRank && x.CurrentRankScore <= endRank)
                    .OrderBy(x => x.CurrentRankScore)
                    .ToListAsync();
            }
            else
            {
                return await _context.UserRankingData
                    .Where(x => x.CurrentRankActivity >= startRank && x.CurrentRankActivity <= endRank)
                    .OrderBy(x => x.CurrentRankActivity)
                    .ToListAsync();
            }
        }

        public async Task<UserBestRanking?> GetByUserIdBestRankAsync(string userId)
        {
            return await _context.UserBestRanking
                .FirstOrDefaultAsync(x => x.UserId == userId);
        }

        public async Task<UserBestRanking> CreateAsync(UserBestRanking bestRanking)
        {
            _context.UserBestRanking.Add(bestRanking);
            await _context.SaveChangesAsync();
            return bestRanking;
        }

        public async Task<UserBestRanking> UpdateAsync(UserBestRanking bestRanking)
        {
            bestRanking.UpdatedAt = DateTime.UtcNow;
            _context.UserBestRanking.Update(bestRanking);
            await _context.SaveChangesAsync();
            return bestRanking;
        }
        
        public async Task UpdateWordLearning(string UserId)
        {
            int learnedCount = await _context.UserWords
                .Where(uw => uw.UserId == UserId && uw.HasLearned == 1)
                .CountAsync();
            
            // 在 UserRankingData 表中更新用户学习单词数量
            var userRanking = await _context.UserRankingData.FirstOrDefaultAsync(ur => ur.UserId == UserId);

            if(userRanking != null)
            {
                userRanking.Score += learnedCount - userRanking.WordCount;
                userRanking.ActivityScore += learnedCount - userRanking.WordCount;
                await InserHistory(UserId, learnedCount - userRanking.WordCount, learnedCount - userRanking.WordCount);
                userRanking.WordCount = learnedCount;
            }

            await _context.SaveChangesAsync();
        }

        public async Task UpdateArticleLearning(string UserId)
        {
            int completedArticles = await _context.Attempts
                .Where(a => a.UserId == UserId && a.Status == "已完成")
                .GroupBy(a => a.ArticleId)
                .Select(g => g.Max(a => a.TotalScore))
                .SumAsync();

            var userRanking = await _context.UserRankingData
                .FirstOrDefaultAsync(ur => ur.UserId == UserId);

            int delta = 0;

            if (userRanking != null)
            {
                delta = completedArticles - userRanking.ReadingCount;
                userRanking.Score += 3*(completedArticles-userRanking.ReadingCount);
                userRanking.ActivityScore += 2 * (completedArticles - userRanking.ReadingCount);
                userRanking.ReadingCount = completedArticles;
            }
            else
            {
                delta = completedArticles;
                _context.UserRankingData.Add(new UserRankingData
                {
                    UserId = UserId,
                    ReadingCount = completedArticles,
                    Score = completedArticles 
                });
            }
            await InserHistory(UserId, 3*delta, 2*delta);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateListeningLearning(string UserId)
        {
            int completedListening = await _context.ListeningPracticeRecords
                .Where(a => a.UserId == UserId)
                .GroupBy(a => a.ListeningPaperId)
                .Select(g => g.Max(a => a.Score))
                .CountAsync();

            var userRanking = await _context.UserRankingData
                .FirstOrDefaultAsync(ur => ur.UserId == UserId);

            if (userRanking != null)
            {
                userRanking.Score += 3 * (completedListening - userRanking.ListeningCount);
                userRanking.ActivityScore += 2 * (completedListening - userRanking.ListeningCount);
                await InserHistory(UserId, 3 * (completedListening - userRanking.ListeningCount), 2 * (completedListening - userRanking.ListeningCount));
                userRanking.ListeningCount = completedListening;
            }
            await _context.SaveChangesAsync();
        }

        private async Task InserHistory(string userId, int scoreDelta,int activityDelta)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            var maxId = await _context.UserRankHistory.MaxAsync(f => (int?)f.Id) ?? 0;
            var newId = maxId + 1;


            var history = new UserRankHistory
            {
                Id = newId,
                UserId = userId,
                UserName = user.UserName,
                Score = scoreDelta,
                Activity = activityDelta,
                CreatedAt = DateTime.Now
            };

            _context.UserRankHistory.Add(history);
            await _context.SaveChangesAsync();
        }

        public async Task<List<UserRankHistory>> GetByPeriodAsync(DateTime start,DateTime end)
        {
            return await _context.UserRankHistory
                .Where(x => x.CreatedAt >= start && x.CreatedAt<=end)
                .ToListAsync();
        }

    }
}

