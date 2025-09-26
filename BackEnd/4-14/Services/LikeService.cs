using AllEnBackend.Models;
using AllEnBackend.Repository;

namespace AllEnBackend.Services
{
    public class LikeService : ILikeService
    {
        private readonly ILikeRepository _likeRepository;
        private readonly IPostRepository _postRepository;

        public LikeService(ILikeRepository likeRepository, IPostRepository postRepository)
        {
            _likeRepository = likeRepository;
            _postRepository = postRepository;
        }

        public async Task<bool> ToggleLikeAsync(int postId, string userId)
        {
            // 验证帖子是否存在
            var post = await _postRepository.GetPostByIdAsync(postId);
            if (post == null) return false;

            // 检查用户是否已经点赞
            var isLiked = await _likeRepository.IsPostLikedByUserAsync(postId, userId);
            var maxId = await _likeRepository.GetMaxLikeIdAsync();
            var nextId = maxId + 1;

            if (isLiked)
            {
                // 取消点赞
                return await _likeRepository.DeleteLikeAsync(postId, userId);
            }
            else
            {
                // 添加点赞
                var like = new Like
                {
                    Id = nextId,
                    PostId = postId,
                    UserId = userId,
                    CreatedAt = DateTime.Now
                };

                var createdLike = await _likeRepository.CreateLikeAsync(like);
                return createdLike != null;
            }
        }

        public async Task<int> GetLikesCountAsync(int postId)
        {
            return await _likeRepository.GetLikesCountByPostIdAsync(postId);
        }

        public async Task<bool> IsPostLikedByUserAsync(int postId, string userId)
        {
            return await _likeRepository.IsPostLikedByUserAsync(postId, userId);
        }
    }
}

