using AllEnBackend.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AllEnBackend.Services
{
    public interface IUserAdventureService
    {
        Task<UserAdventureDto> GetUserAdventureAsync(string userId, long adventureId);
        Task<List<UserAdventureDto>> GetUserAdventuresAsync(string userId);
        Task<bool> UpdateUserAdventureAsync(string userId, long adventureId, string status);
        Task<AdventureProgressDto> GetUserAdventureProgressAsync(string userId);
    }
}