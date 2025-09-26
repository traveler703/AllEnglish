using Microsoft.AspNetCore.Mvc;
using AllEnBackend.Services;
using AllEnBackend.Dtos;

namespace AllEnBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeaderboardController : ControllerBase
    {
        private readonly ILeaderboardService _leaderboardService;

        public LeaderboardController(ILeaderboardService leaderboardService)
        {
            _leaderboardService = leaderboardService;
        }

        [HttpGet("{type}")]
        public async Task<ActionResult<LeaderboardResponseDto>> GetLeaderboard(
            string type,
            [FromQuery] int page = 1,
            [FromQuery] int size = 20)
        {
            try
            {
                if (size > 100) size = 100; 

                var response = await _leaderboardService.GetLeaderboardAsync(type, page, size);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "服务器内部错误");
            }
        }

        [HttpGet("{type}/user/{userId}")]
        public async Task<ActionResult<UserRankInfoDto>> GetUserRank(string type, string userId)
        {
            try
            {
                var rankInfo = await _leaderboardService.GetUserRankInfoAsync(type, userId);
                return Ok(rankInfo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "服务器内部错误");
            }
        }

        [HttpPost("update-score")]
        public async Task<IActionResult> UpdateScore([FromBody] ScoreUpdateRequestDto request)
        {
            try
            {
                await _leaderboardService.UpdateScoreAsync(request);
                return Ok(new { message = "分数更新成功" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "服务器内部错误");
            }
        }

        [HttpPost("refresh-rankings")]
        public async Task<IActionResult> RefreshRankings()
        {
            try
            {
                await _leaderboardService.UpdateAllRankingsAsync();
                return Ok(new { message = "排行榜刷新成功" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "服务器内部错误");
            }
        }

        [HttpGet("day/score")]
        public async Task<IActionResult> GetDayScoreRank()
        => Ok(await _leaderboardService.GetDayRankAsync("score"));

        [HttpGet("week/score")]
        public async Task<IActionResult> GetWeekScoreRank()
            => Ok(await _leaderboardService.GetWeekRankAsync("score"));

        [HttpGet("month/score")]
        public async Task<IActionResult> GetMonthScoreRank()
            => Ok(await _leaderboardService.GetMonthRankAsync("score"));


        // Activity 排名（日榜/周榜/月榜）
        [HttpGet("day/activity")]
        public async Task<IActionResult> GetDayActivityRank()
            => Ok(await _leaderboardService.GetDayRankAsync("activity"));

        [HttpGet("week/activity")]
        public async Task<IActionResult> GetWeekActivityRank()
            => Ok(await _leaderboardService.GetWeekRankAsync("activity"));

        [HttpGet("month/activity")]
        public async Task<IActionResult> GetMonthActivityRank()
            => Ok(await _leaderboardService.GetMonthRankAsync("activity"));
    }
}
