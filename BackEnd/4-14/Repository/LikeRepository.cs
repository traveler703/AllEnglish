using AllEnBackend.Data;
using AllEnBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace AllEnBackend.Repository
{
    public class LikeRepository: ILikeRepository
    {
        private readonly AppDbContext _context;

        public LikeRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Like> CreateLikeAsync(Like like)
        {
            _context.Likes.Add(like);
            await _context.SaveChangesAsync();
            return like;
        }

        public async Task<bool> DeleteLikeAsync(int postId, string userId)
        {
            var like = await _context.Likes
                .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);

            if (like == null) return false;

            _context.Likes.Remove(like);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsPostLikedByUserAsync(int postId, string userId)
        {
            var like = await _context.Likes
                .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);
            return like != null;
        }


        public async Task<int> GetLikesCountByPostIdAsync(int postId)
        {
            return await _context.Likes
                .CountAsync(l => l.PostId == postId);
        }

        public async Task<IEnumerable<Like>> GetLikesByPostIdAsync(int postId)
        {
            return await _context.Likes
                .Include(l => l.User)
                .Where(l => l.PostId == postId)
                .ToListAsync();
        }

        public async Task<int> GetMaxLikeIdAsync()
        {
            var maxId = await _context.Likes.MaxAsync(p => (int?)p.Id);
            return maxId ?? 0;
        }

    }
}
