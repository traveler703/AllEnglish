// IAdventureTreasureService.cs
using AllEnBackend.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AllEnBackend.Services
{
    public interface IAdventureTreasureService
    {
        Task<List<AdventureTreasureDto>> GetAllAsync();
        Task<AdventureTreasureDto> GetByIdAsync(long id);
        Task<List<AdventureTreasureDto>> GetByLevelAsync(int level);
        Task<long> CreateAsync(CreateAdventureTreasureDto dto);
        Task UpdateAsync(long id, CreateAdventureTreasureDto dto);
        Task DeleteAsync(long id);
    }
}