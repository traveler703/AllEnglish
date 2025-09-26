using AllEnBackend.Models;
using Oracle.ManagedDataAccess.Client;

namespace AllEnBackend.Services
{
    public class AdminService : IAdminService
    {
        //链接数据库
        private readonly string _connectionString;

        // 构造函数
        public AdminService(IConfiguration configuration)
        {
            // 连接数据库
            _connectionString = configuration.GetConnectionString("OracleDb")
                ?? throw new ArgumentNullException("OracleDb 连接字符串未配置");
        }

        public async Task<List<User>> SearchUserAsync()
        {
            var users = new List<User>();
            using var connection = new OracleConnection(_connectionString);
            await connection.OpenAsync();

            var sql = "SELECT id, username, password, email, gender, dateofbirth, phonenumber, category FROM users";
            using var command = new OracleCommand(sql, connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                users.Add(new User
                {
                    Id = reader.GetString(0),
                    UserName = reader.GetString(1),
                    Email = reader.GetString(3),
                    Gender = reader.GetString(4),
                    PhoneNumber = reader.GetString(6),
                    Category = reader.GetString(7),
                    DateOfBirth = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
                });
            }

            return users;
            ;
        }
        public async Task<bool> UpdateUserAsync(User user)
        {
            using var connection = new OracleConnection(_connectionString);
            await connection.OpenAsync();

        
            var sql = @"UPDATE users SET 
                      username = :username,
                      email = :email,
                      category = :category
                    WHERE id = :id";

            using var command = new OracleCommand(sql, connection);
            command.Parameters.Add(new OracleParameter("username", user.UserName));
            command.Parameters.Add(new OracleParameter("email", user.Email));
            command.Parameters.Add(new OracleParameter("category", user.Category));
            command.Parameters.Add(new OracleParameter("id", user.Id));

            return await command.ExecuteNonQueryAsync() > 0;
        }
    
        //删除用户
        public async Task<bool> DeleteUserAsync(string email)
        {
            using var connection = new OracleConnection(_connectionString);
            await connection.OpenAsync();
            string sql = "DELETE FROM users WHERE email=:email";
            using var command = new OracleCommand(sql, connection);
            command.Parameters.Add(new OracleParameter("email", email));
            int deleteAffected = await command.ExecuteNonQueryAsync();
            return deleteAffected > 0;
        }

        public async Task<bool> SaveAsync(Material material)
        {
            using var connection = new OracleConnection(_connectionString);
            await connection.OpenAsync();

            material.UpdateDate = DateTime.Now;

            bool isNew = string.IsNullOrEmpty(material.Id);
            string sql;

            if (isNew)
            {
                material.Id = Guid.NewGuid().ToString();
                sql = @"INSERT INTO material (
            id, 
            materialtype,   
            examtype,      
            skilltype,      
            price, 
            updatedate,     
            is_active, 
            description, 
            url,
            previewurl  
        ) VALUES (
            :id, 
            :materialtype, 
            :examtype, 
            :skilltype, 
            :price, 
            :updatedate, 
            :is_active, 
            :description, 
            :url,
            :previewurl  
        )";
            }
            else
            {
                sql = @"UPDATE material SET
            materialtype = :materialtype, 
            examtype = :examtype,          
            skilltype = :skilltype,        
            price = :price,
            updatedate = :updatedate,      
            is_active = :is_active,
            description = :description,
            url = :url,
            previewurl = :previewurl  
        WHERE id = :id";
            }

            using var command = new OracleCommand(sql, connection);

            // 添加参数 (使用修正后的参数名)
            command.Parameters.Add(new OracleParameter("id", material.Id));
            command.Parameters.Add(new OracleParameter("materialtype", material.MaterialType));
            command.Parameters.Add(new OracleParameter("examtype", material.ExamType));
            command.Parameters.Add(new OracleParameter("skilltype", material.SkillType));
            command.Parameters.Add(new OracleParameter("price", material.Price));
            command.Parameters.Add(new OracleParameter("updatedate", material.UpdateDate));
            command.Parameters.Add(new OracleParameter("is_active", 1));
            command.Parameters.Add(new OracleParameter("description", material.Description ?? string.Empty));
            command.Parameters.Add(new OracleParameter("url", material.Url));
            command.Parameters.Add(new OracleParameter("previewurl", material.PreviewUrl ?? string.Empty));

            int affectedRows = await command.ExecuteNonQueryAsync();
            Console.WriteLine(material.IsActive);
            return affectedRows > 0;
        }

        public async Task<bool> DeleteMatAsync(string id)
        {
            using var connection = new OracleConnection(_connectionString);
            await connection.OpenAsync();

            string sql = "DELETE FROM material WHERE id = :id";
            using var command = new OracleCommand(sql, connection);
            command.Parameters.Add(new OracleParameter("id", id));

            int affectedRows = await command.ExecuteNonQueryAsync();
            return affectedRows > 0;
        }

        public async Task<List<Material>> GetAllMaterialsAsync()
        {
            var materials = new List<Material>();
            using var connection = new OracleConnection(_connectionString);
            await connection.OpenAsync();

            string sql = @"SELECT id, materialtype, examtype, skilltype, price, updatedate, is_active, description, url FROM material";
            using var command = new OracleCommand(sql, connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                materials.Add(new Material
                {
                    Id = reader.GetString(0),
                    MaterialType = reader.GetString(1),
                    ExamType = reader.GetString(2),
                    SkillType = reader.GetString(3),
                    Price = reader.GetDecimal(4),
                    UpdateDate = reader.GetDateTime(5),
                    IsActive = reader.GetInt32(6),
                    Description = reader.IsDBNull(7) ? null : reader.GetString(7),
                    Url = reader.GetString(8)
                });
            }

            return materials;
        }

        public async Task<bool> UpdateMaterialStatusAsync(string id)
        {
            using var connection = new OracleConnection(_connectionString);
            await connection.OpenAsync();

            // 查询当前 is_active 状态
            string Activesql = "SELECT is_active FROM material WHERE id = :id";
            using var command1 = new OracleCommand(Activesql, connection);
            command1.Parameters.Add(new OracleParameter("id", id));

            int isActive = 0;

            using (var reader = await command1.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    isActive = reader.GetInt32(0);
                }
                else
                {
                    throw new Exception("未找到指定的课程");
                }
            }


            string sql = "UPDATE material SET is_active = :is_active WHERE id = :id";
            using var command = new OracleCommand(sql, connection);
            command.Parameters.Add(new OracleParameter("is_active", isActive==0 ? 1 : 0));
            command.Parameters.Add(new OracleParameter("id", id));

            int affectedRows = await command.ExecuteNonQueryAsync();
            return affectedRows > 0;
        }

        public async Task<bool> SaveAdAsync(Advertisement ad)
        {
            using var connection = new OracleConnection(_connectionString);
            await connection.OpenAsync();

            bool isNew = string.IsNullOrEmpty(ad.Id);
            string sql;

            if (isNew)
            {
                ad.Id = Guid.NewGuid().ToString();
                if (ad.CreateTime == default) ad.CreateTime = DateTime.Now;
                sql = @"
        INSERT INTO advertisement (id, mediaurl, targetid, context, status, clickcount, createtime)
        VALUES (:id, :mediaurl, :targetid, :context, :status, :clickcount, :createtime)";
            }
            else
            {
                sql = @"
        UPDATE advertisement
           SET mediaurl = :mediaurl,
               targetid = :targetid,
               context = :context,
               status = :status
         WHERE id = :id";
            }

            using var cmd = new OracleCommand(sql, connection);
            cmd.Parameters.Add(new OracleParameter("id", ad.Id));
            cmd.Parameters.Add(new OracleParameter("mediaurl", ad.MediaUrl ?? string.Empty));
            cmd.Parameters.Add(new OracleParameter("targetid", ad.TargetId ?? string.Empty));
            cmd.Parameters.Add(new OracleParameter("context", ad.Context ?? string.Empty));
            cmd.Parameters.Add(new OracleParameter("status", ad.Status));
            if (isNew)
            {
                cmd.Parameters.Add(new OracleParameter("clickcount", ad.ClickCount));
                cmd.Parameters.Add(new OracleParameter("createtime", ad.CreateTime));
            }

            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public async Task<List<Advertisement>> GetAllAdsAsync()
        {
            var ads = new List<Advertisement>();
            using var connection = new OracleConnection(_connectionString);
            await connection.OpenAsync();

            string sql = @"SELECT id, mediaurl, targetid, context, status, clickcount, createtime
                     FROM advertisement
                 ORDER BY createtime DESC";

            using var cmd = new OracleCommand(sql, connection);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                ads.Add(new Advertisement
                {
                    Id = reader.GetString(0),
                    MediaUrl = reader.GetString(1),
                    TargetId = reader.GetString(2),
                    Context = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                    Status = reader.GetInt32(4),
                    ClickCount = reader.IsDBNull(5) ? 0 : reader.GetInt32(5),
                    CreateTime = reader.GetDateTime(6)
                });
            }
            return ads;
        }

        public async Task<bool> UpdateAdStatusAsync(string id)
        {
            using var connection = new OracleConnection(_connectionString);
            await connection.OpenAsync();

            // 1) 查当前状态
            string getSql = "SELECT status FROM advertisement WHERE id = :id";
            int status = 0;
            using (var getCmd = new OracleCommand(getSql, connection))
            {
                getCmd.Parameters.Add(new OracleParameter("id", id));
                using var r = await getCmd.ExecuteReaderAsync();
                if (await r.ReadAsync())
                    status = r.GetInt32(0);
                else
                    return false; // 未找到
            }

            // 2) 取反写回
            string upd = "UPDATE advertisement SET status = :status WHERE id = :id";
            using var updCmd = new OracleCommand(upd, connection);
            updCmd.Parameters.Add(new OracleParameter("status", status == 0 ? 1 : 0));
            updCmd.Parameters.Add(new OracleParameter("id", id));

            return await updCmd.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> DeleteAdAsync(string id)
        {
            using var connection = new OracleConnection(_connectionString);
            await connection.OpenAsync();

            string sql = "DELETE FROM advertisement WHERE id = :id";
            using var cmd = new OracleCommand(sql, connection);
            cmd.Parameters.Add(new OracleParameter("id", id));
            return await cmd.ExecuteNonQueryAsync() > 0;
        }


    }
}

