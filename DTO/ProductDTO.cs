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
        public string? Status { get; set; }
        public string? Story { get; set; }
        public string? Color { get; set; }
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }

        public int? CollectionId { get; set; }

        public string?  CollectionName { get; set; }

        public List<string>? ProductImages { get; set; }
        public List<ReviewDTO>? Reviews { get; set; }
    }
}
