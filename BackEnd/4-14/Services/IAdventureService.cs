using AllEnBackend.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AllEnBackend.Services
{
    public interface IAdventureService
    {
        Task<List<AdventureDto>> GetAllAsync();
        Task<AdventureDto> GetByIdAsync(long id);
        Task<long> CreateAsync(CreateAdventureDto dto);
        Task UpdateAsync(long id, CreateAdventureDto dto);
        Task DeleteAsync(long id);
    }
}