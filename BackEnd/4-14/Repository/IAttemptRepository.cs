using AllEnBackend.Models;
using AllEnBackend.Dtos;

namespace AllEnBackend.Repository
{
    public interface IAttemptRepository
    {
        Task<Attempt> InsertAsync(Attempt attempt);

        Task<int> GetMaxIdAsync();

        Task<int> GetHighestScoreAsync(string userId, int articleId);
        Task<int> GetCompletedArticleCountAsync(string userId);
        Task<List<int>> GetCompletedArticleIdsAsync(string userId);
    }
}
