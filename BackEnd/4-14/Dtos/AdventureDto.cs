namespace AllEnBackend.Dtos
{
    public class AdventureDto
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
    }

    public class CreateAdventureDto
    {
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
    }
}