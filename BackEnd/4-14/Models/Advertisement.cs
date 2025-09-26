namespace AllEnBackend.Models
{
    public class Advertisement
    {
        public string Id { get; set; } = string.Empty; 
        public string MediaUrl { get; set; } = string.Empty;         // Ԥ��ͼ��Դ��ַ
        public string TargetId { get; set; } = string.Empty;        // �����ת��ַ
        public string Context { get; set; } = string.Empty;          // ������ֲ���
        public int Status { get; set; } = 1;                         // ״̬��0=ͣ�� 1=����
        public int ClickCount { get; set; } = 0;                    // �ۼƵ������
        public DateTime CreateTime { get; set; } = DateTime.Now;    // ����ʱ��
    }
}