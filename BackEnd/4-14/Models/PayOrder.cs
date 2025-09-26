namespace AllEnBackend.Models
{
    public class PayOrder
    {
        public string Id { get; set; } = string.Empty;              // ����ID
        public string Type { get; set; } = string.Empty;            // ��������
        public decimal Amount { get; set; } = 0.0m;                 // ֧�����
        public DateTime CreateTime { get; set; } = DateTime.Now;    // ����ʱ��
        public DateTime? PayTime { get; set; }                      // ֧��ʱ�䣨�ɿգ�
        public string PayMethod { get; set; } = string.Empty;       // ֧����ʽ
        public int IsPaid { get; set; } = 0;                        // �Ƿ���֧����0=δ֧����1=��֧��
    }
}
