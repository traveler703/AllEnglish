using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AllEnBackend.Data;
using AllEnBackend.Dtos;
using AllEnBackend.Models;
using System.Linq;
using System.Threading.Tasks;

namespace AllEnBackend.Controllers
{
    [ApiController]
    [Route("api/listening-papers")]
    public class ListeningPapersController : ControllerBase
    {
        private readonly AppDbContext _db;
        public ListeningPapersController(AppDbContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> GetPapers(
            [FromQuery] string? level,
            [FromQuery] int? year,
            [FromQuery] string? q, // nullable
            [FromQuery] int page = 1,
            [FromQuery] int size = 8)
        {
            var query = _db.ListeningPapers.AsQueryable();
            if (!string.IsNullOrEmpty(level)) query = query.Where(p => p.Level == level);
            if (year.HasValue) query = query.Where(p => p.Year == year.Value);
            if (!string.IsNullOrEmpty(q)) query = query.Where(p => p.Session.Contains(q));

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(p => p.Year)
                .ThenBy(p => p.Session)
                .Skip((page - 1) * size)
                .Take(size)
                .Select(p => new ListeningPaperDto
                {
                    Id = p.Id,
                    Level = p.Level,
                    Year = p.Year,
                    Session = p.Session,
                    AudioUrl = p.AudioUrl,
                    SectionCount = p.Sections.Count
                })
                .ToListAsync();

            return Ok(new { items, total });
        }

        [HttpGet("{id:int}/sections")]
        public async Task<IActionResult> GetSections(int id)
        {
            var paper = await _db.ListeningPapers
                .Include(p => p.Sections)
                    .ThenInclude(s => s.Questions)
                        .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (paper == null) return NotFound();

            var sections = paper.Sections
                .OrderBy(s => s.Order)
                .Select(s => new SectionDto
                {
                    Id = s.Id,
                    Order = s.Order,
                    Questions = s.Questions
                        .OrderBy(q => q.Order)
                        .Select(q => new ListeningQuestionDto
                        {
                            Id = q.Id,
                            Order = q.Order,
                            Stem = q.Stem,
                            CorrectOption = q.CorrectOption,
                            Options = q.Options
                                .OrderBy(o => o.Label)
                                .Select(o => new OptionDto
                                {
                                    Label = o.Label,
                                    Content = o.Content
                                })
                                .ToList()
                        })
                        .ToList()
                })
                .ToList();

            return Ok(new
            {
                Id = paper.Id,
                Level = paper.Level,
                Year = paper.Year,
                Session = paper.Session,
                AudioUrl = paper.AudioUrl,
                Sections = sections
            });
        }
    }
}
