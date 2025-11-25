namespace JewelryShop.DTO
{
    public class ProductImageDTO
    {
        public int ProductImageId { get; set; }
        public int ProductId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
    }

    public class CreateProductImageDTO
    {
        public int ProductId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
    }

    public class UpdateProductImageDTO
    {
        public string ImageUrl { get; set; } = string.Empty;
    }
}
