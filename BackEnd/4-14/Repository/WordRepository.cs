using Microsoft.EntityFrameworkCore;
using AllEnBackend.Models;
using AllEnBackend.Data;
using System;
using System.Threading.Tasks;
using AllEnBackend.Dtos;
using static AllEnBackend.Dtos.WordDetailDto;

namespace AllEnBackend.Repositories
{
    public class WordRepository : IWordRepository
    {
        private readonly AppDbContext _context;

        public WordRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<WordDetailDto?> GetWordDetailByNameAsync(string wordName)
        {
            var vocab = await _context.Vocabularies
                .Include(v => v.Definitions)
                .Include(v => v.Translations)
                .Include(v => v.Phonetics)
                .Include(v => v.Examples)
                .FirstOrDefaultAsync(v => v.WordName == wordName);

            if (vocab == null) return null;

            return new WordDetailDto
            {
                WordName = vocab.WordName,
                EnglishDefinitions = vocab.Definitions.Select(d => d.EnglishDefinition).ToList(),
                ChineseTranslations = vocab.Translations.Select(t => t.ChineseTranslation).ToList(),
                Phonetics = vocab.Phonetics.Select(p => p.PhoneticText).ToList(),
                Examples = vocab.Examples.Select(e => e.ExampleText).ToList()
            };
        }

        public async Task<WordDetailDto?> GetWordDetailByIdAsync(int wordId)
        {
            var vocab = await _context.Vocabularies
                .Include(v => v.Definitions)
                .Include(v => v.Translations)
                .Include(v => v.Phonetics)
                .Include(v => v.Examples)
                .FirstOrDefaultAsync(v => v.Id == wordId);

            if (vocab == null) return null;

            return new WordDetailDto
            {
                WordName = vocab.WordName,
                EnglishDefinitions = vocab.Definitions.Select(d => d.EnglishDefinition).ToList(),
                ChineseTranslations = vocab.Translations.Select(t => t.ChineseTranslation).ToList(),
                Phonetics = vocab.Phonetics.Select(p => p.PhoneticText).ToList(),
                Examples = vocab.Examples.Select(e => e.ExampleText).ToList()
            };
        }

        public Task SaveAsync() =>
            _context.SaveChangesAsync();
        public async Task<UserWordDto> GetUserWordsByUserIdAsync(string userId, UserWordQueryDto queryParams)
        {
            // 基础查询：用户ID匹配
            var query = _context.UserWords
                .Where(uw => uw.UserId == userId)
                .AsQueryable();

            // 根据条件筛选
            if (queryParams.HasLearned != -1)
            {
                query = query.Where(uw => uw.HasLearned == queryParams.HasLearned);
            }

            if (queryParams.HasBookmarked != -1)
            {
                query = query.Where(uw => uw.HasBookmarked == queryParams.HasBookmarked);
            }

            if (queryParams.MinCorrectCount != -1)
            {
                query = query.Where(uw => uw.CorrectCount >= queryParams.MinCorrectCount);
            }

            if (queryParams.MaxCorrectCount != -1)
            {
                query = query.Where(uw => uw.CorrectCount <= queryParams.MaxCorrectCount);
            }

            if (queryParams.MinLearnCount != -1)
            {
                query = query.Where(uw => uw.LearnCount >= queryParams.MinLearnCount);
            }

            if (queryParams.MaxLearnCount != -1)
            {
                query = query.Where(uw => uw.LearnCount <= queryParams.MaxLearnCount);
            }

            // 新增：考纲范围筛选
            if (queryParams.SyllabusScope != "-1")
            {
                var syllabusScopes = queryParams.SyllabusScope.Split(',')
                    .Select(s => s.Trim().ToLower())
                    .ToList();

                query = query.Join(
                    _context.Vocabularies,
                    uw => uw.WordId,
                    v => v.Id,
                    (uw, v) => new { UserWord = uw, Vocabulary = v }
                )
                .Where(x => syllabusScopes.Contains(x.Vocabulary.Coverage.ToLower()))
                .Select(x => x.UserWord);
            }

            var wordIds = await query
                .Select(uw => uw.WordId)
                .ToListAsync();

            return new UserWordDto
            {
                WordIds = wordIds,
                TotalCount = wordIds.Count
            };
        }

        public async Task<bool> AddOrUpdateUserWordAsync(string userId, int wordId, int hasLearned, int correctCount, int learnCount, int hasBookmarked)
        {
            try
            {
                var userWord = await _context.UserWords
                    .FirstOrDefaultAsync(uw => uw.UserId == userId && uw.WordId == wordId);

                if (userWord == null)
                {
                    userWord = new UserWord
                    {
                        UserId = userId,
                        WordId = wordId,
                        HasLearned = hasLearned,
                        CorrectCount = correctCount,
                        LearnCount = learnCount,
                        HasBookmarked = hasBookmarked
                    };
                    _context.UserWords.Add(userWord);
                }
                else
                {
                    userWord.HasLearned = hasLearned;
                    userWord.CorrectCount = correctCount;
                    userWord.LearnCount = learnCount;
                    userWord.HasBookmarked = hasBookmarked;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch 
            {
                throw;
                // return false;
            }
        }

        public async Task<bool> UpdateLearningStatsAsync(string userId, int wordId, bool isCorrect)
        {
            var userWord = await _context.UserWords
                .FirstOrDefaultAsync(uw => uw.UserId == userId && uw.WordId == wordId);

            if (userWord == null)
            {
                userWord = new UserWord
                {
                    UserId = userId,
                    WordId = wordId,
                    HasLearned = 1,
                    CorrectCount = isCorrect ? 1 : 0,
                    LearnCount = 1,
                    HasBookmarked = 0
                };
                _context.UserWords.Add(userWord);
            }
            else
            {
                userWord.LearnCount++;
                if (isCorrect) userWord.CorrectCount++;
                userWord.HasLearned = 1;
            }

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ToggleBookmarkAsync(string userId, int wordId)
        {
            var userWord = await _context.UserWords
                .FirstOrDefaultAsync(uw => uw.UserId == userId && uw.WordId == wordId);

            if (userWord == null)
            {
                userWord = new UserWord
                {
                    UserId = userId,
                    WordId = wordId,
                    HasLearned = 0,
                    CorrectCount = 0,
                    LearnCount = 0,
                    HasBookmarked = 1
                };
                _context.UserWords.Add(userWord);
            }
            else
            {
                userWord.HasBookmarked = userWord.HasBookmarked == 1 ? 0 : 1;
            }

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RemoveUserWordAsync(string userId, int wordId)
        {
            var userWord = await _context.UserWords
                .FirstOrDefaultAsync(uw => uw.UserId == userId && uw.WordId == wordId);

            if (userWord == null) return false;

            _context.UserWords.Remove(userWord);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<UserWordDetailDto?> GetUserWordDetailAsync(string userId, int wordId)
        {
            var userWord = await _context.UserWords
                .Include(uw => uw.Vocabulary)
                .FirstOrDefaultAsync(uw => uw.UserId == userId && uw.WordId == wordId);

            if (userWord == null)
                return null;

            return new UserWordDetailDto
            {
                WordId = userWord.WordId,
                WordName = userWord.Vocabulary?.WordName ?? string.Empty,
                HasLearned = userWord.HasLearned == 1,
                CorrectCount = userWord.CorrectCount,
                LearnCount = userWord.LearnCount,
                HasBookmarked = userWord.HasBookmarked == 1
            };
        }
    }
}
