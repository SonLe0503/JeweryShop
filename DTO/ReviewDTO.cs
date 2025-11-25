namespace JewelryShop.DTO
{
    public class ReviewDTO
    {
        public int ReviewId { get; set; }
        
        public int UserId { get; set; }
        public string? UserEmail { get; set; }
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? Comment { get; set; }
        public int Rating { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? Status { get; set; }
    }
    public class CreateReviewRequest
    {
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }
}
