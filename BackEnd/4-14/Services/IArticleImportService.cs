using AllEnBackend.Models;

namespace AllEnBackend.Services
{
    public interface IArticleImportService
    {
        Task<int> ImportArticlesFromJsonAsync(string jsonFilePath);
        Task<int> ImportArticlesFromJsonStringAsync(string jsonContent);
        Task<List<Article>> ParseArticlesFromJsonAsync(string jsonContent);
    }
}
