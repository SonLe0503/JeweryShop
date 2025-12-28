using AutoMapper;
using JewelryShop.DTO;
using JewelryShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JewelryShop.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CollectionController : Controller
    {
        private readonly JewelryShopContext _context;
        private readonly IMapper _mapper;

        public CollectionController(JewelryShopContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/Collection
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CollectionDTO>>> GetCollections()
        {
            var collections = await _context.Collections.Include(c => c.Products).ToListAsync();
            return Ok(_mapper.Map<List<CollectionDTO>>(collections));
        }

        // GET: api/Collection/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CollectionDTO>> GetCollection(int id)
        {
            var collection = await _context.Collections.Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.CollectionId == id);

            if (collection == null)
                return NotFound();

            return Ok(_mapper.Map<CollectionDTO>(collection));
        }

        // POST: api/Collection
        [HttpPost]
        public async Task<ActionResult<CollectionDTO>> CreateCollection(CollectionRequest request)
        {
            var collection = _mapper.Map<Collection>(request);
            _context.Collections.Add(collection);
            await _context.SaveChangesAsync();

            var collectionDto = _mapper.Map<CollectionDTO>(collection);

            return CreatedAtAction(nameof(GetCollection), new { id = collection.CollectionId }, collectionDto);
        }

        // PUT: api/Collection/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCollection(int id, CollectionRequest request)
        {
            var collection = await _context.Collections.FindAsync(id);
            if (collection == null)
                return NotFound();

            _mapper.Map(request, collection);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Collection/5
        [HttpDelete("{id}")]
        public IActionResult DeleteCollection(int id)
        {
            var collection = _context.Collections
                .Include(c => c.Products)
                .FirstOrDefault(c => c.CollectionId == id);

            if (collection == null)
                return NotFound(new { message = "BST không tồn tại" });

            if (collection.Products.Any())
                return BadRequest(new { message = "Không thể xóa BST vì vẫn còn sản phẩm trong danh mục này" });

            //collection.Status = "Deleted";
            _context.Remove(collection);
            _context.SaveChanges();

            return Ok(new { message = "Xóa BST thành công" });
        }
    }
}
