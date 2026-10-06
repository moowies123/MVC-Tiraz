namespace MVC_Tiraz.Models
{
    namespace TIRAZ.Models
    {
        public class Review
        {
            public int ReviewId { get; set; }
            public string UserId { get; set; } = string.Empty;
            public int ProductId { get; set; }
            public int Rate { get; set; }
            public string Comment { get; set; } = string.Empty;
            public ApplicationUser User { get; set; } = null!;

            public Product Product { get; set; } = null!;
        }
    }
}
