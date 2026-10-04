using System;
using System.Collections.Generic;

namespace ArtGalleryFinal.Models
{
    // Every payment attempt (success or fail, real or dummy/simulated) gets one row here.
    // This is separate from the existing "Payment" table so that table's structure isn't touched.
    // NOTE: Requires the PaymentTransactions table - run Database/Setup_PaymentTransactions.sql once.
    public partial class PaymentTransaction
    {
        public int TransactionId { get; set; }

        public int? OrderId { get; set; }

        public string? OrderNumber { get; set; } // e.g. ORD-000123, easy to track/search

        public string? Gateway { get; set; } // "JazzCash", "Stripe", "CashOnDelivery"

        public string? TransactionReference { get; set; } // gateway transaction id, or a generated dummy reference

        public decimal Amount { get; set; }

        public string? Status { get; set; } // "Success" or "Failed"

        public string? CustomerEmail { get; set; }

        public DateTime CreatedAt { get; set; }

        public virtual Order? Order { get; set; }
    }
}
