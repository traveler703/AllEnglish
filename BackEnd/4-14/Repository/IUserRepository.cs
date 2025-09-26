using AllEnBackend.Models;
using System.Threading.Tasks;

namespace AllEnBackend.Repositories
{
  public interface IUserRepository
  {
    Task<bool> RegisterUserAsync(User user);
    Task<User?> GetUserByUserNameAsync(string userName);
  }
}
