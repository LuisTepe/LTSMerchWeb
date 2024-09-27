using System;
using System.Collections.Generic;

namespace LTSMerchWebApp.Models;

public partial class PaymentStatusType
{
    public int PaymentStatusTypeId { get; set; }

    public string StatusName { get; set; } = null!;

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
