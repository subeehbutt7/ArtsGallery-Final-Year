using ArtGalleryFinal.Models;
using Microsoft.AspNetCore.Mvc;

namespace ArtGalleryFinal.Controllers
{
    public class WishlistController : Controller
    {
        private readonly ArtGalleryContext _db;

        public WishlistController(ArtGalleryContext db)
        {
            _db = db;
        }

        // Show the logged-in customer's wishlist
        public IActionResult Index()
        {
            var id = HttpContext.Session.GetString("CustomerId");
            if (id == null)
            {
                TempData["Error"] = "Please log in as a customer to see your wishlist.";
                return RedirectToAction("Login", "Account");
            }
            int customerId = int.Parse(id);
            var artworks = _db.Wishlists
                .Where(w => w.CustomerId == customerId)
                .Join(_db.Artworks, w => w.ArtId, a => a.ArtId, (w, a) => a)
                .ToList();
            return View(artworks);
        }

        // Heart button: add to wishlist, or remove if already there
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Toggle(int id, string? back)
        {
            var cid = HttpContext.Session.GetString("CustomerId");
            if (cid == null)
            {
                TempData["Error"] = "Please log in as a customer to use the wishlist.";
                return RedirectToAction("Login", "Account");
            }
            int customerId = int.Parse(cid);

            var item = _db.Wishlists.FirstOrDefault(w => w.CustomerId == customerId && w.ArtId == id);
            if (item == null)
            {
                _db.Wishlists.Add(new Wishlist { CustomerId = customerId, ArtId = id });
                TempData["Message"] = "Added to your wishlist.";
            }
            else
            {
                _db.Wishlists.Remove(item);
                TempData["Message"] = "Removed from your wishlist.";
            }
            _db.SaveChanges();

            // Go back to the page the customer was on
            if (!string.IsNullOrEmpty(back) && Url.IsLocalUrl(back)) return LocalRedirect(back);
            return RedirectToAction("Artworks", "Artwork");
        }
    }
}
