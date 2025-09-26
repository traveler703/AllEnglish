using AllEnBackend.Dtos;
using AllEnBackend.Models;

public interface IFriendRepository
{
    Task<Friend> AddFriendRequestAsync(string userId, string friendUserId);
    Task<Friend> UpdateFriendStatusAsync(string userId, string friendsId, int status);
    Task<Friend> GetFriendshipAsync(string userId, string friendsUserId);
    Task<Friend> GetFriendshipByIdAsync(string userId,string friendsId);
    Task<List<Friend>> GetUserFriendsAsync(string userId, int? status = null); 
    Task<List<Friend>> GetFriendRequestsAsync(string userId);
    Task<List<Friend>> GetSentFriendRequestsAsync(string userId);
    Task<bool> AreFriendsAsync(string userId, string friendsUserId);
    Task<bool> RemoveFriendAsync(int friendsId);
    Task<int> GetFriendCountAsync(string userId);
    Task<List<Friend>> SearchFriendsAsync(string userId, string searchTerm);

    Task<Friend?> GetFriendRecordByIdAsync(int friendsId);

    Task<List<User>> SearchFriendsByNameAsync(string userName);
}

