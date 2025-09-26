namespace AllEnBackend.Dtos
{
    // ������ϸ��Ϣ���ݴ������
    public class WordDetailDto
    {
        // ��������
        public string WordName { get; set; } = string.Empty;

        // Ӣ�������б�
        public List<string> EnglishDefinitions { get; set; } = new();

        // ���ķ����б�
        public List<string> ChineseTranslations { get; set; } = new();

        // �����б�
        public List<string> Phonetics { get; set; } = new();

        // �����б�
        public List<string> Examples { get; set; } = new();
    }

    // �û�������ϸ��Ϣ���ݴ������
    public class UserWordDetailDto
    {
        // ����ID
        public int WordId { get; set; }

        // ��������
        public string WordName { get; set; } = string.Empty;

        // �Ƿ���ѧϰ
        public bool HasLearned { get; set; }

        // ��ȷ�ش����
        public int CorrectCount { get; set; }

        // ѧϰ����
        public int LearnCount { get; set; }

        // �Ƿ����ղ�
        public bool HasBookmarked { get; set; }
    }

    // �û����ʲ�ѯ�������ݴ������
    public class UserWordQueryDto
    {
        // ѧϰ״̬ɸѡ (-1��ʾ�����Ǵ�����)
        public int HasLearned { get; set; } = -1;

        // �ղ�״̬ɸѡ (-1��ʾ�����Ǵ�����)
        public int HasBookmarked { get; set; } = -1;

        // ��С��ȷ����ɸѡ (-1��ʾ�����Ǵ�����)
        public int MinCorrectCount { get; set; } = -1;

        // �����ȷ����ɸѡ (-1��ʾ�����Ǵ�����)
        public int MaxCorrectCount { get; set; } = -1;

        // ��Сѧϰ����ɸѡ (-1��ʾ�����Ǵ�����)
        public int MinLearnCount { get; set; } = -1;

        // ���ѧϰ����ɸѡ (-1��ʾ�����Ǵ�����)
        public int MaxLearnCount { get; set; } = -1;

        // ���ٷ�Χɸѡ (��ʽ�� "cet4,cet6" �� "postgraduate"��"-1"��ʾ������)
        public string SyllabusScope { get; set; } = "-1";
    }

    // �û������б����ݴ������
    public class UserWordDto
    {
        // ����ID�б�
        public List<int> WordIds { get; set; } = new();

        // ���������ĵ�������
        public int TotalCount { get; set; }
    }
}