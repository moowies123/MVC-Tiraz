namespace MVC_Tiraz.Models
{
    namespace TIRAZ.Models
    {
        public class Cart
        {
            public int CartId { get; set; }
            public string UserId { get; set; } = string.Empty;
            public ApplicationUser User { get; set; } = null!;
            public ICollection<Product> Products { get; set; } = new List<Product>();
        }
    }
}
