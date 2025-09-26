public class LeaderboardResponseDto
{
    public long Total { get; set; }
    public int Page { get; set; }
    public int Size { get; set; }
    public string Type { get; set; }
    public DateTime UpdateTime { get; set; }
    public List<RankingItemDto> Rankings { get; set; } = new();
}

public class RankingItemDto
{
    public string UserId { get; set; }
    public string Username { get; set; }
    public string Avartar { get; set; }
    public int Rank { get; set; }
    public int Score { get; set; }
    public string Change { get; set; }  // 排名变化
    public string Trend { get; set; }   // up/down/same
    public Dictionary<string, object> ExtraData { get; set; } = new();
}

public class UserRankInfoDto
{
    public int Rank { get; set; }
    public int Score { get; set; }
    public long TotalUsers { get; set; }
    public double Percentile { get; set; }
    public string Change { get; set; }
    public string Trend { get; set; }
    public UserBestRankingDto BestRanking { get; set; }
    public List<NearbyUserDto> NearbyUsers { get; set; } = new();
}

public class UserBestRankingDto
{
    public int BestRankScore { get; set; }
    public int BestRankActivity { get; set; }
    public int BestScore { get; set; }
    public int BestActivityScore { get; set; }
    public DateTime? AchievedAt { get; set; }
}

public class NearbyUserDto
{
    public int Rank { get; set; }
    public string Username { get; set; }
    public int Score { get; set; }
}

public class ScoreUpdateRequestDto
{
    public string UserId { get; set; }
    public int ScoreChange { get; set; }
    public string Operation { get; set; } = "add"; // add/set/subtract
    public string Reason { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class UserLearningStats
{
    public string UserId { get; set; }
    public string Username { get; set; }

    public int LearnedWordCount { get; set; }
    public int TotalLearningCount { get; set; }
    public int TotalCorrectCount { get; set; }
    public int TotalAnswerCount { get; set; }
    public decimal AccuracyRate { get; set; }
    public int BookmarkedCount { get; set; }
    public int TotalWordCount { get; set; }
    public decimal CompletionRate { get; set; }
    public int TotalScore { get; set; }
    public int ActivityScore { get; set; }
    public int ReadingCount { get; set; }
    public int ListeningCount { get; set; }
    public int OralScore { get; set; }
    public DateTime? LastStudyTime { get; set; }
    public DateTime StatisticsTime { get; set; }
    public UserLearningStats()
    {
        StatisticsTime = DateTime.UtcNow;
    }
    public void CalculateAccuracyRate()
    {
        if (TotalAnswerCount > 0)
        {
            AccuracyRate = Math.Round((decimal)TotalCorrectCount / TotalAnswerCount * 100, 2);
        }
        else
        {
            AccuracyRate = 0;
        }
    }
    public void CalculateCompletionRate()
    {
        if (TotalWordCount > 0)
        {
            CompletionRate = Math.Round((decimal)LearnedWordCount / TotalWordCount * 100, 2);
        }
        else
        {
            CompletionRate = 0;
        }
    }
}

public class RankDto
{
    public string UserId { get; set; }
    public string UserName { get; set; }
    public string AvatarUrl { get; set; }
    public int? TotalScore { get; set; } = 0;
    public int? TotalActivity { get; set; } = 0;
    public int RankNo { get; set; }
}
