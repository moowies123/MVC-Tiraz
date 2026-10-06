using MVC_Tiraz.Models.Enums;

namespace MVC_Tiraz.Models
{
    namespace TIRAZ.Models
    {
        public class Payment
        {
            public int PaymentId { get; set; }

            public PaymentMethod Method { get; set; }

            public decimal Amount { get; set; }

            public DateTime Date { get; set; }

            public string Status { get; set; } = string.Empty;

            public int OrderId { get; set; }
            public Order? Order { get; set; }
        }
    }
}
