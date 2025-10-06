namespace JewelryShop.DTO
{
    public class ProductRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public decimal? Discount { get; set; }
        public int? StockQuantity { get; set; }
        public string? Material { get; set; }
        public string? ImageUrl { get; set; } // ảnh chính
        public int? CategoryId { get; set; }
    }
    public class AddProductImageRequest
    {
        public string ImageUrl { get; set; } = null!;
    }
}
