using AllEnBackend.Models;

namespace AllEnBackend.Services
{
    public interface IAdminService
    {
        Task<List<User>> SearchUserAsync(); // 查询所有用户
        Task<bool> UpdateUserAsync(User user); // 更新用户信息
        Task<bool> DeleteUserAsync(string userId); // 删除用户信息

        Task<bool> SaveAsync(Material material);// 上传学习资源
        Task <bool>DeleteMatAsync(string id);// 下架学习资源

        Task<bool> UpdateMaterialStatusAsync(string id);// 更新学习资源状态
        Task<List<Material>> GetAllMaterialsAsync();// 获取学习资源列表

        // 广告相关
        Task<bool> SaveAdAsync(Advertisement ad);           // 新增/更新
        Task<List<Advertisement>> GetAllAdsAsync();
        Task<bool> UpdateAdStatusAsync(string id);          // 切换启用/停用
        Task<bool> DeleteAdAsync(string id);

    }
}
