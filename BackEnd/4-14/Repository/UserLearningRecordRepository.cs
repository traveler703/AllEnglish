using AllEnBackend.Data;
using AllEnBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace AllEnBackend.Repository
{
    // �û�ѧϰ��¼�ִ�ʵ��
    public class UserLearningRecordRepository : IUserLearningRecordRepository
    {
        private readonly AppDbContext _context;

        public UserLearningRecordRepository(AppDbContext context)
        {
            _context = context;
        }

        // �����û�ID��ȡ�û�ѧϰ��¼
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

        // �����û�ѧϰ��¼
        public async Task<bool> UpdateAsync(UserLearningRecord record)
        {
            try
            {
                var existingRecord = await _context.UserLearningRecords
                    .FirstOrDefaultAsync(ulr => ulr.UserId == record.UserId);

                if (existingRecord == null)
                {
                    // �����¼�����ڣ������¼�¼
                    _context.UserLearningRecords.Add(record);
                }
                else
                {
                    // �������м�¼
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
