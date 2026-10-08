namespace MVC_Tiraz.Models
{
    namespace TIRAZ.Models
    {
        public class ApplicationUser : IdentityUser
        {
            public required string FirstName { get; set; }
            public required string LastName { get; set; }
            public required string Address { get; set; }
            public Cart? Cart { get; set; }
            public ICollection<Order> Orders { get; set; } = new List<Order>();
            public ICollection<Has> HasProducts { get; set; } = new List<Has>();
            public ICollection<Review> Reviews { get; set; } = new List<Review>();
            public ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
        }
    }
}
