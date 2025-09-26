using AllEnBackend.Dtos;
using AllEnBackend.Models;
using AllEnBackend.Repository;

namespace AllEnBackend.Services
{
    // �û�ѧϰ��¼����ʵ��
    public class UserLearningRecordService : IUserLearningRecordService
    {
        private readonly IUserLearningRecordRepository _repository;

        public UserLearningRecordService(IUserLearningRecordRepository repository)
        {
            _repository = repository;
        }

        // �����û�ID��ȡ�û�ѧϰ��¼
        public async Task<UserLearningRecordDto?> GetUserLearningRecordAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return null;
            }

            try
            {
                var record = await _repository.GetByUserIdAsync(userId);

                if (record == null)
                {
                    return null;
                }

                return new UserLearningRecordDto
                {
                    UserId = record.UserId,
                    ArticleCount = record.ArticleCount,
                    WordCount = record.WordCount,
                    OralTime = record.OralTime,
                    ListeningTime = record.ListeningTime,
                    ArticlePerDay = record.ArticlePerDay,
                    WordPerDay = record.WordPerDay,
                    OralPerDay = record.OralPerDay,
                    ListeningPerDay = record.ListeningPerDay
                };
            }
            catch (Exception)
            {
                return null;
            }
        }

        // �����û�ѧϰ��¼
        public async Task<bool> UpdateUserLearningRecordAsync(UpdateUserLearningRecordDto updateDto)
        {
            if (updateDto == null || string.IsNullOrWhiteSpace(updateDto.UserId))
            {
                return false;
            }

            try
            {
                var record = new UserLearningRecord
                {
                    UserId = updateDto.UserId,
                    ArticleCount = updateDto.ArticleCount,
                    WordCount = updateDto.WordCount,
                    OralTime = updateDto.OralTime,
                    ListeningTime = updateDto.ListeningTime,
                    ArticlePerDay = updateDto.ArticlePerDay,
                    WordPerDay = updateDto.WordPerDay,
                    OralPerDay = updateDto.OralPerDay,
                    ListeningPerDay = updateDto.ListeningPerDay
                };

                return await _repository.UpdateAsync(record);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
