using System.Security.Claims;

namespace MVC_Tiraz.Areas.Identity.Controllers
{
    [Area("Identity")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly IRepository<ApplicationUserOtp> _applicationUserOtpRepository;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IEmailSender emailSender, IRepository<ApplicationUserOtp> applicationUserOtpRepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _applicationUserOtpRepository = applicationUserOtpRepository;
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var user = new ApplicationUser()
            {
                UserName = vm.UserName,
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                Email = vm.Email,
                Address = vm.Address,
                PhoneNumber = vm.Phone,
            };

            if (await _userManager.FindByEmailAsync(vm.Email) != null)
            {
                ModelState.AddModelError(nameof(vm.Email), "This email is already registered");
                return View(vm);
            }

            var result = await _userManager.CreateAsync(user, vm.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                    return View(vm);
                }
            }
            else TempData["Success"] = "Account created successfully!";

            await _userManager.AddToRoleAsync(user, CD.CUSTOMER_ROLE);

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var link = Url.Action("Login", "Account", new { area = "Identity", userId = user.Id, token }, Request.Scheme);

            // Email Confirmation Message
            var body = $$"""
            <div style="background-color:#f4f4f4; padding:30px 0; font-family:Arial, sans-serif;">
              <div style="max-width:500px; margin:auto; background-color:#ffffff; border-radius:8px; padding:30px;">
                <h1 style="text-align:center; color:#333333; margin:0 0 20px;">Tiraz</h1>
                <p style="font-size:16px; color:#555555; line-height:1.6;">
                  Hi {{user.FirstName}},<br><br>
                  Welcome to Tiraz! Please confirm your email address to activate your account.
                </p>
                <p style="text-align:center; margin:25px 0;">
                  <a href="{{link}}" style="background-color:#198754; color:#ffffff; text-decoration:none; padding:12px 28px; border-radius:5px; font-size:16px; display:inline-block;">Confirm Email</a>
                </p>
                <p style="font-size:13px; color:#888888; line-height:1.5;">
                  If the button doesn't work, copy and paste this link into your browser:<br>
                  <a href="{{link}}" style="color:#198754; word-break:break-all;">{{link}}</a><br><br>
                  If you didn't create an account, you can safely ignore this email.
                </p>
              </div>
            </div>
            """;
            // End of Message.

            await _emailSender.SendEmailAsync(
                vm.Email,
                "Tiraz email confirmation",
                body);

            return RedirectToAction(nameof(Register));
        }
        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string token, string userId)
        {
            if (userId is null || token is null) RedirectToAction(nameof(Register));

            var user = await _userManager.FindByIdAsync(userId);

            if (user is null) return RedirectToAction(nameof(Register));

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (!result.Succeeded)
            {
                TempData["Error"] = "An error happend while confirming your email try again please.";
                return RedirectToAction(nameof(Register));
            }
            else
            {
                TempData["Success"] = "Email confirmed successfully";
                return RedirectToAction("Login");
            }
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home", new { area = "" });

            return View("Register");
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM vm)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Enter your email and password.";
                return RedirectToAction("Login");
            }

            var user = await _userManager.FindByEmailAsync(vm.Email);

            if (user is null)
            {
                ModelState.AddModelError("", "invalid Email or password");
                return RedirectToAction("Login");
            }

            var result = await _signInManager.PasswordSignInAsync(user, vm.Password, vm.RememberMe, true);

            if (!result.Succeeded)
            {
                TempData["Error"] = result.IsLockedOut ? "Too many attempts try again later"
                    : result.IsNotAllowed ? "You're not allowed to login"
                    : "invalid Email or password";
                return RedirectToAction("Login");
            }

            TempData["Success"] = "Welcome!";

            return RedirectToAction("Index", "Home", new { area = "" });
        }

        [HttpGet]
        public IActionResult ForgetPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordVM vm)
        {
            var user = await _userManager.FindByEmailAsync(vm.Email);

            if (user is null)
            {
                ModelState.AddModelError(nameof(vm.Email), "Invalid Email");
                return View(vm);
            }



            var otps = await _applicationUserOtpRepository.GetAllAsync(o => o.ApplicationUserId == user.Id);

            var count = otps.Count(o => (DateTime.Now - o.CreatedDate).TotalHours <= 24);
            if (count > 5)
            {
                ModelState.AddModelError(nameof(vm.Email), "Too many attempts, Try again later");
                return View(vm);
            }

            var otp = new Random().Next(100000, 999999).ToString();

            var UserOtp = new ApplicationUserOtp()
            {
                Id = Guid.NewGuid().ToString(),
                ApplicationUserId = user.Id,
                OTP = otp
            };

            await _applicationUserOtpRepository.AddAsync(UserOtp);

            await _applicationUserOtpRepository.SaveChangesAsync();

            var body = $$"""
                <div style="background-color:#B0BA99; padding:30px 0; font-family:Arial, sans-serif;">
                  <div style="max-width:460px; margin:auto; background-color:#F7F1DE; border-radius:12px; padding:30px; text-align:center;">
                    <h1 style="color:#4E220F; margin:0 0 20px;">Tiraz</h1>
                    <p style="font-size:16px; color:#4E220F; line-height:1.6; margin:0 0 20px;">
                      Hi {{user.FirstName}}, use this code to reset your password:
                    </p>
                    <p style="font-size:34px; font-weight:bold; letter-spacing:10px; color:#F7F1DE; background-color:#9D6638; border-radius:8px; padding:14px 0; margin:0 0 20px;">
                      {{otp}}
                    </p>
                    <p style="font-size:13px; color:#9D6638; line-height:1.5; margin:0;">
                      This code expires in 10 minutes.<br>
                      If you didn't request it, you can safely ignore this email.
                    </p>
                  </div>
                </div>
                """;

            await _emailSender.SendEmailAsync(user.Email, "Your Tiraz verification code", body);

            TempData["Success"] = "OTP sent successfully!";

            return RedirectToAction(nameof(VerifyOtp), new { userId = user.Id });
        }

        [HttpGet]
        public IActionResult VerifyOtp(string userId)
        {
            return View(new VerifyOtpVM { UserId = userId });
        }
        [HttpPost]
        public async Task<IActionResult> VerifyOtp(VerifyOtpVM vm)
        {
            var user = await _userManager.FindByIdAsync(vm.UserId);

            if (user is null)
            {
                ModelState.AddModelError("", "Invalid User");
                return View(vm);
            }

            var otps = await _applicationUserOtpRepository.GetAllAsync(o =>
                o.ApplicationUserId == user.Id.ToString() &&
                o.IsValid == true &&
                o.ValidTo >= DateTime.Now
                );

            var applicationUserOtp = otps.OrderByDescending(o => o.CreatedDate).FirstOrDefault();

            if (applicationUserOtp == null || applicationUserOtp.OTP != vm.OTP)
            {
                ModelState.AddModelError("", " Invalid/Expired OTP");
                return View(vm);
            }
            applicationUserOtp.IsValid = false;

            await _applicationUserOtpRepository.SaveChangesAsync();

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            TempData["Success"] = "OTP verified successfully!";

            return RedirectToAction(nameof(ResetPassword), new { userId = user.Id, token });
        }

        [HttpGet]
        public async Task<IActionResult> ResetPassword(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);

            return View(new ResetPasswordVM { UserId = userId, Token = token, Email = user?.Email });
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordVM vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = await _userManager.FindByIdAsync(vm.UserId);

            if (user is null)
            {
                ModelState.AddModelError("", $"Invalid User");
                return View(vm);
            }

            var result = await _userManager.ResetPasswordAsync(user, vm.Token, vm.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(vm);
            }

            TempData["Successful_Notification"] = "Password reset successfully";

            return RedirectToAction("Login");
        }

        [HttpPost]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });

            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

            return Challenge(properties, provider);
        }

        [HttpGet]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                TempData["Error"] = $"Error from external provider: {remoteError}";
                return RedirectToAction(nameof(Login));
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();

            if (info == null)
            {
                TempData["Error"] = "Couldn't read your Google login info. Try again.";
                return RedirectToAction(nameof(Login));
            }

            var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);

            if (signInResult.Succeeded)
            {
                var linkedUser = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);

                if (linkedUser != null)
                    return await RedirectByRoleAsync(linkedUser);
            }

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var firstName = info.Principal.FindFirstValue(ClaimTypes.GivenName);
            var lastName = info.Principal.FindFirstValue(ClaimTypes.Surname);
            var address = info.Principal.FindFirstValue(ClaimTypes.StreetAddress);
            var username = info.Principal.FindFirstValue(ClaimTypes.Name);

            if (email != null)
            {
                TempData["Error"] = "Google didn't send an email address.";
                return RedirectToAction(nameof(Login));
            }

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                user = new ApplicationUser
                {
                    FirstName = firstName ?? username ?? "User",
                    LastName = lastName ?? "",
                    UserName = email,
                    Address = address ?? "",
                    Email = email,
                    EmailConfirmed = true
                };

                var createUserResult = await _userManager.CreateAsync(user);

                if (!createUserResult.Succeeded)
                {
                    TempData["Error"] = string.Join(" ", createUserResult.Errors.Select(e => e.Description));
                    return RedirectToAction(nameof(Login));
                }

                await _userManager.AddToRoleAsync(user, CD.CUSTOMER_ROLE);
            }

            var existingLogins = await _userManager.GetLoginsAsync(user);

            var hasGoogleLogin = existingLogins.Any(l => l.LoginProvider == info.LoginProvider);

            if (!hasGoogleLogin)
            {
                var addLoginResult = await _userManager.AddLoginAsync(user, info);

                if (!addLoginResult.Succeeded)
                {
                    TempData["Error"] = "Error linking your Google account.";
                    return RedirectToAction(nameof(Login));
                }
            }

            await _signInManager.SignInAsync(user, isPersistent: false);

            TempData["Success"] = "Welcome!";

            return await RedirectByRoleAsync(user);
        }

        private async Task<IActionResult> RedirectByRoleAsync(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            if (roles.Contains(CD.SUPER_ADMIN_ROLE) || roles.Contains(CD.ADMIN_ROLE) || roles.Contains(CD.EMPLOYEE_ROLE))
                return RedirectToAction("Index", "Home", new { area = "Admin" });

            return RedirectToAction("Index", "Home", new { area = "Customer" });
        }
    }
}
