using AllEnBackend.Models;

namespace AllEnBackend.Services
{
    public interface IUserService
    {
        //登陆前发验证码
        Task<bool> SendVerificationCodeAsync(string email, string purpose);
        //验证验证码
        Task<bool> VerifyEmailCodeAsync(string email, string code);
        // 用户注册
        Task<bool> RegisterAsync(User user);

        // 用户登录
        Task<User?> LoginAsync(string email, string password);
        // 用户退出登录
        Task<bool> LogoutAsync(string email);
        // 用户注销
        Task<bool> DeleteUserAsync(string userId);
        // 更新用户个人信息
        Task<bool> UpdateProfileAsync( string userId,string TypeOfContent, string content, UserProfileUpdateRequest request);
        // 修改密码
        Task<bool> ChangePasswordAsync(string userEmail, string newPassword);
        //查找所有用户信息
        Task<List<string>> GetAllUserIdsAsync();

    }
}

