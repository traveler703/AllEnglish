using AllEnBackend.Models;
using System.Numerics;

namespace AllEnBackend.Repository
{
    public interface ICommentRepository
    {
        Task<Comment> CreateCommentAsync(Comment comment);
        Task<IEnumerable<Comment>> GetCommentsByPostIdAsync(int postId);
        Task<Comment> GetCommentByIdAsync(int id);
        Task<bool> DeleteCommentAsync(int id);
        Task<Comment> UpdateCommentAsync(Comment comment);

        Task<int> GetMaxCommentIdAsync();
    }
}
