using AllEnBackend.Dtos;
using AllEnBackend.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AllEnBackend.Repository
{
    public interface IUserAdventureRepository
    {
        Task<List<UserAdventure>> GetUserAdventuresAsync(string userId);
        Task<UserAdventure> GetByUserAndAdventureAsync(string userId, long adventureId);
        Task<bool> UpdateAsync(UserAdventure userAdventure);
        Task<bool> AddAsync(UserAdventure userAdventure);
    }
}