using System;
using System.Collections.Generic;

namespace JewelryShop.Models;

public partial class BotResponse
{
    public int BotResponseId { get; set; }

    public string Keyword { get; set; } = null!;

    public string Response { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }
}
