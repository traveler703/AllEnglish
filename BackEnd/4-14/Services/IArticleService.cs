using AllEnBackend.Dtos;

namespace AllEnBackend.Services
{
    public interface IArticleService
    {
        Task<PaginatedResult<ArticleListDto>> GetArticlesAsync(ArticleQueryParams queryParams);
        Task<ArticleDto?> GetArticleByIdAsync(int id);
        Task<List<ArticleListDto>> GetArticlesByCourseIdAsync(int courseId);
        Task<List<ArticleListDto>> GetArticlesByDifficultyAsync(int difficulty);
        Task<List<ArticleListDto>> GetArticlesByCategoryAsync(string category);
        Task<ArticleDetailDto?> GetArticleWithQuestionsAsync(int id);
        Task<AnswerResultDto> SubmitAnswerAsync(SubmitAnswerDto submitDto);
        Task<int> GetNextIdAsync();
        Task<int> GetHighestScoreAsync(string userId, int articleId);
        Task<int> GetCompletedArticleCountAsync(string userId);
        Task<List<int>> GetCompletedArticleIdsAsync(string userId);
    }
}
