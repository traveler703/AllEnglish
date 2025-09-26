using AllEnBackend.Dtos;
using AllEnBackend.Models;

namespace AllEnBackend.Services
{
    public interface ILeaderboardService
    {
        Task CompleteWordLearningAsync(string userId, int wordCount);
        Task CompleteReadingAsync(string userId, int readingCount);
        Task CompleteListeningAsync(string userId, int listeningCount);
        Task<LeaderboardResponseDto> GetLeaderboardAsync(string type, int page, int size);
        Task<UserRankInfoDto> GetUserRankInfoAsync(string type, string userId);
        Task UpdateScoreAsync(ScoreUpdateRequestDto request);
        Task UpdateAllRankingsAsync();
        Task UpdateUserWordLearningAsync(string userId);
        Task UpdateUserListening(string userId);
        Task<List<RankDto>> GetDayRankAsync(string rankType);
        Task<List<RankDto>> GetWeekRankAsync(string rankType);
        Task<List<RankDto>> GetMonthRankAsync(string rankType);
    }

}
