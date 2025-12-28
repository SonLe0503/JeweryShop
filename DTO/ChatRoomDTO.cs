namespace JewelryShop.DTO
{
    public class ChatRoomDTO
    {
        public int ChatRoomId { get; set; }

        public int UserId { get; set; }

        public int? AdminId { get; set; }

        public int? OrderId { get; set; }

        public DateTime? CreatedAt { get; set; }

        public string? Status { get; set; }
    }
    public class CreateRoomDTO
    {
        public int UserId { get; set; }
        public int? OrderId { get; set; }
    }

}
