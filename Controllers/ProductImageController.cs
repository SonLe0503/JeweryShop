using AutoMapper;
using JewelryShop.DTO;
using JewelryShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JewelryShop.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class ProductImageController : Controller
    {
        private readonly JewelryShopContext _context;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _env;

        public ProductImageController(JewelryShopContext context, IMapper mapper, IWebHostEnvironment env)
        {
            _context = context;
            _mapper = mapper;
            _env = env;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var images = _context.ProductImages.Include(p => p.Product).ToList();
            var result = _mapper.Map<List<ProductImageDTO>>(images);
            return Ok(result);
        }

        [HttpGet("{productId}")]
        public IActionResult GetByProductId(int productId)
        {
            var images = _context.ProductImages
                .Where(pi => pi.ProductId == productId)
                .ToList();
            if(!images.Any())
            {
                return NotFound(new { message = "Không tìm thấy ảnh cho sản phẩm này " });
            }
            var result = _mapper.Map<List<ProductImageDTO>>(images);
            return Ok(result);
        }

        [HttpPost]
        public IActionResult UploadImage(int productId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Không có file được chọn." });

            var product = _context.Products.FirstOrDefault(p => p.ProductId == productId);
            if (product == null)
                return NotFound(new { message = "Sản phẩm không tồn tại." });

            // Thư mục lưu ảnh
            string uploadDir = Path.Combine(_env.WebRootPath, "uploads", "products");
            if (!Directory.Exists(uploadDir))
                Directory.CreateDirectory(uploadDir);

            // Tạo tên file duy nhất
            string fileName = $"{Guid.NewGuid()}_{file.FileName}";
            string filePath = Path.Combine(uploadDir, fileName);

            // Lưu file lên server
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            // Đường dẫn public (frontend có thể truy cập)
            string imageUrl = $"/uploads/products/{fileName}";

            // Lưu vào DB
            var productImage = new ProductImage
            {
                ProductId = productId,
                ImageUrl = imageUrl
            };
            _context.ProductImages.Add(productImage);
            _context.SaveChanges();

            return Ok(new
            {
                message = "Upload ảnh thành công",
                imageUrl
            });
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] UpdateProductImageDTO dto)
        {
            var image = _context.ProductImages.FirstOrDefault(i => i.ProductImageId == id);
            if (image == null)
                return NotFound(new { message = "Không tìm thấy ảnh." });

            _mapper.Map(dto, image);
            _context.SaveChanges();

            var result = _mapper.Map<ProductImageDTO>(image);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var image = _context.ProductImages.FirstOrDefault(i => i.ProductImageId == id);
            if (image == null)
                return NotFound(new { message = "Không tìm thấy ảnh." });

            _context.ProductImages.Remove(image);
            _context.SaveChanges();
            return Ok(new { message = "Xóa ảnh sản phẩm thành công." });
        }
    }
}
