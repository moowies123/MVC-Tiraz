using Microsoft.AspNetCore.Mvc;
using MVC_Tiraz.Models.TIRAZ.Models;

namespace MVC_Tiraz.Areas.Admin
{
    [Area("Admin")]
    [Authorize(Roles = $"{CD.SUPER_ADMIN_ROLE} , {CD.ADMIN_ROLE} , {CD.EMPLOYEE_ROLE}")]
    public class ProductController : Controller
    {
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(
            IRepository<Product> productRepository,
            IRepository<Category> categoryRepository,
            IWebHostEnvironment webHostEnvironment)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _productRepository.GetAllAsync();
            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _categoryRepository.GetAllAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Product product, IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _categoryRepository.GetAllAsync();
                TempData["Error_Notification"] = "Invalid Data";
                return View(product);
            }

     
            if (imageFile != null && imageFile.Length > 0)
            {
                product.Image = await SaveImageAsync(imageFile);
            }

            await _productRepository.AddAsync(product);
            await _productRepository.SaveChangesAsync();
            TempData["Successful_Notification"] = "Product Created Successfully";
            return RedirectToAction(nameof(Index));
        }

    
        [HttpGet]
        [Authorize(Roles = $"{CD.SUPER_ADMIN_ROLE} , {CD.ADMIN_ROLE}")]
        public async Task<IActionResult> Update(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                return RedirectToAction("NotFoundPage", "Home");

            ViewBag.Categories = await _categoryRepository.GetAllAsync();
            return View(product);
        }

        [HttpPost]
        [Authorize(Roles = $"{CD.SUPER_ADMIN_ROLE} , {CD.ADMIN_ROLE}")]
        public async Task<IActionResult> Update(Product product, IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _categoryRepository.GetAllAsync();
                TempData["Error_Notification"] = "Invalid Data";
                return View(product);
            }
            if (imageFile != null && imageFile.Length > 0)
            {
              
                DeleteImage(product.Image);
                product.Image = await SaveImageAsync(imageFile);
            }
            _productRepository.Update(product);
            await _productRepository.SaveChangesAsync();
            TempData["Successful_Notification"] = "Product Updated Successfully";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = $"{CD.SUPER_ADMIN_ROLE}")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                TempData["Error_Notification"] = "Product Not Found";
                return RedirectToAction(nameof(Index));
            }

            DeleteImage(product.Image);

            _productRepository.Remove(product);
            await _productRepository.SaveChangesAsync();
            TempData["Successful_Notification"] = "Product Deleted Successfully";
            return RedirectToAction(nameof(Index));
        }

        private async Task<string> SaveImageAsync(IFormFile imageFile)
        {
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await imageFile.CopyToAsync(stream);

            return "/images/products/" + fileName;
        }

        private void DeleteImage(string? imagePath)
        {
            if (string.IsNullOrEmpty(imagePath)) return;

            var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, imagePath.TrimStart('/'));
            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);
        }
    }
}
