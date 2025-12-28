using System;
using System.Collections.Generic;

namespace JewelryShop.Models;

public partial class Message
{
    public int MessageId { get; set; }

    public int ChatRoomId { get; set; }

    public int? SenderId { get; set; }

    public string MessageText { get; set; } = null!;

    public bool? IsBot { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? Status { get; set; }

    public virtual ChatRoom ChatRoom { get; set; } = null!;

    public virtual User? Sender { get; set; }
}
