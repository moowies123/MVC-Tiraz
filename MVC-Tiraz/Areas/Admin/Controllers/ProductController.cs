using MVC_Tiraz.Services;
using static MVC_Tiraz.ViewModels.ProductVM;

namespace MVC_Tiraz.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = $"{CD.SUPER_ADMIN_ROLE},{CD.ADMIN_ROLE},{CD.EMPLOYEE_ROLE}")]
    public class ProductController : Controller
    {
        private const string Includes = "Sizes.Colors";
        private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".jfif" };

        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IImageStorage _imageStorage;
        private readonly IImageValidator _imageValidator;

        public ProductController(
            IRepository<Product> productRepository,
            IRepository<Category> categoryRepository,
            IImageStorage imageStorage,
            IImageValidator imageValidator)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _imageStorage = imageStorage;
            _imageValidator = imageValidator;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _productRepository.GetAllIncludingAsync(null, includePaths: ["Sizes.Colors", "Images"]);

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
        public async Task<IActionResult> Create(ProductVM vm, List<IFormFile> imageFiles)
        {
            ValidateVariants(vm);

            foreach (var error in _imageValidator.Validate(imageFiles))
                ModelState.AddModelError("", error);

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

            foreach (var url in await _imageStorage.SaveAsync(imageFiles))
                product.Images.Add(new ProductImage { Url = url });

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
            var product = (await _productRepository.GetAllIncludingAsync(p => p.ProductId == id, includePaths: ["Sizes.Colors", "Images"])).FirstOrDefault();

            if (product == null)
                return RedirectToAction("NotFoundPage", "Home", new { area = "" });

            ViewBag.Categories = await _categoryRepository.GetAllAsync();
            return View(ToVM(product));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{CD.SUPER_ADMIN_ROLE},{CD.ADMIN_ROLE}")]
        public async Task<IActionResult> Update(ProductVM vm, List<IFormFile> imageFiles, List<int> removeImageIds)
        {
            ValidateVariants(vm);
            foreach (var error in _imageValidator.Validate(imageFiles))
                ModelState.AddModelError("", error);

            if (!ModelState.IsValid)
            {
                vm.ExistingImages = await GetExistingImagesAsync(vm.ProductId);
                ViewBag.Categories = await _categoryRepository.GetAllAsync();
                TempData["Error_Notification"] = "Invalid Data";
                return View(vm);
            }

            var product = (await _productRepository.GetAllIncludingAsync(p => p.ProductId == vm.ProductId, includePaths: ["Sizes.Colors", "Images"])).FirstOrDefault();

            if (product == null)
                return RedirectToAction("NotFoundPage", "Home", new { area = "" });

            var removed = product.Images.Where(i => removeImageIds.Contains(i.ProductImageId)).ToList();

            foreach (var image in removed)
                product.Images.Remove(image);

            var newUrls = await _imageStorage.SaveAsync(imageFiles);

            foreach (var url in newUrls)
                product.Images.Add(new ProductImage { Url = url });

            SyncVariants(product, vm.Sizes);

            try
            {
                await _productRepository.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                _imageStorage.Delete(newUrls);

                vm.ExistingImages = await GetExistingImagesAsync(vm.ProductId);

                ViewBag.Categories = await _categoryRepository.GetAllAsync();

                ModelState.AddModelError("", "A size or color you removed is used in a cart or an order. Keep it and set its quantity to 0 instead.");
                return View(vm);
            }

            _imageStorage.Delete(removed.Select(i => i.Url));

            TempData["Successful_Notification"] = "Product Updated Successfully";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{CD.SUPER_ADMIN_ROLE},{CD.ADMIN_ROLE}")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = (await _productRepository.GetAllIncludingAsync(p => p.ProductId == id, includePaths: ["Images"])).FirstOrDefault();

            if (product == null)
            {
                TempData["Error_Notification"] = "Product Not Found";
                return RedirectToAction(nameof(Index));
            }

            var urls = product.Images.Select(i => i.Url).ToList();

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

            _imageStorage.Delete(urls);

            TempData["Successful_Notification"] = "Product Deleted Successfully";
            return RedirectToAction(nameof(Index));
        }

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
            ExistingImages = p.Images.Select(i => new ProductImageVM(i.ProductImageId, i.Url)).ToList(),
            Description = p.Description,
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

        private async Task<List<ProductImageVM>> GetExistingImagesAsync(int productId)
        {
            var product = (await _productRepository.GetAllIncludingAsync(p => p.ProductId == productId, includePaths: ["Images"])).FirstOrDefault();
            return product?.Images.Select(i => new ProductImageVM(i.ProductImageId, i.Url)).ToList() ?? new();
        }
    }
}
