using System.Security.Claims;
using Stripe;
using Stripe.Checkout;
using Stripe.Climate;

namespace MVC_Tiraz.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IRepository<Cart> _cartRepository;
        private readonly IRepository<Promotion> _promotionRepository;
        private readonly IRepository<ProductColor> _colorRepository;
        private readonly IRepository<MVC_Tiraz.Models.TIRAZ.Models.Product> _productRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartController(
            IRepository<Cart> cartRepository,
            IRepository<MVC_Tiraz.Models.TIRAZ.Models.Product> productRepository,
            UserManager<ApplicationUser> userManager,
            IRepository<ProductColor> colorRepository,
            IRepository<Promotion> promotionRepository)
        {
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _userManager = userManager;
            _colorRepository = colorRepository;
            _promotionRepository = promotionRepository;
        }

        [Authorize]
        public async Task<IActionResult> Index(string? promotionCode)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var lines = await _cartRepository.GetAllIncludingAsync(c => c.UserId == userId,
                includePaths: ["Product.Images", "ProductColor.ProductSize"]);

            var items = lines.Select(c => new CartItemVM
            {
                ProductColorId = c.ProductColorId,
                ProductName = c.Product.Name,
                ImageUrl = c.Product.Images.OrderBy(i => i.ProductImageId).Select(i => i.Url).FirstOrDefault(),
                Size = c.ProductColor.ProductSize.Size,
                Color = c.ProductColor.Color,
                UnitPrice = c.Product.Price - c.Product.Price * (c.Product.Discount ?? 0) / 100,
                Quantity = c.Count,
                LineTotal = c.TotalPrice
            }).ToList();

            return View(items);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplyPromo(string? promotionCode)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var code = promotionCode?.Trim();

            var promotion = await _promotionRepository.FirstOrDefaultAsync(p =>
                p.Code == code && p.IsValid && p.ValidTo > DateTime.UtcNow && p.MaxUsage > 0);

            if (promotion == null)
            {
                TempData["Error_Notification"] = "Invalid / expired code";
                return BackToCart();
            }

            var lines = (await _cartRepository.FindAsync(c => c.UserId == userId && c.ProductId == promotion.ProductId)).ToList();

            if (lines.Count == 0)
            {
                TempData["Error_Notification"] = "This code doesn't apply to anything in your cart.";
                return BackToCart();
            }

            if (lines.Any(l => l.PromotionCode != null))
            {
                TempData["Error_Notification"] = "A promo code is already applied to this product.";
                return BackToCart();
            }

            foreach (var line in lines)
            {
                line.PromotionCode = promotion.Code;
                line.PromotionDiscount = promotion.Discount;
                line.TotalPrice -= line.TotalPrice * promotion.Discount / 100;
            }

            await _cartRepository.SaveChangesAsync();

            TempData["Successful_Notification"] = "Promotion applied successfully!";
            return BackToCart();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int productId, int count, int productColorId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var color = await _colorRepository.FirstOrDefaultAsync(c => c.ProductColorId == productColorId && c.ProductSize.ProductId == productId);
            if (color == null) return NotFound();

            var line = await _cartRepository.FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId && c.ProductColorId == productColorId);

            var newCount = (line?.Count ?? 0) + count;

            if (newCount > color.Quantity)
            {
                TempData["Error_Notification"] = $"Only {color.Quantity} in stock.";
                return RedirectToAction("ProductDetails", "Home", new { area = "Customer", id = productId });
            }

            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null) return NotFound();

            var unitPrice = product.Price - product.Price * (product.Discount ?? 0) / 100;

            decimal Total(int qty, decimal? promoPercent) => unitPrice * qty - unitPrice * qty * (promoPercent ?? 0) / 100;

            if (line == null)
            {
                var promoLine = await _cartRepository.FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId && c.PromotionCode != null);

                await _cartRepository.AddAsync(new Cart
                {
                    UserId = userId,
                    ProductId = productId,
                    ProductColorId = productColorId,
                    Count = count,
                    PromotionCode = promoLine?.PromotionCode,
                    PromotionDiscount = promoLine?.PromotionDiscount,
                    TotalPrice = Total(count, promoLine?.PromotionDiscount)
                });
            }
            else
            {
                line.Count = newCount;
                line.TotalPrice = Total(newCount, line.PromotionDiscount);
            }

            await _cartRepository.SaveChangesAsync();

            TempData["Successful_Notification"] = "Added to cart";
            return RedirectToAction("ProductDetails", "Home", new { area = "Customer", id = productId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Remove(int productColorId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var line = await _cartRepository.FirstOrDefaultAsync(c => c.UserId == userId && c.ProductColorId == productColorId);

            if (line != null)
            {
                _cartRepository.Remove(line);

                await _cartRepository.SaveChangesAsync();

                TempData["Successful_Notification"] = "Removed from cart";
            }

            return BackToCart();
        }

        private IActionResult BackToCart()
        {
            return Request.Headers["X-Requested-With"] == "XMLHttpRequest"
                ? RedirectToAction("Index", "Cart", new { area = "Customer" })
                : RedirectToAction("Index", "Home", new { area = "Customer" });
        }

        private async Task ConsumePromotionsAsync(IEnumerable<Cart> lines)
        {
            var codes = lines.Where(l => l.PromotionCode != null)
                             .Select(l => l.PromotionCode!)
                             .Distinct()
                             .ToList();

            foreach (var code in codes)
            {
                var promotion = await _promotionRepository.FirstOrDefaultAsync(p => p.Code == code);

                if (promotion != null && promotion.MaxUsage > 0)
                    promotion.MaxUsage--;
            }

            await _promotionRepository.SaveChangesAsync();
        }


        public async Task<IActionResult> Pay()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null) return NotFound();

            var carts = (await _cartRepository.GetAllIncludingAsync(
                c => c.UserId == user.Id,
                includePaths: ["Product", "ProductColor.ProductSize"])).ToList();

            if (carts.Count == 0)
                return RedirectToAction("Index", "Home", new { area = "Customer" });

            var domain = $"{Request.Scheme}://{Request.Host}/";

            var options = new SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                ClientReferenceId = user.Id,
                SuccessUrl = domain + "Customer/Cart/Success?session_id={CHECKOUT_SESSION_ID}",
                CancelUrl = domain + "Customer/Cart/Index"
            };

            foreach (var item in carts)
            {
                options.LineItems.Add(new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "egp",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = $"{item.Product.Name} ({item.ProductColor.ProductSize.Size} / {item.ProductColor.Color}) x{item.Count}",
                            Description = item.Product.Description,
                        },
                        UnitAmount = (long)Math.Round(item.TotalPrice * 100),
                    },
                    Quantity = 1,
                });
            }

            var service = new SessionService();
            var session = await service.CreateAsync(options);
            return Redirect(session.Url);
        }

        public async Task<IActionResult> Success(string session_id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var session = await new SessionService().GetAsync(session_id);

            if (session.PaymentStatus != "paid" || session.ClientReferenceId != userId)
                return RedirectToAction("Index", "Home", new { area = "Customer" });

            var carts = (await _cartRepository.GetAllIncludingAsync(
                c => c.UserId == userId, includePaths: ["ProductColor"])).ToList();

            foreach (var item in carts)
                item.ProductColor.Quantity = Math.Max(0, item.ProductColor.Quantity - item.Count);

            await ConsumePromotionsAsync(carts);

            _cartRepository.RemoveRange(carts);

            await _cartRepository.SaveChangesAsync();

            TempData["Successful_Notification"] = "Payment received. Thank you for your order!";
            return RedirectToAction("Index", "Home", new { area = "Customer" });
        }

    }
}
