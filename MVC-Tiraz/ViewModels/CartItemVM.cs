namespace MVC_Tiraz.ViewModels
{
    public class CartItemVM
    {
        public int ProductColorId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string Size { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal LineTotal { get; set; }
    }
}