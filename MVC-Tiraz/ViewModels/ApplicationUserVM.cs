namespace MVC_Tiraz.ViewModels
{
    public class ApplicationUserVM
    {
        //[Required(ErrorMessage = "User Name is Required")]
        [MaxLength(15)]
        public string UserName { get; set; }

        [DataType(DataType.EmailAddress), EmailAddress(ErrorMessage = "Enter a valid email")]
        public string? Email { get; set; }

        [RegularExpression(@"^01[0125]\d{8}$", ErrorMessage = "Enter a valid Egyptian mobile number")]
        [StringLength(11)]
        public string? PhoneNumber { get; set; }

        //[Required(ErrorMessage = "Address is required")]
        public string Address { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }

        [Required]
        [StringLength(20, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }
    }
}