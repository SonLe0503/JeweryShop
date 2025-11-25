using AutoMapper;
using JewelryShop.DTO;
using JewelryShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JewelryShop.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class OrderDetailController : Controller
    {
        private readonly JewelryShopContext _context;
        private readonly IMapper _mapper;

        public OrderDetailController(JewelryShopContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("{orderId}")]
        public IActionResult GetOrderDetails(int orderId)
        {
            var orderDetails = _context.OrderDetails.Include(od => od.Product)
                .Where(od => od.OrderId == orderId)
                .ToList();
            var result = _mapper.Map<List<OrderDetailDTO>>(orderDetails);
            return Ok(result);
        }

        [HttpPost]
        public IActionResult AddOrderDetail([FromBody] CreateOrderDetailDTO dto, int orderId)
        {
            var order = _context.Orders.FirstOrDefault(o => o.OrderId == orderId);
            if (order == null) return NotFound("Không tìm thấy đơn hàng");

            var detail = _mapper.Map<OrderDetail>(dto);
            detail.OrderId = orderId;

            _context.OrderDetails.Add(detail);
            _context.SaveChanges();

            var result = _mapper.Map<OrderDetailDTO>(detail);
            return Ok(result);
        }
    }
}
