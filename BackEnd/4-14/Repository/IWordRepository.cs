using AllEnBackend.Dtos;
using AllEnBackend.Models;
using static AllEnBackend.Dtos.WordDetailDto;

// 单词数据访问接口，定义了对单词数据的操作规范
public interface IWordRepository
{
    // 根据单词名称获取单词详细信息
    Task<WordDetailDto?> GetWordDetailByNameAsync(string wordName);

    // 根据单词ID获取单词详细信息
    Task<WordDetailDto?> GetWordDetailByIdAsync(int wordId);

    // 保存对数据库的更改
    Task SaveAsync();

    // 获取指定用户的单词列表
    Task<UserWordDto> GetUserWordsByUserIdAsync(string userId, UserWordQueryDto queryParams);

    // 添加或更新用户单词记录
    Task<bool> AddOrUpdateUserWordAsync(string userId, int wordId, int hasLearned, int correctCount, int learnCount, int hasBookmarked);

    // 更新用户单词学习统计信息
    Task<bool> UpdateLearningStatsAsync(string userId, int wordId, bool isCorrect);

    // 切换单词收藏状态
    Task<bool> ToggleBookmarkAsync(string userId, int wordId);

    // 移除用户单词记录
    Task<bool> RemoveUserWordAsync(string userId, int wordId);

    // 获取用户单词的详细信息
    Task<UserWordDetailDto?> GetUserWordDetailAsync(string userId, int wordId);
}