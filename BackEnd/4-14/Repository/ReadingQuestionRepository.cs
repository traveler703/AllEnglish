using AllEnBackend.Data;
using AllEnBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace AllEnBackend.Repository
{
    public class ReadingQuestionRepository : IReadingQuestionRepository
    {
        private readonly AppDbContext _context;

        public ReadingQuestionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ReadingQuestion>> GetByArticleIdAsync(int articleId)
        {
            return await _context.Questions
                .Where(q => q.ArticleId == articleId)
                .OrderBy(q => q.Seqo)
                .ToListAsync();
        }
    }
}
