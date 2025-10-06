namespace JewelryShop.DTO
{
    public class ReviewDTO
    {
        public int ReviewId { get; set; }
        public string? UserEmail { get; set; } // lấy từ User.Email
        public string? Comment { get; set; }
        public int? Rating { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
