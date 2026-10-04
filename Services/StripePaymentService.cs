using Microsoft.Extensions.Configuration;
using Stripe;
using Stripe.Checkout;
using System.Collections.Generic;

namespace ArtGalleryFinal.Services
{
    public class StripePaymentService : IStripePaymentService
    {
        private readonly IConfiguration _config;

        public StripePaymentService(IConfiguration config)
        {
            _config = config;
            var secretKey = _config["Stripe:SecretKey"];
            if (!string.IsNullOrWhiteSpace(secretKey))
            {
                StripeConfiguration.ApiKey = secretKey;
            }
        }

        public bool IsSimulationMode =>
            string.IsNullOrWhiteSpace(_config["Stripe:SecretKey"]) ||
            _config.GetValue<bool>("Stripe:SimulationMode", true);

        public string PublishableKey => _config["Stripe:PublishableKey"] ?? "";

        public Session CreateCheckoutSession(int orderId, decimal amount, string? customerEmail, string orderNumber)
        {
            string currency = _config["Stripe:Currency"] ?? "usd";
            string successUrl = (_config["Stripe:SuccessUrl"] ?? "https://localhost:5001/Order/StripeSuccess")
                                 + $"?session_id={{CHECKOUT_SESSION_ID}}&orderId={orderId}";
            string cancelUrl = (_config["Stripe:CancelUrl"] ?? "https://localhost:5001/Order/StripeCancel")
                                 + $"?orderId={orderId}";

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = currency,
                            UnitAmount = (long)(amount * 100), // Stripe expects the smallest currency unit
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = $"Art Gallery - Order {orderNumber}",
                                Description = "Payment to Art Gallery"
                            }
                        },
                        Quantity = 1
                    }
                },
                Mode = "payment",
                CustomerEmail = customerEmail,
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
                Metadata = new Dictionary<string, string>
                {
                    { "OrderId", orderId.ToString() },
                    { "OrderNumber", orderNumber }
                }
            };

            var service = new SessionService();
            return service.Create(options);
        }

        public Session RetrieveSession(string sessionId)
        {
            var service = new SessionService();
            return service.Get(sessionId);
        }
    }
}
