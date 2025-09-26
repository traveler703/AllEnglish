using Microsoft.EntityFrameworkCore;
using AllEnBackend.Models;

namespace AllEnBackend.Repository
{
    public class AdventureRepository : IAdventureRepository
    {
        private readonly DbContext _context;

        public AdventureRepository(DbContext context)
        {
            _context = context;
        }

        public async Task<Adventure> GetByIdAsync(long id)
        {
            return await _context.Set<Adventure>().FindAsync(id);
        }

        public async Task<List<Adventure>> GetAllAsync()
        {
            return await _context.Set<Adventure>().ToListAsync();
        }

        public async Task<List<Adventure>> GetActiveAdventuresAsync()
        {
            return await _context.Set<Adventure>()
                .Where(a => a.IsActive)
                .ToListAsync();
        }

        public async Task AddAsync(Adventure entity)
        {
            await _context.Set<Adventure>().AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Adventure entity)
        {
            _context.Set<Adventure>().Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(long id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                _context.Set<Adventure>().Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}