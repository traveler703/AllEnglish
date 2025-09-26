namespace AllEnBackend.Models
{
    public class UserRankHistory
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public int? Score { get; set; }
        public int? Activity { get; set; }
        public DateTime? CreatedAt { get; set; } =default(DateTime);
    }
}
