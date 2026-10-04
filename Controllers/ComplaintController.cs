using ArtGalleryFinal.Models;
using Microsoft.AspNetCore.Mvc;

namespace ArtGalleryFinal.Controllers
{
    public class ComplaintController : Controller
    {
        private readonly ArtGalleryContext _db;

        public ComplaintController(ArtGalleryContext db)
        {
            _db = db;
        }

        // Complaint form (customer must be logged in)
        [HttpGet]
        public IActionResult Create(int? artId)
        {
            if (HttpContext.Session.GetString("CustomerId") == null)
            {
                TempData["Error"] = "Please log in as a customer to send a complaint.";
                return RedirectToAction("Login", "Account");
            }
            ViewBag.ArtId = artId;
            return View();
        }

        // Save the complaint
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(int? artId, string? message)
        {
            var cid = HttpContext.Session.GetString("CustomerId");
            if (cid == null) return RedirectToAction("Login", "Account");

            if (string.IsNullOrWhiteSpace(message))
            {
                TempData["Error"] = "Please write your complaint.";
                return RedirectToAction("Create", new { artId });
            }

            _db.Complaints.Add(new Complaint { CustomerId = int.Parse(cid), ArtId = artId, Message = message.Trim() });
            _db.SaveChanges();
            TempData["Message"] = "Your complaint was sent. Thank you!";
            return RedirectToAction("Artworks", "Artwork");
        }
    }
}
