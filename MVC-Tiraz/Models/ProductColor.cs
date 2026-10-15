namespace MVC_Tiraz.Models.TIRAZ.Models;

public class ProductColor
{
    public int ProductColorId { get; set; }

    public int ProductSizeId { get; set; }
    public ProductSize ProductSize { get; set; } = null!;

    [Required(ErrorMessage = "Color is required")]
    [StringLength(30)]
    public string Color { get; set; } = string.Empty; 

    [Range(0, 100000, ErrorMessage = "Quantity cannot be negative")]
    public int Quantity { get; set; }   
}
