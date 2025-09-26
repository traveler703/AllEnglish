using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AllEnBackend.Data;
using AllEnBackend.Models;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using AllEnBackend.Dtos;
using AllEnBackend.Services;

namespace AllEnBackend.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/listening-practice")]
    public class ListeningPracticeController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly ILeaderboardService _leaderboardService;
        public ListeningPracticeController(
        AppDbContext db,
        ILeaderboardService leaderboardService)
        {
            _db = db;
            _leaderboardService = leaderboardService;
        }


        // 1) 获取当前用户完成次数
        [HttpGet("completed/count")]
        public async Task<IActionResult> GetCompletedCount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var count = await _db.ListeningPracticeRecords
                                 .CountAsync(r => r.UserId == userId);
            return Ok(new { count });
        }

        // 2) 提交练习答案，计算得分并存数据库
        [HttpPost("submit")]
        public async Task<IActionResult> Submit([FromBody] SubmitListeningPracticeDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // 只计算分数，不存每题详情
            var questions = await _db.ListeningQuestions
                                     .Where(q => dto.Answers.Select(a => a.QuestionId).Contains(q.Id))
                                     .Select(q => new { q.Id, q.CorrectOption })
                                     .ToListAsync();

            int score = questions.Count(q =>
                dto.Answers.Any(a =>
                    a.QuestionId == q.Id
                    && string.Equals(a.Response, q.CorrectOption, StringComparison.OrdinalIgnoreCase)
                )
            );

            var record = new ListeningPracticeRecord
            {
                UserId = userId,
                ListeningPaperId = dto.PaperId,
                CompletedAt = DateTime.UtcNow,
                Score = score
            };

            _db.ListeningPracticeRecords.Add(record);
            await _db.SaveChangesAsync();

            await _leaderboardService.UpdateUserListening(userId);

            return Ok(new { total = questions.Count, score });
        }

    }
}
