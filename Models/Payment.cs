using System;
using System.Collections.Generic;

namespace ArtGalleryFinal.Models;

public partial class Payment
{
    public int PaymentId { get; set; }

    public int? OrderId { get; set; }

    public DateOnly? PaymentDate { get; set; }

    public int? Amount { get; set; }

    public string? PaymentStatus { get; set; }

    public string? PaymentMethod { get; set; }

    public int? AdminId { get; set; }

    public virtual Admin? Admin { get; set; }

    public virtual Order? Order { get; set; }
}
