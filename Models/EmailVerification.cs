using System;
using System.Collections.Generic;

namespace JewelryShop.Models;

public partial class EmailVerification
{
    public int VerificationId { get; set; }

    public string Email { get; set; } = null!;

    public string Token { get; set; } = null!;

    public DateTime ExpirationTime { get; set; }

    public DateTime CreatedAt { get; set; }

    public string PasswordHash { get; set; } = null!;

    public string? PhoneNumber { get; set; }
}
