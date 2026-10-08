namespace MVC_Tiraz.ViewModels
{
    public class LoginVM
    {
        public int Id { get; set; }

        [DataType(DataType.EmailAddress), EmailAddress, Required]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }
        public bool RememberMe { get; set; }
    }
}
