namespace AllEnBackend.Models
{
    public class Clue
    {
        public string ClueId { get; set; } = string.Empty;
        public string PuzzleId { get; set; } = string.Empty;         
        public string Direction { get; set; } = string.Empty;                // �ƶ�����
        public string ClueDescription { get; set; } = string.Empty;          // ��������
        public string Answer { get; set; } = string.Empty;                   // ��������
        public string BeginH { get; set; } = string.Empty;                   // ��ʼ����
        public string BeginL { get; set; } = string.Empty;                   // ��ʼ����
    }
}