using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVC_Tiraz.Models
{
    namespace TIRAZ.Models
    {
        public class Product
        {
            public int ProductId { get; set; }
            
            [Required(ErrorMessage = "Product name is required")]
            [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be 2 to 100 characters")]
            public string Name { get; set; } = string.Empty;
            public string Material { get; set; } = string.Empty;

            [Range(0.01, 1000000, ErrorMessage = "Price must be greater than 0")]
            public decimal Price { get; set; }

            [Range(0, 100000, ErrorMessage = "Stock cannot be negative")]
            public string Image { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;

            [NotMapped]
            public int TotalStock => Sizes.Sum(s => s.Colors.Sum(c => c.Quantity));

            [NotMapped]
            public int SalesCount { get; set; }
            public ICollection<ProductSize> Sizes { get; set; } = new List<ProductSize>();
            public bool Status { get; set; } = true;

            [ForeignKey(nameof(Category))]
            [Range(1, int.MaxValue, ErrorMessage = "Please select a category")]
            public int CategoryId { get; set; }
            public Category Category { get; set; } = null!;
            public ICollection<Review> Reviews { get; set; } = new List<Review>();
            public ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
            public ICollection<Has> HasCustomers { get; set; } = new List<Has>();
            public ICollection<Contains> Orders { get; set; } = new List<Contains>();
        }
    }
}
