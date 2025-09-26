namespace AllEnBackend.Dtos
{
    public class PurchaseResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public decimal RemainingCoins { get; set; }
        public string OrderId { get; set; } = string.Empty;
    }
} 