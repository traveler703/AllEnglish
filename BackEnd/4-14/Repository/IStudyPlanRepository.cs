using AllEnBackend.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AllEnBackend.Repositories
{
    public interface IStudyPlanRepository
    {
        Task<StudyPlan> GetByIdAsync(int id);
        Task<IEnumerable<StudyPlan>> GetAllAsync();
        Task<IEnumerable<StudyPlan>> GetByFilterAsync(string userId, int wordCount, string planType, int duration, int articleCount, int oralTime, bool? isPublic);
        Task AddAsync(StudyPlan studyPlan);
        Task UpdateAsync(StudyPlan studyPlan);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<IEnumerable<StudyPlan>> GetRandomStudyPlansAsync(int count);
    }
}