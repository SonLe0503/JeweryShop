namespace JewelryShop.DTO
{
    public class OrderDTO
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }

        public string? PaymentMethod { get; set; }

        public string? ShippingAddress { get; set; }

        public List<OrderDetailDTO> OrderDetails { get; set; } = new();
    }

    public class CreateOrderDTO
    {
        public int UserId { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }

        public string? PaymentMethod { get; set; }

        public string? ShippingAddress { get; set; }
        public List<CreateOrderDetailDTO> OrderDetails { get; set; } = new();
    }

    public class UpdateOrderStatusDTO
    {
        public string Status { get; set; }
    }
}
