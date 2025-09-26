using AllEnBackend.Dtos;
using AllEnBackend.Models;
using AllEnBackend.Repository;
using Microsoft.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;

namespace AllEnBackend.Services
{
    public class ArticleService : IArticleService
    {
        private readonly IArticleRepository _articleRepository;
        private readonly IReadingQuestionRepository _questionRepository;
        private readonly IAttemptRepository _attemptRepository;
        private readonly ILeaderboardRepository _leaderboardRepository;

        public ArticleService(IArticleRepository articleRepository,IReadingQuestionRepository questionRepository,IAttemptRepository attemptRepository, ILeaderboardRepository leaderboardRepository)
        {
            _articleRepository = articleRepository;
            _questionRepository = questionRepository;
            _attemptRepository = attemptRepository;
            _leaderboardRepository = leaderboardRepository;
        }

        public async Task<AnswerResultDto> SubmitAnswerAsync(SubmitAnswerDto submitDto)
        {
            // 获取文章的所有问题
            var questions = await _questionRepository.GetByArticleIdAsync(submitDto.ArticleID);

            // 计算分数
            int totalScore = 0;
            int maxPossibleScore = 0;
            var questionResults = new List<QuestionResultDto>();

            foreach (var question in questions)
            {
                var userAnswer = submitDto.Answers.FirstOrDefault(a => a.QuestionId == question.Id);
                bool isCorrect = false;
                int earnedScore = 0;

                if (userAnswer != null)
                {
                    // 比较用户答案和正确答案
                    isCorrect = string.Equals(userAnswer.UserResponse?.Trim(),
                                             question.AnswerKey?.Trim(),
                                             StringComparison.OrdinalIgnoreCase);

                    if (isCorrect && question.Score.HasValue)
                    {
                        earnedScore = question.Score.Value;
                        totalScore += earnedScore;
                    }
                }

                if (question.Score.HasValue)
                {
                    maxPossibleScore += question.Score.Value;
                }

                questionResults.Add(new QuestionResultDto
                {
                    QuestionId = question.Id,
                    IsCorrect = isCorrect,
                    Score = earnedScore,
                    CorrectAnswer = question.AnswerKey
                });
            }

            int nextId = await GetNextIdAsync();

            var attempt = new Attempt
            {
                Id = nextId,
                UserId = submitDto.UserId,
                ArticleId = submitDto.ArticleID,
                StartTime = submitDto.StartTime,
                EndTime = submitDto.EndTime,
                TotalScore = totalScore,
                Status = "已完成",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            // 使用仓储保存实体
            await _attemptRepository.InsertAsync(attempt);
            await _leaderboardRepository.UpdateArticleLearning(submitDto.UserId);

            // 返回结果
            return new AnswerResultDto
            {
                TotalScore = totalScore,
                MaxPossibleScore = maxPossibleScore,
                Percentage = maxPossibleScore > 0 ? (double)totalScore / maxPossibleScore * 100 : 0,
                QuestionResults = questionResults
            };
        }

        public async Task<int> GetNextIdAsync()
        {
            int maxId = await _attemptRepository.GetMaxIdAsync();
            return maxId + 1;
        }
        public async Task<int> GetHighestScoreAsync(string userId, int articleId)
        {
            return await _attemptRepository.GetHighestScoreAsync(userId, articleId);
        }

        public async Task<int> GetCompletedArticleCountAsync(string userId)
        {
            return await _attemptRepository.GetCompletedArticleCountAsync(userId);
        }

        public async Task<List<int>> GetCompletedArticleIdsAsync(string userId)
        {
            return await _attemptRepository.GetCompletedArticleIdsAsync(userId);
        }

        public async Task<ArticleDetailDto> GetArticleWithQuestionsAsync(int id)
        {
            // 获取文章
            var article = await _articleRepository.GetByIdAsync(id);
            if (article == null)
                return null;

            // 获取相关题目
            var questions = await _questionRepository.GetByArticleIdAsync(id);

            // 获取难度级别描述
            string difficultyLevel = article.Difficulty switch
            {
                1 => "基础",
                2 => "进阶",
                3 => "高级",
                _ => "未知"
            };

            // 构建返回结果
            var articleDetail = new ArticleDetailDto
            {
                ArticleId = article.ArticleId,
                CourseId = article.CourseId,
                Title = article.Title,
                Content = article.Content,  // 文章全文内容
                Difficulty = article.Difficulty,
                DifficultyLevel = difficultyLevel,
                CoverImage = article.CoverImage,
                Category = article.Category,
                ReadingTime = article.ReadingTime,
                WordCount = article.WordCount,
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

        public ArticleService(IArticleRepository articleRepository)
        {
            _articleRepository = articleRepository;
        }

        public async Task<PaginatedResult<ArticleListDto>> GetArticlesAsync(ArticleQueryParams queryParams)
        {
            var (articles, totalCount) = await _articleRepository.GetArticlesAsync(queryParams);

            var articleDtos = articles.Select(article => new ArticleListDto
            {
                ArticleId = article.ArticleId,
                Title = article.Title,
                Difficulty = article.Difficulty,
                DifficultyLevel = GetDifficultyLevel(article.Difficulty),
                CoverImage = article.CoverImage,
                Category = article.Category,
                ReadingTime = article.ReadingTime,
                WordCount = article.WordCount,
                Description = GetDescription(article.Content)
            }).ToList();

            return new PaginatedResult<ArticleListDto>
            {
                Items = articleDtos,
                TotalItems = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / queryParams.PageSize),
                CurrentPage = queryParams.PageNumber,
                PageSize = queryParams.PageSize
            };
        }

        public async Task<ArticleDto?> GetArticleByIdAsync(int id)
        {
            var article = await _articleRepository.GetArticleByIdAsync(id);
            if (article == null) return null;

            return new ArticleDto
            {
                ArticleId = article.ArticleId,
                CourseId = article.CourseId,
                Title = article.Title,
                Content = article.Content,
                Difficulty = article.Difficulty,
                DifficultyLevel = GetDifficultyLevel(article.Difficulty),
                CoverImage = article.CoverImage,
                Category = article.Category,
                ReadingTime = article.ReadingTime,
                WordCount = article.WordCount,
                Description = GetDescription(article.Content)
            };
        }

        public async Task<List<ArticleListDto>> GetArticlesByCourseIdAsync(int courseId)
        {
            var articles = await _articleRepository.GetArticlesByCourseIdAsync(courseId);
            return articles.Select(article => new ArticleListDto
            {
                ArticleId = article.ArticleId,
                Title = article.Title,
                Difficulty = article.Difficulty,
                DifficultyLevel = GetDifficultyLevel(article.Difficulty),
                CoverImage = article.CoverImage,
                Category = article.Category,
                ReadingTime = article.ReadingTime,
                WordCount = article.WordCount,
                Description = GetDescription(article.Content)
            }).ToList();
        }

        public async Task<List<ArticleListDto>> GetArticlesByDifficultyAsync(int difficulty)
        {
            var articles = await _articleRepository.GetArticlesByDifficultyAsync(difficulty);
            return articles.Select(article => new ArticleListDto
            {
                ArticleId = article.ArticleId,
                Title = article.Title,
                Difficulty = article.Difficulty,
                DifficultyLevel = GetDifficultyLevel(article.Difficulty),
                CoverImage = article.CoverImage,
                Category = article.Category,
                ReadingTime = article.ReadingTime,
                WordCount = article.WordCount,
                Description = GetDescription(article.Content)
            }).ToList();
        }

        public async Task<List<ArticleListDto>> GetArticlesByCategoryAsync(string category)
        {
            var articles = await _articleRepository.GetArticlesByCategoryAsync(category);
            return articles.Select(article => new ArticleListDto
            {
                ArticleId = article.ArticleId,
                Title = article.Title,
                Difficulty = article.Difficulty,
                DifficultyLevel = GetDifficultyLevel(article.Difficulty),
                CoverImage = article.CoverImage,
                Category = article.Category,
                ReadingTime = article.ReadingTime,
                WordCount = article.WordCount,
                Description = GetDescription(article.Content)
            }).ToList();
        }

        private string GetDifficultyLevel(int difficulty)
        {
            return difficulty switch
            {
                <= 1 => "Beginner",
                <= 2 => "Intermediate",
                <= 3 => "Advanced"
            };
        }

        private string GetDescription(string content)
        {
            if (string.IsNullOrEmpty(content)) return string.Empty;
            return content.Length > 100 ? content.Substring(0, 100) + "..." : content;
        }
    }
}