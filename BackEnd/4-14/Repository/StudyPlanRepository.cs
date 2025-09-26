using AllEnBackend.Data;
using AllEnBackend.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AllEnBackend.Repositories
{
    public class StudyPlanRepository : IStudyPlanRepository
    {
        private readonly AppDbContext _context;

        public StudyPlanRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<StudyPlan> GetByIdAsync(int id)
        {
            return await _context.StudyPlans.FindAsync(id);
        }

        public async Task<IEnumerable<StudyPlan>> GetAllAsync()
        {
            return await _context.StudyPlans.ToListAsync();
        }

        public async Task<IEnumerable<StudyPlan>> GetByFilterAsync(
            string userId,
            int wordCount,
            string planType,
            int duration,
            int articleCount,
            int oralTime,
            bool? isPublic)
        {
            var query = _context.StudyPlans.AsQueryable();

            if (userId != "-1") query = query.Where(sp => sp.UserId == userId);
            if (wordCount != -1) query = query.Where(sp => sp.WordCount == wordCount);
            if (planType != "-1") query = query.Where(sp => sp.PlanType == planType);
            if (duration != -1) query = query.Where(sp => sp.Duration == duration);
            if (articleCount != -1) query = query.Where(sp => sp.ArticleCount == articleCount);
            if (oralTime != -1) query = query.Where(sp => sp.OralTime == oralTime);
            if (isPublic.HasValue) query = query.Where(sp => sp.IsPublic == isPublic.Value);

            return await query.ToListAsync();
        }

        public async Task AddAsync(StudyPlan studyPlan)
        {
            await _context.StudyPlans.AddAsync(studyPlan);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(StudyPlan studyPlan)
        {
            _context.StudyPlans.Update(studyPlan);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var studyPlan = await _context.StudyPlans.FindAsync(id);
            if (studyPlan != null)
            {
                _context.StudyPlans.Remove(studyPlan);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.StudyPlans.AnyAsync(e => e.Id == id);
        }

        public async Task<IEnumerable<StudyPlan>> GetRandomStudyPlansAsync(int count)
        {
            var total = await _context.StudyPlans.CountAsync();
            var skip = new Random().Next(0, Math.Max(0, total - count));

            return await _context.StudyPlans
                .OrderBy(x => Guid.NewGuid()) // Random ordering
                .Take(count)
                .ToListAsync();
        }
    }
}