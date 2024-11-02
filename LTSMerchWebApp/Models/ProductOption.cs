using System;
using System.Collections.Generic;

namespace LTSMerchWebApp.Models;

public partial class ProductOption
{
    public int ProductOptionId { get; set; }

    public int? ProductId { get; set; }

    public int? SizeId { get; set; }

    public int? ColorId { get; set; }

    public int? Stock { get; set; }

    public int? StateId { get; set; }

    public int? CategoryId { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual ProductCategory? Category { get; set; }

    public virtual Color? Color { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual Product? Product { get; set; }

    public virtual Size? Size { get; set; }

    public virtual ProductState? State { get; set; }
}
