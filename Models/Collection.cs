using System;
using System.Collections.Generic;

namespace JewelryShop.Models;

public partial class Collection
{
    public int CollectionId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
