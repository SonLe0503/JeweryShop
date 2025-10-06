using System;
using System.Collections.Generic;

namespace JewelryShop.Models;

public partial class PaymentTransaction
{
    public int TransactionId { get; set; }

    public int? OrderId { get; set; }

    public decimal Amount { get; set; }

    public DateTime? PaymentDate { get; set; }

    public string? PaymentStatus { get; set; }

    public virtual Order? Order { get; set; }
}
