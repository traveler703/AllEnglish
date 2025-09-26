using AllEnBackend.Dtos;
using AllEnBackend.Models;

namespace AllEnBackend.Services.Interfaces
{
    public interface IFriendService
    {
        Task<FriendResponseDto> AddFriendAsync(string userId, AddFriendDto addFriendDto);
        Task<FriendResponseDto> UpdateFriendStatusAsync(string userId, FriendStatusUpdateDto updateDto);
        Task<List<FriendResponseDto>> GetUserFriendsAsync(string userId);
        Task<List<FriendResponseDto>> GetFriendRequestsAsync(string userId);
        Task<List<FriendResponseDto>> GetSentFriendRequestsAsync(string userId);
        Task<bool> RemoveFriendAsync(string userId, int friendsId);
        Task<List<FriendResponseDto>> SearchFriendsAsync(string userId, string searchTerm);
        Task<bool> AreFriendsAsync(string userId, string friendsUserId);
        Task<int> GetFriendCountAsync(string userId);
        Task<List<FriendSearchResponseDto>> SearchUsersByNicknameAsync(string userId,string userName);
    }
}
