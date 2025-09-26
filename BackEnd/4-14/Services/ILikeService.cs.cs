namespace AllEnBackend.Services
{
    public interface ILikeService
    {
        Task<bool> ToggleLikeAsync(int postId, string userId);
        Task<int> GetLikesCountAsync(int postId);
        Task<bool> IsPostLikedByUserAsync(int postId, string userId);
    }
}
