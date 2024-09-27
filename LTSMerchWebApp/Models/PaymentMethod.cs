using System;
using System.Collections.Generic;

namespace LTSMerchWebApp.Models;

public partial class PaymentMethod
{
    public int PaymentMethodId { get; set; }

    public string MethodName { get; set; } = null!;

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
