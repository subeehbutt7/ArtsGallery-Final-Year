using System;
using System.Collections.Generic;

namespace ArtGalleryFinal.Models;

public partial class Shipment
{
    public int ShipmentId { get; set; }

    public int? OrderId { get; set; }

    public int? ShipmentCharges { get; set; }

    public string? Address { get; set; }

    public string? Country { get; set; }

    public string? Email { get; set; }

    public string? PhoneNo { get; set; }

    public DateOnly? ShipmentDate { get; set; }

    public int? AdminId { get; set; }

    public virtual Admin? Admin { get; set; }

    public virtual Order? Order { get; set; }
}
