using System;
using System.Collections.Generic;

namespace LTSMerchWebApp.Models;

public partial class RoleType
{
    public int RoleTypeId { get; set; }

    public string RoleName { get; set; } = null!;

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
