using Microsoft.AspNetCore.Mvc.Rendering;

namespace MVC_Tiraz.ViewModels
{
    public class ApplicationUserVM
    {
        [MaxLength(15)]
        public string UserName { get; set; }

        [MinLength(3 , ErrorMessage = "First Name must be at least 3 chars long")]
        public string FirstName { get; set; }

        [MinLength(3 , ErrorMessage = "Last Name must be at least 3 chars long")]
        public string LastName { get; set; }

        [DataType(DataType.EmailAddress), EmailAddress(ErrorMessage = "Enter a valid email")]
        public string? Email { get; set; }

        [RegularExpression(@"^01[0125]\d{8}$", ErrorMessage = "Enter a valid Egyptian mobile number")]
        [StringLength(11)]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Email field is required")]
        public string Address { get; set; }

        [DataType(DataType.Password)]
        public string? CurrentPassword { get; set; }

        [StringLength(20, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [StringLength(20, MinimumLength = 6, ErrorMessage = "Passwords don't match")]
        [DataType(DataType.Password),Compare(nameof(NewPassword))]
        public string ConfirmPassword { get; set; }
        public ApplicationUser? User { get; set; }
        public string? Role { get; set; }
        public IEnumerable<SelectListItem> Roles { get; set; } = new List<SelectListItem>();
    }
}