﻿using System;
using System.Collections.Generic;

namespace LTSMerchWebApp.Models;

public partial class Faq
{
    public int FaqId { get; set; }

    public string Question { get; set; } = null!;

    public string Answer { get; set; } = null!;
}
