using System;
using System.Collections.Generic;

namespace JewelryShop.Models;

public partial class Reply
{
    public int ReplyId { get; set; }

    public int ReviewId { get; set; }

    public int UserId { get; set; }

    public string Comment { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public string? Status { get; set; }

    public virtual Review Review { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
