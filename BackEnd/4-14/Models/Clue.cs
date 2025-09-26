namespace AllEnBackend.Models
{
    public class Clue
    {
        public string ClueId { get; set; } = string.Empty;
        public string PuzzleId { get; set; } = string.Empty;         
        public string Direction { get; set; } = string.Empty;                // 移动方向
        public string ClueDescription { get; set; } = string.Empty;          // 单词描述
        public string Answer { get; set; } = string.Empty;                   // 单词内容
        public string BeginH { get; set; } = string.Empty;                   // 开始的行
        public string BeginL { get; set; } = string.Empty;                   // 开始的列
    }
}