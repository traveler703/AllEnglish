using AllEnBackend.Dtos;
using AllEnBackend.Services;
using Microsoft.AspNetCore.Mvc;

namespace AllEnBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserLearningRecordController : ControllerBase
    {
        private readonly IUserLearningRecordService _userLearningRecordService;

        public UserLearningRecordController(IUserLearningRecordService userLearningRecordService)
        {
            _userLearningRecordService = userLearningRecordService;
        }

        // 根据用户ID获取用户学习记录
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserLearningRecord(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return BadRequest("用户ID不能为空");
                }

                var record = await _userLearningRecordService.GetUserLearningRecordAsync(userId);
                if (record == null)
                {
                    return NotFound("未找到该用户的学习记录");
                }

                return Ok(new
                {
                    success = true,
                    message = "获取用户学习记录成功",
                    data = record
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

        // 更新用户学习记录
        [HttpPut]
        public async Task<IActionResult> UpdateUserLearningRecord([FromBody] UpdateUserLearningRecordDto updateDto)
        {
            try
            {
                if (updateDto == null)
                {
                    return BadRequest("请求数据不能为空");
                }

                if (string.IsNullOrWhiteSpace(updateDto.UserId))
                {
                    return BadRequest("用户ID不能为空");
                }

                // 验证数据有效性
                if (updateDto.ArticleCount < 0 || updateDto.WordCount < 0 || 
                    updateDto.OralTime < 0 || updateDto.ListeningTime < 0 ||
                    updateDto.ArticlePerDay < 0 || updateDto.WordPerDay < 0 ||
                    updateDto.OralPerDay < 0 || updateDto.ListeningPerDay < 0)
                {
                    return BadRequest("学习记录数据不能为负数");
                }

                var result = await _userLearningRecordService.UpdateUserLearningRecordAsync(updateDto);

                if (result)
                {
                    return Ok(new
                    {
                        success = true,
                        message = "更新用户学习记录成功"
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "更新用户学习记录失败"
                    });
                }
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
