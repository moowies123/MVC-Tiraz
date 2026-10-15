using System.ComponentModel.DataAnnotations;

namespace MVC_Tiraz.ViewModels;

public class ProductVM
{
    public int ProductId { get; set; }

    [Required(ErrorMessage = "Product name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be 2 to 100 characters")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Material is required")]
    [StringLength(50, ErrorMessage = "Material is too long")]
    public string Material { get; set; } = string.Empty;

    [Range(0.01, 1000000, ErrorMessage = "Price must be greater than 0")]
    public decimal Price { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Please select a category")]
    public int CategoryId { get; set; }

    public bool Status { get; set; } = true;

    public string? Description { get; set; }

    public string? Image { get; set; }

    public List<ProductSizeVM> Sizes { get; set; } = new();
}



