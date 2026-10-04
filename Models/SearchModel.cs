namespace ArtGalleryFinal.Models
{
    public class SearchModel
    {
        // Search query input from the user
        public string Query { get; set; }

        // Results for Artists matching the query
        public List<Artist> Artists { get; set; }

        // Results for Artworks matching the query
        public List<Artwork> Artworks { get; set; }

        // Results for Categories matching the query
        public List<ArtCategory> Categories { get; set; }

        // Constructor to initialize lists to avoid null references
        public SearchModel()
        {
            Artists = new List<Artist>();
            Artworks = new List<Artwork>();
            Categories = new List<ArtCategory>();
        }
    }
}
