using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtGalleryFinal.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public int? CustomerId { get; set; }
    public string? OrderStatus { get; set; }

    public int? OrderAmount { get; set; }

    public int? Discount { get; set; }

    public DateOnly? OrderDate { get; set; }

    public int? AdminId { get; set; }

    public virtual Admin? Admin { get; set; }

    public virtual Customer? Customer { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();

    // Human-friendly, trackable order number generated from the OrderId - no database change needed.
    [NotMapped]
    public string OrderNumber => $"ORD-{OrderId:D6}";
}
