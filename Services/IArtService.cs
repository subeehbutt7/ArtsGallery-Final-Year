using ArtGalleryFinal.Models;
using System.Collections.Generic;

namespace ArtGallery.Services
{
    public interface IArtService
    {
        IEnumerable<Artwork> GetAllArts(); // Retrieve all art items
        Artwork GetArtById(int id); // Get a specific art by ID
        void AddArt(Artwork art); // Add a new art item
        void UpdateArt(Artwork art); // Update an existing art item
        void DeleteArt(int id); // Delete an art item by ID

        IEnumerable<Artwork> GetMoreArts(); // Fetch additional art items
        IEnumerable<Artwork> GetUserArts(); // Retrieve all art items owned by a specific user

        // New method to fetch art items by CategoryId
        IEnumerable<Artwork> GetArtsByCategoryId(int categoryId); // Retrieve all art items by category
    }
}
