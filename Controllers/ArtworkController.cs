using ArtGalleryFinal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ArtGalleryFinal.Controllers
{
    public class ArtworkController : Controller
    {
        private readonly ArtGalleryContext _dbContext;

        // Constructor to inject the database context
        public ArtworkController(ArtGalleryContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Default action: Show Artwork dashboard or redirect to login
        public IActionResult Index()
        {
            var artistId = HttpContext.Session.GetString("ArtistId");
            if (string.IsNullOrEmpty(artistId))
            {
                TempData["Error"] = "You need to log in first!";
                return RedirectToAction("Login", "Account"); // Redirect to AccountController's Login action
            }

            return RedirectToAction("ManageMyArtworks"); // Redirect to Manage Artworks
        }
        // Gallery page: filter by category / artist and sort by price (low / high)
        public IActionResult Artworks(string? sort, int? categoryId, int? artistId)
        {
            var query = _dbContext.Artworks.Where(a => a.AdminApproved == "Yes");
            if (categoryId != null) query = query.Where(a => a.CategoryId == categoryId);
            if (artistId != null) query = query.Where(a => a.ArtistId == artistId);
            if (sort == "low") query = query.OrderBy(a => a.Price);
            else if (sort == "high") query = query.OrderByDescending(a => a.Price);

            ViewBag.Categories = _dbContext.ArtCategories.ToList();
            ViewBag.Artists = _dbContext.Artists.ToList();

            // Artworks this customer already saved in wishlist (to colour the heart)
            int customerId = int.Parse(HttpContext.Session.GetString("CustomerId") ?? "0");
            ViewBag.WishIds = _dbContext.Wishlists.Where(w => w.CustomerId == customerId).Select(w => w.ArtId).ToList();

            return View("~/Views/Artwork/Artworks.cshtml", query.ToList());
        }

        // 360 degree virtual gallery
        public IActionResult VirtualGallery()
        {
            var list = _dbContext.Artworks.Where(a => a.AdminApproved == "Yes" && a.ImageUrl != null).ToList();
            return View("~/Views/Artwork/VirtualGallery.cshtml", list);
        }

        // Fills the category dropdown of Add / Edit forms from the database
        private void LoadCategories() => ViewBag.Categories = _dbContext.ArtCategories.ToList();


        // Manage Artworks: Display all artworks by the logged-in artist
        public IActionResult ManageMyArtworks()
        {
            var artistId = HttpContext.Session.GetString("ArtistId");
            if (string.IsNullOrEmpty(artistId))
            {
                TempData["Error"] = "You need to log in first!";
                return RedirectToAction("Login", "Account");
            }

            var artworks = _dbContext.Artworks
                .Include(a => a.Category)
                .Where(a => a.ArtistId == Convert.ToInt32(artistId))
                .ToList();

            return View("~/Views/Artwork/ManageMyArtworks.cshtml", artworks);
        }

        // GET: Add Artwork Form
        [HttpGet]
        public IActionResult AddArtwork()
        {
            var artistId = HttpContext.Session.GetString("ArtistId");
            if (string.IsNullOrEmpty(artistId))
            {
                TempData["Error"] = "You need to log in first!";
                return RedirectToAction("Login", "Account");
            }

            LoadCategories();
            return View("~/Views/Artwork/AddArtwork.cshtml", new Artwork()); // Render empty Artwork form
        }

        // POST: Add Artwork Logic
        [HttpPost]

        public IActionResult AddArtwork([FromForm] Artwork artwork, IFormFile ImageUrl)
        {
            try
            {
                if (!ModelState.IsValid) { LoadCategories(); return View(artwork); } // Reload form with errors if validation fails

                // Handle image file upload
                if (ImageUrl != null && ImageUrl.Length > 0)
                {
                    string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                    Directory.CreateDirectory(uploadsFolder); // Ensure directory exists
                    string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageUrl.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        ImageUrl.CopyTo(fileStream);
                    }

                    artwork.ImageUrl = "/images/" + uniqueFileName; // Save the image URL
                }

                artwork.ArtistId = Convert.ToInt32(HttpContext.Session.GetString("ArtistId"));
                artwork.Quantity ??= 1;
                artwork.Status = artwork.Quantity > 0 ? "For Sale" : "Sold"; // status follows quantity
                artwork.AdminApproved = "Pending";

                _dbContext.Artworks.Add(artwork); // Save the artwork to the database or collection
                _dbContext.SaveChanges();
                TempData["Message"] = "Artwork added successfully!";
                return RedirectToAction("ManageMyArtworks");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred: {ex.Message}";
                return RedirectToAction("Error");
            }
        }

        // GET: Edit Artwork Form
        [HttpGet]
        public IActionResult EditArtwork(int id)
        {
            var artistId = HttpContext.Session.GetString("ArtistId");
            if (string.IsNullOrEmpty(artistId))
            {
                TempData["Error"] = "You need to log in first!";
                return RedirectToAction("Login", "Account");
            }

            var artwork = _dbContext.Artworks
                .FirstOrDefault(a => a.ArtId == id && a.ArtistId == Convert.ToInt32(artistId));
            if (artwork == null)
            {
                TempData["Error"] = "Artwork not found or unauthorized access.";
                return RedirectToAction("ManageMyArtworks");
            }

            LoadCategories();
            return View("~/Views/Artwork/EditArtwork.cshtml", artwork); // Render Edit Artwork form
        }

        // POST: Edit Artwork Logic
        [HttpPost]
        [ValidateAntiForgeryToken] // Prevent CSRF attacks
        public IActionResult EditArtwork([FromForm] Artwork artwork, IFormFile ImageUrl)
        {

            {
                // Retrieve the existing artwork from the database
                var existingArtwork = _dbContext.Artworks.FirstOrDefault(a => a.ArtId == artwork.ArtId);
                if (existingArtwork == null)
                {
                    TempData["Error"] = "Artwork not found.";
                    return RedirectToAction("ManageMyArtworks");
                }
                // If art details change (not just quantity), admin must approve again
                bool detailsChanged = existingArtwork.Title != artwork.Title ||
                    existingArtwork.Description != artwork.Description ||
                    existingArtwork.Price != artwork.Price ||
                    existingArtwork.CategoryId != artwork.CategoryId ||
                    existingArtwork.ArtName != artwork.ArtName ||
                    ImageUrl != null; // new image uploaded
                if (detailsChanged)
                {
                    existingArtwork.AdminApproved = "Pending";
                }

                // Quantity decides the status: 0 = Sold (red), more than 0 = For Sale (green)
                existingArtwork.Quantity = artwork.Quantity ?? 0;
                existingArtwork.Status = existingArtwork.Quantity > 0 ? "For Sale" : "Sold";
                // Handle image file upload
                if (ImageUrl != null && ImageUrl.Length > 0)
                {
                    string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                    Directory.CreateDirectory(uploadsFolder); // Ensure directory exists
                    string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageUrl.FileName);
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        ImageUrl.CopyTo(fileStream);
                    }

                    existingArtwork.ImageUrl = "/images/" + uniqueFileName; // Save the image URL
                }
                existingArtwork.Title = artwork.Title;
                existingArtwork.Description = artwork.Description;
                existingArtwork.Price = artwork.Price;
                existingArtwork.CategoryId = artwork.CategoryId;
                existingArtwork.ArtName = artwork.ArtName;

                _dbContext.Artworks.Update(existingArtwork);
                _dbContext.SaveChanges();

                TempData["Message"] = "Artwork updated successfully!";
                return RedirectToAction("ManageMyArtworks");
            }


            TempData["Error"] = "There was an error updating the artwork. Please try again.";
            return View("~/Views/Artwork/EditArtwork.cshtml", artwork); // Redisplay the form with validation errors
        }
        [HttpPost]
        [HttpPost]
        public IActionResult DeleteArtwork(int Id)
        {
            try
            {
                var artistId = HttpContext.Session.GetString("ArtistId");
                if (string.IsNullOrEmpty(artistId))
                {
                    TempData["Error"] = "You need to log in first!";
                    return RedirectToAction("Login", "Account");
                }

                // Check if the artwork is in the OrderDetail table
                var isInOrder = _dbContext.OrderDetails.Any(o => o.ArtId == Id);
                if (isInOrder)
                {
                    TempData["Error"] = "This art is in order process and cannot be deleted.";
                    return RedirectToAction("ManageMyArtworks"); // Redirect to artwork management
                }

                // Retrieve the artwork, ensuring it belongs to the logged-in artist
                var artwork = _dbContext.Artworks.FirstOrDefault(a => a.ArtId == Id && a.ArtistId == Convert.ToInt32(artistId));
                if (artwork == null)
                {
                    TempData["Error"] = "Artwork not found or unauthorized access.";
                    return RedirectToAction("ManageMyArtworks");
                }

                // Delete the artwork
                _dbContext.Artworks.Remove(artwork);
                _dbContext.SaveChanges();

                TempData["Message"] = "Artwork deleted successfully!";
                return RedirectToAction("ManageMyArtworks");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred: {ex.Message}";
                return RedirectToAction("ManageMyArtworks");
            }
        }
        [HttpGet]
        public IActionResult ArtworkDetails(int Id)
        {
            // Fetch the artwork with its associated ratings and customer details
            var artwork = _dbContext.Artworks
                .Include(a => a.Ratings)
                .ThenInclude(r => r.Customer) // Include customer details for reviews
                .Include(a => a.Artist) // Ensure artist details are included
                .Include(a => a.Category) // Ensure category details are included
                .FirstOrDefault(a => a.ArtId == Id);

            // If artwork is not found, return an error
            if (artwork == null)
            {
                TempData["Error"] = "Artwork not found.";
                return RedirectToAction("Artworks", "Artwork");
            }

            // Sold out artworks cannot be opened
            if (artwork.IsSoldOut)
            {
                TempData["Error"] = "This artwork is sold out.";
                return RedirectToAction("Artworks", "Artwork");
            }

            // Calculate the average rating
            double? averageRating = artwork.Ratings.Any()
                ? artwork.Ratings.Average(r => r.RatingValue ?? 0) // Handle potential null RatingValue
                : null;

            // Process ratings to format review details properly
            var reviews = artwork.Ratings.Select(r => new ReviewViewModel
            {
                RatingValue = r.RatingValue ?? 0, // Default to 0 if RatingValue is null
                Review = !string.IsNullOrWhiteSpace(r.Review) ? r.Review : "No review provided.", // Default review text if none provided
                ReviewDate = r.ReviewDate ?? DateTime.MinValue, // Handle null ReviewDate
                CustomerName = _dbContext.Customers
        .Where(c => c.CustomerId == r.CustomerId) // Assuming CustomerId links Ratings to Customers
        .Select(c => c.Name)
        .FirstOrDefault() ?? "Unknown Customer"

            }).ToList();
           

            // Map artwork details and processed reviews to the view model
            var artworkViewModel = new ArtworkViewModel
            {
                ArtId = artwork.ArtId,
                Title = artwork.Title,
                Description = artwork.Description,
                Price = artwork.Price ?? 0,
                AdminApproved = artwork.AdminApproved,
                Status = artwork.Status,
                ArtistName = artwork.Artist?.Name, // Handle missing artist details
                CategoryName = artwork.Category?.CategoryName ?? "None", // Handle missing category details
                ImageUrl = artwork.ImageUrl,
                ArtName = artwork.ArtName,
                Quantity = artwork.Quantity ?? 1,
                IsSoldOut = artwork.IsSoldOut,
                AverageRating = averageRating ?? 0,
                Ratings = reviews, // Assign processed reviews to the view model
            };

            // Return the ArtworkDetails view with the view model
            return View("~/Views/Artwork/ArtworkDetails.cshtml", artworkViewModel);
        }
		public IActionResult ViewArtwork(int id)
		{
            var artwork = _dbContext.Artworks
                .Include(a => a.Category)
                .FirstOrDefault(a => a.ArtId == id);
			if (artwork == null)
			{
				return NotFound();
			}
			return View(artwork);
		}



	}
}

