using Microsoft.AspNetCore.Mvc;
using ArtGalleryFinal.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace ArtGalleryFinal.Controllers
{
    public class AdminController : Controller
    {
        private readonly ArtGalleryContext _context;

        // Constructor for dependency injection
        public AdminController(ArtGalleryContext context)
        {
            _context = context;
        }

        // GET: Admin Dashboard
        public IActionResult Index()
        {
            return View("~/Views/AdminFunctions/Artworks.cshtml");
        }

        // GET: All complaints sent by customers
        public IActionResult Complaints()
        {
            if (HttpContext.Session.GetString("AdminId") == null)
            {
                TempData["Error"] = "You must log in as an admin!";
                return RedirectToAction("Login", "Account");
            }
            ViewBag.Names = _context.Customers.ToDictionary(c => c.CustomerId, c => c.Name);
            return View("~/Views/AdminFunctions/Complaints.cshtml", _context.Complaints.OrderByDescending(c => c.CreatedDate).ToList());
        }

        // GET: Sales Dashboard (Graph)
        public IActionResult SalesDashboard()
        {
            if (HttpContext.Session.GetString("AdminId") == null)
            {
                TempData["Error"] = "You must log in as an admin!";
                return RedirectToAction("Login", "Account");
            }

            var orders = _context.Orders
                .Where(o => o.OrderDate != null)
                .ToList();

            var monthlySales = orders
                .GroupBy(o => new { o.OrderDate.Value.Year, o.OrderDate.Value.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new
                {
                    Label = new DateOnly(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                    Total = g.Sum(o => o.OrderAmount ?? 0)
                })
                .ToList();

            var model = new SalesChartViewModel
            {
                Months = monthlySales.Select(m => m.Label).ToList(),
                SalesAmounts = monthlySales.Select(m => m.Total).ToList(),
                TotalSales = orders.Sum(o => o.OrderAmount ?? 0),
                TotalOrders = orders.Count
            };

            return View("~/Views/AdminFunctions/SalesDashboard.cshtml", model);
        }

        // GET: Manage All Accounts (Artists and Customers)
        public IActionResult ManageAccounts()
        {
            var model = new ManageAccountsViewModel
            {
                Artists = _context.Artists.ToList(),
                Customers = _context.Customers.ToList()
            };
            return View("~/Views/AdminFunctions/ManageAccounts.cshtml", model);
        }

        // GET: Edit Artist
        [HttpGet]
        public IActionResult AdminEditArtistInfo(int id)
        {
            var artist = _context.Artists.FirstOrDefault(a => a.ArtistId == id);
            if (artist == null)
            {
                TempData["Error"] = "Artist not found.";
                return RedirectToAction("ManageAccounts");
            }
            return View("~/Views/AdminFunctions/AdminEditArtistInfo.cshtml", artist);
        }

        // POST: Update Artist
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AdminEditArtistInfo(Artist artist)
        {
            if (ModelState.IsValid)
            {
                var existingArtist = _context.Artists.FirstOrDefault(a => a.ArtistId == artist.ArtistId);
                if (existingArtist != null)
                {
                    // Update artist fields
                    existingArtist.Name = artist.Name;
                    existingArtist.Email = artist.Email;
                    existingArtist.PhoneNo = artist.PhoneNo;
                    existingArtist.Country = artist.Country;
                    existingArtist.Address = artist.Address;

                    _context.SaveChanges();
                    TempData["Message"] = "Artist information updated successfully!";
                    return RedirectToAction("ManageAccounts");
                }

                TempData["Error"] = "Artist not found.";
            }
            else
            {
                TempData["Error"] = "Failed to update artist. Please review the form.";
            }

            return View("~/Views/AdminFunctions/AdminEditArtistInfo.cshtml", artist);
        }

        // POST: Delete Artist
        [HttpPost]
        public IActionResult DeleteArtist(int id)
        {
			try
			{
				// Check if the artist exists
				var artist = _context.Artists.FirstOrDefault(a => a.ArtistId == id);
				if (artist == null)
				{
					TempData["Error"] = "Artist not found or unauthorized access.";
					return RedirectToAction("ManageAccounts");
				}

				// Check if any artworks by this artist are in the OrderDetail table
				var isInOrder = _context.OrderDetails.Any(o =>
					_context.Artworks.Any(art => art.ArtistId == artist.ArtistId && art.ArtId == o.ArtId)
				);

				if (isInOrder)
				{
					TempData["Error"] = "The artist cannot be deleted because their artworks are in an ongoing order process.";
					return RedirectToAction("ManageAccounts");
				}

				// Delete the artist
				_context.Artists.Remove(artist);
				_context.SaveChanges();

				TempData["Message"] = "Artist deleted successfully!";
				return RedirectToAction("ManageAccounts");
			}
			catch (Exception ex)
			{
				TempData["Error"] = $"An error occurred: {ex.Message}";
				return RedirectToAction("ManageMyArtists");
			}
		}

		// GET: Edit Customer
		[HttpGet]
        public IActionResult AdminEditCustomerInfo(int id)
        {
            var customer = _context.Customers.FirstOrDefault(c => c.CustomerId == id);
            if (customer == null)
            {
                TempData["Error"] = "Customer not found.";
                return RedirectToAction("ManageAccounts");
            }
            return View("~/Views/AdminFunctions/AdminEditCustomerInfo.cshtml", customer);
        }

        // POST: Update Customer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AdminEditCustomerInfo(Customer customer)
        {
            if (ModelState.IsValid)
            {
                var existingCustomer = _context.Customers.FirstOrDefault(c => c.CustomerId == customer.CustomerId);
                if (existingCustomer != null)
                {
                    // Update other fields
                    existingCustomer.Name = customer.Name;
                    existingCustomer.Email = customer.Email;
                    existingCustomer.PhoneNo = customer.PhoneNo;
                    existingCustomer.Country = customer.Country;
                    existingCustomer.Address = customer.Address;

                   
                    _context.SaveChanges();
                    TempData["Message"] = "Customer information updated successfully!";
                    return RedirectToAction("ManageAccounts");
                }

                TempData["Error"] = "Customer not found.";
            }
            else
            {
                TempData["Error"] = "Failed to update customer. Please review the form.";
            }

            return View("~/Views/AdminFunctions/AdminEditCustomerInfo.cshtml", customer);
        }
		[HttpPost]
		public IActionResult DeleteCustomer(int id)
		{
			try
			{
				// Check if the customer exists
				var customer = _context.Customers.FirstOrDefault(c => c.CustomerId == id);
				if (customer == null)
				{
					TempData["Error"] = "Customer not found.";
					return RedirectToAction("ManageAccounts");
				}

				// Check if the customer has any orders
				var hasOrders = _context.Orders.Any(o => o.CustomerId == customer.CustomerId);

				if (hasOrders)
				{
					TempData["Error"] = "The customer cannot be deleted because they have placed orders.";
					return RedirectToAction("ManageAccounts");
				}

				// Delete the customer
				_context.Customers.Remove(customer);
				_context.SaveChanges();

				TempData["Message"] = "Customer deleted successfully!";
				return RedirectToAction("ManageAccounts");
			}
			catch (Exception ex)
			{
				TempData["Error"] = $"An error occurred: {ex.Message}";
				return RedirectToAction("ManageAccounts");
			}
		}



		// GET: Manage Artworks (Admin Approval Pending)
		public IActionResult ManageArtworks()
        {
            if (HttpContext.Session.GetString("AdminId") == null)
            {
                TempData["Error"] = "You must log in as an admin!";
                return RedirectToAction("Login", "Account");
            }

            var artworks = _context.Artworks
                .Include(a => a.Artist)
                .Include(a => a.Category)
                .Select(a => new ArtworkViewModel
                {
                    ArtId = a.ArtId,
                    Title = a.Title,
                    Description = a.Description,
                    ArtName = a.ArtName,
                    ImageUrl = a.ImageUrl,
                    Price = a.Price ?? 0,
                    Status = a.Status,
                    AdminApproved = a.AdminApproved == "Yes" ? "Yes" : "Pending",
                    ArtistName = a.Artist.Name ?? "Unknown Artist",
                    CategoryName = a.Category.CategoryName ?? "No Category"
                }).ToList();

            return View("~/Views/AdminFunctions/ManageArtworks.cshtml", artworks);
        }

        // POST: Approve Artwork
        [HttpPost]
        public IActionResult ApproveArtwork(int id)
        {
            var artwork = _context.Artworks.FirstOrDefault(a => a.ArtId == id);
            if (artwork != null)
            {
                artwork.AdminApproved = "Yes";
                _context.SaveChanges();
                TempData["Message"] = "Artwork approved successfully!";
            }
            else
            {
                TempData["Error"] = "Artwork not found.";
            }
            return RedirectToAction("ManageArtworks");
        }

        // POST: Disapprove Artwork
        [HttpPost]
        public IActionResult DisapproveArtwork(int id)
        {
            var artwork = _context.Artworks.FirstOrDefault(a => a.ArtId == id);
            if (artwork != null)
            {
                artwork.AdminApproved = "Pending";
                _context.SaveChanges();
                TempData["Message"] = "Artwork disapproved successfully!";
            }
            else
            {
                TempData["Error"] = "Artwork not found.";
            }
            return RedirectToAction("ManageArtworks");
        }
    }
}
