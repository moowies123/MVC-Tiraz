namespace MVC_Tiraz.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = $"{CD.SUPER_ADMIN_ROLE} , {CD.ADMIN_ROLE}  ,{CD.EMPLOYEE_ROLE} ")]
    public class CategoryController : Controller
    {
        private readonly IRepository<Category> _categoryRepository;

        public CategoryController(IRepository<Category> categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }
        public async Task<IActionResult> Index()
        {
            var categories = await _categoryRepository.GetAllAsync();

            return View(categories.AsEnumerable());
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
                ModelState.AddModelError("", "invalid Data");
                return View(category);
            }

            await _categoryRepository.AddAsync(category);

            await _categoryRepository.SaveChangesAsync();
            
            TempData["Successful_Notification"] = "Category Created Successfully";
            
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize(Roles = $"{CD.SUPER_ADMIN_ROLE} , {CD.ADMIN_ROLE} ")]
        public async Task<IActionResult> Update(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            
            if (category == null)
            {
                return RedirectToAction("NotFoundPage", "Home");
            }

            return View(category);
        }

        [HttpPost]
        [Authorize(Roles = $"{CD.SUPER_ADMIN_ROLE} , {CD.ADMIN_ROLE} ")]
        public async Task<IActionResult> Update(Category category)
        {
            _categoryRepository.Update(category);

            await _categoryRepository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = $"{CD.SUPER_ADMIN_ROLE} , {CD.ADMIN_ROLE} ")]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);

            if (category == null)
            {
                return RedirectToAction("NotFoundPage", "Home");
            }

            _categoryRepository.Remove(category);

            await _categoryRepository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
