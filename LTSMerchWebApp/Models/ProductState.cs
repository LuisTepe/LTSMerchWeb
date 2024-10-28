using System;
using System.Collections.Generic;

namespace LTSMerchWebApp.Models;

public partial class ProductState
{
    public int StateId { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<ProductOption> ProductOptions { get; set; } = new List<ProductOption>();
}
