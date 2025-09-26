using System;

namespace AllEnBackend.Dtos
{
    public class InventoryItemDto
    {
        public string Id { get; set; } = string.Empty;
        public string MaterialId { get; set; } = string.Empty;
        public string MaterialType { get; set; } = string.Empty;
        public string ExamType { get; set; } = string.Empty;
        public string SkillType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string PreviewUrl { get; set; } = string.Empty;
        public DateTime PurchaseDate { get; set; }
        public decimal PurchasePrice { get; set; }
        public string OrderId { get; set; } = string.Empty;
    }
} 