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
    public class ReplyController : Controller
    {
        private readonly JewelryShopContext _context;
        private readonly IMapper _mapper;
        public ReplyController(JewelryShopContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var replies = await _context.Replies
                .Include(r => r.User)
                .Include(r => r.Review)
                .ToListAsync();

            var result = _mapper.Map<List<ReplyDTO>>(replies);
            return Ok(result);
        }

        // ✅ Get by id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var reply = await _context.Replies
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.ReplyId == id);

            if (reply == null)
                return NotFound("Reply not found");

            return Ok(_mapper.Map<ReplyDTO>(reply));
        }

        // ✅ Get replies by review id
        [HttpGet("{reviewId}")]
        public async Task<IActionResult> GetByReviewId(int reviewId)
        {
            var replies = await _context.Replies
                .Include(r => r.User)
                .Where(r => r.ReviewId == reviewId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return Ok(_mapper.Map<List<ReplyDTO>>(replies));
        }

        // ✅ Create reply
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] ReplyRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var reply = _mapper.Map<Reply>(request);
            reply.CreatedAt = DateTime.Now;
            reply.Status = "Visible";

            _context.Replies.Add(reply);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Reply created successfully" });
        }

        // ✅ Update reply
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] ReplyRequest request)
        {
            var existing = await _context.Replies.FindAsync(id);
            if (existing == null)
                return NotFound("Reply not found");

            existing.Comment = request.Comment;
            existing.ReviewId = request.ReviewId;
            existing.UserId = request.UserId;

            _context.Replies.Update(existing);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Reply updated successfully" });
        }

        // ✅ Delete reply (soft delete)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var reply = await _context.Replies.FindAsync(id);
            if (reply == null)
                return NotFound("Reply not found");

            _context.Replies.Remove(reply);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Reply deleted successfully" });
        }
    }
}
