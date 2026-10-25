namespace MVC_Tiraz.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = $"{CD.SUPER_ADMIN_ROLE},{CD.ADMIN_ROLE}")]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            if (users == null) return NotFound();

            var UserRole = new List<ApplicationUserVM>();

            foreach (var user in users)
            {
                var Roles = await _userManager.GetRolesAsync(user);

                UserRole.Add(new ApplicationUserVM
                {
                    User = user,
                    Role = Roles.FirstOrDefault() ?? "No role assigned"
                });
            }

            return View(UserRole);
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

        [HttpGet]
        public IActionResult CreateUser()
        {
            var model = new ApplicationUserVM();

            LoadRoles(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(ApplicationUserVM vm)
        {
            if (!ModelState.IsValid)
            {
                LoadRoles(vm);
                return View(vm);
            }

            var user = new ApplicationUser
            {
                UserName = vm.UserName,
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                Address = vm.Address,
                Email = vm.Email,
                EmailConfirmed = true
            };

            if (await _userManager.FindByEmailAsync(vm.Email) != null)
            {
                ModelState.AddModelError(nameof(vm.Email), "This email is already registered");
                return View(vm);
            }

            var result = await _userManager.CreateAsync(user, vm.NewPassword);
            if (!result.Succeeded)
            {
                foreach(var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                    return View(vm);
                }
            }

            TempData["Successful_Notification"] = "User created successfully!";

            await _userManager.AddToRoleAsync(user, vm.Role);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> UpdateUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var UserRole = new ApplicationUserVM();

            var Roles = await _userManager.GetRolesAsync(user);

            var model = new ApplicationUserVM
            {
                UserName = user.UserName,
                Email = user.Email,
                User = user,
                Role = Roles.FirstOrDefault() ?? "",
                Roles = _roleManager.Roles
            .Select(r => new SelectListItem
            {
                Value = r.Name!,
                Text = r.Name!
            })
            .ToList()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUser(string id, ApplicationUserVM vm)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            vm.User = user;

            LoadRoles(vm);

            var currentRoles = await _userManager.GetRolesAsync(user);

            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);

            if (!removeResult.Succeeded)
            {
                ModelState.AddModelError("", "Unable to remove existing role");
                return View(vm);
            }

            var addResult = await _userManager.AddToRoleAsync(user, vm.Role);

            if (!addResult.Succeeded)
            {
                ModelState.AddModelError("", "Unable to assign the selected role");
                return View(vm);
            }

            TempData["Successful_Notification"] = "User has been updated successfully";

            return RedirectToAction(nameof(Index));
        }
        private void LoadRoles(ApplicationUserVM vm)
        {
            vm.Roles = _roleManager.Roles
                .Select(r => new SelectListItem { Value = r.Name!, Text = r.Name! })
                .ToList();
        }
    }
}
