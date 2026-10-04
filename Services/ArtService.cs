using ArtGalleryFinal.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ArtGallery.Services
{
    public class ArtService : IArtService
    {
        private readonly List<Artwork> _artCollection = new List<Artwork>
        {
            new Artwork { ArtId = 1, Title = "Starry Night", ArtName = "Painting", Price = 5000, Status = "Available", ArtistId = 1, CategoryId = 101 },
            new Artwork { ArtId = 2, Title = "Mona Lisa", ArtName = "Portrait", Price = 10000, Status = "Sold", ArtistId = 2, CategoryId = 102 },
            new Artwork { ArtId = 3, Title = "The Scream", ArtName = "Expressionist", Price = 7500, Status = "Available", ArtistId = 1, CategoryId = 103 }
        };

        // Fetch all art items
        public IEnumerable<Artwork> GetAllArts()
        {
            return _artCollection.OrderBy(a => a.ArtId); // Return sorted by ArtId for consistency
        }

        // Fetch art by ID
        public Artwork GetArtById(int id)
        {
            var art = _artCollection.FirstOrDefault(a => a.ArtId == id);
            if (art == null)
            {
                throw new KeyNotFoundException($"Art with ID {id} not found.");
            }
            return art;
        }

        // Fetch art by CategoryId
        public IEnumerable<Artwork> GetArtsByCategoryId(int categoryId)
        {
            return _artCollection.Where(a => a.CategoryId == categoryId).OrderBy(a => a.ArtId);
        }

        // Add a new art item
        public void AddArt(Artwork art)
        {
            if (art == null)
            {
                throw new ArgumentNullException(nameof(art), "Art cannot be null.");
            }

            // Generate unique ID
            art.ArtId = _artCollection.Any() ? _artCollection.Max(a => a.ArtId) + 1 : 1;

            // Add the art item to the collection
            _artCollection.Add(art);
        }

        // Update an existing art item
        public void UpdateArt(Artwork art)
        {
            if (art == null)
            {
                throw new ArgumentNullException(nameof(art), "Art cannot be null.");
            }

            var existingArt = _artCollection.FirstOrDefault(a => a.ArtId == art.ArtId);
            if (existingArt == null)
            {
                throw new KeyNotFoundException($"Art with ID {art.ArtId} not found.");
            }

            existingArt.Title = art.Title;
            existingArt.ArtName = art.ArtName;
            existingArt.Price = art.Price;
            existingArt.Status = art.Status;
            existingArt.Description = art.Description;
            existingArt.ImageUrl = art.ImageUrl;
            existingArt.ArtistId = art.ArtistId;
            existingArt.CategoryId = art.CategoryId; // Ensure CategoryId is updated
        }

        // Delete an art item by ID
        public void DeleteArt(int id)
        {
            var artToRemove = _artCollection.FirstOrDefault(a => a.ArtId == id);
            if (artToRemove == null)
            {
                throw new KeyNotFoundException($"Art with ID {id} not found.");
            }
            _artCollection.Remove(artToRemove);
        }

        // Fetch additional art items for "Explore More"
        public IEnumerable<Artwork> GetMoreArts()
        {
            if (_artCollection.Count <= 2)
            {
                return new List<Artwork>(); // Return an empty collection if no additional items are available
            }

            // Fetch more art items, excluding the first 2
            return _artCollection.Skip(2).OrderBy(a => a.ArtId);
        }

        // Fetch user-specific artworks
        public IEnumerable<Artwork> GetUserArts()
        {
            // Replace this with actual logic to retrieve the current user's ID
            int userId = GetCurrentUserId();

            // Return artworks for the specific user
            return _artCollection.Where(a => a.ArtistId == userId).OrderBy(a => a.ArtId);
        }

        // Dummy method to get the current user's ID
        private int GetCurrentUserId()
        {
            // Replace with logic to fetch the current logged-in user's ID (e.g., from an authentication system)
            return 1; // Example UserId for demonstration
        }
    }
}
