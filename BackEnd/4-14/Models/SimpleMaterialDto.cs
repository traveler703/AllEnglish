namespace AllEnBackend.Models
{
    public class SimpleMaterialDto
    {
        public string Id { get; set; } = "";
        public string Title { get; set; } = "";
        public string SkillType { get; set; } = "";   
        public string ExamType { get; set; } = "";     
        public decimal Price { get; set; }
        public string PreviewUrl { get; set; } = "";
    }
}
