namespace ArtGalleryFinal.Models
{
    public class ViewOrdersModel
    {
        public string Role { get; set; } // Admin, Artist, Customer
        public IEnumerable<OrderViewModel> Orders { get; set; }
    }
}
