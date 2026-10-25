namespace MVC_Tiraz.ViewModels
{
    public class ProductFilterVM
    {
        public string Name { get; set; }
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public bool IsHot { get; set; }
        public string? Sort { get; set; }
        public bool LowQ { get; set; }
        public int Page { get; set; } = 1;
        public int CategoryId { get; set; }
    }
}
