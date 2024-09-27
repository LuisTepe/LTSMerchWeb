using System;
using System.Collections.Generic;

namespace LTSMerchWebApp.Models;

public partial class CartItem
{
    public int CartItemId { get; set; }

    public int? CartId { get; set; }

    public int? ProductOptionId { get; set; }

    public int Quantity { get; set; }

    public virtual Cart? Cart { get; set; }

    public virtual ProductOption? ProductOption { get; set; }
}
