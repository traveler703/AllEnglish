namespace AllEnBackend.Models
{
    public class HomeCardsResponse
    {
        public List<SimpleMaterialDto> Hot { get; set; } = new();     // 4 门（四大技能各 1 门）
        public List<SimpleMaterialDto> ForYou { get; set; } = new();  // 为你推荐（3 门）
    }
}
