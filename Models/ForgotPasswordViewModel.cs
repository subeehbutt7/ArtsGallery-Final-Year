using System.ComponentModel.DataAnnotations;

namespace ArtGalleryFinal.Models
{
    public class ForgotPasswordViewModel
    {

        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
