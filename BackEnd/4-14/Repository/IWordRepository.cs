using AllEnBackend.Dtos;
using AllEnBackend.Models;
using static AllEnBackend.Dtos.WordDetailDto;

// �������ݷ��ʽӿڣ������˶Ե������ݵĲ����淶
public interface IWordRepository
{
    // ���ݵ������ƻ�ȡ������ϸ��Ϣ
    Task<WordDetailDto?> GetWordDetailByNameAsync(string wordName);

    // ���ݵ���ID��ȡ������ϸ��Ϣ
    Task<WordDetailDto?> GetWordDetailByIdAsync(int wordId);

    // ��������ݿ�ĸ���
    Task SaveAsync();

    // ��ȡָ���û��ĵ����б�
    Task<UserWordDto> GetUserWordsByUserIdAsync(string userId, UserWordQueryDto queryParams);

    // ��ӻ�����û����ʼ�¼
    Task<bool> AddOrUpdateUserWordAsync(string userId, int wordId, int hasLearned, int correctCount, int learnCount, int hasBookmarked);

    // �����û�����ѧϰͳ����Ϣ
    Task<bool> UpdateLearningStatsAsync(string userId, int wordId, bool isCorrect);

    // �л������ղ�״̬
    Task<bool> ToggleBookmarkAsync(string userId, int wordId);

    // �Ƴ��û����ʼ�¼
    Task<bool> RemoveUserWordAsync(string userId, int wordId);

    // ��ȡ�û����ʵ���ϸ��Ϣ
    Task<UserWordDetailDto?> GetUserWordDetailAsync(string userId, int wordId);
}