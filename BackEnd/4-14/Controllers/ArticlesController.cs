using Microsoft.AspNetCore.Mvc;
using AllEnBackend.Services;
using AllEnBackend.Dtos;

namespace AllEnBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ArticlesController : ControllerBase
    {
        private readonly IArticleService _articleService;

        public ArticlesController(IArticleService articleService)
        {
            _articleService = articleService;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResult<ArticleListDto>>> GetArticles(
            [FromQuery] ArticleQueryParams queryParams)
        {
            try
            {
                var result = await _articleService.GetArticlesAsync(queryParams);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "获取文章列表失败", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ArticleDto>> GetArticle(int id)
        {
            try
            {
                var article = await _articleService.GetArticleByIdAsync(id);
                if (article == null)
                {
                    return NotFound(new { message = "文章不存在" });
                }
                return Ok(article);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "获取文章详情失败", error = ex.Message });
            }
        }

        [HttpGet("course/{courseId}")]
        public async Task<ActionResult<List<ArticleListDto>>> GetArticlesByCourse(int courseId)
        {
            try
            {
                var articles = await _articleService.GetArticlesByCourseIdAsync(courseId);
                return Ok(articles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "获取课程文章失败", error = ex.Message });
            }
        }

        [HttpGet("difficulty/{difficulty}")]
        public async Task<ActionResult<List<ArticleListDto>>> GetArticlesByDifficulty(int difficulty)
        {
            try
            {
                var articles = await _articleService.GetArticlesByDifficultyAsync(difficulty);
                return Ok(articles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "获取指定难度文章失败", error = ex.Message });
            }
        }

        [HttpGet("category/{category}")]
        public async Task<ActionResult<List<ArticleListDto>>> GetArticlesByCategory(string category)
        {
            try
            {
                var articles = await _articleService.GetArticlesByCategoryAsync(category);
                return Ok(articles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "获取指定类别文章失败", error = ex.Message });
            }
        }

        [HttpGet("{id}/detail")]
        public async Task<ActionResult<ArticleDetailDto>> GetArticleDetail(int id)
        {
            try
            {
                var articleDetail = await _articleService.GetArticleWithQuestionsAsync(id);
                if (articleDetail == null)
                {
                    return NotFound(new { message = "文章不存在" });
                }
                return Ok(articleDetail);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "获取文章详情及相关题目失败", error = ex.Message });
            }
        }

        [HttpPost("submit")]
        public async Task<ActionResult<AnswerResultDto>> SubmitAnswer(SubmitAnswerDto submitDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _articleService.SubmitAnswerAsync(submitDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                // 日志记录异常
                return StatusCode(500, $"提交答案时发生错误: {ex.Message}");
            }
        }

        [HttpGet("user/{userId}/article/{articleId}/highest-score")]
        public async Task<IActionResult> GetHighestScore(string userId, int articleId)
        {
            var highestScore = await _articleService.GetHighestScoreAsync(userId, articleId);
            return Ok(new { highestScore });
        }

        [HttpGet("user/{userId}/completed-articles/count")]
        public async Task<IActionResult> GetCompletedArticleCount(string userId)
        {
            var count = await _articleService.GetCompletedArticleCountAsync(userId);
            return Ok(new { completedCount = count });
        }

        [HttpGet("user/{userId}/completed-articles")]
        public async Task<IActionResult> GetCompletedArticles(string userId)
        {
            var articleIds = await _articleService.GetCompletedArticleIdsAsync(userId);
            return Ok(new { completedArticleIds = articleIds });
        }

    }
}
