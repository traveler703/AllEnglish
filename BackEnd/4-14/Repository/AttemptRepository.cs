using AllEnBackend.Models;
using AllEnBackend.Dtos;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace AllEnBackend.Repository
{
    public class AttemptRepository : IAttemptRepository
    {
        private readonly DbContext _dbContext;

        public AttemptRepository(DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> GetHighestScoreAsync(string userId, int articleId)
        {
            var highestScore = await _dbContext.Set<Attempt>()
                .Where(a => a.UserId == userId && a.ArticleId == articleId)
                .MaxAsync(a => (int?)a.TotalScore) ?? 0;

            return highestScore;
        }

        public async Task<int> GetCompletedArticleCountAsync(string userId)
        {
            var completedCount = await _dbContext.Set<Attempt>()
                .Where(a => a.UserId == userId && a.Status == "已完成")
                .Select(a => a.ArticleId)
                .Distinct()
                .CountAsync();

            return completedCount;
        }

        public async Task<List<int>> GetCompletedArticleIdsAsync(string userId)
        {
            // 返回用户已完成的所有文章ID
            var articleIds = await _dbContext.Set<Attempt>()
                .Where(a => a.UserId == userId && a.Status == "已完成")
                .Select(a => a.ArticleId.Value)
                .Distinct()
                .ToListAsync();

            return articleIds;
        }

        public async Task<Attempt> InsertAsync(Attempt attempt)
        {
            await _dbContext.Set<Attempt>().AddAsync(attempt);
            await _dbContext.SaveChangesAsync();
            return attempt;
        }
        public async Task<int> GetMaxIdAsync()
        {
            if (await _dbContext.Set<Attempt>().CountAsync() == 0)
                return 0;

            var maxId = await _dbContext.Set<Attempt>().MaxAsync(a => (int?)a.Id) ?? 0;
            return maxId;
        }
    }
}
