namespace MVC_Tiraz.Models
{
    namespace TIRAZ.Models
    {
        public class Category
        {
            public int CategoryId { get; set; }

            [StringLength(50, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 50 characters")]
            public string Name { get; set; } = string.Empty;
            public bool Status { get; set; } = true;
            public ICollection<Product> Products { get; set; } = new List<Product>();
        }
    }
}
