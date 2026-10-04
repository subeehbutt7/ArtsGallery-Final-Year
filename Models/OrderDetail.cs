using System;
using System.Collections.Generic;

namespace ArtGalleryFinal.Models;

public partial class OrderDetail
{
    public int OrderdetailId { get; set; }

    public int? ArtId { get; set; }

    public int? Quantity { get; set; }

    public int? OrderId { get; set; }
    public int? Price { get; set; }

    public virtual Artwork? Art { get; set; }

    public virtual Order? Order { get; set; }
}
