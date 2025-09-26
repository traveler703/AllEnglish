namespace AllEnBackend.Models
{
    public class Material
    {
        public string Id { get; set; } = string.Empty;                // 材料ID
        public string MaterialType { get; set; } = string.Empty;      // 材料类型
        public string ExamType { get; set; } = string.Empty;          // 考试类型
        public string SkillType { get; set; } = string.Empty;         // 技能类型
        public decimal Price { get; set; } = 0.0m;                    // 价格
        public DateTime UpdateDate { get; set; } = DateTime.Now;      // 更新时间
        public int IsActive { get; set; } = 0;                         // 是否激活：0=未激活，1=已激活
        public string? Description { get; set; } = string.Empty;      // 描述
        public string Url { get; set; } = string.Empty;               // URL
        public string PreviewUrl { get; set; } = string.Empty;        // 预览图URL
    }
}
