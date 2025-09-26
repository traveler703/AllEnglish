namespace AllEnBackend.Dtos;

public class QuestionDto
{
    public int Id { get; set; }
    public int ArticleId { get; set; }
    public string Seqo { get; set; } = string.Empty;
    public int? Kind { get; set; }
    public string Stem { get; set; } = string.Empty;
    public string Options { get; set; } = string.Empty;
    public string AnswerKey { get; set; } = string.Empty;
    public int? Score { get; set; }
    public DateTime? CreatedAt { get; set; }
}

public class ArticleDetailDto : ArticleDto
{
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<string> Tags { get; set; } = new();
    public List<QuestionDto> Questions { get; set; } = new List<QuestionDto>();
}

public class ArticleDto
{
    public int ArticleId { get; set; }
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int Difficulty { get; set; }
    public string DifficultyLevel { get; set; } = string.Empty;
    public string? CoverImage { get; set; }
    public string? Category { get; set; }
    public int? ReadingTime { get; set; }
    public int? WordCount { get; set; }
    public string? Description { get; set; }
}

public class ArticleListDto
{
    public int ArticleId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Difficulty { get; set; }
    public string DifficultyLevel { get; set; } = string.Empty;
    public string? CoverImage { get; set; }
    public string? Category { get; set; }
    public int? ReadingTime { get; set; }
    public int? WordCount { get; set; }
    public string? Description { get; set; }
}

public class PaginatedResult<T>
{
    public List<T> Items { get; set; } = new List<T>();
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;
}

public class ArticleQueryParams
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 8;
    public int? CourseId { get; set; }
    public int? Difficulty { get; set; }
    public string? Category { get; set; }
    public string? SearchTerm { get; set; }
}