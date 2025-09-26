using AllEnBackend.Dtos;
using AllEnBackend.Services;
using Microsoft.AspNetCore.Mvc;

namespace AllEnBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserDailyWordsController : ControllerBase
    {
        private readonly IUserDailyWordsService _userDailyWordsService;

        public UserDailyWordsController(IUserDailyWordsService userDailyWordsService)
        {
            _userDailyWordsService = userDailyWordsService;
        }

        // 根据用户ID和日期获取用户每日学习的单词数量
        [HttpGet]
        public async Task<IActionResult> GetUserDailyWords([FromQuery] string userId, [FromQuery] DateTime studyDate)
        {
            try
            {
                // 验证输入参数
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "用户ID不能为空"
                    });
                }

                if (studyDate == default)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "学习日期不能为空"
                    });
                }

                // 调用服务获取数据
                var result = await _userDailyWordsService.GetUserDailyWordsAsync(userId, studyDate);

                return Ok(new
                {
                    success = true,
                    message = "获取用户每日单词学习数量成功",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "服务器内部错误",
                    error = ex.Message
                });
            }
        }
    }
}
