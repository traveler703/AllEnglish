namespace AllEnBackend.Dtos
{
    // �û�ѧϰ��¼��ӦDTO
    public class UserLearningRecordDto
    {
        public string UserId { get; set; } = string.Empty;    // �û�ID
        public int ArticleCount { get; set; }    // ���Ķ������µ�����
        public int WordCount { get; set; }    // ��ѧϰ�ĵ��ʵ�����
        public int OralTime { get; set; }    // ��ѵ���Ŀ������ʱ�������ӣ�
        public int ListeningTime { get; set; }    // ��ѵ������������ʱ�������ӣ�
        public int ArticlePerDay { get; set; }    // �������Ķ������µ�����
        public int WordPerDay { get; set; }    // ������ѧϰ�ĵ��ʵ�����
        public int OralPerDay { get; set; }    // ������ѵ���Ŀ����ʱ�������ӣ�
        public int ListeningPerDay { get; set; }    // ������ѵ����������ʱ�������ӣ�
    }

    // ��ȡ�û�ѧϰ��¼����DTO
    public class GetUserLearningRecordRequestDto
    {
        public string UserId { get; set; } = string.Empty;    // �û�ID
    }

    // �����û�ѧϰ��¼����DTO
    public class UpdateUserLearningRecordDto
    {
        public string UserId { get; set; } = string.Empty;    // �û�ID
        public int ArticleCount { get; set; }    // ���Ķ������µ�����
        public int WordCount { get; set; }    // ��ѧϰ�ĵ��ʵ�����
        public int OralTime { get; set; }    // ��ѵ���Ŀ������ʱ�������ӣ�
        public int ListeningTime { get; set; }    // ��ѵ������������ʱ�������ӣ�
        public int ArticlePerDay { get; set; }    // �������Ķ������µ�����
        public int WordPerDay { get; set; }    // ������ѧϰ�ĵ��ʵ�����
        public int OralPerDay { get; set; }    // ������ѵ���Ŀ����ʱ�������ӣ�
        public int ListeningPerDay { get; set; }    // ������ѵ����������ʱ�������ӣ�
    }
}
