using AllEnBackend.Data;
using AllEnBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace AllEnBackend.Repository
{
    // 用户学习记录仓储实现
    public class UserLearningRecordRepository : IUserLearningRecordRepository
    {
        private readonly AppDbContext _context;

        public UserLearningRecordRepository(AppDbContext context)
        {
            _context = context;
        }

        // 根据用户ID获取用户学习记录
        public async Task<UserLearningRecord?> GetByUserIdAsync(string userId)
        {
            try
            {
                return await _context.UserLearningRecords
                    .Include(ulr => ulr.User)
                    .FirstOrDefaultAsync(ulr => ulr.UserId == userId);
            }
            catch (Exception)
            {
                return null;
            }
        }

        // 更新用户学习记录
        public async Task<bool> UpdateAsync(UserLearningRecord record)
        {
            try
            {
                var existingRecord = await _context.UserLearningRecords
                    .FirstOrDefaultAsync(ulr => ulr.UserId == record.UserId);

                if (existingRecord == null)
                {
                    // 如果记录不存在，创建新记录
                    _context.UserLearningRecords.Add(record);
                }
                else
                {
                    // 更新现有记录
                    existingRecord.ArticleCount = record.ArticleCount;
                    existingRecord.WordCount = record.WordCount;
                    existingRecord.OralTime = record.OralTime;
                    existingRecord.ListeningTime = record.ListeningTime;
                    existingRecord.ArticlePerDay = record.ArticlePerDay;
                    existingRecord.WordPerDay = record.WordPerDay;
                    existingRecord.OralPerDay = record.OralPerDay;
                    existingRecord.ListeningPerDay = record.ListeningPerDay;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
