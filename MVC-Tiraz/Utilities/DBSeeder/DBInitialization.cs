
using TIRAZ.Data;

namespace MVC_Tiraz.Utilities.DBSeeder
{
    public class DBInitialization : IDBInitialization
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public DBInitialization(
            ApplicationDbContext context,
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task InitializeAsync()
        {
            try
            {
                if ((await _context.Database.GetPendingMigrationsAsync()).Any())
                {
                    await _context.Database.MigrateAsync();
                }

                // Create roles if they don't exist
                string[] roles =
                {
                    CD.SUPER_ADMIN_ROLE,
                    CD.ADMIN_ROLE,
                    CD.EMPLOYEE_ROLE,
                    CD.CUSTOMER_ROLE
                };

                foreach (var role in roles)
                {
                    if (!await _roleManager.RoleExistsAsync(role))
                    {
                        var roleResult = await _roleManager.CreateAsync(
                            new IdentityRole(role));

                        if (!roleResult.Succeeded)
                        {
                            throw new Exception(
                                string.Join(", ",
                                    roleResult.Errors.Select(e => e.Description)));
                        }
                    }
                }

                // Create Super Admin if it doesn't exist
                var user = await _userManager.FindByNameAsync("SuperAdmin123");

                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        FirstName = "Super",
                        LastName = "Admin",
                        UserName = "SuperAdmin123",
                        Address = "Cairo",
                        Email = "superAdmin123@test.com",
                        EmailConfirmed = true
                    };

                    var createResult = await _userManager.CreateAsync(
                        user, "sS@123");

                    if (!createResult.Succeeded)
                    {
                        throw new Exception(
                            string.Join(", ",
                                createResult.Errors.Select(e => e.Description)));
                    }
                }
                else if (!user.EmailConfirmed)
                {
                    user.EmailConfirmed = true;

                    var updateResult = await _userManager.UpdateAsync(user);

                    if (!updateResult.Succeeded)
                    {
                        throw new Exception(
                            string.Join(", ",
                                updateResult.Errors.Select(e => e.Description)));
                    }
                }

                // Ensure Super Admin has the correct role
                if (!await _userManager.IsInRoleAsync(
                    user, CD.SUPER_ADMIN_ROLE))
                {
                    var roleResult = await _userManager.AddToRoleAsync(
                        user, CD.SUPER_ADMIN_ROLE);

                    if (!roleResult.Succeeded)
                    {
                        throw new Exception(
                            string.Join(", ",
                                roleResult.Errors.Select(e => e.Description)));
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception instead of silently ignoring it
                Console.WriteLine(
                    $"Database initialization failed: {ex}");

                throw;
            }
        }
    }
}
