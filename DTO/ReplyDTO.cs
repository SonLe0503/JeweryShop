namespace JewelryShop.DTO
{
    public class ReplyDTO
    {
        public int ReplyId { get; set; }
        public int ReviewId { get; set; }
        public int UserId { get; set; }
        public string Comment { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }
        public string? Status { get; set; }
    }
    public class ReplyRequest
    {
        public int ReviewId { get; set; }
        public int UserId { get; set; }
        public string Comment { get; set; } = null!;
    }
}
