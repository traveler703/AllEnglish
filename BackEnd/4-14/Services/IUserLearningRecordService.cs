using AllEnBackend.Dtos;

namespace AllEnBackend.Services
{
    // �û�ѧϰ��¼����ӿ�
    public interface IUserLearningRecordService
    {
        // �����û�ID��ȡ�û�ѧϰ��¼
        Task<UserLearningRecordDto?> GetUserLearningRecordAsync(string userId);
        
        // �����û�ѧϰ��¼
        Task<bool> UpdateUserLearningRecordAsync(UpdateUserLearningRecordDto updateDto);
    }
}
