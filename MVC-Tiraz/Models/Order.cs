using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVC_Tiraz.Models
{
    namespace TIRAZ.Models
    {
        public class Order
        {
            public int OrderId { get; set; }
            public DateTime Date { get; set; }
            public string UserId { get; set; } = string.Empty;
            [ForeignKey(nameof(UserId))]
            public ApplicationUser User { get; set; } = null!;
            public Payment? Payment { get; set; } = null!;
            public ICollection<Contains> Products { get; set; } = new List<Contains>();
        }
    }
}
