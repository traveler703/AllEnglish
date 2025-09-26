using Microsoft.EntityFrameworkCore;
using AllEnBackend.Models;

namespace AllEnBackend.Repository
{
    public class AdventureTreasureRepository : IAdventureTreasureRepository
    {
        private readonly DbContext _context;

        public AdventureTreasureRepository(DbContext context)
        {
            _context = context;
        }

        public async Task<AdventureTreasure> GetByIdAsync(long id)
        {
            return await _context.Set<AdventureTreasure>().FindAsync(id);
        }

        public async Task<List<AdventureTreasure>> GetAllAsync()
        {
            return await _context.Set<AdventureTreasure>().ToListAsync();
        }

        public async Task<List<AdventureTreasure>> GetTreasuresByLevelAsync(int level)
        {
            return await _context.Set<AdventureTreasure>()
                .Where(t => t.LevelNumber == level && t.IsActive)
                .ToListAsync();
        }

        public async Task AddAsync(AdventureTreasure entity)
        {
            await _context.Set<AdventureTreasure>().AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(AdventureTreasure entity)
        {
            _context.Set<AdventureTreasure>().Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(long id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _context.Set<AdventureTreasure>().Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}