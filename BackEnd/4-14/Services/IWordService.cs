using AllEnBackend.Dtos;
using AllEnBackend.Models;
using static AllEnBackend.Dtos.WordDetailDto;

public interface IWordService
{
    // 用名称获取单词详情
    Task<WordDetailDto?> GetWordDetailByNameAsync(string wordName);
    // 根据ID获取单词详情
    Task<WordDetailDto?> GetWordDetailByIdAsync(int wordId);
    // 更新单词信息
    Task<bool> UpdateWordAsync(string wordName, WordDetailDto newData);
    // 用user_id查询学过的单词的id
    Task<UserWordDto> GetUserWordsByUserIdAsync(string userId, UserWordQueryDto queryParams);
    // 添加或更新用户单词学习记录
    Task<bool> AddOrUpdateUserWordAsync(string userId, int wordId, int hasLearned, int correctCount, int learnCount, int hasBookmarked);
    // 更新用户单词学习统计信息
    Task<bool> UpdateLearningStatsAsync(string userId, int wordId, bool isCorrect);
    // 切换单词收藏状态
    Task<bool> ToggleBookmarkAsync(string userId, int wordId);
    // 移除用户的单词学习记录
    Task<bool> RemoveUserWordAsync(string userId, int wordId);
    // 获取用户特定单词的详细学习信息
    Task<UserWordDetailDto?> GetUserWordDetailAsync(string userId, int wordId);
}
