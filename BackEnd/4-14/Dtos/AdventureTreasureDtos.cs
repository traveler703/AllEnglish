using System.ComponentModel.DataAnnotations;

namespace AllEnBackend.Dtos
{
    public class AdventureTreasureDto
    {
        public long Id { get; set; }
        [Required]
        public int LevelNumber { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; }
        public string Description { get; set; }
        public string Rewards { get; set; }
        public string Icon { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateAdventureTreasureDto
    {
        public int LevelNumber { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Rewards { get; set; }
        public string Icon { get; set; }
        public bool IsActive { get; set; } = true;
    }
}