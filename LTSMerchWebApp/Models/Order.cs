using System;
using System.Collections.Generic;

namespace LTSMerchWebApp.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public int? UserId { get; set; }

    public decimal Total { get; set; }

    public string? ShippingAddress { get; set; }

    public int? StatusTypeId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual OrderStatusType? StatusType { get; set; }

    public virtual User? User { get; set; }
}
