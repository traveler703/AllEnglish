using AllEnBackend.Models;
using AllEnBackend.Dtos;

namespace AllEnBackend.Services
{
    public interface IAdvertisementService
    {
        public Task<List<Advertisement>> GetAvailableAdAsync();

        public Task<List<Material>> GetMaterial(string Id);

    }
}

