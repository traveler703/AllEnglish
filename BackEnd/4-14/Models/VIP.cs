namespace AllEnBackend.Models
{
    public class VIP
    {
        public string UserId { get; set; } = string.Empty;             // �û�ID
        public DateTime StartTime { get; set; } = DateTime.Now;        // VIP ��ʼʱ��
        public DateTime ExpirationTime { get; set; }                   // VIP ����ʱ��
        public int IsActive { get; set; } = 0;                         // �Ƿ񼤻0=δ���1=�Ѽ���
    }
}
