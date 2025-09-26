using AllEnBackend.Dtos;
using AllEnBackend.Models;
using AllEnBackend.Repository;

namespace AllEnBackend.Services
{
    public class PostService: IPostService
    {
        private readonly IPostRepository _postRepository;
        private readonly ILikeRepository _likeRepository;

        public PostService(IPostRepository postRepository, ILikeRepository likeRepository)
        {
            _postRepository = postRepository;
            _likeRepository = likeRepository;
        }

        public async Task<IEnumerable<PostResponseDto>> GetAllPostsAsync(string? currentUserId = null)
        {
            var posts = await _postRepository.GetAllPostsAsync();
            var result = new List<PostResponseDto>();

            foreach (var post in posts)
            {
                var postDto = await MapToPostResponseDto(post, currentUserId);
                result.Add(postDto);
            }

            return result;
        }

        public async Task<PostResponseDto> GetPostByIdAsync(int id, string? currentUserId = null)
        {
            var post = await _postRepository.GetPostByIdAsync(id);
            if (post == null) return null;

            return await MapToPostResponseDto(post, currentUserId);
        }

        public async Task<PostResponseDto> CreatePostAsync(CreatePostDto createPostDto, string userId)
        {
            // 获取下一个可用的ID
            var maxId = await _postRepository.GetMaxPostIdAsync();
            var nextId = maxId + 1;

            var post = new Post
            {
                Id = nextId, // 手动设置ID
                UserId = userId,
                Content = createPostDto.Content,
                ImageUrl = createPostDto.ImageUrl,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            var createdPost = await _postRepository.CreatePostAsync(post);
            var fullPost = await _postRepository.GetPostByIdAsync(createdPost.Id);

            return await MapToPostResponseDto(fullPost, userId);
        }


        public async Task<PostResponseDto> UpdatePostAsync(int id, UpdatePostDto updatePostDto, string userId)
        {
            var post = await _postRepository.GetPostByIdAsync(id);
            if (post == null || post.UserId != userId) return null;

            post.Content = updatePostDto.Content ?? post.Content;
            post.ImageUrl = updatePostDto.ImageUrl ?? post.ImageUrl;

            var updatedPost = await _postRepository.UpdatePostAsync(post);
            var fullPost = await _postRepository.GetPostByIdAsync(updatedPost.Id);

            return await MapToPostResponseDto(fullPost, userId);
        }

        public async Task<bool> DeletePostAsync(int id, string userId)
        {
            var post = await _postRepository.GetPostByIdAsync(id);
            if (post == null || post.UserId != userId) return false;

            return await _postRepository.DeletePostAsync(id);
        }

        public async Task<IEnumerable<PostResponseDto>> GetPostsByUserIdAsync(string userId, string? currentUserId = null)
        {
            var posts = await _postRepository.GetPostsByUserIdAsync(userId);
            var result = new List<PostResponseDto>();

            foreach (var post in posts)
            {
                var postDto = await MapToPostResponseDto(post, currentUserId);
                result.Add(postDto);
            }

            return result;
        }

        private async Task<PostResponseDto> MapToPostResponseDto(Post post, string? currentUserId = null)
        {
            var isLiked = false;
            if (!string.IsNullOrEmpty(currentUserId))
            {
                isLiked = await _likeRepository.IsPostLikedByUserAsync(post.Id, currentUserId);
            }

            return new PostResponseDto
            {
                Id = post.Id,
                UserId = post.UserId,
                Username = post.User?.UserName,
                AvatarUrl = post.User?.AvatarUrl,
                Content = post.Content,
                ImageUrl = post.ImageUrl,
                CreatedAt = post.CreatedAt,
                UpdatedAt = post.UpdatedAt,
                LikesCount = post.Likes?.Count ?? 0,
                CommentsCount = post.Comments?.Count ?? 0,
                IsLiked = isLiked,
                Comments = post.Comments?.Select(c => new CommentResponseDto
                {
                    Id = c.Id,
                    PostId = c.PostId,
                    UserId = c.UserId,
                    Username = c.User?.UserName,
                    AvatarUrl = c.User?.AvatarUrl,
                    Content = c.Content,
                    CreatedAt = c.CreatedAt
                }).ToList() ?? new List<CommentResponseDto>()
            };
        }
    }
}
