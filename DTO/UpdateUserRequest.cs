namespace JewelryShop.DTO
{
    public class UpdateUserRequest
    {
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Password { get; set; }  
        public string? OldPassword { get; set; } 
        public string? Role { get; set; }
    }
}
