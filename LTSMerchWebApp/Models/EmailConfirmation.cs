using System;
using System.Collections.Generic;

namespace LTSMerchWebApp.Models;

public partial class EmailConfirmation
{
    public int ConfirmationId { get; set; }

    public int? UserId { get; set; }

    public string Token { get; set; } = null!;

    public bool? IsConfirmed { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual User? User { get; set; }
}
