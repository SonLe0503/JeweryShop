namespace JewelryShop.DTO
{
    public class CollectionDTO
    {
        public int CollectionId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string Status { get; set; } = null!;
        public DateTime CreatedAt { get; set; }

        public int ProductCount { get; set; }
        public List<ProductDTO>? Products { get; set; }
    }

    public class CollectionRequest
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
    }
}
