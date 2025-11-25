using AutoMapper;
using JewelryShop.DTO;
using JewelryShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JewelryShop.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class PaymentTransactionController : Controller
    {
        private readonly JewelryShopContext _context;
        private readonly IMapper _mapper;

        public PaymentTransactionController(JewelryShopContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        [HttpGet]
        public IActionResult GetAllTransactions()
        {
            var transactions = _context.PaymentTransactions
                .Include(p => p.Order)
                .ToList();

            var result = _mapper.Map<List<PaymentTransactionDTO>>(transactions);
            return Ok(result);
        }

        // 🔹 GET BY ID
        [HttpGet("{id}")]
        public IActionResult GetTransactionById(int id)
        {
            var transaction = _context.PaymentTransactions
                .Include(p => p.Order)
                .FirstOrDefault(t => t.TransactionId == id);

            if (transaction == null)
                return NotFound("Không tìm thấy giao dịch.");

            var result = _mapper.Map<PaymentTransactionDTO>(transaction);
            return Ok(result);
        }

        // 🔹 GET BY ORDER ID
        [HttpGet("{orderId}")]
        public IActionResult GetTransactionByOrder(int orderId)
        {
            var transactions = _context.PaymentTransactions
                .Include(p => p.Order)
                .Where(t => t.OrderId == orderId)
                .ToList();

            var result = _mapper.Map<List<PaymentTransactionDTO>>(transactions);
            return Ok(result);
        }

        // 🔹 CREATE
        [HttpPost("Create")]
        public IActionResult CreateTransaction([FromBody] CreatePaymentTransactionDTO dto)
        {
            var order = _context.Orders.FirstOrDefault(o => o.OrderId == dto.OrderId);
            if (order == null)
                return NotFound("Không tìm thấy đơn hàng tương ứng.");

            var transaction = _mapper.Map<PaymentTransaction>(dto);
            _context.PaymentTransactions.Add(transaction);
            _context.SaveChanges();

            var result = _mapper.Map<PaymentTransactionDTO>(transaction);
            return Ok(new
            {
                message = "Tạo giao dịch thanh toán thành công",
                transaction = result
            });
        }

        // 🔹 UPDATE STATUS
        [HttpPut("{id}")]
        public IActionResult UpdateTransactionStatus(int id, [FromBody] string newStatus)
        {
            var transaction = _context.PaymentTransactions.FirstOrDefault(t => t.TransactionId == id);
            if (transaction == null)
                return NotFound("Không tìm thấy giao dịch.");

            transaction.PaymentStatus = newStatus;
            _context.SaveChanges();

            return Ok(new
            {
                message = "Cập nhật trạng thái giao dịch thành công",
                transaction
            });
        }

        // 🔹 DELETE
        [HttpDelete("{id}")]
        public IActionResult DeleteTransaction(int id)
        {
            var transaction = _context.PaymentTransactions.FirstOrDefault(t => t.TransactionId == id);
            if (transaction == null)
                return NotFound("Không tìm thấy giao dịch.");

            _context.PaymentTransactions.Remove(transaction);
            _context.SaveChanges();

            return Ok("Xóa giao dịch thành công.");
        }
    }
}
