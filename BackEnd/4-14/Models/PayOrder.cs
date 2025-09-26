namespace AllEnBackend.Models
{
    public class PayOrder
    {
        public string Id { get; set; } = string.Empty;              // 订单ID
        public string Type { get; set; } = string.Empty;            // 订单类型
        public decimal Amount { get; set; } = 0.0m;                 // 支付金额
        public DateTime CreateTime { get; set; } = DateTime.Now;    // 创建时间
        public DateTime? PayTime { get; set; }                      // 支付时间（可空）
        public string PayMethod { get; set; } = string.Empty;       // 支付方式
        public int IsPaid { get; set; } = 0;                        // 是否已支付：0=未支付，1=已支付
    }
}
