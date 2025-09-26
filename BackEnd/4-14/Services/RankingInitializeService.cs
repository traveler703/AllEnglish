using AllEnBackend.Data;
using AllEnBackend.Models;
using AllEnBackend.Dtos;
using Microsoft.EntityFrameworkCore;

namespace AllEnBackend.Services
{
    public class RankingInitializeService : IRankingInitializeService    
    {
        private readonly AppDbContext _context;

        // 分数计算规则
        private const int WORD_LEARNED_SCORE = 10;
        private const int ACTIVITY_SCORE_PER_WORD = 5;

        public RankingInitializeService(AppDbContext context)
        {
            _context = context;
        }

        public async Task InitializeExistingUsersAsync()
        {
            var allUsers = await _context.Users
                .ToListAsync();

            foreach (var user in allUsers)
            {
                var stats = await CalculateUserLearningStatsAsync(user.Id);

                var ranking = await _context.UserRankingData
                    .FirstOrDefaultAsync(r => r.UserId == user.Id);

                if (ranking != null)
                {
                    // 已有：更新
                    ranking.Username = user.UserName;
                    ranking.Score = stats.TotalScore;
                    ranking.ActivityScore = stats.ActivityScore;
                    ranking.WordCount = stats.LearnedWordCount;
                    ranking.ReadingCount = stats.ReadingCount;
                    ranking.ListeningCount = stats.ListeningCount;
                    ranking.LastUpdated = DateTime.UtcNow;
                }
                else
                {
                    // 新建：插入
                    _context.UserRankingData.Add(new UserRankingData
                    {
                        UserId = user.Id,
                        Username = user.UserName,
                        Score = stats.TotalScore,
                        ActivityScore = stats.ActivityScore,
                        WordCount = stats.LearnedWordCount,
                        ReadingCount = stats.ReadingCount,
                        ListeningCount = stats.ListeningCount,
                        CurrentRankScore = 0,
                        CurrentRankActivity = 0,
                        LastRankScore = 0,
                        LastRankActivity = 0,
                        CreatedAt = DateTime.UtcNow,
                        LastUpdated = DateTime.UtcNow
                    });
                }
                
            }
            await _context.SaveChangesAsync();
            await RecalculateRankingsAsync();
        }

        public async Task<UserLearningStats> CalculateUserLearningStatsAsync(string userId)
        {
            // 单词部分更新
            var learningWordRecords = await _context.Set<UserWord>() 
                .Where(r => r.UserId == userId)
                .ToListAsync();
            
            // 文章部分更新
            int learningArticleScore = await _context.Set<Attempt>()
                .Where(r => r.UserId == userId)
                .GroupBy(r => r.ArticleId)
                .Select(g => g.Max( a=> a.TotalScore))
                .SumAsync();

            // 听力部分更新
            var listeningPracticeScore = await _context.Set<ListeningPracticeRecord>()
                .Where(r => r.UserId == userId)
                .GroupBy(r => r.ListeningPaperId)
                .Select(g => g.Max(r => r.Score))
                .SumAsync();

            var stats = new UserLearningStats
            {
                UserId = userId,
                LearnedWordCount = learningWordRecords.Count(r => r.HasLearned == 1),   // 已学习的单词数
                TotalLearningCount = learningWordRecords.Sum(r => r.LearnCount),        // 总学习次数
                TotalCorrectCount = learningWordRecords.Sum(r => r.CorrectCount),       // 总正确次数
                BookmarkedCount = learningWordRecords.Count(r => r.HasBookmarked == 1),  // 收藏的单词数
                ReadingCount = learningArticleScore ,
                ListeningCount = listeningPracticeScore ,
            };

            // 计算总分数
            stats.TotalScore = CalculateTotalScore(stats);

            // 计算活跃度分数
            stats.ActivityScore = CalculateActivityScore(stats);

            return stats;
        }

        private int CalculateTotalScore(UserLearningStats stats)
        {
            int totalScore = 0;

            // 基于学会的单词数量得分
            totalScore = stats.LearnedWordCount + stats.TotalLearningCount + stats.ReadingCount*3 + stats.ListeningCount*3;

            return totalScore;
        }

        private int CalculateActivityScore(UserLearningStats stats)
        {
            int activityScore = 0;

            // 基于学习的单词数量
            activityScore = stats.LearnedWordCount + stats.TotalLearningCount + stats.ReadingCount * 2 + stats.ListeningCount * 2 ;

            return activityScore;
        }

        public async Task InitializeNewUserAsync(string userId, string username)
        {
            // 检查是否已存在（防重复）
            var exists = await _context.UserRankingData.AnyAsync(r => r.UserId == userId);
            if (exists)
            { 
                return;
            }

            // 获取当前总用户数，新用户排名为最后一名
            var totalUsers = await _context.UserRankingData.CountAsync();
            var newRank = totalUsers + 1;

            // 创建排行榜数据
            var userRanking = new UserRankingData
            {
                UserId = userId,
                Username = username,
                Score = 0,
                ActivityScore = 0,
                ReadingCount = 0,
                WordCount = 0,
                ListeningCount = 0,
                CurrentRankScore = newRank,      // 新用户排名最后
                CurrentRankActivity = newRank,   // 新用户排名最后
                LastRankScore = 0,
                LastRankActivity = 0,
                CreatedAt = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow
            };

            // 创建最佳排名记录
            var bestRanking = new UserBestRanking
            {
                UserId = userId,
                BestRankScore = newRank,
                BestRankActivity = newRank,
                BestScore = 0,
                BestActivityScore = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.UserRankingData.Add(userRanking);
            _context.UserBestRanking.Add(bestRanking);
            await _context.SaveChangesAsync();
        }

        public async Task RecalculateRankingsAsync()
        {
            // 按分数排序更新排名
            var usersByScore = await _context.UserRankingData
                .OrderByDescending(x => x.Score)
                .ThenBy(x => x.Username)  // 分数相同按用户名排序
                .ToListAsync();

            for (int i = 0; i < usersByScore.Count; i++)
            {
                usersByScore[i].CurrentRankScore = i + 1;
            }

            // 按活跃度排序更新排名
            var usersByActivity = await _context.UserRankingData
                .OrderByDescending(x => x.ActivityScore)
                .ThenBy(x => x.Username)
                .ToListAsync();

            for (int i = 0; i < usersByActivity.Count; i++)
            {
                var user = usersByActivity[i];
                var userInScoreList = usersByScore.First(u => u.UserId == user.UserId);
                userInScoreList.CurrentRankActivity = i + 1;
            }

            _context.UserRankingData.UpdateRange(usersByScore);
            await _context.SaveChangesAsync();
        }
    }
}
