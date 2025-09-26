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
                    throw new FileNotFoundException($"δ�ҵ� JSON �ļ�: {jsonFilePath}");

                var jsonContent = await File.ReadAllTextAsync(jsonFilePath);
                return await ImportArticlesFromJsonStringAsync(jsonContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "���� JSON �ļ�ʱ����");
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
                        _logger.LogInformation("�Ѵ������£�����: {Title}", article.Title);
                        continue;
                    }

                    await _articleRepository.AddAsync(article);
                    importCount++;
                }

                return importCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "���� JSON �ַ���ʧ��");
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
                _logger.LogError(ex, "���� JSON ����ʧ��");
                throw new InvalidOperationException("JSON ��ʽ����", ex);
            }
        }
        private int DifficultyLevelToInt(string? level)
        {
            return level switch
            {
                "����" => 1,
                "�м�" => 2,
                "�߼�" => 3,
                _ => 0 // ����ǿջ�δ֪������ 0
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
