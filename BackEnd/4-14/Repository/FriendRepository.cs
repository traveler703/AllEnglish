using AllEnBackend.Data;
using AllEnBackend.Dtos;
using AllEnBackend.Models;
using Microsoft.EntityFrameworkCore;

public class FriendRepository : IFriendRepository
{
    private readonly AppDbContext _context;

    public FriendRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Friend> AddFriendRequestAsync(string userId, string friendsUserId)
    {
        // 检查是否已经存在好友关系
        var existingFriendship = await _context.Friends
            .FirstOrDefaultAsync(f =>
                (f.UserId == userId && f.FriendUserId == friendsUserId) ||
                (f.UserId == friendsUserId && f.FriendUserId == userId));

        if (existingFriendship != null)
        {
            if(existingFriendship.Status != 1)
            {
                existingFriendship.Status = 0;
                existingFriendship.UpdateAt = DateTime.Now;

                _context.Friends.Update(existingFriendship);
                await _context.SaveChangesAsync();
                return existingFriendship;
            }
            else
            {
                throw new InvalidOperationException("好友关系已存在");
            }
        }
        else
        {
            // 获取当前最大ID并加1
            var maxId = await _context.Friends.MaxAsync(f => (int?)f.FriendId) ?? 0;
            var newId = maxId + 1;

            var friend = new Friend
            {
                FriendId = newId,
                UserId = userId,
                FriendUserId = friendsUserId,
                Status = FriendStatus.Pending, // 使用常量
                CreatedAt = DateTime.Now,
                UpdateAt = DateTime.Now
            };

            _context.Friends.Add(friend);
            await _context.SaveChangesAsync();
            return friend;
        }
    }

    public async Task<Friend> UpdateFriendStatusAsync(string userId, string friendsId, int status)
    {
        // 验证状态值
        if (!FriendStatus.IsValidStatus(status))
        {
            throw new ArgumentException("无效的状态值");
        }

        // 查询好友关系
        var friend = await _context.Friends
            .FirstOrDefaultAsync(f => f.UserId == friendsId && f.FriendUserId == userId);

        // 判断是否已经存在
        if (friend == null)
        {
            throw new ArgumentException("好友关系不存在2");
        }
        else
        {
            friend.Status = status;
            friend.UpdateAt = DateTime.Now;
        }

        await _context.SaveChangesAsync();
        return friend;
    }

    public async Task<Friend> GetFriendshipAsync(string userId, string friendsUserId)
    {
        return await _context.Friends
            .Include(f => f.User)
            .Include(f => f.FriendUser)
            .FirstOrDefaultAsync(f => f.UserId == userId && f.FriendUserId == friendsUserId);
    }

    public async Task<Friend> GetFriendshipByIdAsync(string userId, string friendsId)
    {
        return await _context.Friends
            .FirstOrDefaultAsync(f => f.FriendUserId==userId && f.UserId == friendsId);
    }

    public async Task<List<Friend>> GetUserFriendsAsync(string userId, int? status = null)
    {
        var query = _context.Friends
            .Include(f => f.FriendUser)
            .Where(f => f.UserId == userId || f.FriendUserId == userId);

        if (status.HasValue)
        {
            query = query.Where(f => f.Status == status.Value);
        }

        return await query.ToListAsync();
    }

    public async Task<List<Friend>> GetFriendRequestsAsync(string userId)
    {
        return await _context.Friends
            .Where(f => f.FriendUserId == userId && f.Status == FriendStatus.Pending)
            .ToListAsync();
    }

    public async Task<List<Friend>> GetSentFriendRequestsAsync(string userId)
    {
        try
        {
            var friends = await _context.Friends
                .Include(f => f.User)
                .Include(f => f.FriendUser)
                .Where(f => f.UserId == userId && f.Status == 0)
                .ToListAsync();

            // 检查每个记录的每个字段
            foreach (var friend in friends)
            {
                Console.WriteLine($"Friend ID: {friend.FriendId}");
                Console.WriteLine($"User ID: {friend.UserId ?? "NULL"}");
                Console.WriteLine($"Friend User ID: {friend.FriendUserId ?? "NULL"}");

                if (friend.User == null)
                    Console.WriteLine("User object is NULL");
                else
                {
                    Console.WriteLine($"User ID: {friend.User.Id ?? "NULL"}");
                    Console.WriteLine($"User Name: {friend.User.UserName ?? "NULL"}");
                    Console.WriteLine($"User Email: {friend.User.Email ?? "NULL"}");
                    // 其他User属性...
                }

                if (friend.FriendUser == null)
                    Console.WriteLine("FriendUser object is NULL");
                else
                {
                    Console.WriteLine($"FriendUser ID: {friend.FriendUser.Id ?? "NULL"}");
                    Console.WriteLine($"FriendUser Name: {friend.FriendUser.UserName ?? "NULL"}");
                    Console.WriteLine($"FriendUser Email: {friend.FriendUser.Email ?? "NULL"}");
                    // 其他FriendUser属性...
                }

                Console.WriteLine("-------------------");
            }

            return friends;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetSentFriendRequestsAsync: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            throw;
        }
    }

    public async Task<bool> AreFriendsAsync(string userId, string friendsUserId)
    {
        var count = await _context.Friends
            .Where(f =>
                ((f.UserId == userId && f.FriendUserId == friendsUserId) ||
                 (f.UserId == friendsUserId && f.FriendUserId == userId)) &&
                f.Status == 1)
            .CountAsync();

        return count > 0;
    }

    public async Task<bool> RemoveFriendAsync(int friendsId)
    {
        var entity = await _context.Friends
            .FirstOrDefaultAsync(f => f.FriendId == friendsId);

        _context.Friends.Remove(entity);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<Friend?> GetFriendRecordByIdAsync(int friendsId)
    {
        return await _context.Friends
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.FriendId == friendsId);
    }

    public async Task<int> GetFriendCountAsync(string userId)
    {
        return await _context.Friends
            .CountAsync(f => f.UserId == userId && f.Status == FriendStatus.Accepted);
    }

    public async Task<List<Friend>> SearchFriendsAsync(string userId, string searchTerm)
    {
        return await _context.Friends
            .Include(f => f.FriendUser)
            .Where(f => f.UserId == userId &&
                       f.Status == FriendStatus.Accepted &&
                       f.FriendUser.UserName.Contains(searchTerm))
            .ToListAsync();
    }

    public async Task<List<User>> SearchFriendsByNameAsync(string userName)
    {
        return await _context.Users
            .Where(u => u.UserName.Contains(userName))
            .ToListAsync();
    }
}