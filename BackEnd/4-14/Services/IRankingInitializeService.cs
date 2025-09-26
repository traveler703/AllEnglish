namespace AllEnBackend.Services
{
    public interface IRankingInitializeService
    {
        Task InitializeExistingUsersAsync();
        Task InitializeNewUserAsync(string userId, string username);
        Task RecalculateRankingsAsync();
        Task<UserLearningStats> CalculateUserLearningStatsAsync(string userId);
    }
}
