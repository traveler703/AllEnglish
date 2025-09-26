using AllEnBackend.Data;
using AllEnBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace AllEnBackend.Repository
{
    // 用户每日单词学习仓储实现
    public class UserDailyWordsRepository : IUserDailyWordsRepository
    {
        private readonly AppDbContext _context;

        public UserDailyWordsRepository(AppDbContext context)
        {
            _context = context;
        }

        // 根据用户ID和日期获取用户每日单词学习记录
        public async Task<UserDailyWords?> GetByUserIdAndDateAsync(string userId, DateTime studyDate)
        {
            return await _context.UserDailyWords
                .FirstOrDefaultAsync(x => x.UserId == userId && x.StudyDate.Date == studyDate.Date);
        }

        // 创建或更新用户每日单词学习记录
        public async Task<bool> CreateOrUpdateAsync(UserDailyWords record)
        {
            try
            {
                var existingRecord = await _context.UserDailyWords
                    .FirstOrDefaultAsync(x => x.UserId == record.UserId && x.StudyDate.Date == record.StudyDate.Date);

                if (existingRecord != null)
                {
                    // 更新现有记录
                    existingRecord.WordCount = record.WordCount;
                    _context.UserDailyWords.Update(existingRecord);
                }
                else
                {
                    // 创建新记录
                    await _context.UserDailyWords.AddAsync(record);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
