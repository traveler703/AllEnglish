using AllEnBackend.Models;

namespace AllEnBackend.Repository
{
    public interface IReadingQuestionRepository
    {
        Task<IEnumerable<ReadingQuestion>> GetByArticleIdAsync(int articleId);
    }
}