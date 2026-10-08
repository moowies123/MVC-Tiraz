namespace MVC_Tiraz.ViewModels
{
    public class RegisterVM
    {
        [Required(ErrorMessage = "User Name is Required")]
        [MaxLength(15)]
        public required string UserName { get; set; }

        [Required(ErrorMessage = "First Name is Required")]
        [RegularExpression(@"^[A-Za-z\u0600-\u06FF ]+$", ErrorMessage = "Letters only")]
        [MaxLength(15)]
        public required string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is Required")]
        [RegularExpression(@"^[A-Za-z\u0600-\u06FF ]+$", ErrorMessage = "Letters only")]
        [MaxLength(15)]
        public required string LastName { get; set; }

        [Required(ErrorMessage = "Address is required")]
        public required string Address { get; set; }

        [DataType(DataType.EmailAddress), EmailAddress(ErrorMessage = "Enter a valid email")]
        public required string Email { get; set; }

        [RegularExpression(@"^01[0125]\d{8}$", ErrorMessage = "Enter a valid Egyptian mobile number")]
        [StringLength(11)]
        public required string Phone { get; set; }

        [StringLength(20,MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        [DataType(DataType.Password)]
        public required string Password { get; set; }

        [DataType(DataType.Password), Compare(nameof(Password), ErrorMessage = "Passwords don't match")]
        public required string ConfirmPassword { get; set; }
    }
}
