using AllEnBackend.Models;

namespace AllEnBackend.Repository
{
    // �û�ѧϰ��¼�ִ��ӿ�
    public interface IUserLearningRecordRepository
    {
        // �����û�ID��ȡ�û�ѧϰ��¼
        Task<UserLearningRecord?> GetByUserIdAsync(string userId);
        
        // �����û�ѧϰ��¼
        Task<bool> UpdateAsync(UserLearningRecord record);
    }
}
