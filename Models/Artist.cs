using System;
using System.Collections.Generic;

namespace ArtGalleryFinal.Models;

public partial class Artist
{
    public int ArtistId { get; set; }

    public string? Name { get; set; }

    public string? Email { get; set; }

    public string? Password { get; set; }

    public string? Country { get; set; }

    public string? Address { get; set; }

    public int? AdminId { get; set; }

    public string? PhoneNo { get; set; }

    public virtual Admin? Admin { get; set; }
    public string? PasswordResetToken { get; set; } // Verification code
    public DateTime? TokenExpiration { get; set; }

    public virtual ICollection<Artwork> Artworks { get; set; } = new List<Artwork>();
}
