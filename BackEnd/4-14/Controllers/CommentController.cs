using AllEnBackend.Dtos;
using AllEnBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AllEnBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        // GET: api/comment/post/{postId}
        [HttpGet("post/{postId}")]
        public async Task<ActionResult<IEnumerable<CommentResponseDto>>> GetCommentsByPostId(int postId)
        {
            try
            {
                var comments = await _commentService.GetCommentsByPostIdAsync(postId);
                return Ok(new { success = true, data = comments });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // POST: api/comment
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<CommentResponseDto>> CreateComment([FromBody] CreateCommentDto createCommentDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { success = false, message = "用户未认证" });

                // 验证输入
                if (string.IsNullOrWhiteSpace(createCommentDto.Content))
                    return BadRequest(new { success = false, message = "评论内容不能为空" });

                var comment = await _commentService.CreateCommentAsync(createCommentDto, userId);

                if (comment == null)
                    return NotFound(new { success = false, message = "帖子不存在" });

                return CreatedAtAction(nameof(GetCommentsByPostId),
                    new { postId = comment.PostId },
                    new { success = true, data = comment });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // DELETE: api/comment/{id}
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteComment(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { success = false, message = "用户未认证" });

                var result = await _commentService.DeleteCommentAsync(id, userId);

                if (!result)
                    return NotFound(new { success = false, message = "评论不存在或无权限删除" });

                return Ok(new { success = true, message = "评论删除成功" });
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

