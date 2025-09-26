using AllEnBackend.Dtos;
using AllEnBackend.Models;
using AllEnBackend.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AllEnBackend.Services
{
    public class AdventureService : IAdventureService
    {
        private readonly IAdventureRepository _repository;
        private readonly IUserAdventureRepository _userAdventureRepository;

        public AdventureService(
            IAdventureRepository repository,
            IUserAdventureRepository userAdventureRepository)
        {
            _repository = repository;
            _userAdventureRepository = userAdventureRepository;
        }

        public async Task<List<AdventureDto>> GetAllAsync()
        {
            var list = await _repository.GetAllAsync();
            return list.Select(a => MapToDto(a)).ToList();
        }

        public async Task<AdventureDto> GetByIdAsync(long id)
        {
            var adventure = await _repository.GetByIdAsync(id);
            return adventure == null ? null : MapToDto(adventure);
        }

        public async Task<long> CreateAsync(CreateAdventureDto dto)
        {
            var entity = new Adventure
            {
                LevelNumber = dto.LevelNumber,
                Title = dto.Title,
                Description = dto.Description,
                Type = dto.Type,
                Difficulty = dto.Difficulty,
                TargetType = dto.TargetType,
                TargetValue = dto.TargetValue,
                RoutePath = dto.RoutePath,
                RouteParams = dto.RouteParams,
                Icon = dto.Icon,
                RewardExp = dto.RewardExp,
                RewardCoins = dto.RewardCoins,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _repository.AddAsync(entity);
            return entity.Id;
        }

        public async Task UpdateAsync(long id, CreateAdventureDto dto)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return;

            entity.LevelNumber = dto.LevelNumber;
            entity.Title = dto.Title;
            entity.Description = dto.Description;
            entity.Type = dto.Type;
            entity.Difficulty = dto.Difficulty;
            entity.TargetType = dto.TargetType;
            entity.TargetValue = dto.TargetValue;
            entity.RoutePath = dto.RoutePath;
            entity.RouteParams = dto.RouteParams;
            entity.Icon = dto.Icon;
            entity.RewardExp = dto.RewardExp;
            entity.RewardCoins = dto.RewardCoins;
            entity.IsActive = dto.IsActive;
            entity.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(entity);
        }

        public async Task DeleteAsync(long id)
        {
            await _repository.DeleteAsync(id);
        }


        private AdventureDto MapToDto(Adventure adventure)
        {
            return new AdventureDto
            {
                Id = adventure.Id,
                LevelNumber = adventure.LevelNumber,
                Title = adventure.Title,
                Description = adventure.Description,
                Type = adventure.Type,
                Difficulty = adventure.Difficulty,
                TargetType = adventure.TargetType,
                TargetValue = adventure.TargetValue,
                RoutePath = adventure.RoutePath,
                RouteParams = adventure.RouteParams,
                Icon = adventure.Icon,
                RewardExp = adventure.RewardExp,
                RewardCoins = adventure.RewardCoins,
                IsActive = adventure.IsActive
            };
        }
    }
}