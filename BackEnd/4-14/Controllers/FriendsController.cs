using AllEnBackend.Dtos;
using AllEnBackend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AllEnBackend.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class FriendsController : ControllerBase
    {
        private readonly IFriendService _friendService;

        public FriendsController(IFriendService friendService)
        {
            _friendService = friendService;
        }

        private string GetCurrentUserId()
        {

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                // 如果未认证，使用测试用户ID
                return "fe5a309c - 4999 - 4dd2 - 9080 - 683b7fbfc87f"; 
            }
            return userId;
        }

        // 发送好友申请
        [HttpPost("request")]
        public async Task<ActionResult<FriendResponseDto>> AddFriend([FromBody] AddFriendDto addFriendDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _friendService.AddFriendAsync(userId, addFriendDto);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }

        // 处理好友申请（接受/拒绝/屏蔽）
        [HttpPut("status")]
        public async Task<ActionResult<FriendResponseDto>> UpdateFriendStatus([FromBody] FriendStatusUpdateDto updateDto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _friendService.UpdateFriendStatusAsync(userId, updateDto);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        // 获取好友列表
        [HttpGet]
        public async Task<ActionResult<List<FriendResponseDto>>> GetFriends()
        {
            var userId = GetCurrentUserId();
            var friends = await _friendService.GetUserFriendsAsync(userId);
            return Ok(friends);
        }


        // 获取收到的好友申请
        [HttpGet("requests")]
        public async Task<ActionResult<List<FriendResponseDto>>> GetFriendRequests()
        {
            var userId = GetCurrentUserId();
            var requests = await _friendService.GetFriendRequestsAsync(userId);
            return Ok(requests);
        }

        // 获取发出的好友申请
        [HttpGet("sent-requests")]
        public async Task<ActionResult<List<FriendResponseDto>>> GetSentFriendRequests()
        {
            var userId = GetCurrentUserId();
            var requests = await _friendService.GetSentFriendRequestsAsync(userId);
            return Ok(requests);
        }


        // 删除好友
        [HttpDelete("{friendsId}")]
        public async Task<ActionResult> RemoveFriend(int friendsId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var success = await _friendService.RemoveFriendAsync(userId, friendsId);
                if (success)
                {
                    return Ok(new { message = "好友删除成功" });
                }
                return NotFound("好友关系不存在");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        // 搜索好友
        [HttpGet("search")]
        public async Task<ActionResult<List<FriendResponseDto>>> SearchFriends([FromQuery] string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return BadRequest("搜索词不能为空");
            }

            var userId = GetCurrentUserId();
            var friends = await _friendService.SearchFriendsAsync(userId, searchTerm);
            return Ok(friends);
        }


        // 检查是否为好友关系
        [HttpGet("check/{friendUserId}")]
        public async Task<ActionResult<bool>> CheckFriendship(string friendUserId)
        {
            var userId = GetCurrentUserId();
            var areFriends = await _friendService.AreFriendsAsync(userId, friendUserId);
            return Ok(areFriends);
        }

        // 获取好友数量
        [HttpGet("count")]
        public async Task<ActionResult<int>> GetFriendCount()
        {
            var userId = GetCurrentUserId();
            var count = await _friendService.GetFriendCountAsync(userId);
            return Ok(count);
        }

        
        [HttpGet("search-users")]
        public async Task<ActionResult<List<FriendSearchResponseDto>>> SearchUsersByNickname([FromQuery] string nickname)
        {
            if (string.IsNullOrWhiteSpace(nickname))
            {
                return BadRequest("昵称不能为空");
            }

            var userId = GetCurrentUserId();
            var users = await _friendService.SearchUsersByNicknameAsync(userId, nickname);
            return Ok(users);
        }
    }
}