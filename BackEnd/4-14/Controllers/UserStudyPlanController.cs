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
    public class UserStudyPlanController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserStudyPlanController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/UserStudyPlan
        [HttpPost]
        public async Task<ActionResult<UserStudyPlanDto>> CreateUserStudyPlan(CreateUserStudyPlanDto createDto)
        {
            // 检查用户是否存在
            var userExists = await _context.Users.CountAsync(u => u.Id == createDto.UserId) > 0;
            if (!userExists)
            {
                return NotFound($"User with ID {createDto.UserId} not found");
            }

            // 检查学习计划是否存在
            var planExists = await _context.StudyPlans.CountAsync(p => p.Id == createDto.PlanId) > 0;
            if (!planExists)
            {
                return NotFound($"StudyPlan with ID {createDto.PlanId} not found");
            }

            // 检查是否已存在相同的用户学习计划
            var exists = await _context.UserStudyPlans
                .CountAsync(usp => usp.UserId == createDto.UserId && usp.PlanId == createDto.PlanId) > 0;
            if (exists)
            {
                return Conflict("This user already has this study plan");
            }

            var userStudyPlan = new UserStudyPlan
            {
                UserId = createDto.UserId,
                PlanId = createDto.PlanId,
                StartDate = createDto.StartDate,
                LearnedWordCount = 0,
                LearnedArticleCount = 0,
                ListeningTime = 0,
                LearnedOralTime = 0,
                Completed = 0
            };

            _context.UserStudyPlans.Add(userStudyPlan);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUserStudyPlan), new { userId = userStudyPlan.UserId, planId = userStudyPlan.PlanId },
                new UserStudyPlanDto
                {
                    UserId = userStudyPlan.UserId,
                    PlanId = userStudyPlan.PlanId,
                    StartDate = userStudyPlan.StartDate,
                    LearnedWordCount = userStudyPlan.LearnedWordCount,
                    LearnedArticleCount = userStudyPlan.LearnedArticleCount,
                    ListeningTime = userStudyPlan.ListeningTime,
                    LearnedOralTime = userStudyPlan.LearnedOralTime,
                    Completed = userStudyPlan.Completed
                });
        }


        // PUT: api/UserStudyPlan/{userId}/{planId}
        [HttpPut("{userId}/{planId}")]
        public async Task<IActionResult> UpdateUserStudyPlan(string userId, int planId, UpdateUserStudyPlanDto updateDto)
        {

            if (userId != updateDto.UserId || planId != updateDto.PlanId)
            {
                return BadRequest("User ID or Plan ID mismatch");
            }

            var userStudyPlan = await _context.UserStudyPlans
                .FirstOrDefaultAsync(usp => usp.UserId == userId && usp.PlanId == planId);

            if (userStudyPlan == null)
            {
                return NotFound();
            }

            // 更新可修改字段
            if (updateDto.LearnedWordCount.HasValue)
            {
                userStudyPlan.LearnedWordCount = updateDto.LearnedWordCount.Value;
            }
            if (updateDto.LearnedArticleCount.HasValue)
            {
                userStudyPlan.LearnedArticleCount = updateDto.LearnedArticleCount.Value;
            }
            if (updateDto.ListeningTime.HasValue)
            {
                userStudyPlan.ListeningTime = updateDto.ListeningTime.Value;
            }
            if (updateDto.LearnedOralTime.HasValue)
            {
                userStudyPlan.LearnedOralTime = updateDto.LearnedOralTime.Value;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserStudyPlanExists(userId, planId))
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

        // DELETE: api/UserStudyPlan/{userId}/{planId}
        [HttpDelete("{userId}/{planId}")]
        public async Task<IActionResult> DeleteUserStudyPlan(string userId, int planId)
        {
            var userStudyPlan = await _context.UserStudyPlans
                .FirstOrDefaultAsync(usp => usp.UserId == userId && usp.PlanId == planId);

            if (userStudyPlan == null)
            {
                return NotFound();
            }

            _context.UserStudyPlans.Remove(userStudyPlan);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/UserStudyPlan/{userId}/{planId}
        [HttpGet("{userId}/{planId}")]
        public async Task<ActionResult<UserStudyPlanDto>> GetUserStudyPlan(string userId, int planId)
        {
            var userStudyPlan = await _context.UserStudyPlans
                .FirstOrDefaultAsync(usp => usp.UserId == userId && usp.PlanId == planId);

            if (userStudyPlan == null)
            {
                return NotFound();
            }

            return new UserStudyPlanDto
            {
                UserId = userStudyPlan.UserId,
                PlanId = userStudyPlan.PlanId,
                StartDate = userStudyPlan.StartDate,
                LearnedWordCount = userStudyPlan.LearnedWordCount,
                LearnedArticleCount = userStudyPlan.LearnedArticleCount,
                ListeningTime = userStudyPlan.ListeningTime,
                LearnedOralTime = userStudyPlan.LearnedOralTime,
                Completed = userStudyPlan.Completed
            };
        }

        // GET: api/UserStudyPlan/ByUser/{userId}
        [HttpGet("ByUser/{userId}")]
        public async Task<ActionResult<IEnumerable<UserStudyPlanDto>>> GetUserStudyPlansByUser(string userId)
        {
            var userStudyPlans = await _context.UserStudyPlans
                .Where(usp => usp.UserId == userId)
                .ToListAsync();

            return userStudyPlans.Select(usp => new UserStudyPlanDto
            {
                UserId = usp.UserId,
                PlanId = usp.PlanId,
                StartDate = usp.StartDate,
                LearnedWordCount = usp.LearnedWordCount,
                LearnedArticleCount = usp.LearnedArticleCount,
                ListeningTime = usp.ListeningTime,
                LearnedOralTime = usp.LearnedOralTime,
                Completed = usp.Completed
            }).ToList();
        }

        private bool UserStudyPlanExists(string userId, int planId)
        {
            return _context.UserStudyPlans.Any(e => e.UserId == userId && e.PlanId == planId);
        }

        // GET: api/UserStudyPlan/DetailsByUser/{userId}
        [HttpGet("DetailsByUser/{userId}")]
        public async Task<ActionResult<IEnumerable<UserStudyPlanDetailDto>>> GetUserStudyPlanDetailsByUser(string userId)
        {
            // 检查用户是否存在
            var userExists = await _context.Users.CountAsync(u => u.Id == userId) > 0;
            if (!userExists)
            {
                return NotFound($"User with ID {userId} not found");
            }

            var userStudyPlanDetails = await _context.UserStudyPlans
                .Where(usp => usp.UserId == userId)
                .Join(
                    _context.StudyPlans,
                    usp => usp.PlanId,
                    sp => sp.Id,
                    (usp, sp) => new UserStudyPlanDetailDto
                    {
                        UserId = usp.UserId,
                        PlanId = usp.PlanId,
                        StartDate = usp.StartDate,
                        LearnedWordCount = usp.LearnedWordCount,
                        LearnedArticleCount = usp.LearnedArticleCount,
                        ListeningTime = usp.ListeningTime,
                        LearnedOralTime = usp.LearnedOralTime,
                        Completed = usp.Completed,

                        // 学习计划详情
                        PlanTitle = sp.Title,
                        PlanType = sp.PlanType,
                        WordCount = sp.WordCount,
                        Duration = sp.Duration,
                        IsPublic = sp.IsPublic,
                        ArticleCount = sp.ArticleCount,
                        OralTime = sp.OralTime,
                        PlanListeningTime = sp.PlanListeningTime,
                    })
                .ToListAsync();

            return userStudyPlanDetails;
        }
    }
}