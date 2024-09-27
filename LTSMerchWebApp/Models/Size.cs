using System;
using System.Collections.Generic;

namespace LTSMerchWebApp.Models;

public partial class Size
{
    public int SizeId { get; set; }

    public string SizeName { get; set; } = null!;

    public virtual ICollection<ProductOption> ProductOptions { get; set; } = new List<ProductOption>();
}
