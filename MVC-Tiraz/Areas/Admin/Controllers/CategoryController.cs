namespace MVC_Tiraz.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = $"{CD.SUPER_ADMIN_ROLE},{CD.ADMIN_ROLE},{CD.EMPLOYEE_ROLE}")]
    public class CategoryController : Controller
    {
        private readonly IRepository<Category> _categoryRepository;
        public CategoryController(IRepository<Category> categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var categories = await _categoryRepository.GetAllAsync();

            return View(categories);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error_Notification"] = "Invalid Data";
                return View(category);
            }

            await _categoryRepository.AddAsync(category);

            await _categoryRepository.SaveChangesAsync();

            TempData["Successful_Notification"] = "Category Created Successfully";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize(Roles = $"{CD.SUPER_ADMIN_ROLE},{CD.ADMIN_ROLE}")]
        public async Task<IActionResult> Update(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);

            if (category == null)
            {
                return RedirectToAction("NotFoundPage", "Home",
                    new { area = "" });
            }

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{CD.SUPER_ADMIN_ROLE},{CD.ADMIN_ROLE}")]
        public async Task<IActionResult> Update(Category category)
        {
            if (!ModelState.IsValid)
            {
                return View(category);
            }

            var existingCategory = await _categoryRepository.GetByIdAsync(category.CategoryId);

            if (existingCategory == null)
            {
                return RedirectToAction("NotFoundPage", "Home", new { area = "" });
            }

            existingCategory.Name = category.Name;
            existingCategory.Status = category.Status;

            await _categoryRepository.SaveChangesAsync();

            TempData["Successful_Notification"] = "Category Updated Successfully";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{CD.SUPER_ADMIN_ROLE},{CD.ADMIN_ROLE}")]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);

            if (category == null)
            {
                return RedirectToAction("NotFoundPage", "Home", new { area = "" });
            }
            try
            {
                _categoryRepository.Remove(category);

                await _categoryRepository.SaveChangesAsync();

                TempData["Successful_Notification"] = "Category Deleted Successfully";
            }
            catch (DbUpdateException)
            {
                TempData["Error_Notification"] =
                    "Cannot delete this category because it may be linked to products.";
            }

            return RedirectToAction(nameof(Index));
        }
    }

}
