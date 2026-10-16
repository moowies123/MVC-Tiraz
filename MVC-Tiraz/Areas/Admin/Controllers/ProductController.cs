//namespace MVC_Tiraz.Areas.Admin.Controllers
//{
//    [Area("Admin")]
//    [Authorize(Roles = $"{CD.SUPER_ADMIN_ROLE} , {CD.ADMIN_ROLE} , {CD.EMPLOYEE_ROLE}")]
//    public class ProductController : Controller
//    {
//        private readonly IRepository<Product> _productRepository;
//        private readonly IRepository<Category> _categoryRepository;
//        private readonly IWebHostEnvironment _webHostEnvironment;

//        public ProductController(
//            IRepository<Product> productRepository,
//            IRepository<Category> categoryRepository,
//            IWebHostEnvironment webHostEnvironment)
//        {
//            _productRepository = productRepository;
//            _categoryRepository = categoryRepository;
//            _webHostEnvironment = webHostEnvironment;
//        }

//        public async Task<IActionResult> Index()
//        {
//            var products = await _productRepository.GetAllAsync();

//            ViewBag.Categories = await _categoryRepository.GetAllAsync();

//            return View(products);
//        }

//        [HttpGet]
//        public async Task<IActionResult> Create()
//        {
//            ViewBag.Categories = await _categoryRepository.GetAllAsync();

//            return View();
//        }

//        [HttpPost]
//        public async Task<IActionResult> Create(Product product, IFormFile? imageFile)
//        {
//            ModelState.Remove(nameof(Product.Category));

//            if (!ModelState.IsValid)
//            {
//                ViewBag.Categories = await _categoryRepository.GetAllAsync();

//                ModelState.AddModelError("", "Invalid Data");

//                return View(product);
//            }


//            if (imageFile != null && imageFile.Length > 0)
//            {
//                product.Image = await SaveImageAsync(imageFile);
//            }

//            await _productRepository.AddAsync(product);

//            await _productRepository.SaveChangesAsync();

//            TempData["Successful_Notification"] = "Product Created Successfully";

//            return RedirectToAction(nameof(Index));
//        }


//        [HttpGet]
//        [Authorize(Roles = $"{CD.SUPER_ADMIN_ROLE} , {CD.ADMIN_ROLE}")]
//        public async Task<IActionResult> Update(int id)
//        {
//            var product = await _productRepository.GetByIdAsync(id);

//            if (product == null)
//                return RedirectToAction("NotFoundPage", "Home");

//            ViewBag.Categories = await _categoryRepository.GetAllAsync();

//            return View(product);
//        }

//        [HttpPost]
//        [Authorize(Roles = $"{CD.SUPER_ADMIN_ROLE} , {CD.ADMIN_ROLE}")]
//        public async Task<IActionResult> Update(Product product, IFormFile? imageFile)
//        {
//            ModelState.Remove(nameof(Product.Category));

//            if (!ModelState.IsValid)
//            {
//                ViewBag.Categories = await _categoryRepository.GetAllAsync();

//                ModelState.AddModelError("", "Invalid Data");

//                return View(product);
//            }

//            if (imageFile != null && imageFile.Length > 0)
//            {

//                DeleteImage(product.Image);
//                product.Image = await SaveImageAsync(imageFile);
//            }

//            _productRepository.Update(product);

//            await _productRepository.SaveChangesAsync();

//            TempData["Successful_Notification"] = "Product Updated Successfully";

//            return RedirectToAction(nameof(Index));
//        }

//        [HttpPost]
//        [Authorize(Roles = $"{CD.SUPER_ADMIN_ROLE}")]
//        public async Task<IActionResult> Delete(int id)
//        {
//            var product = await _productRepository.GetByIdAsync(id);

//            if (product == null)
//            {
//                ModelState.AddModelError("", "product Not Found");

//                return RedirectToAction(nameof(Index));
//            }

//            DeleteImage(product.Image);

//            _productRepository.Remove(product);

//            await _productRepository.SaveChangesAsync();

//            TempData["Successful_Notification"] = "Product Deleted Successfully";

//            return RedirectToAction(nameof(Index));
//        }

//        private async Task<string> SaveImageAsync(IFormFile imageFile)
//        {
//            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products");

//            if (!Directory.Exists(uploadsFolder))
//                Directory.CreateDirectory(uploadsFolder);

//            var fileName = Guid.NewGuid() + Path.GetExtension(imageFile.FileName);
//            var filePath = Path.Combine(uploadsFolder, fileName);

//            using var stream = new FileStream(filePath, FileMode.Create);
//            await imageFile.CopyToAsync(stream);

//            return "/images/products/" + fileName;
//        }

//        private void DeleteImage(string? imagePath)
//        {
//            if (string.IsNullOrEmpty(imagePath)) return;

//            var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, imagePath.TrimStart('/'));
//            if (System.IO.File.Exists(fullPath))
//                System.IO.File.Delete(fullPath);
//        }
//    }
//}


using MVC_Tiraz.ViewModels;

namespace MVC_Tiraz.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = $"{CD.SUPER_ADMIN_ROLE},{CD.ADMIN_ROLE},{CD.EMPLOYEE_ROLE}")]
    public class ProductController : Controller
    {
        private const string Includes = "Sizes.Colors";
        private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

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
            var products = await _productRepository.GetAllAsync(null, Includes);

            ViewBag.Categories = await _categoryRepository.GetAllAsync();
            
            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _categoryRepository.GetAllAsync();

            return View(new ProductVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductVM vm, IFormFile? imageFile)
        {
            ValidateVariants(vm);
            ValidateImage(imageFile);

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _categoryRepository.GetAllAsync();
                TempData["Error_Notification"] = "Invalid Data";
                return View(vm);
            }

            var product = new Product
            {
                Name = vm.Name.Trim(),
                Material = vm.Material.Trim(),
                Price = vm.Price,
                Description = vm.Description?.Trim() ?? "",
                Status = vm.Status,
                CategoryId = vm.CategoryId
            };

            if (imageFile != null && imageFile.Length > 0)
                product.Image = await SaveImageAsync(imageFile);

            SyncVariants(product, vm.Sizes);

            await _productRepository.AddAsync(product);
            await _productRepository.SaveChangesAsync();

            TempData["Successful_Notification"] = "Product Created Successfully";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize(Roles = $"{CD.SUPER_ADMIN_ROLE},{CD.ADMIN_ROLE}")]
        public async Task<IActionResult> Update(int id)
        {
            var product = (await _productRepository.GetAllAsync(p => p.ProductId == id, Includes)).FirstOrDefault();

            if (product == null)
                return RedirectToAction("NotFoundPage", "Home", new { area = "" });

            ViewBag.Categories = await _categoryRepository.GetAllAsync();
            return View(ToVM(product));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{CD.SUPER_ADMIN_ROLE},{CD.ADMIN_ROLE}")]
        public async Task<IActionResult> Update(ProductVM vm, IFormFile? imageFile)
        {
            ValidateVariants(vm);
            ValidateImage(imageFile);

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _categoryRepository.GetAllAsync();
                TempData["Error_Notification"] = "Invalid Data";
                return View(vm);
            }

            // Load the saved product with its sizes and colors, then change only what the form changed
            var product = (await _productRepository.GetAllAsync(p => p.ProductId == vm.ProductId, Includes)).FirstOrDefault();

            if (product == null)
                return RedirectToAction("NotFoundPage", "Home", new { area = "" });

            product.Name = vm.Name.Trim();
            product.Material = vm.Material.Trim();
            product.Price = vm.Price;
            product.Description = vm.Description?.Trim() ?? "";
            product.Status = vm.Status;
            product.CategoryId = vm.CategoryId;

            string? oldImage = null;
            string? newImage = null;

            if (imageFile != null && imageFile.Length > 0)
            {
                oldImage = product.Image;
                newImage = await SaveImageAsync(imageFile);
                product.Image = newImage;
            }

            SyncVariants(product, vm.Sizes);

            try
            {
                await _productRepository.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                DeleteImage(newImage);

                ViewBag.Categories = await _categoryRepository.GetAllAsync();
                ModelState.AddModelError("", "A size or color you removed is used in a cart or an order. Keep it and set its quantity to 0 instead.");
                return View(vm);
            }

            DeleteImage(oldImage);

            TempData["Successful_Notification"] = "Product Updated Successfully";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{CD.SUPER_ADMIN_ROLE},{CD.ADMIN_ROLE}")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);

            if (product == null)
            {
                TempData["Error_Notification"] = "Product Not Found";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _productRepository.Remove(product);
                await _productRepository.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                TempData["Error_Notification"] = "Cannot delete this product because it is used in orders or carts.";
                return RedirectToAction(nameof(Index));
            }

            DeleteImage(product.Image);

            TempData["Successful_Notification"] = "Product Deleted Successfully";
            return RedirectToAction(nameof(Index));
        }

        // used to compare colors names ex: "red" , "Red"
        private static bool Same(string? a, string? b) =>
            string.Equals(a?.Trim(), b?.Trim(), StringComparison.OrdinalIgnoreCase);

        private void ValidateVariants(ProductVM vm)
        {
            if (vm.Sizes.Count == 0)
            {
                ModelState.AddModelError("", "Add at least one size.");
                return;
            }

            var seenSizes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var size in vm.Sizes)
            {
                var sizeName = size.Size?.Trim() ?? "";

                if (sizeName == "")
                {
                    ModelState.AddModelError("", "Every size needs a name.");
                    continue;
                }

                if (!seenSizes.Add(sizeName))
                    ModelState.AddModelError("", $"Size \"{sizeName}\" is added twice.");

                if (size.Colors.Count == 0)
                {
                    ModelState.AddModelError("", $"Size \"{sizeName}\" needs at least one color.");
                    continue;
                }

                var seenColors = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var color in size.Colors)
                {
                    var colorName = color.Color?.Trim() ?? "";

                    if (colorName == "")
                        ModelState.AddModelError("", $"Size \"{sizeName}\": every color needs a name.");
                    else if (!seenColors.Add(colorName))
                        ModelState.AddModelError("", $"Size \"{sizeName}\": color \"{colorName}\" is added twice.");

                    if (color.Quantity is null or < 0)
                        ModelState.AddModelError("", $"Size \"{sizeName}\": quantity must be 0 or more.");
                }
            }
        }

        private static void SyncVariants(Product product, List<ProductSizeVM> sizes)
        {
            foreach (var size in product.Sizes.ToList())
            {
                var sizeVm = sizes.FirstOrDefault(s => Same(s.Size, size.Size));

                if (sizeVm == null)
                {
                    product.Sizes.Remove(size);
                    continue;
                }

                foreach (var color in size.Colors.ToList())
                {
                    if (!sizeVm.Colors.Any(c => Same(c.Color, color.Color)))
                        size.Colors.Remove(color);
                }
            }

            foreach (var sizeVm in sizes)
            {
                var size = product.Sizes.FirstOrDefault(s => Same(s.Size, sizeVm.Size));

                if (size == null)
                {
                    size = new ProductSize { Size = sizeVm.Size!.Trim() };
                    product.Sizes.Add(size);
                }

                foreach (var colorVm in sizeVm.Colors)
                {
                    var color = size.Colors.FirstOrDefault(c => Same(c.Color, colorVm.Color));

                    if (color == null)
                        size.Colors.Add(new ProductColor { Color = colorVm.Color!.Trim(), Quantity = colorVm.Quantity ?? 0 });
                    else
                        color.Quantity = colorVm.Quantity ?? 0;
                }
            }
        }

        private static ProductVM ToVM(Product p) => new()
        {
            ProductId = p.ProductId,
            Name = p.Name,
            Material = p.Material,
            Price = p.Price,
            Description = p.Description,
            Image = p.Image,
            Status = p.Status,
            CategoryId = p.CategoryId,
            Sizes = p.Sizes
                .OrderBy(s => s.ProductSizeId)
                .Select(s => new ProductSizeVM
                {
                    Size = s.Size,
                    Colors = s.Colors
                        .OrderBy(c => c.ProductColorId)
                        .Select(c => new ProductColorVM { Color = c.Color, Quantity = c.Quantity })
                        .ToList()
                })
                .ToList()
        };


        private void ValidateImage(IFormFile? imageFile)
        {
            if (imageFile == null || imageFile.Length == 0) return;

            if (!AllowedImageExtensions.Contains(Path.GetExtension(imageFile.FileName).ToLowerInvariant()))
                ModelState.AddModelError("", "Image must be a .jpg, .png or .webp file.");

            if (imageFile.Length > 2 * 1024 * 1024)
                ModelState.AddModelError("", "Image must be 2 MB or smaller.");
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
