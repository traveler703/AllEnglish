namespace AllEnBackend.Models
{
    public class EmailRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Purpose { get; set; } = "register";
    }
}
