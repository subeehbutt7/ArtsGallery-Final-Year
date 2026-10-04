using System;

namespace ArtGalleryFinal.Models
{
    // Holds registration details temporarily (in Session) until the user verifies their email.
    // The actual Customer/Artist row is only created in the database after verification succeeds.
    public class PendingRegistrationModel
    {
        public string Role { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string HashedPassword { get; set; }
        public string PhoneNo { get; set; }
        public string Address { get; set; }
        public string Country { get; set; }
        public string VerificationCode { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
