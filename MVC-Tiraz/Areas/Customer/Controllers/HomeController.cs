namespace MVC_Tiraz.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<ProductSize> _productSizeRepository;

        public HomeController(IRepository<Product> productRepository, IRepository<Category> categoryRepository, IRepository<ProductSize> productSizeRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _productSizeRepository = productSizeRepository;
        }

        public async Task<IActionResult> Index(ProductFilterVM vm)
        {
            var products = await _productRepository.GetAllIncludingAsync(null, includePaths: ["Category", "Sizes.Colors", "Images"]);

            if (vm.Name != null)
            {
                products = products.Where(p => p.Name.Contains(vm.Name));
                ViewBag.Name = vm.Name;
            }
            if (vm.MinPrice > 0)
            {
                products = products.Where(p => p.Price - p.Price * (p.Discount ?? 0) / 100 >= vm.MinPrice);
                ViewBag.MinPrice = vm.MinPrice;
            }

            if (vm.MaxPrice > 0)
            {
                products = products.Where(p => p.Price - p.Price * (p.Discount ?? 0) / 100 <= vm.MaxPrice);
                ViewBag.MaxPrice = vm.MaxPrice;
            }
            if (vm.CategoryId > 0)
            {
                products = products.Where(p => p.CategoryId == vm.CategoryId);
                ViewBag.CategoryId = vm.CategoryId;

            }
            if (vm.IsHot)
            {
                products = products.Where(p => p.Discount > 0).OrderByDescending(p => p.Discount);
                ViewBag.IsHot = vm.IsHot;
            }
            if (vm.Sort == "low")
                products = products.OrderBy(p => p.Price - p.Price * (p.Discount ?? 0) / 100);
            else if (vm.Sort == "high")
                products = products.OrderByDescending(p => p.Price - p.Price * (p.Discount ?? 0) / 100);

            ViewBag.Sort = vm.Sort;

            ViewBag.Categories = await _categoryRepository.GetAllAsync();

            ViewBag.TotalPages = (int)Math.Ceiling(products.Count() / 8.0);
            ViewBag.CurrentPage = vm.Page;
            products = products.Skip((vm.Page - 1) * 8).Take(8);

            return View(products.AsEnumerable());
        }

        public async Task<IActionResult> ProductDetails(int id)
        {
            var product = (await _productRepository.GetAllIncludingAsync(
                p => p.ProductId == id, includePaths: ["Category", "Sizes.Colors", "Images"])).FirstOrDefault();

            if (product == null)
                return RedirectToAction("NotFoundPage", "Home", new { area = "" });

            return View(product);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
