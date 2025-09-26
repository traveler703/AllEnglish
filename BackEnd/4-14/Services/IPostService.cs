using AllEnBackend.Dtos;

namespace AllEnBackend.Services
{
    public interface IPostService
    {
        Task<IEnumerable<PostResponseDto>> GetAllPostsAsync(string? currentUserId = null);
        Task<PostResponseDto> GetPostByIdAsync(int id, string? currentUserId = null);
        Task<PostResponseDto> CreatePostAsync(CreatePostDto createPostDto, string userId);
        Task<PostResponseDto> UpdatePostAsync(int id, UpdatePostDto updatePostDto, string userId);
        Task<bool> DeletePostAsync(int id, string userId);
        Task<IEnumerable<PostResponseDto>> GetPostsByUserIdAsync(string userId, string? currentUserId = null);
    }
}
