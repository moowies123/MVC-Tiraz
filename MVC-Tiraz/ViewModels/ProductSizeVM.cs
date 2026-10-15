namespace MVC_Tiraz.ViewModels
{
    public class ProductSizeVM
    {
        public string? Size { get; set; }
        public List<ProductColorVM> Colors { get; set; } = new();
    }
}
