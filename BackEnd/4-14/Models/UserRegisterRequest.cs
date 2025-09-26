namespace AllEnBackend.Models
{
	public class UserRegisterRequest
	{
		public string UserName { get; set; } = string.Empty;  // 用户名
		public string Password { get; set; } = string.Empty;  // 密码
		public string Email { get; set; } = string.Empty;     // 邮箱
        public string Gender { get; set; } = string.Empty;          // 性别（如 "男"/"女" 或 "M"/"F"）
        public string DateOfBirth { get; set; } = string.Empty;            // 出生日期，允许为空
        public string PhoneNumber { get; set; } = string.Empty;     // 电话号码
        public string AvatarUrl { get; set; } = string.Empty;    // 用户头像路径
    }
}
