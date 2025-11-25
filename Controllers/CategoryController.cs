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
    public class CategoryController : Controller
    {
        private readonly JewelryShopContext _context;
        private readonly IMapper _mapper;

        public CategoryController(JewelryShopContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult GetAllCategories()
        {
            var categories = _context.Categories.Include(c => c.Products).ToList();
            var result = _mapper.Map<List<CategoryDTO>>(categories);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public IActionResult GetCategoryById(int id)
        {
            var category = _context.Categories
                .Include(c => c.Products)
                .FirstOrDefault(c => c.CategoryId == id);

            if (category == null)
                return NotFound(new { message = "Không tìm thấy danh mục" });

            var result = _mapper.Map<CategoryDTO>(category);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult CreateCategory([FromBody] CategoryRequest request)
        {
            if (string.IsNullOrEmpty(request.CategoryName))
                return BadRequest(new { message = "Tên danh mục là bắt buộc" });

            var exists = _context.Categories.Any(c => c.Name == request.CategoryName);
            if (exists)
                return BadRequest(new { message = "Danh mục này đã tồn tại" });

            var category = _mapper.Map<Category>(request);

            _context.Categories.Add(category);
            _context.SaveChanges();
            _context.Entry(category).Reload(); // Reload để lấy ID mới tạo

            var result = _mapper.Map<CategoryDTO>(category);
            return Ok(new { message = "Tạo danh mục thành công", category = result });
        }

        [HttpPut]
        [Authorize(Roles = "Admin")]
        public IActionResult EditCategory(int id, [FromBody] CategoryRequest request)
        {
            var category = _context.Categories.FirstOrDefault(c => c.CategoryId == id);
            if (category == null)
                return NotFound(new { message = "Không tìm thấy danh mục" });

            if (!string.IsNullOrEmpty(request.CategoryName))
                category.Name = request.CategoryName;

            if (!string.IsNullOrEmpty(request.Description))
                category.Description = request.Description;

            _context.SaveChanges();

            var result = _mapper.Map<CategoryDTO>(category);
            return Ok(new { message = "Cập nhật danh mục thành công", category = result });
        }

        // ❌ 5. Xóa danh mục (Admin)
        [HttpDelete]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteCategory(int id)
        {
            var category = _context.Categories
                .Include(c => c.Products)
                .FirstOrDefault(c => c.CategoryId == id);

            if (category == null)
                return NotFound(new { message = "Danh mục không tồn tại" });

            if (category.Products.Any())
                return BadRequest(new { message = "Không thể xóa danh mục vì vẫn còn sản phẩm trong danh mục này" });

            //category.Status = "Deleted";
            _context.Remove(category);
            _context.SaveChanges();

            return Ok(new { message = "Xóa danh mục thành công" });
        }
    }
}
