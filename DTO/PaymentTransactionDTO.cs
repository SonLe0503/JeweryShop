namespace JewelryShop.DTO
{
    public class PaymentTransactionDTO
    {
        public int TransactionId { get; set; }
        public int? OrderId { get; set; }
        public decimal Amount { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string? PaymentStatus { get; set; }

        public decimal? OrderTotalPrice { get; set; } // Thông tin tổng giá trị đơn hàng
        public string? OrderStatus { get; set; } // Thông tin trạng thái đơn hàng
    }
    public class CreatePaymentTransactionDTO
    {
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string? PaymentStatus { get; set; }
    }
}
