namespace ArtGalleryFinal.Models
{
    public class ArtworkViewModel
    {
        public int ArtId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Price { get; set; }
        public string AdminApproved { get; set; }
        public string Status { get; set; }
        public string ArtistName { get; set; }
        public string CustomerName { get; set; }
        public string CategoryName { get; set; }

        public string? ImageUrl { get; set; }
     
        public string? ArtName { get; set; }
        public int Quantity { get; set; }
        public bool IsSoldOut { get; set; }

        public double? AverageRating { get; set; }
        public List<ReviewViewModel>? Ratings { get; set; }


	}
}
