using AutoMapper;
using JewelryShop.DTO;
using JewelryShop.Models;
using JewelryShop.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JewelryShop.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class OrderController : Controller
    {
        private readonly JewelryShopContext _context;
        private readonly IMapper _mapper;
        private readonly EmailVerificationService _emailService;

        public OrderController(JewelryShopContext context, IMapper mapper, EmailVerificationService emailService)
        {
            _context = context;
            _mapper = mapper;
            _emailService = emailService;
        }

        [HttpGet]
        public IActionResult GetAllOrders()
        {
            var orders = _context.Orders.Include(o => o.OrderDetails).ThenInclude(od => od.Product).ToList();
            var result = _mapper.Map<List<OrderDTO>>(orders);
            return Ok(result);
        }
        [HttpGet("{userId}")]
        public IActionResult GetOrdersByUser(int userId)
        {
            var orders = _context.Orders.Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product).Where(o => o.UserId == userId)
                .ToList();
            var result = _mapper.Map<List<OrderDTO>>(orders);
            return Ok(result);
        }
        [HttpPost]
        public IActionResult CreateOrder([FromBody] CreateOrderDTO dto)
        {
            if (dto == null || dto.OrderDetails == null || dto.OrderDetails.Count == 0)
            {
                return BadRequest("Đơn hàng không hợp lệ.");
            }

            // Map DTO -> Order entity
            var order = _mapper.Map<Order>(dto);
            order.OrderDate = DateTime.Now;

            decimal total = 0;

            foreach (var detail in order.OrderDetails)
            {
                var product = _context.Products.FirstOrDefault(p => p.ProductId == detail.ProductId);
                if (product == null)
                    return BadRequest($"Không tìm thấy sản phẩm ID {detail.ProductId}");

                detail.UnitPrice = product.Price; // ✅ cập nhật đúng giá sản phẩm
                total += detail.UnitPrice * detail.Quantity;
            }

            order.TotalAmount = total; // ✅ ghi vào TotalAmount (trong model Order)

            _context.Orders.Add(order);
            _context.SaveChanges();

            var user = _context.Users.FirstOrDefault(u => u.UserId == order.UserId);
            if (user != null)
            {
                string subject = $"Xác nhận đơn hàng #{order.OrderId}";

                // Tạo body email HTML với chi tiết đơn hàng
                string body = $@"
            <h2>Chào {user.Email},</h2>
            <p>Cảm ơn bạn đã đặt hàng tại Jewelry Shop!</p>
            <p>Đơn hàng #{order.OrderId} của bạn đã được ghi nhận với chi tiết:</p>
            <ul>";

                foreach (var detail in order.OrderDetails)
                {
                    body += $"<li>{detail.Product.Name} x {detail.Quantity} - {detail.UnitPrice:C} / cái</li>";
                }

                body += $@"</ul>
            <p><strong>Tổng cộng: {order.TotalAmount:C}</strong></p>
            <br/>
            <p>Trân trọng,</p>
            <p>Jewelry Shop</p>
        ";

                // Gọi service để gửi email
                _emailService.SendEmail(user.Email, subject, body);
            }

            // Lấy lại đơn hàng có thông tin Product để trả về DTO
            var savedOrder = _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefault(o => o.OrderId == order.OrderId);

            var result = _mapper.Map<OrderDTO>(savedOrder);
            return Ok(result);
        }

        [HttpPatch("{orderId}")]
        public IActionResult UpdateOrderStatus(int orderId, [FromBody] UpdateOrderStatusDTO dto)
        {
            var order = _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefault(o => o.OrderId == orderId);

            if (order == null)
                return NotFound(new { message = "Không tìm thấy đơn hàng." });

            if (string.IsNullOrEmpty(dto.Status))
                return BadRequest(new { message = "Trạng thái không hợp lệ." });

            // ✅ Nếu chuyển sang "Đã thanh toán" và trước đó chưa trừ hàng
            if (dto.Status == "Paid" && order.Status != "Paid")
            {
                foreach (var detail in order.OrderDetails)
                {
                    var product = detail.Product;
                    if (product == null)
                        return BadRequest(new { message = $"Không tìm thấy sản phẩm có ID {detail.ProductId}." });

                    if (product.StockQuantity < detail.Quantity)
                        return BadRequest(new { message = $"Sản phẩm '{product.Name}' không đủ hàng. Hiện còn {product.StockQuantity}." });
                    product.StockQuantity -= detail.Quantity;
                }
            }

            order.Status = dto.Status;
            _context.SaveChanges();

            return Ok(new
            {
                message = $"Cập nhật trạng thái đơn hàng #{orderId} thành công.",
                newStatus = order.Status
            });
        }


        [HttpDelete("{id}")]
        public IActionResult DeleteOrder(int id)
        {
            var order = _context.Orders
                .Include(o => o.OrderDetails)
                .Include(o => o.PaymentTransactions)
                .FirstOrDefault(o => o.OrderId == id);
            if (order == null)
            {
                return NotFound(new { message = "Không tìm thấy đơn hàng" });
            }
            if (order.PaymentTransactions != null)
                _context.PaymentTransactions.RemoveRange(order.PaymentTransactions);
            _context.OrderDetails.RemoveRange(order.OrderDetails);
            _context.Orders.Remove(order);
            _context.SaveChanges();
            return Ok(" Xoá đơn hàng thành công");
        }
    }
}
