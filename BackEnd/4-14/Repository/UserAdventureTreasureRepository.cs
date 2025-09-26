using AllEnBackend.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AllEnBackend.Repository
{
    public class UserAdventureTreasureRepository : IUserAdventureTreasureRepository
    {
        private readonly DbContext _context;

        public UserAdventureTreasureRepository(DbContext context)
        {
            _context = context;
        }

        public async Task<UserAdventureTreasure> GetByIdAsync(long id)
        {
            return await _context.Set<UserAdventureTreasure>()
                .Include(ut => ut.Treasure)
                .FirstOrDefaultAsync(ut => ut.Id == id);
        }

        public async Task<List<UserAdventureTreasure>> GetAllAsync()
        {
            return await _context.Set<UserAdventureTreasure>()
                .Include(ut => ut.Treasure)
                .ToListAsync();
        }

        public async Task<List<UserAdventureTreasure>> GetUserTreasuresAsync(long userId)
        {
            return await _context.Set<UserAdventureTreasure>()
                .Where(ut => ut.UserId == userId)
                .Include(ut => ut.Treasure)
                .OrderByDescending(ut => ut.OpenedAt)
                .ToListAsync();
        }

        public async Task AddAsync(UserAdventureTreasure entity)
        {
            entity.OpenedAt = DateTime.UtcNow;

            await _context.Set<UserAdventureTreasure>().AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(UserAdventureTreasure entity)
        {
            _context.Set<UserAdventureTreasure>().Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(long id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _context.Set<UserAdventureTreasure>().Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> HasUserOpenedTreasure(long userId, long treasureId)
        {
            return await _context.Set<UserAdventureTreasure>()
                .AnyAsync(ut => ut.UserId == userId && ut.TreasureId == treasureId);
        }
    }
}