using AllEnBackend.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AllEnBackend.Services
{
    public interface IStudyPlanService
    {
        Task<StudyPlanDto> CreateStudyPlanAsync(CreateStudyPlanDto createDto);
        Task<StudyPlanDto> GetStudyPlanByIdAsync(int id);
        Task<IEnumerable<StudyPlanDto>> GetStudyPlansWithFilterAsync(StudyPlanFilterDto filter);
        Task<StudyPlanDto> UpdateStudyPlanAsync(int id, UpdateStudyPlanDto updateDto);
        Task<bool> DeleteStudyPlanAsync(int id);
        Task<bool> UserOwnsStudyPlanAsync(string userId, int planId);
        Task<IEnumerable<StudyPlanDto>> GetRandomStudyPlansAsync(int count);
    }
}