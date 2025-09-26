using System;

namespace AllEnBackend.Models
{
    public class UserInventory
    {
        public string Id { get; set; } = string.Empty;              // ID
        public string UserId { get; set; } = string.Empty;          // USER_ID
        public string MaterialId { get; set; } = string.Empty;      // MATERIAL_ID
        public DateTime PurchaseDate { get; set; } = DateTime.Now;  // PURCHASE_DATE
        public decimal PurchasePrice { get; set; } = 0.0m;          // PURCHASE_PRICE
        public string OrderId { get; set; } = string.Empty;         // ORDER_ID
        public int IsActive { get; set; } = 1;                      // IS_ACTIVE
    }
} 