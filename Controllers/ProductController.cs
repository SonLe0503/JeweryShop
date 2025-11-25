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
    public class ProductController : Controller
    {
        private readonly JewelryShopContext _context;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _env;

        public ProductController(JewelryShopContext context, IMapper mapper,IWebHostEnvironment env)
        {
            _context = context;
            _mapper = mapper;
            _env = env;
        }

        [HttpGet]

        public IActionResult GetAllProducts()
        {
            var products = _context.Products
                .Include(p => p.Category) // nếu bạn muốn lấy luôn thông tin Category
                .Include(p => p.Collection)
                .ToList();

            var result = _mapper.Map<List<ProductDTO>>(products);

            return Ok(result);
        }

        [HttpGet("{id}")]
        public IActionResult GetProductById(int id)
        {
            var product = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Collection)
                .Include(p => p.ProductImages)
                .Include(p => p.Reviews)
                .ThenInclude(r => r.User) // nếu bạn muốn lấy thông tin người dùng của đánh giá
                .FirstOrDefault(p => p.ProductId == id);
            if (product == null)
                return NotFound(new { message = "Không tìm thấy sản phẩm" });
            var result = _mapper.Map<ProductDTO>(product);
            return Ok(result);
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateProduct([FromForm] ProductRequest request )
        {
            if (request.CategoryId == 0)
                request.CategoryId = null;

            if (request.CollectionId == 0)
                request.CollectionId = null;
            if (request.CategoryId.HasValue && !_context.Categories.Any(c => c.CategoryId == request.CategoryId))
                return BadRequest(new { message = "Danh mục không tồn tại" });

            if (string.IsNullOrEmpty(request.Name) || !request.Price.HasValue)
                return BadRequest(new { message = "Name và Price là bắt buộc" });

            string? imageUrl = null;

            // 📂 Upload ảnh nếu có
            if (request.ImageFile != null && request.ImageFile.Length > 0)
            {
                string uploadPath = Path.Combine(_env.WebRootPath, "uploads", "products");
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(request.ImageFile.FileName);
                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await request.ImageFile.CopyToAsync(stream);
                }

                // đường dẫn tương đối lưu trong DB
                imageUrl = $"/uploads/products/{fileName}";
            }

            var product = new Product
            {
                Name = request.Name,
                Description = request.Description,
                Price = request.Price.Value,
                Discount = request.Discount,
                StockQuantity = request.StockQuantity,
                Material = request.Material,
                ImageUrl = imageUrl,
                Color = request.Color,
                Story = request.Story,
                CategoryId = request.CategoryId,
                CollectionId = request.CollectionId,
                CreatedAt = DateTime.Now,
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            var productDto = _mapper.Map<ProductDTO>(product);
            return Ok(new { message = "Tạo sản phẩm thành công", product = productDto });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{productId}")]
        public async Task<IActionResult> EditProduct(int productId, [FromForm] ProductRequest request)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == productId);
            if (product == null)
                return NotFound(new { message = "Sản phẩm không tồn tại" });

            // ✅ Cập nhật thông tin cơ bản nếu có
            if (!string.IsNullOrEmpty(request.Name)) product.Name = request.Name;
            if (!string.IsNullOrEmpty(request.Description)) product.Description = request.Description;
            if (request.Price.HasValue) product.Price = request.Price.Value;
            if (request.Discount.HasValue) product.Discount = request.Discount.Value;
            if (request.StockQuantity.HasValue) product.StockQuantity = request.StockQuantity.Value;
            if (!string.IsNullOrEmpty(request.Material)) product.Material = request.Material;
            if (!string.IsNullOrEmpty(request.Story)) product.Story = request.Story;
            if (!string.IsNullOrEmpty(request.Color)) product.Color = request.Color;

            if (request.CategoryId == 0)
                request.CategoryId = null;
            if (request.CollectionId == 0)
                request.CollectionId = null;

            if (request.CategoryId.HasValue)
                product.CategoryId = request.CategoryId;
            if (request.CollectionId.HasValue)
                product.CollectionId = request.CollectionId;

            // ✅ Nếu có file ảnh mới, upload và thay thế ảnh cũ
            if (request.ImageFile != null && request.ImageFile.Length > 0)
            {
                string uploadPath = Path.Combine(_env.WebRootPath, "uploads", "products");
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                // Xóa ảnh cũ (nếu có)
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    var oldImagePath = Path.Combine(_env.WebRootPath, product.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                        System.IO.File.Delete(oldImagePath);
                }

                // Upload ảnh mới
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(request.ImageFile.FileName);
                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await request.ImageFile.CopyToAsync(stream);
                }

                product.ImageUrl = $"/uploads/products/{fileName}";
            }

            await _context.SaveChangesAsync();

            var productDto = _mapper.Map<ProductDTO>(product);
            return Ok(new { message = "Cập nhật sản phẩm thành công", product = productDto });
        }

        [Authorize]
        [HttpPatch("{productId}/update-stock")]
        public async Task<IActionResult> UpdateProductStock(int productId, [FromBody] UpdateStockRequest request)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == productId);
            if (product == null)
                return NotFound(new { message = "Sản phẩm không tồn tại" });

            if (!request.StockQuantity.HasValue)
                return BadRequest(new { message = "StockQuantity là bắt buộc" });

            // Cập nhật số lượng
            product.StockQuantity = request.StockQuantity.Value;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Cập nhật số lượng sản phẩm thành công", stockQuantity = product.StockQuantity });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete]
        public IActionResult DeleteProduct(int productId)
        {
            var product = _context.Products
                .Include(p => p.ProductImages) 
                .Include(p => p.Reviews)       
                .FirstOrDefault(p => p.ProductId == productId);

            if (product == null)
                return NotFound(new { message = "Sản phẩm không tồn tại" });

            if (product.ProductImages.Any())
            {
                _context.ProductImages.RemoveRange(product.ProductImages);
            }

            if (product.Reviews.Any())
            {
                _context.Reviews.RemoveRange(product.Reviews);
            }

            product.Status = "Deleted";
            //_context.Products.Remove(product);
            _context.SaveChanges();

            return Ok(new { message = "Xóa sản phẩm thành công" });
        }
    }
}
