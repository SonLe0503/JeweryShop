using System;
using System.Collections.Generic;

namespace JewelryShop.Models;

public partial class ChatRoom
{
    public int ChatRoomId { get; set; }

    public int UserId { get; set; }

    public int? AdminId { get; set; }

    public int? OrderId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? Status { get; set; }

    public virtual User? Admin { get; set; }

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual Order? Order { get; set; }

    public virtual User User { get; set; } = null!;
}
