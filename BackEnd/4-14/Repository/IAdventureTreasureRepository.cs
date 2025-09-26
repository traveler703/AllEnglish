using AllEnBackend.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AllEnBackend.Repository
{
    public interface IAdventureTreasureRepository : IRepository<AdventureTreasure>
    {
        // 可以添加AdventureTreasure特有的方法
        Task<List<AdventureTreasure>> GetTreasuresByLevelAsync(int level);
    }
}