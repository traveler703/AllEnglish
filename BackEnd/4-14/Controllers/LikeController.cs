using AllEnBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AllEnBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LikeController : ControllerBase
    {
        private readonly ILikeService _likeService;

        public LikeController(ILikeService likeService)
        {
            _likeService = likeService;
        }

        // POST: api/like/{postId}
        [HttpPost("{postId}")]
        [Authorize]
        public async Task<IActionResult> ToggleLike(int postId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { success = false, message = "用户未认证" });

                var result = await _likeService.ToggleLikeAsync(postId, userId);

                if (!result)
                    return NotFound(new { success = false, message = "帖子不存在" });

                // 获取最新的点赞状态和数量
                var isLiked = await _likeService.IsPostLikedByUserAsync(postId, userId);
                var likesCount = await _likeService.GetLikesCountAsync(postId);

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        isLiked = isLiked,
                        likesCount = likesCount,
                        message = isLiked ? "点赞成功" : "取消点赞成功"
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // GET: api/like/{postId}/count
        [HttpGet("{postId}/count")]
        public async Task<ActionResult<int>> GetLikesCount(int postId)
        {
            try
            {
                var count = await _likeService.GetLikesCountAsync(postId);
                return Ok(new { success = true, data = count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // GET: api/like/{postId}/status
        [HttpGet("{postId}/status")]
        [Authorize]
        public async Task<ActionResult<bool>> GetLikeStatus(int postId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { success = false, message = "用户未认证" });

                var isLiked = await _likeService.IsPostLikedByUserAsync(postId, userId);
                return Ok(new { success = true, data = isLiked });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        private string GetCurrentUserId()
        {
            return User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                   User?.FindFirst("sub")?.Value ??
                   User?.FindFirst("userId")?.Value;
        }
    }
}
