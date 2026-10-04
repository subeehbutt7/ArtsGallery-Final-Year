using ArtGalleryFinal.Models;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using System;
using System.Linq;

namespace ArtGalleryFinal.Services
{
    // Sends order confirmation emails using the same Gmail/MailKit setup already
    // used in AccountController for verification emails (see EmailSettings in appsettings.json).
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _config;

        public EmailService(ILogger<EmailService> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        public void SendOrderConfirmationEmail(string toEmail, string customerName, Order order)
        {
            if (string.IsNullOrWhiteSpace(toEmail))
            {
                _logger.LogWarning($"Skipped order confirmation email for Order #{order.OrderId}: no email address available.");
                return;
            }

            try
            {
                string fromEmail = _config["EmailSettings:FromEmail"] ?? "najamartgallery@gmail.com";
                string appPassword = _config["EmailSettings:AppPassword"] ?? "itscybgjxzqskums";

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Art Gallery", fromEmail));
                message.To.Add(new MailboxAddress(customerName, toEmail));
                message.Subject = $"Art Gallery - Order Confirmation #{order.OrderId}";

                string itemsList = order.OrderDetails != null && order.OrderDetails.Any()
                    ? string.Join("\n", order.OrderDetails.Select(od =>
                        $"- {od.Art?.Title ?? "Artwork"} x{od.Quantity ?? 0} = Rs {(od.Price ?? 0) * (od.Quantity ?? 0)}"))
                    : "- (item details not available)";

                int shipmentCharges = order.Shipments?.FirstOrDefault()?.ShipmentCharges ?? 0;
                int totalAmount = (order.OrderAmount ?? 0) + shipmentCharges;
                string paymentMethod = order.Payments?.FirstOrDefault()?.PaymentMethod ?? "N/A";
                string paymentStatus = order.Payments?.FirstOrDefault()?.PaymentStatus ?? "Pending";

                message.Body = new TextPart("plain")
                {
                    Text = $@"Hi {customerName},

Thank you for shopping with Art Gallery! Your order has been confirmed.

Order #: {order.OrderId}

Items:
{itemsList}

Shipment Charges: Rs {shipmentCharges}
Total Amount: Rs {totalAmount}
Payment Method: {paymentMethod}
Payment Status: {paymentStatus}

We will notify you once your order is shipped.

-- Art Gallery Team"
                };

                using var client = new SmtpClient();
                client.Connect("smtp.gmail.com", 587, false);
                client.Authenticate(fromEmail, appPassword);
                client.Send(message);
                client.Disconnect(true);

                _logger.LogInformation($"Order confirmation email sent to {toEmail} for Order #{order.OrderId}");
            }
            catch (Exception ex)
            {
                // Email failure should never break order placement, so we log and continue.
                _logger.LogError(ex, $"Failed to send order confirmation email to {toEmail} for Order #{order.OrderId}");
            }
        }
        public void SendAdminPaymentNotification(Order order, string gateway, bool success, string? transactionReference, string customerName, string? customerEmail)
        {
            try
            {
                string fromEmail = _config["EmailSettings:FromEmail"] ?? "najamartgallery@gmail.com";
                string appPassword = _config["EmailSettings:AppPassword"] ?? "itscybgjxzqskums";

                // Try to find the real admin's email from the DB (order.AdminId); fall back to config value.
                string adminEmail = _config["EmailSettings:AdminNotificationEmail"] ?? fromEmail;

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Art Gallery", fromEmail));
                message.To.Add(new MailboxAddress("Admin", adminEmail));
                message.Subject = $"Art Gallery - Payment {(success ? "Received" : "Failed")} for Order {order.OrderNumber}";

                int shipmentCharges = order.Shipments?.FirstOrDefault()?.ShipmentCharges ?? 0;
                int totalAmount = (order.OrderAmount ?? 0) + shipmentCharges;

                message.Body = new TextPart("plain")
                {
                    Text = $@"Hi Admin,

A payment attempt was just {(success ? "completed successfully" : "made but it failed")} on Art Gallery.

Order Number: {order.OrderNumber}
Order Id: {order.OrderId}
Customer Name: {customerName}
Customer Email: {customerEmail}
Payment Gateway: {gateway}
Transaction Reference: {transactionReference ?? "N/A"}
Amount: Rs {totalAmount}
Status: {(success ? "SUCCESS" : "FAILED")}

-- Art Gallery System"
                };

                using var client = new SmtpClient();
                client.Connect("smtp.gmail.com", 587, false);
                client.Authenticate(fromEmail, appPassword);
                client.Send(message);
                client.Disconnect(true);

                _logger.LogInformation($"Admin payment notification sent for Order {order.OrderNumber}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send admin payment notification for Order {order.OrderNumber}");
            }
        }

        public void SendRegistrationVerificationEmail(string toEmail, string name, string verificationCode)
        {
            try
            {
                string fromEmail = _config["EmailSettings:FromEmail"] ?? "najamartgallery@gmail.com";
                string appPassword = _config["EmailSettings:AppPassword"] ?? "itscybgjxzqskums";

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Art Gallery", fromEmail));
                message.To.Add(new MailboxAddress(name, toEmail));
                message.Subject = "Art Gallery - Verify your Email";

                message.Body = new TextPart("plain")
                {
                    Text = $@"Hi {name},

Thank you for registering with Art Gallery. Please use the code below to verify your email and activate your account:

Verification Code: {verificationCode}

This code will expire in 15 minutes. If you did not try to register, you can ignore this email - your account will not be created.

-- Art Gallery Team"
                };

                using var client = new SmtpClient();
                client.Connect("smtp.gmail.com", 587, false);
                client.Authenticate(fromEmail, appPassword);
                client.Send(message);
                client.Disconnect(true);

                _logger.LogInformation($"Registration verification email sent to {toEmail}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send registration verification email to {toEmail}");
                throw;
            }
        }
    }
}
