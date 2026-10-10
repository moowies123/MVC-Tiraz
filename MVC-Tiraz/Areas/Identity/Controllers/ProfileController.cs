using Mapster;

namespace MVC_Tiraz.Areas.Identity.Controllers
{
    [Area("Identity")]
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public ProfileController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> UserInfo()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null) return NotFound();

            var applicationUserVM = user.Adapt<ApplicationUserVM>();

            return View(applicationUserVM);
        }

        [HttpGet]
        public async Task<IActionResult> UpdateProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            return View(user.Adapt<ApplicationUserVM>());   // or your manual mapping
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(ApplicationUserVM applicationUserVM)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null) return NotFound();
            
            user.UserName = applicationUserVM.UserName;
            user.Address = applicationUserVM.Address;
            user.PhoneNumber = applicationUserVM.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                var errors = string.Join(" ,", result.Errors.Select(e => e.Description));

                TempData["Error_Notification"] = errors;

                return RedirectToAction(nameof(UserInfo), applicationUserVM);
            }
            else
            {
                TempData["Successful_Notification"] = "profile updated Successfully";

                return RedirectToAction(nameof(UserInfo), applicationUserVM);
            }

        }

        [HttpGet]
        public IActionResult UpdatePassword() => View(new ApplicationUserVM());

        [HttpPost]
        public async Task<IActionResult> UpdatePassword(ApplicationUserVM applicationUserVM)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null) return NotFound();

            var result = await _userManager.ChangePasswordAsync(user, applicationUserVM.CurrentPassword, applicationUserVM.NewPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(" ,", result.Errors.Select(e => e.Description));

                TempData["Error_Notification"] = errors;
                
                return RedirectToAction(nameof(UserInfo));
            }
            else
            {
                TempData["Successful_Notification"] = "Password changed Successfully";
                
                return RedirectToAction(nameof(UserInfo));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }
    }
}