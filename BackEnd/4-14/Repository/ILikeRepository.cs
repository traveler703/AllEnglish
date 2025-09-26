using AllEnBackend.Models;

namespace AllEnBackend.Repository
{
    public interface ILikeRepository
    {
        Task<Like> CreateLikeAsync(Like like);
        Task<bool> DeleteLikeAsync(int postId, string userId);
        Task<bool> IsPostLikedByUserAsync(int postId, string userId);
        Task<int> GetLikesCountByPostIdAsync(int postId);
        Task<IEnumerable<Like>> GetLikesByPostIdAsync(int postId);
        Task<int> GetMaxLikeIdAsync();
    }
}
