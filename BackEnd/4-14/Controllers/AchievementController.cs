using AllEnBackend.Dtos;
using AllEnBackend.Services;
using Microsoft.AspNetCore.Mvc;

namespace AllEnBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AchievementController : ControllerBase
    {
        private readonly IAchievementService _achievementService;

        public AchievementController(IAchievementService achievementService)
        {
            _achievementService = achievementService;
        }

        // 1. 根据成就的ID查询成就的信息
        // GET: api/Achievement/{achievementId}
        [HttpGet("{achievementId}")]
        public async Task<ActionResult<AchievementDto>> GetAchievementById(int achievementId)
        {
            var achievement = await _achievementService.GetAchievementByIdAsync(achievementId);
            
            if (achievement == null)
            {
                return NotFound($"未找到ID为 {achievementId} 的成就");
            }

            return Ok(achievement);
        }

        // 2. 根据用户ID查询该用户所有已取得的成就
        // GET: api/Achievement/UserGained/{userId}
        [HttpGet("UserGained/{userId}")]
        public async Task<ActionResult<UserGainedAchievementsDto>> GetUserGainedAchievements(string userId)
        {
            var gainedAchievements = await _achievementService.GetUserGainedAchievementsAsync(userId);
            return Ok(gainedAchievements);
        }

        // 3. 根据用户ID和成就ID查询该用户是否已取得该成就
        // GET: api/Achievement/UserStatus/{userId}/{achievementId}
        [HttpGet("UserStatus/{userId}/{achievementId}")]
        public async Task<ActionResult<UserAchievementStatusDto>> GetUserAchievementStatus(string userId, int achievementId)
        {
            var status = await _achievementService.GetUserAchievementStatusAsync(userId, achievementId);
            return Ok(status);
        }

        // 4. 根据用户ID查询所有成就的取得情况
        // GET: api/Achievement/AllUserAchievements/{userId}
        [HttpGet("AllUserAchievements/{userId}")]
        public async Task<ActionResult<List<UserAchievementDto>>> GetAllUserAchievements(string userId)
        {
            var allAchievements = await _achievementService.GetAllUserAchievementsAsync(userId);
            return Ok(allAchievements);
        }
    }
} 