namespace MVC_Tiraz.ViewModels
{
    public class ForgetPasswordVM
    {
        public string Id { get; set; }

        [DataType(DataType.EmailAddress), EmailAddress]
        public string Email { get; set; }
    }
}
