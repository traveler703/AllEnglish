using AllEnBackend.Dtos;          // 引入包含 WordDetailDto 数据传输对象的命名空间

// WordService 实现了 IWordService 接口，提供与单词相关的业务逻辑
public class WordService : IWordService
{
    private readonly IWordRepository _repo;  // 定义只读字段用于访问底层数据仓储
    private readonly ILeaderboardRepository _leaderboardRepo;

    // 构造函数注入 IWordRepository，实现依赖注入
    public WordService(IWordRepository repo, ILeaderboardRepository leaderboardRepository)
    {
        _repo = repo;
        _leaderboardRepo = leaderboardRepository;
    }

    // 根据单词名称异步获取单词详情（包含定义和翻译）
    public Task<WordDetailDto?> GetWordDetailByNameAsync(string wordName) =>
      _repo.GetWordDetailByNameAsync(wordName);

    // 根据单词 ID 异步获取单词详情（包含定义和翻译）
    public Task<WordDetailDto?> GetWordDetailByIdAsync(int wordId)
    {
        return _repo.GetWordDetailByIdAsync(wordId);
    }

    // 异步更新指定单词的中英文释义
    public async Task<bool> UpdateWordAsync(string wordName, WordDetailDto newData)
    {
        // 先查找当前是否已有该单词
        var existing = await _repo.GetWordDetailByNameAsync(wordName);
        if (existing == null) return false;  // 若不存在则更新失败

        // 更新单词的英文释义和中文翻译
        existing.EnglishDefinitions = newData.EnglishDefinitions;
        existing.ChineseTranslations = newData.ChineseTranslations;

        // 保存更改
        await _repo.SaveAsync();
        return true;  // 更新成功
    }
    public async Task<UserWordDto> GetUserWordsByUserIdAsync(string userId, UserWordQueryDto queryParams)
    {
        // 验证考纲范围参数
        if (queryParams.SyllabusScope != "-1")
        {
            var validScopes = new[] { "cet4", "cet6", "postgraduate" };
            var inputScopes = queryParams.SyllabusScope.Split(',')
                .Select(s => s.Trim().ToLower())
                .ToList();

            if (inputScopes.Any(s => !validScopes.Contains(s)))
            {
                throw new ArgumentException("无效的考纲范围，只支持 cet4, cet6, postgraduate 或其组合");
            }
        }

        return await _repo.GetUserWordsByUserIdAsync(userId, queryParams);
    }

    public async Task<bool> AddOrUpdateUserWordAsync(string userId, int wordId, int hasLearned, int correctCount, int learnCount, int hasBookmarked)
    {
        return await _repo.AddOrUpdateUserWordAsync(userId, wordId, hasLearned, correctCount, learnCount, hasBookmarked);
    }

    public async Task<bool> UpdateLearningStatsAsync(string userId, int wordId, bool isCorrect)
    {
        await _repo.UpdateLearningStatsAsync(userId, wordId, isCorrect);
        await _leaderboardRepo.UpdateWordLearning(userId);
        return true;
    }

    public async Task<bool> ToggleBookmarkAsync(string userId, int wordId)
    {
        return await _repo.ToggleBookmarkAsync(userId, wordId);
    }

    public async Task<bool> RemoveUserWordAsync(string userId, int wordId)
    {
        return await _repo.RemoveUserWordAsync(userId, wordId);
    }

    public async Task<UserWordDetailDto?> GetUserWordDetailAsync(string userId, int wordId)
    {
        return await _repo.GetUserWordDetailAsync(userId, wordId);
    }
}
