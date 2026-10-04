namespace ArtGalleryFinal.Models
{
    public class CartItemViewModel
    {
        public int ArtId { get; set; }
        public string Title { get; set; }
        public string ImageUrl { get; set; }
        public int Price { get; set; }
        public int Quantity { get; set; }
        public int TotalPrice
        {
            get { return Price * Quantity; }
        }
        public Artwork Artwork { get; set; }
    }
}
