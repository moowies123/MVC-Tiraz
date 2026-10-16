using Microsoft.AspNetCore.Mvc;

namespace MVC_Tiraz.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = $"{CD.SUPER_ADMIN_ROLE},{CD.ADMIN_ROLE}")]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            if (users == null) return NotFound();
            
            return View(users);
        }

        public async Task<IActionResult> LockUnLock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            bool isSuper = await _userManager.IsInRoleAsync(user, CD.SUPER_ADMIN_ROLE);

            if (isSuper)
            {
                TempData["Error_Notification"] = "Can't lock a Super Admin!"; 
                return RedirectToAction(nameof(Index));
            }

            if (user.LockoutEnd == null || user.LockoutEnd < DateTime.UtcNow)
            {
                await _userManager.SetLockoutEndDateAsync(user, DateTime.UtcNow.AddMinutes(10));
                TempData["Successful_Notification"] = "User Locked";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                await _userManager.SetLockoutEndDateAsync(user, null);
                TempData["Successful_Notification"] = "User UnLocked";
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
