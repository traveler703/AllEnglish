using AllEnBackend.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AllEnBackend.Repository
{
    public interface IUserAdventureTreasureRepository : IRepository<UserAdventureTreasure>
    {
        Task<List<UserAdventureTreasure>> GetUserTreasuresAsync(long userId);
    }
}