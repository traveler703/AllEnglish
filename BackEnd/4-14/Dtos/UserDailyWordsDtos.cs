namespace AllEnBackend.Dtos
{
    // ��ȡ�û�ÿ�յ���ѧϰ��������DTO
    public class GetUserDailyWordsRequestDto
    {
        public string UserId { get; set; } = string.Empty;    // �û�ID
        public DateTime StudyDate { get; set; }    // ѧϰ����
    }

    // �û�ÿ�յ���ѧϰ������ӦDTO
    public class UserDailyWordsResponseDto
    {
        public string UserId { get; set; } = string.Empty;    // �û�ID
        public DateTime StudyDate { get; set; }    // ѧϰ����
        public int WordCount { get; set; }    // ѧϰ�ĵ�������
    }
}
