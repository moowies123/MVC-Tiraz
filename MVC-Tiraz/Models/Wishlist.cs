namespace MVC_Tiraz.Models
{
    namespace TIRAZ.Models
    {
        public class Wishlist
        {
            public string UserId { get; set; } = string.Empty;
            public int ProductId { get; set; }
            public ApplicationUser User { get; set; } = null!;
            public Product Product { get; set; } = null!;
        }
    }
}
