using AllEnBackend.Models;
using AllEnBackend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AllEnBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    /* URL: https://localhost:7071/api/user/action */

    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _cfg;
        public UserController(IUserService userService, IConfiguration cfg)
        {
            _userService = userService;
            _cfg = cfg;
        }
        // 发送验证码
        [HttpPost("send-verification-code")]
        public async Task<IActionResult> SendVerificationCode([FromBody] EmailRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest("邮箱不能为空");
            }

            var success = await _userService.SendVerificationCodeAsync(request.Email,request.Purpose);
            if (!success)
                return BadRequest("发送验证码失败，可能该邮箱已注册或邮箱地址无效");

            return Ok("验证码已发送，请检查您的邮箱");
        }
        //验证验证码
        [HttpPost("verify-code")]
        public async Task<IActionResult> VerifyCode([FromBody] VerifyCodeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Code))
            {
                return BadRequest("邮箱和验证码均不能为空");
            }

            var isValid = await _userService.VerifyEmailCodeAsync(request.Email, request.Code);
            if (!isValid)
                return BadRequest("验证码无效或已过期");

            return Ok("验证码验证成功");
        }
        // 用户注册
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterRequest request)
        {
            if (!DateTime.TryParse(request.DateOfBirth, out var birthDate))
            {
                return BadRequest("日期格式不正确，应为 YYYY-MM-DD");
            }

            var user = new User
            {
                UserName = request.UserName,
                Password = request.Password, // 可加密
                Email = request.Email,
                Gender = request.Gender,
                PhoneNumber = request.PhoneNumber,
                DateOfBirth = birthDate,
                AvatarUrl = request.AvatarUrl,

            };

            var success = await _userService.RegisterAsync(user);
            if (!success)
                return BadRequest("注册失败，用户名或邮箱可能已存在");
            return Ok("注册成功");
        }
        // 用户登录
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest request)
        {
            try
            {
                var user = await _userService.LoginAsync(request.Email, request.Password);

                if (user == null)
                    return Unauthorized("用户名或密码错误");


                // 1) 准备声明（Claims）
                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.Role, user.Category)
                };

                // 2) 读取 JWT 配置
                var secret = _cfg["Jwt:SecretKey"];
                var issuer = _cfg["Jwt:Issuer"];
                var audience = _cfg["Jwt:Audience"];
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                // 3) 生成 token
                var token = new JwtSecurityToken(
                    issuer: issuer,
                    audience: audience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddHours(2),
                    signingCredentials: creds
                );
                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                // 4) 返回给前端
                return Ok(new
                {
                    message = "登录成功",
                    token = tokenString,
                    id = user.Id, 
                    userName = user.UserName,
                    category = user.Category,
                    avatarUrl = user.AvatarUrl,
                    birthday = user.DateOfBirth,
                    gender = user.Gender,
                    phoneNumber = user.PhoneNumber,
                    email = user.Email
                });
            }
            catch (Exception ex)
            {
                // 可记录日志 ex.Message
                return StatusCode(500, "服务器内部错误: " + ex.Message);
            }
        }
        // 用户退出登录
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromQuery] string email)
        {
            try
            {
                var success = await _userService.LogoutAsync(email);

                if (!success)
                    return NotFound("用户不存在或已注销");

                return Ok("用户已成功退出登录");
            }
            catch (Exception ex)
            {
                // 可记录日志
                return StatusCode(500, "服务器内部错误: " + ex.Message);
            }
        }
        // 用户注销
        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteUser([FromQuery] string email)
        {
            try
            {
                var success = await _userService.DeleteUserAsync(email);

                if (!success)
                    return NotFound("用户不存在或已注销");

                return Ok("用户已成功注销并删除账号");
            }
            catch (Exception ex)
            {
                // 可记录日志 ex.Message
                return StatusCode(500, "服务器内部错误: " + ex.Message);
            }
        }
        // 更改用户信息（用户密码除外）
        [HttpPut("updateprofile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UserProfileUpdateRequest request)
        {
            string userId = request.Id;
            string TypeOfContent = request.TypeOfContent; 
            string content = request.Content; 

            // 调用 UpdateProfileAsync 函数更新用户信息
            var success = await _userService.UpdateProfileAsync(userId, TypeOfContent, content, request);

            if (!success)
            {
                // 如果更新失败，返回错误信息
                return BadRequest("更新失败");
            }

            // 如果成功，返回成功消息
            return Ok(new { message = "个人信息更新成功" });

        }
        // 更改用户密码
        [HttpPut("changePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] UserPasswordUpdateRequest request)
        {
            // 获取请求中的邮箱和新密码
            var userEmail = request.Email;
            var newPassword = request.NewPassword;

            // 调用服务层的ChangePasswordAsync方法，传入用户ID、邮箱和新密码
            var success = await _userService.ChangePasswordAsync( userEmail, newPassword);

            // 根据返回结果，返回相应的HTTP响应
            if (!success)
            {
                return BadRequest("修改密码失败");
            }

            return Ok("密码修改成功");
        }
        // 上传用户头像
        [HttpPost("upload-avatar")]
        public async Task<IActionResult> UploadAvatar()
        {
            var file = Request.Form.Files.FirstOrDefault();
            if (file == null || file.Length == 0)
                return BadRequest("未上传文件");

            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatars");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // 生成唯一文件名
            string fileExt = Path.GetExtension(file.FileName);
            string newFileName = $"{Guid.NewGuid()}{fileExt}";
            string filePath = Path.Combine(uploadsFolder, newFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            string relativePath = $"/uploads/avatars/{newFileName}"; // 前端可直接访问
            return Ok(new { path = relativePath });
        }
        [HttpGet("all-ids")]
        public async Task<IActionResult> GetAllUserIds()
        {
            try
            {
                var userIds = await _userService.GetAllUserIdsAsync();
                return Ok(userIds);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"获取用户ID时出错: {ex.Message}");
            }
        }
    }
}

