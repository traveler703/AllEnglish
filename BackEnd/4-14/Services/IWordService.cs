using AllEnBackend.Dtos;
using AllEnBackend.Models;
using static AllEnBackend.Dtos.WordDetailDto;

public interface IWordService
{
    // �����ƻ�ȡ��������
    Task<WordDetailDto?> GetWordDetailByNameAsync(string wordName);
    // ����ID��ȡ��������
    Task<WordDetailDto?> GetWordDetailByIdAsync(int wordId);
    // ���µ�����Ϣ
    Task<bool> UpdateWordAsync(string wordName, WordDetailDto newData);
    // ��user_id��ѯѧ���ĵ��ʵ�id
    Task<UserWordDto> GetUserWordsByUserIdAsync(string userId, UserWordQueryDto queryParams);
    // ��ӻ�����û�����ѧϰ��¼
    Task<bool> AddOrUpdateUserWordAsync(string userId, int wordId, int hasLearned, int correctCount, int learnCount, int hasBookmarked);
    // �����û�����ѧϰͳ����Ϣ
    Task<bool> UpdateLearningStatsAsync(string userId, int wordId, bool isCorrect);
    // �л������ղ�״̬
    Task<bool> ToggleBookmarkAsync(string userId, int wordId);
    // �Ƴ��û��ĵ���ѧϰ��¼
    Task<bool> RemoveUserWordAsync(string userId, int wordId);
    // ��ȡ�û��ض����ʵ���ϸѧϰ��Ϣ
    Task<UserWordDetailDto?> GetUserWordDetailAsync(string userId, int wordId);
}
