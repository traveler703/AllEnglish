using AllEnBackend.Dtos;

namespace AllEnBackend.Services
{
    // �û�ÿ�յ���ѧϰ����ӿ�
    public interface IUserDailyWordsService
    {
        // �����û�ID�����ڻ�ȡ�û�ÿ��ѧϰ�ĵ�������
        Task<UserDailyWordsResponseDto> GetUserDailyWordsAsync(string userId, DateTime studyDate);
    }
}
