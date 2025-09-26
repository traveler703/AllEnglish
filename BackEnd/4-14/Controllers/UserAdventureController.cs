using AllEnBackend.Dtos;
using AllEnBackend.Services;
using Microsoft.AspNetCore.Mvc;

namespace AllEnBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserAdventureController : ControllerBase
    {
        private readonly IUserAdventureService _userAdventureService;

        public UserAdventureController(IUserAdventureService userAdventureService)
        {
            _userAdventureService = userAdventureService;
        }

        // 获取特定用户冒险
        [HttpGet("{userId}/{adventureId}")]
        public async Task<ActionResult<UserAdventureDto>> GetUserAdventure(string userId, long adventureId)
        {
            var userAdventure = await _userAdventureService.GetUserAdventureAsync(userId, adventureId);
            if (userAdventure == null)
            {
                return NotFound();
            }
            return Ok(userAdventure);
        }

        // 获取用户所有冒险
        [HttpGet("{userId}")]
        public async Task<ActionResult<List<UserAdventureDto>>> GetUserAdventures(string userId)
        {
            var userAdventures = await _userAdventureService.GetUserAdventuresAsync(userId);
            return Ok(userAdventures);
        }

        // 获取用户冒险进度
        [HttpGet("{userId}/progress")]
        public async Task<ActionResult<AdventureProgressDto>> GetUserAdventureProgress(string userId)
        {
            var progress = await _userAdventureService.GetUserAdventureProgressAsync(userId);
            return Ok(progress);
        }

        // 更新用户冒险状态
        [HttpPut("{userId}/{adventureId}")]
        public async Task<IActionResult> UpdateUserAdventure(string userId, long adventureId, [FromBody] string status)
        {
            try
            {
                var result = await _userAdventureService.UpdateUserAdventureAsync(userId, adventureId, status);
                if (result)
                {
                    return NoContent();
                }
                return BadRequest("更新用户冒险状态失败");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}