using AllEnBackend.Models;
using AllEnBackend.Dtos;

namespace AllEnBackend.Services
{
    public interface IPayingService
    {
        Task<int> GetUserCoinAsync(string userId);
        Task<SignInResultDto> DailySignInAsync(string userId);
        Task<PurchaseResultDto> PurchaseMaterialAsync(string userId, string materialId);
        Task<List<InventoryItemDto>> GetUserInventoryAsync(string userId);
        Task<bool> IsMaterialInInventoryAsync(string userId, string materialId);
        Task<List<Material>> GetAvailableMaterialsAsync();
        Task<string?> GetUserCategoryAsync(string userId);
        Task<string?> ISUserReinstate(string userId);
    }
}

