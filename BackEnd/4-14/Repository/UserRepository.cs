using AllEnBackend.Models;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using System.Threading.Tasks;

namespace AllEnBackend.Repositories
{
  public class UserRepository : IUserRepository
  {
    private readonly string _connectionString;

    public UserRepository(IConfiguration configuration)
    {
      _connectionString = configuration.GetConnectionString("OracleDb");
    }

    public async Task<bool> RegisterUserAsync(User user)
    {
      using (var connection = new OracleConnection(_connectionString))
      {
        await connection.OpenAsync();

        string sql = "INSERT INTO USERS (ID, USERNAME, PASSWORD, EMAIL) " +
                     "VALUES (USER_SEQ.NEXTVAL, :username, :password, :email)";
        using (var command = new OracleCommand(sql, connection))
        {
          command.Parameters.Add(new OracleParameter("username", user.UserName));
          command.Parameters.Add(new OracleParameter("password", user.Password));
          command.Parameters.Add(new OracleParameter("email", user.Email));
          int rows = await command.ExecuteNonQueryAsync();
          return rows > 0;
        }
      }
    }

    public async Task<User?> GetUserByUserNameAsync(string userName)
    {
      using (var connection = new OracleConnection(_connectionString))
      {
        await connection.OpenAsync();

        string sql = "SELECT ID, USERNAME, PASSWORD, EMAIL FROM USERS WHERE USERNAME = :username";
        using (var command = new OracleCommand(sql, connection))
        {
          command.Parameters.Add(new OracleParameter("username", userName));
          using (var reader = await command.ExecuteReaderAsync())
          {
            if (await reader.ReadAsync())
            {
              return new User
              {
                Id = reader.GetString(0),
                UserName = reader.GetString(1),
                Password = reader.GetString(2),
                Email = reader.GetString(3)
              };
            }
          }
        }
      }
      return null;
    }
  }
}
