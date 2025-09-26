using AllEnBackend.Dtos;
using AllEnBackend.Models;

namespace AllEnBackend.Repository
{
    public interface IArticleRepository
    {
        Task<(List<Article> Items, int TotalCount)> GetArticlesAsync(ArticleQueryParams queryParams);
        Task<Article?> GetArticleByIdAsync(int id);
        Task<List<Article>> GetArticlesByCourseIdAsync(int courseId);
        Task<List<Article>> GetArticlesByDifficultyAsync(int difficulty);
        Task<List<Article>> GetArticlesByCategoryAsync(string category);

        Task<ArticleDetailDto> GetArticleWithQuestionsAsync(int articleId);

        Task<Article> GetByIdAsync(int id);

        // 导入功能
        Task<Article?> GetByTitleAsync(string title);   // 用于判断是否已存在文章
        Task AddAsync(Article article);                 // 用于添加新文章
    }
}
