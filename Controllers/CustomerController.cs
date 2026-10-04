using ArtGalleryFinal.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace ArtGalleryFinal.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ArtGalleryContext _dbcontext;

        // Constructor for dependency injection
        public CustomerController(ArtGalleryContext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        // GET: Customer Dashboard
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Profile()
        {
            var customerId = HttpContext.Session.GetString("CustomerId");
            if (string.IsNullOrEmpty(customerId))
            {
                TempData["Error"] = "You need to log in first!";
                return RedirectToAction("Login", "Account"); // Redirect to the login page
            }

            var customer = _dbcontext.Customers.FirstOrDefault(a => a.CustomerId == int.Parse(customerId));
            if (customer == null)
            {
                TempData["Error"] = "Customer not found.";
                return RedirectToAction("Index"); // Redirect to Artist Panel or Home
            }

            return View("~/Views/CustomerFunctions/Profile.cshtml", customer);
        }

        // GET: Edit Customer Profile
        [HttpGet]
        public IActionResult EditProfile()
        {
            var customerId = HttpContext.Session.GetString("CustomerId");
            if (string.IsNullOrEmpty(customerId))
            {
                TempData["Error"] = "You need to log in first!";
                return RedirectToAction("Login", "Account"); // Redirect to the login page
            }

            var customer = _dbcontext.Customers.FirstOrDefault(a => a.CustomerId == int.Parse(customerId));
            if (customer == null)
            {
                TempData["Error"] = "Customer not found.";
                return RedirectToAction("Index"); // Redirect to Customer Panel or Home
            }

            return View("~/Views/CustomerFunctions/EditProfile.cshtml", customer);
        }

        // POST: Update Customer Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditProfile(Customer updatedCustomer)
        {
            var customerId = HttpContext.Session.GetString("CustomerId");
            if (string.IsNullOrEmpty(customerId))
            {
                TempData["Error"] = "You need to log in first!";
                return RedirectToAction("Login", "Account"); // Redirect to the login page
            }

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "There was an error updating your profile. Please ensure all fields are correctly filled.";
                return View("~/Views/CustomerFunctions/EditProfile.cshtml", updatedCustomer);
            }

            var existingCustomer = _dbcontext.Customers.FirstOrDefault(a => a.CustomerId == int.Parse(customerId));
            if (existingCustomer == null)
            {
                TempData["Error"] = "Customer not found.";
                return RedirectToAction("Index"); // Redirect to Customer Panel or Home
            }

            // Update the artist's information
            existingCustomer.Name = updatedCustomer.Name;
            existingCustomer.Address = updatedCustomer.Address;
            existingCustomer.Country = updatedCustomer.Country;
            existingCustomer.PhoneNo = updatedCustomer.PhoneNo;

            _dbcontext.SaveChanges();

            TempData["Message"] = "Profile updated successfully!";
            return RedirectToAction("Profile"); // Redirect back to the profile view
        }


        // CHECKOUT: Process Cart Data to Create Order
        [HttpPost]
        public IActionResult Checkout([FromBody] List<CartItemViewModel> cartItems)
        {
            var customerId = HttpContext.Session.GetString("CustomerId");
            if (string.IsNullOrEmpty(customerId))
            {
                TempData["Error"] = "You need to log in first!";
                return Unauthorized("You need to log in first!");
            }

            if (cartItems == null || !cartItems.Any())
            {
                return BadRequest("Cart is empty!");
            }

            // Create a new order
            var order = new Order
            {
                CustomerId = int.Parse(customerId), // Associate the order with the customer
                OrderDate = DateOnly.FromDateTime(DateTime.Now), // Use current date
                OrderAmount = cartItems.Sum(item => item.Price * item.Quantity), // Calculate total order amount
                Discount = 0 // Add discount logic here if needed
            };

            // Add the order to the database
            _dbcontext.Orders.Add(order);
            _dbcontext.SaveChanges();

            // Add order details for each cart item
            foreach (var cartItem in cartItems)
            {
                var orderDetail = new OrderDetail
                {
                    OrderId = order.OrderId, // Associate with the newly created order
                    ArtId = cartItem.ArtId, // Artwork ID
                    Quantity = cartItem.Quantity // Quantity of the item
                };

                _dbcontext.OrderDetails.Add(orderDetail);
            }

            // Save the order details to the database
            _dbcontext.SaveChanges();

            TempData["Message"] = "Order placed successfully!";
            return Ok("Order placed successfully!");
        }

    }
}
