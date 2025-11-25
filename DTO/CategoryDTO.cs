namespace JewelryShop.DTO
{
    public class CategoryDTO
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } 
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public string ? Status { get; set; }
        public int ProductCount { get; set; } // Số lượng sản phẩm trong danh mục


        public List<ProductDTO>? Products { get; set; } // Danh sách sản phẩm thuộc danh mục
    }
    public class CategoryRequest
    {
        public string CategoryName { get; set; } 
        public string? Description { get; set; }
    }
}
