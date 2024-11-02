using System;
using System.Collections.Generic;

namespace LTSMerchWebApp.Models;

public partial class ProductCategory
{
    public int CategoryId { get; set; }

    public string Description { get; set; } = null!;

    public virtual ICollection<ProductOption> ProductOptions { get; set; } = new List<ProductOption>();
}
