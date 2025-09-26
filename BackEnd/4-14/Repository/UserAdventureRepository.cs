using AllEnBackend.Data;
using AllEnBackend.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AllEnBackend.Repository
{
    public class UserAdventureRepository : IUserAdventureRepository
    {
        private readonly AppDbContext _context;

        public UserAdventureRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UserAdventure> GetByUserAndAdventureAsync(string userId, long adventureId)
        {
            return await _context.UserAdventures
                .FirstOrDefaultAsync(ua => ua.UserId == userId && ua.AdventureId == adventureId);
        }

        public async Task<List<UserAdventure>> GetUserAdventuresAsync(string userId)
        {
            return await _context.UserAdventures
                .Where(ua => ua.UserId == userId)
                .OrderBy(ua => ua.AdventureId) // 按冒险ID排序
                .ToListAsync();
        }

        public async Task<List<UserAdventure>> GetUserAdventuresByStatusAsync(string userId, string status)
        {
            return await _context.UserAdventures
                .Where(ua => ua.UserId == userId && ua.Status == status)
                .ToListAsync();
        }

        public async Task<bool> UpdateAsync(UserAdventure userAdventure)
        {
            _context.UserAdventures.Update(userAdventure);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> AddAsync(UserAdventure userAdventure)
        {
            await _context.UserAdventures.AddAsync(userAdventure);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<int> GetUserAdventureCountAsync(string userId)
        {
            return await _context.UserAdventures
                .CountAsync(ua => ua.UserId == userId);
        }

        public async Task<int> GetUserAdventureCountByStatusAsync(string userId, string status)
        {
            return await _context.UserAdventures
                .CountAsync(ua => ua.UserId == userId && ua.Status == status);
        }
    }
}