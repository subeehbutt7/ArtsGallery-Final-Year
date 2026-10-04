using ArtGalleryFinal.Models;

namespace ArtGalleryFinal.Services
{
    public interface IJazzCashService
    {
        // True when real JazzCash sandbox/merchant credentials are not configured yet.
        // In this mode the app shows a built-in demo payment page instead of calling JazzCash,
        // so the project still works for demos/viva without a real merchant account.
        bool IsSimulationMode { get; }

        // The JazzCash URL the payment form should be posted to
        string GatewayUrl { get; }

        // Builds the signed request (with secure hash) that gets posted to JazzCash
        JazzCashPaymentModel BuildPaymentRequest(int orderId, decimal amount);
    }
}
