using ArtGalleryFinal.Models;

namespace ArtGalleryFinal.Services
{
    public interface IEmailService
    {
        void SendOrderConfirmationEmail(string toEmail, string customerName, Order order);

        // Notifies the admin whenever a payment (real or dummy/simulated) succeeds or fails
        void SendAdminPaymentNotification(Order order, string gateway, bool success, string? transactionReference, string customerName, string? customerEmail);

        // Registration email verification
        void SendRegistrationVerificationEmail(string toEmail, string name, string verificationCode);
    }
}
