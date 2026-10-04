using System;
using System.Collections.Generic;

namespace ArtGalleryFinal.Models;

public partial class ArtCategory
{
    public int CategoryId { get; set; }

    public string? CategoryName { get; set; }

    public virtual ICollection<Artwork> Artworks { get; set; } = new List<Artwork>();
}
