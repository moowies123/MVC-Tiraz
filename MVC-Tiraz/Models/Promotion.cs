namespace MVC_Tiraz.Models
{
    public class Promotion
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public DateTime ValidTo { get; set; }
        public bool IsValid { get; set; }
        public decimal Discount { get; set; }
        public int ProductId { get; set; }
        public int MaxUsage { get; set; }

        [ForeignKey(nameof(ProductId))]
        public Product Product { get; set; }
    }
}
