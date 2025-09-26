namespace AllEnBackend.Dtos
{
    public class CreatePostDto
    {
        public string? Content { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class UpdatePostDto
    {
        public string? Content { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class PostResponseDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string? Username { get; set; }
        public string? AvatarUrl { get; set; }

        public string? Content { get; set; }

        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int LikesCount { get; set; }
        public int CommentsCount { get; set; }
        public bool IsLiked { get; set; }
        public List<CommentResponseDto> Comments { get; set; } = new List<CommentResponseDto>();
    }

    public class CreateCommentDto
    {
        public int PostId { get; set; }
        public string Content { get; set; }
    }

    public class CommentResponseDto
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public string UserId { get; set; }
        public string? Username { get; set; }
        public string? AvatarUrl { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
