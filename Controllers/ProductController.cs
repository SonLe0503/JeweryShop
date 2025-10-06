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

        public ProductController(JewelryShopContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult GetAllProducts()
        {
            var products = _context.Products
                .Include(p => p.Category) // nếu bạn muốn lấy luôn thông tin Category
                .ToList();

            var result = _mapper.Map<List<ProductDTO>>(products);

            return Ok(result);
        }

        [HttpGet("{id}")]
        public IActionResult GetProductById(int id)
        {
            var product = _context.Products
                .Include(p => p.Category)
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
        public IActionResult CreateProduct([FromBody] ProductRequest request )
        {
            if (request.CategoryId.HasValue && !_context.Categories.Any(c => c.CategoryId == request.CategoryId))
            {
                return BadRequest(new { message = "Danh mục không tồn tại" });
            }

            if (string.IsNullOrEmpty(request.Name) || !request.Price.HasValue)
                return BadRequest(new { message = "Name và Price là bắt buộc" });

            var product = new Product
            {
                Name = request.Name,
                Description = request.Description,
                Price = request.Price.Value,
                Discount = request.Discount,
                StockQuantity = request.StockQuantity,
                Material = request.Material,
                ImageUrl = request.ImageUrl,
                CategoryId = request.CategoryId,
                CreatedAt = DateTime.Now
            };

            _context.Products.Add(product);
            _context.SaveChanges();
            var productDto = _mapper.Map<ProductDTO>(product);
            return Ok(new {message = "Tạo sản phẩm thành công", product = productDto });
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult AddProductImage(int productId, [FromBody] AddProductImageRequest request)
        {
            var product = _context.Products.FirstOrDefault(p => p.ProductId == productId);
            if (product == null)
            {
                return NotFound(new { message = "Sản phẩm không tồn tại" });
            }

            var productImage = new ProductImage
            {
                ProductId = productId,
                ImageUrl = request.ImageUrl
            };

            _context.ProductImages.Add(productImage);
            _context.SaveChanges();

            return Ok(new { message = "Thêm ảnh thành công", imageUrl = request.ImageUrl });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut]
        public IActionResult EditProduct(int productId, [FromBody] ProductRequest request)
        {
            var product = _context.Products.FirstOrDefault(p => p.ProductId == productId);
            if (product == null)
                return NotFound(new { message = "Sản phẩm không tồn tại" });

            if (!string.IsNullOrEmpty(request.Name)) product.Name = request.Name;
            if (!string.IsNullOrEmpty(request.Description)) product.Description = request.Description;
            if (request.Price.HasValue) product.Price = request.Price.Value;
            if (request.Discount.HasValue) product.Discount = request.Discount.Value;
            if (request.StockQuantity.HasValue) product.StockQuantity = request.StockQuantity.Value;
            if (!string.IsNullOrEmpty(request.Material)) product.Material = request.Material;
            if (!string.IsNullOrEmpty(request.ImageUrl)) product.ImageUrl = request.ImageUrl;
            if (request.CategoryId.HasValue) product.CategoryId = request.CategoryId;

            _context.SaveChanges();

            var productDto = _mapper.Map<ProductDTO>(product);
            return Ok(new { message = "Cập nhật sản phẩm thành công", product = productDto });
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

            _context.Products.Remove(product);
            _context.SaveChanges();

            return Ok(new { message = "Xóa sản phẩm thành công" });
        }
    }
}
