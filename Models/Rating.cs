using System;
using System.Collections.Generic;

namespace ArtGalleryFinal.Models;

public partial class Rating
{
    public int RatingId { get; set; }

    public int? CustomerId { get; set; }
    public int? OrderId { get; set; }

    public int? ArtId { get; set; }

    public int? RatingValue { get; set; }

    public string? Review { get; set; }
    public DateTime? ReviewDate { get; set; }

    public virtual Artwork? Art { get; set; }

    public virtual Customer? Customer { get; set; }
}
