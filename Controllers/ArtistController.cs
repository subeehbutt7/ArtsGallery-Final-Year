using ArtGalleryFinal.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArtGalleryFinal.Controllers
{
    public class ArtistController : Controller
    {
        private readonly ArtGalleryContext _dbcontext; // DbContext for database operations

        // Constructor for dependency injection
        public ArtistController(ArtGalleryContext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        // Artist Dashboard
        public IActionResult Index()
        {
            return View();
        }

        // GET: Artist Profile
        public IActionResult Profile()
        {
            var artistId = HttpContext.Session.GetString("ArtistId");
            if (string.IsNullOrEmpty(artistId))
            {
                TempData["Error"] = "You need to log in first!";
                return RedirectToAction("Login", "Account"); // Redirect to the login page
            }

            var artist = _dbcontext.Artists.FirstOrDefault(a => a.ArtistId == int.Parse(artistId));
            if (artist == null)
            {
                TempData["Error"] = "Artist not found.";
                return RedirectToAction("Index"); // Redirect to Artist Panel or Home
            }

            return View("~/Views/ArtistFunctions/Profile.cshtml", artist);
        }

        // GET: Edit Artist Profile
        [HttpGet]
        public IActionResult EditProfile()
        {
            var artistId = HttpContext.Session.GetString("ArtistId");
            if (string.IsNullOrEmpty(artistId))
            {
                TempData["Error"] = "You need to log in first!";
                return RedirectToAction("Login", "Account"); // Redirect to the login page
            }

            var artist = _dbcontext.Artists.FirstOrDefault(a => a.ArtistId == int.Parse(artistId));
            if (artist == null)
            {
                TempData["Error"] = "Artist not found.";
                return RedirectToAction("Index"); // Redirect to Artist Panel or Home
            }

            return View("~/Views/ArtistFunctions/EditProfile.cshtml", artist);
        }

        // POST: Update Artist Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditProfile(Artist updatedArtist)
        {
            var artistId = HttpContext.Session.GetString("ArtistId");
            if (string.IsNullOrEmpty(artistId))
            {
                TempData["Error"] = "You need to log in first!";
                return RedirectToAction("Login", "Account"); // Redirect to the login page
            }

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "There was an error updating your profile. Please ensure all fields are correctly filled.";
                return View("~/Views/ArtistFunctions/EditProfile.cshtml", updatedArtist);
            }

            var existingArtist = _dbcontext.Artists.FirstOrDefault(a => a.ArtistId == int.Parse(artistId));
            if (existingArtist == null)
            {
                TempData["Error"] = "Artist not found.";
                return RedirectToAction("Index"); // Redirect to Artist Panel or Home
            }

            // Update the artist's information
            existingArtist.Name = updatedArtist.Name;
            existingArtist.Address = updatedArtist.Address;
            existingArtist.Country = updatedArtist.Country;
            existingArtist.PhoneNo = updatedArtist.PhoneNo;

            _dbcontext.SaveChanges();

            TempData["Message"] = "Profile updated successfully!";
            return RedirectToAction("Profile"); // Redirect back to the profile view
        }
    }
}
