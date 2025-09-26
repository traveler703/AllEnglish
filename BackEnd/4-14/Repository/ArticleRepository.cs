using Microsoft.EntityFrameworkCore;
using AllEnBackend.Data;
using AllEnBackend.Dtos;
using AllEnBackend.Models;

namespace AllEnBackend.Repository
{
    public class ArticleRepository : IArticleRepository
    {
        private readonly AppDbContext _context;

        public ArticleRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(List<Article> Items, int TotalCount)> GetArticlesAsync(ArticleQueryParams queryParams)
        {
            IQueryable<Article> query = _context.Articles;

            if (queryParams.CourseId.HasValue)
                query = query.Where(a => a.CourseId == queryParams.CourseId.Value);

            if (queryParams.Difficulty.HasValue)
                query = query.Where(a => a.Difficulty == queryParams.Difficulty.Value);

            if (!string.IsNullOrEmpty(queryParams.Category))
                query = query.Where(a => a.Category == queryParams.Category);

            if (!string.IsNullOrEmpty(queryParams.SearchTerm))
                query = query.Where(a => a.Title.Contains(queryParams.SearchTerm) ||
                                         a.Content.Contains(queryParams.SearchTerm));

            int totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(a => a.ArticleId)
                .Skip((queryParams.PageNumber - 1) * queryParams.PageSize)
                .Take(queryParams.PageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<Article?> GetArticleByIdAsync(int id)
        {
            return await _context.Articles.FirstOrDefaultAsync(a => a.ArticleId == id);
        }

        public async Task<List<Article>> GetArticlesByCourseIdAsync(int courseId)
        {
            return await _context.Articles
                .Where(a => a.CourseId == courseId)
                .OrderBy(a => a.ArticleId)
                .ToListAsync();
        }

        public async Task<List<Article>> GetArticlesByDifficultyAsync(int difficulty)
        {
            return await _context.Articles
                .Where(a => a.Difficulty == difficulty)
                .OrderBy(a => a.ArticleId)
                .ToListAsync();
        }

        public async Task<List<Article>> GetArticlesByCategoryAsync(string category)
        {
            return await _context.Articles
                .Where(a => a.Category == category)
                .OrderBy(a => a.ArticleId)
                .ToListAsync();
        }

        // 新增：根据标题查找文章（用于判断是否重复）
        public async Task<Article?> GetByTitleAsync(string title)
        {
            return await _context.Articles.FirstOrDefaultAsync(a => a.Title == title);
        }

        // 新增：添加文章
        public async Task AddAsync(Article article)
        {
            await _context.Articles.AddAsync(article);
        }

        public async Task<Article> GetByIdAsync(int id)
        {
            return await _context.Articles
                .FirstOrDefaultAsync(a => a.ArticleId == id);
        }


        // 查询文章及相关题目
        public async Task<ArticleDetailDto> GetArticleWithQuestionsAsync(int articleId)
        {
            // 查询文章
            var article = await _context.Articles
                .FirstOrDefaultAsync(a => a.ArticleId == articleId);

            if (article == null)
                return null;

            // 获取难度级别描述
            string difficultyLevel = article.Difficulty switch
            {
                1 => "基础",
                2 => "进阶",
                3 => "高级",
                _ => "未知"
            };

            // 查询相关题目    
            var questions = await _context.Questions
                .Where(q => q.ArticleId == articleId)
                .OrderBy(q => q.Seqo)
                .ToListAsync();

            // 组合结果
            var articleDetail = new ArticleDetailDto
            {
                ArticleId = article.ArticleId,
                CourseId = article.CourseId,
                Title = article.Title,
                Content = article.Content,
                Difficulty = article.Difficulty,
                DifficultyLevel = difficultyLevel,
                CoverImage = article.CoverImage,
                Category = article.Category,
                ReadingTime = article.ReadingTime,
                WordCount = article.WordCount,
                Description = article.Content.Length > 100 ? article.Content.Substring(0, 100) + "..." : article.Content,
                CreatedAt = article.CreatedAt,
                UpdatedAt = article.UpdatedAt,
                Tags = article.Tags ?? new List<string>(),
                Questions = questions.Select(q => new QuestionDto
                {
                    Id = q.Id,
                    ArticleId = q.ArticleId,
                    Seqo = q.Seqo ?? string.Empty,
                    Kind = q.Kind,
                    Stem = q.Stem ?? string.Empty,
                    Options = q.Options ?? string.Empty,
                    AnswerKey = q.AnswerKey ?? string.Empty,
                    Score = q.Score,
                    CreatedAt = q.CreatedAt
                }).ToList()
            };

            return articleDetail;
        }
    }
}
