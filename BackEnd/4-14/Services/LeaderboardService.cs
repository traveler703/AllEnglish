using AllEnBackend.Services;
using AllEnBackend.Models;
using Microsoft.EntityFrameworkCore;
using AllEnBackend.Data;

public class LeaderboardService : ILeaderboardService
{
    private readonly ILeaderboardRepository _repository;
    private readonly AppDbContext _context;
 
    // 分数配置
    private const int WORD_SCORE = 10;
    private const int READING_SCORE = 50;
    private const int LISTENING_SCORE = 30;
    private const int ACTIVITY_SCORE = 5;

    public LeaderboardService(ILeaderboardRepository rankingRepository,AppDbContext context)
    {
        _repository = rankingRepository;
        _context = context;
    }

    public async Task CompleteWordLearningAsync(string userId, int wordCount)
    {
        await UpdateUserScoreAsync(userId, wordCount * WORD_SCORE, wordCount, 0, 0, "WORD");
    }

    public async Task CompleteReadingAsync(string userId, int readingCount)
    {
        await UpdateUserScoreAsync(userId, readingCount * READING_SCORE, 0, readingCount, 0, "READING");
    }

    public async Task CompleteListeningAsync(string userId, int listeningCount)
    {
        await UpdateUserScoreAsync(userId, listeningCount * LISTENING_SCORE, 0, 0, listeningCount, "LISTENING");
    }

    private async Task UpdateUserScoreAsync(string userId, int scoreIncrease, int wordIncrease, int readingIncrease, int listeningIncrease, string type)
    {
        // 1. 获取或创建用户排行数据
        var userRanking = await _repository.GetByUserIdAsync(userId);
        if (userRanking == null)
        {
            userRanking = await CreateNewUserRankingAsync(userId);
        }

        // 2. 更新用户数据
        userRanking.Score += scoreIncrease;
        userRanking.ActivityScore += ACTIVITY_SCORE;
        userRanking.WordCount += wordIncrease;
        userRanking.ReadingCount += readingIncrease;
        userRanking.ListeningCount += listeningIncrease;

        // 3. 保存用户数据变更
        await _repository.UpdateAsync(userRanking);

        // 4. 异步更新排行榜排名
        _ = Task.Run(async () => await UpdateAllRankingsAsync());

        // 5. 更新最佳排名记录
        await UpdateBestRankingAsync(userId);
    }

    private async Task<UserRankingData> CreateNewUserRankingAsync(string userId)
    {
        var newRanking = new UserRankingData
        {
            UserId = userId,
            Username = $"User_{userId}",
            Score = 0,
            ActivityScore = 0
        };

        await _repository.CreateAsync(newRanking);

        // 同时创建最佳排名记录
        var bestRanking = new UserBestRanking
        {
            UserId = userId
        };
        await _repository.CreateAsync(bestRanking);

        return newRanking;
    }

    private List<RankDto> AggregateRank(IEnumerable<UserRankHistory> histories, IQueryable<User> users,string rankType)
    {
        var grouped = histories
            .GroupBy(h => new { h.UserId, h.UserName })
            .Select(g => new {
                g.Key.UserId,
                g.Key.UserName,
                TotalScore = g.Sum(x => x.Score),
                TotalActivity = g.Sum(x => x.Activity)
            })
            .ToList();

        // 一次性取所有用户的头像
        var userDict = users.ToDictionary(u => u.Id, u => u.AvatarUrl);

        var ordered = grouped.OrderByDescending(r => r.TotalScore);

        if(rankType == "activity")
        {
            ordered = grouped.OrderByDescending(r => r.TotalActivity);
        }
        else
        {
            ordered = grouped.OrderByDescending(r => r.TotalScore);
        }

        var result = ordered.Select((g, index) => new RankDto
        {
            UserId = g.UserId,
            UserName = g.UserName,
            AvatarUrl = userDict[g.UserId],
            TotalScore = g.TotalScore,
            TotalActivity = g.TotalActivity,
            RankNo = index + 1
        }).ToList();

        return result;
    }

    public async Task<List<RankDto>> GetDayRankAsync(string rankType)
    {
        var today = DateTime.Now.Date;
        var tomorrow = today.AddDays(1);

        var data = await _repository.GetByPeriodAsync(today, tomorrow);
        return AggregateRank(data, _context.Users,rankType);
    }

    public async Task<List<RankDto>> GetWeekRankAsync(string rankType)
    {
        var today = DateTime.Now.Date;
        var diff = (int)today.DayOfWeek == 0 ? 6 : (int)today.DayOfWeek - 1;
        var weekStart = today.AddDays(-diff);
        var weekEnd = weekStart.AddDays(7);

        var data = await _repository.GetByPeriodAsync(weekStart, weekEnd);
        return AggregateRank(data, _context.Users, rankType);
    }

    public async Task<List<RankDto>> GetMonthRankAsync(string rankType)
    {
        var today = DateTime.Now.Date;
        var monthStart = new DateTime(today.Year, today.Month, 1);
        var monthEnd = monthStart.AddMonths(1);

        var data = await _repository.GetByPeriodAsync(monthStart, monthEnd);
        return AggregateRank(data, _context.Users, rankType);
    }

    public async Task UpdateAllRankingsAsync()
    {
        await UpdateScoreRankingsAsync();
        await UpdateActivityRankingsAsync();
    }

    private async Task UpdateScoreRankingsAsync()
    {
        var users = await _repository.GetAllOrderByScoreAsync();

        for (int i = 0; i < users.Count; i++)
        {
            var user = users[i];
            int newRank = i + 1;

            user.LastRankScore = user.CurrentRankScore;
            user.CurrentRankScore = newRank;
        }

        await _repository.UpdateRankingsAsync(users);
    }

    private async Task UpdateActivityRankingsAsync()
    {
        var users = await _repository.GetAllOrderByActivityAsync();

        for (int i = 0; i < users.Count; i++)
        {
            var user = users[i];
            int newRank = i + 1;

            user.LastRankActivity = user.CurrentRankActivity;
            user.CurrentRankActivity = newRank;
        }

        await _repository.UpdateRankingsAsync(users);
    }

    private async Task UpdateBestRankingAsync(string userId)
    {
        var current = await _repository.GetByUserIdAsync(userId);
        var best = await _repository.GetByUserIdBestRankAsync(userId);

        if (current == null || best == null) return;

        bool updated = false;

        // 检查是否刷新最佳分数排名
        if (current.CurrentRankScore > best.BestRankScore)
        {
            best.BestRankScore = current.CurrentRankScore;
            best.BestScore = current.Score;
            updated = true;
        }

        // 检查是否刷新最佳活跃度排名
        if (current.CurrentRankActivity < best.BestRankActivity)
        {
            best.BestRankActivity = current.CurrentRankActivity;
            best.BestActivityScore = current.ActivityScore;
            updated = true;
        }

        if (updated)
        {
            best.AchievedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(best);
        }
    }

    public async Task<LeaderboardResponseDto> GetLeaderboardAsync(string type, int page, int size)
    {
        List<UserRankingData> users;
        var totalUsers = await _repository.GetTotalUsersCountAsync();

        switch (type.ToLower())
        {
            case "score":
                users = await _repository.GetLeaderboardByScoreAsync(page, size);
                break;
            case "activity":
                users = await _repository.GetLeaderboardByActivityAsync(page, size);
                break;
            default:
                users = await _repository.GetLeaderboardByUsernameAsync(page, size);
                break;
        }

        // 先取所有要查的 UserId
        var userIds = users.Select(u => u.UserId).ToList();

        // 一次性查出头像字典
        var avatars = await _context.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.AvatarUrl);

        var response = new LeaderboardResponseDto
        {
            Total = totalUsers,
            Page = page,
            Size = size,
            Type = type,
            UpdateTime = DateTime.UtcNow,
            Rankings = users.Select(user => new RankingItemDto
            {
                UserId = user.UserId,
                Username = user.Username, 
                Avartar = avatars[user.UserId],
                Rank = type.ToLower() == "score" ? user.CurrentRankScore : user.CurrentRankActivity,
                Score = type.ToLower() == "score" ? user.Score : user.ActivityScore,
                Change = CalculateRankChange(
                    type.ToLower() == "score" ? user.CurrentRankScore : user.CurrentRankActivity,
                    type.ToLower() == "score" ? user.LastRankScore : user.LastRankActivity),
                Trend = CalculateTrend(
                    type.ToLower() == "score" ? user.CurrentRankScore : user.CurrentRankActivity,
                    type.ToLower() == "score" ? user.LastRankScore : user.LastRankActivity)
            }).ToList()
        };

        return response;
    }

    public async Task<UserRankInfoDto> GetUserRankInfoAsync(string type, string userId)
    {
        var user = await _repository.GetByUserIdAsync(userId);
        var bestRanking = await _repository.GetByUserIdBestRankAsync(userId);
        var totalUsers = await _repository.GetTotalUsersCountAsync();
        var nearbyUsers = await _repository.GetNearbyUsersAsync(userId, type);

        if (user == null)
        {
            return new UserRankInfoDto();
        }

        var currentRank = type.ToLower() == "score" ? user.CurrentRankScore : user.CurrentRankActivity;
        var currentScore = type.ToLower() == "score" ? user.Score : user.ActivityScore;
        var lastRank = type.ToLower() == "score" ? user.LastRankScore : user.LastRankActivity;

        return new UserRankInfoDto
        {
            Rank = currentRank,
            Score = currentScore,
            TotalUsers = totalUsers,
            Percentile = totalUsers > 0 ? (double)(totalUsers - currentRank + 1) / totalUsers * 100 : 0,
            Change = CalculateRankChange(currentRank, lastRank),
            Trend = CalculateTrend(currentRank, lastRank),
            BestRanking = bestRanking != null ? new UserBestRankingDto
            {
                BestRankScore = bestRanking.BestRankScore,
                BestRankActivity = bestRanking.BestRankActivity,
                BestScore = bestRanking.BestScore,
                BestActivityScore = bestRanking.BestActivityScore,
                AchievedAt = bestRanking.AchievedAt
            } : null,
            NearbyUsers = nearbyUsers.Select(u => new NearbyUserDto
            {
                Rank = type.ToLower() == "score" ? u.CurrentRankScore : u.CurrentRankActivity,
                Username = u.Username,
                Score = type.ToLower() == "score" ? u.Score : u.ActivityScore
            }).ToList()
        };
    }

    public async Task UpdateScoreAsync(ScoreUpdateRequestDto request)
    {
        var user = await _repository.GetByUserIdAsync(request.UserId);
        if (user == null) return;

        switch (request.Operation.ToLower())
        {
            case "add":
                user.Score += request.ScoreChange;
                break;
            case "subtract":
                user.Score -= request.ScoreChange;
                break;
            case "set":
                user.Score = request.ScoreChange;
                break;
        }

        await _repository.UpdateAsync(user);
        await UpdateBestRankingAsync(request.UserId);
    }

    private static string CalculateRankChange(int currentRank, int lastRank)
    {
        if (lastRank == 0) return "new";
        int change = lastRank - currentRank;
        return change > 0 ? $"+{change}" : change.ToString();
    }

    private static string CalculateTrend(int currentRank, int lastRank)
    {
        if (lastRank == 0 || currentRank == lastRank) return "same";
        return currentRank < lastRank ? "up" : "down";
    }

    public async Task UpdateUserWordLearningAsync(string userId)
    {
        await _repository.UpdateWordLearning(userId);
    }

    public async Task UpdateUserListening(string userId)
    {
        await _repository.UpdateListeningLearning(userId);
    }
}
