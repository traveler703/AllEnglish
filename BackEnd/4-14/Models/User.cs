namespace AllEnBackend.Models
{
    public class User
    {
        public string Id { get; set; }                           // 用户ID
        public string UserName { get; set; } = string.Empty;     // 用户名
        public string Password { get; set; } = string.Empty;     // 密码
        public string Email { get; set; } = string.Empty;        // 电子邮件
        public string Gender { get; set; } = string.Empty;       // 性别
        public DateTime? DateOfBirth { get; set; }               // 生日
        public string PhoneNumber { get; set; } = string.Empty;  // 电话号码
        public string Category { get; set; } = string.Empty;     // 用户类型
        public string AvatarUrl { get; set; } = string.Empty;    // 用户头像路径
    }
}
