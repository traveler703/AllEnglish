using AllEnBackend.Data;
using AllEnBackend.Dtos;
using AllEnBackend.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace AllEnBackend.Services
{
    public class StudyPlanService : IStudyPlanService
    {
        private readonly AppDbContext _context;

        public StudyPlanService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<StudyPlanDto> CreateStudyPlanAsync(CreateStudyPlanDto createDto)
        {
            var studyPlan = new StudyPlan
            {
                UserId = createDto.UserId,
                WordCount = createDto.WordCount,
                PlanType = createDto.PlanType,
                Title = createDto.Title,
                Duration = createDto.Duration,
                IsPublic = createDto.IsPublic,
                ArticleCount = createDto.ArticleCount,
                OralTime = createDto.OralTime,
                PlanListeningTime = createDto.PlanListeningTime
            };

            _context.StudyPlans.Add(studyPlan);
            await _context.SaveChangesAsync();

            return MapToDto(studyPlan);
        }

        public async Task<StudyPlanDto> GetStudyPlanByIdAsync(int id)
        {
            var studyPlan = await _context.StudyPlans.FindAsync(id);
            if (studyPlan == null) return null;

            return MapToDto(studyPlan);
        }

        public async Task<IEnumerable<StudyPlanDto>> GetStudyPlansWithFilterAsync(StudyPlanFilterDto filter)
        {
            var query = _context.StudyPlans.AsQueryable();

            // 构建动态查询条件
            if (filter.UserId != "-1") query = query.Where(sp => sp.UserId == filter.UserId);
            if (filter.WordCount != -1) query = query.Where(sp => sp.WordCount == filter.WordCount);
            if (filter.PlanType != "-1") query = query.Where(sp => sp.PlanType == filter.PlanType);
            if (filter.Duration != -1) query = query.Where(sp => sp.Duration == filter.Duration);
            if (filter.ArticleCount != -1) query = query.Where(sp => sp.ArticleCount == filter.ArticleCount);
            if (filter.OralTime != -1) query = query.Where(sp => sp.OralTime == filter.OralTime);
            if (filter.IsPublic.HasValue) query = query.Where(sp => sp.IsPublic == filter.IsPublic.Value);

            var studyPlans = await query.ToListAsync();
            return studyPlans.Select(MapToDto);
        }

        public async Task<StudyPlanDto> UpdateStudyPlanAsync(int id, UpdateStudyPlanDto updateDto)
        {
            var studyPlan = await _context.StudyPlans.FindAsync(id);
            if (studyPlan == null) return null;

            // 更新字段
            if (updateDto.UserId != null) studyPlan.UserId = updateDto.UserId;
            if (updateDto.WordCount.HasValue) studyPlan.WordCount = updateDto.WordCount.Value;
            if (updateDto.PlanType != null) studyPlan.PlanType = updateDto.PlanType;
            if (updateDto.Title != null) studyPlan.Title = updateDto.Title;
            if (updateDto.Duration.HasValue) studyPlan.Duration = updateDto.Duration.Value;
            if (updateDto.IsPublic.HasValue) studyPlan.IsPublic = updateDto.IsPublic.Value;
            if (updateDto.ArticleCount.HasValue) studyPlan.ArticleCount = updateDto.ArticleCount.Value;
            if (updateDto.OralTime.HasValue) studyPlan.OralTime = updateDto.OralTime.Value;
            if (updateDto.PlanListeningTime.HasValue) studyPlan.PlanListeningTime = updateDto.PlanListeningTime.Value;

            await _context.SaveChangesAsync();
            return MapToDto(studyPlan);
        }

        public async Task<bool> DeleteStudyPlanAsync(int id)
        {
            var studyPlan = await _context.StudyPlans.FindAsync(id);
            if (studyPlan == null) return false;

            _context.StudyPlans.Remove(studyPlan);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UserOwnsStudyPlanAsync(string userId, int planId)
        {
            return await _context.StudyPlans
                .AnyAsync(sp => sp.Id == planId && sp.UserId == userId);
        }

        private StudyPlanDto MapToDto(StudyPlan studyPlan)
        {
            return new StudyPlanDto
            {
                Id = studyPlan.Id,
                UserId = studyPlan.UserId,
                WordCount = studyPlan.WordCount,
                PlanType = studyPlan.PlanType,
                Title = studyPlan.Title,
                Duration = studyPlan.Duration,
                IsPublic = studyPlan.IsPublic,
                ArticleCount = studyPlan.ArticleCount,
                OralTime = studyPlan.OralTime,
                PlanListeningTime = studyPlan.PlanListeningTime
            };
        }

        public async Task<IEnumerable<StudyPlanDto>> GetRandomStudyPlansAsync(int count)
        {
            var studyPlans = await _context.StudyPlans
                .OrderBy(x => Guid.NewGuid())
                .Take(count)
                .ToListAsync();

            return studyPlans.Select(MapToDto);
        }
    }
}