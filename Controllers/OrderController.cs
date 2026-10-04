using ArtGalleryFinal.Models;
using ArtGalleryFinal.Utilities;
using ArtGalleryFinal.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ArtGalleryFinal.Controllers
{
    public class OrderController : Controller
    {
        private readonly ArtGalleryContext _dbcontext;
        private readonly IJazzCashService _jazzCashService;
        private readonly IStripePaymentService _stripeService;
        private readonly IEmailService _emailService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(ArtGalleryContext dbcontext, IJazzCashService jazzCashService, IStripePaymentService stripeService, IEmailService emailService, ILogger<OrderController> logger)
        {
            _dbcontext = dbcontext;
            _jazzCashService = jazzCashService;
            _stripeService = stripeService;
            _emailService = emailService;
            _logger = logger;
        }


        [HttpPost]
        public IActionResult PlaceOrder(Shipment shipment, string PaymentMethod)
        {
            using (var transaction = _dbcontext.Database.BeginTransaction())
            {
                try
                {
                    // Retrieve cart items
                    var cart = HttpContext.Session.GetObjectFromJson<List<CartItemViewModel>>("Cart");
                    if (cart == null || !cart.Any())
                    {
                        TempData["Error"] = "Your cart is empty.";
                        return RedirectToAction("ViewCart", "Cart");
                    }

                    // Retrieve CustomerId from session
                    string customerIdString = HttpContext.Session.GetString("CustomerId");
                    if (string.IsNullOrEmpty(customerIdString))
                    {
                        TempData["Error"] = "No customer is logged in.";
                        return RedirectToAction("ViewCart", "Cart");
                    }

                    int customerId = int.Parse(customerIdString);

                    // Stock check: stop if an artwork is sold out or not enough copies are left
                    foreach (var item in cart)
                    {
                        var art = _dbcontext.Artworks.Find(item.ArtId);
                        if (art == null || art.IsSoldOut || item.Quantity > (art.Quantity ?? 1))
                        {
                            TempData["Error"] = $"'{item.Title}' is sold out or not available in that quantity.";
                            return RedirectToAction("ViewCart", "Cart");
                        }
                    }

                    // Create a new order
                    var order = new Order
                    {
                        CustomerId = customerId,
                        OrderAmount = cart.Sum(c => c.Price * c.Quantity), // Calculate total amount
                        Discount = 0, // Default discount
                        OrderDate = DateOnly.FromDateTime(DateTime.Now),
                        AdminId = 1,
                        OrderStatus="Pending"
                    };

                    _dbcontext.Orders.Add(order);
                    _dbcontext.SaveChanges();

                    // Save each cart item as OrderDetail
                    foreach (var item in cart)
                    {
                        var orderDetail = new OrderDetail
                        {
                            OrderId = order.OrderId,
                            ArtId = item.ArtId,
                            Quantity = item.Quantity,
                            Price = item.Price
                        };
                        _dbcontext.OrderDetails.Add(orderDetail);

                        // Reduce artwork quantity; 0 left = Sold
                        var boughtArt = _dbcontext.Artworks.Find(item.ArtId);
                        if (boughtArt != null)
                        {
                            boughtArt.Quantity = Math.Max(0, (boughtArt.Quantity ?? 1) - item.Quantity);
                            if (boughtArt.Quantity == 0) boughtArt.Status = "Sold";
                        }
                    }

                    // Save shipment details
                    shipment.OrderId = order.OrderId;
                    shipment.ShipmentDate = DateOnly.FromDateTime(DateTime.Now);
                    shipment.ShipmentCharges = 250; // Default shipment charges
                    shipment.AdminId = 1;
                   

                    _dbcontext.Shipments.Add(shipment);

                    // Save payment details in Payment table
                    var payment = new Payment
                    {
                        OrderId = order.OrderId,
                        PaymentDate = DateOnly.FromDateTime(DateTime.Now),
                        Amount = order.OrderAmount + shipment.ShipmentCharges, // Total amount + shipment charges
                        PaymentStatus = "Pending", // Default status
                        PaymentMethod = PaymentMethod, // Store user-selected payment method
                        AdminId = 1 // Default AdminId
                    };

                    _dbcontext.Payments.Add(payment);
                    _dbcontext.SaveChanges();

                    // Commit transaction
                    transaction.Commit();
                    HttpContext.Session.Remove("Cart");

                    // Online gateways: send the customer to the payment page before we confirm the order.
                    if (string.Equals(PaymentMethod, "Jazzcash", StringComparison.OrdinalIgnoreCase))
                    {
                        return RedirectToAction("PayWithJazzCash", new { orderId = order.OrderId });
                    }
                    if (string.Equals(PaymentMethod, "Stripe", StringComparison.OrdinalIgnoreCase))
                    {
                        return RedirectToAction("PayWithStripe", new { orderId = order.OrderId });
                    }

                    // Cash on Delivery: order is confirmed immediately, so send the confirmation email + log the statement now.
                    var fullOrder = _dbcontext.Orders
                        .Include(o => o.OrderDetails).ThenInclude(od => od.Art)
                        .Include(o => o.Shipments)
                        .Include(o => o.Payments)
                        .FirstOrDefault(o => o.OrderId == order.OrderId);

                    if (fullOrder != null)
                    {
                        var customerName = _dbcontext.Customers.Where(c => c.CustomerId == customerId).Select(c => c.Name).FirstOrDefault() ?? "Customer";

                        LogPaymentTransaction(fullOrder, "CashOnDelivery", "COD-" + fullOrder.OrderNumber, payment.Amount ?? 0, "Pending", shipment.Email);
                        _emailService.SendOrderConfirmationEmail(shipment.Email, customerName, fullOrder);
                        _emailService.SendAdminPaymentNotification(fullOrder, "CashOnDelivery", true, "COD-" + fullOrder.OrderNumber, customerName, shipment.Email);
                    }

                    TempData["Message"] = $"Your order {order.OrderNumber} has been placed successfully! A confirmation email has been sent.";
                    return RedirectToAction("ViewCart", "Cart");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    TempData["Error"] = "An error occurred: " + ex.Message;
                    return RedirectToAction("ViewCart", "Cart");
                }
            }
        }
        // Step 1 of JazzCash payment: build the request and send the customer to the gateway.
        // If no real JazzCash sandbox credentials are configured yet, show a built-in demo page instead
        // (see JazzCash:SimulationMode in appsettings.json) so the project still works for demo/viva.
        [HttpGet]
        public IActionResult PayWithJazzCash(int orderId)
        {
            var order = _dbcontext.Orders.Include(o => o.Payments).Include(o => o.Shipments).FirstOrDefault(o => o.OrderId == orderId);
            if (order == null)
            {
                TempData["Error"] = "Order not found.";
                return RedirectToAction("ViewMyOrders");
            }

            decimal amount = order.Payments.FirstOrDefault()?.Amount ?? order.OrderAmount ?? 0;

            if (_jazzCashService.IsSimulationMode)
            {
                ViewBag.OrderId = orderId;
                ViewBag.OrderNumber = order.OrderNumber;
                ViewBag.Amount = amount;
                ViewBag.Gateway = "JazzCash";
                return View("SimulateJazzCashPayment");
            }

            var model = _jazzCashService.BuildPaymentRequest(orderId, amount);
            ViewBag.GatewayUrl = _jazzCashService.GatewayUrl;
            return View(model);
        }

        // Demo "gateway" used only while JazzCash:SimulationMode is true (no real merchant account yet)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SimulateJazzCashPayment(int orderId, string result)
        {
            bool success = string.Equals(result, "success", StringComparison.OrdinalIgnoreCase);
            string reference = "JC-SIMULATED-" + Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper();
            return ProcessPaymentResult(orderId, "JazzCash", success, reference);
        }

        // Step 2 of real JazzCash payment: JazzCash POSTs the transaction result back to this URL
        [HttpPost]
        public IActionResult JazzCashReturn(IFormCollection form)
        {
            string responseCode = form["pp_ResponseCode"];
            string txnRef = form["pp_TxnRefNo"];
            int orderId = int.TryParse(form["pp_BillReference"], out var id) ? id : 0;
            return ProcessPaymentResult(orderId, "JazzCash", responseCode == "000", txnRef);
        }

        // Step 1 of Stripe payment: create a Stripe Checkout Session and redirect the customer to it.
        // If no real Stripe test/live secret key is configured yet, show a built-in demo page instead
        // (see Stripe:SimulationMode in appsettings.json) so the project still works for demo/viva.
        [HttpGet]
        public IActionResult PayWithStripe(int orderId)
        {
            var order = _dbcontext.Orders.Include(o => o.Payments).Include(o => o.Shipments).FirstOrDefault(o => o.OrderId == orderId);
            if (order == null)
            {
                TempData["Error"] = "Order not found.";
                return RedirectToAction("ViewMyOrders");
            }

            decimal amount = order.Payments.FirstOrDefault()?.Amount ?? order.OrderAmount ?? 0;
            string email = order.Shipments.FirstOrDefault()?.Email;

            if (_stripeService.IsSimulationMode)
            {
                ViewBag.OrderId = orderId;
                ViewBag.OrderNumber = order.OrderNumber;
                ViewBag.Amount = amount;
                ViewBag.Gateway = "Stripe";
                return View("SimulateJazzCashPayment"); // same generic demo view works for any gateway
            }

            try
            {
                var session = _stripeService.CreateCheckoutSession(orderId, amount, email, order.OrderNumber);
                return Redirect(session.Url);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Stripe checkout session creation failed for order {orderId}");
                TempData["Error"] = "Could not start Stripe payment. Please try again.";
                return RedirectToAction("ViewMyOrders");
            }
        }

        // Demo "gateway" used only while Stripe:SimulationMode is true (no real Stripe keys yet)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SimulateStripePayment(int orderId, string result)
        {
            bool success = string.Equals(result, "success", StringComparison.OrdinalIgnoreCase);
            string reference = "STRIPE-SIMULATED-" + Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper();
            return ProcessPaymentResult(orderId, "Stripe", success, reference);
        }

        // Stripe redirects the browser here after a real checkout
        [HttpGet]
        public IActionResult StripeSuccess(string session_id, int orderId)
        {
            try
            {
                var session = _stripeService.RetrieveSession(session_id);
                bool success = session.PaymentStatus == "paid";
                string reference = session.PaymentIntentId ?? session.Id;
                return ProcessPaymentResult(orderId, "Stripe", success, reference);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to verify Stripe session for order {orderId}");
                TempData["Error"] = "Could not verify your payment. Please contact support if you were charged.";
                return RedirectToAction("ViewMyOrders");
            }
        }

        [HttpGet]
        public IActionResult StripeCancel(int orderId)
        {
            return ProcessPaymentResult(orderId, "Stripe", false, null);
        }

        // Shared by JazzCash and Stripe (real + simulated):
        // updates payment status, saves a payment statement row, and sends both
        // the customer confirmation email and the admin notification email.
        private IActionResult ProcessPaymentResult(int orderId, string gateway, bool success, string? transactionReference)
        {
            var order = _dbcontext.Orders
                .Include(o => o.OrderDetails).ThenInclude(od => od.Art)
                .Include(o => o.Shipments)
                .Include(o => o.Payments)
                .FirstOrDefault(o => o.OrderId == orderId);

            if (order == null)
            {
                TempData["Error"] = "Order not found.";
                return RedirectToAction("ViewMyOrders");
            }

            var payment = order.Payments.FirstOrDefault();
            var customerName = _dbcontext.Customers.Where(c => c.CustomerId == order.CustomerId).Select(c => c.Name).FirstOrDefault() ?? "Customer";
            var email = order.Shipments.FirstOrDefault()?.Email;

            LogPaymentTransaction(order, gateway, transactionReference, payment?.Amount ?? 0, success ? "Success" : "Failed", email);
            _emailService.SendAdminPaymentNotification(order, gateway, success, transactionReference, customerName, email);

            if (success)
            {
                if (payment != null) payment.PaymentStatus = "Paid";
                _dbcontext.SaveChanges();

                _emailService.SendOrderConfirmationEmail(email, customerName, order);

                TempData["Message"] = $"Payment successful! Your order {order.OrderNumber} is confirmed and a confirmation email has been sent.";
            }
            else
            {
                if (payment != null) payment.PaymentStatus = "Failed";
                _dbcontext.SaveChanges();
                TempData["Error"] = $"Payment for order {order.OrderNumber} failed or was cancelled. You can try again from My Orders.";
            }

            return RedirectToAction("ViewMyOrders");
        }

        // Saves one row per payment attempt (dummy or real, success or fail) for a proper payment statement/history.
        // Requires the PaymentTransactions table - see Database/Setup_PaymentTransactions.sql
        private void LogPaymentTransaction(Order order, string gateway, string? transactionReference, decimal amount, string status, string? customerEmail)
        {
            try
            {
                _dbcontext.PaymentTransactions.Add(new PaymentTransaction
                {
                    OrderId = order.OrderId,
                    OrderNumber = order.OrderNumber,
                    Gateway = gateway,
                    TransactionReference = transactionReference,
                    Amount = amount,
                    Status = status,
                    CustomerEmail = customerEmail,
                    CreatedAt = DateTime.Now
                });
                _dbcontext.SaveChanges();
            }
            catch (Exception ex)
            {
                // Never let statement logging break the payment flow itself.
                _logger.LogError(ex, $"Failed to log payment transaction for order {order.OrderId}. Did you run Database/Setup_PaymentTransactions.sql?");
            }
        }

        public IActionResult ViewMyOrders()
        {
            var customerId = HttpContext.Session.GetString("CustomerId");
            var artistId = HttpContext.Session.GetString("ArtistId");
            var adminId = HttpContext.Session.GetString("AdminId");

            if (string.IsNullOrEmpty(customerId) && string.IsNullOrEmpty(artistId) && string.IsNullOrEmpty(adminId))
            {
                TempData["Error"] = "You need to log in first!";
                return RedirectToAction("Login", "Account");
            }
            string role =!string.IsNullOrEmpty(adminId) ? "Admin"
            :string.IsNullOrEmpty(customerId) ? "Artist" : "Customer";

            // Fetch orders based on role
            var orders = role == "Customer" ?
                _dbcontext.Orders
                    .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Art)
                    .Include(o => o.Payments)
                    .Include(o => o.Shipments)
                    .Where(o => o.CustomerId == int.Parse(customerId))
                    .ToList()
                : role == "Artist" ?
                _dbcontext.Orders
                    .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Art)
                    .Include(o => o.Payments)
                    .Include(o => o.Shipments)
                    .Where(o => o.OrderDetails.Any(od => od.Art.ArtistId == int.Parse(artistId)))
                    .ToList()
                    : 
                      _dbcontext.Orders
            .Include(o => o.OrderDetails)
            .ThenInclude(od => od.Art)
            .Include(o => o.Payments)
            .Include(o => o.Shipments)
            .ToList();

			var orderViewModels = orders.Select(o => new OrderViewModel
            {
                OrderId = o.OrderId,
                OrderNumber = o.OrderNumber,
                Date = o.OrderDate.HasValue ? o.OrderDate.Value : DateOnly.MinValue,
                OrderAmount = o.OrderAmount ?? 0,
                Discount = o.Discount ?? 0,
                ArtName = string.Join(", ", o.OrderDetails.Select(od => od.Art.Title)),
                ImageUrl=o.OrderDetails.Select(od=>od.Art.ImageUrl).FirstOrDefault(),
                Quantity = o.OrderDetails.Sum(od => od.Quantity ?? 0),
                ShipmentCharges = o.Shipments.FirstOrDefault()?.ShipmentCharges ?? 0,
                OrderStatus = o.OrderStatus ??"NO Status",
                TotalAmount = (o.OrderAmount ?? 0) + (o.Shipments.FirstOrDefault()?.ShipmentCharges ?? 0),
                PaymentStatus=o.Payments.FirstOrDefault()?.PaymentStatus ??"Pending",
                CustomerName = role == "Customer" ? _dbcontext.Customers.Where(c => c.CustomerId == o.CustomerId).Select(c => c.Name).FirstOrDefault() : null
            }).ToList();

            ViewBag.Role = role; // Pass the role to the view
            return View("~/Views/Order/ViewMyOrders.cshtml", orderViewModels);
        }
		[HttpPost]
		public IActionResult UpdateOrderStatus(int OrderId, string OrderStatus)
		{
			// Find the order
			var order = _dbcontext.Orders
				.Include(o => o.Payments) // Include Payments
				.FirstOrDefault(o => o.OrderId == OrderId);

			if (order == null)
			{
				TempData["Error"] = "Order or shipment not found.";
				return RedirectToAction("ViewMyOrders");
			}

			order.OrderStatus = OrderStatus;

			// Handle null payment object
			var payment = order.Payments.FirstOrDefault();
			if (OrderStatus == "Delivered")
			{
				if (payment != null)
				{
					payment.PaymentStatus = "Recived"; // Update PaymentStatus
				}
				else
				{
					TempData["Error"] = "Payment record not found for the order.";
					return RedirectToAction("ViewMyOrders");
				}
			}

			_dbcontext.SaveChanges();
			TempData["Message"] = "Order status updated successfully!";
			return RedirectToAction("ViewMyOrders");
		}
        // Display Edit Form
        public IActionResult EditOrder(int Id)
        {
            var order = _dbcontext.Orders
                .Include(o => o.OrderDetails)
                .Include(o=>o.Shipments)
                .FirstOrDefault(o => o.OrderId == Id);

            if (order == null || order.OrderStatus != "Pending")
            {
                return NotFound("Order cannot be edited unless it is pending.");
            }

            // Map order data to OrderViewModel
            var orderViewModel = new OrderViewModel
            {
                OrderId = order.OrderId,
                Quantity = order.OrderDetails.Sum(od => od.Quantity ?? 0),
               Address = order.Shipments.FirstOrDefault()?.Address,
               Country=order.Shipments.FirstOrDefault()?.Country,
            };

            return View("~/Views/Order/EditOrder.cshtml", orderViewModel);
        }

        // Update Order
        [HttpPost]
        public IActionResult EditOrder(OrderViewModel updatedOrder)
        {
            var order = _dbcontext.Orders
                .Include(o => o.OrderDetails)
                .Include(o => o.Shipments)
                .FirstOrDefault(o => o.OrderId == updatedOrder.OrderId);

            if ( order == null || order.OrderStatus != "Pending" )
            {
				return BadRequest("Only pending orders can be edited.");
			}

			var orderDetail = order.OrderDetails.FirstOrDefault();
			if (orderDetail != null)
			{
				orderDetail.Quantity = updatedOrder.Quantity;
                order.OrderAmount = orderDetail.Quantity * orderDetail.Price;
			}

			// Update shipping address
			var shipment = order.Shipments.FirstOrDefault();
			if (shipment != null)
			{
				shipment.Address = updatedOrder.Address;
                shipment.Country= updatedOrder.Country;
			}
			_dbcontext.SaveChanges();
            TempData["Message"] = "Order updated successfully!";
            return RedirectToAction("ViewMyOrders");
        }

        // Cancel Order
        [HttpPost]
        public IActionResult CancelOrder(int OrderId)
        {
            var order = _dbcontext.Orders.FirstOrDefault(o => o.OrderId == OrderId);

            if (order == null)
            {
                TempData["Error"] = "Order not found.";
                return RedirectToAction("ViewMyOrders");
            }

            if (order.OrderStatus == "Pending")
            {
                order.OrderStatus = "Canceled";

                // Give the copies back to stock
                foreach (var d in _dbcontext.OrderDetails.Where(od => od.OrderId == OrderId).ToList())
                {
                    var art = d.ArtId == null ? null : _dbcontext.Artworks.Find(d.ArtId);
                    if (art != null)
                    {
                        art.Quantity = (art.Quantity ?? 0) + (d.Quantity ?? 0);
                        art.Status = art.Quantity > 0 ? "For Sale" : "Sold";
                    }
                }

                _dbcontext.SaveChanges();
                TempData["Message"] = "Order canceled successfully!";
                return RedirectToAction("ViewMyOrders");
            }
            return BadRequest("You can only cancel orders that are not shipped.");
        }
		[HttpPost]
		public IActionResult SubmitRating(Rating rating)
		{
			if (rating.CustomerId == null || rating.ArtId == null || rating.RatingValue == null)
			{
				TempData["Error"] = "Rating information is incomplete.";
				return RedirectToAction("ViewMyOrders", "Order");
			}

			// Save Rating to Database
			_dbcontext.Ratings.Add(rating);
			_dbcontext.SaveChanges();

			TempData["Message"] = "Thank you for your review!";
			return RedirectToAction("ViewMyOrders", "Order");
		}





	}








}


