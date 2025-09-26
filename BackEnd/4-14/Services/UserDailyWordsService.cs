using AllEnBackend.Dtos;
using AllEnBackend.Models;
using AllEnBackend.Repository;

namespace AllEnBackend.Services
{
    // �û�ÿ�յ���ѧϰ����ʵ��
    public class UserDailyWordsService : IUserDailyWordsService
    {
        private readonly IUserDailyWordsRepository _userDailyWordsRepository;

        public UserDailyWordsService(IUserDailyWordsRepository userDailyWordsRepository)
        {
            _userDailyWordsRepository = userDailyWordsRepository;
        }

        // �����û�ID�����ڻ�ȡ�û�ÿ��ѧϰ�ĵ�������
        public async Task<UserDailyWordsResponseDto> GetUserDailyWordsAsync(string userId, DateTime studyDate)
        {
            // �����ݿ��ȡ��¼
            var record = await _userDailyWordsRepository.GetByUserIdAndDateAsync(userId, studyDate);

            // �����¼�����ڣ�����0
            if (record == null)
            {
                return new UserDailyWordsResponseDto
                {
                    UserId = userId,
                    StudyDate = studyDate,
                    WordCount = 0
                };
            }

            // �����ҵ��ļ�¼
            return new UserDailyWordsResponseDto
            {
                UserId = record.UserId,
                StudyDate = record.StudyDate,
                WordCount = record.WordCount
            };
        }
    }
}
