using AllEnBackend.Dtos;
using AllEnBackend.Models;
using AllEnBackend.Repository;

namespace AllEnBackend.Services
{
    public class UserAdventureService : IUserAdventureService
    {
        private readonly IUserAdventureRepository _userAdventureRepository;
        private readonly IAdventureRepository _adventureRepository;

        public UserAdventureService(
            IUserAdventureRepository userAdventureRepository,
            IAdventureRepository adventureRepository)
        {
            _userAdventureRepository = userAdventureRepository;
            _adventureRepository = adventureRepository;
        }

        public async Task<UserAdventureDto> GetUserAdventureAsync(string userId, long adventureId)
        {
            var userAdventure = await _userAdventureRepository.GetByUserAndAdventureAsync(userId, adventureId);
            if (userAdventure == null) return null;

            return MapToDto(userAdventure);
        }

        public async Task<List<UserAdventureDto>> GetUserAdventuresAsync(string userId)
        {
            var userAdventures = await _userAdventureRepository.GetUserAdventuresAsync(userId);
            return userAdventures.Select(MapToDto).ToList();
        }

        public async Task<AdventureProgressDto> GetUserAdventureProgressAsync(string userId)
        {
            var adventures = await _adventureRepository.GetAllAsync();
            var userAdventures = await _userAdventureRepository.GetUserAdventuresAsync(userId);

            var completedCount = userAdventures.Count(ua => ua.Status == "completed");
            var inProgressCount = userAdventures.Count(ua => ua.Status == "started");
            var totalCount = adventures.Count;

            return new AdventureProgressDto
            {
                TotalAdventures = totalCount,
                CompletedAdventures = completedCount,
                InProgressAdventures = inProgressCount,
                CompletionPercentage = totalCount > 0 ? (int)((double)completedCount / totalCount * 100) : 0
            };
        }

        public async Task<bool> UpdateUserAdventureAsync(string userId, long adventureId, string status)
        {
            // 验证状态值
            if (!IsValidStatus(status))
            {
                throw new ArgumentException($"无效的状态值: {status}", nameof(status));
            }

            //// 检查冒险是否存在
            //var adventureExists = await _adventureRepository.ExistsAsync(adventureId);
            //if (!adventureExists)
            //{
            //    throw new KeyNotFoundException($"未找到ID为 {adventureId} 的冒险");
            //}

            // 获取现有记录或创建新记录
            var userAdventure = await _userAdventureRepository.GetByUserAndAdventureAsync(userId, adventureId);

            if (userAdventure == null)
            {
                // 创建新记录
                userAdventure = new UserAdventure
                {
                    UserId = userId,
                    AdventureId = adventureId,
                    Status = status
                };

                return await _userAdventureRepository.AddAsync(userAdventure);
            }
            else
            {
                // 更新现有记录
                userAdventure.Status = status;

                return await _userAdventureRepository.UpdateAsync(userAdventure);
            }
        }

        private bool IsValidStatus(string status)
        {
            var validStatuses = new[] { "locked", "unlocked", "completed"};
            return validStatuses.Contains(status?.ToLower());
        }

        private UserAdventureDto MapToDto(UserAdventure userAdventure)
        {
            return new UserAdventureDto
            {
                UserId = userAdventure.UserId,
                AdventureId = userAdventure.AdventureId,
                Status = userAdventure.Status
            };
        }
    }
}