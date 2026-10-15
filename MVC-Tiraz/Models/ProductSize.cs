namespace MVC_Tiraz.Models.TIRAZ.Models;

public class ProductSize
{
    public int ProductSizeId { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    [Required(ErrorMessage = "Size is required")]
    [StringLength(10)]
    public string Size { get; set; } = string.Empty; 

    public ICollection<ProductColor> Colors { get; set; } = new List<ProductColor>();
}
