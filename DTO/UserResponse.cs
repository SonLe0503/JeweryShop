namespace JewelryShop.DTO
{
    public class UserResponse
    {
        public int UserId { get; set; }
        public string Email { get; set; } 
        public string? PhoneNumber { get; set; } 
        public string Role { get; set; } 
        public DateTime? CreatedAt { get; set; }
    }
}
