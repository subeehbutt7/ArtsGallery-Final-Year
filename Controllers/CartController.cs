using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using ArtGalleryFinal.Models;
using System.Collections.Generic;
using ArtGalleryFinal.Utilities;

namespace ArtGalleryFinal.Controllers
{
	public class CartController : Controller
	{
		private readonly ArtGalleryContext _dbContext;

		public CartController(ArtGalleryContext dbContext)
		{
			_dbContext = dbContext;
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult AddToCart(int Id)
		{
			// Check if the user is logged in as a customer
			var customerId = HttpContext.Session.GetString("CustomerId");

			if (string.IsNullOrEmpty(customerId))
			{
				// Show a message asking the user to log in
				TempData["Error"] = "Please log in as a customer to add artworks to your cart.";
				return RedirectToAction("Artworks", "Artwork"); // Redirect back to Artworks page
			}

			try
			{
				// Retrieve or create the cart from the session
				var cart = HttpContext.Session.GetObjectFromJson<List<CartItemViewModel>>("Cart") ?? new List<CartItemViewModel>();

				// Fetch the artwork and check stock
				var artwork = _dbContext.Artworks.FirstOrDefault(a => a.ArtId == Id);
				if (artwork == null)
				{
					TempData["Error"] = "Artwork not found.";
					return RedirectToAction("Artworks", "Artwork");
				}
				if (artwork.IsSoldOut)
				{
					TempData["Error"] = "Sorry, this artwork is sold out.";
					return RedirectToAction("Artworks", "Artwork");
				}

				// Check if the artwork is already in the cart
				var existingItem = cart.Find(c => c.ArtId == Id);
				if (existingItem != null)
				{
					if (existingItem.Quantity >= (artwork.Quantity ?? 1))
					{
						TempData["Error"] = "You already added all available copies of this artwork.";
						return RedirectToAction("Artworks", "Artwork");
					}
					existingItem.Quantity++; // Increase quantity if artwork already exists
				}
				else
				{
					cart.Add(new CartItemViewModel
					{
						ArtId = artwork.ArtId,
						Title = artwork.Title,
						ImageUrl = artwork.ImageUrl,
						Price = artwork.Price ?? 0,
						Quantity = 1
					});
				}

				// Save the updated cart back to the session
				HttpContext.Session.SetObjectAsJson("Cart", cart);
				TempData["Message"] = "Artwork added to your cart successfully!";
				return RedirectToAction("Artworks", "Artwork");
			}
			catch (Exception ex)
			{
				TempData["Error"] = $"An error occurred: {ex.Message}";
				return RedirectToAction("Artworks", "Artwork");
			}
		}

		public IActionResult ViewCart()
		{
			try
			{
				// Retrieve the cart from the session
				var cart = HttpContext.Session.GetObjectFromJson<List<CartItemViewModel>>("Cart") ?? new List<CartItemViewModel>();

				// Return the cart view with the cart items
				return View(cart);
			}
			catch (Exception ex)
			{
				TempData["Error"] = $"An error occurred: {ex.Message}";
				return RedirectToAction("Artworks", "Artwork");
			}
		}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveFromCart(int Id)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItemViewModel>>("Cart") ?? new List<CartItemViewModel>();
            var itemToRemove = cart.FirstOrDefault(c => c.ArtId == Id);
            if (itemToRemove != null)
            {
                cart.Remove(itemToRemove);
                HttpContext.Session.SetObjectAsJson("Cart", cart);
                TempData["Message"] = "Item removed from the cart successfully!";
            }
            else
            {
                TempData["Error"] = "Item not found in the cart.";
            }
            return RedirectToAction("ViewCart");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PlaceOrder()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItemViewModel>>("Cart");
            if (cart == null || !cart.Any())
            {
                TempData["Error"] = "Your cart is empty. Cannot place an order.";
                return RedirectToAction("ViewCart", "Cart");
            }

            // Logic to process the order
            // Example: Save the order to the database

            HttpContext.Session.Remove("Cart"); // Clear the cart after placing the order
            TempData["Message"] = "Your order has been placed successfully!";
            return RedirectToAction("Artworks", "Artwork"); // Redirect to the Artworks page
        }

		[HttpPost]
		public IActionResult ShowShipmentForm()
		{
			TempData["ShowShipmentForm"] = true; // Set the flag to display the shipment form
			return RedirectToAction("ViewCart"); // Reload the ViewCart page with the form visible
		}


	}
}

	
