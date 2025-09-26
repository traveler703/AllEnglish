using AllEnBackend.Models;
using Microsoft.AspNetCore.Identity;
using Oracle.ManagedDataAccess.Client;
using System.Net.Mail;
using System.Net;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.PortableExecutable;
using Microsoft.EntityFrameworkCore;
using AllEnBackend.Data;

namespace AllEnBackend.Services
{
    public class UserService : IUserService
    {
        //哈希密码
        private readonly PasswordHasher<User> _passwordHasher = new PasswordHasher<User>();
        //链接数据库
        private readonly string _connectionString;

        // 构造函数
        public UserService(IConfiguration configuration)
        {
            // 连接数据库
            _connectionString = configuration.GetConnectionString("OracleDb")
                ?? throw new ArgumentNullException("OracleDb 连接字符串未配置");
        }

        //登陆前发邮箱验证码
        public async Task<bool> SendVerificationCodeAsync(string email, string purpose)
        {
            try
            {
                using var connection = new OracleConnection(_connectionString);
                await connection.OpenAsync();

                // 检查邮箱是否已存在
                string checkSql = "SELECT COUNT(*) FROM users WHERE email = :email";
                using var checkCommand = new OracleCommand(checkSql, connection);
                checkCommand.Parameters.Add(new OracleParameter("email", email));
                var count = Convert.ToInt32(await checkCommand.ExecuteScalarAsync());

                // 判断用途
                if (purpose == "register" && count > 0)
                {
                    Console.WriteLine("邮箱已注册，注册失败");
                    return false; // 注册时邮箱不能已存在
                }

                if (purpose == "reset" && count == 0)
                {
                    Console.WriteLine("邮箱未注册，找回密码失败");
                    return false; // 找回密码时邮箱必须已存在
                }

                Console.WriteLine("准备发送验证码");

                // 生成验证码
                string verificationCode = GenerateVerificationCode();
                Console.WriteLine($"生成的验证码为：{verificationCode}");

                // 发送验证码邮件
                bool mailSent = SendVerificationEmail(email, verificationCode);
                if (!mailSent)
                {
                    Console.WriteLine("验证码发送失败");
                    return false;
                }
                else
                {
                    Console.WriteLine("验证码发送成功");
                }

                //存入verification表
                string insertSql = @"INSERT INTO verification_codes (email, code, created_at)   
                             VALUES (:email, :code, SYSDATE)";
                using var insertCommand = new OracleCommand(insertSql, connection);
                insertCommand.Parameters.Add(new OracleParameter("email", email));
                insertCommand.Parameters.Add(new OracleParameter("code", verificationCode));
                await insertCommand.ExecuteNonQueryAsync();
                Console.WriteLine("验证码已写入数据库");
                return true;
            }
            catch (OracleException ex)
            {
                Console.Error.WriteLine("数据库错误：" + ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("发送验证码时发生未知错误：" + ex.Message);
                return false;
            }
        }

        //验证验证码
        public async Task<bool> VerifyEmailCodeAsync(string email, string code)
        {
            try
            {
                using var connection = new OracleConnection(_connectionString);
                await connection.OpenAsync();

                string sql = @"
                    SELECT code, created_at, is_used FROM (
                        SELECT code, created_at, is_used
                        FROM verification_codes
                        WHERE email = :email
                        AND created_at >= SYSDATE - INTERVAL '10' MINUTE
                        ORDER BY created_at DESC
                    ) WHERE ROWNUM = 1";

                using var command = new OracleCommand(sql, connection);
                command.Parameters.Add(new OracleParameter("email", email));

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    string latestCode = reader.GetString(0);
                    DateTime createdAt = reader.GetDateTime(1);
                    int isUsed = reader.GetInt32(2);
                    Console.WriteLine("读取成功");
                    Console.WriteLine(createdAt.ToString());
                    if (isUsed == 0 && latestCode == code)
                    {
                        // 验证成功后，标记该验证码为已使用 
                        Console.WriteLine("验证码一致时间没问题");
                        string updateSql = @"UPDATE verification_codes
                                     SET is_used = 1
                                     WHERE email = :email AND code = :code";

                        using var updateCommand = new OracleCommand(updateSql, connection);
                        updateCommand.Parameters.Add(new OracleParameter("email", email));
                        updateCommand.Parameters.Add(new OracleParameter("code", code));
                        Console.WriteLine("问题不大");
                        await updateCommand.ExecuteNonQueryAsync();
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("验证码验证错误：" + ex.Message);
                return false;
            }
        }

        // 用户注册
        public async Task<bool> RegisterAsync(User user)
        {
            try
            {
                using var connection = new OracleConnection(_connectionString);
                await connection.OpenAsync();

                // 检查邮箱是否已存在
                string checkSql = "SELECT COUNT(*) FROM users WHERE email = :email";
                using var checkCommand = new OracleCommand(checkSql, connection);
                checkCommand.Parameters.Add(new OracleParameter("email", user.Email));
                var count = Convert.ToInt32(await checkCommand.ExecuteScalarAsync());

                if (count > 0)
                {
                    Console.WriteLine("邮箱已经存在，注册失败");
                    return false; // 邮箱已存在，注册失败
                }
                Console.WriteLine("邮箱不存在");

                // 构造 SQL 插入语句
                string insertSql = @"
                    INSERT INTO users (
                        id, username, password, email,
                        gender, dateofbirth, phonenumber,
                        category, avatarUrl
                    )
                    VALUES (
                        :id, :username, :password, :email,
                        :gender, :dateofbirth, :phonenumber,
                        :category, :avatarUrl
                    )";

                string userid = Guid.NewGuid().ToString();

                using var insertCommand = new OracleCommand(insertSql, connection);
                insertCommand.Parameters.Add(new OracleParameter("id", userid));
                insertCommand.Parameters.Add(new OracleParameter("username", user.UserName));
                string hashedPassword = _passwordHasher.HashPassword(user, user.Password);
                insertCommand.Parameters.Add(new OracleParameter("password", hashedPassword));
                insertCommand.Parameters.Add(new OracleParameter("email", user.Email));
                insertCommand.Parameters.Add(new OracleParameter("gender", user.Gender));
                insertCommand.Parameters.Add(new OracleParameter("dateofbirth", user.DateOfBirth));
                insertCommand.Parameters.Add(new OracleParameter("phonenumber", user.PhoneNumber));
                insertCommand.Parameters.Add(new OracleParameter("category", "user"));
                insertCommand.Parameters.Add(new OracleParameter("avatarUrl", user.AvatarUrl));
                Console.WriteLine(user.AvatarUrl);
                int rowsAffected = await insertCommand.ExecuteNonQueryAsync();
                Console.WriteLine("插入成功");

                if (rowsAffected > 0)
                {
                    await InitializeUserCoinAsync(userid);
                }

                return rowsAffected > 0;
            }
            catch (OracleException ex)
            {
                Console.Error.WriteLine("数据库错误：" + ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("注册时发生未知错误：" + ex.Message);
                return false;
            }
        }

        // 用户登录
        public async Task<User?> LoginAsync(string email, string password)
        {
            try
            {
                using var connection = new OracleConnection(_connectionString);
                await connection.OpenAsync();
                Console.WriteLine("数据库连接成功!");

                string sql = @"SELECT id, username, password, email ,category, avatarUrl, PhoneNumber FROM users WHERE email = :email";

                using var command = new OracleCommand(sql, connection);
                command.Parameters.Add(new OracleParameter("email", email));

                using var reader = await command.ExecuteReaderAsync();
                if (!reader.HasRows)
                    return null; // 用户不存在
                await reader.ReadAsync();
                Console.WriteLine("用户存在");


                // 从数据库读取用户字段
                var user = new User
                {
                    Id = reader.GetString(0),
                    UserName = reader.GetString(1),
                    Email = reader.GetString(3),
                    Category=reader.GetString(4),
                    AvatarUrl=reader.GetString(5),
                    PhoneNumber = reader.GetString(6)
                };
                Console.WriteLine("读取正确");

                // 验证密码
                string hashedPassword = reader.GetString(2);
                PasswordVerificationResult result = _passwordHasher.VerifyHashedPassword(user, hashedPassword, password);
                Console.WriteLine("读取正确");
                if (result == PasswordVerificationResult.Success)
                {
                    return user;
                }
                else
                    return null;
            }
            catch (OracleException ex)
            {
                // 可以打印日志、写入错误表，或返回 null 代表失败
                Console.Error.WriteLine("数据库错误：" + ex.Message);
                return null;
            }
            catch (Exception ex)
            {
                // 捕获所有未预料异常
                Console.Error.WriteLine("未知错误：" + ex.Message);
                return null;
            }
        }

        //用户退出登录
        public async Task<bool> LogoutAsync(string email)
        {
            return true;
        }

        //用户注销
        public async Task<bool> DeleteUserAsync(string email)
        {
            using var connection = new OracleConnection(_connectionString);
            await connection.OpenAsync();

            // 1. 查找用户ID
            string getUserIdSql = "SELECT id FROM users WHERE email = :email";
            using var getUserIdCommand = new OracleCommand(getUserIdSql, connection);
            getUserIdCommand.Parameters.Add(new OracleParameter("email", email));
            var userIdObj = await getUserIdCommand.ExecuteScalarAsync();
            if (userIdObj == null)
                return false;
            string userId = userIdObj.ToString();

            // 2. 先删除USERCOIN表
            string deleteCoinSql = "DELETE FROM USERCOIN WHERE USERID = :userId";
            using var deleteCoinCommand = new OracleCommand(deleteCoinSql, connection);
            deleteCoinCommand.Parameters.Add(new OracleParameter("userId", userId));
            await deleteCoinCommand.ExecuteNonQueryAsync();

            // 3. 再删除USERS表
            string deleteUserSql = "DELETE FROM users WHERE email=:email";
            using var deleteUserCommand = new OracleCommand(deleteUserSql, connection);
            deleteUserCommand.Parameters.Add(new OracleParameter("email", email));
            int deleteAffected = await deleteUserCommand.ExecuteNonQueryAsync();

            return deleteAffected > 0;
        }

        // 更新用户个人信息（注意：一次只能修改一个类型）
        public async Task<bool> UpdateProfileAsync(string userId, string TypeOfContent, string content, UserProfileUpdateRequest request)
        {
            string sql_id = "SELECT Id, UserName, Gender, PhoneNumber, AvatarUrl FROM Users WHERE Id = :userId";

            List<string> users_temp = new List<string>();

            using (OracleConnection conn = new OracleConnection(_connectionString))
            {
                try
                {
                    await conn.OpenAsync();
                    Console.WriteLine("数据库连接成功!");

                    using (OracleCommand cmd = new OracleCommand(sql_id, conn))
                    {
                        cmd.Parameters.Add(new OracleParameter(":userId", userId));

                        using (OracleDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (reader.Read())
                            {
                                string id = reader.GetString(0);
                                string userName = reader.GetString(1);
                                string gender = reader.GetString(2);
                                string phoneNumber = reader.GetString(3);
                                string avatarUrl = reader.IsDBNull(4) ? string.Empty : reader.GetString(4);

                                users_temp.Add($"Id: {id}, UserName: {userName}, Gender: {gender}, PhoneNumber: {phoneNumber}, AvatarUrl: {avatarUrl}");
                            }
                        }
                    }
                }
                catch (OracleException ex)
                {
                    Console.WriteLine($"数据库操作失败: {ex.Message}");
                    return false;
                }
            }

            if (users_temp.Count == 0)
            {
                Console.WriteLine("没有找到符合要求的用户。");
                return false;
            }

            var user = users_temp.FirstOrDefault();
            Console.WriteLine($"查询到的用户为：{user}");

            string originalValue = string.Empty;
            switch (TypeOfContent)
            {
                case "UserName":
                    originalValue = user?.Split(',')[1].Trim().Split(':')[1].Trim() ?? string.Empty;
                    break;
                case "Gender":
                    originalValue = user?.Split(',')[2].Trim().Split(':')[1].Trim() ?? string.Empty;
                    break;
                case "PhoneNumber":
                    originalValue = user?.Split(',')[3].Trim().Split(':')[1].Trim() ?? string.Empty;
                    break;
                case "AvatarUrl":
                    originalValue = user?.Split(',')[4].Trim().Split(':')[1].Trim() ?? string.Empty;
                    break;
                default:
                    Console.WriteLine("没有匹配的修改信息类型");
                    return false;
            }

            if (originalValue == content)
            {
                Console.WriteLine("新内容与原内容相同，更新操作被跳过。");
                return false;
            }

            using (OracleConnection connection = new OracleConnection(_connectionString))
            {
                await connection.OpenAsync();

                string sql_change = string.Empty;
                switch (TypeOfContent)
                {
                    case "UserName":
                        sql_change = "UPDATE Users SET UserName = :content WHERE Id = :userId";
                        break;
                    case "Gender":
                        sql_change = "UPDATE Users SET Gender = :content WHERE Id = :userId";
                        break;
                    case "PhoneNumber":
                        sql_change = "UPDATE Users SET PhoneNumber = :content WHERE Id = :userId";
                        break;
                    case "AvatarUrl":
                        sql_change = "UPDATE Users SET AvatarUrl = :content WHERE Id = :userId";
                        break;
                    default:
                        Console.WriteLine("没有匹配的修改信息类型");
                        return false;
                }

                using (OracleCommand command = new OracleCommand(sql_change, connection))
                {
                    command.Parameters.Add(new OracleParameter(":content", content));
                    command.Parameters.Add(new OracleParameter(":userId", userId));

                    try
                    {
                        int rowsAffected = await command.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                            Console.WriteLine("用户信息更新成功！");
                            return true;
                        }
                        else
                        {
                            Console.WriteLine("没有找到匹配的用户，更新失败！");
                            return false;
                        }
                    }
                    catch (OracleException ex)
                    {
                        Console.WriteLine($"更新操作失败: {ex.Message}");
                        return false;
                    }
                }
            }
        }

        public async Task<bool> ChangePasswordAsync( string userEmail, string newPassword)
        {
            // 查询用户信息
            string sql_id = "SELECT Id, Email, Password FROM Users WHERE email = :email";
            string userId = "";

            List<string> users_temp = new List<string>();
            string oldPassword = string.Empty;

            // 执行查询语句
            using (OracleConnection conn = new OracleConnection(_connectionString))
            {
                try
                {
                    await conn.OpenAsync();
                    Console.WriteLine("数据库连接成功!");

                    // 创建 OracleCommand 对象来执行查询
                    using (OracleCommand cmd = new OracleCommand(sql_id, conn))
                    {
                        cmd.Parameters.Add(new OracleParameter(":email", userEmail));

                        using (OracleDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (reader.Read())
                            {
                                string id = reader.GetString(0);
                                userId = reader.GetString(0);
                                string email = reader.GetString(1);
                                oldPassword = reader.GetString(2);

                                users_temp.Add($"Id: {id}, Email: {email}, Password: {oldPassword}");
                            }
                        }
                    }
                    // 处理查询到的结果
                    if (users_temp.Count == 0)
                    {
                        Console.WriteLine("没有找到符合要求的用户。");
                        return false;
                    }


                    // 更新密码
                    return await UpdatePasswordInDatabase(userId, newPassword);
                }
                catch (OracleException ex)
                {
                    Console.WriteLine($"数据库操作失败: {ex.Message}");
                    return false;
                }
            }


        }

        // 生成6位验证码
        static string GenerateVerificationCode()
        {
            Random random = new Random();
            StringBuilder code = new StringBuilder();
            for (int i = 0; i < 6; i++)
            {
                code.Append(random.Next(0, 10)); // 生成0-9之间的数字
            }
            return code.ToString();
        }

        // 发送验证码到指定邮箱
        static bool SendVerificationEmail(string toEmail, string verificationCode)
        {
            try
            {
                string fromEmail = "3478448962@qq.com";
                string fromPassword = "pmjvsaviwhcedadj";

                // 设置邮件内容
                string subject = "验证码";
                string body = $"<h1>您的验证码是: {verificationCode}</h1><p>请勿将验证码泄露给他人。</p>";

                SmtpClient smtpClient = new SmtpClient("smtp.qq.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(fromEmail, fromPassword),
                    EnableSsl = true
                };

                MailMessage mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);  // 设置收件人

                smtpClient.Send(mailMessage);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"发送邮件失败: {ex.Message}");
                return false;
            }
        }

        // 更新密码到数据库
        async Task<bool> UpdatePasswordInDatabase(string userId, string newPassword)
        {
            string selectSql = @"SELECT
                        id, username, password, email,
                        gender, dateofbirth, phonenumber,
                        category, avatarUrl
                    FROM USERS
                    WHERE id=:userId";

            string sql_change = "UPDATE Users SET Password = :newpassword WHERE Id = :userId";

            using (OracleConnection conn = new OracleConnection(_connectionString))
            {
                try
                {
                    await conn.OpenAsync();
                    using var selectCommand = new OracleCommand(selectSql, conn);
                    string hashedPassword = " ";
                    selectCommand.Parameters.Add(new OracleParameter("userId", userId));
                    using var reader = await selectCommand.ExecuteReaderAsync();
                    User user = new User();
                    if (await reader.ReadAsync())
                    {
                        user.Id = reader.GetString(0);
                        user.UserName = reader.GetString(1);
                        user.Password = reader.GetString(2);
                        user.Email = reader.GetString(3);
                        user.Gender = reader.GetString(4);
                        user.DateOfBirth = reader.IsDBNull(5) ? (DateTime?)null : reader.GetDateTime(5);
                        user.PhoneNumber = reader.GetString(6);
                        user.Category = reader.GetString(7);
                        user.AvatarUrl = reader.IsDBNull(8) ? string.Empty : reader.GetString(8);
                    }
                    else
                    {
                        Console.WriteLine("没有找到匹配的用户，更新失败！");
                        return false;
                    }

                    
                    
                    using (OracleCommand cmd = new OracleCommand(sql_change, conn))
                    {
                        hashedPassword = _passwordHasher.HashPassword(user, newPassword);
                        cmd.Parameters.Add(new OracleParameter(":newpassword", hashedPassword));
                        cmd.Parameters.Add(new OracleParameter(":userId", userId));

                        int rowsAffected = await cmd.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                            Console.WriteLine("用户密码更新成功！");
                            return true;
                        }
                        else
                        {
                            Console.WriteLine("没有找到匹配的用户，更新失败！");
                            return false;
                        }
                    }
                    
                }
                catch (OracleException ex)
                {
                    Console.WriteLine($"更新密码操作失败: {ex.Message}");
                    return false;
                }
            }
        }

        //获取初始虚拟币
       public async Task InitializeUserCoinAsync(string userId)
        {
            using var connection = new OracleConnection(_connectionString);
            await connection.OpenAsync();

            string insertSql = @"
                INSERT INTO USERCOIN (ID, USERID, COIN, LASTSIGNDATE)
                VALUES (:id, :userId, :coin, :lastSignDate)";

            using var command = new OracleCommand(insertSql, connection);
            command.Parameters.Add(new OracleParameter("id", Guid.NewGuid().ToString()));
            command.Parameters.Add(new OracleParameter("userId", userId));
            command.Parameters.Add(new OracleParameter("coin", 1000));
            command.Parameters.Add(new OracleParameter("lastSignDate", DBNull.Value));

            await command.ExecuteNonQueryAsync();
        }
        // 获取所有用户的信息
        public async Task<List<string>> GetAllUserIdsAsync()
        {
            var userIds = new List<string>();

            try
            {
                using var connection = new OracleConnection(_connectionString);
                await connection.OpenAsync();

                const string sql = "SELECT id FROM users";

                using var command = new OracleCommand(sql, connection);
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    userIds.Add(reader.GetString(0));
                }

                return userIds;
            }
            catch (OracleException ex)
            {
                Console.Error.WriteLine($"数据库错误: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"获取用户ID时发生错误: {ex.Message}");
                throw; 
            }
        }


    }
}
