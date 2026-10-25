namespace MVC_Tiraz.Models
{
    [PrimaryKey(nameof(ProductId), nameof(UserId), nameof(ProductColorId))]
    public class Cart
    {
        public string UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; } = null!;

        public int ProductId { get; set; }

        [ForeignKey(nameof(ProductId))]
        public Product Product { get; set; } = null!;

        public int ProductColorId { get; set; }

        [ForeignKey(nameof(ProductColorId))]
        public ProductColor ProductColor { get; set; } = null!;

        public int Count { get; set; }
        public decimal TotalPrice { get; set; }

        public string? PromotionCode { get; set; }
        public decimal? PromotionDiscount { get; set; }
    }
}