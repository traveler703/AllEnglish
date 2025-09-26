using AllEnBackend.Models;

namespace AllEnBackend.Repository
{
    public interface IPostRepository
    {
        Task<IEnumerable<Post>> GetAllPostsAsync();
        Task<Post> GetPostByIdAsync(int id);
        Task<Post> CreatePostAsync(Post post);
        Task<Post> UpdatePostAsync(Post post);
        Task<bool> DeletePostAsync(int id);
        Task<IEnumerable<Post>> GetPostsByUserIdAsync(string userId);
        Task<int> GetMaxPostIdAsync();
    }
}
