using AllEnBackend.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AllEnBackend.Repository
{
    public interface IAdventureRepository : IRepository<Adventure>
    {
        // 可以添加Adventure特有的方法
        Task<List<Adventure>> GetActiveAdventuresAsync();
    }
}