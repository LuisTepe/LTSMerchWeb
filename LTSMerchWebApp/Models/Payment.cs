using System;
using System.Collections.Generic;

namespace LTSMerchWebApp.Models;

public partial class Payment
{
    public int PaymentId { get; set; }

    public int? OrderId { get; set; }

    public int? PaymentMethodId { get; set; }

    public decimal Amount { get; set; }

    public int? PaymentStatusTypeId { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? UserId { get; set; }

    public string? VoucherPath { get; set; }

    public virtual Order? Order { get; set; }

    public virtual PaymentMethod? PaymentMethod { get; set; }

    public virtual PaymentStatusType? PaymentStatusType { get; set; }

    public virtual User? User { get; set; }
}
