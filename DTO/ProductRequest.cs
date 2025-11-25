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
        public string? Story { get; set; }
        public string? Color { get; set; }
        public int? CategoryId { get; set; }

        public int? CollectionId { get; set; }
        public IFormFile? ImageFile { get; set; }
    }
    public class UpdateStockRequest
    {
        public int? StockQuantity { get; set; }
    }
}
