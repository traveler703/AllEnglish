using Microsoft.AspNetCore.Mvc;
using AllEnBackend.Models;
using AllEnBackend.Services;
using Microsoft.AspNetCore.Cors.Infrastructure;
using AllEnBackend.Dtos;
using static AllEnBackend.Dtos.WordDetailDto;

namespace AllEnBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WordController : ControllerBase
    {
        private readonly IWordService _wordService; 
        public WordController(IWordService wordService)
        {
            _wordService = wordService;
        }

        // 按 ID 查询： GET /api/word/by-id/1
        [HttpGet("by-id/{id:int}")]
        public async Task<ActionResult<WordDetailDto>> GetWordById(int id)
        {
            var result = await _wordService.GetWordDetailByIdAsync(id);
            if (result == null)
                return NotFound($"未找到 ID 为 {id} 的单词");
            return Ok(result);
        }

        // 按单词名查询：GET /api/word/by-name/apple
        [HttpGet("by-name/{wordName}")]
        public async Task<ActionResult<WordDetailDto>> GetWordByName(string wordName)
        {
            var result = await _wordService.GetWordDetailByNameAsync(wordName);
            if (result == null)
                return NotFound($"未找到单词 {wordName}");
            return Ok(result);
        }


        // 修改单词
        [HttpPut("{wordName}")]
        public async Task<IActionResult> UpdateWord(string wordName, [FromBody] WordDetailDto updateData)
        {
            var success = await _wordService.UpdateWordAsync(wordName, updateData);
            if (!success)
                return NotFound("更新失败，单词不存在");
            return Ok("更新成功");
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<UserWordDto>> GetUserWords(
            string userId,
            [FromQuery] int hasLearned = -1,
            [FromQuery] int hasBookmarked = -1,
            [FromQuery] int minCorrectCount = -1,
            [FromQuery] int maxCorrectCount = -1,
            [FromQuery] int minLearnCount = -1,
            [FromQuery] int maxLearnCount = -1,
            [FromQuery] string syllabusScope = "-1")  // 新增考纲范围参数
        {
            var queryParams = new UserWordQueryDto
            {
                HasLearned = hasLearned,
                HasBookmarked = hasBookmarked,
                MinCorrectCount = minCorrectCount,
                MaxCorrectCount = maxCorrectCount,
                MinLearnCount = minLearnCount,
                MaxLearnCount = maxLearnCount,
                SyllabusScope = syllabusScope  // 设置考纲范围
            };

            var result = await _wordService.GetUserWordsByUserIdAsync(userId, queryParams);
            return Ok(result);
        }

        // 添加或更新用户单词数据
        [HttpPost("user/{userId}/{wordId}")]
        public async Task<IActionResult> AddOrUpdateUserWord(string userId, int wordId,
            [FromQuery] int hasLearned, [FromQuery] int correctCount,
            [FromQuery] int learnCount, [FromQuery] int hasBookmarked)
        {
            try
            {
                var success = await _wordService.AddOrUpdateUserWordAsync(
                userId, wordId, hasLearned, correctCount, learnCount, hasBookmarked);
                if (!success) return BadRequest("操作失败");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok("操作成功");
        }

        // 更新学习统计
        [HttpPut("user/learn/{userId}/{wordId}")]
        public async Task<IActionResult> UpdateLearningStats(string userId, int wordId, [FromQuery] bool isCorrect)
        {
            var success = await _wordService.UpdateLearningStatsAsync(userId, wordId, isCorrect);
            if (!success) return BadRequest("更新学习统计失败");
            return Ok("更新学习统计成功");
        }

        // 切换收藏状态
        [HttpPut("user/bookmark/{userId}/{wordId}")]
        public async Task<IActionResult> ToggleBookmark(string userId, int wordId)
        {
            var success = await _wordService.ToggleBookmarkAsync(userId, wordId);
            if (!success) return BadRequest("切换收藏状态失败");
            return Ok("切换收藏状态成功");
        }

        // 删除用户单词数据
        [HttpDelete("user/{userId}/{wordId}")]
        public async Task<IActionResult> RemoveUserWord(string userId, int wordId)
        {
            var success = await _wordService.RemoveUserWordAsync(userId, wordId);
            if (!success) return NotFound("未找到该用户单词数据");
            return Ok("删除成功");
        }


        [HttpGet("user/{userId}/detail/{wordId}")]
        public async Task<ActionResult<UserWordDetailDto>> GetUserWordDetail(string userId, int wordId)
        {
            var result = await _wordService.GetUserWordDetailAsync(userId, wordId);
            if (result == null)
                return NotFound($"未找到用户 {userId} 的单词 {wordId} 的学习记录");
            return Ok(result);
        }
    }
}
