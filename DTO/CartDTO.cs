namespace JewelryShop.DTO
{
    public class CartDTO
    {
        public int CartId { get; set; }
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string CategoryName { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public string ImgUrl { get; set; }

    }

    public class AddToCartRequest
    {
        public int ProductId { get; set; }
        public int UserId { get; set; }
        public int Quantity { get; set; }
    }

    public class UpdateCartQuantityRequest
    {
        public int CartId { get; set; }
        public int Quantity { get; set; }
    }
}
