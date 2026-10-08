namespace MVC_Tiraz.ViewModels
{
    public class ForgetPasswordVM
    {
        public int Id { get; set; }

        [DataType(DataType.EmailAddress), EmailAddress]
        public string Email { get; set; }
    }
}
