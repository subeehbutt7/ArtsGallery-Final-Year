using Microsoft.AspNetCore.Mvc;
using ArtGalleryFinal.Models;
using System.Collections.Generic;
using System.Linq;

namespace ArtGalleryFinal.Controllers
{
    public class SearchController : Controller
    {
        private readonly ArtGalleryContext _context;

        public SearchController(ArtGalleryContext context)
        {
            _context = context;
        }

        // Handles the main search query and categorizes results
        public IActionResult SearchResult(string query)
        {
            // Validate query
            if (string.IsNullOrWhiteSpace(query))
            {
                TempData["Error"] = "Please enter a valid search term.";
                return View("~/Views/Search/SearchResult.cshtml", new SearchModel());
            }

            var lowerCaseQuery = query.ToLower();

            // Categorize results
            var searchResult = new SearchModel
            {
                Query = query,
                Artists = _context.Artists
                    .Where(a => a.Name.ToLower().Contains(lowerCaseQuery))
                    .ToList(),
                Artworks = _context.Artworks
                    .Where(a => a.ArtName.ToLower().Contains(lowerCaseQuery))
                    .ToList(),
                Categories = _context.ArtCategories
                    .Where(c => c.CategoryName.ToLower().Contains(lowerCaseQuery))
                    .ToList()
            };

            // Return the SearchResult view
            return View("~/Views/Search/SearchResult.cshtml", searchResult);
        }

        // Filters artworks by artist
        public IActionResult FilterByArtist(int artistId)
        {
            var artworks = _context.Artworks
                .Where(a => a.ArtistId == artistId && a.AdminApproved == "Yes")
                .ToList();

            return View("~/Views/Artwork/Artworks.cshtml", artworks);
        }

        // Displays details of a specific artwork
        public IActionResult ViewArtwork(int artworkId)
        {
            var artwork = _context.Artworks
                .FirstOrDefault(a => a.ArtId == artworkId);

            if (artwork == null)
            {
                TempData["Error"] = "Artwork not found.";
                return RedirectToAction("SearchResult");
            }

            return View("~/Views/Artwork/Artworks.cshtml", new List<Artwork> { artwork });
        }

        // Filters artworks by category
        public IActionResult FilterByCategory(int categoryId)
        {
            var artworks = _context.Artworks
                .Where(a => a.CategoryId == categoryId && a.AdminApproved == "Yes")
                .ToList();

            return View("~/Views/Artwork/Artworks.cshtml", artworks);
        }
    }
}
