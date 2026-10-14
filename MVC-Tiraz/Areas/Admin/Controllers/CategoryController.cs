using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVC_Tiraz.Models.TIRAZ.Models;
<<<<<<< HEAD:MVC-Tiraz/Areas/Admin/Controllers/CategoryController.cs
namespace MVC_Tiraz.Areas.Admin.Controllers
=======

namespace MVC_Tiraz.Areas.Admin
>>>>>>> origin:MVC-Tiraz/Areas/Admin/CategoryController.cs
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

<<<<<<< HEAD:MVC-Tiraz/Areas/Admin/Controllers/CategoryController.cs
            return View((categories.AsEnumerable()));
=======
            return View(categories);
>>>>>>> origin:MVC-Tiraz/Areas/Admin/CategoryController.cs
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
<<<<<<< HEAD:MVC-Tiraz/Areas/Admin/Controllers/CategoryController.cs
            
            TempData["Successful_Notification"] = "Category Created Successfully";
            
=======

            TempData["Successful_Notification"] =
                "Category Created Successfully";

>>>>>>> origin:MVC-Tiraz/Areas/Admin/CategoryController.cs
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
<<<<<<< HEAD:MVC-Tiraz/Areas/Admin/Controllers/CategoryController.cs

        [HttpPost]
        [Authorize(Roles = $"{CD.SUPER_ADMIN_ROLE} , {CD.ADMIN_ROLE} ")]
=======
>>>>>>> origin:MVC-Tiraz/Areas/Admin/CategoryController.cs

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{CD.SUPER_ADMIN_ROLE},{CD.ADMIN_ROLE}")]
        public async Task<IActionResult> Update(Category category)
        {
<<<<<<< HEAD:MVC-Tiraz/Areas/Admin/Controllers/CategoryController.cs
            _categoryRepository.Update(category);

            await _categoryRepository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = $"{CD.SUPER_ADMIN_ROLE} , {CD.ADMIN_ROLE} ")]
=======
            if (!ModelState.IsValid)
            {
                return View(category);
            }

            var existingCategory =
                await _categoryRepository.GetByIdAsync(category.CategoryId);

            if (existingCategory == null)
            {
                return RedirectToAction("NotFoundPage", "Home",
                    new { area = "" });
            }

            existingCategory.Name = category.Name;
            existingCategory.Status = category.Status;

            await _categoryRepository.SaveChangesAsync();

            TempData["Successful_Notification"] =
                "Category Updated Successfully";

            return RedirectToAction(nameof(Index));
        }

      
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{CD.SUPER_ADMIN_ROLE},{CD.ADMIN_ROLE}")]
>>>>>>> origin:MVC-Tiraz/Areas/Admin/CategoryController.cs
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);

            if (category == null)
            {
                return RedirectToAction("NotFoundPage", "Home",
                    new { area = "" });
            }

<<<<<<< HEAD:MVC-Tiraz/Areas/Admin/Controllers/CategoryController.cs
            _categoryRepository.Remove(category);

            await _categoryRepository.SaveChangesAsync();
=======
            try
            {
                _categoryRepository.Remove(category);

                await _categoryRepository.SaveChangesAsync();

                TempData["Successful_Notification"] =
                    "Category Deleted Successfully";
            }
            catch (DbUpdateException)
            {
                TempData["Error_Notification"] =
                    "Cannot delete this category because it may be linked to products.";
            }
>>>>>>> origin:MVC-Tiraz/Areas/Admin/CategoryController.cs

            return RedirectToAction(nameof(Index));
        }
    }

}
