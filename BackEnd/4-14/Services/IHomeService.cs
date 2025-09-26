using AllEnBackend.Models;

namespace AllEnBackend.Services
{
    public interface IHomeService
    {
        public  Task<HomeCardsResponse> GetHomeCardsAsync(string userId);
    }
}
