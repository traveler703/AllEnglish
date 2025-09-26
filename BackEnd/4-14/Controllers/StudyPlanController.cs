using AllEnBackend.Data;
using AllEnBackend.Dtos;
using AllEnBackend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AllEnBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudyPlanController : ControllerBase
    {
        private readonly AppDbContext _context;

        public StudyPlanController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/StudyPlan
        [HttpPost]
        public async Task<ActionResult<StudyPlanDto>> CreateStudyPlan(CreateStudyPlanDto createDto)
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
                OralTime = createDto.OralTime
            };

            _context.StudyPlans.Add(studyPlan);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetStudyPlan), new { id = studyPlan.Id }, MapToDto(studyPlan));
        }

        // PUT: api/StudyPlan/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStudyPlan(int id, UpdateStudyPlanDto updateDto)
        {
            var studyPlan = await _context.StudyPlans.FindAsync(id);
            if (studyPlan == null)
            {
                return NotFound();
            }

            // 更新字段
            if (updateDto.UserId != null) studyPlan.UserId = updateDto.UserId;
            if (updateDto.WordCount.HasValue) studyPlan.WordCount = updateDto.WordCount.Value;
            if (updateDto.PlanType != null) studyPlan.PlanType = updateDto.PlanType;
            if (updateDto.Title != null) studyPlan.Title = updateDto.Title;
            if (updateDto.Duration.HasValue) studyPlan.Duration = updateDto.Duration.Value;
            if (updateDto.IsPublic.HasValue) studyPlan.IsPublic = updateDto.IsPublic.Value;
            if (updateDto.ArticleCount.HasValue) studyPlan.ArticleCount = updateDto.ArticleCount.Value;
            if (updateDto.OralTime.HasValue) studyPlan.OralTime = updateDto.OralTime.Value;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StudyPlanExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/StudyPlan/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStudyPlan(int id)
        {
            var studyPlan = await _context.StudyPlans.FindAsync(id);
            if (studyPlan == null)
            {
                return NotFound();
            }

            _context.StudyPlans.Remove(studyPlan);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/StudyPlan/5
        [HttpGet("{id}")]
        public async Task<ActionResult<StudyPlanDto>> GetStudyPlan(int id)
        {
            var studyPlan = await _context.StudyPlans.FindAsync(id);

            if (studyPlan == null)
            {
                return NotFound();
            }

            return MapToDto(studyPlan);
        }

        // GET: api/StudyPlan/Filter
        [HttpGet("Filter")]
        public async Task<ActionResult<IEnumerable<StudyPlanDto>>> GetStudyPlansWithFilter(
            [FromQuery] StudyPlanFilterDto filter)
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
            return studyPlans.Select(MapToDto).ToList();
        }

        private bool StudyPlanExists(int id)
        {
            return _context.StudyPlans.Any(e => e.Id == id);
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
                OralTime = studyPlan.OralTime
            };
        }

        [HttpGet("Random/{count}")]
        public async Task<ActionResult<IEnumerable<StudyPlanDto>>> GetRandomStudyPlans(int count)
        {
            if (count <= 0)
            {
                return BadRequest("Count must be greater than 0");
            }

            var studyPlans = await _context.StudyPlans
                .OrderBy(x => Guid.NewGuid())
                .Take(count)
                .ToListAsync();

            return studyPlans.Select(MapToDto).ToList();
        }
    }
}