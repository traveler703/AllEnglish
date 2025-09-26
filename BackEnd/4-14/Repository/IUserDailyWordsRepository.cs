using AllEnBackend.Models;

namespace AllEnBackend.Repository
{
    // �û�ÿ�յ���ѧϰ�ִ��ӿ�
    public interface IUserDailyWordsRepository
    {
        // �����û�ID�����ڻ�ȡ�û�ÿ�յ���ѧϰ��¼
        Task<UserDailyWords?> GetByUserIdAndDateAsync(string userId, DateTime studyDate);
        
        // ����������û�ÿ�յ���ѧϰ��¼
        Task<bool> CreateOrUpdateAsync(UserDailyWords record);
    }
}
