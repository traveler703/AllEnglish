namespace AllEnBackend.Models
{
    public class Adventure
    {
        public long Id { get; set; }
        public int LevelNumber { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string Difficulty { get; set; }
        public string TargetType { get; set; }
        public long TargetValue { get; set; }
        public string RoutePath { get; set; }
        public string RouteParams { get; set; }
        public string Icon { get; set; }
        public long RewardExp { get; set; }
        public long RewardCoins { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class AdventureTreasure
    {
        public long Id { get; set; }
        public int LevelNumber { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Rewards { get; set; }
        public string Icon { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}

