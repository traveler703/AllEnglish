using AllEnBackend.Dtos;
using AllEnBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AllEnBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostController : ControllerBase
    {
        private readonly IPostService _postService;

        public PostController(IPostService postService)
        {
            _postService = postService;
        }

        // GET: api/post
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PostResponseDto>>> GetAllPosts()
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var posts = await _postService.GetAllPostsAsync(currentUserId);
                return Ok(new { success = true, data = posts });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // GET: api/post/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<PostResponseDto>> GetPost(int id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var post = await _postService.GetPostByIdAsync(id, currentUserId);

                if (post == null)
                    return NotFound(new { success = false, message = "帖子不存在" });

                return Ok(new { success = true, data = post });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // POST: api/post
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<PostResponseDto>> CreatePost([FromBody] CreatePostDto createPostDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { success = false, message = "用户未认证" });

                // 验证输入
                if (string.IsNullOrEmpty(createPostDto.Content) && string.IsNullOrEmpty(createPostDto.ImageUrl))
                    return BadRequest(new { success = false, message = "帖子内容不能为空" });

                var post = await _postService.CreatePostAsync(createPostDto, userId);
                return CreatedAtAction(nameof(GetPost), new { id = post.Id },
                    new { success = true, data = post });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // PUT: api/post/{id}
        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<PostResponseDto>> UpdatePost(int id, [FromBody] UpdatePostDto updatePostDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { success = false, message = "用户未认证" });

                var post = await _postService.UpdatePostAsync(id, updatePostDto, userId);

                if (post == null)
                    return NotFound(new { success = false, message = "帖子不存在或无权限修改" });

                return Ok(new { success = true, data = post });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // DELETE: api/post/{id}
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeletePost(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { success = false, message = "用户未认证" });

                var result = await _postService.DeletePostAsync(id, userId);

                if (!result)
                    return NotFound(new { success = false, message = "帖子不存在或无权限删除" });

                return Ok(new { success = true, message = "帖子删除成功" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // GET: api/post/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<PostResponseDto>>> GetPostsByUserId(string userId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var posts = await _postService.GetPostsByUserIdAsync(userId, currentUserId);
                return Ok(new { success = true, data = posts });
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

