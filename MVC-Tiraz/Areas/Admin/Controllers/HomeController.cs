namespace MVC_Tiraz.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = $"{CD.SUPER_ADMIN_ROLE},{CD.ADMIN_ROLE},{CD.EMPLOYEE_ROLE}")]
    public class HomeController : Controller
    {
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<Contains> _containsRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(
            IRepository<Order> orderRepository,
            IRepository<Product> productRepository,
            UserManager<ApplicationUser> userManager,
            IRepository<Contains> containsRepository)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _userManager = userManager;
            _containsRepository = containsRepository;
        }

        public async Task<IActionResult> DashBoard()
        {
            var orders = await _orderRepository.GetAllAsync();
            var products = await _productRepository.GetAllAsync();
            var contains = await _containsRepository.GetAllAsync();

            var soldPerProduct = contains
                .GroupBy(c => c.ProductId)
                .ToDictionary(g => g.Key, g => g.Sum(c => c.Quantity));

            foreach (var p in products)
                p.SalesCount = soldPerProduct.GetValueOrDefault(p.ProductId);

            var vm = new DashboardVM
            {
                TotalSales = products.Sum(p => p.SalesCount * p.Price),
                TotalSold = products.Sum(p => p.SalesCount),
                TotalVisitors = _userManager.Users.Count(),
                Product = products.OrderByDescending(p => p.SalesCount).ThenByDescending(p => p.ProductId).ToList()
            };

            var roles = new[] { CD.SUPER_ADMIN_ROLE, CD.ADMIN_ROLE, CD.EMPLOYEE_ROLE, CD.CUSTOMER_ROLE };
            var usersByRole = new Dictionary<string, int>();

            foreach (var role in roles)
            {
                var inRole = await _userManager.GetUsersInRoleAsync(role);
                usersByRole[role] = inRole.Count;
            }

            var withoutRole = vm.TotalVisitors - usersByRole.Values.Sum();
            if (withoutRole > 0) usersByRole["No role"] = withoutRole;

            vm.UsersByRole = usersByRole;

            return View(vm);
        }

        [AllowAnonymous]
        public IActionResult NotFoundPage()
        {
            Response.StatusCode = 404;
            return View();
        }
    }
}
