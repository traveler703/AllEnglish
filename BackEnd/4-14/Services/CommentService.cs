using AllEnBackend.Dtos;
using AllEnBackend.Models;
using AllEnBackend.Repository;

namespace AllEnBackend.Services
{
    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IPostRepository _postRepository;

        public CommentService(ICommentRepository commentRepository, IPostRepository postRepository)
        {
            _commentRepository = commentRepository;
            _postRepository = postRepository;
        }

        public async Task<CommentResponseDto> CreateCommentAsync(CreateCommentDto createCommentDto, string userId)
        {
            // 验证帖子是否存在
            var post = await _postRepository.GetPostByIdAsync(createCommentDto.PostId);
            if (post == null) return null;

            var maxId = await _commentRepository.GetMaxCommentIdAsync();
            var nextId = maxId + 1;

            var comment = new Comment
            {
                Id=nextId,
                PostId = createCommentDto.PostId,
                UserId = userId,
                Content = createCommentDto.Content,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            var createdComment = await _commentRepository.CreateCommentAsync(comment);
            var fullComment = await _commentRepository.GetCommentByIdAsync(createdComment.Id);

            return new CommentResponseDto
            {
                Id = fullComment.Id,
                PostId = fullComment.PostId,
                UserId = fullComment.UserId,
                Username = fullComment.User?.UserName,
                AvatarUrl = fullComment.User?.AvatarUrl,
                Content = fullComment.Content,
                CreatedAt = fullComment.CreatedAt
            };
        }

        public async Task<IEnumerable<CommentResponseDto>> GetCommentsByPostIdAsync(int postId)
        {
            var comments = await _commentRepository.GetCommentsByPostIdAsync(postId);

            return comments.Select(c => new CommentResponseDto
            {
                Id = c.Id,
                PostId = c.PostId,
                UserId = c.UserId,
                Username = c.User?.UserName,
                AvatarUrl = c.User?.AvatarUrl,
                Content = c.Content,
                CreatedAt = c.CreatedAt
            }).ToList();
        }

        public async Task<bool> DeleteCommentAsync(int id, string userId)
        {
            var comment = await _commentRepository.GetCommentByIdAsync(id);
            if (comment == null || comment.UserId != userId) return false;

            return await _commentRepository.DeleteCommentAsync(id);
        }
    }
}

