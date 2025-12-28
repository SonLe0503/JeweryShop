using AutoMapper;
using JewelryShop.DTO;
using JewelryShop.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JewelryShop.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class CartController : Controller
    {
        private readonly JewelryShopContext _context;
        private readonly IMapper _mapper;
        public CartController(JewelryShopContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [Authorize]
        [HttpGet("{userId}")]
        public IActionResult GetCartByUser(int userId)
        {
            var carts = _context.Carts
                .Include(c => c.Product)
                .ThenInclude(p => p.Category)
                .Where(c => c.UserId == userId).ToList();
            var cartDTOs = carts.Select(c =>
            {
                var unitPrice = c.Product.Price - (c.Product?.Discount ?? 0);
                return new CartDTO
                {
                    CartId = c.CartId,
                    UserId = c.UserId ?? 0,
                    ProductId = c.ProductId ?? 0,
                    Quantity = c.Quantity,
                    ProductName = c.Product.Name,
                    CategoryName = c.Product.Category.Name,
                    UnitPrice = unitPrice,
                    TotalPrice = unitPrice * c.Quantity,
                    ImgUrl = c.Product.ImageUrl
                };
            }).ToList();
            return Ok(cartDTOs);
        }

        [HttpPost]
        [Authorize]
        public IActionResult AddOrUpdateCart([FromBody] AddToCartRequest request)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserId == request.UserId);
            if (user == null)
                return NotFound(new { message = "Người dùng không tồn tại" });

            var product = _context.Products.FirstOrDefault(p => p.ProductId == request.ProductId);
            if (product == null)
                return NotFound(new { message = "Sản phẩm không tồn tại" });

            var existingCartItem = _context.Carts
                .FirstOrDefault(c => c.UserId == request.UserId && c.ProductId == request.ProductId);

            int newQuantity = request.Quantity;
            if (existingCartItem != null)
            {
                newQuantity += existingCartItem.Quantity; // tính tổng số lượng
            }

            if (newQuantity > product.StockQuantity)
            {
                return BadRequest(new
                {
                    message = $"Số lượng sản phẩm trong kho không đủ. Tối đa có thể thêm: {product.StockQuantity - (existingCartItem?.Quantity ?? 0)}"
                });
            }

            if (existingCartItem != null)
            {
                existingCartItem.Quantity = newQuantity;
            }
            else
            {
                var newCart = new Cart
                {
                    UserId = request.UserId,
                    ProductId = request.ProductId,
                    Quantity = request.Quantity
                };
                _context.Carts.Add(newCart);
            }

            _context.SaveChanges();

            var cart = _context.Carts
                .Include(c => c.Product)
                .FirstOrDefault(c => c.UserId == request.UserId && c.ProductId == request.ProductId);

            var result = _mapper.Map<CartDTO>(cart);
            return Ok(new { message = "Thêm hoặc cập nhật sản phẩm thành công", cart = result });
        }

        [HttpPut]
        [Authorize]
        public IActionResult UpdateCartQuantity([FromBody] UpdateCartQuantityRequest request)
        {
            var cartItem = _context.Carts
                .Include(c => c.Product)
                .FirstOrDefault(c => c.CartId == request.CartId);

            if (cartItem == null)
                return NotFound(new { message = "Không tìm thấy sản phẩm trong giỏ hàng" });

            if (request.Quantity <= 0)
                return BadRequest(new { message = "Số lượng không hợp lệ" });

            if (cartItem.Product.StockQuantity < request.Quantity)
                return BadRequest(new { message = "Số lượng trong kho không đủ" });

            cartItem.Quantity = request.Quantity;
            _context.SaveChanges();

            var result = _mapper.Map<CartDTO>(cartItem);
            return Ok(new { message = "Cập nhật số lượng thành công", cart = result });
        }
        [HttpDelete("{cartId}")]
        [Authorize]
        public IActionResult RemoveFromCart(int cartId)
        {
            var cartItem = _context.Carts
                .Include(c => c.Product)
                .FirstOrDefault(c => c.CartId == cartId);

            if (cartItem == null)
                return NotFound(new { message = "Không tìm thấy sản phẩm trong giỏ hàng" });

            _context.Carts.Remove(cartItem);
            _context.SaveChanges();

            var result = _mapper.Map<CartDTO>(cartItem);
            return Ok(new { message = "Đã xóa sản phẩm khỏi giỏ hàng", removedItem = result });
        }

        // 🧹 5. Xóa toàn bộ giỏ hàng của người dùng
        [HttpDelete("{userId}")]
        [Authorize]
        public IActionResult ClearCart(int userId)
        {
            var carts = _context.Carts
                .Where(c => c.UserId == userId)
                .ToList();

            if (!carts.Any())
                return NotFound(new { message = "Giỏ hàng đã trống" });

            _context.Carts.RemoveRange(carts);
            _context.SaveChanges();

            return Ok(new { message = "Đã xóa toàn bộ giỏ hàng" });
        }
    }
}
