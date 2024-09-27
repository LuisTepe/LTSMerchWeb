using System;
using System.Collections.Generic;

namespace LTSMerchWebApp.Models;

public partial class OrderStatusType
{
    public int StatusTypeId { get; set; }

    public string StatusName { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
