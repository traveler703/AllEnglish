using AllEnBackend.Dtos;

namespace AllEnBackend.Services
{
    public interface ICommentService
    {
        Task<CommentResponseDto> CreateCommentAsync(CreateCommentDto createCommentDto, string userId);
        Task<IEnumerable<CommentResponseDto>> GetCommentsByPostIdAsync(int postId);
        Task<bool> DeleteCommentAsync(int id, string userId);
    }
}

