namespace ArtGalleryFinal.Models
{
    public class ReviewViewModel
    {
        public int ArtId { get; set; }
        public int OrderId { get; set; }
        public string CustomerName { get; set; }
        public int RatingValue { get; set; }
		public int CustomerId { get; set; }
        public string Title { get; set; }
		public string? Review { get; set; }
		public DateTime? ReviewDate { get; set; }

		
	}
}
