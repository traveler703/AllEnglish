using AllEnBackend.Models;
using AllEnBackend.Dtos;
public interface ILeaderboardRepository
{
    Task<UserRankingData?> GetByUserIdAsync(string userId);
    Task<UserRankingData> CreateAsync(UserRankingData userRanking);
    Task<UserRankingData> UpdateAsync(UserRankingData userRanking);
    Task<List<UserRankingData>> GetLeaderboardByScoreAsync(int page, int size);
    Task<List<UserRankingData>> GetLeaderboardByActivityAsync(int page, int size);
    Task<List<UserRankingData>> GetLeaderboardByUsernameAsync(int page, int size);
    Task<long> GetTotalUsersCountAsync();
    Task<List<UserRankingData>> GetAllOrderByScoreAsync();
    Task<List<UserRankingData>> GetAllOrderByActivityAsync();
    Task UpdateRankingsAsync(List<UserRankingData> rankings);
    Task<List<UserRankingData>> GetNearbyUsersAsync(string userId, string type, int count = 5);
    Task<UserBestRanking?> GetByUserIdBestRankAsync(string userId);
    Task<UserBestRanking> CreateAsync(UserBestRanking bestRanking);
    Task<UserBestRanking> UpdateAsync(UserBestRanking bestRanking);
    Task UpdateWordLearning(string userId);
    Task UpdateArticleLearning(string userId);
    Task UpdateListeningLearning(string userId);

    Task<List<UserRankHistory>> GetByPeriodAsync(DateTime start, DateTime end);
}
