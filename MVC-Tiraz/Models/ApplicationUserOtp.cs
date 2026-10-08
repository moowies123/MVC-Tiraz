namespace MVC_Tiraz.Models
{
    public class ApplicationUserOtp
    {
        public string Id { get; set; }
        public string OTP { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime ValidTo { get; set; } = DateTime.Now.AddMinutes(5);
        public bool IsValid { get; set; }
        public ApplicationUser applicationUser { get; set; }

        [ForeignKey(nameof(applicationUser))]
        public string ApplicationUserId { get; set; }
    
        public ApplicationUserOtp() { }
        public ApplicationUserOtp(string userId, string otp)
        {
            Id = Guid.NewGuid().ToString();
            OTP = otp;
            CreatedDate = DateTime.Now;
            ValidTo = DateTime.Now.AddMinutes(5);
            IsValid = true;
            ApplicationUserId = userId;
        }
    }
}
