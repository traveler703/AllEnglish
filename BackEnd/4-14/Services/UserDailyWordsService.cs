using AllEnBackend.Dtos;
using AllEnBackend.Models;
using AllEnBackend.Repository;

namespace AllEnBackend.Services
{
    // 用户每日单词学习服务实现
    public class UserDailyWordsService : IUserDailyWordsService
    {
        private readonly IUserDailyWordsRepository _userDailyWordsRepository;

        public UserDailyWordsService(IUserDailyWordsRepository userDailyWordsRepository)
        {
            _userDailyWordsRepository = userDailyWordsRepository;
        }

        // 根据用户ID和日期获取用户每日学习的单词数量
        public async Task<UserDailyWordsResponseDto> GetUserDailyWordsAsync(string userId, DateTime studyDate)
        {
            // 从数据库获取记录
            var record = await _userDailyWordsRepository.GetByUserIdAndDateAsync(userId, studyDate);

            // 如果记录不存在，返回0
            if (record == null)
            {
                return new UserDailyWordsResponseDto
                {
                    UserId = userId,
                    StudyDate = studyDate,
                    WordCount = 0
                };
            }

            // 返回找到的记录
            return new UserDailyWordsResponseDto
            {
                UserId = record.UserId,
                StudyDate = record.StudyDate,
                WordCount = record.WordCount
            };
        }
    }
}
