using ArtGalleryFinal.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ArtGalleryFinal.Controllers
{
    public class ReviewController : Controller
    {
        private readonly ArtGalleryContext _dbContext;
        private readonly ILogger<ReviewController> _logger;

        public ReviewController(ArtGalleryContext dbContext, ILogger<ReviewController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult AddReview(int Id)
        {
            // Retrieve CustomerId from session
            var customerIdString = HttpContext.Session.GetString("CustomerId");

            if (string.IsNullOrEmpty(customerIdString))
            {
                TempData["Error"] = "You need to log in to add a review.";
                return RedirectToAction("Login", "Account");
            }

            // Convert CustomerId to integer
            int customerId = int.Parse(customerIdString);

            // Fetch the order and associated artwork details
            var order = _dbContext.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Art)
                .Include(o => o.Customer)
                .FirstOrDefault(o => o.OrderId == Id && o.CustomerId == customerId); // Ensure order belongs to the logged-in customer

            if (order == null)
            {
                _logger.LogWarning("Order object is null.");
                TempData["Error"] = "Order not found or does not belong to you.";
                return RedirectToAction("Orders", "Order");
            }

            if (order.Customer == null)
            {
                _logger.LogWarning("Customer object is null for the given order.");
                TempData["Error"] = "Customer details are not available for this order.";
                return RedirectToAction("Orders", "Order");
            }

            // Assume one artwork per order for simplicity
            var orderDetail = order.OrderDetails.FirstOrDefault();
            var artwork = orderDetail?.Art;

            if (artwork == null)
            {
                _logger.LogWarning("Artwork object is null.");
                TempData["Error"] = "Artwork not found or not associated with this order.";
                return RedirectToAction("Orders", "Order");
            }

            // Populate the ReviewViewModel
            var reviewViewModel = new ReviewViewModel
            {
                ArtId = artwork.ArtId,
                Title = artwork.Title,
                CustomerId = order.Customer.CustomerId,
                CustomerName = order.Customer.Name ?? "Anonymous",
                OrderId = Id
            };

            return View(reviewViewModel); // Render AddReview.cshtml
        }





        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SubmitReview(ReviewViewModel reviewViewModel)
        {
            try
            {
                // Save to database
                var rating = new Rating
                {
                    ArtId = reviewViewModel.ArtId,
                    OrderId = reviewViewModel.OrderId,
                    CustomerId = int.Parse(HttpContext.Session.GetString("CustomerId")),
                    RatingValue = reviewViewModel.RatingValue,
                    Review = reviewViewModel.Review,
                    ReviewDate = reviewViewModel.ReviewDate ?? DateTime.Now
                };

                _dbContext.Ratings.Add(rating);
                _dbContext.SaveChanges();

                TempData["Success"] = "Review submitted successfully!";
                return RedirectToAction("ArtworkDetails", "Artwork", new { id = reviewViewModel.ArtId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while submitting review.");
                TempData["Error"] = "An unexpected error occurred.";
                return View("AddReview", reviewViewModel);
            }
        }

    }
}
