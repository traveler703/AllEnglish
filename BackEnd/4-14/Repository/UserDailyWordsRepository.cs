using AllEnBackend.Data;
using AllEnBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace AllEnBackend.Repository
{
    // �û�ÿ�յ���ѧϰ�ִ�ʵ��
    public class UserDailyWordsRepository : IUserDailyWordsRepository
    {
        private readonly AppDbContext _context;

        public UserDailyWordsRepository(AppDbContext context)
        {
            _context = context;
        }

        // �����û�ID�����ڻ�ȡ�û�ÿ�յ���ѧϰ��¼
        public async Task<UserDailyWords?> GetByUserIdAndDateAsync(string userId, DateTime studyDate)
        {
            return await _context.UserDailyWords
                .FirstOrDefaultAsync(x => x.UserId == userId && x.StudyDate.Date == studyDate.Date);
        }

        // ����������û�ÿ�յ���ѧϰ��¼
        public async Task<bool> CreateOrUpdateAsync(UserDailyWords record)
        {
            try
            {
                var existingRecord = await _context.UserDailyWords
                    .FirstOrDefaultAsync(x => x.UserId == record.UserId && x.StudyDate.Date == record.StudyDate.Date);

                if (existingRecord != null)
                {
                    // �������м�¼
                    existingRecord.WordCount = record.WordCount;
                    _context.UserDailyWords.Update(existingRecord);
                }
                else
                {
                    // �����¼�¼
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
