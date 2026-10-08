using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVC_Tiraz.Models
{
    namespace TIRAZ.Models
    {
        public class Product
        {
            public int ProductId { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Color { get; set; } = string.Empty;
            public string Material { get; set; } = string.Empty;
            public decimal Price { get; set; }
            public string Image { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public int StockQuantity { get; set; }
            public string Size { get; set; } = string.Empty;

            [ForeignKey(nameof(Category))]
            public int CategoryId { get; set; }
            public Category Category { get; set; } = null!;
            public ICollection<Review> Reviews { get; set; } = new List<Review>();
            public ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
            public ICollection<Has> HasCustomers { get; set; } = new List<Has>();
            public ICollection<Contains> Orders { get; set; } = new List<Contains>();
        }
    }
}
