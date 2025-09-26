namespace AllEnBackend.Models
{
    public class Achievement
    {
        public int Id { get; set; }                           // �ɾ�ID
        public string Title { get; set; } = string.Empty;     // �ɾͱ���
        public string Description { get; set; } = string.Empty; // �ɾ�����
        public int CoinCount { get; set; } = 0;               // ������
        public int ArticleCount { get; set; } = 0;            // �����Ķ���������
        public int WordCount { get; set; } = 0;               // ����ѧϰ��������
        public int OralTime { get; set; } = 0;                // �������ѵ����ʱ��
        public int ListeningTime { get; set; } = 0;           // ��������ѵ����ʱ��
        public int ArticlePerday { get; set; } = 0;           // ���������Ķ���������
        public int WordPerday { get; set; } = 0;              // ��������ѧϰ��������
        public int OralPerday { get; set; } = 0;              // �����������ѵ��ʱ��
        public int ListeningPerday { get; set; } = 0;         // ������������ѵ��ʱ��
    }
} 