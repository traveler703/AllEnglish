using AllEnBackend.Data;
using AllEnBackend.Dtos;
using AllEnBackend.Models;
using AllEnBackend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AllEnBackend.Services.Implementations
{
    public class FriendService : IFriendService
    {
        private readonly IFriendRepository _friendRepository;
        private readonly AppDbContext _context;

        public FriendService(IFriendRepository friendRepository, AppDbContext db)
        {
            _friendRepository = friendRepository;
            _context = db;
        }

        public async Task<FriendResponseDto> AddFriendAsync(string userId, AddFriendDto addFriendDto)
        {
            if (userId == addFriendDto.FriendsUserId)
            {
                throw new ArgumentException("不能添加自己为好友");
            }

            var friend = await _friendRepository.AddFriendRequestAsync(userId, addFriendDto.FriendsUserId);
            return MapToResponseDto(friend);
        }

        public async Task<FriendResponseDto> UpdateFriendStatusAsync(string userId, FriendStatusUpdateDto updateDto)
        {
            // 验证状态值
            if (!updateDto.IsValidStatus())
            {
                throw new ArgumentException("无效的状态值");
            }

            var friendship = await _friendRepository.GetFriendshipByIdAsync(userId, updateDto.friendsId);
            if (friendship == null)
            {
                throw new ArgumentException("好友关系不存在1");
            }
            var updatedFriend = await _friendRepository.UpdateFriendStatusAsync(userId,updateDto.friendsId, updateDto.Status);
            return MapToResponseDto(updatedFriend);
        }

        public async Task<List<FriendResponseDto>> GetUserFriendsAsync(string userId)
        {

            var left = _context.Friends.AsNoTracking()
                .Where(f => f.Status == 1 && f.UserId == userId)
                .Select(f => new FriendResponseDto
                {
                    FriendsId = f.FriendId,
                    UserId = userId,
                    FriendsUserId = f.FriendUserId,
                    FriendUserName = f.FriendUser.UserName,
                    FriendAvatarUrl = f.FriendUser.AvatarUrl,
                    Status = f.Status,
                    CreatedAt = f.CreatedAt,
                    UpdateAt = f.UpdateAt
                });

            var right = _context.Friends.AsNoTracking()
                .Where(f => f.Status == 1 && f.FriendUserId == userId)
                .Select(f => new FriendResponseDto
                {
                    FriendsId = f.FriendId,
                    UserId = userId,
                    FriendsUserId = f.UserId,
                    FriendUserName = f.User.UserName,
                    FriendAvatarUrl = f.User.AvatarUrl,
                    Status = f.Status,
                    CreatedAt = f.CreatedAt,
                    UpdateAt = f.UpdateAt
                });

            return await left.Concat(right).ToListAsync();

        }

        public async Task<List<FriendResponseDto>> GetFriendRequestsAsync(string userId)
        {
            var requests = await _friendRepository.GetFriendRequestsAsync(userId);
            
            List<FriendResponseDto> friendRequests = new List<FriendResponseDto>();

            foreach (var request in requests)
            {
                string senderName = request.UserId;
                var sender = await _context.Users
                    .Where(u => u.Id == senderName)
                    .FirstOrDefaultAsync();

                if (sender != null)
                {
                    var friendResponse = new FriendResponseDto
                    {
                        FriendsId = request.FriendId,
                        UserId = request.UserId,
                        FriendsUserId = request.FriendUserId,
                        FriendUserName = sender.UserName,
                        FriendAvatarUrl = sender.AvatarUrl,
                        Status = request.Status,
                        CreatedAt = request.CreatedAt,
                        UpdateAt = request.UpdateAt
                    };
                    friendRequests.Add(friendResponse);
                }
            }

            return friendRequests;

        }

        public async Task<List<FriendResponseDto>> GetSentFriendRequestsAsync(string userId)
        {
            var requests = await _friendRepository.GetSentFriendRequestsAsync(userId);
            return requests.Select(MapToResponseDto).ToList();
        }

        public async Task<bool> RemoveFriendAsync(string userId, int friendsId)
        {
            var friendship = await _friendRepository.GetFriendRecordByIdAsync(friendsId);
            if (friendship == null) return false;

            // 确保只有相关用户才能删除好友关系
            if (friendship.UserId != userId && friendship.FriendUserId != userId)
            {
                throw new UnauthorizedAccessException("无权限删除此好友关系");
            }

            return await _friendRepository.RemoveFriendAsync(friendsId);
        }

        public async Task<List<FriendResponseDto>> SearchFriendsAsync(string userId, string searchTerm)
        {
            var friends = await _friendRepository.SearchFriendsAsync(userId, searchTerm);
            return friends.Select(MapToResponseDto).ToList();
        }

        public async Task<bool> AreFriendsAsync(string userId, string friendsUserId)
        {
            return await _friendRepository.AreFriendsAsync(userId, friendsUserId);
        }

        public async Task<int> GetFriendCountAsync(string userId)
        {
            return await _friendRepository.GetFriendCountAsync(userId);
        }


        private FriendResponseDto MapToResponseDto(Friend friend)
        {
            return new FriendResponseDto
            {
                FriendsId = friend.FriendId,
                UserId = friend.UserId,
                FriendsUserId = friend.FriendUserId,
                FriendUserName = friend.FriendUser?.UserName ?? "",
                FriendAvatarUrl = friend.FriendUser?.AvatarUrl ?? "",
                Status = friend.Status,
                CreatedAt = friend.CreatedAt,
                UpdateAt = friend.UpdateAt
            };
        }

        public async Task<List<FriendSearchResponseDto>> SearchUsersByNicknameAsync(string userId,string userName)
        {
            var users = await _friendRepository.SearchFriendsByNameAsync(userName);
            var filteredUsers = users.Where(u => u.Id != userId);

            return filteredUsers.Select(u => new FriendSearchResponseDto
            {
                FriendsId =u.Id,
                FriendName = u.UserName,
                FriendAvatarUrl = u.AvatarUrl,
            }).ToList();
        }
    }
}

