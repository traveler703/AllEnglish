// Models/UpdateUserRequest.cs
namespace AllEnBackend.Models
{
    public class UpdateUserRequest
    {
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }
}
