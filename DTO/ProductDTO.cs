namespace JewelryShop.DTO
{
    public class ProductDTO
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public decimal? Discount { get; set; }
        public int? StockQuantity { get; set; }
        public string? Material { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime? CreatedAt { get; set; }

        // Thông tin category (nếu cần)
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }

        // Optional: chỉ fill khi lấy chi tiết
        public List<string>? ProductImages { get; set; }
        public List<ReviewDTO>? Reviews { get; set; }
    }
}
