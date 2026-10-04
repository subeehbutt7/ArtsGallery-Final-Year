using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtGalleryFinal.Models;

public partial class Artwork
{
    public int ArtId { get; set; }

    public string? ArtName { get; set; }

    public int? ArtistId { get; set; }

    public string? Description { get; set; }

    public int? CategoryId { get; set; }

    public string? ImageUrl { get; set; }

    public int? Price { get; set; }

    public string? Status { get; set; }

    public string? Title { get; set; }

    public string? AdminApproved { get; set; }

    // How many copies the artist has (0 = sold out)
    public int? Quantity { get; set; }

    // True when the artwork can no longer be bought
    [NotMapped]
    public bool IsSoldOut => Status == "Sold" || (Quantity ?? 1) <= 0;

    public virtual Artist? Artist { get; set; }

    public virtual ArtCategory? Category { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();
}
