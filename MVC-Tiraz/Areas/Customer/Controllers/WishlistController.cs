
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MVC_Tiraz.Models;
using TIRAZ.Repositories;

namespace MVC_Tiraz.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize(Roles = CD.CUSTOMER_ROLE)]
    public class WishlistController : Controller
    {
        private readonly IRepository<Wishlist> _wishlistRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public WishlistController(
            IRepository<Wishlist> wishlistRepository,
            IRepository<Product> productRepository,
            UserManager<ApplicationUser> userManager)
        {
            _wishlistRepository = wishlistRepository;
            _productRepository = productRepository;
            _userManager = userManager;
        }

        
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
                return Challenge();

            var wishlist = await _wishlistRepository.GetAllIncludingAsync(
                w => w.UserId == userId,
                includePaths: ["Product", "Product.Images", "Product.Category" , "Product.sizes"]
            );

            return View(wishlist);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int productId)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
                return Challenge();

            var productExists = await _productRepository.AnyAsync(
                p => p.ProductId == productId && p.Status
            );

            if (!productExists)
                return NotFound();

            var alreadyExists = await _wishlistRepository.AnyAsync(
                w => w.UserId == userId && w.ProductId == productId
            );

            if (!alreadyExists)
            {
                await _wishlistRepository.AddAsync(new Wishlist
                {
                    UserId = userId,
                    ProductId = productId
                });

                await _wishlistRepository.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int productId)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
                return Challenge();

            var wishlistItem = await _wishlistRepository.FirstOrDefaultAsync(
                w => w.UserId == userId && w.ProductId == productId
            );

            if (wishlistItem == null)
                return NotFound();

            _wishlistRepository.Remove(wishlistItem);
            await _wishlistRepository.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
