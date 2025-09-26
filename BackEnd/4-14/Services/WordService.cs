using AllEnBackend.Dtos;          // ������� WordDetailDto ���ݴ������������ռ�

// WordService ʵ���� IWordService �ӿڣ��ṩ�뵥����ص�ҵ���߼�
public class WordService : IWordService
{
    private readonly IWordRepository _repo;  // ����ֻ���ֶ����ڷ��ʵײ����ݲִ�
    private readonly ILeaderboardRepository _leaderboardRepo;

    // ���캯��ע�� IWordRepository��ʵ������ע��
    public WordService(IWordRepository repo, ILeaderboardRepository leaderboardRepository)
    {
        _repo = repo;
        _leaderboardRepo = leaderboardRepository;
    }

    // ���ݵ��������첽��ȡ�������飨��������ͷ��룩
    public Task<WordDetailDto?> GetWordDetailByNameAsync(string wordName) =>
      _repo.GetWordDetailByNameAsync(wordName);

    // ���ݵ��� ID �첽��ȡ�������飨��������ͷ��룩
    public Task<WordDetailDto?> GetWordDetailByIdAsync(int wordId)
    {
        return _repo.GetWordDetailByIdAsync(wordId);
    }

    // �첽����ָ�����ʵ���Ӣ������
    public async Task<bool> UpdateWordAsync(string wordName, WordDetailDto newData)
    {
        // �Ȳ��ҵ�ǰ�Ƿ����иõ���
        var existing = await _repo.GetWordDetailByNameAsync(wordName);
        if (existing == null) return false;  // �������������ʧ��

        // ���µ��ʵ�Ӣ����������ķ���
        existing.EnglishDefinitions = newData.EnglishDefinitions;
        existing.ChineseTranslations = newData.ChineseTranslations;

        // �������
        await _repo.SaveAsync();
        return true;  // ���³ɹ�
    }
    public async Task<UserWordDto> GetUserWordsByUserIdAsync(string userId, UserWordQueryDto queryParams)
    {
        // ��֤���ٷ�Χ����
        if (queryParams.SyllabusScope != "-1")
        {
            var validScopes = new[] { "cet4", "cet6", "postgraduate" };
            var inputScopes = queryParams.SyllabusScope.Split(',')
                .Select(s => s.Trim().ToLower())
                .ToList();

            if (inputScopes.Any(s => !validScopes.Contains(s)))
            {
                throw new ArgumentException("��Ч�Ŀ��ٷ�Χ��ֻ֧�� cet4, cet6, postgraduate �������");
            }
        }

        return await _repo.GetUserWordsByUserIdAsync(userId, queryParams);
    }

    public async Task<bool> AddOrUpdateUserWordAsync(string userId, int wordId, int hasLearned, int correctCount, int learnCount, int hasBookmarked)
    {
        return await _repo.AddOrUpdateUserWordAsync(userId, wordId, hasLearned, correctCount, learnCount, hasBookmarked);
    }

    public async Task<bool> UpdateLearningStatsAsync(string userId, int wordId, bool isCorrect)
    {
        await _repo.UpdateLearningStatsAsync(userId, wordId, isCorrect);
        await _leaderboardRepo.UpdateWordLearning(userId);
        return true;
    }

    public async Task<bool> ToggleBookmarkAsync(string userId, int wordId)
    {
        return await _repo.ToggleBookmarkAsync(userId, wordId);
    }

    public async Task<bool> RemoveUserWordAsync(string userId, int wordId)
    {
        return await _repo.RemoveUserWordAsync(userId, wordId);
    }

    public async Task<UserWordDetailDto?> GetUserWordDetailAsync(string userId, int wordId)
    {
        return await _repo.GetUserWordDetailAsync(userId, wordId);
    }
}
