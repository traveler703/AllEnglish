// AdventureTreasureService.cs
using AllEnBackend.Dtos;
using AllEnBackend.Models;
using AllEnBackend.Repository;

namespace AllEnBackend.Services
{
    public class AdventureTreasureService : IAdventureTreasureService
    {
        private readonly IAdventureTreasureRepository _repository;

        public AdventureTreasureService(IAdventureTreasureRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<AdventureTreasureDto>> GetAllAsync()
        {
            var list = await _repository.GetAllAsync();
            return list.Select(t => MapToDto(t)).ToList();
        }

        public async Task<AdventureTreasureDto> GetByIdAsync(long id)
        {
            var treasure = await _repository.GetByIdAsync(id);
            return treasure == null ? null : MapToDto(treasure);
        }

        public async Task<List<AdventureTreasureDto>> GetByLevelAsync(int level)
        {
            var treasures = await _repository.GetTreasuresByLevelAsync(level);
            return treasures.Select(t => MapToDto(t)).ToList();
        }

        public async Task<long> CreateAsync(CreateAdventureTreasureDto dto)
        {
            var entity = new AdventureTreasure
            {
                LevelNumber = dto.LevelNumber,
                Title = dto.Title,
                Description = dto.Description,
                Rewards = dto.Rewards,
                Icon = dto.Icon,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _repository.AddAsync(entity);
            return entity.Id;
        }

        public async Task UpdateAsync(long id, CreateAdventureTreasureDto dto)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return;

            entity.LevelNumber = dto.LevelNumber;
            entity.Title = dto.Title;
            entity.Description = dto.Description;
            entity.Rewards = dto.Rewards;
            entity.Icon = dto.Icon;
            entity.IsActive = dto.IsActive;
            entity.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(entity);
        }

        public async Task DeleteAsync(long id)
        {
            await _repository.DeleteAsync(id);
        }

        private AdventureTreasureDto MapToDto(AdventureTreasure treasure)
        {
            return new AdventureTreasureDto
            {
                Id = treasure.Id,
                LevelNumber = treasure.LevelNumber,
                Title = treasure.Title,
                Description = treasure.Description,
                Rewards = treasure.Rewards,
                Icon = treasure.Icon,
                IsActive = treasure.IsActive
            };
        }
    }
}