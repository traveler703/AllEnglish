using AllEnBackend.Models;
using AllEnBackend.Repository;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AllEnBackend.Services
{
    public class ArticleImportService : IArticleImportService
    {
        private readonly IArticleRepository _articleRepository;
        private readonly ILogger<ArticleImportService> _logger;

        public ArticleImportService(IArticleRepository articleRepository, ILogger<ArticleImportService> logger)
        {
            _articleRepository = articleRepository;
            _logger = logger;
        }

        public async Task<int> ImportArticlesFromJsonAsync(string jsonFilePath)
        {
            try
            {
                if (!File.Exists(jsonFilePath))
                    throw new FileNotFoundException($"未找到 JSON 文件: {jsonFilePath}");

                var jsonContent = await File.ReadAllTextAsync(jsonFilePath);
                return await ImportArticlesFromJsonStringAsync(jsonContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "导入 JSON 文件时出错");
                throw;
            }
        }

        public async Task<int> ImportArticlesFromJsonStringAsync(string jsonContent)
        {
            try
            {
                var articles = await ParseArticlesFromJsonAsync(jsonContent);
                int importCount = 0;

                foreach (var article in articles)
                {
                    var existing = await _articleRepository.GetByTitleAsync(article.Title);
                    if (existing != null)
                    {
                        _logger.LogInformation("已存在文章，跳过: {Title}", article.Title);
                        continue;
                    }

                    await _articleRepository.AddAsync(article);
                    importCount++;
                }

                return importCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "导入 JSON 字符串失败");
                throw;
            }
        }

        public async Task<List<Article>> ParseArticlesFromJsonAsync(string jsonContent)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    Converters = { new JsonStringEnumConverter() }
                };

                var wrapper = JsonSerializer.Deserialize<ArticleJsonData>(jsonContent, options);

                var articles = new List<Article>();
                foreach (var item in wrapper.Articles)
                {
                    var article = new Article
                    {
                        Title = item.Title,
                        Content = item.Content,
                        Category = item.ArticleType, 
                        Difficulty = DifficultyLevelToInt(item.DifficultyLevel),
                        CoverImage = item.CoverImage,
                        WordCount = item.WordCount,
                        Tags = item.Tags ?? new List<string>(),
                        CreatedAt = item.CreatedAt ?? DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    articles.Add(article);
                }

                return articles;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "解析 JSON 数据失败");
                throw new InvalidOperationException("JSON 格式错误", ex);
            }
        }
        private int DifficultyLevelToInt(string? level)
        {
            return level switch
            {
                "初级" => 1,
                "中级" => 2,
                "高级" => 3,
                _ => 0 // 如果是空或未知，返回 0
            };
        }

    }

    public class ArticleJsonData
    {
        public List<ArticleJsonItem> Articles { get; set; } = new();
    }

    public class ArticleJsonItem
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string ArticleType { get; set; } = string.Empty;
        public string DifficultyLevel { get; set; } = string.Empty;
        public string CourseLevel { get; set; } = string.Empty;
        public string? CoverImage { get; set; }
        public int EstimatedTime { get; set; }
        public int WordCount { get; set; }
        public List<string>? Tags { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
