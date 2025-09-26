namespace AllEnBackend.Models
{
    public class MaterialUploadRequest
    {
        public string MaterialType { get; set; } = string.Empty;  // 视频/文章
        public string ExamType { get; set; } = string.Empty;      // 考试类型
        public string SkillType { get; set; } = string.Empty;     // 技能类型
        public decimal Price { get; set; } = 0.0m;                // 价格
        public string Description { get; set; } = string.Empty;   // 描述
        public string fileUrl { get; set; } = string.Empty;       // 文件URL
        public bool IsActive { get; set; } = false;               // 激活状态
        public string PreviewUrl { get; set; } = string.Empty;   // 新增预览图URL字段
    }
}

