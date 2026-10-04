namespace ArtGalleryFinal.Models;

// One row = one customer saved one artwork
public class Wishlist
{
    public int WishlistId { get; set; }
    public int CustomerId { get; set; }
    public int ArtId { get; set; }
}
