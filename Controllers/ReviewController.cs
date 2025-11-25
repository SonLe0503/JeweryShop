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
    public class ReviewController : Controller
    {
        private readonly JewelryShopContext _context;
        private readonly IMapper _mapper;

        public ReviewController(JewelryShopContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult GetAllReviews()
        {
            var reviews = _context.Reviews.Include(r => r.User).Include(r => r.Product).ToList();
            var result = _mapper.Map<List<ReviewDTO>>(reviews);
            return Ok(result);
        }

        [HttpGet("{productId}")]
        public IActionResult GetReviewsByProduct(int productId)
        {
            var reviews = _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Product)
                .Where(r => r.ProductId == productId)
                .ToList();

            var result = _mapper.Map<List<ReviewDTO>>(reviews);
            return Ok(result);
        }

        [HttpGet("ByUser/{userId}")]
        public IActionResult GetReviewsByUser(int userId)
        {
            var reviews = _context.Reviews
                .Include(r => r.Product)
                .Include(r => r.User)
                .Where(r => r.UserId == userId)
                .ToList();

            var result = _mapper.Map<List<ReviewDTO>>(reviews);
            return Ok(result);
        }

        [HttpPost]
        [Authorize] 
        public IActionResult CreateReview([FromBody] CreateReviewRequest request)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserId == request.UserId);
            var product = _context.Products.FirstOrDefault(p => p.ProductId == request.ProductId);

            if (user == null) return NotFound(new { message = "Người dùng không tồn tại" });
            if (product == null) return NotFound(new { message = "Sản phẩm không tồn tại" });

            if (request.Rating < 1 || request.Rating > 5)
                return BadRequest(new { message = "Điểm đánh giá phải từ 1 đến 5" });

            var existingReview = _context.Reviews
                .FirstOrDefault(r => r.UserId == request.UserId && r.ProductId == request.ProductId);

            if (existingReview != null)
                return BadRequest(new { message = "Bạn đã đánh giá sản phẩm này rồi" });

            var review = _mapper.Map<Review>(request);
            _context.Reviews.Add(review);
            _context.SaveChanges();

            var result = _mapper.Map<ReviewDTO>(review);
            return Ok(new { message = "Đánh giá thành công", review = result });
        }

        [HttpPut("{reviewId}")]
        [Authorize]
        public IActionResult UpdateReview(int reviewId, [FromBody] CreateReviewRequest request)
        {
            var review = _context.Reviews.FirstOrDefault(r => r.ReviewId == reviewId);
            if (review == null)
                return NotFound(new { message = "Không tìm thấy đánh giá" });

            if (request.Rating < 1 || request.Rating > 5)
                return BadRequest(new { message = "Điểm đánh giá phải từ 1 đến 5" });

            review.Rating = request.Rating;
            review.Comment = request.Comment;
            _context.SaveChanges();

            var result = _mapper.Map<ReviewDTO>(review);
            return Ok(new { message = "Cập nhật đánh giá thành công", review = result });
        }

        [HttpDelete("{reviewId}")]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteReview(int reviewId)
        {
            var review = _context.Reviews.FirstOrDefault(r => r.ReviewId == reviewId);
            if (review == null)
                return NotFound(new { message = "Không tìm thấy đánh giá" });

            _context.Reviews.Remove(review);
            _context.SaveChanges();

            return Ok(new { message = "Xóa đánh giá thành công" });
        }
    }
}
