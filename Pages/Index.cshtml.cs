using ArtGalleryFinal.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace ArtGalleryFinal.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ArtGalleryContext _dbContext;
        private readonly ILogger<IndexModel> _logger;

        // Single constructor to inject both the database context and logger
        public IndexModel(ArtGalleryContext dbContext, ILogger<IndexModel> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public List<Artwork> LatestArtworks { get; set; }

        public void OnGet()
        {
            try
            {
                // Fetch the latest 6 artworks ordered by ArtId in descending order
                LatestArtworks = _dbContext.Artworks
                    .Where(a=>a.AdminApproved=="Yes")
                    .OrderByDescending(a => a.ArtId) // Replace this with CreatedAt if you use timestamps
                    .Take(6) // Fetch the top 6 records
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while fetching the latest artworks: {ex.Message}");
                LatestArtworks = new List<Artwork>(); // Initialize with an empty list in case of error
            }
        }
    }
}
