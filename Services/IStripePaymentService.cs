using Stripe.Checkout;

namespace ArtGalleryFinal.Services
{
    public interface IStripePaymentService
    {
        // True when no real Stripe test/live secret key is configured yet - a demo
        // "Stripe-style" checkout page is shown instead so the project still works for demos/viva.
        bool IsSimulationMode { get; }

        string PublishableKey { get; }

        Session CreateCheckoutSession(int orderId, decimal amount, string? customerEmail, string orderNumber);

        Session RetrieveSession(string sessionId);
    }
}
