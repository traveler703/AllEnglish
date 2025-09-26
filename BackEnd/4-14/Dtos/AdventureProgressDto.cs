namespace AllEnBackend.Dtos
{
    public class AdventureProgressDto
    {
        public int TotalAdventures { get; set; }
        public int CompletedAdventures { get; set; }
        public int InProgressAdventures { get; set; }
        public int CompletionPercentage { get; set; }
    }
}