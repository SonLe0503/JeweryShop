namespace JewelryShop.DTO
{
    public class MessageDTO
    {
        public int MessageId { get; set; }

        public int ChatRoomId { get; set; }

        public int? SenderId { get; set; }

        public string MessageText { get; set; } = null!;

        public bool? IsBot { get; set; }

        public DateTime? CreatedAt { get; set; }

        public string? Status { get; set; }
    }

    public class SendMessageDTO
    {
        public int ChatRoomId { get; set; }
        public int SenderId { get; set; }
        public string MessageText { get; set; } = string.Empty;
    }
}
