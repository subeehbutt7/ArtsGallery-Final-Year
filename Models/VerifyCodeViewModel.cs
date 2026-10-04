using System.ComponentModel.DataAnnotations;

namespace ArtGalleryFinal.Models
{
    public class VerifyCodeViewModel
    {

        [Required]
        public string VerificationCode { get; set; }
    }
}
