using System;
using System.Collections.Generic;

namespace LTSMerchWebApp.Models;

public partial class Color
{
    public int ColorId { get; set; }

    public string ColorName { get; set; } = null!;

    public virtual ICollection<ProductOption> ProductOptions { get; set; } = new List<ProductOption>();
}
