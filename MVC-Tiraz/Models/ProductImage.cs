namespace MVC_Tiraz.Models
{
    public class ProductImage
    {
        public int ProductImageId { get; set; }
        public string Url { get; set; } = string.Empty;

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
    }
}
